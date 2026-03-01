using System.Drawing;

namespace DreamsLive_Solutions_PresenterApp1
{
    internal static class Constants
    {
        // Theme Colors for btnClearPresenterDisplay (Blackout Button)
        public static readonly Color LightTheme_BlackoutButton_Normal_Back = SystemColors.Control;
        public static readonly Color LightTheme_BlackoutButton_Normal_Fore = SystemColors.ControlText;
        public static readonly Color LightTheme_BlackoutButton_Active_Back = Color.LightCoral;
        public static readonly Color LightTheme_BlackoutButton_Active_Fore = Color.White;

        public static readonly Color DarkTheme_BlackoutButton_Normal_Back = Color.FromArgb(63, 63, 70);
        public static readonly Color DarkTheme_BlackoutButton_Normal_Fore = Color.White;
        public static readonly Color DarkTheme_BlackoutButton_Active_Back = Color.DarkRed;
        public static readonly Color DarkTheme_BlackoutButton_Active_Fore = Color.White;

        // Border colors for picSecondaryPreview's wrapper panel
        public static readonly Color BorderColorDefault = Color.Gray;
        public static readonly Color BorderColorStagedNotLive = Color.Green;
        public static readonly Color BorderColorLive = Color.Red;
    }
}
