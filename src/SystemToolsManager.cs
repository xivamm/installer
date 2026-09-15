using System;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Runtime.InteropServices;
using System.Text;
using Microsoft.Win32;

namespace TechInstaller
{
    public static class SystemToolsManager
    {
        public static void OpenDeviceManager()
        {
            Process.Start("devmgmt.msc");
        }

        public static void OpenDateAndTimeSettings()
        {
            Process.Start("ms-settings:dateandtime");
        }

        public static void OpenTimeZoneSettings()
        {
            try
            {
                Process.Start("control.exe", "timedate.cpl,,/tz");
            }
            catch
            {
                Process.Start("ms-settings:dateandtime");
            }
        }

        public static void OpenDesktopBackgroundSettings()
        {
            try
            {
                Process.Start("ms-settings:personalization-background");
            }
            catch
            {
                Process.Start("control.exe", "desk.cpl,,@desktop");
            }
        }

        public static void OpenNetworkConnections()
        {
            Process.Start("ncpa.cpl");
        }

        public static void OpenActivationSettings()
        {
            try
            {
                Process.Start("ms-settings:activation");
            }
            catch
            {
                Process.Start("slmgr.vbs", "/dli");
            }
        }

        public static void OpenDiskManagement()
        {
            Process.Start("diskmgmt.msc");
        }

        public static void OpenSystemPropertiesAdvanced()
        {
            Process.Start("sysdm.cpl");
        }

        public static void OpenTaskManager()
        {
            Process.Start("taskmgr.exe");
        }

        public static void OpenServicesManager()
        {
            Process.Start("services.msc");
        }

        public static void OpenWindowsSecurity()
        {
            try
            {
                Process.Start("windowsdefender:");
            }
            catch
            {
                Process.Start("control.exe", "/name Microsoft.WindowsDefender");
            }
        }

        public static void OpenElevatedPowerShell()
        {
            try
            {
                ProcessStartInfo psi = new ProcessStartInfo();
                psi.FileName = "powershell.exe";
                psi.Arguments = "-NoExit -ExecutionPolicy Bypass";
                psi.UseShellExecute = true;
                Process.Start(psi);
            }
            catch { }
        }

        public static bool OpenCustomPowerShellScript()
        {
            string scriptsDir = System.IO.Path.Combine(ConfigManager.GetAppDirectory(), "scripts");
            if (!System.IO.Directory.Exists(scriptsDir))
            {
                try { System.IO.Directory.CreateDirectory(scriptsDir); } catch { }
            }

            string scriptPath = System.IO.Path.Combine(scriptsDir, "custom.ps1");
            if (!System.IO.File.Exists(scriptPath))
            {
                System.Text.StringBuilder sb = new System.Text.StringBuilder();
                sb.AppendLine("# ===========================================================");
                sb.AppendLine("# TechInstaller - Custom Post-Install Script (custom.ps1)");
                sb.AppendLine("# Dito mo ilalagay ang sarili mong mga PowerShell commands.");
                sb.AppendLine("# ===========================================================");
                sb.AppendLine("Write-Host 'Running TechInstaller custom script...' -ForegroundColor Cyan");
                sb.AppendLine("# Ilagay ang iyong code dito:");
                sb.AppendLine("# irm https://get.activated.win | iex");
                System.IO.File.WriteAllText(scriptPath, sb.ToString());
            }

            try
            {
                ProcessStartInfo psi = new ProcessStartInfo();
                psi.FileName = "powershell.exe";
                psi.Arguments = string.Format("-NoExit -ExecutionPolicy Bypass -File \"{0}\"", scriptPath);
                psi.UseShellExecute = true;
                Process.Start(psi);
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error launching custom script: " + ex.Message);
                return false;
            }
        }

        public static void EditCustomPowerShellScript()
        {
            string scriptsDir = System.IO.Path.Combine(ConfigManager.GetAppDirectory(), "scripts");
            if (!System.IO.Directory.Exists(scriptsDir))
            {
                try { System.IO.Directory.CreateDirectory(scriptsDir); } catch { }
            }

            string scriptPath = System.IO.Path.Combine(scriptsDir, "custom.ps1");
            if (!System.IO.File.Exists(scriptPath))
            {
                OpenCustomPowerShellScript();
            }

            try
            {
                Process.Start("notepad.exe", scriptPath);
            }
            catch { }
        }

        public static string SyncTimeNow()
        {
            try
            {
                // Ensure Windows Time service is running
                RunSilentCommand("net.exe", "start w32time");

                // Trigger resynchronization
                string output = RunSilentCommand("w32tm.exe", "/resync /force");
                if (output.Contains("command completed successfully") || output.Contains("The command completed"))
                {
                    return "System clock synchronized successfully with internet time server.";
                }
                return "Time Sync: " + output.Trim();
            }
            catch (Exception ex)
            {
                return "Failed to sync time: " + ex.Message;
            }
        }

        public static string FlushDns()
        {
            try
            {
                string output = RunSilentCommand("ipconfig.exe", "/flushdns");
                return "DNS Flush: " + output.Trim();
            }
            catch (Exception ex)
            {
                return "Failed to flush DNS: " + ex.Message;
            }
        }

        public static string SetDnsServers(string dnsType)
        {
            try
            {
                string script = "";
                if (dnsType == "cloudflare")
                {
                    script = "$adapters = Get-NetAdapter | Where-Object Status -eq 'Up'; foreach ($a in $adapters) { Set-DnsClientServerAddress -InterfaceIndex $a.InterfaceIndex -ServerAddresses ('1.1.1.1','1.0.0.1') }; 'Configured Cloudflare DNS (1.1.1.1, 1.0.0.1)'";
                }
                else if (dnsType == "google")
                {
                    script = "$adapters = Get-NetAdapter | Where-Object Status -eq 'Up'; foreach ($a in $adapters) { Set-DnsClientServerAddress -InterfaceIndex $a.InterfaceIndex -ServerAddresses ('8.8.8.8','8.8.4.4') }; 'Configured Google DNS (8.8.8.8, 8.8.4.4)'";
                }
                else if (dnsType == "dhcp")
                {
                    script = "$adapters = Get-NetAdapter | Where-Object Status -eq 'Up'; foreach ($a in $adapters) { Set-DnsClientServerAddress -InterfaceIndex $a.InterfaceIndex -ResetServerAddresses }; 'Reset DNS to Automatic (DHCP)'";
                }
                else
                {
                    return "Unknown DNS option.";
                }

                string output = RunSilentCommand("powershell.exe", "-NoProfile -ExecutionPolicy Bypass -Command \"" + script + "\"");
                FlushDns();
                return output.Trim();
            }
            catch (Exception ex)
            {
                return "Failed to set DNS: " + ex.Message;
            }
        }

        [DllImport("shell32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern void SHChangeNotify(int wEventId, uint uFlags, IntPtr dwItem1, IntPtr dwItem2);

        public static string EnableUltimatePerformance()
        {
            try
            {
                string outScheme = RunSilentCommand("powercfg.exe", "-duplicatescheme e9a42b02-d5df-448d-aa00-03f14749eb61");
                string guid = "e9a42b02-d5df-448d-aa00-03f14749eb61";
                int idx = outScheme.IndexOf("GUID: ");
                if (idx >= 0)
                {
                    int start = idx + 6;
                    if (start + 36 <= outScheme.Length)
                    {
                        guid = outScheme.Substring(start, 36).Trim();
                    }
                }
                RunSilentCommand("powercfg.exe", "-setactive " + guid);
                return "Ultimate Performance Power Plan activated successfully!";
            }
            catch (Exception ex)
            {
                return "Error configuring power plan: " + ex.Message;
            }
        }

        public static string ToggleShowFileExtensions()
        {
            try
            {
                using (RegistryKey key = Registry.CurrentUser.OpenSubKey(@"Software\Microsoft\Windows\CurrentVersion\Explorer\Advanced", true))
                {
                    if (key != null)
                    {
                        key.SetValue("HideFileExt", 0, RegistryValueKind.DWord);
                        key.SetValue("Hidden", 1, RegistryValueKind.DWord);
                    }
                }

                SHChangeNotify(0x08000000, 0x0000, IntPtr.Zero, IntPtr.Zero);
                return "Applied: File extensions (.exe, .zip, etc.) and hidden files are now visible!";
            }
            catch (Exception ex)
            {
                return "Failed to update registry: " + ex.Message;
            }
        }

        public static string DisableHibernation()
        {
            try
            {
                RunSilentCommand("powercfg.exe", "-h off");
                return "Hibernation disabled (hiberfil.sys deleted). Saved 8GB to 16GB storage on C: Drive!";
            }
            catch (Exception ex)
            {
                return "Error disabling hibernation: " + ex.Message;
            }
        }

        public static string CleanJunkAndTempFiles()
        {
            int deletedFiles = 0;
            long freedBytes = 0;

            string[] targetDirs = new string[]
            {
                Path.GetTempPath(),
                @"C:\Windows\Temp",
                @"C:\Windows\Prefetch"
            };

            foreach (string dir in targetDirs)
            {
                if (!Directory.Exists(dir)) continue;
                try
                {
                    DirectoryInfo di = new DirectoryInfo(dir);
                    foreach (FileInfo fi in di.GetFiles("*", SearchOption.AllDirectories))
                    {
                        try
                        {
                            long len = fi.Length;
                            fi.Delete();
                            freedBytes += len;
                            deletedFiles++;
                        }
                        catch { }
                    }
                    foreach (DirectoryInfo sub in di.GetDirectories())
                    {
                        try
                        {
                            sub.Delete(true);
                        }
                        catch { }
                    }
                }
                catch { }
            }

            double freedMB = Math.Round((double)freedBytes / 1048576.0, 1);
            return string.Format("Cleaned {0} temporary files, freed approx {1} MB of disk space!", deletedFiles, freedMB);
        }

        public static void RunSystemFileCheck()
        {
            try
            {
                ProcessStartInfo psi = new ProcessStartInfo();
                psi.FileName = "cmd.exe";
                psi.Arguments = "/k title System File Integrity Repair && echo Starting SFC Scannow... && sfc /scannow && echo. && echo Running DISM RestoreHealth... && dism /online /cleanup-image /restorehealth && echo. && echo [DONE] System check complete.";
                psi.UseShellExecute = true;
                Process.Start(psi);
            }
            catch { }
        }

        public static string LaunchOrDeployDriverBooster()
        {
            return LaunchOrDeployPortableApp("IObit Driver Booster", "DriverBoosterPortable.zip", @"C:\Tools\DriverBooster", "DriverBoosterPortable.exe");
        }

        public static string LaunchCrystalDiskInfo()
        {
            return LaunchOrDeployPortableApp("CrystalDiskInfo", "CrystalDiskInfo9_4_4.zip", @"C:\Tools\CrystalDiskInfo", "DiskInfo64.exe");
        }

        public static string LaunchCpuZ()
        {
            return LaunchOrDeployPortableApp("CPU-Z", "cpu-z_2.11-en.zip", @"C:\Tools\CPU-Z", "cpuz_x64.exe");
        }

        public static string LaunchOrDeployPortableApp(string appTitle, string zipName, string targetDir, string mainExeName)
        {
            string portableExe = Path.Combine(targetDir, mainExeName);
            if (File.Exists(portableExe))
            {
                Process.Start(portableExe);
                return "Launched " + appTitle + " from " + portableExe;
            }

            string cacheDir = ConfigManager.GetCacheDirectory();
            string zipPath = Path.Combine(cacheDir, zipName);
            if (!File.Exists(zipPath))
            {
                zipPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, zipName);
            }

            if (!File.Exists(zipPath))
            {
                return zipName + " not found in USB cache. Place it in 'cache' folder or install via Software tab.";
            }

            if (!Directory.Exists(targetDir))
            {
                Directory.CreateDirectory(targetDir);
            }

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
            }
            catch (Exception ex)
            {
                return "Extraction error: " + ex.Message;
            }

            // Desktop shortcut
            try
            {
                Type shellType = Type.GetTypeFromProgID("WScript.Shell");
                if (shellType != null)
                {
                    object shell = Activator.CreateInstance(shellType);
                    string desktopPath = Environment.GetFolderPath(Environment.SpecialFolder.DesktopDirectory);
                    string shortcutFile = Path.Combine(desktopPath, appTitle + ".lnk");
                    object shortcut = shellType.InvokeMember("CreateShortcut",
                        System.Reflection.BindingFlags.InvokeMethod, null, shell, new object[] { shortcutFile });
                    if (shortcut != null)
                    {
                        Type scType = shortcut.GetType();
                        scType.InvokeMember("TargetPath", System.Reflection.BindingFlags.SetProperty, null, shortcut, new object[] { portableExe });
                        scType.InvokeMember("WorkingDirectory", System.Reflection.BindingFlags.SetProperty, null, shortcut, new object[] { targetDir });
                        scType.InvokeMember("Description", System.Reflection.BindingFlags.SetProperty, null, shortcut, new object[] { appTitle + " Portable" });
                        scType.InvokeMember("Save", System.Reflection.BindingFlags.InvokeMethod, null, shortcut, null);
                    }
                }
            }
            catch { }

            if (File.Exists(portableExe))
            {
                Process.Start(portableExe);
                return "Extracted to " + targetDir + ", created Desktop shortcut, and launched " + appTitle + "!";
            }

            string[] anyExe = Directory.GetFiles(targetDir, "*.exe", SearchOption.AllDirectories);
            if (anyExe.Length > 0)
            {
                Process.Start(anyExe[0]);
                return "Extracted and launched " + Path.GetFileName(anyExe[0]);
            }

            return "Extracted to " + targetDir + " successfully.";
        }

        private static string RunSilentCommand(string fileName, string arguments)
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
                if (p == null) return "";
                string stdout = p.StandardOutput.ReadToEnd();
                string stderr = p.StandardError.ReadToEnd();
                p.WaitForExit(15000);
                if (!string.IsNullOrEmpty(stderr) && string.IsNullOrEmpty(stdout))
                {
                    return stderr;
                }
                return stdout;
            }
        }
    }
}
