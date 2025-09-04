namespace TODO
{
    partial class Form1
    {
        /// <summary>
        ///  Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        ///  Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (components != null)
                {
                    components.Dispose();
                }
                // Ensure tray icon is properly disposed
                if (notifyIcon != null)
                {
                    notifyIcon.Visible = false;
                    notifyIcon.Dispose();
                }
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        ///  Required method for Designer support - do not modify
        ///  the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            components = new System.ComponentModel.Container();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(Form1));
            txtTask = new TextBox();
            btnAdd = new Button();
            transparentTaskList = new TransparentTaskListControl();
            btnDelete = new Button();
            label1 = new Label();
            QuitButton = new Button();
            btnDropdown = new Button();
            btnSettings = new Button();
            notifyIcon = new NotifyIcon(components);
            SuspendLayout();
            // 
            // txtTask
            // 
            txtTask.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            txtTask.BackColor = Color.FromArgb(30, 30, 30);
            txtTask.BorderStyle = BorderStyle.FixedSingle;
            txtTask.Font = new Font("Segoe UI", 11.25F, FontStyle.Regular, GraphicsUnit.Point, 0);
            txtTask.ForeColor = Color.White;
            txtTask.Location = new Point(262, 84);
            txtTask.Name = "txtTask";
            txtTask.Size = new Size(284, 27);
            txtTask.TabIndex = 0;
            txtTask.Text = "New task";
            // 
            // btnAdd
            // 
            btnAdd.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            btnAdd.BackColor = Color.FromArgb(0, 122, 204);
            btnAdd.FlatAppearance.BorderSize = 0;
            btnAdd.FlatStyle = FlatStyle.Flat;
            btnAdd.Font = new Font("Segoe UI", 9F, FontStyle.Bold, GraphicsUnit.Point, 0);
            btnAdd.ForeColor = Color.White;
            btnAdd.Location = new Point(552, 83);
            btnAdd.Name = "btnAdd";
            btnAdd.Size = new Size(80, 28);
            btnAdd.TabIndex = 1;
            btnAdd.Text = "+ Add";
            btnAdd.UseVisualStyleBackColor = false;
            btnAdd.Click += btnAdd_Click;
            // 
            // transparentTaskList
            // 
            transparentTaskList.AllowDrop = true;
            transparentTaskList.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            transparentTaskList.AutoScroll = true;
            transparentTaskList.BackColor = Color.Transparent;
            transparentTaskList.Font = new Font("Segoe UI", 11F, FontStyle.Regular, GraphicsUnit.Point, 0);
            transparentTaskList.ForeColor = Color.SpringGreen;
            transparentTaskList.Location = new Point(262, 125);
            transparentTaskList.Name = "transparentTaskList";
            transparentTaskList.Size = new Size(460, 336);
            transparentTaskList.TabIndex = 2;
            transparentTaskList.TabStop = true;
            // 
            // btnDelete
            // 
            btnDelete.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;
            btnDelete.BackColor = Color.FromArgb(196, 43, 28);
            btnDelete.FlatAppearance.BorderSize = 0;
            btnDelete.FlatStyle = FlatStyle.Flat;
            btnDelete.Font = new Font("Segoe UI", 9F, FontStyle.Bold, GraphicsUnit.Point, 0);
            btnDelete.ForeColor = Color.White;
            btnDelete.Location = new Point(647, 467);
            btnDelete.Name = "btnDelete";
            btnDelete.Size = new Size(75, 28);
            btnDelete.TabIndex = 3;
            btnDelete.Text = "🗑 Delete";
            btnDelete.UseVisualStyleBackColor = false;
            btnDelete.Click += btnDelete_Click;
            // 
            // label1
            // 
            label1.AutoSize = true;
            label1.BackColor = Color.Transparent;
            label1.Font = new Font("Segoe UI", 9F, FontStyle.Bold, GraphicsUnit.Point, 0);
            label1.ForeColor = Color.FromArgb(220, 220, 220);
            label1.Location = new Point(262, 65);
            label1.Name = "label1";
            label1.Size = new Size(63, 15);
            label1.TabIndex = 4;
            label1.Text = "New Task:";
            // 
            // QuitButton
            // 
            QuitButton.Anchor = AnchorStyles.Bottom | AnchorStyles.Left;
            QuitButton.BackColor = Color.FromArgb(70, 70, 70);
            QuitButton.FlatAppearance.BorderSize = 0;
            QuitButton.FlatStyle = FlatStyle.Flat;
            QuitButton.Font = new Font("Segoe UI", 9F, FontStyle.Bold, GraphicsUnit.Point, 0);
            QuitButton.ForeColor = Color.White;
            QuitButton.Location = new Point(262, 467);
            QuitButton.Name = "QuitButton";
            QuitButton.Size = new Size(60, 28);
            QuitButton.TabIndex = 5;
            QuitButton.Text = "✕ Quit";
            QuitButton.UseVisualStyleBackColor = false;
            QuitButton.Click += QuitButton_Click;
            // 
            // btnDropdown
            // 
            btnDropdown.Anchor = AnchorStyles.Bottom | AnchorStyles.Left;
            btnDropdown.BackColor = Color.FromArgb(70, 70, 70);
            btnDropdown.FlatAppearance.BorderSize = 0;
            btnDropdown.FlatStyle = FlatStyle.Flat;
            btnDropdown.Font = new Font("Segoe UI", 8F, FontStyle.Bold, GraphicsUnit.Point, 0);
            btnDropdown.ForeColor = Color.White;
            btnDropdown.Location = new Point(328, 467);
            btnDropdown.Name = "btnDropdown";
            btnDropdown.Size = new Size(27, 28);
            btnDropdown.TabIndex = 7;
            btnDropdown.Text = "▼";
            btnDropdown.UseVisualStyleBackColor = false;
            btnDropdown.Click += btnDropdown_Click;
            // 
            // btnSettings
            // 
            btnSettings.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            btnSettings.BackColor = Color.FromArgb(85, 85, 85);
            btnSettings.FlatAppearance.BorderSize = 0;
            btnSettings.FlatStyle = FlatStyle.Flat;
            btnSettings.Font = new Font("Segoe UI", 9F, FontStyle.Bold, GraphicsUnit.Point, 0);
            btnSettings.ForeColor = Color.White;
            btnSettings.Location = new Point(642, 83);
            btnSettings.Name = "btnSettings";
            btnSettings.Size = new Size(80, 28);
            btnSettings.TabIndex = 7;
            btnSettings.Text = "⚙ Settings";
            btnSettings.UseVisualStyleBackColor = false;
            // 
            // notifyIcon
            // 
            notifyIcon.Icon = (Icon)resources.GetObject("notifyIcon.Icon");
            notifyIcon.Text = "TODO App";
            // 
            // Form1
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            BackgroundImage = (Image)resources.GetObject("$this.BackgroundImage");
            BackgroundImageLayout = ImageLayout.Zoom;
            ClientSize = new Size(977, 533);
            Controls.Add(btnSettings);
            Controls.Add(btnDropdown);
            Controls.Add(QuitButton);
            Controls.Add(label1);
            Controls.Add(btnDelete);
            Controls.Add(transparentTaskList);
            Controls.Add(btnAdd);
            Controls.Add(txtTask);
            DoubleBuffered = true;
            FormBorderStyle = FormBorderStyle.None;
            Icon = (Icon)resources.GetObject("$this.Icon");
            KeyPreview = true;
            MinimumSize = new Size(600, 400);
            Name = "Form1";
            StartPosition = FormStartPosition.CenterScreen;
            Text = "TODO App";
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private TextBox txtTask;
        private Button btnAdd;
        private TransparentTaskListControl transparentTaskList;
        private Button btnDelete;
        private Label label1;
        private Button QuitButton;
        private Button btnDropdown;
        private Button btnSettings;
        private NotifyIcon notifyIcon;
    }
}
