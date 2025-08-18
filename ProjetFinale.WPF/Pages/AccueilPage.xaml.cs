using ProjetFinale.Models;
using ProjetFinale.Services;
using ProjetFinale.Utils;
using ProjetFinale.Views;
using System;
using System.Windows;
using System.Windows.Controls;

namespace ProjetFinale.WPF
{
    public partial class AccueilPage : Page
    {
        private Utilisateur _user;

        public AccueilPage()
        {
            InitializeComponent();
            UserService.UtilisateurActifChanged += OnUserChanged;
            LoadUser();
        }

        private void LoadUser()
        {
            _user = UserService.UtilisateurActif;
            DataContext = _user;
        }

        private void OnUserChanged(Utilisateur? _)
        {
            LoadUser();
        }

        // Actions rapides
        private void NouvelleSeance_Click(object sender, RoutedEventArgs e)
        {
                                     // fenetre de WPF
            //(MaFenetre)Application.Current.MainWindow).FunctionNavigation();
            ((MainWindow)Application.Current.MainWindow).NavigateToExercices();
        }

        private void VoirProgres_Click(object sender, RoutedEventArgs e)
        {
            // page statistique pas crée
        }

        private void MesObjectifs_Click(object sender, RoutedEventArgs e)
        {
            ((MainWindow)Application.Current.MainWindow).NavigateToObjectifs();
        }

        private void Planning_Click(object sender, RoutedEventArgs e)
        {
            ((MainWindow)Application.Current.MainWindow).NavigateToSchedule();
        }

        // Profil (poids, taille, âge)
        private void SauvegarderProfil_Click(object sender, RoutedEventArgs e)
        {
            if (_user == null) return;

            double poids = double.Parse(PoidsTextBox.Text);
            double taille = double.Parse(TailleTextBox.Text);
            int age = int.Parse(AgeTextBox.Text);

            _user.Poids = poids;
            _user.Taille = taille;
            _user.Age = age;

            JsonService.SauvegarderUtilisateur(_user);

            MessageBox.Show($"Profil sauvegardé.\nIMC: {_user.IMC:F1}", "OK",
                MessageBoxButton.OK, MessageBoxImage.Information);
        }

        // Objectifs (poids visé, date)
        private void SauvegarderObjectifs_Click(object sender, RoutedEventArgs e)
        {
            if (_user == null) return;

            double poidsVise = double.Parse(ObjectifPoidsTextBox.Text);
            DateTime dateVisee = DateObjectifPicker.SelectedDate.Value;

            _user.ObjectifPoids = poidsVise;
            _user.DateObjectif = dateVisee;

            JsonService.SauvegarderUtilisateur(_user);

            double imcVise = poidsVise / Math.Pow(_user.Taille / 100.0, 2);

            MessageBox.Show(
                $"Objectifs sauvegardés.\nPoids: {poidsVise} kg\nIMC: {imcVise:F1}\nDate: {dateVisee:dd/MM/yyyy}",
                "OK", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private void Page_Unloaded(object sender, RoutedEventArgs e)
        {
            UserService.UtilisateurActifChanged -= OnUserChanged;
        }
    }
}
