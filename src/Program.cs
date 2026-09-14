using System;
using System.Net;
using System.Security.Principal;
using System.Windows.Forms;

namespace TechInstaller
{
    static class Program
    {
        [STAThread]
        static void Main()
        {
            // Enforce modern TLS security protocols for high-speed downloads on modern CDNs
            try
            {
                ServicePointManager.SecurityProtocol = (SecurityProtocolType)3072 | (SecurityProtocolType)12288 | SecurityProtocolType.Tls12;
            }
            catch { }

            // Auto-elevate to Administrator privileges if running under standard user
            if (!IsAdministrator())
            {
                System.Diagnostics.ProcessStartInfo psi = new System.Diagnostics.ProcessStartInfo();
                psi.FileName = Application.ExecutablePath;
                psi.UseShellExecute = true;
                psi.Verb = "runas";
                try
                {
                    System.Diagnostics.Process.Start(psi);
                }
                catch
                {
                    MessageBox.Show("Administrator privileges are required to install system runtimes, software, and configure Windows settings.",
                        "Admin Elevation Required", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                }
                return;
            }

            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

            Application.Run(new MainForm());
        }

        public static bool IsAdministrator()
        {
            WindowsIdentity identity = WindowsIdentity.GetCurrent();
            WindowsPrincipal principal = new WindowsPrincipal(identity);
            return principal.IsInRole(WindowsBuiltInRole.Administrator);
        }
    }
}
