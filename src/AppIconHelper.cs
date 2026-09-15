using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.IO;
using System.Reflection;

namespace TechInstaller
{
    public static class AppIconHelper
    {
        private static readonly Dictionary<string, Image> _cache36 = new Dictionary<string, Image>(StringComparer.OrdinalIgnoreCase);
        private static readonly Dictionary<string, Image> _cache48 = new Dictionary<string, Image>(StringComparer.OrdinalIgnoreCase);
        private static readonly Dictionary<string, Image> _rawAssetCache = new Dictionary<string, Image>(StringComparer.OrdinalIgnoreCase);

        public static Image GetAppIcon(string id, string category, int size)
        {
            return GetAppIcon(id, id, category, size);
        }

        public static Image GetAppIcon(string id, string name, string category, int size)
        {
            int targetSize = (size > 36) ? 48 : 36;
            Dictionary<string, Image> cache = (targetSize == 36) ? _cache36 : _cache48;
            string key = string.Format("{0}_{1}_{2}_{3}", id ?? "", name ?? "", category ?? "", targetSize);

            if (cache.ContainsKey(key))
            {
                return cache[key];
            }

            Image rawLogo = ResolveRawLogo(id, name, category);
            Bitmap presentedIcon = CreatePresentedContainer(rawLogo, targetSize);
            cache[key] = presentedIcon;
            return presentedIcon;
        }

        private static Image ResolveRawLogo(string id, string name, string category)
        {
            string assetFileName = MapToAssetFileName(id, name);

            if (!string.IsNullOrEmpty(assetFileName))
            {
                if (_rawAssetCache.ContainsKey(assetFileName))
                {
                    return _rawAssetCache[assetFileName];
                }

                Image loaded = LoadAssetImage(assetFileName);
                if (loaded != null)
                {
                    _rawAssetCache[assetFileName] = loaded;
                    return loaded;
                }
            }

            // Fallback to checking cache folder for actual installer executable
            Image exeIcon = TryExtractFromCacheExe(id, name);
            if (exeIcon != null)
            {
                return exeIcon;
            }

            // Fallback: neutral generic fallback icon from the UI icon library
            string cleanId = (id ?? "").ToLowerInvariant();
            string cleanName = (name ?? "").ToLowerInvariant();
            if (cleanId.Contains("activation") || cleanName.Contains("activation") || cleanId.Contains("license"))
            {
                return UiIconHelper.GetIcon(UiIcon.Key, 24, Color.FromArgb(96, 165, 250));
            }
            return UiIconHelper.GetIcon(UiIcon.Package, 26, Color.FromArgb(96, 165, 250));
        }

        private static string MapToAssetFileName(string id, string name)
        {
            string cid = (id ?? "").ToLowerInvariant();
            string cname = (name ?? "").ToLowerInvariant();

            // Match by specific application identifiers
            if (cid.Contains("crystal") || cname.Contains("crystal") || cid.Contains("diskinfo") || cname.Contains("diskinfo"))
                return "crystaldiskinfo.png";

            if (cid.Contains("cpuz") || cname.Contains("cpu-z") || cid.Contains("cpu_z"))
                return "cpuz.png";

            if (cid.Contains("driver") || cname.Contains("driver booster"))
                return "iobit_driver_booster_portable.png";

            if (cid.Contains("vcredist") || cname.Contains("visual c") || cid.Contains("visualcpp"))
                return "vcredist_aio.png";

            if (cid.Contains("directx") || cname.Contains("directx"))
                return "directx_redist.png";

            if (cid.Contains("net35") || cname.Contains("3.5"))
                return "enable_net35.png";

            if (cid.Contains("dotnet") || cname.Contains(".net"))
                return "dotnet8_desktop.png";

            if (cid.Contains("chrome") || cname.Contains("chrome"))
                return "chrome.png";

            if (cid.Contains("brave") || cname.Contains("brave"))
                return "brave.png";

            if (cid.Contains("firefox") || cname.Contains("firefox") || cid.Contains("mozilla") || cname.Contains("mozilla"))
                return "firefox.png";

            if (cid.Contains("winrar") || cname.Contains("winrar") || cid.Contains("rar"))
                return "winrar.png";

            if (cid.Contains("7zip") || cid.Contains("sevenzip") || cname.Contains("7-zip") || cname.Contains("7zip") || cid.Contains("7z"))
                return "7zip.png";

            if (cid.Contains("anydesk") || cname.Contains("anydesk"))
                return "anydesk.png";

            if (cid.Contains("revo") || cname.Contains("revo"))
                return "revo_uninstaller.png";

            if (cid.Contains("office") || cname.Contains("office"))
                return "office.png";

            if (cid.Contains("sumatra") || cname.Contains("sumatra") || cid.Contains("pdf") || cname.Contains("pdf"))
                return "sumatrapdf.png";

            if (cid.Contains("vlc") || cname.Contains("vlc"))
                return "vlc.png";

            if (cid.Contains("steam") || cname.Contains("steam"))
                return "steam.png";

            if (cid.Contains("discord") || cname.Contains("discord"))
                return "discord.png";

            return null;
        }

        private static Image LoadAssetImage(string fileName)
        {
            try
            {
                // 1. Try disk paths
                string appDir = ConfigManager.GetAppDirectory();
                string[] searchDirs = new string[]
                {
                    Path.Combine(appDir, "assets", "icons"),
                    Path.Combine(appDir, "..", "assets", "icons"),
                    Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "assets", "icons")
                };

                foreach (string dir in searchDirs)
                {
                    if (Directory.Exists(dir))
                    {
                        string fullPath = Path.Combine(dir, fileName);
                        if (File.Exists(fullPath))
                        {
                            using (FileStream fs = new FileStream(fullPath, FileMode.Open, FileAccess.Read, FileShare.Read))
                            {
                                return Image.FromStream(fs);
                            }
                        }
                    }
                }

                // 2. Try embedded assembly resource
                Assembly asm = Assembly.GetExecutingAssembly();
                string resourceName = "TechInstaller.Icons." + fileName;
                using (Stream resStream = asm.GetManifestResourceStream(resourceName))
                {
                    if (resStream != null)
                    {
                        return Image.FromStream(resStream);
                    }
                }
            }
            catch { }

            return null;
        }

        private static Image TryExtractFromCacheExe(string id, string name)
        {
            try
            {
                string cacheDir = ConfigManager.GetCacheDirectory();
                if (Directory.Exists(cacheDir))
                {
                    string clean = ((id ?? "") + " " + (name ?? "")).ToLowerInvariant();
                    string[] exeFiles = Directory.GetFiles(cacheDir, "*.exe");
                    foreach (string exe in exeFiles)
                    {
                        string exeName = Path.GetFileNameWithoutExtension(exe).ToLowerInvariant();
                        if (clean.Contains(exeName) || exeName.Contains(clean))
                        {
                            Icon ico = Icon.ExtractAssociatedIcon(exe);
                            if (ico != null)
                            {
                                Bitmap bmp = ico.ToBitmap();
                                ico.Dispose();
                                return bmp;
                            }
                        }
                    }
                }
            }
            catch { }

            return null;
        }

        private static Bitmap CreatePresentedContainer(Image rawLogo, int size)
        {
            Bitmap bmp = new Bitmap(size, size);
            using (Graphics g = Graphics.FromImage(bmp))
            {
                g.SmoothingMode = SmoothingMode.AntiAlias;
                g.PixelOffsetMode = PixelOffsetMode.HighQuality;
                g.InterpolationMode = InterpolationMode.HighQualityBicubic;
                g.CompositingQuality = CompositingQuality.HighQuality;
                g.Clear(Color.Transparent);

                int radius = (size >= 48) ? 8 : 6;
                Rectangle rect = new Rectangle(1, 1, size - 3, size - 3);

                using (GraphicsPath path = GetRoundedRect(rect, radius))
                {
                    // Dark navy rounded container
                    using (SolidBrush brush = new SolidBrush(Color.FromArgb(16, 26, 50)))
                    {
                        g.FillPath(brush, path);
                    }

                    // Very subtle container border
                    using (Pen pen = new Pen(Color.FromArgb(34, 50, 86), 1f))
                    {
                        g.DrawPath(pen, path);
                    }
                }

                // Draw raw logo centered maintaining exact aspect ratio
                if (rawLogo != null)
                {
                    int padding = (size >= 48) ? 7 : 5;
                    int targetW = rect.Width - (padding * 2);
                    int targetH = rect.Height - (padding * 2);

                    float scale = Math.Min((float)targetW / rawLogo.Width, (float)targetH / rawLogo.Height);
                    int drawW = Math.Max(1, (int)(rawLogo.Width * scale));
                    int drawH = Math.Max(1, (int)(rawLogo.Height * scale));

                    int drawX = rect.X + (rect.Width - drawW) / 2;
                    int drawY = rect.Y + (rect.Height - drawH) / 2;

                    g.DrawImage(rawLogo, drawX, drawY, drawW, drawH);
                }
            }

            return bmp;
        }

        public static GraphicsPath GetRoundedRect(Rectangle bounds, int radius)
        {
            int diameter = radius * 2;
            Size size = new Size(diameter, diameter);
            Rectangle arc = new Rectangle(bounds.Location, size);
            GraphicsPath path = new GraphicsPath();

            if (radius == 0)
            {
                path.AddRectangle(bounds);
                return path;
            }

            path.AddArc(arc, 180, 90);
            arc.X = bounds.Right - diameter;
            path.AddArc(arc, 270, 90);
            arc.Y = bounds.Bottom - diameter;
            path.AddArc(arc, 0, 90);
            arc.X = bounds.Left;
            path.AddArc(arc, 90, 90);

            path.CloseFigure();
            return path;
        }
    }
}
