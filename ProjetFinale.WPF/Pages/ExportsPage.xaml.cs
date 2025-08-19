using Microsoft.Win32;
using ProjetFinale.Models;
using ProjetFinale.Services;
using ProjetFinale.Utils;
using System;
using System.IO;
using System.Windows;
using System.Windows.Controls;

namespace ProjetFinale.WPF
{
    public partial class ExportsPage : Page
    {
        private readonly ExportService _exportService = new();
        private Utilisateur? _utilisateur;

        private const string DefaultXmlDir =
            @"C:\Users\grany\OneDrive\HEPL\BAC2\Q2\Programmation orientée objet en C#\ProjetFinale\ProjetFinale.WPF\Datafile\";

        public ExportsPage()
        {
            InitializeComponent();
            InitialiserUtilisateur();
            UserService.UtilisateurActifChanged += OnUtilisateurChanged;
            Unloaded += (_, __) => UserService.UtilisateurActifChanged -= OnUtilisateurChanged;
        }

        private void InitialiserUtilisateur()
        {
            _utilisateur = UserService.UtilisateurActif ?? JsonService.ChargerUtilisateur();
            if (UserService.UtilisateurActif == null && _utilisateur != null)
                UserService.UtilisateurActif = _utilisateur;

            if (_utilisateur != null)
                DataContext = _utilisateur;
            else
                MessageBox.Show("❌ Aucun utilisateur trouvé.\nVeuillez vous connecter ou importer des données.",
                                "Aucune donnée", MessageBoxButton.OK, MessageBoxImage.Warning);
        }

        private void OnUtilisateurChanged(Utilisateur? u)
        {
            _utilisateur = u;
            DataContext = u;
        }

        // === Exports ===
        private void ExportJSON_Click(object sender, RoutedEventArgs e)
        {
            if (_utilisateur == null) { AlerteNoUser(); return; }

            var dlg = new SaveFileDialog { Filter = "Fichier JSON (*.json)|*.json", FileName = $"{_utilisateur.Pseudo}_complet.json" };
            if (dlg.ShowDialog() != true) return;

            try { _exportService.ExportJson(_utilisateur, dlg.FileName); Succes("JSON", dlg.FileName); }
            catch (Exception ex) { Erreur("JSON", ex.Message); }
        }

        private void ExportSerialize_Click(object sender, RoutedEventArgs e)
        {
            if (_utilisateur == null) { AlerteNoUser(); return; }

            var dlg = new SaveFileDialog
            {
                Filter = "Fichier XML (*.xml)|*.xml",
                FileName = $"{_utilisateur.Pseudo}_complet.xml",
                InitialDirectory = Directory.Exists(DefaultXmlDir)
                    ? DefaultXmlDir
                    : Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments)
            };
            if (dlg.ShowDialog() != true) return;

            try { _exportService.ExportXml(_utilisateur, dlg.FileName); Succes("XML", dlg.FileName); }
            catch (Exception ex) { Erreur("XML", ex.Message); }
        }

        private void ExportCSV_Click(object sender, RoutedEventArgs e)
        {
            if (_utilisateur == null) { AlerteNoUser(); return; }

            var dlg = new SaveFileDialog { Filter = "Fichier CSV (*.csv)|*.csv", FileName = $"{_utilisateur.Pseudo}_complet.csv" };
            if (dlg.ShowDialog() != true) return;

            try { _exportService.ExportCsv(_utilisateur, dlg.FileName); Succes("CSV", dlg.FileName); }
            catch (Exception ex) { Erreur("CSV", ex.Message); }
        }

        private void ExportAll_Click(object sender, RoutedEventArgs e)
        {
            if (_utilisateur == null) { AlerteNoUser(); return; }

            try
            {
                var folder = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Exports");
                Directory.CreateDirectory(folder);
                var baseName = $"{_utilisateur.Pseudo}_{DateTime.Now:yyyyMMdd_HHmmss}";

                var json = Path.Combine(folder, $"{baseName}.json");
                var xml = Path.Combine(folder, $"{baseName}.xml");
                var csv = Path.Combine(folder, $"{baseName}.csv");

                _exportService.ExportJson(_utilisateur, json);
                _exportService.ExportXml(_utilisateur, xml);
                _exportService.ExportCsv(_utilisateur, csv);

                MessageBox.Show(
                    $"✅ Export complet réussi !\n\n" +
                    $"📂 Emplacement : {folder}\n" +
                    $"📊 Données exportées :\n" +
                    $"   • {_utilisateur.ListeTaches?.Count ?? 0} tâches\n" +
                    $"   • {_utilisateur.ListeActivites?.Count ?? 0} activités\n" +
                    $"   • {_utilisateur.ListeAgenda?.Count ?? 0} événements agenda\n" +
                    $"📁 Formats créés : JSON, XML, CSV",
                    "Export terminé", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                Erreur("COMPLET", ex.Message);
            }
        }

        // === UI utils courts ===
        private static void AlerteNoUser() =>
            MessageBox.Show("❌ Aucun utilisateur chargé.\nVeuillez vous connecter ou importer des données.",
                            "Aucune donnée", MessageBoxButton.OK, MessageBoxImage.Warning);

        private static void Succes(string format, string fichier)
        {
            var size = new FileInfo(fichier).Length;
            var human = size < 1024 ? $"{size} B" : (size < 1024 * 1024 ? $"{size / 1024d:F1} KB" : $"{size / 1024d / 1024d:F1} MB");

            MessageBox.Show(
                $"✅ Export {format} réussi !\n\n" +
                $"📁 Fichier : {Path.GetFileName(fichier)}\n" +
                $"📏 Taille : {human}\n" +
                $"📂 Emplacement : {Path.GetDirectoryName(fichier)}",
                "Export terminé", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private static void Erreur(string format, string msg) =>
            MessageBox.Show($"❌ Erreur lors de l'export {format} :\n{msg}",
                            "Erreur d'export", MessageBoxButton.OK, MessageBoxImage.Error);
    }
}