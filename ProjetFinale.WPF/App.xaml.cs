using System.Windows;
using ProjetFinale.Services;
using ProjetFinale.Utils;   // SettingsContext, ThemeManager
using ProjetFinale.Views;   // LoginWindow
using ProjetFinale.WPF;     // WpfThemeBridge

namespace ProjetFinale
{
    public partial class App : Application
    {
        private SettingsManager _settingsManager = null!;
        private dynamic _settings = null!; // type retourné par GetCurrentSettings()

        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);
            ShutdownMode = ShutdownMode.OnMainWindowClose;

            InitSettings();
            ApplyUnitsToContext();
            ApplyTheme();
            ApplyWpfResources();

            var window = CreateStartupWindow();
            MainWindow = window;
            window.Show();
        }

        // ---------- Steps ----------

        private void InitSettings()
        {
            _settingsManager = new SettingsManager();
            _settings = _settingsManager.GetCurrentSettings();
        }

        private void ApplyUnitsToContext()
        {
            SettingsContext.Instance.WeightUnit =
                _settings.FormatPoids == "LBS" ? WeightUnit.LBS : WeightUnit.KG;

            SettingsContext.Instance.HeightUnit =
                _settings.FormatTaille == "INCH" ? HeightUnit.INCH : HeightUnit.CM;
        }

        private void ApplyTheme()
        {
            ThemeManager.ApplyTheme(_settings.ThemeCouleur); // Violet/Bleu/Rose/Vert
            ThemeManager.SetDarkMode(_settings.ModeSombre);  // true = Dark
        }

        private void ApplyWpfResources()
        {
            WpfThemeBridge.InitializeAndApplyCurrent();
        }

        private Window CreateStartupWindow()
        {
            if (_settingsManager.IsLogin)
            {
                var user = JsonService.ChargerUtilisateur();
                if (user != null)
                {
                    UserService.UtilisateurActif = user;
                    return new MainWindow();
                }
            }
            return new LoginWindow();
        }
    }
}