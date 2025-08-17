using System;
using System.Windows;
using ProjetFinale.Utils;

namespace ProjetFinale.WPF
{
    public static class WpfThemeBridge
    {
        private static ResourceDictionary? _currentTheme;
        private static readonly Uri DarkUri =
            new Uri("pack://application:,,,/ProjetFinale.WPF;component/Assets/Themes/Theme.Dark.xaml");
        private static readonly Uri LightUri =
            new Uri("pack://application:,,,/ProjetFinale.WPF;component/Assets/Themes/Theme.Light.xaml");

        public static void InitializeAndApplyCurrent()
        {
            ThemeManager.ThemeChanged -= OnThemeChanged;
            ThemeManager.ThemeChanged += OnThemeChanged;
            Apply(ThemeManager.CurrentTheme, ThemeManager.IsDark);
        }

        private static void OnThemeChanged(object? s, AppThemeChangedEventArgs e) =>
            Apply(e.NewTheme, e.IsDark);

        private static void Apply(AppTheme theme, bool isDark)
        {
            var app = Application.Current;
            if (app == null) return;

            // swap du dictionnaire
            if (_currentTheme != null) app.Resources.MergedDictionaries.Remove(_currentTheme);
            _currentTheme = new ResourceDictionary { Source = isDark ? DarkUri : LightUri };
            app.Resources.MergedDictionaries.Add(_currentTheme);

            // (facultatif) exposer l’état
            app.Resources["Theme.Name"] = theme.ToString();
            app.Resources["Theme.IsDark"] = isDark;
        }
    }
}