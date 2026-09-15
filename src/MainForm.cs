using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Drawing2D;
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
        private AppItem _selectedApp;

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
        private Label _lblSearchHint;

        private SplitContainer _splitContainer;
        private DataGridView _gridApps;

        // Right Info Card & Terminal
        private Panel _rightPanel;
        private Panel _detailsPanel;
        private PictureBox _picDetailsIcon;
        private Label _lblDetailsTitle;
        private Label _lblDetailsCategoryBadge;
        private Label _lblDetailsTypeBadge;

        private Label _lblValSize;
        private Label _lblValStatus;
        private Label _lblValSource;
        private Label _lblValSwitch;
        private TextBox _txtDetailsDesc;

        private Button _btnDetailsOpenUrl;
        private Button _btnDetailsOpenFolder;
        private FlowLayoutPanel _badgesPanel;
        private TableLayoutPanel _infoTable;
        private FlowLayoutPanel _detailsActions;

        private Panel _logHeaderPanel;
        private Label _lblLogTitle;
        private Label _lblLogIndicator;
        private Button _btnClearLog;
        private RichTextBox _rtbLog;

        // Bottom Installation Bar
        private Panel _bottomPanel;
        private TableLayoutPanel _bottomLayout;
        private Label _lblOverallStatus;
        private Label _lblTimeEstimate;
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
        private PictureBox _picCloudIcon;
        private Label _lblCloudTitle;
        private Label _lblCloudCategoryBadge;
        private Label _lblCloudVersionBadge;
        private Label _lblCloudValCat;
        private Label _lblCloudValVer;
        private Label _lblCloudValSize;
        private TextBox _txtCloudDesc;
        private TextBox _txtCloudUrl;
        private Button _btnCloudLaunchDirect;
        private FlowLayoutPanel _cloudBadges;
        private TableLayoutPanel _cloudMetaTable;

        // --- System Tools Controls ---
        private FlowLayoutPanel _toolsFlowPanel;
        private RichTextBox _rtbToolsLog;

        // Modern Slate & Neon Blue Theme Colors
        private readonly Color ColBg = Color.FromArgb(8, 14, 30);            // Deep navy / black #080E1E
        private readonly Color ColCard = Color.FromArgb(13, 23, 46);          // Dark card surface #0D172E
        private readonly Color ColCardAlt = Color.FromArgb(19, 31, 58);       // Secondary surface #131F3A
        private readonly Color ColCardBorder = Color.FromArgb(30, 45, 75);    // Card border #1E2D4B
        private readonly Color ColBorder = Color.FromArgb(26, 40, 68);        // Subtle border #1A2844
        private readonly Color ColHover = Color.FromArgb(28, 48, 86);         // Hover color #1C3056
        private readonly Color ColRowAlt = Color.FromArgb(11, 19, 38);        // Table alternate row #0B1326
        private readonly Color ColRowSelect = Color.FromArgb(12, 58, 107);     // Active row select #0C3A6B
        private readonly Color ColAccentBlue = Color.FromArgb(56, 189, 248);   // Sky 400
        private readonly Color ColAccentCyan = Color.FromArgb(14, 165, 233);   // Sky 500
        private readonly Color ColAccentGreen = Color.FromArgb(16, 185, 129);  // Emerald 500
        private readonly Color ColAccentAmber = Color.FromArgb(245, 158, 11);  // Amber 500
        private readonly Color ColAccentRed = Color.FromArgb(239, 68, 68);     // Red 500
        private readonly Color ColAccentPurple = Color.FromArgb(168, 85, 247); // Purple 500
        private readonly Color ColTextPrimary = Color.FromArgb(248, 250, 252); // Off-white
        private readonly Color ColTextSecondary = Color.FromArgb(148, 163, 184); // Slate 400
        private readonly Color ColTextMuted = Color.FromArgb(100, 116, 139);     // Slate 500

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
            this.Text = "TECH INSTALLER — Post-Install Tech Toolbox";
            this.Size = new Size(1260, 780);
            this.MinimumSize = new Size(1060, 660);
            this.StartPosition = FormStartPosition.CenterScreen;
            this.BackColor = ColBg;
            this.ForeColor = ColTextPrimary;
            this.Font = new Font("Segoe UI", 9.5F, FontStyle.Regular, GraphicsUnit.Point);
            this.KeyPreview = true;

            // ==================== TOP HEADER ====================
            _topPanel = new Panel();
            _topPanel.Dock = DockStyle.Top;
            _topPanel.Height = 70;
            _topPanel.BackColor = ColCard;
            _topPanel.Padding = new Padding(18, 10, 18, 8);
            _topPanel.Paint += delegate(object s, PaintEventArgs pe) {
                using (Pen p = new Pen(ColBorder, 1))
                {
                    pe.Graphics.DrawLine(p, 0, _topPanel.Height - 1, _topPanel.Width, _topPanel.Height - 1);
                }
            };

            _lblTitle = new Label();
            _lblTitle.Text = "TECH INSTALLER — Post-Install Tech Toolbox";
            _lblTitle.Font = new Font("Segoe UI", 13.5F, FontStyle.Bold);
            _lblTitle.ForeColor = ColTextPrimary;
            _lblTitle.AutoSize = true;
            _lblTitle.Location = new Point(18, 12);

            _lblSubtitle = new Label();
            string osName = Environment.OSVersion.ToString();
            string arch = Environment.Is64BitOperatingSystem ? "64-bit" : "32-bit";
            string rootDrive = Path.GetPathRoot(ConfigManager.GetAppDirectory());
            bool isOnline = NetworkInterface.GetIsNetworkAvailable();
            _lblSubtitle.Text = string.Format("OS: {0} ({1})   •   Drive: {2}   •   Network: {3}   •   Administrator Mode",
                osName, arch, rootDrive, isOnline ? "Online" : "Offline");
            _lblSubtitle.Font = new Font("Segoe UI", 9.0F);
            _lblSubtitle.ForeColor = ColTextSecondary;
            _lblSubtitle.AutoSize = true;
            _lblSubtitle.Location = new Point(20, 41);

            _topButtonsPanel = new FlowLayoutPanel();
            _topButtonsPanel.Dock = DockStyle.Right;
            _topButtonsPanel.FlowDirection = FlowDirection.RightToLeft;
            _topButtonsPanel.Width = 640;
            _topButtonsPanel.Height = 52;
            _topButtonsPanel.BackColor = Color.Transparent;
            _topButtonsPanel.Padding = new Padding(0, 8, 0, 0);

            _btnReload = CreatePillButton(" Reload", UiIcon.Reload, ColCardAlt, ColTextPrimary, 105, 34);
            _btnReload.Click += delegate { LoadAppCatalog(); LoadCloudAppCatalog(); };

            Button btnSyncGitHub = CreatePillButton(" Sync from GitHub", UiIcon.SyncGit, ColAccentCyan, Color.White, 175, 34);
            btnSyncGitHub.Click += delegate {
                string msg;
                bool ok = ConfigManager.FetchFromGitHub(out msg);
                LoadCloudAppCatalog();
                LoadAppCatalog();
                MessageBox.Show(msg, ok ? "GitHub Sync Complete" : "GitHub Sync Notice", MessageBoxButtons.OK, ok ? MessageBoxIcon.Information : MessageBoxIcon.Warning);
            };

            _btnDownloadAll = CreatePillButton(" Cache All to USB", UiIcon.DownloadCloud, Color.FromArgb(37, 99, 235), Color.White, 165, 34);
            _btnDownloadAll.Click += delegate { DownloadAllMissingToCache(); };

            _btnOpenCache = CreatePillButton(" Open USB Cache", UiIcon.OpenFolder, ColCardAlt, ColTextPrimary, 155, 34);
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
            _navPanel.Height = 46;
            _navPanel.BackColor = Color.FromArgb(10, 18, 38);
            _navPanel.Padding = new Padding(16, 6, 16, 6);
            _navPanel.Paint += delegate(object s, PaintEventArgs pe) {
                using (Pen p = new Pen(ColBorder, 1))
                {
                    pe.Graphics.DrawLine(p, 0, _navPanel.Height - 1, _navPanel.Width, _navPanel.Height - 1);
                }
            };

            _btnNavSoftware = CreateNavTabButton("Standard Software (USB)", UiIcon.NavSoftware, true);
            _btnNavSoftware.Click += delegate { ShowTab("software"); };

            _btnNavCloud = CreateNavTabButton("Google Drive Apps", UiIcon.NavCloud, false);
            _btnNavCloud.Click += delegate { ShowTab("cloud"); };

            _btnNavTools = CreateNavTabButton("System Tools & Tweaks", UiIcon.NavTools, false);
            _btnNavTools.Click += delegate { ShowTab("tools"); };

            _navPanel.Controls.Add(_btnNavTools);
            _navPanel.Controls.Add(_btnNavCloud);
            _navPanel.Controls.Add(_btnNavSoftware);

            // Build Main Panels
            InitializeSoftwarePanel();
            InitializeCloudPanel();
            InitializeToolsPanel();

            this.Controls.Add(_panelSoftware);
            this.Controls.Add(_panelCloud);
            this.Controls.Add(_panelTools);
            this.Controls.Add(_navPanel);
            this.Controls.Add(_topPanel);

            this.Load += delegate { AdjustSplitters(); };
            this.Shown += delegate { AdjustSplitters(); };
            this.Resize += delegate { AdjustSplitters(); };
        }

        private void AdjustSplitters()
        {
            try
            {
                int targetRight = 390;
                if (_splitContainer != null && _splitContainer.Width > 500)
                {
                    int maxDist = _splitContainer.Width - targetRight;
                    if (maxDist > 250)
                    {
                        _splitContainer.Panel1MinSize = 300;
                        _splitContainer.Panel2MinSize = 260;
                        _splitContainer.SplitterDistance = maxDist;
                    }
                }
                if (_cloudSplitContainer != null && _cloudSplitContainer.Width > 500)
                {
                    int maxDist = _cloudSplitContainer.Width - targetRight;
                    if (maxDist > 250)
                    {
                        _cloudSplitContainer.Panel1MinSize = 300;
                        _cloudSplitContainer.Panel2MinSize = 260;
                        _cloudSplitContainer.SplitterDistance = maxDist;
                    }
                }
                UpdateSoftwareDetailsLayout();
                UpdateCloudDetailsLayout();
            }
            catch { }
        }

        private void UpdateSoftwareDetailsLayout()
        {
            try
            {
                if (_detailsPanel == null) return;
                int pad = 16;
                int avail = Math.Max(180, _detailsPanel.ClientSize.Width - (pad * 2));
                int headW = Math.Max(100, _detailsPanel.ClientSize.Width - 72 - pad);

                if (_lblDetailsTitle != null) _lblDetailsTitle.Width = headW;
                if (_badgesPanel != null) _badgesPanel.Width = headW;
                if (_infoTable != null) _infoTable.Width = avail;
                if (_txtDetailsDesc != null) _txtDetailsDesc.Width = avail;
                if (_detailsActions != null) _detailsActions.Width = avail;
            }
            catch { }
        }

        private void UpdateCloudDetailsLayout()
        {
            try
            {
                if (_cloudDetailsPanel == null) return;
                int pad = 16;
                int avail = Math.Max(180, _cloudDetailsPanel.ClientSize.Width - (pad * 2));
                int headW = Math.Max(100, _cloudDetailsPanel.ClientSize.Width - 74 - pad);

                if (_lblCloudTitle != null) _lblCloudTitle.Width = headW;
                if (_cloudBadges != null) _cloudBadges.Width = headW;
                if (_cloudMetaTable != null) _cloudMetaTable.Width = avail;
                if (_txtCloudDesc != null) _txtCloudDesc.Width = avail;
                if (_txtCloudUrl != null) _txtCloudUrl.Width = avail;
                if (_btnCloudLaunchDirect != null) _btnCloudLaunchDirect.Width = avail;
            }
            catch { }
        }



        protected override bool ProcessCmdKey(ref Message msg, Keys keyData)
        {
            if (keyData == (Keys.Control | Keys.F))
            {
                if (_panelSoftware != null && _panelSoftware.Visible && _txtSearch != null)
                {
                    _txtSearch.Focus();
                    _txtSearch.SelectAll();
                    return true;
                }
                if (_panelCloud != null && _panelCloud.Visible && _txtCloudSearch != null)
                {
                    _txtCloudSearch.Focus();
                    _txtCloudSearch.SelectAll();
                    return true;
                }
            }
            return base.ProcessCmdKey(ref msg, keyData);
        }

        private Button CreateNavTabButton(string text, UiIcon icon, bool isActive)
        {
            Button btn = new Button();
            btn.UseMnemonic = false;
            btn.Text = "  " + text;
            btn.Image = UiIconHelper.GetIcon(icon, 16, isActive ? Color.White : ColTextSecondary);
            btn.TextImageRelation = TextImageRelation.ImageBeforeText;
            btn.ImageAlign = ContentAlignment.MiddleLeft;
            btn.Height = 34;
            btn.Width = 235;
            btn.Dock = DockStyle.Left;
            btn.FlatStyle = FlatStyle.Flat;
            btn.FlatAppearance.BorderSize = 0;
            btn.Font = new Font("Segoe UI", 9.5F, FontStyle.Bold);
            btn.Cursor = Cursors.Hand;
            btn.BackColor = isActive ? ColAccentCyan : Color.Transparent;
            btn.ForeColor = isActive ? Color.White : ColTextSecondary;
            return btn;
        }

        private void ShowTab(string tab)
        {
            _panelSoftware.Visible = (tab == "software");
            _panelCloud.Visible = (tab == "cloud");
            _panelTools.Visible = (tab == "tools");

            SetNavTabActive(_btnNavSoftware, UiIcon.NavSoftware, tab == "software");
            SetNavTabActive(_btnNavCloud, UiIcon.NavCloud, tab == "cloud");
            SetNavTabActive(_btnNavTools, UiIcon.NavTools, tab == "tools");

            AdjustSplitters();
        }

        private void SetNavTabActive(Button btn, UiIcon icon, bool active)
        {
            btn.BackColor = active ? ColAccentCyan : Color.Transparent;
            btn.ForeColor = active ? Color.White : ColTextSecondary;
            btn.Image = UiIconHelper.GetIcon(icon, 16, active ? Color.White : ColTextSecondary);
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
            _presetPanel.Height = 48;
            _presetPanel.BackColor = ColCardAlt;
            _presetPanel.Padding = new Padding(16, 7, 16, 7);
            _presetPanel.Paint += delegate(object s, PaintEventArgs pe) {
                using (Pen p = new Pen(ColBorder, 1))
                {
                    pe.Graphics.DrawLine(p, 0, _presetPanel.Height - 1, _presetPanel.Width, _presetPanel.Height - 1);
                }
            };

            _presetButtonsPanel = new FlowLayoutPanel();
            _presetButtonsPanel.Dock = DockStyle.Fill;
            _presetButtonsPanel.FlowDirection = FlowDirection.LeftToRight;
            _presetButtonsPanel.WrapContents = false;
            _presetButtonsPanel.BackColor = Color.Transparent;

            _btnPresetEssentials = CreatePillButton(" Essentials", UiIcon.Star, ColCard, ColAccentAmber, 120, 32);
            _btnPresetEssentials.Click += delegate { ApplyPreset("essential"); };

            _btnPresetRuntimes = CreatePillButton(" All Runtimes", UiIcon.Lightning, ColCard, ColAccentPurple, 125, 32);
            _btnPresetRuntimes.Click += delegate { ApplyPreset("runtime"); };

            _btnPresetGaming = CreatePillButton(" Gaming PC", UiIcon.Gamepad, ColCard, ColAccentBlue, 120, 32);
            _btnPresetGaming.Click += delegate { ApplyPreset("gaming"); };

            _btnPresetOffice = CreatePillButton(" Office Setup", UiIcon.Briefcase, ColCard, ColAccentGreen, 120, 32);
            _btnPresetOffice.Click += delegate { ApplyPreset("office"); };

            Label lblDivider = new Label();
            lblDivider.Text = "|";
            lblDivider.ForeColor = ColTextMuted;
            lblDivider.AutoSize = true;
            lblDivider.Margin = new Padding(6, 6, 6, 0);
            lblDivider.Font = new Font("Segoe UI", 11.0F);

            _btnSelectAll = CreatePillButton(" Select All", UiIcon.CheckAll, Color.FromArgb(24, 42, 77), ColAccentBlue, 110, 32);
            _btnSelectAll.Click += delegate { SetAllSelection(true); };

            _btnDeselectAll = CreatePillButton(" Clear", UiIcon.Clear, ColCard, ColTextSecondary, 85, 32);
            _btnDeselectAll.Click += delegate { SetAllSelection(false); };

            _presetButtonsPanel.Controls.Add(_btnPresetEssentials);
            _presetButtonsPanel.Controls.Add(_btnPresetRuntimes);
            _presetButtonsPanel.Controls.Add(_btnPresetGaming);
            _presetButtonsPanel.Controls.Add(_btnPresetOffice);
            _presetButtonsPanel.Controls.Add(lblDivider);
            _presetButtonsPanel.Controls.Add(_btnSelectAll);
            _presetButtonsPanel.Controls.Add(_btnDeselectAll);

            // Search Panel with modern badge hint
            _searchPanel = new Panel();
            _searchPanel.Dock = DockStyle.Right;
            _searchPanel.Width = 270;
            _searchPanel.BackColor = Color.Transparent;

            _lblSearch = new Label();
            _lblSearch.Font = UiIconHelper.CreateIconFont(11.0F);
            _lblSearch.Text = UiIconHelper.GetGlyph(UiIcon.Search);
            _lblSearch.AutoSize = true;
            _lblSearch.Location = new Point(4, 9);
            _lblSearch.ForeColor = ColTextSecondary;

            _txtSearch = new TextBox();
            _txtSearch.Width = 185;
            _txtSearch.Location = new Point(28, 6);
            _txtSearch.BackColor = ColBg;
            _txtSearch.ForeColor = ColTextPrimary;
            _txtSearch.BorderStyle = BorderStyle.FixedSingle;
            _txtSearch.Font = new Font("Segoe UI", 9.5F);
            _txtSearch.TextChanged += delegate { FilterApps(_txtSearch.Text); };

            _lblSearchHint = new Label();
            _lblSearchHint.Text = "Ctrl+F";
            _lblSearchHint.Font = new Font("Segoe UI", 7.5F, FontStyle.Bold);
            _lblSearchHint.ForeColor = ColTextMuted;
            _lblSearchHint.BackColor = ColCard;
            _lblSearchHint.BorderStyle = BorderStyle.FixedSingle;
            _lblSearchHint.AutoSize = false;
            _lblSearchHint.Size = new Size(46, 22);
            _lblSearchHint.TextAlign = ContentAlignment.MiddleCenter;
            _lblSearchHint.Location = new Point(218, 6);
            _lblSearchHint.Cursor = Cursors.Hand;
            _lblSearchHint.Click += delegate { _txtSearch.Focus(); _txtSearch.SelectAll(); };

            _searchPanel.Controls.Add(_lblSearch);
            _searchPanel.Controls.Add(_txtSearch);
            _searchPanel.Controls.Add(_lblSearchHint);

            _presetPanel.Controls.Add(_searchPanel);
            _presetPanel.Controls.Add(_presetButtonsPanel);
            _searchPanel.SendToBack();
            _presetButtonsPanel.BringToFront();

            // Bottom Action & Progress Bar
            _bottomPanel = new Panel();
            _bottomPanel.Dock = DockStyle.Bottom;
            _bottomPanel.Height = 94;
            _bottomPanel.BackColor = ColCard;
            _bottomPanel.Padding = new Padding(18, 8, 18, 8);
            _bottomPanel.Paint += delegate(object s, PaintEventArgs pe) {
                using (Pen p = new Pen(ColBorder, 1))
                {
                    pe.Graphics.DrawLine(p, 0, 0, _bottomPanel.Width, 0);
                }
            };

            _bottomLayout = new TableLayoutPanel();
            _bottomLayout.Dock = DockStyle.Fill;
            _bottomLayout.ColumnCount = 2;
            _bottomLayout.RowCount = 1;
            _bottomLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 74F));
            _bottomLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 26F));

            Panel bottomProgressContainer = new Panel();
            bottomProgressContainer.Dock = DockStyle.Fill;
            bottomProgressContainer.BackColor = Color.Transparent;

            _lblOverallStatus = new Label();
            _lblOverallStatus.Text = "Ready. Select software packages and click 'Start Installation'.";
            _lblOverallStatus.AutoSize = true;
            _lblOverallStatus.ForeColor = ColTextPrimary;
            _lblOverallStatus.Font = new Font("Segoe UI", 9.5F, FontStyle.Bold);
            _lblOverallStatus.Location = new Point(0, 3);

            _lblTimeEstimate = new Label();
            _lblTimeEstimate.Text = "Estimated time: ~2-5 min  •  Automated silent installation";
            _lblTimeEstimate.AutoSize = true;
            _lblTimeEstimate.ForeColor = ColTextSecondary;
            _lblTimeEstimate.Font = new Font("Segoe UI", 8.5F);
            _lblTimeEstimate.Location = new Point(0, 22);

            _pbOverall = new ProgressBar();
            _pbOverall.Height = 14;
            _pbOverall.Location = new Point(0, 42);
            _pbOverall.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;

            _lblCurrentStatus = new Label();
            _lblCurrentStatus.Text = "Idle";
            _lblCurrentStatus.AutoSize = true;
            _lblCurrentStatus.ForeColor = ColTextMuted;
            _lblCurrentStatus.Font = new Font("Segoe UI", 8.0F);
            _lblCurrentStatus.Location = new Point(0, 60);

            _pbCurrent = new ProgressBar();
            _pbCurrent.Height = 8;
            _pbCurrent.Location = new Point(0, 76);
            _pbCurrent.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;

            bottomProgressContainer.Controls.Add(_lblOverallStatus);
            bottomProgressContainer.Controls.Add(_lblTimeEstimate);
            bottomProgressContainer.Controls.Add(_pbOverall);
            bottomProgressContainer.Controls.Add(_lblCurrentStatus);
            bottomProgressContainer.Controls.Add(_pbCurrent);

            Panel bottomButtonContainer = new Panel();
            bottomButtonContainer.Dock = DockStyle.Fill;
            bottomButtonContainer.BackColor = Color.Transparent;
            bottomButtonContainer.Padding = new Padding(12, 12, 0, 12);

            _btnAction = new Button();
            _btnAction.Text = "  Start Installation";
            _btnAction.Image = UiIconHelper.GetIcon(UiIcon.Play, 18, Color.White);
            _btnAction.TextImageRelation = TextImageRelation.ImageBeforeText;
            _btnAction.ImageAlign = ContentAlignment.MiddleCenter;
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

            // Split Container (Grid on Left, Details & Terminal on Right)
            _splitContainer = new SplitContainer();
            _splitContainer.Dock = DockStyle.Fill;
            _splitContainer.BackColor = ColBorder;
            _splitContainer.SplitterWidth = 3;
            _splitContainer.FixedPanel = FixedPanel.Panel2;

            // Software Grid Left
            _gridApps = new DataGridView();
            _gridApps.Dock = DockStyle.Fill;
            _gridApps.BackgroundColor = ColBg;
            _gridApps.BorderStyle = BorderStyle.None;
            _gridApps.CellBorderStyle = DataGridViewCellBorderStyle.SingleHorizontal;
            _gridApps.GridColor = Color.FromArgb(20, 32, 54);
            _gridApps.EnableHeadersVisualStyles = false;
            _gridApps.RowHeadersVisible = false;
            _gridApps.AllowUserToAddRows = false;
            _gridApps.AllowUserToDeleteRows = false;
            _gridApps.AllowUserToResizeRows = false;
            _gridApps.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            _gridApps.MultiSelect = false;
            _gridApps.RowTemplate.Height = 42;

            _gridApps.ColumnHeadersHeight = 36;
            _gridApps.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.DisableResizing;
            _gridApps.ColumnHeadersBorderStyle = DataGridViewHeaderBorderStyle.None;
            _gridApps.ColumnHeadersDefaultCellStyle.BackColor = ColCardAlt;
            _gridApps.ColumnHeadersDefaultCellStyle.ForeColor = ColTextSecondary;
            _gridApps.ColumnHeadersDefaultCellStyle.Font = new Font("Segoe UI", 9.0F, FontStyle.Bold);
            _gridApps.ColumnHeadersDefaultCellStyle.Padding = new Padding(6, 4, 4, 4);

            _gridApps.DefaultCellStyle.BackColor = ColBg;
            _gridApps.DefaultCellStyle.ForeColor = ColTextPrimary;
            _gridApps.DefaultCellStyle.SelectionBackColor = ColRowSelect;
            _gridApps.DefaultCellStyle.SelectionForeColor = Color.White;
            _gridApps.DefaultCellStyle.Font = new Font("Segoe UI", 9.5F);

            _gridApps.AlternatingRowsDefaultCellStyle.BackColor = ColRowAlt;
            _gridApps.AlternatingRowsDefaultCellStyle.ForeColor = ColTextPrimary;

            EnableDoubleBuffering(_gridApps);

            // Columns
            DataGridViewCheckBoxColumn colCheck = new DataGridViewCheckBoxColumn();
            colCheck.Width = 32;
            colCheck.HeaderText = "";
            colCheck.Name = "ColCheck";
            colCheck.Resizable = DataGridViewTriState.False;
            _gridApps.Columns.Add(colCheck);

            DataGridViewImageColumn colIcon = new DataGridViewImageColumn();
            colIcon.Width = 44;
            colIcon.HeaderText = "";
            colIcon.Name = "ColIcon";
            colIcon.ImageLayout = DataGridViewImageCellLayout.Normal;
            colIcon.DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;
            colIcon.Resizable = DataGridViewTriState.False;
            _gridApps.Columns.Add(colIcon);

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
            colCat.Width = 135;
            colCat.ReadOnly = true;
            colCat.DefaultCellStyle.ForeColor = ColTextSecondary;
            _gridApps.Columns.Add(colCat);

            DataGridViewTextBoxColumn colSize = new DataGridViewTextBoxColumn();
            colSize.HeaderText = "Size";
            colSize.Name = "ColSize";
            colSize.Width = 70;
            colSize.ReadOnly = true;
            colSize.DefaultCellStyle.ForeColor = ColTextSecondary;
            _gridApps.Columns.Add(colSize);

            DataGridViewTextBoxColumn colSource = new DataGridViewTextBoxColumn();
            colSource.HeaderText = "Source";
            colSource.Name = "ColSource";
            colSource.Width = 110;
            colSource.ReadOnly = true;
            _gridApps.Columns.Add(colSource);

            DataGridViewTextBoxColumn colStatus = new DataGridViewTextBoxColumn();
            colStatus.HeaderText = "Status";
            colStatus.Name = "ColStatus";
            colStatus.Width = 125;
            colStatus.ReadOnly = true;
            _gridApps.Columns.Add(colStatus);

            DataGridViewTextBoxColumn colArrow = new DataGridViewTextBoxColumn();
            colArrow.HeaderText = "";
            colArrow.Name = "ColArrow";
            colArrow.Width = 24;
            colArrow.ReadOnly = true;
            colArrow.DefaultCellStyle.ForeColor = ColTextMuted;
            colArrow.DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;
            _gridApps.Columns.Add(colArrow);

            _gridApps.CellClick += OnGridCellClick;
            _gridApps.CellContentClick += OnGridCellContentClick;
            _gridApps.CellValueChanged += OnGridCellValueChanged;
            _gridApps.CurrentCellDirtyStateChanged += OnGridCurrentCellDirtyStateChanged;
            _gridApps.SelectionChanged += OnGridSelectionChanged;

            _splitContainer.Panel1.Controls.Add(_gridApps);

            // Right Panel (Details Card & Live Terminal)
            _rightPanel = new Panel();
            _rightPanel.Dock = DockStyle.Fill;
            _rightPanel.BackColor = ColCard;

            // Details Card Top
            _detailsPanel = new Panel();
            _detailsPanel.Dock = DockStyle.Top;
            _detailsPanel.Height = 254;
            _detailsPanel.BackColor = ColCard;
            _detailsPanel.Padding = new Padding(16, 12, 16, 10);
            _detailsPanel.Paint += delegate(object s, PaintEventArgs pe) {
                using (Pen p = new Pen(ColBorder, 1))
                {
                    pe.Graphics.DrawLine(p, 0, _detailsPanel.Height - 1, _detailsPanel.Width, _detailsPanel.Height - 1);
                }
            };

            _picDetailsIcon = new PictureBox();
            _picDetailsIcon.Size = new Size(48, 48);
            _picDetailsIcon.Location = new Point(16, 12);
            _picDetailsIcon.SizeMode = PictureBoxSizeMode.Zoom;
            _picDetailsIcon.BackColor = Color.Transparent;

            _lblDetailsTitle = new Label();
            _lblDetailsTitle.Text = "Software Information";
            _lblDetailsTitle.Font = new Font("Segoe UI", 11.5F, FontStyle.Bold);
            _lblDetailsTitle.ForeColor = ColTextPrimary;
            _lblDetailsTitle.Location = new Point(72, 12);
            _lblDetailsTitle.Size = new Size(300, 22);
            _lblDetailsTitle.AutoEllipsis = true;

            _lblDetailsCategoryBadge = CreateBadgeLabel("Category", ColAccentCyan);
            _lblDetailsTypeBadge = CreateBadgeLabel("Type", ColAccentGreen);

            _badgesPanel = new FlowLayoutPanel();
            _badgesPanel.Location = new Point(72, 36);
            _badgesPanel.Size = new Size(300, 26);
            _badgesPanel.FlowDirection = FlowDirection.LeftToRight;
            _badgesPanel.WrapContents = false;
            _badgesPanel.BackColor = Color.Transparent;
            _badgesPanel.Controls.Add(_lblDetailsCategoryBadge);
            _badgesPanel.Controls.Add(_lblDetailsTypeBadge);

            // Info Key-Value Table (2 columns, 4 rows for clean responsive fit)
            _infoTable = new TableLayoutPanel();
            _infoTable.Location = new Point(16, 68);
            _infoTable.Size = new Size(350, 80);
            _infoTable.ColumnCount = 2;
            _infoTable.RowCount = 4;
            _infoTable.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 65F));
            _infoTable.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));

            _infoTable.Controls.Add(CreateMutedLabel("Size:"), 0, 0);
            _lblValSize = CreateBoldValueLabel("-");
            _lblValSize.Dock = DockStyle.Fill;
            _lblValSize.AutoEllipsis = true;
            _infoTable.Controls.Add(_lblValSize, 1, 0);

            _infoTable.Controls.Add(CreateMutedLabel("Status:"), 0, 1);
            _lblValStatus = CreateBoldValueLabel("-");
            _lblValStatus.Dock = DockStyle.Fill;
            _lblValStatus.AutoEllipsis = true;
            _infoTable.Controls.Add(_lblValStatus, 1, 1);

            _infoTable.Controls.Add(CreateMutedLabel("Source:"), 0, 2);
            _lblValSource = CreateBoldValueLabel("-");
            _lblValSource.Dock = DockStyle.Fill;
            _lblValSource.AutoEllipsis = true;
            _infoTable.Controls.Add(_lblValSource, 1, 2);

            _infoTable.Controls.Add(CreateMutedLabel("Switch:"), 0, 3);
            _lblValSwitch = CreateBoldValueLabel("-");
            _lblValSwitch.Font = new Font("Consolas", 8.5F);
            _lblValSwitch.Dock = DockStyle.Fill;
            _lblValSwitch.AutoEllipsis = true;
            _infoTable.Controls.Add(_lblValSwitch, 1, 3);

            _txtDetailsDesc = new TextBox();
            _txtDetailsDesc.Location = new Point(16, 154);
            _txtDetailsDesc.Size = new Size(350, 56);
            _txtDetailsDesc.Multiline = true;
            _txtDetailsDesc.ReadOnly = true;
            _txtDetailsDesc.BackColor = ColBg;
            _txtDetailsDesc.ForeColor = ColTextPrimary;
            _txtDetailsDesc.BorderStyle = BorderStyle.FixedSingle;
            _txtDetailsDesc.Font = new Font("Segoe UI", 8.5F);

            // Quick Actions Links
            _detailsActions = new FlowLayoutPanel();
            _detailsActions.Location = new Point(16, 216);
            _detailsActions.Size = new Size(350, 34);
            _detailsActions.FlowDirection = FlowDirection.LeftToRight;
            _detailsActions.WrapContents = true;

            _btnDetailsOpenUrl = CreatePillButton(" Homepage / Info", UiIcon.ExternalLink, ColCardAlt, ColAccentBlue, 160, 30);
            _btnDetailsOpenUrl.Font = new Font("Segoe UI", 8.5F, FontStyle.Bold);
            _btnDetailsOpenUrl.Click += delegate { OpenAppHomepage(_selectedApp); };

            _btnDetailsOpenFolder = CreatePillButton(" Open USB Location", UiIcon.OpenFolder, ColCardAlt, ColAccentGreen, 170, 30);
            _btnDetailsOpenFolder.Font = new Font("Segoe UI", 8.5F, FontStyle.Bold);
            _btnDetailsOpenFolder.Click += delegate { OpenContainingFolder(_selectedApp); };

            _detailsActions.Controls.Add(_btnDetailsOpenUrl);
            _detailsActions.Controls.Add(_btnDetailsOpenFolder);

            _detailsPanel.Controls.Add(_picDetailsIcon);
            _detailsPanel.Controls.Add(_lblDetailsTitle);
            _detailsPanel.Controls.Add(_badgesPanel);
            _detailsPanel.Controls.Add(_infoTable);
            _detailsPanel.Controls.Add(_txtDetailsDesc);
            _detailsPanel.Controls.Add(_detailsActions);

            _detailsPanel.Resize += delegate { UpdateSoftwareDetailsLayout(); };
            UpdateSoftwareDetailsLayout();

            // Live Terminal Header
            _logHeaderPanel = new Panel();
            _logHeaderPanel.Dock = DockStyle.Top;
            _logHeaderPanel.Height = 32;
            _logHeaderPanel.BackColor = ColCardAlt;
            _logHeaderPanel.Padding = new Padding(12, 5, 12, 5);

            _lblLogIndicator = new Label();
            _lblLogIndicator.Text = "●";
            _lblLogIndicator.ForeColor = ColAccentGreen;
            _lblLogIndicator.Location = new Point(12, 7);
            _lblLogIndicator.AutoSize = true;
            _lblLogIndicator.Font = new Font("Segoe UI", 9.5F, FontStyle.Bold);

            _lblLogTitle = new Label();
            _lblLogTitle.Text = "INSTALLATION CONSOLE";
            _lblLogTitle.Font = new Font("Segoe UI", 8.5F, FontStyle.Bold);
            _lblLogTitle.ForeColor = ColTextSecondary;
            _lblLogTitle.AutoSize = true;
            _lblLogTitle.Location = new Point(28, 8);

            _btnClearLog = CreatePillButton(" Clear", UiIcon.Trash, ColCard, ColTextSecondary, 80, 24);
            _btnClearLog.Font = new Font("Segoe UI", 8.0F);
            _btnClearLog.Dock = DockStyle.Right;
            _btnClearLog.Click += delegate { _rtbLog.Clear(); };

            _logHeaderPanel.Controls.Add(_lblLogIndicator);
            _logHeaderPanel.Controls.Add(_lblLogTitle);
            _logHeaderPanel.Controls.Add(_btnClearLog);

            // Terminal RichTextBox
            _rtbLog = new RichTextBox();
            _rtbLog.Dock = DockStyle.Fill;
            _rtbLog.BackColor = Color.FromArgb(6, 10, 20);
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

        private Label CreateBadgeLabel(string text, Color foreColor)
        {
            Label lbl = new Label();
            lbl.Text = text;
            lbl.Font = new Font("Segoe UI", 8.0F, FontStyle.Bold);
            lbl.ForeColor = foreColor;
            lbl.BackColor = Color.FromArgb(24, 38, 66);
            lbl.BorderStyle = BorderStyle.FixedSingle;
            lbl.AutoSize = true;
            lbl.Padding = new Padding(3, 1, 3, 1);
            return lbl;
        }

        private Label CreateMutedLabel(string text)
        {
            Label lbl = new Label();
            lbl.Text = text;
            lbl.ForeColor = ColTextMuted;
            lbl.Font = new Font("Segoe UI", 8.5F);
            lbl.AutoSize = true;
            return lbl;
        }

        private Label CreateBoldValueLabel(string text)
        {
            Label lbl = new Label();
            lbl.Text = text;
            lbl.ForeColor = ColTextPrimary;
            lbl.Font = new Font("Segoe UI", 8.5F, FontStyle.Bold);
            lbl.AutoSize = true;
            return lbl;
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
            _cloudTopBar.Padding = new Padding(16, 7, 16, 7);
            _cloudTopBar.Paint += delegate(object s, PaintEventArgs pe) {
                using (Pen p = new Pen(ColBorder, 1))
                {
                    pe.Graphics.DrawLine(p, 0, _cloudTopBar.Height - 1, _cloudTopBar.Width, _cloudTopBar.Height - 1);
                }
            };

            _btnCloudOpen = CreatePillButton(" Open in Browser", UiIcon.ExternalLink, ColAccentGreen, Color.White, 160, 32);
            _btnCloudOpen.Click += delegate { LaunchSelectedCloudApp(); };

            _btnCloudEditConfig = CreatePillButton(" Edit Links", UiIcon.Edit, ColCard, ColTextPrimary, 120, 32);
            _btnCloudEditConfig.Click += delegate { ConfigManager.OpenCloudConfigInEditor(); };

            _btnCloudReload = CreatePillButton(" Refresh", UiIcon.Reload, ColCard, ColTextPrimary, 105, 32);
            _btnCloudReload.Click += delegate { LoadCloudAppCatalog(); };

            Button btnCloudAdd = CreatePillButton(" Add App", UiIcon.Add, Color.FromArgb(16, 185, 129), Color.White, 110, 32);
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

            Button btnCloudDelete = CreatePillButton(" Delete", UiIcon.Trash, Color.FromArgb(185, 28, 28), Color.White, 100, 32);
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

            Button btnCloudPush = CreatePillButton(" Push to GitHub", UiIcon.CloudUpload, Color.FromArgb(79, 70, 229), Color.White, 155, 32);
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

            Panel searchWrap = new Panel();
            searchWrap.Dock = DockStyle.Right;
            searchWrap.Width = 240;

            Label lblCSearch = new Label();
            lblCSearch.Font = UiIconHelper.CreateIconFont(11.0F);
            lblCSearch.Text = UiIconHelper.GetGlyph(UiIcon.Search);
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

            _cloudTopBar.Controls.Add(searchWrap);
            _cloudTopBar.Controls.Add(leftFlow);
            searchWrap.SendToBack();
            leftFlow.BringToFront();

            // Split Container
            _cloudSplitContainer = new SplitContainer();
            _cloudSplitContainer.Dock = DockStyle.Fill;
            _cloudSplitContainer.BackColor = ColBorder;
            _cloudSplitContainer.SplitterWidth = 3;
            _cloudSplitContainer.FixedPanel = FixedPanel.Panel2;

            // Grid Left
            _gridCloudApps = new DataGridView();
            _gridCloudApps.Dock = DockStyle.Fill;
            _gridCloudApps.BackgroundColor = ColBg;
            _gridCloudApps.BorderStyle = BorderStyle.None;
            _gridCloudApps.CellBorderStyle = DataGridViewCellBorderStyle.SingleHorizontal;
            _gridCloudApps.GridColor = Color.FromArgb(20, 32, 54);
            _gridCloudApps.EnableHeadersVisualStyles = false;
            _gridCloudApps.RowHeadersVisible = false;
            _gridCloudApps.AllowUserToAddRows = false;
            _gridCloudApps.AllowUserToDeleteRows = false;
            _gridCloudApps.AllowUserToResizeRows = false;
            _gridCloudApps.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            _gridCloudApps.MultiSelect = false;
            _gridCloudApps.RowTemplate.Height = 42;

            _gridCloudApps.ColumnHeadersHeight = 36;
            _gridCloudApps.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.DisableResizing;
            _gridCloudApps.ColumnHeadersBorderStyle = DataGridViewHeaderBorderStyle.None;
            _gridCloudApps.ColumnHeadersDefaultCellStyle.BackColor = ColCardAlt;
            _gridCloudApps.ColumnHeadersDefaultCellStyle.ForeColor = ColTextSecondary;
            _gridCloudApps.ColumnHeadersDefaultCellStyle.Font = new Font("Segoe UI", 9.0F, FontStyle.Bold);
            _gridCloudApps.ColumnHeadersDefaultCellStyle.Padding = new Padding(6, 4, 4, 4);

            _gridCloudApps.DefaultCellStyle.BackColor = ColBg;
            _gridCloudApps.DefaultCellStyle.ForeColor = ColTextPrimary;
            _gridCloudApps.DefaultCellStyle.SelectionBackColor = ColRowSelect;
            _gridCloudApps.DefaultCellStyle.SelectionForeColor = Color.White;
            _gridCloudApps.DefaultCellStyle.Font = new Font("Segoe UI", 9.5F);

            _gridCloudApps.AlternatingRowsDefaultCellStyle.BackColor = ColRowAlt;
            _gridCloudApps.AlternatingRowsDefaultCellStyle.ForeColor = ColTextPrimary;

            EnableDoubleBuffering(_gridCloudApps);

            DataGridViewImageColumn cIcon = new DataGridViewImageColumn();
            cIcon.Width = 44;
            cIcon.HeaderText = "";
            cIcon.Name = "CIcon";
            cIcon.ImageLayout = DataGridViewImageCellLayout.Normal;
            cIcon.DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;
            cIcon.Resizable = DataGridViewTriState.False;
            _gridCloudApps.Columns.Add(cIcon);

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
            cCat.Width = 135;
            cCat.ReadOnly = true;
            cCat.DefaultCellStyle.ForeColor = ColTextSecondary;
            _gridCloudApps.Columns.Add(cCat);

            DataGridViewTextBoxColumn cVer = new DataGridViewTextBoxColumn();
            cVer.HeaderText = "Version";
            cVer.Name = "CVer";
            cVer.Width = 85;
            cVer.ReadOnly = true;
            cVer.DefaultCellStyle.ForeColor = ColTextSecondary;
            _gridCloudApps.Columns.Add(cVer);

            DataGridViewTextBoxColumn cSize = new DataGridViewTextBoxColumn();
            cSize.HeaderText = "Size";
            cSize.Name = "CSize";
            cSize.Width = 70;
            cSize.ReadOnly = true;
            cSize.DefaultCellStyle.ForeColor = ColTextSecondary;
            _gridCloudApps.Columns.Add(cSize);

            DataGridViewTextBoxColumn cArrow = new DataGridViewTextBoxColumn();
            cArrow.HeaderText = "";
            cArrow.Name = "CArrow";
            cArrow.Width = 24;
            cArrow.ReadOnly = true;
            cArrow.DefaultCellStyle.ForeColor = ColTextMuted;
            cArrow.DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;
            _gridCloudApps.Columns.Add(cArrow);

            _gridCloudApps.SelectionChanged += OnGridCloudSelectionChanged;
            _gridCloudApps.CellDoubleClick += delegate { LaunchSelectedCloudApp(); };

            _cloudSplitContainer.Panel1.Controls.Add(_gridCloudApps);

            // Right Details
            _cloudDetailsPanel = new Panel();
            _cloudDetailsPanel.Dock = DockStyle.Fill;
            _cloudDetailsPanel.BackColor = ColCard;
            _cloudDetailsPanel.Padding = new Padding(18, 12, 18, 10);

            _picCloudIcon = new PictureBox();
            _picCloudIcon.Size = new Size(48, 48);
            _picCloudIcon.Location = new Point(16, 12);
            _picCloudIcon.SizeMode = PictureBoxSizeMode.Zoom;
            _picCloudIcon.BackColor = Color.Transparent;

            _lblCloudTitle = new Label();
            _lblCloudTitle.Text = "Application Package Details";
            _lblCloudTitle.Font = new Font("Segoe UI", 11.5F, FontStyle.Bold);
            _lblCloudTitle.ForeColor = ColTextPrimary;
            _lblCloudTitle.Location = new Point(74, 12);
            _lblCloudTitle.Size = new Size(300, 22);
            _lblCloudTitle.AutoEllipsis = true;

            _lblCloudCategoryBadge = CreateBadgeLabel("Category", ColAccentCyan);
            _lblCloudVersionBadge = CreateBadgeLabel("Version", ColAccentGreen);

            _cloudBadges = new FlowLayoutPanel();
            _cloudBadges.Location = new Point(74, 36);
            _cloudBadges.Size = new Size(300, 26);
            _cloudBadges.FlowDirection = FlowDirection.LeftToRight;
            _cloudBadges.WrapContents = false;
            _cloudBadges.BackColor = Color.Transparent;
            _cloudBadges.Controls.Add(_lblCloudCategoryBadge);
            _cloudBadges.Controls.Add(_lblCloudVersionBadge);

            _cloudMetaTable = new TableLayoutPanel();
            _cloudMetaTable.Location = new Point(16, 68);
            _cloudMetaTable.Size = new Size(350, 60);
            _cloudMetaTable.ColumnCount = 2;
            _cloudMetaTable.RowCount = 3;
            _cloudMetaTable.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 75F));
            _cloudMetaTable.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));

            _cloudMetaTable.Controls.Add(CreateMutedLabel("Category:"), 0, 0);
            _lblCloudValCat = CreateBoldValueLabel("-");
            _lblCloudValCat.Dock = DockStyle.Fill;
            _lblCloudValCat.AutoEllipsis = true;
            _cloudMetaTable.Controls.Add(_lblCloudValCat, 1, 0);

            _cloudMetaTable.Controls.Add(CreateMutedLabel("Version:"), 0, 1);
            _lblCloudValVer = CreateBoldValueLabel("-");
            _lblCloudValVer.Dock = DockStyle.Fill;
            _lblCloudValVer.AutoEllipsis = true;
            _cloudMetaTable.Controls.Add(_lblCloudValVer, 1, 1);

            _cloudMetaTable.Controls.Add(CreateMutedLabel("Est. Size:"), 0, 2);
            _lblCloudValSize = CreateBoldValueLabel("-");
            _lblCloudValSize.Dock = DockStyle.Fill;
            _lblCloudValSize.AutoEllipsis = true;
            _cloudMetaTable.Controls.Add(_lblCloudValSize, 1, 2);

            Label lblDescHeader = new Label();
            lblDescHeader.Text = "Description & Technician Notes:";
            lblDescHeader.ForeColor = ColTextSecondary;
            lblDescHeader.Font = new Font("Segoe UI", 8.5F, FontStyle.Bold);
            lblDescHeader.Location = new Point(16, 134);
            lblDescHeader.AutoSize = true;

            _txtCloudDesc = new TextBox();
            _txtCloudDesc.Location = new Point(16, 154);
            _txtCloudDesc.Size = new Size(350, 75);
            _txtCloudDesc.Multiline = true;
            _txtCloudDesc.ReadOnly = true;
            _txtCloudDesc.BackColor = ColBg;
            _txtCloudDesc.ForeColor = ColTextPrimary;
            _txtCloudDesc.BorderStyle = BorderStyle.FixedSingle;
            _txtCloudDesc.Font = new Font("Segoe UI", 8.5F);

            Label lblUrlHeader = new Label();
            lblUrlHeader.Text = "Google Drive Target Link:";
            lblUrlHeader.ForeColor = ColTextSecondary;
            lblUrlHeader.Font = new Font("Segoe UI", 8.5F, FontStyle.Bold);
            lblUrlHeader.Location = new Point(16, 235);
            lblUrlHeader.AutoSize = true;

            _txtCloudUrl = new TextBox();
            _txtCloudUrl.Location = new Point(16, 255);
            _txtCloudUrl.Size = new Size(350, 24);
            _txtCloudUrl.ReadOnly = true;
            _txtCloudUrl.BackColor = ColBg;
            _txtCloudUrl.ForeColor = ColAccentBlue;
            _txtCloudUrl.BorderStyle = BorderStyle.FixedSingle;
            _txtCloudUrl.Font = new Font("Segoe UI", 9.0F);

            _btnCloudLaunchDirect = CreatePillButton(" Open Google Drive Download Page", UiIcon.ExternalLink, ColAccentGreen, Color.White, 350, 38);
            _btnCloudLaunchDirect.Location = new Point(16, 287);
            _btnCloudLaunchDirect.Size = new Size(350, 38);
            _btnCloudLaunchDirect.Font = new Font("Segoe UI", 9.5F, FontStyle.Bold);
            _btnCloudLaunchDirect.Click += delegate { LaunchSelectedCloudApp(); };

            _cloudDetailsPanel.Controls.Add(_picCloudIcon);
            _cloudDetailsPanel.Controls.Add(_lblCloudTitle);
            _cloudDetailsPanel.Controls.Add(_cloudBadges);
            _cloudDetailsPanel.Controls.Add(_cloudMetaTable);
            _cloudDetailsPanel.Controls.Add(lblDescHeader);
            _cloudDetailsPanel.Controls.Add(_txtCloudDesc);
            _cloudDetailsPanel.Controls.Add(lblUrlHeader);
            _cloudDetailsPanel.Controls.Add(_txtCloudUrl);
            _cloudDetailsPanel.Controls.Add(_btnCloudLaunchDirect);

            _cloudDetailsPanel.Resize += delegate { UpdateCloudDetailsLayout(); };
            UpdateCloudDetailsLayout();

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
            toolsBanner.Height = 48;
            toolsBanner.BackColor = ColCardAlt;
            toolsBanner.Padding = new Padding(18, 12, 18, 10);
            toolsBanner.Paint += delegate(object s, PaintEventArgs pe) {
                using (Pen p = new Pen(ColBorder, 1))
                {
                    pe.Graphics.DrawLine(p, 0, toolsBanner.Height - 1, toolsBanner.Width, toolsBanner.Height - 1);
                }
            };

            PictureBox picToolsBanner = new PictureBox();
            picToolsBanner.Size = new Size(18, 18);
            picToolsBanner.Location = new Point(16, 14);
            picToolsBanner.Image = UiIconHelper.GetIcon(UiIcon.Wrench, 16, ColAccentBlue);
            picToolsBanner.BackColor = Color.Transparent;

            Label lblToolsBanner = new Label();
            lblToolsBanner.Text = "Windows System Shortcuts, Optimization, and Configuration Utilities";
            lblToolsBanner.Font = new Font("Segoe UI", 10.0F, FontStyle.Bold);
            lblToolsBanner.ForeColor = ColAccentBlue;
            lblToolsBanner.AutoSize = true;
            lblToolsBanner.Location = new Point(40, 13);
            lblToolsBanner.UseMnemonic = false;

            toolsBanner.Controls.Add(picToolsBanner);
            toolsBanner.Controls.Add(lblToolsBanner);

            // Output Terminal at bottom of Tools Tab
            Panel toolsLogWrap = new Panel();
            toolsLogWrap.Dock = DockStyle.Bottom;
            toolsLogWrap.Height = 170;
            toolsLogWrap.BackColor = ColCard;
            toolsLogWrap.Padding = new Padding(14, 6, 14, 8);
            toolsLogWrap.Paint += delegate(object s, PaintEventArgs pe) {
                using (Pen p = new Pen(ColBorder, 1))
                {
                    pe.Graphics.DrawLine(p, 0, 0, toolsLogWrap.Width, 0);
                }
            };

            Label lblToolsLogTitle = new Label();
            lblToolsLogTitle.Text = "COMMAND EXECUTION OUTPUT:";
            lblToolsLogTitle.Font = new Font("Segoe UI", 8.5F, FontStyle.Bold);
            lblToolsLogTitle.ForeColor = ColTextSecondary;
            lblToolsLogTitle.Location = new Point(14, 6);
            lblToolsLogTitle.AutoSize = true;

            _rtbToolsLog = new RichTextBox();
            _rtbToolsLog.Location = new Point(14, 26);
            _rtbToolsLog.Width = toolsLogWrap.Width - 28;
            _rtbToolsLog.Height = 135;
            _rtbToolsLog.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            _rtbToolsLog.BackColor = Color.FromArgb(6, 10, 20);
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
            // SECTION 1: PERFORMANCE & 1-CLICK TWEAKS
            // -------------------------------------------------------------
            _toolsFlowPanel.Controls.Add(CreateSectionHeader("PERFORMANCE & 1-CLICK WINDOWS TWEAKS", UiIcon.Speed, ColAccentAmber));

            _toolsFlowPanel.Controls.Add(CreateToolCard(
                "Ultimate Performance Plan",
                "Enables Windows Ultimate Performance plan to prevent CPU throttling and maximize FPS.",
                "Activate Ultimate Plan",
                UiIcon.Lightning,
                delegate {
                    LogToolMessage("Activating Ultimate Performance Power Plan...");
                    string res = SystemToolsManager.EnableUltimatePerformance();
                    LogToolMessage(res);
                },
                ColAccentAmber
            ));

            _toolsFlowPanel.Controls.Add(CreateToolCard(
                "Show Extensions & Hidden",
                "Unhides file extensions (.exe, .zip, .iso) and hidden files in File Explorer.",
                "Show Extensions & Hidden",
                UiIcon.Settings,
                delegate {
                    LogToolMessage("Updating Explorer folder view settings...");
                    string res = SystemToolsManager.ToggleShowFileExtensions();
                    LogToolMessage(res);
                },
                ColAccentAmber
            ));

            _toolsFlowPanel.Controls.Add(CreateToolCard(
                "Disable Hibernation (Save 8-16 GB)",
                "Deletes hiberfil.sys and disables hibernation to immediately reclaim SSD storage.",
                "Disable Hibernation",
                UiIcon.Power,
                delegate {
                    LogToolMessage("Disabling Windows Hibernation...");
                    string res = SystemToolsManager.DisableHibernation();
                    LogToolMessage(res);
                },
                ColAccentAmber
            ));

            _toolsFlowPanel.Controls.Add(CreateToolCard(
                "1-Click Temp & Junk Cleaner",
                "Safely cleans %temp%, Windows Temp, and Prefetch junk cache to free up disk space.",
                "Clean Temp Files",
                UiIcon.Clean,
                delegate {
                    LogToolMessage("Cleaning temporary and cache junk files...");
                    string res = SystemToolsManager.CleanJunkAndTempFiles();
                    LogToolMessage(res);
                },
                ColAccentAmber
            ));

            Panel lastSec1Card = CreateToolCard(
                "System File Repair (SFC & DISM)",
                "Scans and repairs corrupted Windows system files and component store.",
                "Run SFC & DISM Scan",
                UiIcon.Shield,
                delegate {
                    LogToolMessage("Launching elevated System File Checker & DISM repair...");
                    SystemToolsManager.RunSystemFileCheck();
                },
                ColAccentAmber
            );
            _toolsFlowPanel.Controls.Add(lastSec1Card);
            _toolsFlowPanel.SetFlowBreak(lastSec1Card, true);

            // -------------------------------------------------------------
            // SECTION 2: HARDWARE & DIAGNOSTICS
            // -------------------------------------------------------------
            _toolsFlowPanel.Controls.Add(CreateSectionHeader("HARDWARE, DRIVERS & DIAGNOSTICS", UiIcon.Chip, ColAccentBlue));

            _toolsFlowPanel.Controls.Add(CreateToolCard(
                "Device Manager",
                "Check hardware devices, installed components, and missing drivers.",
                "Open devmgmt.msc",
                UiIcon.Settings,
                delegate {
                    LogToolMessage("Launching Windows Device Manager...");
                    SystemToolsManager.OpenDeviceManager();
                },
                ColAccentBlue
            ));

            _toolsFlowPanel.Controls.Add(CreateToolCard(
                "IObit Driver Booster",
                "Extract and run Driver Booster to automatically scan & install missing hardware drivers.",
                "Launch Driver Booster",
                UiIcon.Lightning,
                delegate {
                    LogToolMessage("Checking IObit Driver Booster Portable...");
                    string res = SystemToolsManager.LaunchOrDeployDriverBooster();
                    LogToolMessage(res);
                },
                ColAccentBlue
            ));

            _toolsFlowPanel.Controls.Add(CreateToolCard(
                "CrystalDiskInfo (Portable)",
                "Inspect SSD/HDD health, remaining life percentage, temperature, and SMART status.",
                "Launch CrystalDiskInfo",
                UiIcon.HardDrive,
                delegate {
                    LogToolMessage("Checking CrystalDiskInfo Portable...");
                    string res = SystemToolsManager.LaunchCrystalDiskInfo();
                    LogToolMessage(res);
                },
                ColAccentBlue
            ));

            Panel lastSec2Card = CreateToolCard(
                "CPU-Z (Portable)",
                "View detailed CPU clock speeds, Motherboard model, and Dual-Channel RAM specs.",
                "Launch CPU-Z",
                UiIcon.Chip,
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
            // SECTION 3: WINDOWS ADMINISTRATION
            // -------------------------------------------------------------
            _toolsFlowPanel.Controls.Add(CreateSectionHeader("WINDOWS SYSTEM ADMINISTRATION", UiIcon.Wrench, ColAccentGreen));

            _toolsFlowPanel.Controls.Add(CreateToolCard(
                "Disk Management (diskmgmt.msc)",
                "Partition drives, initialize new SSD/HDD, shrink/extend volumes, and create Drive D:.",
                "Open diskmgmt.msc",
                UiIcon.HardDrive,
                delegate {
                    LogToolMessage("Opening Windows Disk Management console...");
                    SystemToolsManager.OpenDiskManagement();
                },
                ColAccentGreen
            ));

            _toolsFlowPanel.Controls.Add(CreateToolCard(
                "System Properties (sysdm.cpl)",
                "Rename PC, change Workgroup, and configure Pagefile / Virtual Memory.",
                "Open sysdm.cpl",
                UiIcon.Document,
                delegate {
                    LogToolMessage("Opening System Properties (Advanced)...");
                    SystemToolsManager.OpenSystemPropertiesAdvanced();
                },
                ColAccentGreen
            ));

            _toolsFlowPanel.Controls.Add(CreateToolCard(
                "Task Manager & Startup Apps",
                "Inspect real-time CPU/RAM usage, kill hanging processes, and manage startup programs.",
                "Open Task Manager",
                UiIcon.Speed,
                delegate {
                    LogToolMessage("Launching Windows Task Manager...");
                    SystemToolsManager.OpenTaskManager();
                },
                ColAccentGreen
            ));

            _toolsFlowPanel.Controls.Add(CreateToolCard(
                "Windows Services (services.msc)",
                "Manage background Windows services, start/stop services, and set startup types.",
                "Open services.msc",
                UiIcon.Settings,
                delegate {
                    LogToolMessage("Opening Windows Services console...");
                    SystemToolsManager.OpenServicesManager();
                },
                ColAccentGreen
            ));

            _toolsFlowPanel.Controls.Add(CreateToolCard(
                "Windows Defender Security",
                "Open Windows Security settings, Virus & Threat Protection, and file exclusions.",
                "Open Windows Security",
                UiIcon.Shield,
                delegate {
                    LogToolMessage("Opening Windows Security...");
                    SystemToolsManager.OpenWindowsSecurity();
                },
                ColAccentGreen
            ));

            Panel lastSec3Card = CreateToolCard(
                "Windows Activation & License",
                "Check genuine activation status or enter product key in Windows Settings.",
                "Open Activation",
                UiIcon.Key,
                delegate {
                    LogToolMessage("Opening Windows Activation & Licensing panel...");
                    SystemToolsManager.OpenActivationSettings();
                },
                ColAccentGreen
            );
            _toolsFlowPanel.Controls.Add(lastSec3Card);
            _toolsFlowPanel.SetFlowBreak(lastSec3Card, true);

            // -------------------------------------------------------------
            // SECTION 4: NETWORK & CONNECTIVITY
            // -------------------------------------------------------------
            _toolsFlowPanel.Controls.Add(CreateSectionHeader("NETWORK, TIME & DNS CONFIGURATION", UiIcon.Network, ColAccentCyan));

            _toolsFlowPanel.Controls.Add(CreateToolCard(
                "Date & Time Settings",
                "Open Windows Settings to configure clock, automatic time, and calendar.",
                "Open Time Settings",
                UiIcon.ClockCircle,
                delegate {
                    LogToolMessage("Opening Windows Date & Time settings...");
                    SystemToolsManager.OpenDateAndTimeSettings();
                },
                ColAccentCyan
            ));

            _toolsFlowPanel.Controls.Add(CreateToolCard(
                "Sync Internet Time Now",
                "Force resynchronization of system clock with Windows Internet Time servers.",
                "Sync Clock Now",
                UiIcon.Reload,
                delegate {
                    LogToolMessage("Synchronizing system time with time servers...");
                    string result = SystemToolsManager.SyncTimeNow();
                    LogToolMessage(result);
                },
                ColAccentCyan
            ));

            _toolsFlowPanel.Controls.Add(CreateToolCard(
                "Time Zone Configuration",
                "Open Time Zone selector to change system region (e.g. UTC+08:00 Manila).",
                "Change Time Zone",
                UiIcon.Network,
                delegate {
                    LogToolMessage("Opening Time Zone selector dialog...");
                    SystemToolsManager.OpenTimeZoneSettings();
                },
                ColAccentCyan
            ));

            _toolsFlowPanel.Controls.Add(CreateToolCard(
                "Network Adapters (ncpa.cpl)",
                "Open classic Network Connections control panel to inspect Ethernet and WiFi.",
                "Open ncpa.cpl",
                UiIcon.Network,
                delegate {
                    LogToolMessage("Opening Network Connections panel...");
                    SystemToolsManager.OpenNetworkConnections();
                },
                ColAccentCyan
            ));

            _toolsFlowPanel.Controls.Add(CreateToolCard(
                "Flush DNS Resolver Cache",
                "Clears DNS resolver cache to fix internet loading issues and resolve hostnames.",
                "Flush DNS Cache",
                UiIcon.Clean,
                delegate {
                    LogToolMessage("Flushing DNS resolver cache via ipconfig...");
                    string result = SystemToolsManager.FlushDns();
                    LogToolMessage(result);
                },
                ColAccentCyan
            ));

            _toolsFlowPanel.Controls.Add(CreateToolCard(
                "Set Cloudflare DNS (1.1.1.1)",
                "Configures high-speed, privacy-focused Cloudflare DNS (1.1.1.1 & 1.0.0.1).",
                "Apply 1.1.1.1",
                UiIcon.Speed,
                delegate {
                    LogToolMessage("Configuring Cloudflare DNS servers...");
                    string res = SystemToolsManager.SetDnsServers("cloudflare");
                    LogToolMessage(res);
                },
                ColAccentCyan
            ));

            _toolsFlowPanel.Controls.Add(CreateToolCard(
                "Set Google DNS (8.8.8.8)",
                "Configures fast Google Public DNS servers (8.8.8.8 & 8.8.4.4).",
                "Apply 8.8.8.8",
                UiIcon.Speed,
                delegate {
                    LogToolMessage("Configuring Google Public DNS servers...");
                    string res = SystemToolsManager.SetDnsServers("google");
                    LogToolMessage(res);
                },
                ColAccentCyan
            ));

            Panel lastSec4Card = CreateToolCard(
                "Reset DNS to Automatic (DHCP)",
                "Restores automatic router/ISP DNS server assignment on all active adapters.",
                "Reset to DHCP",
                UiIcon.Reload,
                delegate {
                    LogToolMessage("Resetting DNS server configuration to DHCP...");
                    string res = SystemToolsManager.SetDnsServers("dhcp");
                    LogToolMessage(res);
                },
                ColAccentCyan
            );
            _toolsFlowPanel.Controls.Add(lastSec4Card);
            _toolsFlowPanel.SetFlowBreak(lastSec4Card, true);

            // -------------------------------------------------------------
            // SECTION 5: AUTOMATION & CUSTOMIZATION
            // -------------------------------------------------------------
            _toolsFlowPanel.Controls.Add(CreateSectionHeader("AUTOMATION, SCRIPTS & CUSTOMIZATION", UiIcon.CommandLine, ColAccentPurple));

            _toolsFlowPanel.Controls.Add(CreateToolCard(
                "Run Custom Script (custom.ps1)",
                "Auto-runs your custom PowerShell code in scripts\\custom.ps1 as Administrator.",
                "Run custom.ps1",
                UiIcon.Play,
                delegate {
                    LogToolMessage("Launching custom PowerShell script (scripts\\custom.ps1)...");
                    SystemToolsManager.OpenCustomPowerShellScript();
                },
                ColAccentPurple
            ));

            _toolsFlowPanel.Controls.Add(CreateToolCard(
                "Edit custom.ps1 Script",
                "Opens scripts\\custom.ps1 in Notepad so you can paste or edit your commands.",
                "Edit in Notepad",
                UiIcon.Edit,
                delegate {
                    LogToolMessage("Opening scripts\\custom.ps1 in Notepad...");
                    SystemToolsManager.EditCustomPowerShellScript();
                },
                ColAccentPurple
            ));

            _toolsFlowPanel.Controls.Add(CreateToolCard(
                "Administrator PowerShell",
                "Opens a clean elevated PowerShell console window ready for any commands.",
                "Open PowerShell",
                UiIcon.CommandLine,
                delegate {
                    LogToolMessage("Opening Administrator PowerShell console...");
                    SystemToolsManager.OpenElevatedPowerShell();
                },
                ColAccentPurple
            ));

            _toolsFlowPanel.Controls.Add(CreateToolCard(
                "Desktop Background & Themes",
                "Change desktop wallpaper, lock screen, colors, and Windows dark/light mode.",
                "Change Background",
                UiIcon.Theme,
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

        private Panel CreateSectionHeader(string title, UiIcon icon, Color accentColor)
        {
            Panel header = new Panel();
            header.Width = 1100;
            header.Height = 36;
            header.Margin = new Padding(10, 16, 10, 6);
            header.BackColor = Color.FromArgb(16, 26, 48);

            Panel bar = new Panel();
            bar.Width = 5;
            bar.Dock = DockStyle.Left;
            bar.BackColor = accentColor;

            PictureBox pic = new PictureBox();
            pic.Size = new Size(18, 18);
            pic.Location = new Point(14, 9);
            pic.Image = UiIconHelper.GetIcon(icon, 16, accentColor);
            pic.BackColor = Color.Transparent;

            Label lbl = new Label();
            lbl.UseMnemonic = false;
            lbl.Text = title;
            lbl.Font = new Font("Segoe UI", 10.0F, FontStyle.Bold);
            lbl.ForeColor = accentColor;
            lbl.Location = new Point(38, 8);
            lbl.AutoSize = true;

            header.Controls.Add(lbl);
            header.Controls.Add(pic);
            header.Controls.Add(bar);

            _toolsFlowPanel.SetFlowBreak(header, true);
            return header;
        }

        private Panel CreateToolCard(string title, string description, string buttonText, UiIcon btnIcon, EventHandler onClick, Color titleColor)
        {
            Panel card = new Panel();
            card.Width = 345;
            card.Height = 138;
            card.BackColor = ColCard;
            card.Margin = new Padding(10);
            card.Padding = new Padding(14);
            card.Paint += delegate(object s, PaintEventArgs pe) {
                using (Pen p = new Pen(ColBorder, 1))
                {
                    pe.Graphics.DrawRectangle(p, 0, 0, card.Width - 1, card.Height - 1);
                }
            };

            Label lblT = new Label();
            lblT.UseMnemonic = false;
            lblT.Text = title;
            lblT.Font = new Font("Segoe UI", 10.0F, FontStyle.Bold);
            lblT.ForeColor = titleColor;
            lblT.AutoSize = true;
            lblT.Location = new Point(12, 10);

            Label lblD = new Label();
            lblD.UseMnemonic = false;
            lblD.Text = description;
            lblD.Font = new Font("Segoe UI", 8.5F);
            lblD.ForeColor = ColTextSecondary;
            lblD.Location = new Point(12, 36);
            lblD.Width = 320;
            lblD.Height = 44;

            Button btn = CreatePillButton("  " + buttonText, btnIcon, ColCardAlt, ColTextPrimary, 320, 32);
            btn.Location = new Point(12, 92);
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
                row.Cells["CIcon"].Value = AppIconHelper.GetAppIcon(app.Name, app.Name, app.Category, 36);
                row.Cells["CName"].Value = app.Name;
                row.Cells["CCat"].Value = app.Category;
                row.Cells["CVer"].Value = app.Version;
                row.Cells["CSize"].Value = app.EstimatedSize;
                row.Cells["CArrow"].Value = "›";
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
            _picCloudIcon.Image = AppIconHelper.GetAppIcon(app.Name, app.Name, app.Category, 48);
            _lblCloudTitle.Text = app.Name;
            _lblCloudCategoryBadge.Text = app.Category;
            _lblCloudVersionBadge.Text = string.IsNullOrEmpty(app.Version) ? "v1.0" : ("v" + app.Version);
            _lblCloudValCat.Text = app.Category;
            _lblCloudValVer.Text = string.IsNullOrEmpty(app.Version) ? "Latest" : app.Version;
            _lblCloudValSize.Text = string.IsNullOrEmpty(app.EstimatedSize) ? "Unknown" : app.EstimatedSize;
            _txtCloudDesc.Text = app.Description + (string.IsNullOrEmpty(app.Notes) ? "" : "\r\n\r\nTechnician Notes: " + app.Notes);
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
        private Button CreatePillButton(string text, Color bg, Color foreColor, int width, int height)
        {
            Button btn = new Button();
            btn.UseMnemonic = false;
            btn.Text = text;
            btn.BackColor = bg;
            btn.ForeColor = foreColor;
            btn.FlatStyle = FlatStyle.Flat;
            btn.FlatAppearance.BorderSize = 1;
            btn.FlatAppearance.BorderColor = ColCardBorder;
            btn.Width = width;
            btn.Height = height;
            btn.Cursor = Cursors.Hand;
            btn.Font = new Font("Segoe UI", 9.0F, FontStyle.Bold);
            return btn;
        }

        private Button CreatePillButton(string text, UiIcon icon, Color bg, Color foreColor, int width, int height)
        {
            Button btn = CreatePillButton(text, bg, foreColor, width, height);
            btn.Image = UiIconHelper.GetIcon(icon, 14, foreColor);
            btn.TextImageRelation = TextImageRelation.ImageBeforeText;
            btn.ImageAlign = ContentAlignment.MiddleCenter;
            return btn;
        }

        private void EnableDoubleBuffering(Control control)
        {
            try
            {
                typeof(Control).InvokeMember(
                    "DoubleBuffered",
                    System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.SetProperty,
                    null,
                    control,
                    new object[] { true }
                );
            }
            catch { }
        }

        private void LoadAppCatalog()
        {
            _allApps = ConfigManager.LoadApps();
            PopulateGrid(_allApps);
            LogText(string.Format("Software catalog initialized ({0} applications ready).", _allApps.Count), LogLevel.Info);
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
                row.Cells["ColIcon"].Value = AppIconHelper.GetAppIcon(app.Id, app.Name, app.Category, 36);
                row.Cells["ColName"].Value = app.Name;
                row.Cells["ColCategory"].Value = app.Category;
                row.Cells["ColSize"].Value = app.EstimatedSizeMB > 0 ? string.Format("{0} MB", app.EstimatedSizeMB) : "-";

                string sourceText = app.IsCached ? "✔ USB Cache" : "☁ Download";
                row.Cells["ColSource"].Value = sourceText;
                row.Cells["ColSource"].Style.ForeColor = app.IsCached ? ColAccentGreen : ColAccentBlue;
                row.Cells["ColSource"].Style.Font = new Font("Segoe UI", 9.0F, FontStyle.Bold);

                string displayStatus = (app.Status == "Pending" && app.IsCached) ? "● Cached (Offline)" : ("● " + app.Status);
                row.Cells["ColStatus"].Value = displayStatus;
                row.Cells["ColArrow"].Value = "›";

                ApplyRowStatusColor(row, app.Status, app.IsCached);
            }

            _gridApps.ResumeLayout();
            if (_gridApps.Rows.Count > 0)
            {
                DisplayAppDetails(_gridApps.Rows[0].Tag as AppItem);
            }
        }

        private void ApplyRowStatusColor(DataGridViewRow row, string status, bool isCached)
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
                row.Cells["ColStatus"].Style.ForeColor = isCached ? ColAccentGreen : ColTextSecondary;
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
            if (app == null) return;
            _selectedApp = app;

            _picDetailsIcon.Image = AppIconHelper.GetAppIcon(app.Id, app.Name, app.Category, 48);
            _lblDetailsTitle.Text = app.Name;
            _lblDetailsCategoryBadge.Text = app.Category;

            bool isPortable = !string.IsNullOrEmpty(app.SpecialAction) && app.SpecialAction.Contains("portable");
            _lblDetailsTypeBadge.Text = isPortable ? "Portable Tool" : (app.IsCached ? "USB Offline Ready" : "Cloud Download");
            _lblDetailsTypeBadge.ForeColor = isPortable ? ColAccentPurple : (app.IsCached ? ColAccentGreen : ColAccentBlue);

            _lblValSize.Text = app.EstimatedSizeMB > 0 ? string.Format("{0} MB", app.EstimatedSizeMB) : "-";
            _lblValStatus.Text = app.IsCached ? "✔ Cached in USB" : "☁ Needs Download";
            _lblValStatus.ForeColor = app.IsCached ? ColAccentGreen : ColAccentBlue;

            _lblValSource.Text = app.IsCached ? "Local USB Storage" : "Direct Download";
            _lblValSwitch.Text = string.IsNullOrEmpty(app.SilentArgs)
                ? (string.IsNullOrEmpty(app.SpecialAction) ? "Standard Setup" : app.SpecialAction)
                : app.SilentArgs;

            _txtDetailsDesc.Text = app.Description;
            _btnDetailsOpenUrl.Enabled = !string.IsNullOrEmpty(app.DownloadUrl);
            _btnDetailsOpenFolder.Enabled = app.IsCached;
        }

        private void OpenAppHomepage(AppItem app)
        {
            if (app == null || string.IsNullOrEmpty(app.DownloadUrl)) return;
            try
            {
                Process.Start(app.DownloadUrl);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Could not open URL: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void OpenContainingFolder(AppItem app)
        {
            if (app == null) return;
            string cacheDir = ConfigManager.GetCacheDirectory();
            if (!string.IsNullOrEmpty(app.CacheFileName))
            {
                string filePath = Path.Combine(cacheDir, app.CacheFileName);
                if (File.Exists(filePath))
                {
                    try
                    {
                        Process.Start("explorer.exe", string.Format("/select,\"{0}\"", filePath));
                        return;
                    }
                    catch { }
                }
            }
            OpenCacheFolder();
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
                _lblOverallStatus.Text = "Ready. Select software packages and click 'Start Installation'.";
                _lblTimeEstimate.Text = "Estimated time: ~0 min  •  Select items to begin";
            }
            else if (totalMB == 0 && count > 0)
            {
                _lblOverallStatus.Text = string.Format("Selected: {0} applications | ALL CACHED OFFLINE (0 MB download needed!)", count);
                int estMinutes = Math.Max(1, (count * 15) / 60);
                _lblTimeEstimate.Text = string.Format("Estimated installation time: ~{0}-{1} min  •  High-speed offline installation", estMinutes, estMinutes + 2);
            }
            else
            {
                _lblOverallStatus.Text = string.Format("Selected: {0} applications ({1} offline, {2} online) | Est. Download: ~{3} MB",
                    count, cachedCount, count - cachedCount, totalMB);
                int estMinutes = Math.Max(2, (count * 25) / 60);
                _lblTimeEstimate.Text = string.Format("Estimated total time: ~{0}-{1} min  •  Downloading {2} MB online", estMinutes, estMinutes + 3, totalMB);
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
                MessageBox.Show("All software packages are already cached in your USB!", "Cache Complete", MessageBoxButtons.OK, MessageBoxIcon.Information);
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
                MessageBox.Show("Please select at least one software package to install.", "No Software Selected", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            _btnAction.Image = UiIconHelper.GetIcon(UiIcon.Cancel, 18, Color.White);
            _btnAction.Text = "  CANCEL QUEUE";
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
                _btnAction.Image = UiIconHelper.GetIcon(UiIcon.Play, 18, Color.White);
                _btnAction.Text = "  Start Installation";
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
                    string displayStatus = (app.Status == "Pending" && app.IsCached) ? "● Cached (Offline)" : ("● " + app.Status);
                    row.Cells["ColStatus"].Value = displayStatus;
                    ApplyRowStatusColor(row, app.Status, app.IsCached);

                    if (app.IsCached)
                    {
                        row.Cells["ColSource"].Value = "✔ USB Cache";
                        row.Cells["ColSource"].Style.ForeColor = ColAccentGreen;
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
