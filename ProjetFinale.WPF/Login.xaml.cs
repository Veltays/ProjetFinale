using ProjetFinale;
using ProjetFinale.Models;
using ProjetFinale.Services;
using ProjetFinale.Views;
using ProjetFinale.WPF;
using ProjetFinale.Utils;
using System;
using System.Windows;

namespace ProjetFinale.Views
{
    public partial class LoginWindow : Window
    {
        private readonly UserService _userService = new UserService();

        public LoginWindow()
        {
            InitializeComponent();
        }

        private void CreerProfil_Click(object sender, RoutedEventArgs e)
        {
            // Récupération des valeurs de l'interface
            string pseudo = PseudoTextBox.Text;
            string nom = NomTextBox.Text;
            string prenom = PrenomTextBox.Text;
            string email = EmailTextBox.Text;
            string motDePasse = MotDePasseBox.Password;
            string confirmMotDePasse = ConfirmMotDePasseBox.Password;

            // Conversions protégées
            bool tailleOk = int.TryParse(TailleTextBox.Text, out int taille);
            bool poidsOk = double.TryParse(PoidsTextBox.Text, out double poids);
            bool ageOk = int.TryParse(AgeTextBox.Text, out int age);
            bool objectifOk = double.TryParse(ObjectifPoidsTextBox.Text, out double objectifPoids);
            bool dateOk = DateTime.TryParse(DateObjectifPicker.Text, out DateTime dateObjectif);

            if (!tailleOk || !poidsOk || !ageOk || !objectifOk || !dateOk)
            {
                MessageBox.Show("Certains champs numériques ou la date sont invalides.");
                return;
            }

            // Appel du UserService pour créer l'utilisateur
            Utilisateur? utilisateur = _userService.CreerUtilisateur(
                pseudo, nom, prenom, taille, poids, age, objectifPoids, dateObjectif, email, motDePasse, confirmMotDePasse
            );

            if (utilisateur == null)
            {
                MessageBox.Show("Veuillez corriger les champs invalides.");
                return;
            }

            // Si OK → continuer
            MessageBox.Show($"Bienvenue {utilisateur.Prenom} ! Profil créé avec succès.", "Succès");

            var paramManager = new SettingsManager();
            paramManager.IsLogin = true;

            var mainWindow = new MainWindow();
            mainWindow.Show();
            this.Close();
        }
    }
}


