using ProjetFinale.Models;
using ProjetFinale.Services;
using ProjetFinale.Utils;
using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace ProjetFinale.WPF
{
    public partial class ObjectifPage : Page
    {
        private Utilisateur? _utilisateur;

        public ObjectifPage()
        {
            InitializeComponent();
            Loaded += OnLoaded;
            Unloaded += OnUnloaded;
        }

        private void OnLoaded(object sender, RoutedEventArgs e)
        {
            InitialiserUtilisateur();
            UserService.UtilisateurActifChanged += OnUtilisateurChanged;
        }

        private void OnUnloaded(object sender, RoutedEventArgs e)
        {
            UserService.UtilisateurActifChanged -= OnUtilisateurChanged;
        }

        private void InitialiserUtilisateur()
        {
            // 1) Prend l’utilisateur actif si présent
            _utilisateur = UserService.UtilisateurActif;

            // 2) Sinon, charge depuis le disque et définis comme actif
            if (_utilisateur == null)
            {
                _utilisateur = JsonService.ChargerUtilisateur();
                if (_utilisateur != null)
                    UserService.UtilisateurActif = _utilisateur;
            }

            // 3) Bind ou avertis
            if (_utilisateur != null)
            {
                DataContext = _utilisateur;
            }
            else
            {
                MessageBox.Show(
                    "Aucun utilisateur trouvé. Connectez-vous d’abord.",
                    "Information",
                    MessageBoxButton.OK,
                    MessageBoxImage.Information
                );
            }
        }

        private void OnUtilisateurChanged(Utilisateur? nouvelUtilisateur)
        {
            _utilisateur = nouvelUtilisateur;
            DataContext = nouvelUtilisateur;
        }

        // === UI handlers ===

        private void TextBox_GotFocus(object sender, RoutedEventArgs e)
        {
            if (NouvelleTacheTextBox.Text == "ENTREZ UNE TACHE....")
                NouvelleTacheTextBox.Text = string.Empty;
        }

        private void NouvelleTacheTextBox_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter) AjouterTacheDepuisChamp();
        }

        private void AjouterTacheButton_Click(object sender, RoutedEventArgs e)
        {
            AjouterTacheDepuisChamp();
        }

        private void AjouterTacheDepuisChamp()
        {
            var texte = NouvelleTacheTextBox.Text.Trim();
            if (string.IsNullOrWhiteSpace(texte) || texte == "ENTREZ UNE TACHE....")
                return;

            AjouterTache(texte);
            NouvelleTacheTextBox.Text = "ENTREZ UNE TACHE....";
        }

        private void AjouterTache(string description)
        {
            if (_utilisateur == null) return;

            try
            {
                _utilisateur.ListeTaches.Add(new Tache
                {
                    Description = description,
                    EstTerminee = false,
                    Date = DateTime.Now
                });

                JsonService.SauvegarderUtilisateur(_utilisateur);

                // NOTE : ceci NE déclenche UtilisateurActifChanged que si l’instance change.
                // On le laisse si d’autres pages s’y attendent.
                UserService.UtilisateurActif = _utilisateur;
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    $"Erreur lors de l'ajout de la tâche :\n{ex.Message}",
                    "Erreur",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error
                );
            }
        }

        private void SupprimerTache_Click(object sender, RoutedEventArgs e)
        {
            if (_utilisateur == null) return;
            if (sender is not Button { Tag: Tache tache }) return;

            try
            {
                _utilisateur.ListeTaches.Remove(tache);
                JsonService.SauvegarderUtilisateur(_utilisateur);
                UserService.UtilisateurActif = _utilisateur; // voir note ci-dessus
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    $"Erreur lors de la suppression :\n{ex.Message}",
                    "Erreur",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error
                );
            }
        }

        private void TacheCheckBox_Changed(object sender, RoutedEventArgs e)
        {
            if (_utilisateur == null) return;
            if (sender is not CheckBox { DataContext: Tache _ }) return;

            try
            {
                // Le binding met déjà à jour EstTerminee.
                JsonService.SauvegarderUtilisateur(_utilisateur);
                UserService.UtilisateurActif = _utilisateur; // voir note ci-dessus
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    $"Erreur lors de la mise à jour :\n{ex.Message}",
                    "Erreur",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error
                );
            }


        }

        // Appel manuel si besoin
        public void RafraichirDonnees() => InitialiserUtilisateur();
    }
}
