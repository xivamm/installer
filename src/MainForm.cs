using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Net.NetworkInformation;
using System.Windows.Forms;

namespace TechInstaller
{
    public class MainForm : Form
    {
        private List<AppItem> _allApps;
        private List<CloudAppItem> _cloudApps;
        private InstallerEngine _engine;

        // Navigation Mode Tabs
        private Panel _navPanel;
        private Button _btnNavSoftware;
        private Button _btnNavCloud;
        private Button _btnNavTools;

        // Content Containers
        private Panel _panelSoftware;
        private Panel _panelCloud;
        private Panel _panelTools;

        // --- Software Panel Controls ---
        private Panel _topPanel;
        private Label _lblTitle;
        private Label _lblSubtitle;
        private FlowLayoutPanel _topButtonsPanel;
        private Button _btnOpenCache;
        private Button _btnDownloadAll;
        private Button _btnReload;

        private Panel _presetPanel;
        private FlowLayoutPanel _presetButtonsPanel;
        private Button _btnPresetEssentials;
        private Button _btnPresetRuntimes;
        private Button _btnPresetGaming;
        private Button _btnPresetOffice;
        private Button _btnSelectAll;
        private Button _btnDeselectAll;
        private Panel _searchPanel;
        private Label _lblSearch;
        private TextBox _txtSearch;

        private SplitContainer _splitContainer;
        private DataGridView _gridApps;

        private Panel _rightPanel;
        private Panel _detailsPanel;
        private Label _lblDetailsTitle;
        private Label _lblDetailsCategory;
        private Label _lblDetailsSize;
        private Label _lblDetailsCache;
        private Label _lblDetailsArgs;
        private TextBox _txtDetailsDesc;

        private Panel _logHeaderPanel;
        private Label _lblLogTitle;
        private Button _btnClearLog;
        private RichTextBox _rtbLog;

        private Panel _bottomPanel;
        private TableLayoutPanel _bottomLayout;
        private Label _lblOverallStatus;
        private ProgressBar _pbOverall;
        private Label _lblCurrentStatus;
        private ProgressBar _pbCurrent;
        private Button _btnAction;

        // --- Cloud Apps Controls ---
        private Panel _cloudTopBar;
        private Button _btnCloudOpen;
        private Button _btnCloudEditConfig;
        private Button _btnCloudReload;
        private TextBox _txtCloudSearch;
        private SplitContainer _cloudSplitContainer;
        private DataGridView _gridCloudApps;
        private Panel _cloudDetailsPanel;
        private Label _lblCloudTitle;
        private Label _lblCloudCategory;
        private Label _lblCloudSize;
        private Label _lblCloudVersion;
        private TextBox _txtCloudDesc;
        private TextBox _txtCloudUrl;
        private Button _btnCloudLaunchDirect;

        // --- System Tools Controls ---
        private FlowLayoutPanel _toolsFlowPanel;
        private RichTextBox _rtbToolsLog;

        // Modern Slate Theme Colors
        private readonly Color ColBg = Color.FromArgb(15, 23, 42);          // Slate 900
        private readonly Color ColCard = Color.FromArgb(30, 41, 59);        // Slate 800
        private readonly Color ColCardAlt = Color.FromArgb(24, 34, 52);     // Slate 850
        private readonly Color ColHover = Color.FromArgb(51, 65, 85);        // Slate 700
        private readonly Color ColBorder = Color.FromArgb(51, 65, 85);       // Slate 700
        private readonly Color ColAccentBlue = Color.FromArgb(56, 189, 248); // Sky 400
        private readonly Color ColAccentGreen = Color.FromArgb(16, 185, 129); // Emerald 500
        private readonly Color ColAccentAmber = Color.FromArgb(245, 158, 11); // Amber 500
        private readonly Color ColAccentRed = Color.FromArgb(239, 68, 68);   // Red 500
        private readonly Color ColTextPrimary = Color.FromArgb(248, 250, 252);
        private readonly Color ColTextSecondary = Color.FromArgb(148, 163, 184);

        public MainForm()
        {
            InitializeIcon();
            InitializeComponent();
            InitializeEngine();

            LoadAppCatalog();
            LoadCloudAppCatalog();
            ShowTab("software");
        }

        private void InitializeIcon()
        {
            try
            {
                string iconPath = Path.Combine(ConfigManager.GetAppDirectory(), "app.ico");
                if (File.Exists(iconPath))
                {
                    this.Icon = new Icon(iconPath);
                }
                else
                {
                    this.Icon = Icon.ExtractAssociatedIcon(Application.ExecutablePath);
                }
            }
            catch { }
        }

        private void InitializeEngine()
        {
            _engine = new InstallerEngine();
            _engine.OnLog += OnEngineLog;
            _engine.OnOverallProgress += OnEngineOverallProgress;
            _engine.OnCurrentProgress += OnEngineCurrentProgress;
            _engine.OnAppStatusChanged += OnEngineAppStatusChanged;
        }

        private void InitializeComponent()
        {
            this.Text = "TechInstaller - Windows Post-Install & Tech Toolbox";
            this.Size = new Size(1160, 760);
            this.MinimumSize = new Size(960, 620);
            this.StartPosition = FormStartPosition.CenterScreen;
            this.BackColor = ColBg;
            this.ForeColor = ColTextPrimary;
            this.Font = new Font("Segoe UI", 9.5F, FontStyle.Regular, GraphicsUnit.Point);

            // ==================== TOP HEADER ====================
            _topPanel = new Panel();
            _topPanel.Dock = DockStyle.Top;
            _topPanel.Height = 70;
            _topPanel.BackColor = ColCard;
            _topPanel.Padding = new Padding(16, 8, 16, 8);

            _lblTitle = new Label();
            _lblTitle.Text = "⚡ TECH INSTALLER - Post-Install & Tech Toolbox";
            _lblTitle.Font = new Font("Segoe UI", 13.5F, FontStyle.Bold);
            _lblTitle.ForeColor = ColAccentBlue;
            _lblTitle.AutoSize = true;
            _lblTitle.Location = new Point(16, 10);

            _lblSubtitle = new Label();
            string osName = Environment.OSVersion.ToString();
            string arch = Environment.Is64BitOperatingSystem ? "64-bit" : "32-bit";
            string rootDrive = Path.GetPathRoot(ConfigManager.GetAppDirectory());
            bool isOnline = NetworkInterface.GetIsNetworkAvailable();
            _lblSubtitle.Text = string.Format("💻 OS: {0} ({1})  •  📁 Drive: {2}  •  {3} {4}  •  🛡️ Admin Mode Active",
                osName, arch, rootDrive, isOnline ? "🟢" : "🔴", isOnline ? "Online" : "Offline");
            _lblSubtitle.Font = new Font("Segoe UI", 9.0F);
            _lblSubtitle.ForeColor = ColTextSecondary;
            _lblSubtitle.AutoSize = true;
            _lblSubtitle.Location = new Point(18, 40);

            _topButtonsPanel = new FlowLayoutPanel();
            _topButtonsPanel.Dock = DockStyle.Right;
            _topButtonsPanel.FlowDirection = FlowDirection.RightToLeft;
            _topButtonsPanel.Width = 600;
            _topButtonsPanel.Height = 52;
            _topButtonsPanel.BackColor = Color.Transparent;
            _topButtonsPanel.Padding = new Padding(0, 8, 0, 0);

            _btnReload = CreateStyledButton("🔄 Reload", ColHover, 90);
            _btnReload.Click += delegate { LoadAppCatalog(); LoadCloudAppCatalog(); };

            Button btnSyncGitHub = CreateStyledButton("☁️ Sync from GitHub", Color.FromArgb(14, 165, 233), 160);
            btnSyncGitHub.Click += delegate {
                string msg;
                bool ok = ConfigManager.FetchFromGitHub(out msg);
                LoadCloudAppCatalog();
                LoadAppCatalog();
                MessageBox.Show(msg, ok ? "GitHub Sync Complete" : "GitHub Sync Notice", MessageBoxButtons.OK, ok ? MessageBoxIcon.Information : MessageBoxIcon.Warning);
            };

            _btnDownloadAll = CreateStyledButton("⬇️ Cache All to USB", Color.FromArgb(37, 99, 235), 145);
            _btnDownloadAll.Click += delegate { DownloadAllMissingToCache(); };

            _btnOpenCache = CreateStyledButton("📁 Open USB Cache", ColHover, 135);
            _btnOpenCache.Click += delegate { OpenCacheFolder(); };

            _topButtonsPanel.Controls.Add(_btnReload);
            _topButtonsPanel.Controls.Add(btnSyncGitHub);
            _topButtonsPanel.Controls.Add(_btnDownloadAll);
            _topButtonsPanel.Controls.Add(_btnOpenCache);

            _topPanel.Controls.Add(_topButtonsPanel);
            _topPanel.Controls.Add(_lblTitle);
            _topPanel.Controls.Add(_lblSubtitle);

            // ==================== NAVIGATION TAB BAR ====================
            _navPanel = new Panel();
            _navPanel.Dock = DockStyle.Top;
            _navPanel.Height = 44;
            _navPanel.BackColor = Color.FromArgb(18, 26, 44);
            _navPanel.Padding = new Padding(12, 4, 12, 4);

            _btnNavSoftware = CreateNavButton("📦 Standard Software (USB / Offline)", true);
            _btnNavSoftware.Click += delegate { ShowTab("software"); };

            _btnNavCloud = CreateNavButton("☁️ Google Drive Apps", false);
            _btnNavCloud.Click += delegate { ShowTab("cloud"); };

            _btnNavTools = CreateNavButton("🛠️ System Tools & Tweaks", false);
            _btnNavTools.Click += delegate { ShowTab("tools"); };

            _navPanel.Controls.Add(_btnNavTools);
            _navPanel.Controls.Add(_btnNavCloud);
            _navPanel.Controls.Add(_btnNavSoftware);

            // Build Panels
            InitializeSoftwarePanel();
            InitializeCloudPanel();
            InitializeToolsPanel();

            this.Controls.Add(_panelSoftware);
            this.Controls.Add(_panelCloud);
            this.Controls.Add(_panelTools);
            this.Controls.Add(_navPanel);
            this.Controls.Add(_topPanel);
        }

        private Button CreateNavButton(string text, bool isActive)
        {
            Button btn = new Button();
            btn.Text = text;
            btn.Height = 34;
            btn.Width = 240;
            btn.Dock = DockStyle.Left;
            btn.FlatStyle = FlatStyle.Flat;
            btn.FlatAppearance.BorderSize = 0;
            btn.Font = new Font("Segoe UI", 9.5F, FontStyle.Bold);
            btn.Cursor = Cursors.Hand;
            btn.BackColor = isActive ? ColAccentBlue : Color.Transparent;
            btn.ForeColor = isActive ? Color.FromArgb(15, 23, 42) : ColTextSecondary;
            return btn;
        }

        private void ShowTab(string tab)
        {
            _panelSoftware.Visible = (tab == "software");
            _panelCloud.Visible = (tab == "cloud");
            _panelTools.Visible = (tab == "tools");

            SetNavButtonActive(_btnNavSoftware, tab == "software");
            SetNavButtonActive(_btnNavCloud, tab == "cloud");
            SetNavButtonActive(_btnNavTools, tab == "tools");
        }

        private void SetNavButtonActive(Button btn, bool active)
        {
            btn.BackColor = active ? ColAccentBlue : Color.Transparent;
            btn.ForeColor = active ? Color.FromArgb(15, 23, 42) : ColTextSecondary;
        }

        // =========================================================================
        // 1. SOFTWARE INSTALLER PANEL
        // =========================================================================
        private void InitializeSoftwarePanel()
        {
            _panelSoftware = new Panel();
            _panelSoftware.Dock = DockStyle.Fill;
            _panelSoftware.BackColor = ColBg;

            // Preset Toolbar
            _presetPanel = new Panel();
            _presetPanel.Dock = DockStyle.Top;
            _presetPanel.Height = 46;
            _presetPanel.BackColor = ColCardAlt;
            _presetPanel.Padding = new Padding(12, 6, 12, 6);

            _presetButtonsPanel = new FlowLayoutPanel();
            _presetButtonsPanel.Dock = DockStyle.Fill;
            _presetButtonsPanel.FlowDirection = FlowDirection.LeftToRight;
            _presetButtonsPanel.BackColor = Color.Transparent;

            _btnPresetEssentials = CreateStyledButton("⭐ Essentials", ColHover, 115);
            _btnPresetEssentials.Click += delegate { ApplyPreset("essential"); };

            _btnPresetRuntimes = CreateStyledButton("⚡ All Runtimes", ColHover, 115);
            _btnPresetRuntimes.Click += delegate { ApplyPreset("runtime"); };

            _btnPresetGaming = CreateStyledButton("🎮 Gaming PC", ColHover, 115);
            _btnPresetGaming.Click += delegate { ApplyPreset("gaming"); };

            _btnPresetOffice = CreateStyledButton("💼 Office Setup", ColHover, 115);
            _btnPresetOffice.Click += delegate { ApplyPreset("office"); };

            _btnSelectAll = CreateStyledButton("✓ Select All", Color.FromArgb(30, 58, 138), 95);
            _btnSelectAll.Click += delegate { SetAllSelection(true); };

            _btnDeselectAll = CreateStyledButton("✕ Clear", Color.FromArgb(71, 85, 105), 75);
            _btnDeselectAll.Click += delegate { SetAllSelection(false); };

            _presetButtonsPanel.Controls.Add(_btnPresetEssentials);
            _presetButtonsPanel.Controls.Add(_btnPresetRuntimes);
            _presetButtonsPanel.Controls.Add(_btnPresetGaming);
            _presetButtonsPanel.Controls.Add(_btnPresetOffice);
            _presetButtonsPanel.Controls.Add(_btnSelectAll);
            _presetButtonsPanel.Controls.Add(_btnDeselectAll);

            _searchPanel = new Panel();
            _searchPanel.Dock = DockStyle.Right;
            _searchPanel.Width = 240;
            _searchPanel.BackColor = Color.Transparent;

            _lblSearch = new Label();
            _lblSearch.Text = "🔍";
            _lblSearch.AutoSize = true;
            _lblSearch.Location = new Point(6, 10);
            _lblSearch.ForeColor = ColTextSecondary;

            _txtSearch = new TextBox();
            _txtSearch.Width = 195;
            _txtSearch.Location = new Point(32, 6);
            _txtSearch.BackColor = ColBg;
            _txtSearch.ForeColor = ColTextPrimary;
            _txtSearch.BorderStyle = BorderStyle.FixedSingle;
            _txtSearch.Font = new Font("Segoe UI", 9.5F);
            _txtSearch.TextChanged += delegate { FilterApps(_txtSearch.Text); };

            _searchPanel.Controls.Add(_lblSearch);
            _searchPanel.Controls.Add(_txtSearch);

            _presetPanel.Controls.Add(_presetButtonsPanel);
            _presetPanel.Controls.Add(_searchPanel);

            // Bottom Action & Progress in Responsive TableLayoutPanel
            _bottomPanel = new Panel();
            _bottomPanel.Dock = DockStyle.Bottom;
            _bottomPanel.Height = 96;
            _bottomPanel.BackColor = ColCard;
            _bottomPanel.Padding = new Padding(16, 8, 16, 8);

            _bottomLayout = new TableLayoutPanel();
            _bottomLayout.Dock = DockStyle.Fill;
            _bottomLayout.ColumnCount = 2;
            _bottomLayout.RowCount = 1;
            _bottomLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 72F));
            _bottomLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 28F));

            Panel bottomProgressContainer = new Panel();
            bottomProgressContainer.Dock = DockStyle.Fill;
            bottomProgressContainer.BackColor = Color.Transparent;

            _lblOverallStatus = new Label();
            _lblOverallStatus.Text = "Ready. Select software items and click 'Start Installation'.";
            _lblOverallStatus.AutoSize = true;
            _lblOverallStatus.ForeColor = ColTextPrimary;
            _lblOverallStatus.Font = new Font("Segoe UI", 9.5F, FontStyle.Bold);
            _lblOverallStatus.Location = new Point(0, 4);

            _pbOverall = new ProgressBar();
            _pbOverall.Height = 15;
            _pbOverall.Location = new Point(0, 26);
            _pbOverall.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;

            _lblCurrentStatus = new Label();
            _lblCurrentStatus.Text = "Idle";
            _lblCurrentStatus.AutoSize = true;
            _lblCurrentStatus.ForeColor = ColTextSecondary;
            _lblCurrentStatus.Font = new Font("Segoe UI", 8.5F);
            _lblCurrentStatus.Location = new Point(0, 46);

            _pbCurrent = new ProgressBar();
            _pbCurrent.Height = 10;
            _pbCurrent.Location = new Point(0, 64);
            _pbCurrent.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;

            bottomProgressContainer.Controls.Add(_lblOverallStatus);
            bottomProgressContainer.Controls.Add(_pbOverall);
            bottomProgressContainer.Controls.Add(_lblCurrentStatus);
            bottomProgressContainer.Controls.Add(_pbCurrent);

            Panel bottomButtonContainer = new Panel();
            bottomButtonContainer.Dock = DockStyle.Fill;
            bottomButtonContainer.BackColor = Color.Transparent;
            bottomButtonContainer.Padding = new Padding(10, 6, 0, 6);

            _btnAction = new Button();
            _btnAction.Text = "🚀 START INSTALLATION";
            _btnAction.Dock = DockStyle.Fill;
            _btnAction.BackColor = ColAccentGreen;
            _btnAction.ForeColor = Color.White;
            _btnAction.FlatStyle = FlatStyle.Flat;
            _btnAction.FlatAppearance.BorderSize = 0;
            _btnAction.Font = new Font("Segoe UI", 11.5F, FontStyle.Bold);
            _btnAction.Cursor = Cursors.Hand;
            _btnAction.Click += OnActionClick;

            bottomButtonContainer.Controls.Add(_btnAction);

            _bottomLayout.Controls.Add(bottomProgressContainer, 0, 0);
            _bottomLayout.Controls.Add(bottomButtonContainer, 1, 0);
            _bottomPanel.Controls.Add(_bottomLayout);

            // Split Container
            _splitContainer = new SplitContainer();
            _splitContainer.Dock = DockStyle.Fill;
            _splitContainer.BackColor = ColBorder;
            _splitContainer.SplitterWidth = 4;
            _splitContainer.SplitterDistance = 640;

            // Grid Left
            _gridApps = new DataGridView();
            _gridApps.Dock = DockStyle.Fill;
            _gridApps.BackgroundColor = ColBg;
            _gridApps.BorderStyle = BorderStyle.None;
            _gridApps.CellBorderStyle = DataGridViewCellBorderStyle.SingleHorizontal;
            _gridApps.GridColor = Color.FromArgb(30, 41, 59);
            _gridApps.EnableHeadersVisualStyles = false;
            _gridApps.RowHeadersVisible = false;
            _gridApps.AllowUserToAddRows = false;
            _gridApps.AllowUserToDeleteRows = false;
            _gridApps.AllowUserToResizeRows = false;
            _gridApps.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            _gridApps.MultiSelect = false;
            _gridApps.RowTemplate.Height = 32;

            _gridApps.ColumnHeadersHeight = 36;
            _gridApps.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.DisableResizing;
            _gridApps.ColumnHeadersBorderStyle = DataGridViewHeaderBorderStyle.None;
            _gridApps.ColumnHeadersDefaultCellStyle.BackColor = ColCard;
            _gridApps.ColumnHeadersDefaultCellStyle.ForeColor = ColTextPrimary;
            _gridApps.ColumnHeadersDefaultCellStyle.Font = new Font("Segoe UI", 9.5F, FontStyle.Bold);
            _gridApps.ColumnHeadersDefaultCellStyle.Padding = new Padding(4);

            _gridApps.DefaultCellStyle.BackColor = ColBg;
            _gridApps.DefaultCellStyle.ForeColor = ColTextPrimary;
            _gridApps.DefaultCellStyle.SelectionBackColor = Color.FromArgb(30, 58, 138);
            _gridApps.DefaultCellStyle.SelectionForeColor = Color.White;
            _gridApps.DefaultCellStyle.Font = new Font("Segoe UI", 9.0F);

            _gridApps.AlternatingRowsDefaultCellStyle.BackColor = Color.FromArgb(20, 30, 48);
            _gridApps.AlternatingRowsDefaultCellStyle.ForeColor = ColTextPrimary;

            DataGridViewCheckBoxColumn colCheck = new DataGridViewCheckBoxColumn();
            colCheck.Width = 36;
            colCheck.HeaderText = "";
            colCheck.Name = "ColCheck";
            colCheck.Resizable = DataGridViewTriState.False;
            _gridApps.Columns.Add(colCheck);

            DataGridViewTextBoxColumn colName = new DataGridViewTextBoxColumn();
            colName.HeaderText = "Software Name";
            colName.Name = "ColName";
            colName.ReadOnly = true;
            colName.AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill;
            colName.DefaultCellStyle.Font = new Font("Segoe UI", 9.5F, FontStyle.Bold);
            _gridApps.Columns.Add(colName);

            DataGridViewTextBoxColumn colCat = new DataGridViewTextBoxColumn();
            colCat.HeaderText = "Category";
            colCat.Name = "ColCategory";
            colCat.Width = 105;
            colCat.ReadOnly = true;
            _gridApps.Columns.Add(colCat);

            DataGridViewTextBoxColumn colSize = new DataGridViewTextBoxColumn();
            colSize.HeaderText = "Size";
            colSize.Name = "ColSize";
            colSize.Width = 75;
            colSize.ReadOnly = true;
            _gridApps.Columns.Add(colSize);

            DataGridViewTextBoxColumn colSource = new DataGridViewTextBoxColumn();
            colSource.HeaderText = "Source";
            colSource.Name = "ColSource";
            colSource.Width = 125;
            colSource.ReadOnly = true;
            _gridApps.Columns.Add(colSource);

            DataGridViewTextBoxColumn colStatus = new DataGridViewTextBoxColumn();
            colStatus.HeaderText = "Status";
            colStatus.Name = "ColStatus";
            colStatus.Width = 100;
            colStatus.ReadOnly = true;
            _gridApps.Columns.Add(colStatus);

            _gridApps.CellClick += OnGridCellClick;
            _gridApps.CellContentClick += OnGridCellContentClick;
            _gridApps.CellValueChanged += OnGridCellValueChanged;
            _gridApps.CurrentCellDirtyStateChanged += OnGridCurrentCellDirtyStateChanged;
            _gridApps.SelectionChanged += OnGridSelectionChanged;

            _splitContainer.Panel1.Controls.Add(_gridApps);

            // Right Panel (Details & Terminal)
            _rightPanel = new Panel();
            _rightPanel.Dock = DockStyle.Fill;
            _rightPanel.BackColor = ColCard;

            _detailsPanel = new Panel();
            _detailsPanel.Dock = DockStyle.Top;
            _detailsPanel.Height = 175;
            _detailsPanel.BackColor = ColCard;
            _detailsPanel.Padding = new Padding(14);

            _lblDetailsTitle = new Label();
            _lblDetailsTitle.Text = "Software Information";
            _lblDetailsTitle.Font = new Font("Segoe UI", 11.5F, FontStyle.Bold);
            _lblDetailsTitle.ForeColor = ColAccentBlue;
            _lblDetailsTitle.AutoSize = true;
            _lblDetailsTitle.Location = new Point(14, 10);

            _lblDetailsCategory = new Label();
            _lblDetailsCategory.Text = "Category: -";
            _lblDetailsCategory.ForeColor = ColTextSecondary;
            _lblDetailsCategory.AutoSize = true;
            _lblDetailsCategory.Location = new Point(14, 34);

            _lblDetailsSize = new Label();
            _lblDetailsSize.Text = "Size: -";
            _lblDetailsSize.ForeColor = ColTextSecondary;
            _lblDetailsSize.AutoSize = true;
            _lblDetailsSize.Location = new Point(150, 34);

            _lblDetailsCache = new Label();
            _lblDetailsCache.Text = "Status: -";
            _lblDetailsCache.ForeColor = ColAccentGreen;
            _lblDetailsCache.AutoSize = true;
            _lblDetailsCache.Location = new Point(14, 54);

            _lblDetailsArgs = new Label();
            _lblDetailsArgs.Text = "Silent Switch: -";
            _lblDetailsArgs.ForeColor = Color.FromArgb(125, 211, 252);
            _lblDetailsArgs.Font = new Font("Consolas", 8.5F);
            _lblDetailsArgs.AutoSize = true;
            _lblDetailsArgs.Location = new Point(14, 74);

            _txtDetailsDesc = new TextBox();
            _txtDetailsDesc.Location = new Point(14, 96);
            _txtDetailsDesc.Width = 380;
            _txtDetailsDesc.Height = 65;
            _txtDetailsDesc.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            _txtDetailsDesc.Multiline = true;
            _txtDetailsDesc.ReadOnly = true;
            _txtDetailsDesc.BackColor = ColBg;
            _txtDetailsDesc.ForeColor = ColTextPrimary;
            _txtDetailsDesc.BorderStyle = BorderStyle.None;
            _txtDetailsDesc.Font = new Font("Segoe UI", 9.0F);

            _detailsPanel.Controls.Add(_lblDetailsTitle);
            _detailsPanel.Controls.Add(_lblDetailsCategory);
            _detailsPanel.Controls.Add(_lblDetailsSize);
            _detailsPanel.Controls.Add(_lblDetailsCache);
            _detailsPanel.Controls.Add(_lblDetailsArgs);
            _detailsPanel.Controls.Add(_txtDetailsDesc);

            _logHeaderPanel = new Panel();
            _logHeaderPanel.Dock = DockStyle.Top;
            _logHeaderPanel.Height = 30;
            _logHeaderPanel.BackColor = ColCardAlt;
            _logHeaderPanel.Padding = new Padding(10, 4, 10, 4);

            _lblLogTitle = new Label();
            _lblLogTitle.Text = "🖥️ LIVE INSTALLATION LOG";
            _lblLogTitle.Font = new Font("Segoe UI", 8.5F, FontStyle.Bold);
            _lblLogTitle.ForeColor = ColTextSecondary;
            _lblLogTitle.AutoSize = true;
            _lblLogTitle.Location = new Point(10, 7);

            _btnClearLog = CreateStyledButton("Clear", ColHover, 60);
            _btnClearLog.Height = 22;
            _btnClearLog.Font = new Font("Segoe UI", 8.0F);
            _btnClearLog.Dock = DockStyle.Right;
            _btnClearLog.Click += delegate { _rtbLog.Clear(); };

            _logHeaderPanel.Controls.Add(_lblLogTitle);
            _logHeaderPanel.Controls.Add(_btnClearLog);

            _rtbLog = new RichTextBox();
            _rtbLog.Dock = DockStyle.Fill;
            _rtbLog.BackColor = Color.FromArgb(10, 15, 29);
            _rtbLog.ForeColor = Color.FromArgb(226, 232, 240);
            _rtbLog.Font = new Font("Consolas", 9.0F, FontStyle.Regular, GraphicsUnit.Point);
            _rtbLog.BorderStyle = BorderStyle.None;
            _rtbLog.ReadOnly = true;

            _rightPanel.Controls.Add(_rtbLog);
            _rightPanel.Controls.Add(_logHeaderPanel);
            _rightPanel.Controls.Add(_detailsPanel);

            _splitContainer.Panel2.Controls.Add(_rightPanel);

            _panelSoftware.Controls.Add(_splitContainer);
            _panelSoftware.Controls.Add(_presetPanel);
            _panelSoftware.Controls.Add(_bottomPanel);
        }

        // =========================================================================
        // 2. GOOGLE DRIVE CLOUD APPS PANEL
        // =========================================================================
        private void InitializeCloudPanel()
        {
            _panelCloud = new Panel();
            _panelCloud.Dock = DockStyle.Fill;
            _panelCloud.BackColor = ColBg;

            // Top Bar
            _cloudTopBar = new Panel();
            _cloudTopBar.Dock = DockStyle.Top;
            _cloudTopBar.Height = 48;
            _cloudTopBar.BackColor = ColCardAlt;
            _cloudTopBar.Padding = new Padding(12, 6, 12, 6);

            _btnCloudOpen = CreateStyledButton("🌐 Open in Browser", ColAccentGreen, 160);
            _btnCloudOpen.Click += delegate { LaunchSelectedCloudApp(); };

            _btnCloudEditConfig = CreateStyledButton("📝 Edit Links (Notepad)", ColHover, 180);
            _btnCloudEditConfig.Click += delegate { ConfigManager.OpenCloudConfigInEditor(); };

            _btnCloudReload = CreateStyledButton("🔄 Refresh List", ColHover, 110);
            _btnCloudReload.Click += delegate { LoadCloudAppCatalog(); };

            Panel searchWrap = new Panel();
            searchWrap.Dock = DockStyle.Right;
            searchWrap.Width = 240;

            Label lblCSearch = new Label();
            lblCSearch.Text = "🔍";
            lblCSearch.AutoSize = true;
            lblCSearch.Location = new Point(6, 10);
            lblCSearch.ForeColor = ColTextSecondary;

            _txtCloudSearch = new TextBox();
            _txtCloudSearch.Width = 195;
            _txtCloudSearch.Location = new Point(32, 6);
            _txtCloudSearch.BackColor = ColBg;
            _txtCloudSearch.ForeColor = ColTextPrimary;
            _txtCloudSearch.BorderStyle = BorderStyle.FixedSingle;
            _txtCloudSearch.Font = new Font("Segoe UI", 9.5F);
            _txtCloudSearch.TextChanged += delegate { FilterCloudApps(_txtCloudSearch.Text); };

            searchWrap.Controls.Add(lblCSearch);
            searchWrap.Controls.Add(_txtCloudSearch);

            Button btnCloudAdd = CreateStyledButton("➕ Add App", Color.FromArgb(16, 185, 129), 115);
            btnCloudAdd.Click += delegate {
                using (AddCloudAppForm dlg = new AddCloudAppForm())
                {
                    if (dlg.ShowDialog(this) == DialogResult.OK && dlg.CreatedItem != null)
                    {
                        _cloudApps.Add(dlg.CreatedItem);
                        ConfigManager.SaveCloudApps(_cloudApps);
                        PopulateCloudGrid(_cloudApps);
                        MessageBox.Show("Successfully added '" + dlg.CreatedItem.Name + "' to Google Drive Apps!", "App Added", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    }
                }
            };

            Button btnCloudDelete = CreateStyledButton("🗑️ Delete", Color.FromArgb(185, 28, 28), 95);
            btnCloudDelete.Click += delegate {
                if (_gridCloudApps.SelectedRows.Count == 0) return;
                CloudAppItem sel = _gridCloudApps.SelectedRows[0].Tag as CloudAppItem;
                if (sel == null) return;
                DialogResult dr = MessageBox.Show("Are you sure you want to remove '" + sel.Name + "' from Google Drive Apps?", "Confirm Removal", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
                if (dr == DialogResult.Yes)
                {
                    _cloudApps.Remove(sel);
                    ConfigManager.SaveCloudApps(_cloudApps);
                    PopulateCloudGrid(_cloudApps);
                }
            };

            Button btnCloudPush = CreateStyledButton("🚀 Push to GitHub", Color.FromArgb(79, 70, 229), 145);
            btnCloudPush.Click += delegate {
                string msg;
                ConfigManager.PushToGitHub(out msg);
            };

            FlowLayoutPanel leftFlow = new FlowLayoutPanel();
            leftFlow.Dock = DockStyle.Fill;
            leftFlow.BackColor = Color.Transparent;
            leftFlow.Controls.Add(_btnCloudOpen);
            leftFlow.Controls.Add(btnCloudAdd);
            leftFlow.Controls.Add(btnCloudDelete);
            leftFlow.Controls.Add(btnCloudPush);
            leftFlow.Controls.Add(_btnCloudEditConfig);
            leftFlow.Controls.Add(_btnCloudReload);

            _cloudTopBar.Controls.Add(leftFlow);
            _cloudTopBar.Controls.Add(searchWrap);

            // Split Container
            _cloudSplitContainer = new SplitContainer();
            _cloudSplitContainer.Dock = DockStyle.Fill;
            _cloudSplitContainer.BackColor = ColBorder;
            _cloudSplitContainer.SplitterWidth = 4;
            _cloudSplitContainer.SplitterDistance = 640;

            // Grid Left
            _gridCloudApps = new DataGridView();
            _gridCloudApps.Dock = DockStyle.Fill;
            _gridCloudApps.BackgroundColor = ColBg;
            _gridCloudApps.BorderStyle = BorderStyle.None;
            _gridCloudApps.CellBorderStyle = DataGridViewCellBorderStyle.SingleHorizontal;
            _gridCloudApps.GridColor = Color.FromArgb(30, 41, 59);
            _gridCloudApps.EnableHeadersVisualStyles = false;
            _gridCloudApps.RowHeadersVisible = false;
            _gridCloudApps.AllowUserToAddRows = false;
            _gridCloudApps.AllowUserToDeleteRows = false;
            _gridCloudApps.AllowUserToResizeRows = false;
            _gridCloudApps.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            _gridCloudApps.MultiSelect = false;
            _gridCloudApps.RowTemplate.Height = 34;

            _gridCloudApps.ColumnHeadersHeight = 36;
            _gridCloudApps.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.DisableResizing;
            _gridCloudApps.ColumnHeadersBorderStyle = DataGridViewHeaderBorderStyle.None;
            _gridCloudApps.ColumnHeadersDefaultCellStyle.BackColor = ColCard;
            _gridCloudApps.ColumnHeadersDefaultCellStyle.ForeColor = ColTextPrimary;
            _gridCloudApps.ColumnHeadersDefaultCellStyle.Font = new Font("Segoe UI", 9.5F, FontStyle.Bold);
            _gridCloudApps.ColumnHeadersDefaultCellStyle.Padding = new Padding(4);

            _gridCloudApps.DefaultCellStyle.BackColor = ColBg;
            _gridCloudApps.DefaultCellStyle.ForeColor = ColTextPrimary;
            _gridCloudApps.DefaultCellStyle.SelectionBackColor = Color.FromArgb(30, 58, 138);
            _gridCloudApps.DefaultCellStyle.SelectionForeColor = Color.White;
            _gridCloudApps.DefaultCellStyle.Font = new Font("Segoe UI", 9.0F);

            _gridCloudApps.AlternatingRowsDefaultCellStyle.BackColor = Color.FromArgb(20, 30, 48);
            _gridCloudApps.AlternatingRowsDefaultCellStyle.ForeColor = ColTextPrimary;

            DataGridViewTextBoxColumn cName = new DataGridViewTextBoxColumn();
            cName.HeaderText = "Application Name";
            cName.Name = "CName";
            cName.ReadOnly = true;
            cName.AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill;
            cName.DefaultCellStyle.Font = new Font("Segoe UI", 9.5F, FontStyle.Bold);
            _gridCloudApps.Columns.Add(cName);

            DataGridViewTextBoxColumn cCat = new DataGridViewTextBoxColumn();
            cCat.HeaderText = "Category";
            cCat.Name = "CCat";
            cCat.Width = 110;
            cCat.ReadOnly = true;
            _gridCloudApps.Columns.Add(cCat);

            DataGridViewTextBoxColumn cVer = new DataGridViewTextBoxColumn();
            cVer.HeaderText = "Version";
            cVer.Name = "CVer";
            cVer.Width = 85;
            cVer.ReadOnly = true;
            _gridCloudApps.Columns.Add(cVer);

            DataGridViewTextBoxColumn cSize = new DataGridViewTextBoxColumn();
            cSize.HeaderText = "Size";
            cSize.Name = "CSize";
            cSize.Width = 80;
            cSize.ReadOnly = true;
            _gridCloudApps.Columns.Add(cSize);

            _gridCloudApps.SelectionChanged += OnGridCloudSelectionChanged;
            _gridCloudApps.CellDoubleClick += delegate { LaunchSelectedCloudApp(); };

            _cloudSplitContainer.Panel1.Controls.Add(_gridCloudApps);

            // Right Details
            _cloudDetailsPanel = new Panel();
            _cloudDetailsPanel.Dock = DockStyle.Fill;
            _cloudDetailsPanel.BackColor = ColCard;
            _cloudDetailsPanel.Padding = new Padding(16);

            _lblCloudTitle = new Label();
            _lblCloudTitle.Text = "Application Package Details";
            _lblCloudTitle.Font = new Font("Segoe UI", 12.0F, FontStyle.Bold);
            _lblCloudTitle.ForeColor = ColAccentBlue;
            _lblCloudTitle.AutoSize = true;
            _lblCloudTitle.Location = new Point(16, 12);

            _lblCloudCategory = new Label();
            _lblCloudCategory.Text = "📁 Category: -";
            _lblCloudCategory.ForeColor = ColTextSecondary;
            _lblCloudCategory.AutoSize = true;
            _lblCloudCategory.Location = new Point(16, 38);

            _lblCloudVersion = new Label();
            _lblCloudVersion.Text = "🏷️ Version: -";
            _lblCloudVersion.ForeColor = ColTextSecondary;
            _lblCloudVersion.AutoSize = true;
            _lblCloudVersion.Location = new Point(16, 58);

            _lblCloudSize = new Label();
            _lblCloudSize.Text = "💾 Package Size: -";
            _lblCloudSize.ForeColor = ColTextSecondary;
            _lblCloudSize.AutoSize = true;
            _lblCloudSize.Location = new Point(16, 78);

            Label lblDescHeader = new Label();
            lblDescHeader.Text = "Description & Technician Notes:";
            lblDescHeader.ForeColor = ColTextSecondary;
            lblDescHeader.Font = new Font("Segoe UI", 9.0F, FontStyle.Bold);
            lblDescHeader.AutoSize = true;
            lblDescHeader.Location = new Point(16, 106);

            _txtCloudDesc = new TextBox();
            _txtCloudDesc.Location = new Point(16, 126);
            _txtCloudDesc.Width = 400;
            _txtCloudDesc.Height = 110;
            _txtCloudDesc.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            _txtCloudDesc.Multiline = true;
            _txtCloudDesc.ReadOnly = true;
            _txtCloudDesc.BackColor = ColBg;
            _txtCloudDesc.ForeColor = ColTextPrimary;
            _txtCloudDesc.BorderStyle = BorderStyle.None;
            _txtCloudDesc.Font = new Font("Segoe UI", 9.0F);

            Label lblUrlHeader = new Label();
            lblUrlHeader.Text = "Google Drive Target Link:";
            lblUrlHeader.ForeColor = ColTextSecondary;
            lblUrlHeader.Font = new Font("Segoe UI", 9.0F, FontStyle.Bold);
            lblUrlHeader.AutoSize = true;
            lblUrlHeader.Location = new Point(16, 246);

            _txtCloudUrl = new TextBox();
            _txtCloudUrl.Location = new Point(16, 266);
            _txtCloudUrl.Width = 400;
            _txtCloudUrl.Height = 24;
            _txtCloudUrl.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            _txtCloudUrl.ReadOnly = true;
            _txtCloudUrl.BackColor = ColBg;
            _txtCloudUrl.ForeColor = ColAccentBlue;
            _txtCloudUrl.BorderStyle = BorderStyle.FixedSingle;
            _txtCloudUrl.Font = new Font("Segoe UI", 9.0F);

            _btnCloudLaunchDirect = CreateStyledButton("🚀 Open Google Drive Download Page", ColAccentGreen, 300);
            _btnCloudLaunchDirect.Height = 44;
            _btnCloudLaunchDirect.Location = new Point(16, 306);
            _btnCloudLaunchDirect.Font = new Font("Segoe UI", 10.5F, FontStyle.Bold);
            _btnCloudLaunchDirect.Click += delegate { LaunchSelectedCloudApp(); };

            _cloudDetailsPanel.Controls.Add(_lblCloudTitle);
            _cloudDetailsPanel.Controls.Add(_lblCloudCategory);
            _cloudDetailsPanel.Controls.Add(_lblCloudVersion);
            _cloudDetailsPanel.Controls.Add(_lblCloudSize);
            _cloudDetailsPanel.Controls.Add(lblDescHeader);
            _cloudDetailsPanel.Controls.Add(_txtCloudDesc);
            _cloudDetailsPanel.Controls.Add(lblUrlHeader);
            _cloudDetailsPanel.Controls.Add(_txtCloudUrl);
            _cloudDetailsPanel.Controls.Add(_btnCloudLaunchDirect);

            _cloudSplitContainer.Panel2.Controls.Add(_cloudDetailsPanel);

            _panelCloud.Controls.Add(_cloudSplitContainer);
            _panelCloud.Controls.Add(_cloudTopBar);
        }

        // =========================================================================
        // 3. SYSTEM TOOLS & QUICK ACTIONS PANEL
        // =========================================================================
        private void InitializeToolsPanel()
        {
            _panelTools = new Panel();
            _panelTools.Dock = DockStyle.Fill;
            _panelTools.BackColor = ColBg;

            // Header banner
            Panel toolsBanner = new Panel();
            toolsBanner.Dock = DockStyle.Top;
            toolsBanner.Height = 50;
            toolsBanner.BackColor = ColCardAlt;
            toolsBanner.Padding = new Padding(16, 12, 16, 10);

            Label lblToolsBanner = new Label();
            lblToolsBanner.Text = "🛠️ Windows System Shortcuts, Optimization, and Configuration Utilities";
            lblToolsBanner.Font = new Font("Segoe UI", 11.0F, FontStyle.Bold);
            lblToolsBanner.ForeColor = ColAccentBlue;
            lblToolsBanner.AutoSize = true;
            toolsBanner.Controls.Add(lblToolsBanner);

            // Output Terminal at bottom of Tools Tab
            Panel toolsLogWrap = new Panel();
            toolsLogWrap.Dock = DockStyle.Bottom;
            toolsLogWrap.Height = 180;
            toolsLogWrap.BackColor = ColCard;
            toolsLogWrap.Padding = new Padding(12, 6, 12, 8);

            Label lblToolsLogTitle = new Label();
            lblToolsLogTitle.Text = "COMMAND EXECUTION OUTPUT:";
            lblToolsLogTitle.Font = new Font("Segoe UI", 8.5F, FontStyle.Bold);
            lblToolsLogTitle.ForeColor = ColTextSecondary;
            lblToolsLogTitle.Location = new Point(12, 6);
            lblToolsLogTitle.AutoSize = true;

            _rtbToolsLog = new RichTextBox();
            _rtbToolsLog.Location = new Point(12, 26);
            _rtbToolsLog.Width = toolsLogWrap.Width - 24;
            _rtbToolsLog.Height = 145;
            _rtbToolsLog.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            _rtbToolsLog.BackColor = Color.FromArgb(10, 15, 29);
            _rtbToolsLog.ForeColor = Color.FromArgb(226, 232, 240);
            _rtbToolsLog.Font = new Font("Consolas", 9.0F);
            _rtbToolsLog.BorderStyle = BorderStyle.None;
            _rtbToolsLog.ReadOnly = true;

            toolsLogWrap.Controls.Add(lblToolsLogTitle);
            toolsLogWrap.Controls.Add(_rtbToolsLog);

            // Flow Panel for tool cards
            _toolsFlowPanel = new FlowLayoutPanel();
            _toolsFlowPanel.Dock = DockStyle.Fill;
            _toolsFlowPanel.AutoScroll = true;
            _toolsFlowPanel.Padding = new Padding(16);
            _toolsFlowPanel.BackColor = ColBg;

            // -------------------------------------------------------------
            // SECTION 1: ⚡ PERFORMANCE & 1-CLICK TWEAKS
            // -------------------------------------------------------------
            _toolsFlowPanel.Controls.Add(CreateSectionHeader("⚡ PERFORMANCE & 1-CLICK WINDOWS TWEAKS", ColAccentAmber));

            _toolsFlowPanel.Controls.Add(CreateToolCard(
                "⚡ Ultimate Performance Plan",
                "Enables Windows Ultimate Performance plan to prevent CPU throttling and maximize FPS.",
                "Activate Ultimate Plan",
                delegate {
                    LogToolMessage("Activating Ultimate Performance Power Plan...");
                    string res = SystemToolsManager.EnableUltimatePerformance();
                    LogToolMessage(res);
                },
                ColAccentAmber
            ));

            _toolsFlowPanel.Controls.Add(CreateToolCard(
                "👁️ Show File Extensions & Hidden",
                "Unhides file extensions (.exe, .zip, .iso) and hidden files in File Explorer.",
                "Show Extensions & Hidden",
                delegate {
                    LogToolMessage("Updating Explorer folder view settings...");
                    string res = SystemToolsManager.ToggleShowFileExtensions();
                    LogToolMessage(res);
                },
                ColAccentAmber
            ));

            _toolsFlowPanel.Controls.Add(CreateToolCard(
                "🔋 Disable Hibernation (Save 8-16 GB)",
                "Deletes hiberfil.sys and disables hibernation to immediately reclaim SSD storage.",
                "Disable Hibernation",
                delegate {
                    LogToolMessage("Disabling Windows Hibernation...");
                    string res = SystemToolsManager.DisableHibernation();
                    LogToolMessage(res);
                },
                ColAccentAmber
            ));

            _toolsFlowPanel.Controls.Add(CreateToolCard(
                "🧹 1-Click Temp & Junk Cleaner",
                "Safely cleans %temp%, Windows Temp, and Prefetch junk cache to free up disk space.",
                "Clean Temp Files",
                delegate {
                    LogToolMessage("Cleaning temporary and cache junk files...");
                    string res = SystemToolsManager.CleanJunkAndTempFiles();
                    LogToolMessage(res);
                },
                ColAccentAmber
            ));

            Panel lastSec1Card = CreateToolCard(
                "🩺 System File Repair (SFC & DISM)",
                "Scans and repairs corrupted Windows system files and component store.",
                "Run SFC & DISM Scan",
                delegate {
                    LogToolMessage("Launching elevated System File Checker & DISM repair...");
                    SystemToolsManager.RunSystemFileCheck();
                },
                ColAccentAmber
            );
            _toolsFlowPanel.Controls.Add(lastSec1Card);
            _toolsFlowPanel.SetFlowBreak(lastSec1Card, true);

            // -------------------------------------------------------------
            // SECTION 2: 🖥️ HARDWARE & DIAGNOSTICS
            // -------------------------------------------------------------
            _toolsFlowPanel.Controls.Add(CreateSectionHeader("🖥️ HARDWARE, DRIVERS & DIAGNOSTICS", ColAccentBlue));

            _toolsFlowPanel.Controls.Add(CreateToolCard(
                "🖥️ Device Manager",
                "Check hardware devices, installed components, and missing drivers.",
                "Open devmgmt.msc",
                delegate {
                    LogToolMessage("Launching Windows Device Manager...");
                    SystemToolsManager.OpenDeviceManager();
                },
                ColAccentBlue
            ));

            _toolsFlowPanel.Controls.Add(CreateToolCard(
                "⚡ IObit Driver Booster",
                "Extract and run Driver Booster to automatically scan & install missing hardware drivers.",
                "🚀 Launch Driver Booster",
                delegate {
                    LogToolMessage("Checking IObit Driver Booster Portable...");
                    string res = SystemToolsManager.LaunchOrDeployDriverBooster();
                    LogToolMessage(res);
                },
                ColAccentBlue
            ));

            _toolsFlowPanel.Controls.Add(CreateToolCard(
                "💿 CrystalDiskInfo (Portable)",
                "Inspect SSD/HDD health, remaining life percentage, temperature, and SMART status.",
                "🚀 Launch CrystalDiskInfo",
                delegate {
                    LogToolMessage("Checking CrystalDiskInfo Portable...");
                    string res = SystemToolsManager.LaunchCrystalDiskInfo();
                    LogToolMessage(res);
                },
                ColAccentBlue
            ));

            Panel lastSec2Card = CreateToolCard(
                "⚙️ CPU-Z (Portable)",
                "View detailed CPU clock speeds, Motherboard model, and Dual-Channel RAM specs.",
                "🚀 Launch CPU-Z",
                delegate {
                    LogToolMessage("Checking CPU-Z Portable...");
                    string res = SystemToolsManager.LaunchCpuZ();
                    LogToolMessage(res);
                },
                ColAccentBlue
            );
            _toolsFlowPanel.Controls.Add(lastSec2Card);
            _toolsFlowPanel.SetFlowBreak(lastSec2Card, true);

            // -------------------------------------------------------------
            // SECTION 3: 🛠️ WINDOWS ADMINISTRATION
            // -------------------------------------------------------------
            _toolsFlowPanel.Controls.Add(CreateSectionHeader("🛠️ WINDOWS SYSTEM ADMINISTRATION", ColAccentGreen));

            _toolsFlowPanel.Controls.Add(CreateToolCard(
                "💾 Disk Management (diskmgmt.msc)",
                "Partition drives, initialize new SSD/HDD, shrink/extend volumes, and create Drive D:.",
                "Open diskmgmt.msc",
                delegate {
                    LogToolMessage("Opening Windows Disk Management console...");
                    SystemToolsManager.OpenDiskManagement();
                },
                ColAccentGreen
            ));

            _toolsFlowPanel.Controls.Add(CreateToolCard(
                "📋 System Properties (sysdm.cpl)",
                "Rename PC, change Workgroup, and configure Pagefile / Virtual Memory.",
                "Open sysdm.cpl",
                delegate {
                    LogToolMessage("Opening System Properties (Advanced)...");
                    SystemToolsManager.OpenSystemPropertiesAdvanced();
                },
                ColAccentGreen
            ));

            _toolsFlowPanel.Controls.Add(CreateToolCard(
                "🚀 Task Manager & Startup Apps",
                "Inspect real-time CPU/RAM usage, kill hanging processes, and manage startup programs.",
                "Open Task Manager",
                delegate {
                    LogToolMessage("Launching Windows Task Manager...");
                    SystemToolsManager.OpenTaskManager();
                },
                ColAccentGreen
            ));

            _toolsFlowPanel.Controls.Add(CreateToolCard(
                "⚙️ Windows Services (services.msc)",
                "Manage background Windows services, start/stop services, and set startup types.",
                "Open services.msc",
                delegate {
                    LogToolMessage("Opening Windows Services console...");
                    SystemToolsManager.OpenServicesManager();
                },
                ColAccentGreen
            ));

            _toolsFlowPanel.Controls.Add(CreateToolCard(
                "🛡️ Windows Defender Security",
                "Open Windows Security settings, Virus & Threat Protection, and file exclusions.",
                "Open Windows Security",
                delegate {
                    LogToolMessage("Opening Windows Security...");
                    SystemToolsManager.OpenWindowsSecurity();
                },
                ColAccentGreen
            ));

            Panel lastSec3Card = CreateToolCard(
                "🔑 Windows Activation & License",
                "Check genuine activation status or enter product key in Windows Settings.",
                "Open Activation",
                delegate {
                    LogToolMessage("Opening Windows Activation & Licensing panel...");
                    SystemToolsManager.OpenActivationSettings();
                },
                ColAccentGreen
            );
            _toolsFlowPanel.Controls.Add(lastSec3Card);
            _toolsFlowPanel.SetFlowBreak(lastSec3Card, true);

            // -------------------------------------------------------------
            // SECTION 4: 🌐 NETWORK & CONNECTIVITY
            // -------------------------------------------------------------
            _toolsFlowPanel.Controls.Add(CreateSectionHeader("🌐 NETWORK, TIME & DNS CONFIGURATION", Color.FromArgb(56, 189, 248)));

            _toolsFlowPanel.Controls.Add(CreateToolCard(
                "🕒 Date & Time Settings",
                "Open Windows Settings to configure clock, automatic time, and calendar.",
                "Open Time Settings",
                delegate {
                    LogToolMessage("Opening Windows Date & Time settings...");
                    SystemToolsManager.OpenDateAndTimeSettings();
                }
            ));

            _toolsFlowPanel.Controls.Add(CreateToolCard(
                "🔄 Sync Internet Time Now",
                "Force resynchronization of system clock with Windows Internet Time servers.",
                "Sync Clock Now",
                delegate {
                    LogToolMessage("Synchronizing system time with time servers...");
                    string result = SystemToolsManager.SyncTimeNow();
                    LogToolMessage(result);
                }
            ));

            _toolsFlowPanel.Controls.Add(CreateToolCard(
                "🌐 Time Zone Configuration",
                "Open Time Zone selector to change system region (e.g. UTC+08:00 Manila).",
                "Change Time Zone",
                delegate {
                    LogToolMessage("Opening Time Zone selector dialog...");
                    SystemToolsManager.OpenTimeZoneSettings();
                }
            ));

            _toolsFlowPanel.Controls.Add(CreateToolCard(
                "🔌 Network Adapters (ncpa.cpl)",
                "Open classic Network Connections control panel to inspect Ethernet and WiFi.",
                "Open ncpa.cpl",
                delegate {
                    LogToolMessage("Opening Network Connections panel...");
                    SystemToolsManager.OpenNetworkConnections();
                }
            ));

            _toolsFlowPanel.Controls.Add(CreateToolCard(
                "🧹 Flush DNS Resolver Cache",
                "Clears DNS resolver cache to fix internet loading issues and resolve hostnames.",
                "Flush DNS Cache",
                delegate {
                    LogToolMessage("Flushing DNS resolver cache via ipconfig...");
                    string result = SystemToolsManager.FlushDns();
                    LogToolMessage(result);
                }
            ));

            _toolsFlowPanel.Controls.Add(CreateToolCard(
                "⚡ Set Cloudflare DNS (1.1.1.1)",
                "Configures high-speed, privacy-focused Cloudflare DNS (1.1.1.1 & 1.0.0.1).",
                "Apply 1.1.1.1",
                delegate {
                    LogToolMessage("Configuring Cloudflare DNS servers...");
                    string res = SystemToolsManager.SetDnsServers("cloudflare");
                    LogToolMessage(res);
                }
            ));

            _toolsFlowPanel.Controls.Add(CreateToolCard(
                "⚡ Set Google DNS (8.8.8.8)",
                "Configures fast Google Public DNS servers (8.8.8.8 & 8.8.4.4).",
                "Apply 8.8.8.8",
                delegate {
                    LogToolMessage("Configuring Google Public DNS servers...");
                    string res = SystemToolsManager.SetDnsServers("google");
                    LogToolMessage(res);
                }
            ));

            Panel lastSec4Card = CreateToolCard(
                "🔄 Reset DNS to Automatic (DHCP)",
                "Restores automatic router/ISP DNS server assignment on all active adapters.",
                "Reset to DHCP",
                delegate {
                    LogToolMessage("Resetting DNS server configuration to DHCP...");
                    string res = SystemToolsManager.SetDnsServers("dhcp");
                    LogToolMessage(res);
                }
            );
            _toolsFlowPanel.Controls.Add(lastSec4Card);
            _toolsFlowPanel.SetFlowBreak(lastSec4Card, true);

            // -------------------------------------------------------------
            // SECTION 5: 💻 AUTOMATION & PERSONALIZATION
            // -------------------------------------------------------------
            Color ColAccentPurple = Color.FromArgb(192, 132, 252);
            _toolsFlowPanel.Controls.Add(CreateSectionHeader("💻 AUTOMATION, SCRIPTS & CUSTOMIZATION", ColAccentPurple));

            _toolsFlowPanel.Controls.Add(CreateToolCard(
                "▶️ Run Custom Script (custom.ps1)",
                "Auto-runs your custom PowerShell code in scripts\\custom.ps1 as Administrator.",
                "▶️ Run custom.ps1",
                delegate {
                    LogToolMessage("Launching custom PowerShell script (scripts\\custom.ps1)...");
                    SystemToolsManager.OpenCustomPowerShellScript();
                },
                ColAccentPurple
            ));

            _toolsFlowPanel.Controls.Add(CreateToolCard(
                "📝 Edit custom.ps1 Script",
                "Opens scripts\\custom.ps1 in Notepad so you can paste or edit your commands.",
                "📝 Edit in Notepad",
                delegate {
                    LogToolMessage("Opening scripts\\custom.ps1 in Notepad...");
                    SystemToolsManager.EditCustomPowerShellScript();
                },
                ColAccentPurple
            ));

            _toolsFlowPanel.Controls.Add(CreateToolCard(
                "💻 Administrator PowerShell",
                "Opens a clean elevated PowerShell console window ready for any commands.",
                "💻 Open PowerShell",
                delegate {
                    LogToolMessage("Opening Administrator PowerShell console...");
                    SystemToolsManager.OpenElevatedPowerShell();
                },
                ColAccentPurple
            ));

            _toolsFlowPanel.Controls.Add(CreateToolCard(
                "🖼️ Desktop Background & Themes",
                "Change desktop wallpaper, lock screen, colors, and Windows dark/light mode.",
                "Change Background",
                delegate {
                    LogToolMessage("Opening Personalization & Wallpaper settings...");
                    SystemToolsManager.OpenDesktopBackgroundSettings();
                },
                ColAccentPurple
            ));

            _panelTools.Controls.Add(_toolsFlowPanel);
            _panelTools.Controls.Add(toolsLogWrap);
            _panelTools.Controls.Add(toolsBanner);
        }

        private Panel CreateSectionHeader(string title, Color accentColor)
        {
            Panel header = new Panel();
            header.Width = 1080;
            header.Height = 36;
            header.Margin = new Padding(10, 16, 10, 6);
            header.BackColor = Color.FromArgb(20, 29, 47);

            Panel bar = new Panel();
            bar.Width = 5;
            bar.Dock = DockStyle.Left;
            bar.BackColor = accentColor;

            Label lbl = new Label();
            lbl.Text = title;
            lbl.Font = new Font("Segoe UI", 10.0F, FontStyle.Bold);
            lbl.ForeColor = accentColor;
            lbl.Location = new Point(14, 8);
            lbl.AutoSize = true;

            header.Controls.Add(lbl);
            header.Controls.Add(bar);

            _toolsFlowPanel.SetFlowBreak(header, true);
            return header;
        }

        private Panel CreateToolCard(string title, string description, string buttonText, EventHandler onClick)
        {
            return CreateToolCard(title, description, buttonText, onClick, ColAccentBlue);
        }

        private Panel CreateToolCard(string title, string description, string buttonText, EventHandler onClick, Color titleColor)
        {
            Panel card = new Panel();
            card.Width = 340;
            card.Height = 135;
            card.BackColor = ColCard;
            card.Margin = new Padding(10);
            card.Padding = new Padding(12);

            Label lblT = new Label();
            lblT.Text = title;
            lblT.Font = new Font("Segoe UI", 10.0F, FontStyle.Bold);
            lblT.ForeColor = titleColor;
            lblT.AutoSize = true;
            lblT.Location = new Point(12, 10);

            Label lblD = new Label();
            lblD.Text = description;
            lblD.Font = new Font("Segoe UI", 8.5F);
            lblD.ForeColor = ColTextSecondary;
            lblD.Location = new Point(12, 36);
            lblD.Width = 316;
            lblD.Height = 44;

            Button btn = CreateStyledButton(buttonText, ColHover, 200);
            btn.Height = 32;
            btn.Location = new Point(12, 88);
            btn.Click += onClick;

            card.Controls.Add(lblT);
            card.Controls.Add(lblD);
            card.Controls.Add(btn);

            return card;
        }

        private void LogToolMessage(string message)
        {
            _rtbToolsLog.SelectionStart = _rtbToolsLog.TextLength;
            _rtbToolsLog.SelectionLength = 0;
            _rtbToolsLog.SelectionColor = ColAccentBlue;
            _rtbToolsLog.AppendText(string.Format("[{0:HH:mm:ss}] {1}\n", DateTime.Now, message));
            _rtbToolsLog.ScrollToCaret();
        }

        // =========================================================================
        // CLOUD APPS LOGIC
        // =========================================================================
        private void LoadCloudAppCatalog()
        {
            _cloudApps = ConfigManager.LoadCloudApps();
            PopulateCloudGrid(_cloudApps);
        }

        private void PopulateCloudGrid(List<CloudAppItem> list)
        {
            _gridCloudApps.SuspendLayout();
            _gridCloudApps.Rows.Clear();

            foreach (CloudAppItem app in list)
            {
                int idx = _gridCloudApps.Rows.Add();
                DataGridViewRow row = _gridCloudApps.Rows[idx];
                row.Tag = app;
                row.Cells["CName"].Value = app.Name;
                row.Cells["CCat"].Value = app.Category;
                row.Cells["CVer"].Value = app.Version;
                row.Cells["CSize"].Value = app.EstimatedSize;
            }

            _gridCloudApps.ResumeLayout();
            if (_gridCloudApps.Rows.Count > 0)
            {
                DisplayCloudAppDetails(_gridCloudApps.Rows[0].Tag as CloudAppItem);
            }
        }

        private void FilterCloudApps(string query)
        {
            if (string.IsNullOrEmpty(query))
            {
                PopulateCloudGrid(_cloudApps);
                return;
            }

            string q = query.ToLowerInvariant();
            List<CloudAppItem> filtered = new List<CloudAppItem>();
            foreach (CloudAppItem c in _cloudApps)
            {
                if (c.Name.ToLowerInvariant().Contains(q) ||
                    c.Category.ToLowerInvariant().Contains(q) ||
                    c.Description.ToLowerInvariant().Contains(q))
                {
                    filtered.Add(c);
                }
            }
            PopulateCloudGrid(filtered);
        }

        private void OnGridCloudSelectionChanged(object sender, EventArgs e)
        {
            if (_gridCloudApps.SelectedRows.Count == 0) return;
            CloudAppItem app = _gridCloudApps.SelectedRows[0].Tag as CloudAppItem;
            if (app != null)
            {
                DisplayCloudAppDetails(app);
            }
        }

        private void DisplayCloudAppDetails(CloudAppItem app)
        {
            if (app == null) return;
            _lblCloudTitle.Text = app.Name;
            _lblCloudCategory.Text = "📁 Category: " + app.Category;
            _lblCloudVersion.Text = "🏷️ Version: " + app.Version;
            _lblCloudSize.Text = "💾 Package Size: " + app.EstimatedSize;
            _txtCloudDesc.Text = app.Description + (string.IsNullOrEmpty(app.Notes) ? "" : "\r\n\r\nNote: " + app.Notes);
            _txtCloudUrl.Text = app.DriveUrl;
        }

        private void LaunchSelectedCloudApp()
        {
            if (_gridCloudApps.SelectedRows.Count == 0)
            {
                MessageBox.Show("Please select an application package from the list.", "No Item Selected", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }
            CloudAppItem app = _gridCloudApps.SelectedRows[0].Tag as CloudAppItem;
            if (app != null && !string.IsNullOrEmpty(app.DriveUrl))
            {
                try
                {
                    Process.Start(app.DriveUrl);
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Could not launch URL: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        // =========================================================================
        // SOFTWARE GRID LOGIC
        // =========================================================================
        private Button CreateStyledButton(string text, Color bg, int width)
        {
            Button btn = new Button();
            btn.Text = text;
            btn.BackColor = bg;
            btn.ForeColor = ColTextPrimary;
            btn.FlatStyle = FlatStyle.Flat;
            btn.FlatAppearance.BorderSize = 0;
            btn.Width = width;
            btn.Height = 32;
            btn.Cursor = Cursors.Hand;
            btn.Font = new Font("Segoe UI", 9.0F, FontStyle.Bold);
            return btn;
        }

        private void LoadAppCatalog()
        {
            _allApps = ConfigManager.LoadApps();
            PopulateGrid(_allApps);
            LogText(string.Format("Software catalog loaded ({0} applications ready).", _allApps.Count), LogLevel.Info);
            UpdateSelectionSummary();
        }

        private void PopulateGrid(List<AppItem> apps)
        {
            _gridApps.SuspendLayout();
            _gridApps.Rows.Clear();

            foreach (AppItem app in apps)
            {
                int rowIndex = _gridApps.Rows.Add();
                DataGridViewRow row = _gridApps.Rows[rowIndex];
                row.Tag = app;

                row.Cells["ColCheck"].Value = app.IsSelected;
                row.Cells["ColName"].Value = app.Name;
                row.Cells["ColCategory"].Value = app.Category;
                row.Cells["ColSize"].Value = app.EstimatedSizeMB > 0 ? string.Format("{0} MB", app.EstimatedSizeMB) : "-";

                string sourceText = app.IsCached ? "✔ USB Cache" : "☁ Download";
                row.Cells["ColSource"].Value = sourceText;
                row.Cells["ColSource"].Style.ForeColor = app.IsCached ? Color.FromArgb(52, 211, 153) : ColAccentBlue;
                row.Cells["ColSource"].Style.Font = new Font("Segoe UI", 9.0F, FontStyle.Bold);

                row.Cells["ColStatus"].Value = app.Status;
                ApplyRowStatusColor(row, app.Status);
            }

            _gridApps.ResumeLayout();
        }

        private void ApplyRowStatusColor(DataGridViewRow row, string status)
        {
            if (status == "Installed")
            {
                row.Cells["ColStatus"].Style.ForeColor = ColAccentGreen;
                row.Cells["ColStatus"].Style.Font = new Font("Segoe UI", 9.0F, FontStyle.Bold);
            }
            else if (status == "Failed")
            {
                row.Cells["ColStatus"].Style.ForeColor = ColAccentRed;
                row.Cells["ColStatus"].Style.Font = new Font("Segoe UI", 9.0F, FontStyle.Bold);
            }
            else if (status == "Downloading" || status == "Installing" || status == "Processing")
            {
                row.Cells["ColStatus"].Style.ForeColor = ColAccentAmber;
                row.Cells["ColStatus"].Style.Font = new Font("Segoe UI", 9.0F, FontStyle.Bold);
            }
            else
            {
                row.Cells["ColStatus"].Style.ForeColor = ColTextSecondary;
            }
        }

        private void FilterApps(string query)
        {
            if (string.IsNullOrEmpty(query))
            {
                PopulateGrid(_allApps);
                return;
            }

            string lower = query.ToLowerInvariant();
            List<AppItem> filtered = new List<AppItem>();
            foreach (AppItem app in _allApps)
            {
                if (app.Name.ToLowerInvariant().Contains(lower) ||
                    app.Category.ToLowerInvariant().Contains(lower) ||
                    app.Description.ToLowerInvariant().Contains(lower))
                {
                    filtered.Add(app);
                }
            }
            PopulateGrid(filtered);
        }

        private void ApplyPreset(string preset)
        {
            foreach (DataGridViewRow row in _gridApps.Rows)
            {
                AppItem app = row.Tag as AppItem;
                if (app != null)
                {
                    bool match = app.MatchesPreset(preset);
                    app.IsSelected = match;
                    row.Cells["ColCheck"].Value = match;
                }
            }
            UpdateSelectionSummary();
        }

        private void SetAllSelection(bool check)
        {
            foreach (DataGridViewRow row in _gridApps.Rows)
            {
                AppItem app = row.Tag as AppItem;
                if (app != null)
                {
                    app.IsSelected = check;
                    row.Cells["ColCheck"].Value = check;
                }
            }
            UpdateSelectionSummary();
        }

        private void OnGridCellContentClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex < 0) return;
            if (e.ColumnIndex == 0)
            {
                _gridApps.CommitEdit(DataGridViewDataErrorContexts.Commit);
            }
        }

        private void OnGridCellValueChanged(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex < 0 || e.ColumnIndex != 0) return;
            DataGridViewRow row = _gridApps.Rows[e.RowIndex];
            AppItem app = row.Tag as AppItem;
            if (app == null) return;

            bool isChecked = Convert.ToBoolean(row.Cells["ColCheck"].Value);
            app.IsSelected = isChecked;
            UpdateSelectionSummary();
        }

        private void OnGridCurrentCellDirtyStateChanged(object sender, EventArgs e)
        {
            if (_gridApps.IsCurrentCellDirty)
            {
                _gridApps.CommitEdit(DataGridViewDataErrorContexts.Commit);
            }
        }

        private void OnGridCellClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex < 0) return;
            DataGridViewRow row = _gridApps.Rows[e.RowIndex];
            AppItem app = row.Tag as AppItem;
            if (app != null)
            {
                DisplayAppDetails(app);
            }

            if (e.ColumnIndex > 0)
            {
                bool current = Convert.ToBoolean(row.Cells["ColCheck"].Value);
                row.Cells["ColCheck"].Value = !current;
                _gridApps.CommitEdit(DataGridViewDataErrorContexts.Commit);
            }
        }

        private void OnGridSelectionChanged(object sender, EventArgs e)
        {
            if (_gridApps.SelectedRows.Count == 0) return;
            AppItem app = _gridApps.SelectedRows[0].Tag as AppItem;
            if (app != null)
            {
                DisplayAppDetails(app);
            }
        }

        private void DisplayAppDetails(AppItem app)
        {
            _lblDetailsTitle.Text = app.Name;
            _lblDetailsCategory.Text = "📁 Category: " + app.Category;
            _lblDetailsSize.Text = "💾 Size: " + (app.EstimatedSizeMB > 0 ? app.EstimatedSizeMB + " MB" : "N/A");
            _lblDetailsCache.Text = app.IsCached 
                ? "✔ Status: Cached in USB (Instant Offline Install)" 
                : "☁ Status: Requires Internet Download";
            _lblDetailsCache.ForeColor = app.IsCached ? ColAccentGreen : ColAccentBlue;

            _lblDetailsArgs.Text = string.IsNullOrEmpty(app.SilentArgs) 
                ? (string.IsNullOrEmpty(app.SpecialAction) ? "" : "⚙ Action: " + app.SpecialAction)
                : "⚙ Silent Switch: " + app.SilentArgs;

            _txtDetailsDesc.Text = app.Description;
        }

        private void UpdateSelectionSummary()
        {
            int count = 0;
            int totalMB = 0;
            int cachedCount = 0;

            foreach (AppItem app in _allApps)
            {
                if (app.IsSelected)
                {
                    count++;
                    if (app.IsCached)
                    {
                        cachedCount++;
                    }
                    else
                    {
                        totalMB += app.EstimatedSizeMB;
                    }
                }
            }

            if (count == 0)
            {
                _lblOverallStatus.Text = "Ready. Select software items and click 'Start Installation'.";
            }
            else if (totalMB == 0 && count > 0)
            {
                _lblOverallStatus.Text = string.Format("Selected: {0} applications | ALL CACHED OFFLINE (0 MB download needed!)", count);
            }
            else
            {
                _lblOverallStatus.Text = string.Format("Selected: {0} applications ({1} offline, {2} online) | Est. Download: ~{3} MB",
                    count, cachedCount, count - cachedCount, totalMB);
            }
        }

        private void OpenCacheFolder()
        {
            string cacheDir = ConfigManager.GetCacheDirectory();
            try
            {
                Process.Start("explorer.exe", cacheDir);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Could not open cache folder: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void DownloadAllMissingToCache()
        {
            List<AppItem> missing = new List<AppItem>();
            foreach (AppItem app in _allApps)
            {
                if (!app.IsCached && !string.IsNullOrEmpty(app.DownloadUrl))
                {
                    missing.Add(app);
                }
            }

            if (missing.Count == 0)
            {
                MessageBox.Show("All software items are already cached in your USB!", "Cache Complete", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            DialogResult dr = MessageBox.Show(
                string.Format("Found {0} software packages not yet in USB cache.\nDo you want to download them now to your USB folder for offline use?", missing.Count),
                "Download All to USB Cache", MessageBoxButtons.YesNo, MessageBoxIcon.Question);

            if (dr != DialogResult.Yes) return;

            foreach (AppItem app in missing)
            {
                app.IsSelected = true;
            }
            PopulateGrid(_allApps);
            UpdateSelectionSummary();

            OnActionClick(this, EventArgs.Empty);
        }

        private void OnActionClick(object sender, EventArgs e)
        {
            if (_engine.IsBusy)
            {
                _btnAction.Enabled = false;
                _btnAction.Text = "Cancelling...";
                _engine.Cancel();
                return;
            }

            List<AppItem> selected = new List<AppItem>();
            foreach (AppItem app in _allApps)
            {
                if (app.IsSelected)
                {
                    selected.Add(app);
                }
            }

            if (selected.Count == 0)
            {
                MessageBox.Show("Please select at least one software item to install.", "No Software Selected", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            _btnAction.Text = "⏹️ CANCEL QUEUE";
            _btnAction.BackColor = ColAccentRed;
            _btnAction.Enabled = true;

            _pbOverall.Value = 0;
            _pbCurrent.Value = 0;

            _engine.RunInstallationQueue(selected);
        }

        private void OnEngineLog(string message, LogLevel level)
        {
            if (this.InvokeRequired)
            {
                this.BeginInvoke(new InstallerEngine.LogHandler(OnEngineLog), new object[] { message, level });
                return;
            }
            LogText(message, level);
        }

        private void OnEngineOverallProgress(int percent, string statusText)
        {
            if (this.InvokeRequired)
            {
                this.BeginInvoke(new InstallerEngine.ProgressHandler(OnEngineOverallProgress), new object[] { percent, statusText });
                return;
            }

            _pbOverall.Value = Math.Max(0, Math.Min(100, percent));
            _lblOverallStatus.Text = statusText;

            if (percent >= 100)
            {
                _btnAction.Text = "🚀 START INSTALLATION";
                _btnAction.BackColor = ColAccentGreen;
                _btnAction.Enabled = true;
                ConfigManager.RefreshCacheStatus(_allApps);
                PopulateGrid(_allApps);
            }
        }

        private void OnEngineCurrentProgress(int percent, string statusText)
        {
            if (this.InvokeRequired)
            {
                this.BeginInvoke(new InstallerEngine.ProgressHandler(OnEngineCurrentProgress), new object[] { percent, statusText });
                return;
            }

            _pbCurrent.Value = Math.Max(0, Math.Min(100, percent));
            _lblCurrentStatus.Text = statusText;
        }

        private void OnEngineAppStatusChanged(AppItem app)
        {
            if (this.InvokeRequired)
            {
                this.BeginInvoke(new InstallerEngine.AppStatusHandler(OnEngineAppStatusChanged), new object[] { app });
                return;
            }

            foreach (DataGridViewRow row in _gridApps.Rows)
            {
                if (row.Tag == app)
                {
                    row.Cells["ColStatus"].Value = app.Status;
                    ApplyRowStatusColor(row, app.Status);

                    if (app.IsCached)
                    {
                        row.Cells["ColSource"].Value = "✔ USB Cache";
                        row.Cells["ColSource"].Style.ForeColor = Color.FromArgb(52, 211, 153);
                    }
                    break;
                }
            }
        }

        private void LogText(string message, LogLevel level)
        {
            Color c = ColTextPrimary;
            if (level == LogLevel.Success) c = ColAccentGreen;
            else if (level == LogLevel.Error) c = ColAccentRed;
            else if (level == LogLevel.Warning) c = ColAccentAmber;
            else if (level == LogLevel.Download) c = ColAccentBlue;
            else if (level == LogLevel.Info) c = Color.FromArgb(203, 213, 225);

            _rtbLog.SelectionStart = _rtbLog.TextLength;
            _rtbLog.SelectionLength = 0;
            _rtbLog.SelectionColor = c;
            _rtbLog.AppendText(string.Format("[{0:HH:mm:ss}] {1}\n", DateTime.Now, message));
            _rtbLog.ScrollToCaret();
        }
    }
}
