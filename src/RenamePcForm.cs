using System;
using System.Drawing;
using System.Text.RegularExpressions;
using System.Windows.Forms;

namespace TechInstaller
{
    public class RenamePcForm : Form
    {
        private TextBox _txtNewName;
        private CheckBox _chkRestart;
        private Label _lblError;
        private Button _btnApply;
        private Button _btnCancel;

        public string NewComputerName { get; private set; }
        public bool RestartNow { get; private set; }

        private readonly Color ColBg = Color.FromArgb(8, 14, 30);
        private readonly Color ColCard = Color.FromArgb(13, 23, 46);
        private readonly Color ColCardAlt = Color.FromArgb(19, 31, 58);
        private readonly Color ColBorder = Color.FromArgb(30, 45, 75);
        private readonly Color ColAccentBlue = Color.FromArgb(56, 189, 248);
        private readonly Color ColTextPrimary = Color.FromArgb(248, 250, 252);
        private readonly Color ColTextSecondary = Color.FromArgb(148, 163, 184);
        private readonly Color ColError = Color.FromArgb(239, 68, 68);

        public RenamePcForm()
        {
            InitializeComponent();
        }

        private void InitializeComponent()
        {
            this.Text = "Rename This PC";
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.StartPosition = FormStartPosition.CenterParent;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.ShowInTaskbar = false;
            this.Size = new Size(500, 400);
            this.BackColor = ColBg;
            this.ForeColor = ColTextPrimary;
            this.Font = new Font("Segoe UI", 9.5F, FontStyle.Regular, GraphicsUnit.Point);

            // Header Banner
            Panel headerPanel = new Panel();
            headerPanel.Dock = DockStyle.Top;
            headerPanel.Height = 60;
            headerPanel.BackColor = ColCard;
            headerPanel.Padding = new Padding(16, 12, 16, 10);
            headerPanel.Paint += delegate(object s, PaintEventArgs pe) {
                using (Pen p = new Pen(ColBorder, 1))
                {
                    pe.Graphics.DrawLine(p, 0, headerPanel.Height - 1, headerPanel.Width, headerPanel.Height - 1);
                }
            };

            PictureBox picIcon = new PictureBox();
            picIcon.Size = new Size(24, 24);
            picIcon.Location = new Point(16, 17);
            picIcon.Image = UiIconHelper.GetIcon(UiIcon.Computer, 20, ColAccentBlue);
            picIcon.BackColor = Color.Transparent;

            Label lblTitle = new Label();
            lblTitle.Text = "Rename Computer (PC Name)";
            lblTitle.Font = new Font("Segoe UI", 11.5F, FontStyle.Bold);
            lblTitle.ForeColor = ColTextPrimary;
            lblTitle.Location = new Point(48, 10);
            lblTitle.AutoSize = true;

            Label lblSub = new Label();
            lblSub.Text = "Change the NetBIOS device name seen on local network";
            lblSub.Font = new Font("Segoe UI", 8.5F);
            lblSub.ForeColor = ColTextSecondary;
            lblSub.Location = new Point(49, 32);
            lblSub.AutoSize = true;

            headerPanel.Controls.Add(picIcon);
            headerPanel.Controls.Add(lblTitle);
            headerPanel.Controls.Add(lblSub);

            // Main Content Container
            Panel contentPanel = new Panel();
            contentPanel.Dock = DockStyle.Fill;
            contentPanel.Padding = new Padding(20, 16, 20, 16);

            // Current PC Name Label
            Label lblCurrentTitle = new Label();
            lblCurrentTitle.Text = "CURRENT COMPUTER NAME:";
            lblCurrentTitle.Font = new Font("Segoe UI", 8.5F, FontStyle.Bold);
            lblCurrentTitle.ForeColor = ColTextSecondary;
            lblCurrentTitle.Location = new Point(20, 16);
            lblCurrentTitle.AutoSize = true;

            Label lblCurrentVal = new Label();
            lblCurrentVal.Text = Environment.MachineName;
            lblCurrentVal.Font = new Font("Segoe UI", 11.0F, FontStyle.Bold);
            lblCurrentVal.ForeColor = ColAccentBlue;
            lblCurrentVal.Location = new Point(20, 36);
            lblCurrentVal.AutoSize = true;

            // New PC Name Input Label
            Label lblNewTitle = new Label();
            lblNewTitle.Text = "ENTER NEW COMPUTER NAME (1-15 CHARACTERS):";
            lblNewTitle.Font = new Font("Segoe UI", 8.5F, FontStyle.Bold);
            lblNewTitle.ForeColor = ColTextSecondary;
            lblNewTitle.Location = new Point(20, 72);
            lblNewTitle.AutoSize = true;

            // TextBox Panel container for styled border
            Panel txtContainer = new Panel();
            txtContainer.Location = new Point(20, 94);
            txtContainer.Size = new Size(420, 34);
            txtContainer.BackColor = ColCardAlt;
            txtContainer.Padding = new Padding(8, 7, 8, 7);
            txtContainer.Paint += delegate(object s, PaintEventArgs pe) {
                using (Pen p = new Pen(ColBorder, 1))
                {
                    pe.Graphics.DrawRectangle(p, 0, 0, txtContainer.Width - 1, txtContainer.Height - 1);
                }
            };

            _txtNewName = new TextBox();
            _txtNewName.Dock = DockStyle.Fill;
            _txtNewName.BackColor = ColCardAlt;
            _txtNewName.ForeColor = ColTextPrimary;
            _txtNewName.Font = new Font("Segoe UI", 10.5F);
            _txtNewName.BorderStyle = BorderStyle.None;
            _txtNewName.MaxLength = 15;
            _txtNewName.Text = Environment.MachineName;
            _txtNewName.TextChanged += delegate { ValidateInput(); };
            txtContainer.Controls.Add(_txtNewName);

            _lblError = new Label();
            _lblError.Location = new Point(20, 132);
            _lblError.Size = new Size(420, 20);
            _lblError.ForeColor = ColError;
            _lblError.Font = new Font("Segoe UI", 8.5F, FontStyle.Regular);
            _lblError.Text = "";

            // Checkbox for reboot
            _chkRestart = new CheckBox();
            _chkRestart.Text = "Restart computer immediately after applying new name";
            _chkRestart.Font = new Font("Segoe UI", 9.0F);
            _chkRestart.ForeColor = ColTextPrimary;
            _chkRestart.Location = new Point(20, 154);
            _chkRestart.Size = new Size(420, 24);
            _chkRestart.Checked = false;

            // Informational Notice
            Label lblNotice = new Label();
            lblNotice.Text = "Note: Standard Windows naming rules: letters (A-Z), numbers (0-9), and hyphens (-). Spaces and special symbols are not allowed.";
            lblNotice.Font = new Font("Segoe UI", 8.0F);
            lblNotice.ForeColor = ColTextSecondary;
            lblNotice.Location = new Point(20, 184);
            lblNotice.Size = new Size(420, 32);

            contentPanel.Controls.Add(lblCurrentTitle);
            contentPanel.Controls.Add(lblCurrentVal);
            contentPanel.Controls.Add(lblNewTitle);
            contentPanel.Controls.Add(txtContainer);
            contentPanel.Controls.Add(_lblError);
            contentPanel.Controls.Add(_chkRestart);
            contentPanel.Controls.Add(lblNotice);

            // Bottom Buttons Panel
            Panel bottomPanel = new Panel();
            bottomPanel.Dock = DockStyle.Bottom;
            bottomPanel.Height = 55;
            bottomPanel.BackColor = ColCard;
            bottomPanel.Padding = new Padding(16, 10, 16, 10);
            bottomPanel.Paint += delegate(object s, PaintEventArgs pe) {
                using (Pen p = new Pen(ColBorder, 1))
                {
                    pe.Graphics.DrawLine(p, 0, 0, bottomPanel.Width, 0);
                }
            };

            _btnCancel = new Button();
            _btnCancel.Text = "Cancel";
            _btnCancel.Size = new Size(100, 34);
            _btnCancel.Location = new Point(bottomPanel.Width - 116 - 130, 10);
            _btnCancel.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;
            _btnCancel.BackColor = ColCardAlt;
            _btnCancel.ForeColor = ColTextSecondary;
            _btnCancel.FlatStyle = FlatStyle.Flat;
            _btnCancel.FlatAppearance.BorderColor = ColBorder;
            _btnCancel.Cursor = Cursors.Hand;
            _btnCancel.DialogResult = DialogResult.Cancel;
            _btnCancel.Click += delegate {
                this.DialogResult = DialogResult.Cancel;
                this.Close();
            };

            _btnApply = new Button();
            _btnApply.Text = "Apply Rename";
            _btnApply.Size = new Size(120, 34);
            _btnApply.Location = new Point(bottomPanel.Width - 136, 10);
            _btnApply.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;
            _btnApply.BackColor = ColAccentBlue;
            _btnApply.ForeColor = Color.Black;
            _btnApply.Font = new Font("Segoe UI", 9.0F, FontStyle.Bold);
            _btnApply.FlatStyle = FlatStyle.Flat;
            _btnApply.FlatAppearance.BorderSize = 0;
            _btnApply.Cursor = Cursors.Hand;
            _btnApply.Click += OnApplyClick;

            bottomPanel.Controls.Add(_btnCancel);
            bottomPanel.Controls.Add(_btnApply);

            this.Controls.Add(contentPanel);
            this.Controls.Add(bottomPanel);
            this.Controls.Add(headerPanel);

            headerPanel.SendToBack();
            bottomPanel.SendToBack();
            contentPanel.BringToFront();

            this.AcceptButton = _btnApply;
            this.CancelButton = _btnCancel;

            this.Shown += delegate {
                _txtNewName.Focus();
                _txtNewName.SelectAll();
            };
        }

        private bool ValidateInput()
        {
            string name = _txtNewName.Text.Trim();
            if (string.IsNullOrEmpty(name))
            {
                _lblError.Text = "Computer name cannot be empty.";
                _btnApply.Enabled = false;
                return false;
            }

            if (name.Length > 15)
            {
                _lblError.Text = "Computer name must be 15 characters or fewer.";
                _btnApply.Enabled = false;
                return false;
            }

            // Must match alphanumeric and hyphen only
            if (!Regex.IsMatch(name, "^[a-zA-Z0-9\\-]+$"))
            {
                _lblError.Text = "Invalid characters. Use only letters, numbers, and hyphens.";
                _btnApply.Enabled = false;
                return false;
            }

            if (name.StartsWith("-") || name.EndsWith("-"))
            {
                _lblError.Text = "Computer name cannot start or end with a hyphen.";
                _btnApply.Enabled = false;
                return false;
            }

            if (string.Equals(name, Environment.MachineName, StringComparison.OrdinalIgnoreCase))
            {
                _lblError.Text = "New name is identical to the current computer name.";
                _btnApply.Enabled = false;
                return false;
            }

            _lblError.Text = "";
            _btnApply.Enabled = true;
            return true;
        }

        private void OnApplyClick(object sender, EventArgs e)
        {
            if (!ValidateInput())
            {
                return;
            }

            NewComputerName = _txtNewName.Text.Trim();
            RestartNow = _chkRestart.Checked;
            this.DialogResult = DialogResult.OK;
            this.Close();
        }
    }
}
