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
            this.Loaded += AccueilPage_Loaded;
        }


        private void AccueilPage_Loaded(object sender, RoutedEventArgs e)
        {
            _user = UserService.UtilisateurActif;
            DataContext = _user;
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
            if (_user == null)
                return;

            try
            {
                double poids = double.Parse(PoidsTextBox.Text);
                double taille = double.Parse(TailleTextBox.Text);
                int age = int.Parse(AgeTextBox.Text);


                // 📝 Mise à jour
                _user.Poids = poids;
                _user.Taille = taille;
                _user.Age = age;

                // 💾 Sauvegarde
                JsonService.SauvegarderUtilisateur(_user);

                MessageBox.Show($"Profil sauvegardé.\nIMC: {_user.IMC:F1}", "OK",
                    MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (FormatException)
            {
                MessageBox.Show("Valeurs saisies invalides.", "Erreur",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }






        // Objectifs (poids visé, date)
        private void SauvegarderObjectifs_Click(object sender, RoutedEventArgs e)
        {
            if (_user == null)
                return;

            try
            {
                double poidsVise = double.Parse(ObjectifPoidsTextBox.Text);
                DateTime dateVisee = DateObjectifPicker.SelectedDate
                    ?? throw new FormatException("Veuillez sélectionner une date.");


                // 📝 Mise à jour
                _user.ObjectifPoids = poidsVise;
                _user.DateObjectif = dateVisee;

                // 💾 Sauvegarde
                JsonService.SauvegarderUtilisateur(_user);

                double imcVise = poidsVise / Math.Pow(_user.Taille / 100.0, 2);

                MessageBox.Show(
                    $"Objectifs sauvegardés.\nPoids: {poidsVise} kg\nIMC: {imcVise:F1}\nDate: {dateVisee:dd/MM/yyyy}",
                    "OK", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (FormatException ex)
            {
                MessageBox.Show(ex.Message, "Erreur",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

    }
}
