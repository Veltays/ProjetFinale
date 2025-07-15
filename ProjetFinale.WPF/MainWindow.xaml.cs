using ProjetFinale.Services;
using ProjetFinale.WPF;
using System.Windows;
using System.Windows.Controls;

namespace ProjetFinale.Views
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();

            // Affiche la page d'accueil au démarrage
            ContentFrame.Navigate(new AccueilPage());
        }

        // 🔁 Navigation vers la page d'objectifs (lorsqu'on clique sur "TASK")
        private void ObjectifsButton_Click(object sender, RoutedEventArgs e)
        {
            ContentFrame.Navigate(new ObjectifPage());
        }


        private void AccueilButton_Click(object sender, RoutedEventArgs e)
        {
            ContentFrame.Navigate(new AccueilPage());
        }


        private void ExportsButton_Click(object sender, RoutedEventArgs e)
        {
            ContentFrame.Navigate(new ExportsPage());
        }


        private void SettingsButton_Click(object sender, RoutedEventArgs e)
        {
            ContentFrame.Navigate(new SettingsPage());
        }

        // 🔓 Déconnexion
        private void Logout_Click(object sender, RoutedEventArgs e)
        {
            var paramManager = new MyAppParamManager();
            paramManager.IsLogin = false;

            var login = new LoginWindow();
            login.Show();

            this.Close();
        }
    }
}
