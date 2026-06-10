using System.Drawing;

namespace DreamsLive_Solutions_PresenterApp1
{
    internal static class Constants
    {
        // Theme Colors for btnClearPresenterDisplay (Blackout Button) - Linear palette.
        public static readonly Color LightTheme_BlackoutButton_Normal_Back = Color.FromArgb(0xFF, 0xFF, 0xFF); // surface-1 light
        public static readonly Color LightTheme_BlackoutButton_Normal_Fore = Color.FromArgb(0x1C, 0x1D, 0x21); // ink light
        public static readonly Color LightTheme_BlackoutButton_Active_Back = Color.FromArgb(0x22, 0xC5, 0x5E); // bright green (Restore Presenter)
        public static readonly Color LightTheme_BlackoutButton_Active_Fore = Color.FromArgb(0x06, 0x23, 0x0F);

        public static readonly Color DarkTheme_BlackoutButton_Normal_Back = Color.FromArgb(0x14, 0x15, 0x18); // surface-1 dark
        public static readonly Color DarkTheme_BlackoutButton_Normal_Fore = Color.FromArgb(0xF7, 0xF8, 0xF8); // ink dark
        public static readonly Color DarkTheme_BlackoutButton_Active_Back = Color.FromArgb(0x22, 0xC5, 0x5E); // bright green (Restore Presenter)
        public static readonly Color DarkTheme_BlackoutButton_Active_Fore = Color.FromArgb(0x06, 0x23, 0x0F);

        // Border colors for picSecondaryPreview's wrapper panel (staging status).
        public static readonly Color BorderColorDefault = Color.FromArgb(0x33, 0x36, 0x3D); // hairline-strong
        public static readonly Color BorderColorStagedNotLive = Color.FromArgb(0x27, 0xA6, 0x44); // success green
        public static readonly Color BorderColorLive = Color.FromArgb(0xD2, 0x5E, 0x5E); // danger
    }
}
