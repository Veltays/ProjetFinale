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
            InitialiserUtilisateur();
        }

        private void InitialiserUtilisateur()
        {
            // Essayer d'abord l'utilisateur actif
            _utilisateur = UserService.UtilisateurActif;
            Console.WriteLine($"🔍 UtilisateurActif : {(_utilisateur != null ? _utilisateur.Pseudo : "NULL")}");

            // Si pas d'utilisateur actif, charger depuis le fichier
            if (_utilisateur == null)
            {
                _utilisateur = JsonService.ChargerUtilisateur();
                Console.WriteLine($"🔍 ChargerUtilisateur : {(_utilisateur != null ? _utilisateur.Pseudo : "NULL")}");

                if (_utilisateur != null)
                {
                    UserService.UtilisateurActif = _utilisateur;
                }
            }

            // 🔥 DÉFINIR LE DATACONTEXT - C'est la magie du data binding !
            if (_utilisateur != null)
            {
                this.DataContext = _utilisateur;
                Console.WriteLine($"✅ DataContext défini pour ObjectifsPage : {_utilisateur.Pseudo}");
                Console.WriteLine($"   Nombre de tâches : {_utilisateur.ListeTaches.Count}");
                Console.WriteLine($"   ListeTaches est null ? {(_utilisateur.ListeTaches == null)}");
            }
            else
            {
                Console.WriteLine("⚠️ Aucun utilisateur trouvé pour ObjectifsPage");
                MessageBox.Show("Aucun utilisateur trouvé ! Assurez-vous d'être connecté.", "Erreur", MessageBoxButton.OK, MessageBoxImage.Warning);
            }

            // 🆕 S'abonner aux changements d'utilisateur
            UserService.UtilisateurActifChanged += OnUtilisateurChanged;
        }

        // 🆕 Méthode appelée quand l'utilisateur change (après import par exemple)
        private void OnUtilisateurChanged(Utilisateur? nouvelUtilisateur)
        {
            _utilisateur = nouvelUtilisateur;
            this.DataContext = nouvelUtilisateur;

            if (nouvelUtilisateur != null)
            {
                Console.WriteLine($"🔄 ObjectifsPage - Utilisateur changé : {nouvelUtilisateur.Pseudo}");
                Console.WriteLine($"   Nouvelles tâches : {nouvelUtilisateur.ListeTaches.Count}");
            }
        }

        // === GESTION DES ÉVÉNEMENTS (simplifiés) ===

        private void TextBox_GotFocus(object sender, RoutedEventArgs e)
        {
            if (NouvelleTacheTextBox.Text == "ENTREZ UNE TACHE....")
            {
                NouvelleTacheTextBox.Text = "";
            }
        }

        private void NouvelleTacheTextBox_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                AjouterTacheDepuisChamp();
            }
        }

        private void AjouterTacheButton_Click(object sender, RoutedEventArgs e)
        {
            AjouterTacheDepuisChamp();
        }

        private void AjouterTacheDepuisChamp()
        {
            string texte = NouvelleTacheTextBox.Text.Trim();

            Console.WriteLine($"🔍 Debug - Texte saisi : '{texte}'");
            Console.WriteLine($"🔍 Debug - Utilisateur : {(_utilisateur != null ? _utilisateur.Pseudo : "NULL")}");

            if (!string.IsNullOrWhiteSpace(texte) && texte != "ENTREZ UNE TACHE....")
            {
                Console.WriteLine($"✅ Validation OK - Ajout de la tâche");
                AjouterTache(texte);
                NouvelleTacheTextBox.Text = "ENTREZ UNE TACHE...."; // Reset placeholder
            }
            else
            {
                Console.WriteLine($"❌ Validation échouée - Texte invalide");
                MessageBox.Show($"Texte invalide : '{texte}'", "Debug", MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }

        // 🔥 MÉTHODE SIMPLIFIÉE : Plus besoin d'AfficherTaches() !
        private void AjouterTache(string texte)
        {
            if (_utilisateur == null || string.IsNullOrWhiteSpace(texte))
                return;

            try
            {
                var nouvelleTache = new Tache
                {
                    Description = texte,
                    EstTerminee = false,
                    Date = DateTime.Now
                };

                // ✨ MAGIE : Juste ajouter à la liste, l'UI se met à jour automatiquement !
                _utilisateur.ListeTaches.Add(nouvelleTache);

                // Sauvegarder et mettre à jour l'utilisateur actif
                JsonService.SauvegarderUtilisateur(_utilisateur);
                UserService.UtilisateurActif = _utilisateur; // Déclenche la notification

                Console.WriteLine($"✅ Nouvelle tâche ajoutée : {texte}");
                Console.WriteLine($"   Total tâches : {_utilisateur.ListeTaches.Count}");
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Erreur lors de l'ajout de la tâche :\n{ex.Message}",
                               "Erreur", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        // 🔥 MÉTHODE SIMPLIFIÉE : Plus de recherche manuelle !
        private void SupprimerTache_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button btn && btn.Tag is Tache tache && _utilisateur != null)
            {
                try
                {
                    // ✨ MAGIE : Juste supprimer de la liste, l'UI se met à jour automatiquement !
                    _utilisateur.ListeTaches.Remove(tache);

                    // Sauvegarder et notifier
                    JsonService.SauvegarderUtilisateur(_utilisateur);
                    UserService.UtilisateurActif = _utilisateur; // Déclenche la notification

                    Console.WriteLine($"🗑️ Tâche supprimée : {tache.Description}");
                    Console.WriteLine($"   Tâches restantes : {_utilisateur.ListeTaches.Count}");
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Erreur lors de la suppression :\n{ex.Message}",
                                   "Erreur", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        // 🆕 NOUVELLE MÉTHODE : Gestion automatique du changement d'état
        private void TacheCheckBox_Changed(object sender, RoutedEventArgs e)
        {
            if (sender is CheckBox checkBox && checkBox.DataContext is Tache tache && _utilisateur != null)
            {
                try
                {
                    // ✨ Le binding s'occupe déjà de mettre à jour EstTerminee !
                    // On n'a qu'à sauvegarder

                    JsonService.SauvegarderUtilisateur(_utilisateur);
                    UserService.UtilisateurActif = _utilisateur; // Déclenche la notification

                    string etat = tache.EstTerminee ? "terminée" : "en cours";
                    Console.WriteLine($"✅ Tâche '{tache.Description}' marquée comme {etat}");
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Erreur lors de la mise à jour :\n{ex.Message}",
                                   "Erreur", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        // 🆕 MÉTHODE UTILITAIRE : Rafraîchir les données (si nécessaire)
        public void RafraichirDonnees()
        {
            InitialiserUtilisateur();
        }

        // 🆕 Nettoyer les événements quand la page se ferme
        ~ObjectifPage()
        {
            UserService.UtilisateurActifChanged -= OnUtilisateurChanged;
        }

        // === MÉTHODES SUPPRIMÉES ===
        // ❌ Plus besoin de AfficherTaches() - le binding s'en occupe !
        // ❌ Plus besoin de CreerTacheElement() - le DataTemplate s'en occupe !
        // ❌ Plus besoin de ToggleTacheEtat() - le binding bidirectionnel s'en occupe !
        // ❌ Plus besoin de boucles foreach - ItemsControl s'en occupe !
        // ❌ Plus besoin de TaskListContainer.Children.Clear() - automatique !
        // ❌ Plus besoin de gestion manuelle de EmptyStateMessage - binding conditionnel !
    }
}