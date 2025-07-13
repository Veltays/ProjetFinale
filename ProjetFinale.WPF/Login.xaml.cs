using System.Windows;

namespace ProjetFinale.Views
{
    public partial class LoginWindow : Window
    {
        public LoginWindow()
        {
            InitializeComponent();
        }



        // clique sur le bouton de connexion
        private void CreerProfil_Click(object sender, RoutedEventArgs e)
        {
            // TODO : Valider les champs, enregistrer le profil...

            // ✅ Exemple de redirection vers MainWindow après login
            var paramManager = new MyAppParamManager();
            paramManager.IsLogin = true;

            var mainWindow = new MainWindow();
            mainWindow.Show();

            this.Close(); // Fermer la fenêtre de login
        }
    }
}
