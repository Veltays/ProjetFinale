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
            // Afficher la page d'accueil au démarrage
            NavigateToAccueil();
        }

        // === NAVIGATION SIDEBAR ===

        private void AccueilButton_Click(object sender, RoutedEventArgs e)
        {
            NavigateToAccueil();
        }

        private void ObjectifsButton_Click(object sender, RoutedEventArgs e)
        {
            ContentFrame.Navigate(new ObjectifPage());
            ContentFrame.Visibility = Visibility.Visible;
        }

        private void ExportsButton_Click(object sender, RoutedEventArgs e)
        {
            ContentFrame.Navigate(new ExportsPage());
            ContentFrame.Visibility = Visibility.Visible;
        }

        private void SettingsButton_Click(object sender, RoutedEventArgs e)
        {
            ContentFrame.Navigate(new SettingsPage());
            ContentFrame.Visibility = Visibility.Visible;
        }

        private void ScheduleButton_Click(object sender, RoutedEventArgs e)
        {
            // ContentFrame.Navigate(new SchedulePage());
            // ContentFrame.Visibility = Visibility.Visible;
            MessageBox.Show("Page Schedule en cours de développement", "Info",
                           MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private void ExercicesButton_Click(object sender, RoutedEventArgs e)
        {
            // ContentFrame.Navigate(new ExercicesPage());
            // ContentFrame.Visibility = Visibility.Visible;
            MessageBox.Show("Page Exercices en cours de développement", "Info",
                           MessageBoxButton.OK, MessageBoxImage.Information);
        }

        // === MÉTHODES DE NAVIGATION PUBLIQUES (pour AccueilPage) ===

        public void NavigateToAccueil()
        {
            ContentFrame.Navigate(new AccueilPage());
            ContentFrame.Visibility = Visibility.Visible;
        }

        public void NavigateToExercices()
        {
            ExercicesButton_Click(null, null);
        }

        public void NavigateToObjectifs()
        {
            ObjectifsButton_Click(null, null);
        }

        public void NavigateToSchedule()
        {
            ScheduleButton_Click(null, null);
        }

        public void NavigateToSettings()
        {
            SettingsButton_Click(null, null);
        }

        public void NavigateToExports()
        {
            ExportsButton_Click(null, null);
        }

        // === DÉCONNEXION ===

        private void Logout_Click(object sender, RoutedEventArgs e)
        {
            // Demander confirmation avant de se déconnecter
            var result = MessageBox.Show("Êtes-vous sûr de vouloir vous déconnecter ?",
                                        "Déconnexion",
                                        MessageBoxButton.YesNo,
                                        MessageBoxImage.Question);

            if (result == MessageBoxResult.Yes)
            {
                var settingsManager = new SettingsManager();
                // Corriger l'accès à IsLogin selon ta classe SettingsManager
                // settingsManager.IsLogin = false; // Si cette propriété existe

                var login = new LoginWindow();
                login.Show();
                this.Close();
            }
        }
    }
}