using System;

namespace ProjetFinale.Utils
{
    public enum AppTheme { Violet, Bleu, Rose, Vert, Light, Dark }

    public sealed class AppThemeChangedEventArgs : EventArgs
    {
        public AppTheme NewTheme { get; }
        public bool IsDark { get; }
        public AppThemeChangedEventArgs(AppTheme newTheme, bool isDark)
        {
            NewTheme = newTheme; IsDark = isDark;
        }
    }

    public static class ThemeManager
    {
        public static AppTheme CurrentTheme { get; private set; } = AppTheme.Violet;
        public static bool IsDark { get; private set; } = true;

        public static event EventHandler<AppThemeChangedEventArgs>? ThemeChanged;

        public static void ApplyTheme(string theme)
        {
            if (string.IsNullOrWhiteSpace(theme)) return;

            if (Enum.TryParse<AppTheme>(theme, true, out var parsed))
            {
                SetTheme(parsed);
                return;
            }

            switch (theme.Trim().ToLowerInvariant())
            {
                case "violet": SetTheme(AppTheme.Violet); break;
                case "bleu": SetTheme(AppTheme.Bleu); break;
                case "rose": SetTheme(AppTheme.Rose); break;
                case "vert": SetTheme(AppTheme.Vert); break;
                case "light": SetTheme(AppTheme.Light); break;
                case "dark": SetTheme(AppTheme.Dark); break;
            }
        }

        public static void SetDarkMode(bool isDark)
        {
            if (IsDark == isDark) return;
            IsDark = isDark;
            RaiseThemeChanged();
        }

        private static void SetTheme(AppTheme theme)
        {
            if (CurrentTheme == theme) return;
            CurrentTheme = theme;
            RaiseThemeChanged();
        }

        private static void RaiseThemeChanged()
        {
            ThemeChanged?.Invoke(null, new AppThemeChangedEventArgs(CurrentTheme, IsDark));
        }
    }
}