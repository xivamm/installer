using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
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
            else if (app.SpecialAction == "portable_driver_booster" || app.SpecialAction == "portable_extract")
            {
                return DeployPortableApp(app, tempDir, cacheDir);
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

        private bool DeployPortableApp(AppItem app, string tempDir, string cacheDir)
        {
            string zipPath = Path.Combine(cacheDir, app.CacheFileName);
            if (!File.Exists(zipPath))
            {
                string localCandidate = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, app.CacheFileName);
                if (File.Exists(localCandidate))
                {
                    zipPath = localCandidate;
                }
            }

            if (!File.Exists(zipPath))
            {
                if (string.IsNullOrEmpty(app.DownloadUrl))
                {
                    Log(string.Format("Archive not found in cache: {0}", app.CacheFileName), LogLevel.Error);
                    return false;
                }

                Log(string.Format("[DOWNLOAD] Downloading {0}...", app.Name), LogLevel.Download);
                bool dl = DownloadFileWithProgress(app.DownloadUrl, zipPath, app.Name);
                if (!dl || !File.Exists(zipPath))
                {
                    return false;
                }
            }

            string folderName = "DriverBooster";
            if (app.Id.IndexOf("driver", StringComparison.OrdinalIgnoreCase) >= 0)
            {
                folderName = "DriverBooster";
            }
            else
            {
                folderName = Path.GetFileNameWithoutExtension(app.CacheFileName).Replace("Portable", "").Replace("portable", "").Trim();
                if (string.IsNullOrEmpty(folderName)) folderName = app.Name;
            }

            string targetDir = Path.Combine(@"C:\Tools", folderName);
            try
            {
                if (!Directory.Exists(targetDir))
                {
                    Directory.CreateDirectory(targetDir);
                }
            }
            catch (Exception ex)
            {
                Log(string.Format("Failed to create directory {0}: {1}", targetDir, ex.Message), LogLevel.Error);
                return false;
            }

            Log(string.Format("[EXTRACT] Unzipping {0} to {1}...", app.Name, targetDir), LogLevel.Info);

            bool extractSuccess = false;
            try
            {
                using (ZipArchive archive = ZipFile.OpenRead(zipPath))
                {
                    foreach (ZipArchiveEntry entry in archive.Entries)
                    {
                        string destPath = Path.Combine(targetDir, entry.FullName);
                        string entryDir = Path.GetDirectoryName(destPath);
                        if (!string.IsNullOrEmpty(entryDir) && !Directory.Exists(entryDir))
                        {
                            Directory.CreateDirectory(entryDir);
                        }

                        if (!string.IsNullOrEmpty(entry.Name))
                        {
                            entry.ExtractToFile(destPath, true);
                        }
                    }
                }
                extractSuccess = true;
            }
            catch (Exception ex)
            {
                Log(string.Format("Zip extraction notice: {0}. Trying fallback extractor...", ex.Message), LogLevel.Warning);
                extractSuccess = ExecuteProcess("tar.exe", string.Format("-xf \"{0}\" -C \"{1}\"", zipPath, targetDir));
                if (!extractSuccess)
                {
                    string psCmd = string.Format("Expand-Archive -Path '{0}' -DestinationPath '{1}' -Force", zipPath, targetDir);
                    extractSuccess = ExecuteProcess("powershell.exe", string.Format("-NoProfile -ExecutionPolicy Bypass -Command \"{0}\"", psCmd));
                }
            }

            if (!extractSuccess)
            {
                Log("Extraction failed.", LogLevel.Error);
                return false;
            }

            // Find executable
            string targetExe = null;
            string directExe = Path.Combine(targetDir, "DriverBoosterPortable.exe");
            if (File.Exists(directExe))
            {
                targetExe = directExe;
            }
            else
            {
                string[] exeCandidates = Directory.GetFiles(targetDir, "*.exe", SearchOption.AllDirectories);
                foreach (string candidate in exeCandidates)
                {
                    string fn = Path.GetFileName(candidate).ToLowerInvariant();
                    if (fn.Contains("driver") || fn.Contains("booster") || fn.Contains("portable"))
                    {
                        targetExe = candidate;
                        break;
                    }
                }
                if (string.IsNullOrEmpty(targetExe) && exeCandidates.Length > 0)
                {
                    targetExe = exeCandidates[0];
                }
            }

            if (string.IsNullOrEmpty(targetExe) || !File.Exists(targetExe))
            {
                Log("Warning: Could not locate main executable inside extracted folder.", LogLevel.Warning);
                return true;
            }

            // Create Desktop Shortcut
            string shortcutTitle = "IObit Driver Booster";
            if (app.Id.IndexOf("driver", StringComparison.OrdinalIgnoreCase) < 0)
            {
                shortcutTitle = app.Name;
            }

            string userDesktop = Environment.GetFolderPath(Environment.SpecialFolder.DesktopDirectory);
            string desktopLnk = Path.Combine(userDesktop, shortcutTitle + ".lnk");
            CreateWindowsShortcut(targetExe, desktopLnk, app.Name);

            try
            {
                string commonDesktop = Environment.GetFolderPath(Environment.SpecialFolder.CommonDesktopDirectory);
                if (!string.IsNullOrEmpty(commonDesktop) && Directory.Exists(commonDesktop))
                {
                    string publicLnk = Path.Combine(commonDesktop, shortcutTitle + ".lnk");
                    CreateWindowsShortcut(targetExe, publicLnk, app.Name);
                }
            }
            catch { }

            try
            {
                string commonStartMenu = Environment.GetFolderPath(Environment.SpecialFolder.CommonStartMenu);
                string programsDir = Path.Combine(commonStartMenu, "Programs");
                if (Directory.Exists(programsDir))
                {
                    string startLnk = Path.Combine(programsDir, shortcutTitle + ".lnk");
                    CreateWindowsShortcut(targetExe, startLnk, app.Name);
                }
            }
            catch { }

            Log(string.Format("[SUCCESS] {0} ready at {1}", app.Name, targetDir), LogLevel.Success);
            Log(string.Format("[SHORTCUT] Created Desktop & Start Menu shortcut: '{0}'", shortcutTitle), LogLevel.Success);
            return true;
        }

        private void CreateWindowsShortcut(string targetPath, string shortcutPath, string description)
        {
            try
            {
                Type shellType = Type.GetTypeFromProgID("WScript.Shell");
                if (shellType == null) return;
                object shell = Activator.CreateInstance(shellType);
                object shortcut = shellType.InvokeMember("CreateShortcut",
                    System.Reflection.BindingFlags.InvokeMethod, null, shell, new object[] { shortcutPath });
                if (shortcut == null) return;

                Type scType = shortcut.GetType();
                scType.InvokeMember("TargetPath", System.Reflection.BindingFlags.SetProperty, null, shortcut, new object[] { targetPath });
                scType.InvokeMember("WorkingDirectory", System.Reflection.BindingFlags.SetProperty, null, shortcut, new object[] { Path.GetDirectoryName(targetPath) });
                scType.InvokeMember("Description", System.Reflection.BindingFlags.SetProperty, null, shortcut, new object[] { description });
                scType.InvokeMember("Save", System.Reflection.BindingFlags.InvokeMethod, null, shortcut, null);
            }
            catch (Exception ex)
            {
                Log(string.Format("Notice: Could not save shortcut ({0})", ex.Message), LogLevel.Warning);
            }
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
