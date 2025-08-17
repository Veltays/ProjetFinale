using System.Windows;
using ProjetFinale.Services;
using ProjetFinale.Utils;   // SettingsContext, ThemeManager
using ProjetFinale.Views;   // LoginWindow
using ProjetFinale.WPF;     // WpfThemeBridge

namespace ProjetFinale
{
    public partial class App : Application
    {
        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            // Optionnel: fermer l'app quand la fenêtre principale est fermée
            ShutdownMode = ShutdownMode.OnMainWindowClose;

            // 1) Charger les préférences depuis le registre
            var settings = new SettingsManager().GetCurrentSettings();

            // 2) Propager aux SINGLETONS utilisés par le binding
            SettingsContext.Instance.WeightUnit =
                settings.FormatPoids == "LBS" ? WeightUnit.LBS : WeightUnit.KG;

            SettingsContext.Instance.HeightUnit =
                settings.FormatTaille == "INCH" ? HeightUnit.INCH : HeightUnit.CM;

            // 3) Thème (par défaut: sombre si aucune valeur enregistrée)
            ThemeManager.ApplyTheme(settings.ThemeCouleur);     // Violet/Bleu/Rose/Vert (si tu l’utilises)
            ThemeManager.SetDarkMode(settings.ModeSombre);      // true = Dark, false = Light

            // 4) Brancher le pont WPF et appliquer immédiatement (charge le bon ResourceDictionary)
            WpfThemeBridge.InitializeAndApplyCurrent();

            // 5) Lancer l’UI
            Window win;
            if (new SettingsManager().IsLogin)
            {
                var utilisateur = JsonService.ChargerUtilisateur();
                win = (utilisateur != null)
                    ? new MainWindow()
                    : new LoginWindow();
                if (utilisateur != null) UserService.UtilisateurActif = utilisateur;
            }
            else
            {
                win = new LoginWindow();
            }

            MainWindow = win;
            win.Show();
        }
    }
}
