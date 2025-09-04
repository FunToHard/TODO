using System.Text.Json;
using System.Runtime.InteropServices;

namespace TODO
{
    public partial class Form1 : Form
    {
        private readonly string tasksFile = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "tasks.json");
        private readonly string settingsFile = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "settings.json");
        private readonly string appName = "TODO";
        private AppSettings appSettings = null!;

        [DllImport("user32.dll")]
        public static extern int SendMessage(IntPtr hWnd, int Msg, int wParam, int lParam);
        [DllImport("user32.dll")]
        public static extern bool ReleaseCapture();

        public Form1()
        {
            LoadSettings();
            InitializeComponent();
            SetupDropdownButton(); // Changed from SetupDropdownQuitButton
            StyleControls();
            LoadTasks();
            ApplySettings();
            SetupSystemTray();
            transparentTaskList.SelectedIndexChanged += TransparentTaskList_SelectedIndexChanged;
            TryCreateStartupShortcut();

            // Setup event handlers
            btnSettings.Click += btnSettings_Click;
            
            // Ensure delete button click is properly connected
            btnDelete.Click += btnDelete_Click;

            // Enable dragging only from the form's background, not all controls
            this.MouseDown += Form1_MouseDown;
            
            // Handle minimize to tray
            this.Resize += Form1_Resize;
            this.FormClosing += Form1_FormClosing;
            
            // Save tasks when they are reordered via drag and drop
            transparentTaskList.MouseUp += (s, e) => SaveTasks();
            
            // Ensure tray icon is cleaned up on application exit
            Application.ApplicationExit += (s, e) => {
                if (notifyIcon != null)
                {
                    notifyIcon.Visible = false;
                    notifyIcon.Dispose();
                }
            };
        }

        private void LoadSettings()
        {
            try
            {
                if (File.Exists(settingsFile))
                {
                    var json = File.ReadAllText(settingsFile);
                    var options = new JsonSerializerOptions();
                    options.Converters.Add(new ColorJsonConverter());
                    appSettings = JsonSerializer.Deserialize<AppSettings>(json, options) ?? new AppSettings();
                }
                else
                {
                    appSettings = new AppSettings();
                }
            }
            catch
            {
                appSettings = new AppSettings();
            }
        }

        private void SaveSettings()
        {
            try
            {
                var options = new JsonSerializerOptions { WriteIndented = true };
                options.Converters.Add(new ColorJsonConverter());
                File.WriteAllText(settingsFile, JsonSerializer.Serialize(appSettings, options));
            }
            catch { /* Handle errors if needed */ }
        }

        private void ApplySettings()
        {
            // Apply font settings to task list
            var taskFont = appSettings.CreateFont();
            transparentTaskList.Font = taskFont;
            transparentTaskList.ForeColor = appSettings.TextColor;

            // Apply background image if specified
            if (!string.IsNullOrEmpty(appSettings.BackgroundImagePath) && File.Exists(appSettings.BackgroundImagePath))
            {
                try
                {
                    this.BackgroundImage = Image.FromFile(appSettings.BackgroundImagePath);
                }
                catch
                {
                    // If custom background fails, keep default
                }
            }

            // Update label styling
            label1.BackColor = Color.Transparent;
            label1.ForeColor = Color.White;
            
            // Update tray visibility based on settings
            UpdateTrayVisibility();
        }

        private void UpdateTrayVisibility()
        {
            // Show/hide tray icon based on MinimizeToTray setting
            if (appSettings.MinimizeToTray)
            {
                // Setup tray if not already done
                SetupSystemTray();
                notifyIcon.Visible = true;
                
                // Show dropdown button when tray is enabled
                btnDropdown.Visible = true;
            }
            else
            {
                notifyIcon.Visible = false;
                
                // Hide dropdown button when tray is disabled
                btnDropdown.Visible = false;
            }
            
            // Update quit button based on settings
            UpdateQuitButtonText();
        }

        private void SetupDropdownButton()
        {
            // Create tooltip for the dropdown button
            var tooltip = new ToolTip();
            tooltip.SetToolTip(btnDropdown, "Select quit/hide behavior");
            tooltip.SetToolTip(QuitButton, "Current action - click dropdown to change");
            
            // Create context menu for dropdown functionality
            var quitContextMenu = new ContextMenuStrip();
            quitContextMenu.BackColor = Color.FromArgb(45, 45, 48);
            quitContextMenu.ForeColor = Color.White;
            
            var hideItem = new ToolStripMenuItem("⏷ Hide to Tray");
            hideItem.ForeColor = Color.White;
            hideItem.Click += (s, e) => {
                // Set user preference to Hide
                appSettings.DefaultQuitBehavior = false;
                SaveSettings();
                UpdateQuitButtonText();
                
                // Show confirmation
                MessageBox.Show("Button behavior changed to 'Hide'. Now clicking the button will hide to tray.", 
                    "Behavior Changed", MessageBoxButtons.OK, MessageBoxIcon.Information);
            };
            
            var quitItem = new ToolStripMenuItem("✕ Quit Application");
            quitItem.ForeColor = Color.White;
            quitItem.Click += (s, e) => {
                // Set user preference to Quit
                appSettings.DefaultQuitBehavior = true;
                SaveSettings();
                UpdateQuitButtonText();
                
                // Show confirmation
                MessageBox.Show("Button behavior changed to 'Quit'. Now clicking the button will exit the application.", 
                    "Behavior Changed", MessageBoxButtons.OK, MessageBoxIcon.Information);
            };
            
            quitContextMenu.Items.Add(hideItem);
            quitContextMenu.Items.Add(new ToolStripSeparator());
            quitContextMenu.Items.Add(quitItem);
            
            // Add checkmarks to show current selection
            quitContextMenu.Opening += (s, e) => {
                hideItem.Checked = !appSettings.DefaultQuitBehavior; // false = Hide
                quitItem.Checked = appSettings.DefaultQuitBehavior;   // true = Quit
            };
            
            // Store the context menu so we can access it from the button click
            btnDropdown.Tag = quitContextMenu;
        }

        private void btnDropdown_Click(object sender, EventArgs e)
        {
            if (btnDropdown.Tag is ContextMenuStrip contextMenu)
            {
                contextMenu.Show(btnDropdown, new Point(0, btnDropdown.Height));
            }
        }

        private void UpdateQuitButtonText()
        {
            // Always show the user's current preference
            if (appSettings.MinimizeToTray)
            {
                // Show user's saved preference
                QuitButton.Text = appSettings.DefaultQuitBehavior ? "✕ Quit" : "⏷ Hide";
            }
            else
            {
                // When tray is disabled, always show Quit (but don't change the saved preference)
                QuitButton.Text = "✕ Quit";
            }
        }

        private void SetupSystemTray()
        {
            try
            {
                // Ensure the notifyIcon has an icon set
                if (notifyIcon.Icon == null)
                {
                    // Try to use the form's icon first
                    if (this.Icon != null)
                    {
                        notifyIcon.Icon = this.Icon;
                    }
                    else
                    {
                        // Fall back to system icon
                        notifyIcon.Icon = SystemIcons.Application;
                    }
                }
                
                notifyIcon.Text = "TODO App";
                
                // Remove any existing event handlers to prevent duplicates
                notifyIcon.Click -= notifyIcon_Click;
                
                // Setup context menu for system tray
                var contextMenu = new ContextMenuStrip();
                contextMenu.BackColor = Color.FromArgb(45, 45, 48);
                contextMenu.ForeColor = Color.White;
                
                // Add Show menu item
                var showItem = new ToolStripMenuItem("⬆ Show Window");
                showItem.ForeColor = Color.White;
                showItem.Click += (s, e) => ShowFromTray();
                contextMenu.Items.Add(showItem);
                
                // Add separator
                contextMenu.Items.Add(new ToolStripSeparator());
                
                // Add About menu item
                var aboutItem = new ToolStripMenuItem("ℹ About");
                aboutItem.ForeColor = Color.White;
                aboutItem.Click += (s, e) => OpenAboutPage();
                contextMenu.Items.Add(aboutItem);
                
                // Add separator
                contextMenu.Items.Add(new ToolStripSeparator());
                
                // Add Exit menu item (this actually exits the app)
                var exitItem = new ToolStripMenuItem("✕ Exit Application");
                exitItem.ForeColor = Color.White;
                exitItem.Click += (s, e) => ExitApplication();
                contextMenu.Items.Add(exitItem);
                
                notifyIcon.ContextMenuStrip = contextMenu;
                
                // Handle single click to show app (instead of double-click)
                notifyIcon.Click += notifyIcon_Click;
                
                // Show tray icon if minimize to tray is enabled
                notifyIcon.Visible = appSettings.MinimizeToTray;
                
                // Handle start minimized
                if (appSettings.StartMinimized)
                {
                    this.WindowState = FormWindowState.Minimized;
                    this.Load += (s, e) => {
                        this.Hide();
                        notifyIcon.Visible = true;
                    };
                }
            }
            catch (Exception ex)
            {
                // If tray setup fails, disable tray functionality
                MessageBox.Show($"System tray setup failed: {ex.Message}\nTray functionality will be disabled.", 
                    "Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                appSettings.MinimizeToTray = false;
                appSettings.StartMinimized = false;
            }
        }

        private void notifyIcon_Click(object? sender, EventArgs e)
        {
            // Check if it's a left mouse click (single click)
            if (e is MouseEventArgs mouseArgs && mouseArgs.Button == MouseButtons.Left)
            {
                ShowFromTray();
            }
        }

        private void OpenAboutPage()
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
                MessageBox.Show($"Unable to open about page: {ex.Message}", "Error", 
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

        private void Form1_Resize(object? sender, EventArgs e)
        {
            if (appSettings.MinimizeToTray && this.WindowState == FormWindowState.Minimized)
            {
                try
                {
                    this.Hide();
                    notifyIcon.Visible = true; // Ensure tray icon is visible when minimized
                    notifyIcon.ShowBalloonTip(2000, "TODO App", "Application minimized to tray", ToolTipIcon.Info);
                }
                catch (Exception ex)
                {
                    // If tray operation fails, restore the window
                    this.Show();
                    this.WindowState = FormWindowState.Normal;
                    MessageBox.Show($"Failed to minimize to tray: {ex.Message}", "Error", 
                        MessageBoxButtons.OK, MessageBoxIcon.Warning);
                }
            }
        }

        private void Form1_FormClosing(object? sender, FormClosingEventArgs e)
        {
            if (appSettings.MinimizeToTray && e.CloseReason == CloseReason.UserClosing)
            {
                e.Cancel = true;
                this.Hide();
                notifyIcon.Visible = true; // Ensure tray icon is visible when closing to tray
                notifyIcon.ShowBalloonTip(2000, "TODO App", "Application minimized to tray", ToolTipIcon.Info);
            }
            else if (e.CloseReason == CloseReason.ApplicationExitCall || 
                     e.CloseReason == CloseReason.FormOwnerClosing ||
                     e.CloseReason == CloseReason.TaskManagerClosing ||
                     e.CloseReason == CloseReason.WindowsShutDown)
            {
                // Ensure tray icon is properly cleaned up when actually exiting
                notifyIcon.Visible = false;
            }
        }

        private void ShowFromTray()
        {
            this.Show();
            this.WindowState = FormWindowState.Normal;
            this.BringToFront();
            this.Activate(); // Ensure the window gets focus
            // Keep tray icon visible - don't hide it when window is restored
        }

        private void StyleControls()
        {
            // Style the Add button
            btnAdd.FlatStyle = FlatStyle.Flat;
            btnAdd.FlatAppearance.BorderSize = 0;
            btnAdd.BackColor = Color.FromArgb(0, 122, 204);
            btnAdd.ForeColor = Color.White;
            btnAdd.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
            btnAdd.Text = "+ Add";
            btnAdd.Cursor = Cursors.Hand;

            // Add hover effect
            btnAdd.MouseEnter += (s, e) => btnAdd.BackColor = Color.FromArgb(0, 100, 180);
            btnAdd.MouseLeave += (s, e) => btnAdd.BackColor = Color.FromArgb(0, 122, 204);

            // Style the Delete button
            btnDelete.FlatStyle = FlatStyle.Flat;
            btnDelete.FlatAppearance.BorderSize = 0;
            btnDelete.BackColor = Color.FromArgb(196, 43, 28);
            btnDelete.ForeColor = Color.White;
            btnDelete.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
            btnDelete.Text = "🗑 Delete";
            btnDelete.Cursor = Cursors.Hand;

            // Add hover effect
            btnDelete.MouseEnter += (s, e) => btnDelete.BackColor = Color.FromArgb(170, 35, 20);
            btnDelete.MouseLeave += (s, e) => btnDelete.BackColor = Color.FromArgb(196, 43, 28);

            // Style the Quit button
            QuitButton.FlatStyle = FlatStyle.Flat;
            QuitButton.FlatAppearance.BorderSize = 0;
            QuitButton.BackColor = Color.FromArgb(70, 70, 70);
            QuitButton.ForeColor = Color.White;
            QuitButton.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
            QuitButton.Text = "✕ Quit";
            QuitButton.Cursor = Cursors.Hand;

            // Add hover effect
            QuitButton.MouseEnter += (s, e) => QuitButton.BackColor = Color.FromArgb(90, 90, 90);
            QuitButton.MouseLeave += (s, e) => QuitButton.BackColor = Color.FromArgb(70, 70, 70);

            // Style the Dropdown button
            btnDropdown.FlatStyle = FlatStyle.Flat;
            btnDropdown.FlatAppearance.BorderSize = 0;
            btnDropdown.BackColor = Color.FromArgb(70, 70, 70);
            btnDropdown.ForeColor = Color.White;
            btnDropdown.Font = new Font("Segoe UI", 8F, FontStyle.Bold);
            btnDropdown.Text = "▼";
            btnDropdown.Cursor = Cursors.Hand;

            // Add hover effect
            btnDropdown.MouseEnter += (s, e) => btnDropdown.BackColor = Color.FromArgb(90, 90, 90);
            btnDropdown.MouseLeave += (s, e) => btnDropdown.BackColor = Color.FromArgb(70, 70, 70);

            // Style the Settings button
            btnSettings.FlatStyle = FlatStyle.Flat;
            btnSettings.FlatAppearance.BorderSize = 0;
            btnSettings.BackColor = Color.FromArgb(85, 85, 85);
            btnSettings.ForeColor = Color.White;
            btnSettings.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
            btnSettings.Text = "⚙ Settings";
            btnSettings.Cursor = Cursors.Hand;

            // Add hover effect
            btnSettings.MouseEnter += (s, e) => btnSettings.BackColor = Color.FromArgb(105, 105, 105);
            btnSettings.MouseLeave += (s, e) => btnSettings.BackColor = Color.FromArgb(85, 85, 85);

            // Style the text box
            txtTask.BackColor = Color.FromArgb(30, 30, 30);
            txtTask.ForeColor = Color.White;
            txtTask.BorderStyle = BorderStyle.FixedSingle;
            txtTask.Font = new Font("Segoe UI", 11F);
        }

        private void btnSettings_Click(object? sender, EventArgs e)
        {
            using var settingsForm = new SettingsForm(appSettings);
            if (settingsForm.ShowDialog() == DialogResult.OK)
            {
                appSettings = settingsForm.Settings;
                ApplySettings();
                SaveSettings();
            }
        }

        protected override bool ProcessCmdKey(ref Message msg, Keys keyData)
        {
            switch (keyData)
            {
                case Keys.Control | Keys.N:
                    txtTask.Focus();
                    txtTask.SelectAll();
                    return true;
                case Keys.Control | Keys.S:
                    SaveTasks();
                    return true;
                case Keys.Escape:
                    if (txtTask.Focused)
                    {
                        txtTask.Clear();
                    }
                    else
                    {
                        // If not in text box, focus the task list for keyboard navigation
                        transparentTaskList.Focus();
                    }
                    return true;
                case Keys.Delete: // Add Delete key handling for the form
                    if (!txtTask.Focused) // Don't interfere with text input
                    {
                        PerformDeleteAction();
                        return true;
                    }
                    break;
                case Keys.Tab: // Tab to cycle focus
                    if (txtTask.Focused)
                    {
                        transparentTaskList.Focus();
                        return true;
                    }
                    else if (transparentTaskList.Focused)
                    {
                        btnAdd.Focus();
                        return true;
                    }
                    break;
                case Keys.Up:
                case Keys.Down:
                case Keys.Home:
                case Keys.End:
                    // If arrow keys are pressed and task list isn't focused, focus it
                    if (!transparentTaskList.Focused && !txtTask.Focused)
                    {
                        transparentTaskList.Focus();
                        // Let the task list handle the key
                        return transparentTaskList.ProcessCmdKey(ref msg, keyData);
                    }
                    break;
                case Keys.Control | Keys.Q:
                    // Use the user's default preference when tray is enabled
                    if (appSettings.MinimizeToTray)
                    {
                        if (appSettings.DefaultQuitBehavior)
                        {
                            // User prefers to quit
                            ExitApplication();
                        }
                        else
                        {
                            // User prefers to hide
                            this.Hide();
                            notifyIcon.Visible = true;
                            notifyIcon.ShowBalloonTip(2000, "TODO App", "Application minimized to tray. Use tray menu to exit.", ToolTipIcon.Info);
                        }
                    }
                    else
                    {
                        // If tray is disabled, always exit the application
                        ExitApplication();
                    }
                    return true;
                case Keys.F12: // F12 for settings
                    btnSettings_Click(this, EventArgs.Empty);
                    return true;
                case Keys.Control | Keys.T: // Ctrl+T to test tray functionality
                    TestTrayFunctionality();
                    return true;
                case Keys.Control | Keys.Shift | Keys.T: // Ctrl+Shift+T to force enable tray
                    ForceEnableTray();
                    return true;
                case Keys.Control | Keys.Up: // Move task up
                    if (transparentTaskList.Focused)
                    {
                        // Let the task list handle this and save tasks after
                        bool handled = transparentTaskList.ProcessCmdKey(ref msg, keyData);
                        if (handled) SaveTasks();
                        return handled;
                    }
                    break;
                case Keys.Control | Keys.Down: // Move task down
                    if (transparentTaskList.Focused)
                    {
                        // Let the task list handle this and save tasks after
                        bool handled = transparentTaskList.ProcessCmdKey(ref msg, keyData);
                        if (handled) SaveTasks();
                        return handled;
                    }
                    break;
            }
            return base.ProcessCmdKey(ref msg, keyData);
        }

        private void PerformDeleteAction()
        {
            try
            {
                // Debug information
                System.Diagnostics.Debug.WriteLine($"Delete action triggered. SelectedIndex: {transparentTaskList.SelectedIndex}, Tasks count: {transparentTaskList.Tasks.Count}");
                
                if (transparentTaskList.SelectedIndex >= 0 && transparentTaskList.SelectedIndex < transparentTaskList.Tasks.Count)
                {
                    // Store the task being deleted for debugging
                    string taskToDelete = transparentTaskList.Tasks[transparentTaskList.SelectedIndex];
                    System.Diagnostics.Debug.WriteLine($"Deleting task: '{taskToDelete}' at index {transparentTaskList.SelectedIndex}");
                    
                    transparentTaskList.RemoveSelectedTask();
                    SaveTasks();
                    
                    System.Diagnostics.Debug.WriteLine($"Task deleted successfully. New count: {transparentTaskList.Tasks.Count}");
                }
                else
                {
                    System.Diagnostics.Debug.WriteLine("Delete action triggered but no valid task selected");
                    // Don't show message for keyboard shortcut - it's less intrusive
                    // User can see that nothing happened by the lack of visual change
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error in PerformDeleteAction: {ex.Message}");
                MessageBox.Show($"Error deleting task: {ex.Message}", "Delete Error", 
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void TestTrayFunctionality()
        {
            try
            {
                // Check if minimize to tray is enabled
                if (!appSettings.MinimizeToTray)
                {
                    MessageBox.Show("Minimize to tray is disabled in settings. Please enable it first in Settings (F12).", 
                        "Tray Test", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    return;
                }

                // Check if icon is available
                if (notifyIcon.Icon == null)
                {
                    MessageBox.Show("Tray icon is null. Setting up default icon.", "Tray Test", 
                        MessageBoxButtons.OK, MessageBoxIcon.Information);
                    notifyIcon.Icon = SystemIcons.Application;
                }

                // Show detailed status
                string status = $"Tray Icon Status:\n" +
                               $"- Icon: {(notifyIcon.Icon != null ? "Available" : "Missing")}\n" +
                               $"- Context Menu: {(notifyIcon.ContextMenuStrip != null ? "Available" : "Missing")}\n" +
                               $"- Minimize to Tray: {appSettings.MinimizeToTray}\n" +
                               $"- Start Minimized: {appSettings.StartMinimized}\n" +
                               $"- Currently Visible: {notifyIcon.Visible}";

                MessageBox.Show(status, "Tray Status", MessageBoxButtons.OK, MessageBoxIcon.Information);

                // Test hiding to tray temporarily
                if (!notifyIcon.Visible)
                {
                    notifyIcon.Visible = true;
                }
                notifyIcon.ShowBalloonTip(3000, "Tray Test", "Tray functionality is working! The tray icon should remain visible.", ToolTipIcon.Info);
                
                // Briefly hide the window for 2 seconds then restore it
                this.Hide();
                System.Threading.Tasks.Task.Delay(2000).ContinueWith(_ => {
                    this.Invoke(() => {
                        this.Show();
                        this.WindowState = FormWindowState.Normal;
                        this.BringToFront();
                        this.Activate();
                        MessageBox.Show("Test complete! The tray icon should still be visible.", "Tray Test Complete", 
                            MessageBoxButtons.OK, MessageBoxIcon.Information);
                    });
                });
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Tray test failed: {ex.Message}\nStack trace: {ex.StackTrace}", "Tray Test Error", 
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void ForceEnableTray()
        {
            try
            {
                // Force enable tray settings
                appSettings.MinimizeToTray = true;
                SaveSettings();
                
                // Force setup system tray
                SetupSystemTray();
                
                // Show confirmation
                MessageBox.Show("Tray functionality has been force-enabled.\n" +
                               "Minimize to Tray: True\n" +
                               "Use Ctrl+T to test tray functionality.", 
                               "Tray Force Enabled", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Failed to force enable tray: {ex.Message}", "Error", 
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void Form1_MouseDown(object? sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                ReleaseCapture();
                SendMessage(this.Handle, 0xA1, 0x2, 0);
            }
        }

        private void TryCreateStartupShortcut()
        {
            string startupFolder = Environment.GetFolderPath(Environment.SpecialFolder.Startup);
            string shortcutPath = Path.Combine(startupFolder, appName + ".lnk");
            string exePath = Application.ExecutablePath;

            if (!File.Exists(shortcutPath))
            {
                try
                {
                    Type? wshShell = Type.GetTypeFromProgID("WScript.Shell");
                    if (wshShell != null)
                    {
                        object? shellObj = Activator.CreateInstance(wshShell);
                        if (shellObj != null)
                        {
                            dynamic shell = shellObj;
                            var shortcut = shell.CreateShortcut(shortcutPath);
                            shortcut.TargetPath = exePath;
                            shortcut.WorkingDirectory = Path.GetDirectoryName(exePath);
                            shortcut.WindowStyle = 1;
                            shortcut.Description = appName + " App";
                            shortcut.Save();
                        }
                    }
                }
                catch (Exception ex)
                {
                    _ = ex;
                }
            }
        }

        private void btnAdd_Click(object sender, EventArgs e)
        {
            if (!string.IsNullOrWhiteSpace(txtTask.Text))
            {
                var task = txtTask.Text.Trim();
                transparentTaskList.AddTask(task);
                txtTask.Clear();
                txtTask.Focus();
                SaveTasks();
            }
        }

        private void btnDelete_Click(object sender, EventArgs e)
        {
            try
            {
                // Debug information
                System.Diagnostics.Debug.WriteLine($"Delete button clicked. SelectedIndex: {transparentTaskList.SelectedIndex}, Tasks count: {transparentTaskList.Tasks.Count}");
                
                if (transparentTaskList.SelectedIndex >= 0 && transparentTaskList.SelectedIndex < transparentTaskList.Tasks.Count)
                {
                    // Use the centralized delete action
                    PerformDeleteAction();
                }
                else
                {
                    System.Diagnostics.Debug.WriteLine("Delete button clicked but no valid task selected");
                    // Show a message for button click since it's a deliberate action
                    MessageBox.Show("Please select a task to delete.", "No Task Selected", 
                        MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error in btnDelete_Click: {ex.Message}");
                MessageBox.Show($"Error deleting task: {ex.Message}", "Delete Error", 
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void SaveTasks()
        {
            try
            {
                File.WriteAllText(tasksFile, JsonSerializer.Serialize(transparentTaskList.Tasks, new JsonSerializerOptions { WriteIndented = true }));
            }
            catch { /* Handle errors if needed */ }
        }

        private void LoadTasks()
        {
            try
            {
                if (File.Exists(tasksFile))
                {
                    var loaded = JsonSerializer.Deserialize<List<string>>(File.ReadAllText(tasksFile));
                    if (loaded != null)
                    {
                        transparentTaskList.ClearTasks();
                        foreach (var task in loaded)
                        {
                            transparentTaskList.AddTask(task);
                        }
                        
                        // If tasks were loaded but no selection, select the first task
                        if (transparentTaskList.Tasks.Count > 0 && transparentTaskList.SelectedIndex < 0)
                        {
                            // Don't auto-select, let user select manually
                            // This ensures the delete button starts in the correct disabled state
                            System.Diagnostics.Debug.WriteLine($"Loaded {transparentTaskList.Tasks.Count} tasks, no auto-selection");
                        }
                    }
                }
                
                // Update delete button state after loading
                btnDelete.Enabled = transparentTaskList.SelectedIndex >= 0;
                System.Diagnostics.Debug.WriteLine($"After loading tasks: Delete button enabled = {btnDelete.Enabled}");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error loading tasks: {ex.Message}");
            }
        }

        private void TransparentTaskList_SelectedIndexChanged(object? sender, EventArgs e)
        {
            // Update delete button state
            bool hasSelection = transparentTaskList.SelectedIndex >= 0 && transparentTaskList.SelectedIndex < transparentTaskList.Tasks.Count;
            btnDelete.Enabled = hasSelection;
            
            // Debug information
            System.Diagnostics.Debug.WriteLine($"Selection changed. SelectedIndex: {transparentTaskList.SelectedIndex}, " +
                $"Tasks count: {transparentTaskList.Tasks.Count}, Delete button enabled: {btnDelete.Enabled}");
            
            if (hasSelection)
            {
                System.Diagnostics.Debug.WriteLine($"Selected task: '{transparentTaskList.Tasks[transparentTaskList.SelectedIndex]}'");
            }
        }

        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);
            
            // Initialize delete button state
            btnDelete.Enabled = transparentTaskList.SelectedIndex >= 0;
            
            txtTask.KeyPress += (s, e) =>
            {
                if (e.KeyChar == (char)Keys.Enter)
                {
                    e.Handled = true;
                    btnAdd.PerformClick();
                }
            };

            // Handle placeholder text behavior
            txtTask.GotFocus += (s, e) =>
            {
                if (txtTask.Text == "New task")
                {
                    txtTask.Text = "";
                    txtTask.ForeColor = Color.White;
                }
            };

            txtTask.LostFocus += (s, e) =>
            {
                if (string.IsNullOrWhiteSpace(txtTask.Text))
                {
                    txtTask.Text = "New task";
                    txtTask.ForeColor = Color.FromArgb(150, 150, 150);
                }
            };

            // Initialize placeholder
            txtTask.Text = "New task";
            txtTask.ForeColor = Color.FromArgb(150, 150, 150);
        }

        private void QuitButton_Click(object sender, EventArgs e)
        {
            // Use the user's default preference when tray is enabled
            if (appSettings.MinimizeToTray)
            {
                if (appSettings.DefaultQuitBehavior)
                {
                    // User prefers to quit
                    ExitApplication();
                }
                else
                {
                    // User prefers to hide
                    this.Hide();
                    notifyIcon.Visible = true;
                    notifyIcon.ShowBalloonTip(2000, "TODO App", "Application minimized to tray. Use tray menu to exit.", ToolTipIcon.Info);
                }
            }
            else
            {
                // If tray is disabled, always exit the application
                ExitApplication();
            }
        }

        private void ExitApplication()
        {
            // Hide tray icon and exit application properly
            notifyIcon.Visible = false;
            notifyIcon.Dispose();
            Application.Exit();
        }
    }

    // JSON converter for Color serialization
    public class ColorJsonConverter : System.Text.Json.Serialization.JsonConverter<Color>
    {
        public override Color Read(ref System.Text.Json.Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            var colorString = reader.GetString();
            if (string.IsNullOrEmpty(colorString))
                return Color.SpringGreen;
            
            return ColorTranslator.FromHtml(colorString);
        }

        public override void Write(System.Text.Json.Utf8JsonWriter writer, Color value, JsonSerializerOptions options)
        {
            writer.WriteStringValue(ColorTranslator.ToHtml(value));
        }
    }
}
