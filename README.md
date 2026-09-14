# ⚡ TechInstaller - Windows Post-Install & Tech Toolbox

Isang magaan, mabilis, at **zero-dependency** na standalone Windows application para sa mga PC technicians, system builders, at IT professionals. Dinisenyo ito para sa mabilisang setup sa mga bagong format na PC (genuine Windows man o custom OS tulad ng Ghost Spectre, ReviOS, AtlasOS, at Tiny11) gamit lamang ang USB flash drive.

---

## 🌟 Mga Tampok (Key Features)

### 1. 🛡️ Automatic Administrator Elevation
- Hindi mo na kailangang mag-right-click at pumili ng "Run as administrator".
- Pagka-double click mo sa `TechInstaller.exe`, **automatic nitong iche-check kung Admin privileges ito**. Kung hindi, kusang magpo-prompt ang Windows UAC at magre-relaunch ito bilang Administrator.

### 2. 🎨 Professional Multi-Resolution Icon
- May kasamang custom Windows `.ico` file (16x16, 32x32, 48x48, at 256x256 resolutions) na naka-embed sa mismong executable file. Malinaw at propesyonal tingnan sa File Explorer, Desktop, at Windows Taskbar.

### 3. 📦 Standard Software Installer (USB / Offline Mode)
- **100% Dark-Themed DataGridView**: Maaliwalas basahin na may alternating colors at status indicators.
- **Pre-Downloaded 15 Latest Offline Installers** sa `cache\` folder (Visual C++ AIO 2005-2022, DirectX June 2010 offline redistributable, Chrome Enterprise MSI, WinRAR, 7-Zip, AnyDesk, Sumatra PDF 3.6.1, VLC Media Player, .NET Desktop Runtime 8, Steam, Discord, Revo Uninstaller, at Office ODT).
- **1-Click Presets**:
  - ⭐ **Essentials**: Chrome, WinRAR, Visual C++ AIO, DirectX, VLC, Sumatra PDF.
  - ⚡ **All Runtimes**: Visual C++ AIO, DirectX June 2010, .NET 8 Desktop, .NET 3.5.
  - 🎮 **Gaming PC**: Runtimes, Chrome, 7-Zip, Discord, Steam, Activation check.
  - 💼 **Office Setup**: Runtimes, Chrome, Microsoft Office, WinRAR, Sumatra PDF, AnyDesk.

### 4. ☁️ Google Drive Pre-installed Apps Section
- Isang hiwalay na tab para sa mga sarili mong application packages na naka-upload sa iyong personal Google Drive.
- **Walang kailangang recompile**: Lahat ng links ay nasa `cloud_apps.json`.
- **1-Click Open**: Pag-click mo sa app, may button na **`🌐 Open in Browser`** para direktang buksan ang Google Drive download link.
- **1-Click Edit**: Pindutin lang ang **`📝 Edit Links (Notepad)`** para i-paste ang sarili mong Google Drive links.

### 5. 🛠️ Clickable System Tools & Shortcuts
Isang pindot lang para sa mga karaniwang ginagawa ng technician pagkatapos mag-format:
- 🖥️ **Device Manager**: Binubuksan ang `devmgmt.msc` para silipin ang kulang na drivers.
- 🕒 **Date & Time Settings**: Binubuksan ang Windows Date & Time page.
- 🔄 **Sync Internet Time Now**: Kusang nagpapatakbo ng `w32tm /resync /force` para itama agad ang oras ng PC via internet time server.
- 🌐 **Time Zone Configuration**: Binubuksan ang dialog para palitan ang timezone (hal. UTC+08:00).
- 🖼️ **Desktop Background & Themes**: Mabilisang pagpalit ng wallpaper at personalization.
- 🧹 **Flush DNS Cache**: Mabilisang nagpa-flush ng DNS resolver cache via `ipconfig /flushdns`.
- ⚡ **DNS Presets**:
  - `Set Cloudflare DNS (1.1.1.1 & 1.0.0.1)`
  - `Set Google DNS (8.8.8.8 & 8.8.4.4)`
  - `Reset DNS to Automatic (DHCP)`
- 🔌 **Network Connections (`ncpa.cpl`)**: Binubuksan ang Network Adapters list.
- 🔑 **Windows Activation**: Binubuksan ang official Windows Activation settings para sa genuine license management.

---

## 📁 Paano Ilagay sa USB Flash Drive

Kopyahin ang buong laman ng folder na `output/` sa iyong USB drive:

```text
USB Flash Drive (hal. E:\)
├── TechInstaller.exe     <-- Standalone program na may custom icon (Auto Run as Admin)
├── apps.json             <-- Listahan ng standard installers at silent flags
├── cloud_apps.json       <-- Listahan ng mga Google Drive apps mo
└── cache\                <-- Offline installers folder (515 MB total)
```

---

## ⚙️ Paano Magdagdag ng Bagong App sa Google Drive List

May dalawang napakadaling paraan para magdagdag:

### Paraan 1: Gamit ang Bagong "➕ Add App" Button (Direct sa UI)
1. Sa loob ng app, pumunta sa tab na **`☁️ Google Drive Apps`**.
2. I-click ang kulay berdeng button na **`➕ Add App`**.
3. May lalabas na form kung saan ilalagay mo ang:
   - **Application Name** (hal. *Filmora 13*, *CorelDraw 2024*, *AutoCAD*)
   - **Category** (Productivity, Design, Video Editing, Engineering, atbp.)
   - **Package Size** (hal. *1.5 GB*)
   - **Google Drive Sharing Link** (I-paste ang Google Drive sharing URL mo)
   - **Description / Notes**
4. Pindutin ang **`💾 Save to List`** — automatic itong mapapasama sa listahan at mase-save sa `cloud_apps.json`!

### Paraan 2: Gamit ang Notepad ("📝 Edit Links")
1. I-click ang **`📝 Edit Links (Notepad)`** sa toolbar.
2. Bubukas ang `cloud_apps.json` sa Notepad. Kopyahin lang ang template na ito at i-paste sa dulo bago ang `]`:

```json
  {
    "Id": "custom_app_id",
    "Name": "Pangalan ng Software",
    "Category": "Design",
    "Description": "Maikling paliwanag sa installer.",
    "DriveUrl": "https://drive.google.com/file/d/xxxx/view?usp=sharing",
    "EstimatedSize": "1.2 GB",
    "Version": "Latest",
    "Notes": "Google Drive pre-installed package."
  }
```
3. Pindutin ang **`Ctrl + S`** para i-save sa Notepad.
4. Bumalik sa TechInstaller at i-click ang **`🔄 Refresh List`**!
