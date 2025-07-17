using ProjetFinale.Services;
using ProjetFinale.Utils;
using ProjetFinale.Views;
using ProjetFinale.WPF;
using System.Windows;

namespace ProjetFinale
{
    public partial class App : Application
    {
        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            var paramManager = new SettingsManager();

            if (paramManager.IsLogin)
            {
                var utilisateur = JsonService.ChargerUtilisateur();
                if (utilisateur != null)
                {
                    UserService.UtilisateurActif = utilisateur;
                    MessageBox.Show("Utilisateur chargé");

                    var mainWindow = new MainWindow();
                    mainWindow.Show();
                }
                else
                {
                    MessageBox.Show("Fichier utilisateur non trouvé ou corrompu");
                    var loginWindow = new LoginWindow();
                    loginWindow.Show();
                }
            }
            else
            {
                var loginWindow = new LoginWindow();
                loginWindow.Show();
            }
        }
    }
}