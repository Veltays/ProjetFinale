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
        public ImportPage()
        {
            InitializeComponent();
        }

        private void ImportCSV_Click(object sender, RoutedEventArgs e)
        {
            var openFileDialog = new OpenFileDialog
            {
                Title = "Sélectionner un fichier CSV",
                Filter = "Fichiers CSV (*.csv)|*.csv|Tous les fichiers (*.*)|*.*",
                DefaultExt = "csv"
            };

            if (openFileDialog.ShowDialog() == true)
            {
                try
                {
                    // Vérifier la taille du fichier (5 MB max)
                    var fileInfo = new FileInfo(openFileDialog.FileName);
                    if (fileInfo.Length > 5 * 1024 * 1024)
                    {
                        MessageBox.Show("Le fichier est trop volumineux. Taille maximum : 5 MB",
                                       "Erreur", MessageBoxButton.OK, MessageBoxImage.Warning);
                        return;
                    }

                    // Traitement de l'import CSV
                    ProcessImport("CSV", openFileDialog.FileName, fileInfo.Length);

                    MessageBox.Show($"Import CSV réussi !\nFichier : {Path.GetFileName(openFileDialog.FileName)}\nTaille : {FormatFileSize(fileInfo.Length)}",
                                   "Import terminé", MessageBoxButton.OK, MessageBoxImage.Information);
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Erreur lors de l'import CSV :\n{ex.Message}",
                                   "Erreur", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        private void ImportJSON_Click(object sender, RoutedEventArgs e)
        {
            var openFileDialog = new OpenFileDialog
            {
                Title = "Sélectionner un fichier JSON",
                Filter = "Fichiers JSON (*.json)|*.json|Tous les fichiers (*.*)|*.*",
                DefaultExt = "json"
            };

            if (openFileDialog.ShowDialog() == true)
            {
                try
                {
                    // Vérifier la taille du fichier (2 MB max)
                    var fileInfo = new FileInfo(openFileDialog.FileName);
                    if (fileInfo.Length > 2 * 1024 * 1024)
                    {
                        MessageBox.Show("Le fichier est trop volumineux. Taille maximum : 2 MB",
                                       "Erreur", MessageBoxButton.OK, MessageBoxImage.Warning);
                        return;
                    }

                    // Traitement de l'import JSON
                    ProcessImport("JSON", openFileDialog.FileName, fileInfo.Length);

                    MessageBox.Show($"Import JSON réussi !\nFichier : {Path.GetFileName(openFileDialog.FileName)}\nTaille : {FormatFileSize(fileInfo.Length)}",
                                   "Import terminé", MessageBoxButton.OK, MessageBoxImage.Information);
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Erreur lors de l'import JSON :\n{ex.Message}",
                                   "Erreur", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        private void ImportExcel_Click(object sender, RoutedEventArgs e)
        {
            var openFileDialog = new OpenFileDialog
            {
                Title = "Sélectionner un fichier Excel",
                Filter = "Fichiers Excel (*.xlsx;*.xls)|*.xlsx;*.xls|Tous les fichiers (*.*)|*.*",
                DefaultExt = "xlsx"
            };

            if (openFileDialog.ShowDialog() == true)
            {
                try
                {
                    // Vérifier la taille du fichier (10 MB max)
                    var fileInfo = new FileInfo(openFileDialog.FileName);
                    if (fileInfo.Length > 10 * 1024 * 1024)
                    {
                        MessageBox.Show("Le fichier est trop volumineux. Taille maximum : 10 MB",
                                       "Erreur", MessageBoxButton.OK, MessageBoxImage.Warning);
                        return;
                    }

                    // Traitement de l'import Excel
                    ProcessImport("EXCEL", openFileDialog.FileName, fileInfo.Length);

                    MessageBox.Show($"Import Excel réussi !\nFichier : {Path.GetFileName(openFileDialog.FileName)}\nTaille : {FormatFileSize(fileInfo.Length)}",
                                   "Import terminé", MessageBoxButton.OK, MessageBoxImage.Information);
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Erreur lors de l'import Excel :\n{ex.Message}",
                                   "Erreur", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        private void ProcessImport(string format, string filePath, long fileSize)
        {
            try
            {
                // Ajouter à l'historique
                AddToHistory(format, Path.GetFileName(filePath), fileSize, DateTime.Now);

                // Logique spécifique par format
                switch (format)
                {
                    case "JSON":
                        ImportJsonFile(filePath);
                        break;
                    case "CSV":
                        // TODO: Implémenter l'import CSV
                        MessageBox.Show("Import CSV pas encore implémenté", "Info",
                                      MessageBoxButton.OK, MessageBoxImage.Information);
                        break;
                    case "EXCEL":
                        // TODO: Implémenter l'import Excel
                        MessageBox.Show("Import Excel pas encore implémenté", "Info",
                                      MessageBoxButton.OK, MessageBoxImage.Information);
                        break;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Erreur lors de l'import {format} :\n{ex.Message}",
                               "Erreur", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void ImportJsonFile(string filePath)
        {
            try
            {
                Console.WriteLine($"🔄 Début import JSON : {filePath}");

                // Lire le fichier JSON
                var json = File.ReadAllText(filePath);
                var utilisateurImporte = JsonSerializer.Deserialize<Utilisateur>(json);

                if (utilisateurImporte != null)
                {
                    Console.WriteLine($"✅ Utilisateur désérialisé : {utilisateurImporte.Pseudo}, Age: {utilisateurImporte.Age}");

                    // ⚠️ ÉCRASEMENT COMPLET des données Account
                    JsonService.SauvegarderUtilisateur(utilisateurImporte);
                    Console.WriteLine("💾 Utilisateur sauvegardé dans le fichier JSON");

                    // Mettre à jour l'utilisateur actif
                    UserService.UtilisateurActif = utilisateurImporte;
                    Console.WriteLine("🔄 UtilisateurActif mis à jour");

                    // 🔥 NOUVEAU : Rafraîchir automatiquement la page Account
                    RafraichirPageAccount();

                    // Message de succès avec détails
                    MessageBox.Show($"✅ Données utilisateur importées et remplacées !\n\n" +
                                   $"👤 Utilisateur : {utilisateurImporte.Pseudo}\n" +
                                   $"📅 Âge : {utilisateurImporte.Age} ans\n" +
                                   $"⚖️ Poids : {utilisateurImporte.Poids} kg\n" +
                                   $"📏 Taille : {utilisateurImporte.Taille} cm\n\n" +
                                   $"➡️ Allez sur la page Account pour voir les changements !",
                                   "Import réussi", MessageBoxButton.OK, MessageBoxImage.Information);
                }
                else
                {
                    throw new Exception("Impossible de désérialiser le fichier JSON");
                }
            }
            catch (JsonException jsonEx)
            {
                throw new Exception($"Format JSON invalide : {jsonEx.Message}");
            }
            catch (Exception ex)
            {
                throw new Exception($"Erreur lors de l'import : {ex.Message}");
            }
        }

        // 🚀 NOUVELLE MÉTHODE : Rafraîchir la page Account
        private void RafraichirPageAccount()
        {
            try
            {
                var mainWindow = Application.Current.MainWindow as Views.MainWindow;
                if (mainWindow != null)
                {
                    // Naviguer vers la page Account pour forcer le rafraîchissement
                    mainWindow.NavigateToAccueil();
                    Console.WriteLine("🔄 Page Account rafraîchie automatiquement");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"⚠️ Erreur lors du rafraîchissement : {ex.Message}");
                // L'erreur n'est pas critique, on continue
            }
        }

        private void AddToHistory(string format, string fileName, long fileSize, DateTime importDate)
        {
            // Créer un nouvel élément d'historique
            var historyItem = new Border
            {
                Background = Brushes.Transparent,
                BorderBrush = new SolidColorBrush(Color.FromArgb(255, 175, 102, 255)),
                BorderThickness = new Thickness(1),
                CornerRadius = new CornerRadius(8),
                Margin = new Thickness(0, 5, 0, 5),
                //Padding = new Thickness(15, 10)
            };

            var stackPanel = new StackPanel { Orientation = Orientation.Horizontal };

            // Icône format
            var formatIcon = new TextBlock
            {
                Text = GetFormatIcon(format),
                FontSize = 16,
                Foreground = new SolidColorBrush(Color.FromArgb(255, 175, 102, 255)),
                VerticalAlignment = VerticalAlignment.Center,
                Margin = new Thickness(0, 0, 15, 0)
            };

            // Info fichier
            var fileInfo = new StackPanel();
            var fileNameText = new TextBlock
            {
                Text = fileName,
                Foreground = Brushes.White,
                FontWeight = FontWeights.Bold,
                FontSize = 14
            };
            var detailsText = new TextBlock
            {
                Text = $"{format} • {FormatFileSize(fileSize)} • {importDate:dd/MM/yyyy HH:mm}",
                Foreground = new SolidColorBrush(Color.FromArgb(255, 204, 204, 204)),
                FontSize = 12
            };

            fileInfo.Children.Add(fileNameText);
            fileInfo.Children.Add(detailsText);

            stackPanel.Children.Add(formatIcon);
            stackPanel.Children.Add(fileInfo);
            historyItem.Child = stackPanel;

            // Ajouter au début de l'historique
            if (HistoryGrid.Children.Count == 1 && HistoryGrid.Children[0] is TextBlock)
            {
                // Remplacer le message "Aucun import"
                HistoryGrid.Children.Clear();
                var historyStack = new StackPanel();
                historyStack.Children.Add(historyItem);
                HistoryGrid.Children.Add(historyStack);
            }
            else if (HistoryGrid.Children[0] is StackPanel existingStack)
            {
                existingStack.Children.Insert(0, historyItem);

                // Garder seulement les 5 derniers imports
                if (existingStack.Children.Count > 5)
                {
                    existingStack.Children.RemoveAt(existingStack.Children.Count - 1);
                }
            }
        }

        private string GetFormatIcon(string format)
        {
            return format switch
            {
                "CSV" => "📊",
                "JSON" => "📄",
                "EXCEL" => "📈",
                _ => "📁"
            };
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
    }
}