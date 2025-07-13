using System.Windows;
using ProjetFinale.Views;

namespace ProjetFinale.Views
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }



        // clique sur le bouton de logout
        private void Logout_Click(object sender, RoutedEventArgs e)
        {
            // Réinitialiser la valeur dans le registre
            var paramManager = new MyAppParamManager();
            paramManager.IsLogin = false;

            // Ouvrir LoginWindow
            var login = new LoginWindow();
            login.Show();

            // Fermer la fenêtre actuelle
            this.Close();
        }
    }
}