using System;

namespace ProjetFinale.Utils
{
    public enum AppTheme { Dark, Light }

    public static class ThemeManager
    {
        public static AppTheme CurrentTheme { get; private set; } = AppTheme.Dark;

        // Notifie l’UI qu’il faut appliquer un nouveau thème
        public static event Action<AppTheme> ThemeChanged;

        public static void SetTheme(AppTheme theme)
        {
            if (CurrentTheme == theme) return;
            CurrentTheme = theme;
            ThemeChanged?.Invoke(CurrentTheme);
        }

        public static void SetDarkMode(bool isDark) => SetTheme(isDark ? AppTheme.Dark : AppTheme.Light);
    }
}