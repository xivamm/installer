using System;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Net;
using System.Runtime.InteropServices;
using System.Text;
using System.Windows.Forms;
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

        public static void OpenEventViewer()
        {
            try
            {
                Process.Start("eventvwr.msc");
            }
            catch (Exception ex)
            {
                MessageBox.Show("Could not launch Event Viewer: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
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
            return LaunchOrDeployPortableApp("IObit Driver Booster", "DriverBoosterPortable.zip", @"C:\Tools\DriverBooster", "DriverBoosterPortable.exe", "https://raw.githubusercontent.com/xivamm/installer/main/output/cache/DriverBoosterPortable.zip");
        }

        public static string LaunchCrystalDiskInfo()
        {
            return LaunchOrDeployPortableApp("CrystalDiskInfo", "CrystalDiskInfo9_4_4.zip", @"C:\Tools\CrystalDiskInfo", "DiskInfo64.exe", "https://downloads.sourceforge.net/project/crystaldiskinfo/9.4.4/CrystalDiskInfo9_4_4.zip");
        }

        public static string LaunchCpuZ()
        {
            return LaunchOrDeployPortableApp("CPU-Z", "cpu-z_2.11-en.zip", @"C:\Tools\CPU-Z", "cpuz_x64.exe", "https://download.cpuid.com/cpu-z/cpu-z_2.11-en.zip");
        }

        public static string LaunchOrDeployPortableApp(string appTitle, string zipName, string targetDir, string mainExeName, string downloadUrl = null)
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

            if (!File.Exists(zipPath) && !string.IsNullOrEmpty(downloadUrl))
            {
                try
                {
                    if (!Directory.Exists(cacheDir))
                    {
                        Directory.CreateDirectory(cacheDir);
                    }
                    string downloadTarget = Path.Combine(cacheDir, zipName);
                    using (WebClient client = new WebClient())
                    {
                        client.Headers.Add("User-Agent", "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko)");
                        client.DownloadFile(downloadUrl, downloadTarget);
                    }
                    if (File.Exists(downloadTarget))
                    {
                        zipPath = downloadTarget;
                    }
                }
                catch (Exception ex)
                {
                    return "Could not download " + zipName + " (" + ex.Message + "). Place it in 'cache' or install via Software tab.";
                }
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

        public static string RenameComputer(string newName)
        {
            if (string.IsNullOrEmpty(newName) || newName.Trim().Length == 0)
            {
                return "Error: Computer name cannot be empty.";
            }

            newName = newName.Trim();
            if (newName.Length > 15)
            {
                return "Error: Computer name must be 15 characters or fewer (NetBIOS limit).";
            }

            try
            {
                string script = string.Format("Rename-Computer -NewName '{0}' -Force -ErrorAction Stop; 'SUCCESS'", newName);
                string output = RunSilentCommand("powershell.exe", "-NoProfile -ExecutionPolicy Bypass -Command \"" + script + "\"");
                if (output.Contains("SUCCESS"))
                {
                    return string.Format("Computer successfully renamed to '{0}'! Please restart the computer to apply the new name.", newName);
                }
                return "Rename notice: " + output.Trim();
            }
            catch (Exception ex)
            {
                return "Failed to rename computer: " + ex.Message;
            }
        }

        public static string DisableAllNetworkAdapters()
        {
            try
            {
                string script = "$adapters = Get-NetAdapter | Where-Object Status -ne 'Disabled'; foreach ($a in $adapters) { Disable-NetAdapter -InterfaceIndex $a.InterfaceIndex -Confirm:$false }; 'Disabled ' + $adapters.Count + ' network adapter(s). Internet is now disconnected.'";
                string output = RunSilentCommand("powershell.exe", "-NoProfile -ExecutionPolicy Bypass -Command \"" + script + "\"");
                return output.Trim();
            }
            catch (Exception ex)
            {
                return "Failed to disable network adapters: " + ex.Message;
            }
        }

        public static string EnableAllNetworkAdapters()
        {
            try
            {
                string script = "$adapters = Get-NetAdapter | Where-Object Status -eq 'Disabled'; foreach ($a in $adapters) { Enable-NetAdapter -InterfaceIndex $a.InterfaceIndex -Confirm:$false }; 'Enabled ' + $adapters.Count + ' network adapter(s). Network connectivity restored.'";
                string output = RunSilentCommand("powershell.exe", "-NoProfile -ExecutionPolicy Bypass -Command \"" + script + "\"");
                return output.Trim();
            }
            catch (Exception ex)
            {
                return "Failed to enable network adapters: " + ex.Message;
            }
        }

        public static string TestNetworkConnection()
        {
            try
            {
                string script = "$pingC = Test-Connection -ComputerName 1.1.1.1 -Count 2 -Quiet -ErrorAction SilentlyContinue; $pingG = Test-Connection -ComputerName 8.8.8.8 -Count 2 -Quiet -ErrorAction SilentlyContinue; $gw = (Get-NetRoute -DestinationPrefix '0.0.0.0/0' -ErrorAction SilentlyContinue | Select-Object -First 1).NextHop; $ad = (Get-NetAdapter | Where-Object Status -eq 'Up' | Select-Object -ExpandProperty Name) -join ', '; '=== NETWORK CONNECTIVITY TEST ==='; 'Active Interface(s) : ' + $(if ($ad) { $ad } else { 'None (Offline)' }); 'Default Gateway     : ' + $(if ($gw) { $gw } else { 'No Gateway' }); 'Cloudflare (1.1.1.1): ' + $(if ($pingC) { 'REACHABLE (OK)' } else { 'UNREACHABLE' }); 'Google DNS (8.8.8.8): ' + $(if ($pingG) { 'REACHABLE (OK)' } else { 'UNREACHABLE' }); 'Status              : ' + $(if ($pingC -or $pingG) { 'ONLINE - Internet working properly' } else { 'OFFLINE - No internet access' })";
                string output = RunSilentCommand("powershell.exe", "-NoProfile -ExecutionPolicy Bypass -Command \"" + script + "\"");
                return output.Trim();
            }
            catch (Exception ex)
            {
                return "Network test error: " + ex.Message;
            }
        }

        public static string GetSystemHealthSummary()
        {
            try
            {
                string script = "$os = Get-CimInstance Win32_OperatingSystem; $up = (Get-Date) - $os.LastBootUpTime; $cpu = Get-CimInstance Win32_Processor | Select-Object -First 1; $tRam = [Math]::Round($os.TotalVisibleMemorySize / 1MB, 2); $fRam = [Math]::Round($os.FreePhysicalMemory / 1MB, 2); $uRam = [Math]::Round($tRam - $fRam, 2); $rPct = [Math]::Round(($uRam / $tRam) * 100, 1); '=================================================='; '             SYSTEM HEALTH DIAGNOSTIC             '; '=================================================='; 'OS Version   : ' + $os.Caption + ' (' + $os.Version + ' ' + $os.OSArchitecture + ')'; 'Uptime       : ' + [int]$up.TotalDays + ' days, ' + $up.Hours + ' hours, ' + $up.Minutes + ' mins'; 'Computer Name: ' + $env:COMPUTERNAME; 'CPU Model    : ' + $cpu.Name.Trim(); 'CPU Topology : ' + $cpu.NumberOfCores + ' Cores / ' + $cpu.NumberOfLogicalProcessors + ' Threads'; 'Memory (RAM) : ' + $uRam + ' GB used / ' + $tRam + ' GB total (' + $rPct + '% utilization)'; ''; 'STORAGE VOLUMES:'; foreach ($d in (Get-Volume | Where-Object { $_.DriveLetter } | Sort-Object DriveLetter)) { $tG = [Math]::Round($d.Size / 1GB, 1); $fG = [Math]::Round($d.SizeRemaining / 1GB, 1); $pFree = if ($tG -gt 0) { [Math]::Round(($fG / $tG) * 100, 1) } else { 0 }; '  Drive ' + $d.DriveLetter + ': [' + $d.FileSystemType + '] ' + $fG + ' GB free of ' + $tG + ' GB (' + $pFree + '% free) - ' + $d.HealthStatus }; $bat = Get-CimInstance Win32_Battery -ErrorAction SilentlyContinue; if ($bat) { ''; 'BATTERY: ' + $bat.EstimatedChargeRemaining + '% charge (' + $(if ($bat.BatteryStatus -eq 2) { 'Charging' } else { 'Discharging/AC' }) + ')' }; '=================================================='";
                string output = RunSilentCommand("powershell.exe", "-NoProfile -ExecutionPolicy Bypass -Command \"" + script + "\"");
                return output.Trim();
            }
            catch (Exception ex)
            {
                return "Failed to generate health summary: " + ex.Message;
            }
        }

        public static string GetRecentSystemErrors()
        {
            try
            {
                string script = "'=================================================='; '        RECENT SYSTEM & APPLICATION ERRORS        '; '               (Last 48 Hours)                    '; '=================================================='; $ev = Get-WinEvent -FilterHashtable @{LogName=@('System','Application'); Level=1,2; StartTime=(Get-Date).AddDays(-2)} -MaxEvents 10 -ErrorAction SilentlyContinue; if (!$ev -or $ev.Count -eq 0) { 'No critical or error events recorded in the last 48 hours! System is healthy.' } else { foreach ($e in $ev) { $lvl = switch ($e.Level) { 1 { '[CRITICAL]' } 2 { '[ERROR]   ' } default { '[INFO]    ' } }; $m = ($e.Message -replace '[\\r\\n\\t]+', ' ').Trim(); if ($m.Length -gt 85) { $m = $m.Substring(0, 85) + '...' }; ($lvl + ' ' + $e.TimeCreated.ToString('yyyy-MM-dd HH:mm') + ' | ' + $e.ProviderName + ' (' + $e.Id + '):'); ('  ' + $m) } }; $bsod = Get-WinEvent -FilterHashtable @{LogName='System'; ProviderName='Microsoft-Windows-WER-SystemErrorReporting','BugCheck'; StartTime=(Get-Date).AddDays(-7)} -MaxEvents 3 -ErrorAction SilentlyContinue; if ($bsod) { ''; 'CRASH / BSOD BUGCHECKS (Last 7 Days):'; foreach ($b in $bsod) { '  ' + $b.TimeCreated.ToString('yyyy-MM-dd HH:mm') + ' - ' + ($b.Message -replace '[\\r\\n\\t]+', ' ').Trim() } }; '=================================================='";
                string output = RunSilentCommand("powershell.exe", "-NoProfile -ExecutionPolicy Bypass -Command \"" + script + "\"");
                return output.Trim();
            }
            catch (Exception ex)
            {
                return "Failed to query system errors: " + ex.Message;
            }
        }

        public static string RestartWindowsExplorer()
        {
            try
            {
                foreach (Process p in Process.GetProcessesByName("explorer"))
                {
                    try { p.Kill(); p.WaitForExit(2000); } catch { }
                }
                System.Threading.Thread.Sleep(600);
                Process.Start("explorer.exe");
                return "Windows Explorer restarted successfully. Taskbar and desktop refreshed.";
            }
            catch (Exception ex)
            {
                return "Failed to restart explorer: " + ex.Message;
            }
        }

        public static string RebuildIconCache()
        {
            try
            {
                string script = "Stop-Process -Name explorer -Force -ErrorAction SilentlyContinue; Start-Sleep -Milliseconds 800; $cp = Join-Path $env:LOCALAPPDATA 'IconCache.db'; if (Test-Path $cp) { Remove-Item $cp -Force -ErrorAction SilentlyContinue }; $ec = Join-Path $env:LOCALAPPDATA 'Microsoft\\Windows\\Explorer'; if (Test-Path $ec) { Get-ChildItem -Path $ec -Filter 'iconcache*.db' -File -ErrorAction SilentlyContinue | Remove-Item -Force -ErrorAction SilentlyContinue; Get-ChildItem -Path $ec -Filter 'thumbcache*.db' -File -ErrorAction SilentlyContinue | Remove-Item -Force -ErrorAction SilentlyContinue }; Start-Sleep -Milliseconds 500; Start-Process explorer.exe; 'Icon and thumbnail cache purged. Windows Explorer restarted!'";
                string output = RunSilentCommand("powershell.exe", "-NoProfile -ExecutionPolicy Bypass -Command \"" + script + "\"");
                return output.Trim();
            }
            catch (Exception ex)
            {
                return "Failed to rebuild icon cache: " + ex.Message;
            }
        }

        public static string GenerateBatteryReport()
        {
            try
            {
                string reportPath = Path.Combine(Path.GetTempPath(), "battery-report.html");
                string res = RunSilentCommand("powercfg.exe", "/batteryreport /output \"" + reportPath + "\"");
                if (File.Exists(reportPath))
                {
                    Process.Start(reportPath);
                    return "Battery report generated: " + reportPath + " (opened in browser).";
                }
                return "Battery Report: " + res.Trim();
            }
            catch (Exception ex)
            {
                return "Failed to generate battery report: " + ex.Message;
            }
        }

        public static string PauseWindowsUpdates()
        {
            try
            {
                string script = "$pd = (Get-Date).AddDays(35).ToString('yyyy-MM-ddTHH:mm:ssZ'); $k = 'HKLM:\\SOFTWARE\\Microsoft\\WindowsUpdate\\UX\\Settings'; if (!(Test-Path $k)) { New-Item -Path $k -Force | Out-Null }; Set-ItemProperty -Path $k -Name 'PauseUpdatesExpiryTime' -Value $pd -Force; Set-ItemProperty -Path $k -Name 'PauseFeatureUpdatesStartTime' -Value (Get-Date).ToString('yyyy-MM-ddTHH:mm:ssZ') -Force; Set-ItemProperty -Path $k -Name 'PauseQualityUpdatesStartTime' -Value (Get-Date).ToString('yyyy-MM-ddTHH:mm:ssZ') -Force; 'Windows automatic updates successfully paused for 35 days (until ' + (Get-Date).AddDays(35).ToString('MMMM dd, yyyy') + ').'";
                string output = RunSilentCommand("powershell.exe", "-NoProfile -ExecutionPolicy Bypass -Command \"" + script + "\"");
                return output.Trim();
            }
            catch (Exception ex)
            {
                return "Failed to pause updates: " + ex.Message;
            }
        }

        public static string OpenGodMode()
        {
            try
            {
                Process.Start("shell:::{ED7BA470-8E54-465E-825C-99712043E01C}");
                return "Opened Windows GodMode (All Tasks Master Control Panel).";
            }
            catch
            {
                try
                {
                    string desktopPath = Environment.GetFolderPath(Environment.SpecialFolder.DesktopDirectory);
                    string godModeFolder = Path.Combine(desktopPath, "GodMode.{ED7BA470-8E54-465E-825C-99712043E01C}");
                    if (!Directory.Exists(godModeFolder))
                    {
                        Directory.CreateDirectory(godModeFolder);
                    }
                    Process.Start(godModeFolder);
                    return "Created GodMode folder on Desktop and opened master control panel.";
                }
                catch (Exception ex)
                {
                    return "Could not open GodMode: " + ex.Message;
                }
            }
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
