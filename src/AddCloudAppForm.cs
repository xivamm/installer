using System;
using System.Drawing;
using System.Windows.Forms;

namespace TechInstaller
{
    public class AddCloudAppForm : Form
    {
        public CloudAppItem CreatedItem { get; private set; }

        private TextBox _txtName;
        private ComboBox _cmbCategory;
        private TextBox _txtVersion;
        private TextBox _txtSize;
        private TextBox _txtDriveUrl;
        private TextBox _txtDescription;
        private Button _btnSave;
        private Button _btnCancel;

        private readonly Color ColBg = Color.FromArgb(15, 23, 42);
        private readonly Color ColCard = Color.FromArgb(30, 41, 59);
        private readonly Color ColAccentBlue = Color.FromArgb(56, 189, 248);
        private readonly Color ColAccentGreen = Color.FromArgb(16, 185, 129);
        private readonly Color ColTextPrimary = Color.FromArgb(248, 250, 252);
        private readonly Color ColTextSecondary = Color.FromArgb(148, 163, 184);

        public AddCloudAppForm()
        {
            InitializeComponent();
        }

        private void InitializeComponent()
        {
            this.Text = "Add New Google Drive Application Package";
            this.Size = new Size(520, 520);
            this.StartPosition = FormStartPosition.CenterParent;
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.BackColor = ColBg;
            this.ForeColor = ColTextPrimary;
            this.Font = new Font("Segoe UI", 9.5F, FontStyle.Regular, GraphicsUnit.Point);

            Label lblHeader = new Label();
            lblHeader.Text = "➕ Add Application Package to Google Drive List";
            lblHeader.Font = new Font("Segoe UI", 12.0F, FontStyle.Bold);
            lblHeader.ForeColor = ColAccentBlue;
            lblHeader.AutoSize = true;
            lblHeader.Location = new Point(20, 16);

            // Name
            Label lblName = CreateLabel("Application Name (e.g. AutoCAD 2024, Filmora 13):", 20, 55);
            _txtName = CreateTextBox(20, 78, 460);

            // Category & Version
            Label lblCat = CreateLabel("Category:", 20, 118);
            _cmbCategory = new ComboBox();
            _cmbCategory.Location = new Point(20, 140);
            _cmbCategory.Width = 220;
            _cmbCategory.BackColor = ColCard;
            _cmbCategory.ForeColor = ColTextPrimary;
            _cmbCategory.FlatStyle = FlatStyle.Flat;
            _cmbCategory.Items.AddRange(new object[] {
                "Productivity",
                "Design & Graphics",
                "Video Editing",
                "Engineering & CAD",
                "Utilities",
                "Drivers",
                "Gaming",
                "Other"
            });
            _cmbCategory.SelectedIndex = 0;

            Label lblVer = CreateLabel("Version / Edition:", 260, 118);
            _txtVersion = CreateTextBox(260, 140, 220);
            _txtVersion.Text = "Latest";

            // Size
            Label lblSize = CreateLabel("Package Size (e.g. 1.5 GB, 800 MB):", 20, 180);
            _txtSize = CreateTextBox(20, 202, 460);
            _txtSize.Text = "1.0 GB";

            // Google Drive URL
            Label lblUrl = CreateLabel("Google Drive Sharing Link (URL):", 20, 242);
            _txtDriveUrl = CreateTextBox(20, 264, 460);
            _txtDriveUrl.ForeColor = ColAccentBlue;

            // Description / Notes
            Label lblDesc = CreateLabel("Description & Technician Notes:", 20, 304);
            _txtDescription = new TextBox();
            _txtDescription.Location = new Point(20, 326);
            _txtDescription.Width = 460;
            _txtDescription.Height = 75;
            _txtDescription.Multiline = true;
            _txtDescription.BackColor = ColCard;
            _txtDescription.ForeColor = ColTextPrimary;
            _txtDescription.BorderStyle = BorderStyle.FixedSingle;

            // Buttons
            _btnSave = new Button();
            _btnSave.Text = "💾 Save to List";
            _btnSave.BackColor = ColAccentGreen;
            _btnSave.ForeColor = Color.White;
            _btnSave.FlatStyle = FlatStyle.Flat;
            _btnSave.FlatAppearance.BorderSize = 0;
            _btnSave.Font = new Font("Segoe UI", 10.0F, FontStyle.Bold);
            _btnSave.Width = 150;
            _btnSave.Height = 38;
            _btnSave.Location = new Point(200, 420);
            _btnSave.Cursor = Cursors.Hand;
            _btnSave.Click += OnSaveClick;

            _btnCancel = new Button();
            _btnCancel.Text = "Cancel";
            _btnCancel.BackColor = Color.FromArgb(51, 65, 85);
            _btnCancel.ForeColor = ColTextPrimary;
            _btnCancel.FlatStyle = FlatStyle.Flat;
            _btnCancel.FlatAppearance.BorderSize = 0;
            _btnCancel.Font = new Font("Segoe UI", 10.0F);
            _btnCancel.Width = 110;
            _btnCancel.Height = 38;
            _btnCancel.Location = new Point(370, 420);
            _btnCancel.Cursor = Cursors.Hand;
            _btnCancel.Click += delegate { this.DialogResult = DialogResult.Cancel; this.Close(); };

            this.Controls.Add(lblHeader);
            this.Controls.Add(lblName);
            this.Controls.Add(_txtName);
            this.Controls.Add(lblCat);
            this.Controls.Add(_cmbCategory);
            this.Controls.Add(lblVer);
            this.Controls.Add(_txtVersion);
            this.Controls.Add(lblSize);
            this.Controls.Add(_txtSize);
            this.Controls.Add(lblUrl);
            this.Controls.Add(_txtDriveUrl);
            this.Controls.Add(lblDesc);
            this.Controls.Add(_txtDescription);
            this.Controls.Add(_btnSave);
            this.Controls.Add(_btnCancel);
        }

        private Label CreateLabel(string text, int x, int y)
        {
            Label lbl = new Label();
            lbl.Text = text;
            lbl.ForeColor = ColTextSecondary;
            lbl.Font = new Font("Segoe UI", 9.0F, FontStyle.Bold);
            lbl.AutoSize = true;
            lbl.Location = new Point(x, y);
            return lbl;
        }

        private TextBox CreateTextBox(int x, int y, int width)
        {
            TextBox txt = new TextBox();
            txt.Location = new Point(x, y);
            txt.Width = width;
            txt.BackColor = ColCard;
            txt.ForeColor = ColTextPrimary;
            txt.BorderStyle = BorderStyle.FixedSingle;
            return txt;
        }

        private void OnSaveClick(object sender, EventArgs e)
        {
            string name = _txtName.Text.Trim();
            if (string.IsNullOrEmpty(name))
            {
                MessageBox.Show("Please enter an Application Name.", "Validation Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                _txtName.Focus();
                return;
            }

            string url = _txtDriveUrl.Text.Trim();
            if (string.IsNullOrEmpty(url))
            {
                MessageBox.Show("Please enter a Google Drive link or download URL.", "Validation Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                _txtDriveUrl.Focus();
                return;
            }

            CreatedItem = new CloudAppItem();
            CreatedItem.Id = "app_" + DateTime.Now.Ticks;
            CreatedItem.Name = name;
            CreatedItem.Category = _cmbCategory.Text;
            CreatedItem.Version = string.IsNullOrEmpty(_txtVersion.Text.Trim()) ? "Latest" : _txtVersion.Text.Trim();
            CreatedItem.EstimatedSize = string.IsNullOrEmpty(_txtSize.Text.Trim()) ? "N/A" : _txtSize.Text.Trim();
            CreatedItem.DriveUrl = url;
            CreatedItem.Description = _txtDescription.Text.Trim();
            CreatedItem.Notes = "Added via TechInstaller UI";

            this.DialogResult = DialogResult.OK;
            this.Close();
        }
    }
}
