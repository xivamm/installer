using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Text;

namespace TechInstaller
{
    public enum UiIcon
    {
        // Navigation
        NavSoftware,
        NavCloud,
        NavTools,

        // Top Toolbar Actions
        OpenFolder,
        DownloadCloud,
        SyncGit,
        Reload,

        // Presets & Filters
        Star,
        Lightning,
        Gamepad,
        Briefcase,
        CheckAll,
        Clear,
        Search,

        // Status & Category
        CheckCircle,
        DownCircle,
        ClockCircle,
        FailedCircle,
        Shield,
        Speed,
        Clean,
        Settings,
        Network,
        CommandLine,
        Document,
        Theme,
        Trash,

        // Action & Controls
        Play,
        Cancel,
        ExternalLink,
        Package,
        Add,
        Edit,
        Wrench,
        HardDrive,
        Chip,
        Key,
        CloudUpload,
        Power,
        HeartPulse,
        AlertCircle,
        Battery,
        WifiOff,
        Wifi,
        RefreshCw,
        Copy,
        Export,
        Computer
    }

    public static class UiIconHelper
    {
        private static readonly string IconFontFamily = DetectIconFont();
        private static readonly Dictionary<string, Image> _iconCache = new Dictionary<string, Image>();

        private static string DetectIconFont()
        {
            try
            {
                foreach (FontFamily family in FontFamily.Families)
                {
                    if (string.Equals(family.Name, "Segoe MDL2 Assets", StringComparison.OrdinalIgnoreCase))
                    {
                        return "Segoe MDL2 Assets";
                    }
                }
                foreach (FontFamily family in FontFamily.Families)
                {
                    if (string.Equals(family.Name, "Segoe Fluent Icons", StringComparison.OrdinalIgnoreCase))
                    {
                        return "Segoe Fluent Icons";
                    }
                }
            }
            catch { }
            return "Segoe UI Symbol";
        }

        public static string GetGlyph(UiIcon icon)
        {
            switch (icon)
            {
                case UiIcon.NavSoftware: return "\uE74C";       // ChromeBack / App grid
                case UiIcon.NavCloud: return "\uE753";          // Cloud
                case UiIcon.NavTools: return "\uE90F";          // Repair / Tools
                case UiIcon.OpenFolder: return "\uED25";        // Folder
                case UiIcon.DownloadCloud: return "\uE896";     // Download
                case UiIcon.SyncGit: return "\uE895";           // Sync
                case UiIcon.Reload: return "\uE72C";            // Refresh
                case UiIcon.Star: return "\uE734";              // FavoriteStar
                case UiIcon.Lightning: return "\uE945";         // Action / Energy
                case UiIcon.Gamepad: return "\uE7FC";           // Game
                case UiIcon.Briefcase: return "\uEC32";         // Work
                case UiIcon.CheckAll: return "\uE73E";          // CheckMark
                case UiIcon.Clear: return "\uE711";             // Cancel
                case UiIcon.Search: return "\uE721";            // Search
                case UiIcon.CheckCircle: return "\uE73E";       // CheckMark
                case UiIcon.DownCircle: return "\uE896";        // Download
                case UiIcon.ClockCircle: return "\uE823";       // Clock
                case UiIcon.FailedCircle: return "\uE711";      // Error / Cancel
                case UiIcon.Shield: return "\uEA18";            // Shield
                case UiIcon.Speed: return "\uEBE8";             // Performance
                case UiIcon.Clean: return "\uE74D";             // Delete / Clean
                case UiIcon.Settings: return "\uE713";          // Settings
                case UiIcon.Network: return "\uE774";           // Globe / Network
                case UiIcon.CommandLine: return "\uE756";       // Command prompt
                case UiIcon.Document: return "\uE8A5";          // Document
                case UiIcon.Theme: return "\uEB9F";             // Theme / Media
                case UiIcon.Trash: return "\uE74D";             // Delete
                case UiIcon.Play: return "\uE768";              // Play
                case UiIcon.Cancel: return "\uE711";            // Cancel
                case UiIcon.ExternalLink: return "\uE8A7";      // OpenInNewWindow
                case UiIcon.Package: return "\uE71D";           // Package
                case UiIcon.Add: return "\uE710";               // Add
                case UiIcon.Edit: return "\uE70F";              // Edit
                case UiIcon.Wrench: return "\uE90F";            // Repair / Wrench
                case UiIcon.HardDrive: return "\uEDA2";         // Storage / Drive
                case UiIcon.Chip: return "\uE950";              // Processor / Chip
                case UiIcon.Key: return "\uE8D7";               // Key / Permissions
                case UiIcon.CloudUpload: return "\uE898";       // Cloud Upload
                case UiIcon.Power: return "\uE7E8";             // Power
                case UiIcon.HeartPulse: return "\uEC02";        // Health / Heart
                case UiIcon.AlertCircle: return "\uE7BA";       // Warning / Error
                case UiIcon.Battery: return "\uE83F";           // Battery
                case UiIcon.WifiOff: return "\uEB5E";           // Network Offline
                case UiIcon.Wifi: return "\uE701";              // Network Online
                case UiIcon.RefreshCw: return "\uE72C";         // Refresh
                case UiIcon.Copy: return "\uE8C8";              // Copy
                case UiIcon.Export: return "\uEDE1";            // Save / Export
                case UiIcon.Computer: return "\uE7F8";          // Device / PC
                default: return "\uE71D";
            }
        }

        public static Image GetIcon(UiIcon icon, int size, Color color)
        {
            string key = string.Format("{0}_{1}_{2}", icon, size, color.ToArgb());
            if (_iconCache.ContainsKey(key))
            {
                return _iconCache[key];
            }

            Bitmap bmp = new Bitmap(size, size);
            using (Graphics g = Graphics.FromImage(bmp))
            {
                g.SmoothingMode = SmoothingMode.AntiAlias;
                g.TextRenderingHint = TextRenderingHint.AntiAliasGridFit;
                g.PixelOffsetMode = PixelOffsetMode.HighQuality;
                g.Clear(Color.Transparent);

                float fontSize = (float)(size * 0.72);
                using (Font font = new Font(IconFontFamily, fontSize, FontStyle.Regular, GraphicsUnit.Pixel))
                using (Brush brush = new SolidBrush(color))
                using (StringFormat sf = new StringFormat())
                {
                    sf.Alignment = StringAlignment.Center;
                    sf.LineAlignment = StringAlignment.Center;
                    g.DrawString(GetGlyph(icon), font, brush, new RectangleF(0, 0, size, size), sf);
                }
            }

            _iconCache[key] = bmp;
            return bmp;
        }

        public static Font CreateIconFont(float sizeInPoints)
        {
            return new Font(IconFontFamily, sizeInPoints, FontStyle.Regular);
        }
    }
}
