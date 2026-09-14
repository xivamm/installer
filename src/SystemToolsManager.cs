using System;
using System.Diagnostics;
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
