using System.Drawing;

namespace TODO
{
    public class AppSettings
    {
        public string FontFamily { get; set; } = "Segoe UI";
        public float FontSize { get; set; } = 11F;
        public bool FontBold { get; set; } = false;
        public Color TextColor { get; set; } = Color.SpringGreen;
        public string BackgroundImagePath { get; set; } = "";
        public bool MinimizeToTray { get; set; } = true;
        public bool StartMinimized { get; set; } = false;
        public bool DefaultQuitBehavior { get; set; } = false; // false = Hide, true = Quit

        public AppSettings Clone()
        {
            return new AppSettings
            {
                FontFamily = this.FontFamily,
                FontSize = this.FontSize,
                FontBold = this.FontBold,
                TextColor = this.TextColor,
                BackgroundImagePath = this.BackgroundImagePath,
                MinimizeToTray = this.MinimizeToTray,
                StartMinimized = this.StartMinimized,
                DefaultQuitBehavior = this.DefaultQuitBehavior
            };
        }

        public Font CreateFont()
        {
            var style = FontBold ? FontStyle.Bold : FontStyle.Regular;
            return new Font(FontFamily, FontSize, style);
        }
    }
}