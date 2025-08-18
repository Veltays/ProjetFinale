using Microsoft.Win32;
using ProjetFinale.Models;
using ProjetFinale.Services;
using ProjetFinale.Utils;
using System;
using System.IO;
using System.Text.Json;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace ProjetFinale.WPF
{
    public partial class ImportPage : Page
    {
        // Limites (octets)
        private const long MAX_JSON_BYTES = 2L * 1024 * 1024;
        private const long MAX_CSV_BYTES = 5L * 1024 * 1024;
        private const long MAX_XLS_BYTES = 10L * 1024 * 1024;

        // Couleurs UI (garde le même rendu)
        private static readonly SolidColorBrush AccentBrush = new(Color.FromArgb(255, 175, 102, 255));
        private static readonly SolidColorBrush MutedTextBrush = new(Color.FromArgb(255, 204, 204, 204));

        public ImportPage()
        {
            InitializeComponent();
        }

        // === Handlers boutons ===
        private void ImportCSV_Click(object sender, RoutedEventArgs e)
            => OpenAndProcess("Sélectionner un fichier CSV", "Fichiers CSV (*.csv)|*.csv|Tous les fichiers (*.*)|*.*", "csv", "CSV", MAX_CSV_BYTES);

        private void ImportJSON_Click(object sender, RoutedEventArgs e)
            => OpenAndProcess("Sélectionner un fichier JSON", "Fichiers JSON (*.json)|*.json|Tous les fichiers (*.*)|*.*", "json", "JSON", MAX_JSON_BYTES);

        private void ImportExcel_Click(object sender, RoutedEventArgs e)
            => OpenAndProcess("Sélectionner un fichier Excel", "Fichiers Excel (*.xlsx;*.xls)|*.xlsx;*.xls|Tous les fichiers (*.*)|*.*", "xlsx", "EXCEL", MAX_XLS_BYTES);

        // === Orchestration commune ===
        private void OpenAndProcess(string title, string filter, string defaultExt, string format, long maxBytes)
        {
            var dlg = new OpenFileDialog { Title = title, Filter = filter, DefaultExt = defaultExt };
            if (dlg.ShowDialog() != true) return;

            var file = new FileInfo(dlg.FileName);
            if (file.Length > maxBytes)
            {
                MessageBox.Show(
                    $"Le fichier est trop volumineux.\nTaille max : {FormatFileSize(maxBytes)}",
                    "Erreur", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            try
            {
                ProcessImport(format, file.FullName, file.Length);
                MessageBox.Show(
                    $"Import {format} réussi !\nFichier : {file.Name}\nTaille : {FormatFileSize(file.Length)}",
                    "Import terminé", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Erreur lors de l'import {format} :\n{ex.Message}",
                    "Erreur", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void ProcessImport(string format, string filePath, long fileSize)
        {
            AddToHistory(format, Path.GetFileName(filePath), fileSize, DateTime.Now);

            switch (format)
            {
                case "JSON":
                    ImportJsonFile(filePath);
                    break;

                case "CSV":
                    MessageBox.Show("Import CSV pas encore implémenté", "Info",
                        MessageBoxButton.OK, MessageBoxImage.Information);
                    break;

                case "EXCEL":
                    MessageBox.Show("Import Excel pas encore implémenté", "Info",
                        MessageBoxButton.OK, MessageBoxImage.Information);
                    break;
            }
        }

        // === JSON ===
        private void ImportJsonFile(string filePath)
        {
            Console.WriteLine($"🔄 Début import JSON : {filePath}");

            var json = File.ReadAllText(filePath);
            var utilisateur = JsonSerializer.Deserialize<Utilisateur>(json)
                ?? throw new Exception("Impossible de désérialiser le fichier JSON");

            Console.WriteLine($"✅ Utilisateur désérialisé : {utilisateur.Pseudo}, Age: {utilisateur.Age}");

            // Remplace les données locales
            JsonService.SauvegarderUtilisateur(utilisateur);
            UserService.UtilisateurActif = utilisateur;

            Console.WriteLine("💾 Utilisateur sauvegardé + UtilisateurActif mis à jour");

            RefreshAccueilPage();

            MessageBox.Show(
                $"✅ Données utilisateur importées et remplacées !\n\n" +
                $"👤 Utilisateur : {utilisateur.Pseudo}\n" +
                $"📅 Âge : {utilisateur.Age} ans\n" +
                $"⚖️ Poids : {utilisateur.Poids} kg\n" +
                $"📏 Taille : {utilisateur.Taille} cm\n\n" +
                $"➡️ Allez sur la page Account pour voir les changements !",
                "Import réussi", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        // Navigue pour forcer un rafraîchissement visuel
        private void RefreshAccueilPage()
        {
            try
            {
                if (Application.Current.MainWindow is Views.MainWindow main)
                {
                    main.NavigateToAccueil();
                    Console.WriteLine("🔄 Page Account/Accueil rafraîchie");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"⚠️ Erreur lors du rafraîchissement : {ex.Message}");
            }
        }

        // === Historique UI ===
        private void AddToHistory(string format, string fileName, long fileSize, DateTime importedAt)
        {
            var item = new Border
            {
                Background = Brushes.Transparent,
                BorderBrush = AccentBrush,
                BorderThickness = new Thickness(1),
                CornerRadius = new CornerRadius(8),
                Margin = new Thickness(0, 5, 0, 5),
            };

            var row = new StackPanel { Orientation = Orientation.Horizontal };

            var icon = new TextBlock
            {
                Text = GetFormatIcon(format),
                FontSize = 16,
                Foreground = AccentBrush,
                VerticalAlignment = VerticalAlignment.Center,
                Margin = new Thickness(0, 0, 15, 0)
            };

            var info = new StackPanel();
            info.Children.Add(new TextBlock
            {
                Text = fileName,
                Foreground = Brushes.White,
                FontWeight = FontWeights.Bold,
                FontSize = 14
            });
            info.Children.Add(new TextBlock
            {
                Text = $"{format} • {FormatFileSize(fileSize)} • {importedAt:dd/MM/yyyy HH:mm}",
                Foreground = MutedTextBrush,
                FontSize = 12
            });

            row.Children.Add(icon);
            row.Children.Add(info);
            item.Child = row;

            // Ajoute en haut, garde 5 éléments max
            if (HistoryGrid.Children.Count == 1 && HistoryGrid.Children[0] is TextBlock)
            {
                HistoryGrid.Children.Clear();
                var stack = new StackPanel();
                stack.Children.Add(item);
                HistoryGrid.Children.Add(stack);
                return;
            }

            if (HistoryGrid.Children.Count > 0 && HistoryGrid.Children[0] is StackPanel s)
            {
                s.Children.Insert(0, item);
                if (s.Children.Count > 5) s.Children.RemoveAt(s.Children.Count - 1);
            }
        }

        // === Utils ===
        private static string GetFormatIcon(string format) => format switch
        {
            "CSV" => "📊",
            "JSON" => "📄",
            "EXCEL" => "📈",
            _ => "📁"
        };

        private static string FormatFileSize(long bytes)
        {
            if (bytes < 1024) return $"{bytes} B";

            double kb = bytes / 1024d;
            if (kb < 1024) return $"{kb:F1} KB";

            double mb = kb / 1024d;
            return $"{mb:F1} MB";
        }
    }
}
