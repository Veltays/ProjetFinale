using System;
using System.Windows;
using ProjetFinale.Utils;

namespace ProjetFinale.WPF
{
    public static class WpfThemeBridge
    {

        private static ResourceDictionary? _lightOverride;
        private static readonly Uri LightOverrideUri =
            new Uri("pack://application:,,,/ProjetFinale.WPF;component/Assets/Themes/LightOverride.xaml");

        private static ResourceDictionary? _objLightOverride;
        private static readonly Uri ObjLightOverrideUri =
            new Uri("pack://application:,,,/ProjetFinale.WPF;component/Assets/Themes/ObjLightTheme.xaml");

        private static ResourceDictionary? _objDarkOverride;
        private static readonly Uri ObjDarkOverrideUri =
            new Uri("pack://application:,,,/ProjetFinale.WPF;component/Assets/Themes/ObjDarkTheme.xaml");

        public static void InitializeAndApplyCurrent()
        {
            ThemeManager.ThemeChanged -= OnThemeChanged;
            ThemeManager.ThemeChanged += OnThemeChanged;
            Apply(ThemeManager.IsDark);
        }

        private static void OnThemeChanged(object? s, AppThemeChangedEventArgs e) => Apply(e.IsDark);

        private static void Apply(bool isDark)
        {
            var app = Application.Current;
            if (app == null) return;

            var dicts = app.Resources.MergedDictionaries;

            // retire l’override si on repasse en dark
            if (isDark)
            {
                if (_lightOverride != null) dicts.Remove(_lightOverride);
                if (_objLightOverride != null) dicts.Remove(_objLightOverride);

                _objDarkOverride ??= new ResourceDictionary { Source = ObjDarkOverrideUri };
                if (!dicts.Contains(_objDarkOverride)) dicts.Add(_objDarkOverride);
            }
            else
            {
                _lightOverride ??= new ResourceDictionary { Source = LightOverrideUri };
                _objLightOverride ??= new ResourceDictionary { Source = ObjLightOverrideUri };

                if (!dicts.Contains(_lightOverride)) dicts.Add(_lightOverride);
                if (!dicts.Contains(_objLightOverride)) dicts.Add(_objLightOverride);

                if (_objDarkOverride != null) dicts.Remove(_objDarkOverride);
            }

            app.Resources["Theme.IsDark"] = isDark; // optionnel, si tu l’utilises
        }
    }
}