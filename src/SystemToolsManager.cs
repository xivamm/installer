using System;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Text;

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

        public static string LaunchOrDeployDriverBooster()
        {
            string portableExe = @"C:\Tools\DriverBooster\DriverBoosterPortable.exe";
            if (File.Exists(portableExe))
            {
                Process.Start(portableExe);
                return "Launched Driver Booster from " + portableExe;
            }

            string cacheDir = ConfigManager.GetCacheDirectory();
            string zipPath = Path.Combine(cacheDir, "DriverBoosterPortable.zip");
            if (!File.Exists(zipPath))
            {
                zipPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "DriverBoosterPortable.zip");
            }

            if (!File.Exists(zipPath))
            {
                return "DriverBoosterPortable.zip not found in cache. Place it in the 'cache' folder or install from Software tab.";
            }

            string targetDir = @"C:\Tools\DriverBooster";
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

            // Create desktop shortcut
            try
            {
                Type shellType = Type.GetTypeFromProgID("WScript.Shell");
                if (shellType != null)
                {
                    object shell = Activator.CreateInstance(shellType);
                    string desktopPath = Environment.GetFolderPath(Environment.SpecialFolder.DesktopDirectory);
                    string shortcutFile = Path.Combine(desktopPath, "IObit Driver Booster.lnk");
                    object shortcut = shellType.InvokeMember("CreateShortcut",
                        System.Reflection.BindingFlags.InvokeMethod, null, shell, new object[] { shortcutFile });
                    if (shortcut != null)
                    {
                        Type scType = shortcut.GetType();
                        scType.InvokeMember("TargetPath", System.Reflection.BindingFlags.SetProperty, null, shortcut, new object[] { portableExe });
                        scType.InvokeMember("WorkingDirectory", System.Reflection.BindingFlags.SetProperty, null, shortcut, new object[] { targetDir });
                        scType.InvokeMember("Description", System.Reflection.BindingFlags.SetProperty, null, shortcut, new object[] { "IObit Driver Booster Portable" });
                        scType.InvokeMember("Save", System.Reflection.BindingFlags.InvokeMethod, null, shortcut, null);
                    }
                }
            }
            catch { }

            if (File.Exists(portableExe))
            {
                Process.Start(portableExe);
                return "Extracted to " + targetDir + ", created Desktop shortcut, and launched Driver Booster!";
            }

            return "Extracted to " + targetDir + " but DriverBoosterPortable.exe was not found.";
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
