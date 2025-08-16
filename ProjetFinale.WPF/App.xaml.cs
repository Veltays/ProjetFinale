using ProjetFinale.Services;
using ProjetFinale.Utils;
using ProjetFinale.Views;
using ProjetFinale.WPF;
using System.Windows;

namespace ProjetFinale
{
    public partial class App : System.Windows.Application
    {
        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            var paramManager = new SettingsManager();

            // Unités au boot
            SettingsContext.Instance.WeightUnit =
                paramManager.FormatPoids == "LBS" ? WeightUnit.LBS : WeightUnit.KG;
            SettingsContext.Instance.HeightUnit =
                paramManager.FormatTaille == "INCH" ? HeightUnit.INCH : HeightUnit.CM;

            // === Thème ===
            ProjetFinale.WPF.WpfThemeBridge.InitializeAndApplyCurrent();          // écoute ThemeManager
            ThemeManager.SetDarkMode(paramManager.ModeSombre);                     // déclenche l’application du dico

            // le reste inchangé...
            if (paramManager.IsLogin)
            {
                var utilisateur = JsonService.ChargerUtilisateur();
                if (utilisateur != null) { UserService.UtilisateurActif = utilisateur; new MainWindow().Show(); }
                else { new LoginWindow().Show(); }
            }
            else { new LoginWindow().Show(); }
        }
    }
}