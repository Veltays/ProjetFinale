using Microsoft.Win32;
using ProjetFinale.Models;
using ProjetFinale.Services;
using ProjetFinale.Utils;
using System;
using System.IO;
using System.Text;
using System.Windows;
using System.Windows.Controls;

namespace ProjetFinale.WPF
{
    public partial class ExportsPage : Page
    {
        private readonly ExportService _exportService = new ExportService();
        private Utilisateur? _utilisateur;

        public ExportsPage()
        {
            InitializeComponent();
            InitialiserUtilisateur();
        }

        private void InitialiserUtilisateur()
        {
            // Essayer d'abord l'utilisateur actif
            _utilisateur = UserService.UtilisateurActif;

            // Si pas d'utilisateur actif, charger depuis le fichier
            if (_utilisateur == null)
            {
                _utilisateur = JsonService.ChargerUtilisateur();
                if (_utilisateur != null)
                {
                    UserService.UtilisateurActif = _utilisateur;
                }
            }

            // 🔥 DÉFINIR LE DATACONTEXT pour les stats dynamiques
            if (_utilisateur != null)
            {
                this.DataContext = _utilisateur;
                Console.WriteLine($"✅ DataContext défini pour ExportsPage : {_utilisateur.Pseudo}");
                Console.WriteLine($"   Données à exporter :");
                Console.WriteLine($"   - {_utilisateur.ListeTaches.Count} tâches");
                Console.WriteLine($"   - {_utilisateur.ListeActivites.Count} activités");
                Console.WriteLine($"   - {_utilisateur.ListeAgenda.Count} événements agenda");
            }
            else
            {
                Console.WriteLine("⚠️ Aucun utilisateur trouvé pour ExportsPage");

                // Afficher un message à l'utilisateur
                MessageBox.Show("❌ Aucun utilisateur trouvé.\nVeuillez vous connecter ou importer des données.",
                               "Aucune donnée", MessageBoxButton.OK, MessageBoxImage.Warning);
            }

            // 🆕 S'abonner aux changements d'utilisateur (après import par exemple)
            UserService.UtilisateurActifChanged += OnUtilisateurChanged;
        }

        // 🆕 Méthode appelée quand l'utilisateur change (après import)
        private void OnUtilisateurChanged(Utilisateur? nouvelUtilisateur)
        {
            _utilisateur = nouvelUtilisateur;
            this.DataContext = nouvelUtilisateur;

            if (nouvelUtilisateur != null)
            {
                Console.WriteLine($"🔄 ExportsPage - Utilisateur changé : {nouvelUtilisateur.Pseudo}");
                Console.WriteLine($"   Nouvelles données : {nouvelUtilisateur.ListeTaches.Count} tâches");
            }
        }

        // === ÉVÉNEMENTS D'EXPORT (améliorés avec validation) ===

        private void ExportJSON_Click(object sender, RoutedEventArgs e)
        {
            if (!VerifierUtilisateur()) return;

            var dialog = new SaveFileDialog
            {
                Filter = "Fichier JSON (*.json)|*.json",
                FileName = $"{_utilisateur.Pseudo}_complet.json"
            };

            if (dialog.ShowDialog() == true)
            {
                try
                {
                    _exportService.ExportJson(_utilisateur, dialog.FileName);
                    AfficherSuccesExport("JSON", dialog.FileName);
                }
                catch (Exception ex)
                {
                    AfficherErreurExport("JSON", ex.Message);
                }
            }
        }

        private void ExportSerialize_Click(object sender, RoutedEventArgs e)
        {
            if (!VerifierUtilisateur()) return;

            var dialog = new SaveFileDialog
            {
                Filter = "Fichier XML (*.xml)|*.xml",
                FileName = $"{_utilisateur.Pseudo}_complet.xml"
            };

            if (dialog.ShowDialog() == true)
            {
                try
                {
                    _exportService.ExportXml(_utilisateur, dialog.FileName);
                    AfficherSuccesExport("XML", dialog.FileName);
                }
                catch (Exception ex)
                {
                    AfficherErreurExport("XML", ex.Message);
                }
            }
        }

        private void ExportCSV_Click(object sender, RoutedEventArgs e)
        {
            if (!VerifierUtilisateur()) return;

            var dialog = new SaveFileDialog
            {
                Filter = "Fichier CSV (*.csv)|*.csv",
                FileName = $"{_utilisateur.Pseudo}_complet.csv"
            };

            if (dialog.ShowDialog() == true)
            {
                try
                {
                    ExportCSVToFile(dialog.FileName);
                    AfficherSuccesExport("CSV", dialog.FileName);
                }
                catch (Exception ex)
                {
                    AfficherErreurExport("CSV", ex.Message);
                }
            }
        }

        private void ScheduleExport_Click(object sender, RoutedEventArgs e)
        {
            if (!VerifierUtilisateur()) return;

            var dialog = new SaveFileDialog
            {
                Filter = "Fichier TXT (*.txt)|*.txt",
                FileName = $"{_utilisateur.Pseudo}_rapport.txt"
            };

            if (dialog.ShowDialog() == true)
            {
                try
                {
                    ExportTXTToFile(dialog.FileName);
                    AfficherSuccesExport("TXT", dialog.FileName);
                }
                catch (Exception ex)
                {
                    AfficherErreurExport("TXT", ex.Message);
                }
            }
        }

        private void ExportAll_Click(object sender, RoutedEventArgs e)
        {
            if (!VerifierUtilisateur()) return;

            try
            {
                string folder = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Exports");
                Directory.CreateDirectory(folder);

                string baseFileName = $"{_utilisateur.Pseudo}_{DateTime.Now:yyyyMMdd_HHmmss}";

                // Export dans tous les formats
                _exportService.ExportJson(_utilisateur, Path.Combine(folder, $"{baseFileName}.json"));
                _exportService.ExportXml(_utilisateur, Path.Combine(folder, $"{baseFileName}.xml"));
                ExportCSVToFile(Path.Combine(folder, $"{baseFileName}.csv"));
                ExportTXTToFile(Path.Combine(folder, $"{baseFileName}.txt"));

                // Message de succès avec statistiques
                var totalTaches = _utilisateur.ListeTaches.Count;
                var totalActivites = _utilisateur.ListeActivites.Count;
                var totalAgenda = _utilisateur.ListeAgenda.Count;
              

                MessageBox.Show($"✅ Export complet réussi !\n\n" +
                               $"📂 Emplacement : {folder}\n" +
                               $"📊 Données exportées :\n" +
                               $"   • {totalTaches} tâches\n" +
                               $"   • {totalActivites} activités\n" +
                               $"   • {totalAgenda} événements agenda\n" +
                               $"📁 Formats créés : JSON, XML, CSV, TXT",
                               "Export terminé", MessageBoxButton.OK, MessageBoxImage.Information);

                Console.WriteLine($"✅ Export complet réussi pour {_utilisateur.Pseudo}");
            }
            catch (Exception ex)
            {
                MessageBox.Show($"❌ Erreur lors de l'export complet :\n{ex.Message}",
                               "Erreur", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        // === MÉTHODES UTILITAIRES ===

        private bool VerifierUtilisateur()
        {
            if (_utilisateur == null)
            {
                MessageBox.Show("❌ Aucun utilisateur chargé.\nVeuillez vous connecter ou importer des données.",
                               "Aucune donnée", MessageBoxButton.OK, MessageBoxImage.Warning);
                return false;
            }
            return true;
        }

        private void AfficherSuccesExport(string format, string fichier)
        {
            var tailleFichier = new FileInfo(fichier).Length;
            string tailleFormatee = FormatFileSize(tailleFichier);

            MessageBox.Show($"✅ Export {format} réussi !\n\n" +
                           $"📁 Fichier : {Path.GetFileName(fichier)}\n" +
                           $"📏 Taille : {tailleFormatee}\n" +
                           $"📂 Emplacement : {Path.GetDirectoryName(fichier)}",
                           "Export terminé", MessageBoxButton.OK, MessageBoxImage.Information);

            Console.WriteLine($"✅ Export {format} réussi : {fichier} ({tailleFormatee})");
        }

        private void AfficherErreurExport(string format, string erreur)
        {
            MessageBox.Show($"❌ Erreur lors de l'export {format} :\n{erreur}",
                           "Erreur d'export", MessageBoxButton.OK, MessageBoxImage.Error);
            Console.WriteLine($"❌ Erreur export {format} : {erreur}");
        }

        private string FormatFileSize(long bytes)
        {
            if (bytes < 1024)
                return $"{bytes} B";
            else if (bytes < 1024 * 1024)
                return $"{bytes / 1024:F1} KB";
            else
                return $"{bytes / (1024 * 1024):F1} MB";
        }

        // 🆕 Méthode publique pour rafraîchir (si nécessaire)
        public void RafraichirDonnees()
        {
            InitialiserUtilisateur();
        }

        // === MÉTHODES D'EXPORT (conservées) ===

        private void ExportCSVToFile(string path)
        {
            var sb = new StringBuilder();

            sb.AppendLine("==== Utilisateur ====");
            sb.AppendLine($"Pseudo,{_utilisateur.Pseudo}");
            sb.AppendLine($"Nom,{_utilisateur.Nom}");
            sb.AppendLine($"Prenom,{_utilisateur.Prenom}");
            sb.AppendLine($"Email,{_utilisateur.Email}");
            sb.AppendLine($"Age,{_utilisateur.Age}");
            sb.AppendLine($"Taille,{_utilisateur.Taille}");
            sb.AppendLine($"Poids,{_utilisateur.Poids}");
            sb.AppendLine($"ObjectifPoids,{_utilisateur.ObjectifPoids}");
            sb.AppendLine($"DateInscription,{_utilisateur.DateInscription:yyyy-MM-dd}");
            sb.AppendLine($"DateObjectif,{_utilisateur.DateObjectif:yyyy-MM-dd}");
            sb.AppendLine();

            sb.AppendLine("==== Tâches ====");
            sb.AppendLine("Description,EstTerminee");
            foreach (var t in _utilisateur.ListeTaches)
                sb.AppendLine($"{t.Description},{t.EstTerminee}");
            sb.AppendLine();

            sb.AppendLine("==== Activités ====");
            sb.AppendLine("Titre,Duree,CaloriesBrulees");
            foreach (var a in _utilisateur.ListeActivites)
                sb.AppendLine($"{a.Titre},{a.Duree},{a.CaloriesBrulees}");
            sb.AppendLine();

            sb.AppendLine("==== Agenda ====");
            sb.AppendLine("HeureDebut,HeureFin,Date,ActiviteTitre");
            foreach (var a in _utilisateur.ListeAgenda)
                sb.AppendLine($"{a.HeureDebut},{a.HeureFin},{a.Date:yyyy-MM-dd},{a.Activite?.Titre}");
            sb.AppendLine();

            File.WriteAllText(path, sb.ToString());
        }

        private void ExportTXTToFile(string path)
        {
            var sb = new StringBuilder();

            sb.AppendLine("=== UTILISATEUR ===");
            sb.AppendLine($"Pseudo : {_utilisateur.Pseudo}");
            sb.AppendLine($"Nom : {_utilisateur.Nom}");
            sb.AppendLine($"Prénom : {_utilisateur.Prenom}");
            sb.AppendLine($"Email : {_utilisateur.Email}");
            sb.AppendLine($"Age : {_utilisateur.Age}");
            sb.AppendLine($"Taille : {_utilisateur.Taille} cm");
            sb.AppendLine($"Poids : {_utilisateur.Poids} kg");
            sb.AppendLine($"IMC : {_utilisateur.IMC:F1}");
            sb.AppendLine($"Objectif : {_utilisateur.ObjectifPoids} kg d'ici le {_utilisateur.DateObjectif:dd/MM/yyyy}");
            sb.AppendLine($"Date d'inscription : {_utilisateur.DateInscription:dd/MM/yyyy}");
            sb.AppendLine();

            sb.AppendLine("=== TÂCHES ===");
            foreach (var t in _utilisateur.ListeTaches)
                sb.AppendLine($"- {t.Description} ({(t.EstTerminee ? "✅ terminée" : "⏳ en cours")})");
            sb.AppendLine();

            sb.AppendLine("=== ACTIVITÉS ===");
            foreach (var a in _utilisateur.ListeActivites)
                sb.AppendLine($"- {a.Titre} : {a.Duree} min, {a.CaloriesBrulees} kcal");
            sb.AppendLine();

            sb.AppendLine("=== AGENDA ===");
            foreach (var ag in _utilisateur.ListeAgenda)
                sb.AppendLine($"- {ag.Date:dd/MM/yyyy} : {ag.HeureDebut}-{ag.HeureFin} ({ag.Activite?.Titre})");
            sb.AppendLine();


            File.WriteAllText(path, sb.ToString());
        }

        // 🆕 Nettoyer les événements quand la page se ferme
        ~ExportsPage()
        {
            UserService.UtilisateurActifChanged -= OnUtilisateurChanged;
        }
    }
}