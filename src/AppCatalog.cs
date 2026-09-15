using System;
using System.Collections.Generic;

namespace TechInstaller
{
    public static class AppCatalog
    {
        public static List<AppItem> GetDefaultApps()
        {
            List<AppItem> list = new List<AppItem>();

            // --- DRIVERS & HARDWARE ---
            list.Add(new AppItem
            {
                Id = "iobit_driver_booster_portable",
                Name = "IObit Driver Booster Pro (Portable)",
                Category = "Drivers & Hardware",
                Description = "Automated driver updater and installer. Extracts portable version to C:\\Tools\\DriverBooster with Desktop shortcut (Ghost Toolbox style).",
                DownloadUrl = "",
                SilentArgs = "",
                CacheFileName = "DriverBoosterPortable.zip",
                PresetTags = "driver,essential,recommended,all",
                EstimatedSizeMB = 36,
                SpecialAction = "portable_driver_booster"
            });

            // --- RUNTIMES ---
            list.Add(new AppItem
            {
                Id = "vcredist_aio",
                Name = "Visual C++ Redistributables AIO (2005-2022)",
                Category = "Runtimes",
                Description = "All Microsoft Visual C++ runtimes (2005-2022 x86 & x64). Required by almost all modern games and software.",
                DownloadUrl = "https://github.com/abbodi1406/vcredist/releases/latest/download/VisualCppRedist_AIO_x86_x64.exe",
                SilentArgs = "/aiA",
                CacheFileName = "VisualCppRedist_AIO_x86_x64.exe",
                PresetTags = "runtime,essential,gaming,office",
                EstimatedSizeMB = 28,
                SpecialAction = ""
            });

            list.Add(new AppItem
            {
                Id = "directx_redist",
                Name = "DirectX End-User Runtimes (June 2010)",
                Category = "Runtimes",
                Description = "Complete standalone offline DirectX 9.0c, 10, and 11 runtime libraries essential for PC gaming.",
                DownloadUrl = "https://download.microsoft.com/download/8/4/A/84A35BF1-DAFE-4AE8-82AF-AD2AE20B6B14/directx_Jun2010_redist.exe",
                SilentArgs = "",
                CacheFileName = "directx_Jun2010_redist.exe",
                PresetTags = "runtime,gaming",
                EstimatedSizeMB = 95,
                SpecialAction = "directx_offline"
            });

            list.Add(new AppItem
            {
                Id = "dotnet8_desktop",
                Name = ".NET Desktop Runtime 8.0 (x64)",
                Category = "Runtimes",
                Description = "Microsoft .NET 8 Desktop Runtime for running modern WPF and Windows Forms apps.",
                DownloadUrl = "https://builds.dotnet.microsoft.com/dotnet/WindowsDesktop/8.0.31/windowsdesktop-runtime-8.0.31-win-x64.exe",
                SilentArgs = "/install /quiet /norestart",
                CacheFileName = "windowsdesktop-runtime-8-x64.exe",
                PresetTags = "runtime",
                EstimatedSizeMB = 55,
                SpecialAction = ""
            });

            list.Add(new AppItem
            {
                Id = "enable_net35",
                Name = "Enable .NET Framework 3.5 (DISM)",
                Category = "Runtimes",
                Description = "Enables legacy .NET 2.0/3.0/3.5 support via Windows built-in DISM feature tool.",
                DownloadUrl = "",
                SilentArgs = "",
                CacheFileName = "",
                PresetTags = "runtime",
                EstimatedSizeMB = 0,
                SpecialAction = "enable_net35"
            });

            // --- BROWSERS ---
            list.Add(new AppItem
            {
                Id = "google_chrome",
                Name = "Google Chrome (64-bit Enterprise)",
                Category = "Browsers",
                Description = "Google Chrome fast & secure web browser, standalone silent MSI installer.",
                DownloadUrl = "https://dl.google.com/chrome/install/GoogleChromeStandaloneEnterprise64.msi",
                SilentArgs = "/qn /norestart",
                CacheFileName = "GoogleChromeStandaloneEnterprise64.msi",
                PresetTags = "essential,office,gaming",
                EstimatedSizeMB = 110,
                SpecialAction = ""
            });

            list.Add(new AppItem
            {
                Id = "brave_browser",
                Name = "Brave Browser (64-bit)",
                Category = "Browsers",
                Description = "Fast, privacy-oriented browser with built-in ad and tracker blocking.",
                DownloadUrl = "https://referrals.brave.com/latest/BraveBrowserSetup.exe",
                SilentArgs = "--silent --install",
                CacheFileName = "BraveBrowserSetup64.exe",
                PresetTags = "essential",
                EstimatedSizeMB = 120,
                SpecialAction = ""
            });

            list.Add(new AppItem
            {
                Id = "mozilla_firefox",
                Name = "Mozilla Firefox (64-bit)",
                Category = "Browsers",
                Description = "Official Mozilla Firefox standalone installer.",
                DownloadUrl = "https://download.mozilla.org/?product=firefox-latest-ssl&os=win64&lang=en-US",
                SilentArgs = "-ms",
                CacheFileName = "Firefox_Setup.exe",
                PresetTags = "browsers",
                EstimatedSizeMB = 60,
                SpecialAction = ""
            });

            // --- UTILITIES ---
            list.Add(new AppItem
            {
                Id = "winrar",
                Name = "WinRAR 64-bit",
                Category = "Utilities",
                Description = "Powerful archive manager supporting RAR, ZIP, and extraction of multiple formats.",
                DownloadUrl = "https://www.rarlab.com/rar/winrar-x64-701.exe",
                SilentArgs = "/s",
                CacheFileName = "winrar-x64.exe",
                PresetTags = "essential,office,gaming",
                EstimatedSizeMB = 4,
                SpecialAction = ""
            });

            list.Add(new AppItem
            {
                Id = "sevenzip",
                Name = "7-Zip 64-bit",
                Category = "Utilities",
                Description = "High-compression open-source file archiver with 7z, ZIP, and TAR support.",
                DownloadUrl = "https://www.7-zip.org/a/7z2408-x64.exe",
                SilentArgs = "/S",
                CacheFileName = "7z-x64.exe",
                PresetTags = "essential,gaming",
                EstimatedSizeMB = 2,
                SpecialAction = ""
            });

            list.Add(new AppItem
            {
                Id = "anydesk",
                Name = "AnyDesk Remote Desktop",
                Category = "Utilities",
                Description = "Fast remote support and screen sharing tool for remote technician assistance.",
                DownloadUrl = "https://download.anydesk.com/AnyDesk.exe",
                SilentArgs = "--install \"C:\\Program Files (x86)\\AnyDesk\" --start-with-win --silent",
                CacheFileName = "AnyDesk.exe",
                PresetTags = "essential,office",
                EstimatedSizeMB = 5,
                SpecialAction = ""
            });

            list.Add(new AppItem
            {
                Id = "revo_uninstaller",
                Name = "Revo Uninstaller Free",
                Category = "Utilities",
                Description = "Deep software uninstaller that cleans leftover registry keys and junk files.",
                DownloadUrl = "https://revouninstaller.b-cdn.net/ruf270/revosetup.exe",
                SilentArgs = "/VERYSILENT /NORESTART",
                CacheFileName = "revosetup.exe",
                PresetTags = "utilities",
                EstimatedSizeMB = 18,
                SpecialAction = ""
            });

            // --- PRODUCTIVITY & OFFICE ---
            list.Add(new AppItem
            {
                Id = "office_deploy",
                Name = "Microsoft Office (Word, Excel, PowerPoint)",
                Category = "Productivity",
                Description = "Automated Office Deployment (Word, Excel, PowerPoint) using official Microsoft ODT.",
                DownloadUrl = "https://download.microsoft.com/download/2/7/A/27AF1BE6-DD20-4CB4-B154-EBAB8A7D4A7E/officedeploymenttool_17830-20162.exe",
                SilentArgs = "",
                CacheFileName = "officedeploymenttool.exe",
                PresetTags = "office",
                EstimatedSizeMB = 5,
                SpecialAction = "office_odt"
            });

            list.Add(new AppItem
            {
                Id = "sumatra_pdf",
                Name = "Sumatra PDF Viewer (64-bit)",
                Category = "Productivity",
                Description = "Ultra-fast, lightweight PDF, ePub, and document reader without heavy bloat.",
                DownloadUrl = "https://www.sumatrapdfreader.org/dl/rel/3.6.1/SumatraPDF-3.6.1-64-install.exe",
                SilentArgs = "-s",
                CacheFileName = "SumatraPDF-64-install.exe",
                PresetTags = "essential,office",
                EstimatedSizeMB = 9,
                SpecialAction = ""
            });

            // --- MEDIA ---
            list.Add(new AppItem
            {
                Id = "vlc_player",
                Name = "VLC Media Player (64-bit)",
                Category = "Media",
                Description = "Plays almost all audio and video file formats without needing external codecs.",
                DownloadUrl = "https://download.videolan.org/pub/videolan/vlc/3.0.21/win64/vlc-3.0.21-win64.exe",
                SilentArgs = "/S",
                CacheFileName = "vlc-3.0.21-win64.exe",
                PresetTags = "essential,gaming,office",
                EstimatedSizeMB = 42,
                SpecialAction = ""
            });

            // --- GAMING ---
            list.Add(new AppItem
            {
                Id = "steam",
                Name = "Steam Client",
                Category = "Gaming",
                Description = "Valve Steam gaming platform and digital store.",
                DownloadUrl = "https://cdn.akamai.steamstatic.com/client/installer/SteamSetup.exe",
                SilentArgs = "/S",
                CacheFileName = "SteamSetup.exe",
                PresetTags = "gaming",
                EstimatedSizeMB = 3,
                SpecialAction = ""
            });

            list.Add(new AppItem
            {
                Id = "discord",
                Name = "Discord",
                Category = "Gaming",
                Description = "Voice, video, and chat application for gaming and communities.",
                DownloadUrl = "https://discord.com/api/download?platform=win",
                SilentArgs = "-s",
                CacheFileName = "DiscordSetup.exe",
                PresetTags = "gaming",
                EstimatedSizeMB = 90,
                SpecialAction = ""
            });

            // --- SYSTEM & ACTIVATION ---
            list.Add(new AppItem
            {
                Id = "win_activation_status",
                Name = "Windows Activation & License Status",
                Category = "System Tools",
                Description = "Opens Windows Activation settings or checks genuine license expiration status safely.",
                DownloadUrl = "",
                SilentArgs = "",
                CacheFileName = "",
                PresetTags = "essential,gaming,office",
                EstimatedSizeMB = 0,
                SpecialAction = "windows_activation"
            });

            return list;
        }
    }
}
