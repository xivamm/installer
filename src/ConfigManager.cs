using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Web.Script.Serialization;

namespace TechInstaller
{
    public static class ConfigManager
    {
        private static readonly string ConfigFileName = "apps.json";
        private static readonly string CloudConfigFileName = "cloud_apps.json";
        private static readonly string GitHubCloudUrl = "https://raw.githubusercontent.com/xivamm/installer/main/output/cloud_apps.json";
        private static readonly string GitHubAppsUrl = "https://raw.githubusercontent.com/xivamm/installer/main/output/apps.json";

        public static string GetAppDirectory()
        {
            string loc = System.Reflection.Assembly.GetExecutingAssembly().Location;
            if (!string.IsNullOrEmpty(loc))
            {
                return Path.GetDirectoryName(loc);
            }
            return AppDomain.CurrentDomain.BaseDirectory;
        }

        public static string GetCacheDirectory()
        {
            string cacheDir = Path.Combine(GetAppDirectory(), "cache");
            if (!Directory.Exists(cacheDir))
            {
                try
                {
                    Directory.CreateDirectory(cacheDir);
                }
                catch
                {
                    // If USB is read-only or error, fallback to temp folder
                    cacheDir = Path.Combine(Path.GetTempPath(), "TechInstaller_Cache");
                    if (!Directory.Exists(cacheDir))
                    {
                        Directory.CreateDirectory(cacheDir);
                    }
                }
            }
            return cacheDir;
        }

        public static List<AppItem> LoadApps()
        {
            string configPath = Path.Combine(GetAppDirectory(), ConfigFileName);
            List<AppItem> list = null;

            if (File.Exists(configPath))
            {
                try
                {
                    string json = File.ReadAllText(configPath);
                    JavaScriptSerializer serializer = new JavaScriptSerializer();
                    list = serializer.Deserialize<List<AppItem>>(json);
                }
                catch (Exception ex)
                {
                    // Fallback to default if json is malformed
                    Console.WriteLine("Error reading apps.json: " + ex.Message);
                }
            }

            if (list == null || list.Count == 0)
            {
                list = AppCatalog.GetDefaultApps();
                // Auto-save default apps.json if not present
                try
                {
                    SaveApps(list);
                }
                catch { }
            }

            // Refresh cache status for each item
            RefreshCacheStatus(list);

            return list;
        }

        public static void RefreshCacheStatus(List<AppItem> apps)
        {
            string cacheDir = GetCacheDirectory();

            foreach (AppItem app in apps)
            {
                if (!string.IsNullOrEmpty(app.CacheFileName))
                {
                    string cachedFile = Path.Combine(cacheDir, app.CacheFileName);
                    if (File.Exists(cachedFile) && new FileInfo(cachedFile).Length > 100)
                    {
                        app.IsCached = true;
                        app.LocalFilePath = cachedFile;
                        app.Status = "Cached (Offline)";
                    }
                    else
                    {
                        app.IsCached = false;
                        app.LocalFilePath = null;
                        if (app.Status == "Cached (Offline)")
                        {
                            app.Status = "Pending";
                        }
                    }
                }
            }
        }

        public static void SaveApps(List<AppItem> apps)
        {
            string configPath = Path.Combine(GetAppDirectory(), ConfigFileName);
            JavaScriptSerializer serializer = new JavaScriptSerializer();
            string json = serializer.Serialize(apps);
            // Format JSON simply with indentations
            json = PrettyPrintJson(json);
            File.WriteAllText(configPath, json);
        }

        public static List<CloudAppItem> LoadCloudApps()
        {
            string configPath = Path.Combine(GetAppDirectory(), CloudConfigFileName);
            List<CloudAppItem> list = null;

            if (File.Exists(configPath))
            {
                try
                {
                    string json = File.ReadAllText(configPath);
                    JavaScriptSerializer serializer = new JavaScriptSerializer();
                    list = serializer.Deserialize<List<CloudAppItem>>(json);
                }
                catch (Exception ex)
                {
                    Console.WriteLine("Error reading cloud_apps.json: " + ex.Message);
                }
            }

            if (list == null || list.Count == 0)
            {
                list = GetDefaultCloudApps();
                try
                {
                    SaveCloudApps(list);
                }
                catch { }
            }

            return list;
        }

        public static void SaveCloudApps(List<CloudAppItem> list)
        {
            string configPath = Path.Combine(GetAppDirectory(), CloudConfigFileName);
            JavaScriptSerializer serializer = new JavaScriptSerializer();
            string json = serializer.Serialize(list);
            json = PrettyPrintJson(json);
            File.WriteAllText(configPath, json);
        }

        public static void OpenCloudConfigInEditor()
        {
            string configPath = Path.Combine(GetAppDirectory(), CloudConfigFileName);
            if (!File.Exists(configPath))
            {
                SaveCloudApps(GetDefaultCloudApps());
            }
            try
            {
                Process.Start("notepad.exe", configPath);
            }
            catch { }
        }

        private static List<CloudAppItem> GetDefaultCloudApps()
        {
            List<CloudAppItem> list = new List<CloudAppItem>();

            CloudAppItem item1 = new CloudAppItem();
            item1.Id = "custom_office_pkg";
            item1.Name = "Microsoft Office Custom Package (Google Drive)";
            item1.Category = "Productivity";
            item1.Description = "Pre-configured Microsoft Office full installer hosted on your personal Google Drive.";
            item1.DriveUrl = "https://drive.google.com/";
            item1.EstimatedSize = "2.8 GB";
            item1.Version = "2024 ProPlus";
            item1.Notes = "Replace this URL in cloud_apps.json with your own Google Drive sharing link.";
            list.Add(item1);

            CloudAppItem item2 = new CloudAppItem();
            item2.Id = "adobe_suite_pkg";
            item2.Name = "Photoshop & Graphic Suite (Google Drive)";
            item2.Category = "Design";
            item2.Description = "Graphic design software installer package hosted on Google Drive.";
            item2.DriveUrl = "https://drive.google.com/";
            item2.EstimatedSize = "3.2 GB";
            item2.Version = "Latest";
            item2.Notes = "Click 'Open in Browser' to access your cloud file directly.";
            list.Add(item2);

            CloudAppItem item3 = new CloudAppItem();
            item3.Id = "cad_engineering_pkg";
            item3.Name = "AutoCAD / Civil Engineering Tools (Google Drive)";
            item3.Category = "Engineering";
            item3.Description = "CAD engineering setup packages and templates repository.";
            item3.DriveUrl = "https://drive.google.com/";
            item3.EstimatedSize = "4.5 GB";
            item3.Version = "2024";
            item3.Notes = "Direct link or folder link to your personal Drive storage.";
            list.Add(item3);

            CloudAppItem item4 = new CloudAppItem();
            item4.Id = "offline_drivers_pkg";
            item4.Name = "Snappy Driver Origin Full Pack (Google Drive)";
            item4.Category = "Drivers";
            item4.Description = "Gigantic full offline driver packs for network, audio, chipset, and GPUs.";
            item4.DriveUrl = "https://drive.google.com/";
            item4.EstimatedSize = "25 GB";
            item4.Version = "SDIO Latest";
            item4.Notes = "Store huge driver pack on cloud to save USB flash drive space.";
            list.Add(item4);

            return list;
        }

        public static bool FetchFromGitHub(out string resultMessage)
        {
            try
            {
                using (System.Net.WebClient client = new System.Net.WebClient())
                {
                    client.Headers.Add("User-Agent", "TechInstaller-App");
                    string cloudJson = client.DownloadString(GitHubCloudUrl);
                    if (!string.IsNullOrEmpty(cloudJson) && cloudJson.Contains("["))
                    {
                        JavaScriptSerializer serializer = new JavaScriptSerializer();
                        List<CloudAppItem> fetchedCloud = serializer.Deserialize<List<CloudAppItem>>(cloudJson);
                        if (fetchedCloud != null && fetchedCloud.Count > 0)
                        {
                            SaveCloudApps(fetchedCloud);
                        }
                    }

                    try
                    {
                        string appsJson = client.DownloadString(GitHubAppsUrl);
                        if (!string.IsNullOrEmpty(appsJson) && appsJson.Contains("["))
                        {
                            JavaScriptSerializer serializer = new JavaScriptSerializer();
                            List<AppItem> fetchedApps = serializer.Deserialize<List<AppItem>>(appsJson);
                            if (fetchedApps != null && fetchedApps.Count > 0)
                            {
                                SaveApps(fetchedApps);
                            }
                        }
                    }
                    catch { }

                    resultMessage = "Successfully fetched the latest apps and links from GitHub!";
                    return true;
                }
            }
            catch (Exception ex)
            {
                resultMessage = "Could not fetch from GitHub: " + ex.Message;
                return false;
            }
        }

        public static bool PushToGitHub(out string resultMessage)
        {
            try
            {
                ProcessStartInfo psi = new ProcessStartInfo();
                psi.FileName = "powershell.exe";
                psi.Arguments = "-NoExit -ExecutionPolicy Bypass -Command \"git add .; git commit -m 'Update apps catalog from TechInstaller'; git push origin main\"";
                psi.WorkingDirectory = GetAppDirectory();
                psi.UseShellExecute = true;
                Process.Start(psi);
                resultMessage = "Launched Git push process in PowerShell window.";
                return true;
            }
            catch (Exception ex)
            {
                resultMessage = "Error launching git push: " + ex.Message;
                return false;
            }
        }

        private static string PrettyPrintJson(string json)
        {
            if (string.IsNullOrEmpty(json)) return "";
            System.Text.StringBuilder sb = new System.Text.StringBuilder();
            bool inQuote = false;
            int indent = 0;

            for (int i = 0; i < json.Length; i++)
            {
                char ch = json[i];
                if (ch == '"' && (i == 0 || json[i - 1] != '\\'))
                {
                    inQuote = !inQuote;
                }

                if (!inQuote)
                {
                    if (ch == '{' || ch == '[')
                    {
                        sb.Append(ch);
                        sb.AppendLine();
                        indent++;
                        sb.Append(new string(' ', indent * 2));
                    }
                    else if (ch == '}' || ch == ']')
                    {
                        sb.AppendLine();
                        indent = Math.Max(0, indent - 1);
                        sb.Append(new string(' ', indent * 2));
                        sb.Append(ch);
                    }
                    else if (ch == ',')
                    {
                        sb.Append(ch);
                        sb.AppendLine();
                        sb.Append(new string(' ', indent * 2));
                    }
                    else if (ch == ':')
                    {
                        sb.Append(": ");
                    }
                    else
                    {
                        sb.Append(ch);
                    }
                }
                else
                {
                    sb.Append(ch);
                }
            }

            return sb.ToString();
        }
    }
}
