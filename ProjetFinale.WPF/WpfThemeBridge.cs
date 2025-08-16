using System;
using System.Linq;
using System.Windows;
using ProjetFinale.Utils;

namespace ProjetFinale.WPF
{
    /// <summary>
    /// Pont UI : écoute ThemeManager (Utils) et applique les ResourceDictionaries WPF.
    /// </summary>
    public static class WpfThemeBridge
    {
        private const string DarkPath = "Assets/Theme.Dark.xaml";
        private const string LightPath = "Assets/Theme.Light.xaml";

        public static void InitializeAndApplyCurrent()
        {
            // S'abonner aux changements
            ThemeManager.ThemeChanged += OnThemeChanged;

            // Appliquer le thème courant au lancement
            Apply(AppThemeFromBool(ThemeManager.CurrentTheme == AppTheme.Dark));
        }

        private static void OnThemeChanged(AppTheme theme) => Apply(theme);

        private static void Apply(AppTheme theme)
        {
            var app = Application.Current;
            if (app is null) return;

            // Retirer anciens dicos de thème
            var olds = app.Resources.MergedDictionaries
                .Where(d => d.Source != null &&
                            (d.Source.OriginalString.EndsWith("Theme.Dark.xaml", StringComparison.OrdinalIgnoreCase) ||
                             d.Source.OriginalString.EndsWith("Theme.Light.xaml", StringComparison.OrdinalIgnoreCase)))
                .ToList();

            foreach (var d in olds)
                app.Resources.MergedDictionaries.Remove(d);

            // Ajouter le bon dico
            var uri = new Uri(theme == AppTheme.Dark ? DarkPath : LightPath, UriKind.Relative);
            var rd = new ResourceDictionary { Source = uri };
            app.Resources.MergedDictionaries.Add(rd);
        }

        private static AppTheme AppThemeFromBool(bool dark) => dark ? AppTheme.Dark : AppTheme.Light;
    }
}
