using System;
using System.Drawing;
using System.IO;
using System.Text.Json;
using System.Windows.Forms;

namespace TODO
{
    public class SettingsForm : Form
    {
        // Mark all fields that are initialized in InitializeComponent as nullable
        private ComboBox? cmbFontFamily;
        private NumericUpDown? nudFontSize;
        private CheckBox? chkFontBold;
        private Button? btnTextColor;
        private Panel? pnlTextColorPreview;
        private Button? btnBackgroundImage;
        private CheckBox? chkMinimizeToTray;
        private CheckBox? chkStartMinimized;
        private Button? btnOK;
        private Button? btnCancel;
        private Button? btnReset;
        private Button? btnAbout;
        private Label? lblFontPreview;
        
        public AppSettings Settings { get; private set; }
        private Color selectedTextColor;
        private string selectedBackgroundPath = "";

        public SettingsForm(AppSettings currentSettings)
        {
            Settings = currentSettings.Clone();
            selectedTextColor = Settings.TextColor;
            selectedBackgroundPath = Settings.BackgroundImagePath;
            InitializeComponent();
            LoadCurrentSettings();
        }

        private void InitializeComponent()
        {
            this.Text = "⚙ TODO Settings";
            this.Size = new Size(450, 520); // Reduced height since we removed the dropdown
            this.StartPosition = FormStartPosition.CenterParent;
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.BackColor = Color.FromArgb(45, 45, 48);
            this.ForeColor = Color.White;

            // Font settings group
            var grpFont = new GroupBox
            {
                Text = "Font Settings",
                Location = new Point(12, 12),
                Size = new Size(410, 120),
                ForeColor = Color.White
            };

            var lblFontFamily = new Label
            {
                Text = "Font Family:",
                Location = new Point(10, 25),
                Size = new Size(80, 20),
                ForeColor = Color.White
            };

            cmbFontFamily = new ComboBox
            {
                Location = new Point(95, 23),
                Size = new Size(150, 23),
                DropDownStyle = ComboBoxStyle.DropDownList,
                BackColor = Color.FromArgb(30, 30, 30),
                ForeColor = Color.White
            };

            var lblFontSize = new Label
            {
                Text = "Size:",
                Location = new Point(260, 25),
                Size = new Size(35, 20),
                ForeColor = Color.White
            };

            nudFontSize = new NumericUpDown
            {
                Location = new Point(300, 23),
                Size = new Size(60, 23),
                Minimum = 8,
                Maximum = 24,
                BackColor = Color.FromArgb(30, 30, 30),
                ForeColor = Color.White
            };

            chkFontBold = new CheckBox
            {
                Text = "Bold",
                Location = new Point(95, 52),
                Size = new Size(60, 20),
                ForeColor = Color.White
            };

            lblFontPreview = new Label
            {
                Text = "Sample Task Text Preview",
                Location = new Point(10, 80),
                Size = new Size(390, 30),
                ForeColor = Color.White,
                BorderStyle = BorderStyle.FixedSingle,
                TextAlign = ContentAlignment.MiddleLeft
            };

            grpFont.Controls.AddRange(new Control[] { lblFontFamily, cmbFontFamily, lblFontSize, nudFontSize, chkFontBold, lblFontPreview });

            // Color settings group
            var grpColor = new GroupBox
            {
                Text = "Color Settings",
                Location = new Point(12, 142),
                Size = new Size(410, 80),
                ForeColor = Color.White
            };

            var lblTextColor = new Label
            {
                Text = "Text Color:",
                Location = new Point(10, 25),
                Size = new Size(70, 20),
                ForeColor = Color.White
            };

            btnTextColor = new Button
            {
                Text = "Select Color",
                Location = new Point(85, 23),
                Size = new Size(100, 25),
                BackColor = Color.FromArgb(0, 122, 204),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat
            };
            btnTextColor.FlatAppearance.BorderSize = 0;

            pnlTextColorPreview = new Panel
            {
                Location = new Point(195, 23),
                Size = new Size(50, 25),
                BorderStyle = BorderStyle.FixedSingle
            };

            grpColor.Controls.AddRange(new Control[] { lblTextColor, btnTextColor, pnlTextColorPreview });

            // Background settings group
            var grpBackground = new GroupBox
            {
                Text = "Background Settings",
                Location = new Point(12, 232),
                Size = new Size(410, 80),
                ForeColor = Color.White
            };

            var lblBackground = new Label
            {
                Text = "Background:",
                Location = new Point(10, 25),
                Size = new Size(80, 20),
                ForeColor = Color.White
            };

            btnBackgroundImage = new Button
            {
                Text = "Select Image",
                Location = new Point(95, 23),
                Size = new Size(100, 25),
                BackColor = Color.FromArgb(0, 122, 204),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat
            };
            btnBackgroundImage.FlatAppearance.BorderSize = 0;

            var btnResetBackground = new Button
            {
                Text = "Reset Default",
                Location = new Point(205, 23),
                Size = new Size(100, 25),
                BackColor = Color.FromArgb(70, 70, 70),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat
            };
            btnResetBackground.FlatAppearance.BorderSize = 0;
            btnResetBackground.Click += (s, e) => {
                selectedBackgroundPath = "";
                MessageBox.Show("Background reset to default.", "Settings", MessageBoxButtons.OK, MessageBoxIcon.Information);
            };

            grpBackground.Controls.AddRange(new Control[] { lblBackground, btnBackgroundImage, btnResetBackground });

            // System settings group
            var grpSystem = new GroupBox
            {
                Text = "System Settings",
                Location = new Point(12, 322),
                Size = new Size(410, 80), // Reduced height since we removed the dropdown
                ForeColor = Color.White
            };

            chkMinimizeToTray = new CheckBox
            {
                Text = "Minimize to system tray",
                Location = new Point(10, 25),
                Size = new Size(180, 20),
                ForeColor = Color.White
            };

            chkStartMinimized = new CheckBox
            {
                Text = "Start minimized to tray",
                Location = new Point(200, 25),
                Size = new Size(180, 20),
                ForeColor = Color.White
            };

            var lblNote = new Label
            {
                Text = "Note: Use the dropdown button (▼) next to Quit/Hide to change behavior",
                Location = new Point(10, 50),
                Size = new Size(380, 20),
                ForeColor = Color.FromArgb(180, 180, 180),
                Font = new Font("Segoe UI", 8F, FontStyle.Italic)
            };

            grpSystem.Controls.AddRange(new Control[] { chkMinimizeToTray, chkStartMinimized, lblNote });

            // Buttons
            btnAbout = new Button
            {
                Text = "ℹ About",
                Location = new Point(105, 425), // Adjusted Y position back down
                Size = new Size(75, 30),
                BackColor = Color.FromArgb(85, 85, 85),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Cursor = Cursors.Hand
            };
            btnAbout.FlatAppearance.BorderSize = 0;

            // Add hover effect for About button
            btnAbout.MouseEnter += (s, e) => btnAbout.BackColor = Color.FromArgb(105, 105, 105);
            btnAbout.MouseLeave += (s, e) => btnAbout.BackColor = Color.FromArgb(85, 85, 85);

            btnOK = new Button
            {
                Text = "OK",
                Location = new Point(186, 425), // Adjusted Y position back down
                Size = new Size(75, 30),
                DialogResult = DialogResult.OK,
                BackColor = Color.FromArgb(0, 122, 204),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat
            };
            btnOK.FlatAppearance.BorderSize = 0;

            btnCancel = new Button
            {
                Text = "Cancel",
                Location = new Point(267, 425), // Adjusted Y position back down
                Size = new Size(75, 30),
                DialogResult = DialogResult.Cancel,
                BackColor = Color.FromArgb(70, 70, 70),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat
            };
            btnCancel.FlatAppearance.BorderSize = 0;

            btnReset = new Button
            {
                Text = "Reset All",
                Location = new Point(347, 425), // Adjusted Y position back down
                Size = new Size(75, 30),
                BackColor = Color.FromArgb(196, 43, 28),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat
            };
            btnReset.FlatAppearance.BorderSize = 0;

            this.Controls.AddRange(new Control[] { 
                grpFont, grpColor, grpBackground, grpSystem, 
                btnAbout, btnOK, btnCancel, btnReset 
            });

            this.AcceptButton = btnOK;
            this.CancelButton = btnCancel;

            // Event handlers
            btnTextColor.Click += BtnTextColor_Click;
            btnBackgroundImage.Click += BtnBackgroundImage_Click;
            btnReset.Click += BtnReset_Click;
            btnOK.Click += BtnOK_Click;
            btnAbout.Click += BtnAbout_Click;
            
            cmbFontFamily.SelectedIndexChanged += UpdateFontPreview;
            nudFontSize.ValueChanged += UpdateFontPreview;
            chkFontBold.CheckedChanged += UpdateFontPreview;

            LoadFontFamilies();
        }

        private void LoadFontFamilies()
        {
            var commonFonts = new[] {
                "Segoe UI", "Arial", "Calibri", "Consolas", "Courier New",
                "Georgia", "Times New Roman", "Trebuchet MS", "Verdana", "Ubuntu Mono"
            };

            foreach (var font in commonFonts)
            {
                cmbFontFamily!.Items.Add(font);
            }
        }

        private void LoadCurrentSettings()
        {
            cmbFontFamily!.Text = Settings.FontFamily;
            nudFontSize!.Value = (decimal)Settings.FontSize;
            chkFontBold!.Checked = Settings.FontBold;
            chkMinimizeToTray!.Checked = Settings.MinimizeToTray;
            chkStartMinimized!.Checked = Settings.StartMinimized;
            pnlTextColorPreview!.BackColor = selectedTextColor;
            UpdateFontPreview(null, null);
        }

        private void UpdateFontPreview(object? sender, EventArgs? e)
        {
            try
            {
                var fontStyle = chkFontBold!.Checked ? FontStyle.Bold : FontStyle.Regular;
                var font = new Font(cmbFontFamily!.Text, (float)nudFontSize!.Value, fontStyle);
                lblFontPreview!.Font = font;
                lblFontPreview.ForeColor = selectedTextColor;
            }
            catch
            {
                // Handle font creation errors
            }
        }

        private void BtnTextColor_Click(object? sender, EventArgs e)
        {
            using var colorDialog = new ColorDialog();
            colorDialog.Color = selectedTextColor;
            colorDialog.FullOpen = true;
            
            if (colorDialog.ShowDialog() == DialogResult.OK)
            {
                selectedTextColor = colorDialog.Color;
                pnlTextColorPreview!.BackColor = selectedTextColor;
                UpdateFontPreview(null, null);
            }
        }

        private void BtnBackgroundImage_Click(object? sender, EventArgs e)
        {
            using var openFileDialog = new OpenFileDialog();
            openFileDialog.Filter = "Image Files|*.jpg;*.jpeg;*.png;*.bmp;*.gif|All Files|*.*";
            openFileDialog.Title = "Select Background Image";
            
            if (openFileDialog.ShowDialog() == DialogResult.OK)
            {
                selectedBackgroundPath = openFileDialog.FileName;
                MessageBox.Show($"Background image selected: {Path.GetFileName(selectedBackgroundPath)}", 
                    "Settings", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
        }

        private void BtnReset_Click(object? sender, EventArgs e)
        {
            if (MessageBox.Show("Reset all settings to default values?", "Confirm Reset", 
                MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
            {
                Settings = new AppSettings();
                selectedTextColor = Settings.TextColor;
                selectedBackgroundPath = Settings.BackgroundImagePath;
                LoadCurrentSettings();
            }
        }

        private void BtnOK_Click(object? sender, EventArgs e)
        {
            Settings.FontFamily = cmbFontFamily!.Text;
            Settings.FontSize = (float)nudFontSize!.Value;
            Settings.FontBold = chkFontBold!.Checked;
            Settings.TextColor = selectedTextColor;
            Settings.BackgroundImagePath = selectedBackgroundPath;
            Settings.MinimizeToTray = chkMinimizeToTray!.Checked;
            Settings.StartMinimized = chkStartMinimized!.Checked;
            // DefaultQuitBehavior is now managed directly from the main form
        }

        private void BtnAbout_Click(object? sender, EventArgs e)
        {
            try
            {
                System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo
                {
                    FileName = "https://funtohard.online/about/todo",
                    UseShellExecute = true
                });
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Unable to open browser: {ex.Message}", "Error", 
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }
    }
}