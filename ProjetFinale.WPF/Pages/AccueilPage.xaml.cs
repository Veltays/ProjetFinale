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
        private Utilisateur _utilisateur;

        public AccueilPage()
        {
            InitializeComponent();

            // S'abonner aux changements d'utilisateur
            UserService.UtilisateurActifChanged += OnUtilisateurChanged;
            ChargerUtilisateur();
        }

        private void ChargerUtilisateur()
        {
            _utilisateur = UserService.UtilisateurActif;
            if (_utilisateur != null)
            {
                this.DataContext = _utilisateur;
                Console.WriteLine($"✅ Utilisateur chargé : {_utilisateur.Pseudo}");
            }
            else
            {
                Console.WriteLine("⚠️ Aucun utilisateur actif trouvé");
            }
        }

        private void OnUtilisateurChanged(Utilisateur? nouvelUtilisateur)
        {
            ChargerUtilisateur();
        }

        // === QUICK ACTIONS ===
        private void NouvelleSeance_Click(object sender, RoutedEventArgs e)
        {
            (Application.Current.MainWindow as MainWindow)?.NavigateToExercices();
        }

        private void VoirProgres_Click(object sender, RoutedEventArgs e)
        {
            
        }

        private void MesObjectifs_Click(object sender, RoutedEventArgs e)
        {
            (Application.Current.MainWindow as MainWindow)?.NavigateToObjectifs();
        }

        private void Planning_Click(object sender, RoutedEventArgs e)
        {
            (Application.Current.MainWindow as MainWindow)?.NavigateToSchedule();
        }

        // 🆕 SAUVEGARDE DU PROFIL (Poids, Taille, Âge)
        private void SauvegarderProfil_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (_utilisateur == null)
                {
                    MessageBox.Show("❌ Aucun utilisateur actif", "Erreur",
                                   MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }

                if (!double.TryParse(PoidsTextBox.Text, out double nouveauPoids) || nouveauPoids <= 0)
                {
                    MessageBox.Show("⚠️ Le poids doit être un nombre positif", "Erreur de validation",
                                   MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                if (!double.TryParse(TailleTextBox.Text, out double nouvelleTaille) || nouvelleTaille <= 0)
                {
                    MessageBox.Show("⚠️ La taille doit être un nombre positif", "Erreur de validation",
                                   MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                if (!int.TryParse(AgeTextBox.Text, out int nouvelAge) || nouvelAge <= 0 || nouvelAge > 150)
                {
                    MessageBox.Show("⚠️ L'âge doit être entre 1 et 150 ans", "Erreur de validation",
                                   MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                _utilisateur.Poids = nouveauPoids;
                _utilisateur.Taille = nouvelleTaille;
                _utilisateur.Age = nouvelAge;

                JsonService.SauvegarderUtilisateur(_utilisateur);

                MessageBox.Show($"✅ Profil sauvegardé avec succès !\n\n" +
                               $"📊 Nouveau profil :\n" +
                               $"• Poids : {nouveauPoids} kg\n" +
                               $"• Taille : {nouvelleTaille} cm\n" +
                               $"• Âge : {nouvelAge} ans\n" +
                               $"• IMC : {_utilisateur.IMC:F1}",
                               "Profil sauvegardé",
                               MessageBoxButton.OK,
                               MessageBoxImage.Information);

                Console.WriteLine($"💾 Profil sauvegardé - Poids: {nouveauPoids}kg, Taille: {nouvelleTaille}cm, Âge: {nouvelAge}ans");
            }
            catch (Exception ex)
            {
                MessageBox.Show($"❌ Erreur lors de la sauvegarde du profil :\n{ex.Message}",
                               "Erreur", MessageBoxButton.OK, MessageBoxImage.Error);
                Console.WriteLine($"❌ Erreur sauvegarde profil : {ex.Message}");
            }
        }

        // 🆕 SAUVEGARDE DES OBJECTIFS (Poids visé, Date objectif)
        private void SauvegarderObjectifs_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (_utilisateur == null)
                {
                    MessageBox.Show("❌ Aucun utilisateur actif", "Erreur",
                                   MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }

                if (!double.TryParse(ObjectifPoidsTextBox.Text, out double nouveauObjectifPoids) || nouveauObjectifPoids <= 0)
                {
                    MessageBox.Show("⚠️ Le poids objectif doit être un nombre positif", "Erreur de validation",
                                   MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                if (!DateObjectifPicker.SelectedDate.HasValue)
                {
                    MessageBox.Show("⚠️ Veuillez sélectionner une date objectif", "Erreur de validation",
                                   MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                DateTime nouvelleDateObjectif = DateObjectifPicker.SelectedDate.Value;

                if (nouvelleDateObjectif <= DateTime.Now)
                {
                    MessageBox.Show("⚠️ La date objectif doit être dans le futur", "Erreur de validation",
                                   MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                _utilisateur.ObjectifPoids = nouveauObjectifPoids;
                _utilisateur.DateObjectif = nouvelleDateObjectif;

                JsonService.SauvegarderUtilisateur(_utilisateur);

                double imcObjectif = nouveauObjectifPoids / Math.Pow(_utilisateur.Taille / 100.0, 2);
                int anneesRestantes = nouvelleDateObjectif.Year - DateTime.Now.Year;

                MessageBox.Show($"🎯 Objectifs sauvegardés avec succès !\n\n" +
                               $"📊 Vos nouveaux objectifs :\n" +
                               $"• Poids visé : {nouveauObjectifPoids} kg\n" +
                               $"• IMC visé : {imcObjectif:F1}\n" +
                               $"• Date limite : {nouvelleDateObjectif:dd/MM/yyyy}\n" +
                               $"• Temps restant : {Math.Max(0, anneesRestantes)} ans",
                               "Objectifs sauvegardés",
                               MessageBoxButton.OK,
                               MessageBoxImage.Information);

                Console.WriteLine($"🎯 Objectifs sauvegardés - Poids: {nouveauObjectifPoids}kg, Date: {nouvelleDateObjectif:dd/MM/yyyy}");
            }
            catch (Exception ex)
            {
                MessageBox.Show($"❌ Erreur lors de la sauvegarde des objectifs :\n{ex.Message}",
                               "Erreur", MessageBoxButton.OK, MessageBoxImage.Error);
                Console.WriteLine($"❌ Erreur sauvegarde objectifs : {ex.Message}");
            }
        }

        // Nettoyage des événements
        private void Page_Unloaded(object sender, RoutedEventArgs e)
        {
            UserService.UtilisateurActifChanged -= OnUtilisateurChanged;
        }

        ~AccueilPage()
        {
            UserService.UtilisateurActifChanged -= OnUtilisateurChanged;
        }
    }
}
