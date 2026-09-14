using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Text;
using System.Threading;

namespace TechInstaller
{
    public enum LogLevel
    {
        Info,
        Success,
        Warning,
        Error,
        Download
    }

    public class InstallerEngine
    {
        public delegate void LogHandler(string message, LogLevel level);
        public delegate void ProgressHandler(int percent, string statusText);
        public delegate void AppStatusHandler(AppItem app);

        public event LogHandler OnLog;
        public event ProgressHandler OnOverallProgress;
        public event ProgressHandler OnCurrentProgress;
        public event AppStatusHandler OnAppStatusChanged;

        private volatile bool _cancelRequested = false;

        public bool IsBusy { get; private set; }

        public void Cancel()
        {
            _cancelRequested = true;
        }

        private void Log(string message, LogLevel level)
        {
            if (OnLog != null)
            {
                OnLog(message, level);
            }
        }

        public void RunInstallationQueue(List<AppItem> selectedApps)
        {
            IsBusy = true;
            _cancelRequested = false;

            Thread worker = new Thread(new ThreadStart(delegate()
            {
                ExecuteQueue(selectedApps);
            }));
            worker.IsBackground = true;
            worker.Start();
        }

        private void ExecuteQueue(List<AppItem> apps)
        {
            // Set modern TLS protocols for WebClient
            try
            {
                ServicePointManager.SecurityProtocol = (SecurityProtocolType)3072 | (SecurityProtocolType)12288 | SecurityProtocolType.Tls12;
            }
            catch { }

            int total = apps.Count;
            int current = 0;
            int successCount = 0;
            int failedCount = 0;

            string cacheDir = ConfigManager.GetCacheDirectory();
            string tempDir = Path.Combine(Path.GetTempPath(), "TechInstaller");
            if (!Directory.Exists(tempDir))
            {
                Directory.CreateDirectory(tempDir);
            }

            Log(string.Format("=== STARTING BATCH INSTALLATION ({0} items selected) ===", total), LogLevel.Info);

            foreach (AppItem app in apps)
            {
                if (_cancelRequested)
                {
                    Log("Installation cancelled by user.", LogLevel.Warning);
                    app.Status = "Cancelled";
                    if (OnAppStatusChanged != null) OnAppStatusChanged(app);
                    break;
                }

                current++;
                int overallPercent = (int)(((float)(current - 1) / total) * 100);
                if (OnOverallProgress != null)
                {
                    OnOverallProgress(overallPercent, string.Format("Processing {0}/{1}: {2}", current, total, app.Name));
                }

                Log(string.Format("\n[{0}/{1}] Starting: {2}", current, total, app.Name), LogLevel.Info);
                app.Status = "Processing";
                if (OnAppStatusChanged != null) OnAppStatusChanged(app);

                bool result = false;
                try
                {
                    if (!string.IsNullOrEmpty(app.SpecialAction))
                    {
                        result = HandleSpecialAction(app, tempDir, cacheDir);
                    }
                    else
                    {
                        result = HandleStandardApp(app, tempDir, cacheDir);
                    }
                }
                catch (Exception ex)
                {
                    Log(string.Format("Exception installing {0}: {1}", app.Name, ex.Message), LogLevel.Error);
                    app.Status = "Failed";
                    app.ErrorMessage = ex.Message;
                    result = false;
                }

                if (result)
                {
                    app.Status = "Installed";
                    successCount++;
                    Log(string.Format("[SUCCESS] Finished installing: {0}", app.Name), LogLevel.Success);
                }
                else
                {
                    if (app.Status != "Cancelled")
                    {
                        app.Status = "Failed";
                        failedCount++;
                        Log(string.Format("[FAILED] Failed to install: {0}", app.Name), LogLevel.Error);
                    }
                }

                if (OnAppStatusChanged != null) OnAppStatusChanged(app);
            }

            if (OnOverallProgress != null)
            {
                OnOverallProgress(100, "All items completed.");
            }
            if (OnCurrentProgress != null)
            {
                OnCurrentProgress(100, "Done");
            }

            Log(string.Format("\n=== BATCH SUMMARY: {0} Succeeded, {1} Failed ===", successCount, failedCount), 
                failedCount == 0 ? LogLevel.Success : LogLevel.Warning);

            IsBusy = false;
        }

        private bool HandleStandardApp(AppItem app, string tempDir, string cacheDir)
        {
            string targetInstallerPath = null;

            // 1. Check if cached in USB
            if (!string.IsNullOrEmpty(app.CacheFileName))
            {
                string cachedCandidate = Path.Combine(cacheDir, app.CacheFileName);
                if (File.Exists(cachedCandidate) && new FileInfo(cachedCandidate).Length > 100)
                {
                    targetInstallerPath = cachedCandidate;
                    Log(string.Format("[CACHE HIT] Found {0} in USB cache.", app.CacheFileName), LogLevel.Info);
                }
            }

            // 2. If not cached, download it
            if (string.IsNullOrEmpty(targetInstallerPath))
            {
                if (string.IsNullOrEmpty(app.DownloadUrl))
                {
                    Log(string.Format("No download URL or cached file for {0}.", app.Name), LogLevel.Error);
                    return false;
                }

                // Prefer saving directly to USB cache so future installs are offline
                string destinationPath = Path.Combine(cacheDir, app.CacheFileName);
                bool canWriteToCache = true;
                try
                {
                    using (FileStream fs = File.Create(destinationPath, 1, FileOptions.DeleteOnClose)) { }
                }
                catch
                {
                    canWriteToCache = false;
                    destinationPath = Path.Combine(tempDir, app.CacheFileName);
                }

                Log(string.Format("[DOWNLOAD] Downloading {0}...", app.Name), LogLevel.Download);
                app.Status = "Downloading";
                if (OnAppStatusChanged != null) OnAppStatusChanged(app);

                bool downloaded = DownloadFileWithProgress(app.DownloadUrl, destinationPath, app.Name);
                if (!downloaded || _cancelRequested)
                {
                    return false;
                }

                targetInstallerPath = destinationPath;
                if (canWriteToCache)
                {
                    app.IsCached = true;
                    app.LocalFilePath = targetInstallerPath;
                }
            }

            // 3. Execute silent installer
            Log(string.Format("[INSTALLING] Executing silent installer for {0}...", app.Name), LogLevel.Info);
            app.Status = "Installing";
            if (OnAppStatusChanged != null) OnAppStatusChanged(app);

            return ExecuteInstallerProcess(targetInstallerPath, app.SilentArgs);
        }

        private bool HandleSpecialAction(AppItem app, string tempDir, string cacheDir)
        {
            if (app.SpecialAction == "enable_net35")
            {
                Log("[ACTION] Enabling .NET Framework 3.5 via DISM...", LogLevel.Info);
                return ExecuteProcess("dism.exe", "/online /enable-feature /featurename:NetFX3 /all /quiet /norestart");
            }
            else if (app.SpecialAction == "windows_activation")
            {
                Log("[ACTION] Opening Windows Activation Settings...", LogLevel.Info);
                SystemToolsManager.OpenActivationSettings();
                return true;
            }
            else if (app.SpecialAction == "custom_powershell" || app.SpecialAction == "custom_script" || app.SpecialAction == "mas_activate")
            {
                Log("[ACTION] Launching Administrator PowerShell for custom script...", LogLevel.Info);
                return SystemToolsManager.OpenCustomPowerShellScript();
            }
            else if (app.SpecialAction == "directx_offline")
            {
                return DeployDirectXOffline(app, tempDir, cacheDir);
            }
            else if (app.SpecialAction == "office_odt")
            {
                return DeployMicrosoftOffice(app, tempDir, cacheDir);
            }

            return false;
        }

        private bool DeployDirectXOffline(AppItem app, string tempDir, string cacheDir)
        {
            string dxInstaller = Path.Combine(cacheDir, app.CacheFileName);
            if (!File.Exists(dxInstaller))
            {
                Log("[DOWNLOAD] Downloading DirectX June 2010 Offline Redistributable...", LogLevel.Download);
                bool dl = DownloadFileWithProgress(app.DownloadUrl, dxInstaller, "DirectX June 2010");
                if (!dl) return false;
            }

            string dxExtractDir = Path.Combine(tempDir, "DirectXExtract");
            if (Directory.Exists(dxExtractDir))
            {
                try { Directory.Delete(dxExtractDir, true); } catch { }
            }
            Directory.CreateDirectory(dxExtractDir);

            Log("[EXTRACT] Extracting DirectX cabinet files...", LogLevel.Info);
            bool extracted = ExecuteProcess(dxInstaller, string.Format("/Q /T:\"{0}\"", dxExtractDir));

            string dxSetupExe = Path.Combine(dxExtractDir, "DXSETUP.exe");
            if (!File.Exists(dxSetupExe))
            {
                Thread.Sleep(2000);
            }

            if (!File.Exists(dxSetupExe))
            {
                Log("DXSETUP.exe not found after extraction.", LogLevel.Error);
                return false;
            }

            Log("[INSTALLING] Installing DirectX 9.0c/10/11 runtimes silently...", LogLevel.Info);
            bool installed = ExecuteProcess(dxSetupExe, "/silent");

            try { Directory.Delete(dxExtractDir, true); } catch { }

            return installed;
        }

        private bool DeployMicrosoftOffice(AppItem app, string tempDir, string cacheDir)
        {
            string odtInstaller = Path.Combine(cacheDir, "officedeploymenttool.exe");
            if (!File.Exists(odtInstaller))
            {
                Log("[DOWNLOAD] Downloading Microsoft Office Deployment Tool...", LogLevel.Download);
                bool dl = DownloadFileWithProgress(app.DownloadUrl, odtInstaller, "Office Deployment Tool");
                if (!dl) return false;
            }

            string officeDir = Path.Combine(tempDir, "OfficeODT");
            if (!Directory.Exists(officeDir)) Directory.CreateDirectory(officeDir);

            Log("[EXTRACT] Extracting Office Deployment Tool...", LogLevel.Info);
            bool extracted = ExecuteProcess(odtInstaller, string.Format("/extract:\"{0}\" /quiet", officeDir));
            
            string setupExe = Path.Combine(officeDir, "setup.exe");
            if (!File.Exists(setupExe))
            {
                // In some cases extraction takes a couple seconds
                Thread.Sleep(2000);
            }

            if (!File.Exists(setupExe))
            {
                Log("setup.exe not found after extracting ODT.", LogLevel.Error);
                return false;
            }

            // Create custom configuration.xml
            string xmlPath = Path.Combine(officeDir, "install_config.xml");
            StringBuilder xml = new StringBuilder();
            xml.AppendLine("<Configuration>");
            xml.AppendLine("  <Add OfficeClientEdition=\"64\" Channel=\"Current\">");
            xml.AppendLine("    <Product ID=\"ProPlus2021Volume\">");
            xml.AppendLine("      <Language ID=\"en-us\" />");
            xml.AppendLine("      <ExcludeApp ID=\"Access\" />");
            xml.AppendLine("      <ExcludeApp ID=\"Groove\" />");
            xml.AppendLine("      <ExcludeApp ID=\"Lync\" />");
            xml.AppendLine("      <ExcludeApp ID=\"OneDrive\" />");
            xml.AppendLine("      <ExcludeApp ID=\"OneNote\" />");
            xml.AppendLine("      <ExcludeApp ID=\"Publisher\" />");
            xml.AppendLine("      <ExcludeApp ID=\"Teams\" />");
            xml.AppendLine("    </Product>");
            xml.AppendLine("  </Add>");
            xml.AppendLine("  <Display Level=\"Full\" AcceptEULA=\"TRUE\" />");
            xml.AppendLine("  <Property Name=\"AUTOACTIVATE\" Value=\"0\" />");
            xml.AppendLine("</Configuration>");

            File.WriteAllText(xmlPath, xml.ToString());

            Log("[INSTALLING] Installing Microsoft Office (Word, Excel, PowerPoint)... Please wait.", LogLevel.Info);
            return ExecuteProcess(setupExe, string.Format("/configure \"{0}\"", xmlPath));
        }

        private bool DownloadFileWithProgress(string url, string destinationPath, string displayName)
        {
            try
            {
                using (WebClient client = new WebClient())
                {
                    client.Headers.Add("User-Agent", "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko)");

                    DateTime lastUpdate = DateTime.Now;
                    long lastBytes = 0;

                    client.DownloadProgressChanged += delegate(object sender, DownloadProgressChangedEventArgs e)
                    {
                        if (_cancelRequested)
                        {
                            client.CancelAsync();
                            return;
                        }

                        if ((DateTime.Now - lastUpdate).TotalMilliseconds > 250)
                        {
                            double speedKB = (e.BytesReceived - lastBytes) / 1024.0 / (DateTime.Now - lastUpdate).TotalSeconds;
                            lastBytes = e.BytesReceived;
                            lastUpdate = DateTime.Now;

                            string status = string.Format("{0}% ({1:0.0} MB / {2:0.0} MB) @ {3:0} KB/s",
                                e.ProgressPercentage,
                                e.BytesReceived / 1048576.0,
                                e.TotalBytesToReceive / 1048576.0,
                                speedKB);

                            if (OnCurrentProgress != null)
                            {
                                OnCurrentProgress(e.ProgressPercentage, status);
                            }
                        }
                    };

                    AutoResetEvent doneEvent = new AutoResetEvent(false);
                    Exception downloadEx = null;

                    client.DownloadFileCompleted += delegate(object sender, AsyncCompletedEventArgs e)
                    {
                        if (e.Error != null)
                        {
                            downloadEx = e.Error;
                        }
                        doneEvent.Set();
                    };

                    client.DownloadFileAsync(new Uri(url), destinationPath);
                    doneEvent.WaitOne();

                    if (_cancelRequested)
                    {
                        if (File.Exists(destinationPath)) File.Delete(destinationPath);
                        return false;
                    }

                    if (downloadEx != null)
                    {
                        Log(string.Format("Download error for {0}: {1}", displayName, downloadEx.Message), LogLevel.Error);
                        if (File.Exists(destinationPath)) File.Delete(destinationPath);
                        return false;
                    }

                    if (OnCurrentProgress != null)
                    {
                        OnCurrentProgress(100, "Download completed");
                    }
                    return true;
                }
            }
            catch (Exception ex)
            {
                Log(string.Format("Failed to download {0}: {1}", displayName, ex.Message), LogLevel.Error);
                return false;
            }
        }

        private bool ExecuteInstallerProcess(string filePath, string arguments)
        {
            string ext = Path.GetExtension(filePath).ToLowerInvariant();
            string fileName = filePath;
            string fullArgs = arguments;

            if (ext == ".msi")
            {
                fileName = "msiexec.exe";
                fullArgs = string.Format("/i \"{0}\" {1}", filePath, arguments);
            }

            return ExecuteProcess(fileName, fullArgs);
        }

        private bool ExecuteProcess(string fileName, string arguments)
        {
            try
            {
                ProcessStartInfo psi = new ProcessStartInfo();
                psi.FileName = fileName;
                psi.Arguments = arguments;
                psi.CreateNoWindow = true;
                psi.UseShellExecute = false;
                psi.RedirectStandardOutput = true;
                psi.RedirectStandardError = true;

                using (Process p = Process.Start(psi))
                {
                    if (p == null) return false;

                    // Read output asynchronously to prevent deadlock
                    p.BeginOutputReadLine();
                    p.BeginErrorReadLine();

                    // Wait up to 20 minutes
                    bool exited = p.WaitForExit(1200000);
                    if (!exited)
                    {
                        try { p.Kill(); } catch { }
                        Log(string.Format("Process timed out: {0}", fileName), LogLevel.Error);
                        return false;
                    }

                    int code = p.ExitCode;
                    // Common installer success codes: 0 = OK, 3010 = Reboot required (success), 1641 = Reboot initiated
                    if (code == 0 || code == 3010 || code == 1641)
                    {
                        if (code == 3010)
                        {
                            Log("Installer reported exit code 3010 (System restart recommended to complete changes).", LogLevel.Warning);
                        }
                        return true;
                    }
                    else
                    {
                        Log(string.Format("Installer exited with non-zero code: {0}", code), LogLevel.Warning);
                        // For some silent installers, non-zero may still mean already installed or minor warning
                        // But we return false so it flags in the summary
                        return false;
                    }
                }
            }
            catch (Exception ex)
            {
                Log(string.Format("Error launching process {0}: {1}", fileName, ex.Message), LogLevel.Error);
                return false;
            }
        }
    }
}
