using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;

namespace TODO
{
    public class TransparentTaskListControl : Panel
    {
        public List<string> Tasks { get; } = new List<string>();
        public int SelectedIndex { get; private set; } = -1;
        private int hoverIndex = -1;
        public event EventHandler? SelectedIndexChanged;
        private ContextMenuStrip? contextMenu;

        // Drag and drop fields
        private bool isDragging = false;
        private int dragStartIndex = -1;
        private int dragTargetIndex = -1;
        private Point dragStartPoint;
        private System.Windows.Forms.Timer dragScrollTimer;
        private const int DragScrollDelay = 100;
        private const int DragThreshold = 5;

        // Track the last scroll position to detect changes
        private Point lastScrollPosition = Point.Empty;

        public TransparentTaskListControl()
        {
            SetStyle(ControlStyles.SupportsTransparentBackColor | 
                    ControlStyles.UserPaint | 
                    ControlStyles.AllPaintingInWmPaint | 
                    ControlStyles.OptimizedDoubleBuffer |
                    ControlStyles.Selectable, true);
            BackColor = Color.Transparent;
            DoubleBuffered = true;
            Font = new Font("Segoe UI", 11F, FontStyle.Regular);
            ForeColor = Color.White;
            TabStop = true;
            
            // Enable auto scroll
            AutoScroll = true;
            
            // Enable drag and drop
            this.AllowDrop = true;
            
            // Initialize drag scroll timer
            dragScrollTimer = new System.Windows.Forms.Timer();
            dragScrollTimer.Interval = DragScrollDelay;
            dragScrollTimer.Tick += DragScrollTimer_Tick;
            
            InitializeContextMenu();
        }

        private void DragScrollTimer_Tick(object? sender, EventArgs e)
        {
            if (!isDragging) return;

            Point clientPoint = this.PointToClient(Control.MousePosition);
            int scrollAmount = 10;
            Point oldScrollPosition = AutoScrollPosition;

            if (clientPoint.Y < 20 && AutoScrollPosition.Y < 0)
            {
                // Scroll up
                AutoScrollPosition = new Point(0, Math.Min(0, AutoScrollPosition.Y + scrollAmount));
                UpdateDragTarget(clientPoint);
            }
            else if (clientPoint.Y > Height - 20)
            {
                // Scroll down
                int maxScroll = Math.Max(0, (Tasks.Count * 35) - Height);
                AutoScrollPosition = new Point(0, Math.Max(-maxScroll, AutoScrollPosition.Y - scrollAmount));
                UpdateDragTarget(clientPoint);
            }
            
            // If scroll position changed, invalidate parent and this control
            if (AutoScrollPosition != oldScrollPosition)
            {
                if (Parent != null)
                {
                    Parent.Invalidate();
                }
                Invalidate();
            }
        }

        private void InitializeContextMenu()
        {
            contextMenu = new ContextMenuStrip();
            contextMenu.BackColor = Color.FromArgb(45, 45, 48);
            contextMenu.ForeColor = Color.White;
            
            var editItem = new ToolStripMenuItem("✏️ Edit Task");
            editItem.Click += EditTask;
            contextMenu.Items.Add(editItem);
            
            var deleteItem = new ToolStripMenuItem("🗑️ Delete Task");
            deleteItem.Click += (s, e) => RemoveSelectedTask();
            contextMenu.Items.Add(deleteItem);
            
            contextMenu.Items.Add(new ToolStripSeparator());
            
            var clearAllItem = new ToolStripMenuItem("🧹 Clear All");
            clearAllItem.Click += (s, e) => ClearTasks();
            contextMenu.Items.Add(clearAllItem);
            
            this.ContextMenuStrip = contextMenu;
        }

        private void EditTask(object? sender, EventArgs e)
        {
            if (SelectedIndex >= 0 && SelectedIndex < Tasks.Count)
            {
                string currentTask = Tasks[SelectedIndex];
                using var dialog = new TaskEditDialog(currentTask);
                if (dialog.ShowDialog() == DialogResult.OK)
                {
                    Tasks[SelectedIndex] = dialog.TaskText;
                    Invalidate();
                }
            }
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            // Check if scroll position has changed since last paint
            if (AutoScrollPosition != lastScrollPosition)
            {
                lastScrollPosition = AutoScrollPosition;
                // Invalidate parent to refresh background when scroll position changes
                if (Parent != null)
                {
                    Parent.Invalidate();
                }
            }

            base.OnPaint(e);
            var g = e.Graphics;
            g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;
            g.TextRenderingHint = System.Drawing.Text.TextRenderingHint.ClearTypeGridFit;
            
            int itemHeight = 35; // Increased height for better readability
            
            // Calculate the scroll offset
            int scrollOffset = AutoScrollPosition.Y;
            
            // Update AutoScrollMinSize to enable scrolling when content exceeds visible area
            AutoScrollMinSize = new Size(0, Tasks.Count * itemHeight);
            
            for (int i = 0; i < Tasks.Count; i++)
            {
                // Apply scroll offset to rectangle position
                var rect = new Rectangle(0, i * itemHeight + scrollOffset, Width, itemHeight);
                var textRect = new Rectangle(rect.X + 15, rect.Y, rect.Width - 30, rect.Height);
                
                // Skip drawing items that are not visible
                if (rect.Bottom < 0 || rect.Top > Height)
                    continue;
                
                // Skip drawing the item being dragged
                if (isDragging && i == dragStartIndex)
                    continue;
                
                // Draw drag target indicator
                if (isDragging && i == dragTargetIndex)
                {
                    using var dropBrush = new SolidBrush(Color.FromArgb(100, 0, 255, 0));
                    using var dropPen = new Pen(Color.FromArgb(200, 0, 255, 0), 2);
                    g.FillRectangle(dropBrush, rect);
                    g.DrawRectangle(dropPen, rect.X, rect.Y, rect.Width - 1, rect.Height - 1);
                }
                
                // Draw hover effect
                if (i == hoverIndex && i != SelectedIndex && !isDragging)
                {
                    using var hoverBrush = new SolidBrush(Color.FromArgb(60, 255, 255, 255));
                    using var hoverPen = new Pen(Color.FromArgb(100, 255, 255, 255), 1);
                    g.FillRectangle(hoverBrush, rect);
                    g.DrawRectangle(hoverPen, rect.X, rect.Y, rect.Width - 1, rect.Height - 1);
                }
                
                // Draw selection highlight
                if (i == SelectedIndex && !isDragging)
                {
                    using var selBrush = new SolidBrush(Color.FromArgb(120, 0, 122, 204));
                    using var borderPen = new Pen(Color.FromArgb(200, 0, 153, 255), 2);
                    g.FillRectangle(selBrush, rect);
                    g.DrawRectangle(borderPen, rect.X, rect.Y, rect.Width - 1, rect.Height - 1);
                }
                
                // Draw task number
                var numberRect = new Rectangle(rect.X + 5, rect.Y, 20, rect.Height);
                using var numberBrush = new SolidBrush(Color.FromArgb(150, 255, 255, 255));
                using var numberFont = new Font("Segoe UI", 8F, FontStyle.Bold);
                TextRenderer.DrawText(g, (i + 1).ToString(), numberFont, numberRect, 
                    Color.FromArgb(180, 255, 255, 255), TextFormatFlags.Left | TextFormatFlags.VerticalCenter);
                
                // Draw task text with ellipsis if too long
                var taskColor = i == SelectedIndex ? Color.White : ForeColor;
                TextRenderer.DrawText(g, Tasks[i], Font, textRect, taskColor, 
                    TextFormatFlags.Left | TextFormatFlags.VerticalCenter | TextFormatFlags.EndEllipsis);
                
                // Draw drag handle indicator (subtle grip dots)
                if (!isDragging || i != dragStartIndex)
                {
                    DrawDragHandle(g, new Rectangle(rect.Width - 15, rect.Y + 10, 10, rect.Height - 20));
                }
            }
            
            // Draw the dragged item at mouse position
            if (isDragging && dragStartIndex >= 0 && dragStartIndex < Tasks.Count)
            {
                Point mousePos = this.PointToClient(Control.MousePosition);
                var dragRect = new Rectangle(mousePos.X - Width / 2, mousePos.Y - 17, Width - 20, 35);
                
                // Draw dragged item with semi-transparent background
                using var dragBrush = new SolidBrush(Color.FromArgb(180, 0, 122, 204));
                using var dragPen = new Pen(Color.FromArgb(255, 0, 153, 255), 2);
                g.FillRectangle(dragBrush, dragRect);
                g.DrawRectangle(dragPen, dragRect);
                
                // Draw the task text
                var dragTextRect = new Rectangle(dragRect.X + 15, dragRect.Y, dragRect.Width - 30, dragRect.Height);
                TextRenderer.DrawText(g, Tasks[dragStartIndex], Font, dragTextRect, Color.White, 
                    TextFormatFlags.Left | TextFormatFlags.VerticalCenter | TextFormatFlags.EndEllipsis);
            }
            
            // Draw focus indicator
            if (Focused && SelectedIndex >= 0 && !isDragging)
            {
                var focusRect = new Rectangle(2, SelectedIndex * itemHeight + scrollOffset + 2, Width - 4, itemHeight - 4);
                // Only draw focus rectangle if it's visible
                if (focusRect.Bottom > 0 && focusRect.Top < Height)
                {
                    ControlPaint.DrawFocusRectangle(g, focusRect);
                }
            }
        }

        private void DrawDragHandle(Graphics g, Rectangle rect)
        {
            using var handleBrush = new SolidBrush(Color.FromArgb(80, 255, 255, 255));
            int dotSize = 2;
            int spacing = 4;
            
            for (int i = 0; i < 3; i++)
            {
                for (int j = 0; j < 2; j++)
                {
                    int x = rect.X + j * spacing;
                    int y = rect.Y + i * spacing;
                    g.FillEllipse(handleBrush, x, y, dotSize, dotSize);
                }
            }
        }

        protected override void OnMouseMove(MouseEventArgs e)
        {
            base.OnMouseMove(e);
            
            if (isDragging)
            {
                UpdateDragTarget(e.Location);
                
                // Auto-scroll when dragging near edges
                if (e.Y < 20 || e.Y > Height - 20)
                {
                    if (!dragScrollTimer.Enabled)
                        dragScrollTimer.Start();
                }
                else
                {
                    dragScrollTimer.Stop();
                }
                
                Invalidate();
                return;
            }
            
            int itemHeight = 35;
            // Account for scroll position when calculating item index
            int adjustedY = e.Y - AutoScrollPosition.Y;
            int newHoverIndex = adjustedY / itemHeight;
            
            if (newHoverIndex >= 0 && newHoverIndex < Tasks.Count && newHoverIndex != hoverIndex)
            {
                hoverIndex = newHoverIndex;
                Invalidate();
                
                // Show different cursor based on position
                if (e.X > Width - 20)
                {
                    Cursor = Cursors.SizeAll; // Drag cursor near the right edge
                }
                else
                {
                    Cursor = Cursors.Hand;
                }
            }
            else if (newHoverIndex >= Tasks.Count || newHoverIndex < 0)
            {
                if (hoverIndex != -1)
                {
                    hoverIndex = -1;
                    Invalidate();
                }
                Cursor = Cursors.Default;
            }
            
            // Check if we should start dragging
            if (e.Button == MouseButtons.Left && !isDragging && hoverIndex >= 0)
            {
                var distance = Math.Sqrt(Math.Pow(e.X - dragStartPoint.X, 2) + Math.Pow(e.Y - dragStartPoint.Y, 2));
                if (distance > DragThreshold)
                {
                    StartDrag(hoverIndex);
                }
            }
        }

        private void UpdateDragTarget(Point location)
        {
            int itemHeight = 35;
            int adjustedY = location.Y - AutoScrollPosition.Y;
            int targetIndex = adjustedY / itemHeight;
            
            // Clamp to valid range
            targetIndex = Math.Max(0, Math.Min(Tasks.Count - 1, targetIndex));
            
            if (targetIndex != dragTargetIndex)
            {
                dragTargetIndex = targetIndex;
                Invalidate();
            }
        }

        private void StartDrag(int index)
        {
            if (index >= 0 && index < Tasks.Count)
            {
                isDragging = true;
                dragStartIndex = index;
                dragTargetIndex = index;
                SelectItem(index);
                this.Capture = true;
                Cursor = Cursors.SizeAll;
                Invalidate();
            }
        }

        private void EndDrag()
        {
            if (isDragging && dragStartIndex >= 0 && dragTargetIndex >= 0 && 
                dragStartIndex != dragTargetIndex && dragStartIndex < Tasks.Count && dragTargetIndex < Tasks.Count)
            {
                // Perform the move
                var task = Tasks[dragStartIndex];
                Tasks.RemoveAt(dragStartIndex);
                Tasks.Insert(dragTargetIndex, task);
                
                // Update selection to follow the moved item
                SelectedIndex = dragTargetIndex;
                SelectedIndexChanged?.Invoke(this, EventArgs.Empty);
            }
            
            isDragging = false;
            dragStartIndex = -1;
            dragTargetIndex = -1;
            dragScrollTimer.Stop();
            this.Capture = false;
            Cursor = Cursors.Default;
            Invalidate();
        }

        protected override void OnMouseLeave(EventArgs e)
        {
            base.OnMouseLeave(e);
            if (!isDragging)
            {
                if (hoverIndex != -1)
                {
                    hoverIndex = -1;
                    Invalidate();
                }
                Cursor = Cursors.Default;
            }
        }

        protected override void OnMouseDown(MouseEventArgs e)
        {
            base.OnMouseDown(e);
            Focus(); // Ensure this control gets focus when clicked
            int itemHeight = 35;
            // Account for scroll position when calculating clicked item index
            int adjustedY = e.Y - AutoScrollPosition.Y;
            int idx = adjustedY / itemHeight;
            
            if (e.Button == MouseButtons.Left)
            {
                dragStartPoint = e.Location;
                SelectItem(idx);
            }
        }

        protected override void OnMouseUp(MouseEventArgs e)
        {
            base.OnMouseUp(e);
            
            if (e.Button == MouseButtons.Left && isDragging)
            {
                EndDrag();
            }
        }

        private void SelectItem(int index)
        {
            if (index >= 0 && index < Tasks.Count)
            {
                if (SelectedIndex != index) // Only update if selection actually changed
                {
                    SelectedIndex = index;
                    SelectedIndexChanged?.Invoke(this, EventArgs.Empty);
                    System.Diagnostics.Debug.WriteLine($"Task selected: Index {index}, Task: '{Tasks[index]}'");
                }
                Invalidate();
            }
            else if (index < 0 || index >= Tasks.Count)
            {
                // Clicked outside valid range, clear selection
                if (SelectedIndex != -1)
                {
                    SelectedIndex = -1;
                    SelectedIndexChanged?.Invoke(this, EventArgs.Empty);
                    System.Diagnostics.Debug.WriteLine("Selection cleared - clicked outside task area");
                }
                Invalidate();
            }
        }

        public new bool ProcessCmdKey(ref Message msg, Keys keyData)
        {
            // Don't process keys while dragging
            if (isDragging) return false;
            
            switch (keyData)
            {
                case Keys.Up:
                    if (SelectedIndex > 0)
                    {
                        SelectItem(SelectedIndex - 1);
                        EnsureVisible(SelectedIndex);
                        return true;
                    }
                    break;
                case Keys.Down:
                    if (SelectedIndex < Tasks.Count - 1)
                    {
                        SelectItem(SelectedIndex + 1);
                        EnsureVisible(SelectedIndex);
                        return true;
                    }
                    break;
                case Keys.Home:
                    if (Tasks.Count > 0)
                    {
                        SelectItem(0);
                        EnsureVisible(0);
                        return true;
                    }
                    break;
                case Keys.End:
                    if (Tasks.Count > 0)
                    {
                        SelectItem(Tasks.Count - 1);
                        EnsureVisible(Tasks.Count - 1);
                        return true;
                    }
                    break;
                case Keys.Delete:
                    if (SelectedIndex >= 0)
                    {
                        RemoveSelectedTask();
                        return true;
                    }
                    break;
                case Keys.F2:
                    if (SelectedIndex >= 0)
                    {
                        EditTask(this, EventArgs.Empty);
                        return true;
                    }
                    break;
                // New keyboard shortcuts for moving tasks
                case Keys.Control | Keys.Up:
                    if (SelectedIndex > 0)
                    {
                        MoveTask(SelectedIndex, SelectedIndex - 1);
                        return true;
                    }
                    break;
                case Keys.Control | Keys.Down:
                    if (SelectedIndex >= 0 && SelectedIndex < Tasks.Count - 1)
                    {
                        MoveTask(SelectedIndex, SelectedIndex + 1);
                        return true;
                    }
                    break;
            }
            return base.ProcessCmdKey(ref msg, keyData);
        }

        private void MoveTask(int fromIndex, int toIndex)
        {
            if (fromIndex >= 0 && fromIndex < Tasks.Count && toIndex >= 0 && toIndex < Tasks.Count && fromIndex != toIndex)
            {
                var task = Tasks[fromIndex];
                Tasks.RemoveAt(fromIndex);
                Tasks.Insert(toIndex, task);
                SelectedIndex = toIndex;
                SelectedIndexChanged?.Invoke(this, EventArgs.Empty);
                EnsureVisible(toIndex);
                Invalidate();
            }
        }

        private void EnsureVisible(int index)
        {
            if (index < 0 || index >= Tasks.Count) return;
            
            int itemHeight = 35;
            int itemTop = index * itemHeight;
            int itemBottom = itemTop + itemHeight;
            
            Point oldScrollPosition = AutoScrollPosition;
            
            if (itemTop < AutoScrollPosition.Y * -1)
            {
                AutoScrollPosition = new Point(0, itemTop);
            }
            else if (itemBottom > Height + AutoScrollPosition.Y * -1)
            {
                AutoScrollPosition = new Point(0, itemBottom - Height);
            }
            
            // If scroll position changed, invalidate parent to refresh background
            if (AutoScrollPosition != oldScrollPosition && Parent != null)
            {
                Parent.Invalidate();
                Invalidate();
            }
        }

        public void AddTask(string task)
        {
            Tasks.Add(task);
            if (SelectedIndex == -1)
            {
                SelectItem(Tasks.Count - 1);
            }
            else
            {
                Invalidate();
            }
            EnsureVisible(Tasks.Count - 1);
        }

        public void RemoveSelectedTask()
        {
            if (SelectedIndex >= 0 && SelectedIndex < Tasks.Count)
            {
                string removedTask = Tasks[SelectedIndex];
                int oldIndex = SelectedIndex;
                Tasks.RemoveAt(SelectedIndex);
                
                System.Diagnostics.Debug.WriteLine($"Removed task: '{removedTask}' from index {oldIndex}. Remaining tasks: {Tasks.Count}");
                
                // Adjust selection after removal
                if (Tasks.Count > 0)
                {
                    // Keep the same index if possible, otherwise select the last item
                    int newIndex = oldIndex;
                    if (newIndex >= Tasks.Count)
                    {
                        newIndex = Tasks.Count - 1;
                    }
                    SelectedIndex = newIndex;
                    SelectedIndexChanged?.Invoke(this, EventArgs.Empty);
                    System.Diagnostics.Debug.WriteLine($"New selection after removal: Index {newIndex}, Task: '{Tasks[newIndex]}'");
                }
                else
                {
                    // No tasks left, clear selection
                    SelectedIndex = -1;
                    SelectedIndexChanged?.Invoke(this, EventArgs.Empty);
                    System.Diagnostics.Debug.WriteLine("No tasks remaining, selection cleared");
                }
                
                Invalidate();
            }
            else
            {
                System.Diagnostics.Debug.WriteLine($"RemoveSelectedTask called but no valid selection. SelectedIndex: {SelectedIndex}, Tasks.Count: {Tasks.Count}");
            }
        }

        public void ClearTasks()
        {
            Tasks.Clear();
            SelectedIndex = -1;
            hoverIndex = -1;
            isDragging = false;
            dragStartIndex = -1;
            dragTargetIndex = -1;
            dragScrollTimer.Stop();
            Invalidate();
        }

        protected override void OnEnter(EventArgs e)
        {
            base.OnEnter(e);
            if (SelectedIndex == -1 && Tasks.Count > 0)
            {
                SelectItem(0);
            }
            Invalidate();
        }

        protected override void OnLeave(EventArgs e)
        {
            base.OnLeave(e);
            Invalidate();
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                dragScrollTimer?.Dispose();
            }
            base.Dispose(disposing);
        }

        protected override void OnScroll(ScrollEventArgs se)
        {
            base.OnScroll(se);
            
            // Invalidate the parent form to refresh the background image
            // This fixes the background stretching issue when scrolling without mouse movement
            if (Parent != null)
            {
                Parent.Invalidate();
            }
            
            // Also invalidate this control
            Invalidate();
        }

        protected override void OnMouseWheel(MouseEventArgs e)
        {
            base.OnMouseWheel(e);
            
            // Invalidate the parent form to refresh the background image
            // This ensures immediate refresh when using mouse wheel
            if (Parent != null)
            {
                Parent.Invalidate();
            }
            
            // Also invalidate this control
            Invalidate();
        }

        protected override void WndProc(ref Message m)
        {
            base.WndProc(ref m);
            
            // Handle scroll messages to ensure background refresh
            const int WM_HSCROLL = 0x114;
            const int WM_VSCROLL = 0x115;
            
            if (m.Msg == WM_HSCROLL || m.Msg == WM_VSCROLL)
            {
                if (Parent != null)
                {
                    Parent.Invalidate();
                }
                Invalidate();
            }
        }
    }

    // Simple dialog for editing tasks
    public class TaskEditDialog : Form
    {
        private TextBox? textBox;
        private Button? okButton;
        private Button? cancelButton;

        public string TaskText => textBox!.Text.Trim();

        public TaskEditDialog(string currentText)
        {
            InitializeComponent();
            if (textBox != null)
            {
                textBox.Text = currentText;
                textBox.SelectAll();
            }
        }

        private void InitializeComponent()
        {
            this.Text = "Edit Task";
            this.Size = new Size(400, 150);
            this.StartPosition = FormStartPosition.CenterParent;
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.BackColor = Color.FromArgb(45, 45, 48);
            this.ForeColor = Color.White;

            textBox = new TextBox
            {
                Location = new Point(12, 12),
                Size = new Size(360, 25),
                Font = new Font("Segoe UI", 11F),
                BackColor = Color.FromArgb(30, 30, 30),
                ForeColor = Color.White,
                BorderStyle = BorderStyle.FixedSingle
            };

            okButton = new Button
            {
                Text = "OK",
                Location = new Point(217, 50),
                Size = new Size(75, 30),
                DialogResult = DialogResult.OK,
                BackColor = Color.FromArgb(0, 122, 204),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat
            };
            okButton.FlatAppearance.BorderSize = 0;

            cancelButton = new Button
            {
                Text = "Cancel",
                Location = new Point(297, 50),
                Size = new Size(75, 30),
                DialogResult = DialogResult.Cancel,
                BackColor = Color.FromArgb(70, 70, 70),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat
            };
            cancelButton.FlatAppearance.BorderSize = 0;

            this.Controls.AddRange(new Control[] { textBox, okButton, cancelButton });
            this.AcceptButton = okButton;
            this.CancelButton = cancelButton;
        }
    }
}
