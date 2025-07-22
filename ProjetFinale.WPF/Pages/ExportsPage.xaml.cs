using Microsoft.Win32;
using ProjetFinale.Models;
using ProjetFinale.Utils;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Windows;
using System.Windows.Controls;

namespace ProjetFinale.WPF
{
    public partial class ExportsPage : Page
    {
        private readonly ExportService _exportService = new ExportService();
        private readonly Utilisateur? _utilisateur;

        public ExportsPage()
        {
            InitializeComponent();
            _utilisateur = JsonService.ChargerUtilisateur();
        }

        private void ExportJSON_Click(object sender, RoutedEventArgs e)
        {
            if (_utilisateur == null)
            {
                MessageBox.Show("❌ Aucun utilisateur chargé.");
                return;
            }

            var dialog = new SaveFileDialog
            {
                Filter = "Fichier JSON (*.json)|*.json",
                FileName = "utilisateur_complet.json"
            };

            if (dialog.ShowDialog() == true)
            {
                _exportService.ExportJson(_utilisateur, dialog.FileName);
                MessageBox.Show("✅ Export JSON terminé !");
            }
        }

        private void ExportSerialize_Click(object sender, RoutedEventArgs e)
        {
            if (_utilisateur == null)
            {
                MessageBox.Show("❌ Aucun utilisateur chargé.");
                return;
            }

            var dialog = new SaveFileDialog
            {
                Filter = "Fichier XML (*.xml)|*.xml",
                FileName = "utilisateur_complet.xml"
            };

            if (dialog.ShowDialog() == true)
            {
                _exportService.ExportXml(_utilisateur, dialog.FileName);
                MessageBox.Show("✅ Export XML terminé !");
            }
        }

        private void ExportCSV_Click(object sender, RoutedEventArgs e)
        {
            if (_utilisateur == null)
            {
                MessageBox.Show("❌ Aucun utilisateur chargé.");
                return;
            }

            var dialog = new SaveFileDialog
            {
                Filter = "Fichier CSV (*.csv)|*.csv",
                FileName = "utilisateur_complet.csv"
            };

            if (dialog.ShowDialog() == true)
            {
                ExportCSVToFile(dialog.FileName);
                MessageBox.Show("✅ Export CSV terminé !");
            }
        }

        private void ScheduleExport_Click(object sender, RoutedEventArgs e)
        {
            if (_utilisateur == null)
            {
                MessageBox.Show("❌ Aucun utilisateur chargé.");
                return;
            }

            var dialog = new SaveFileDialog
            {
                Filter = "Fichier TXT (*.txt)|*.txt",
                FileName = "utilisateur_complet.txt"
            };

            if (dialog.ShowDialog() == true)
            {
                ExportTXTToFile(dialog.FileName);
                MessageBox.Show("✅ Export TXT terminé !");
            }
        }

        private void ExportAll_Click(object sender, RoutedEventArgs e)
        {
            if (_utilisateur == null)
            {
                MessageBox.Show("❌ Aucun utilisateur chargé.");
                return;
            }

            string folder = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Exports");
            Directory.CreateDirectory(folder);

            _exportService.ExportJson(_utilisateur, Path.Combine(folder, "utilisateur_complet.json"));
            _exportService.ExportXml(_utilisateur, Path.Combine(folder, "utilisateur_complet.xml"));
            ExportCSVToFile(Path.Combine(folder, "utilisateur_complet.csv"));
            ExportTXTToFile(Path.Combine(folder, "utilisateur_complet.txt"));

            MessageBox.Show("✅ Tous les formats ont été exportés dans le dossier Exports/");
        }

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

            sb.AppendLine("==== Statistiques ====");
            sb.AppendLine("TypeStat,Valeur,Unite");
            foreach (var s in _utilisateur.ListeStatistiques)
                sb.AppendLine($"{s.Type},{s.Valeur},{s.Unite}");

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
            sb.AppendLine($"Taille : {_utilisateur.Taille} m");
            sb.AppendLine($"Poids : {_utilisateur.Poids} kg");
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

            sb.AppendLine("=== STATISTIQUES ===");
            foreach (var s in _utilisateur.ListeStatistiques)
                sb.AppendLine($"- {s.Type} : {s.Valeur} {s.Unite}");

            File.WriteAllText(path, sb.ToString());
        }
    }
}
