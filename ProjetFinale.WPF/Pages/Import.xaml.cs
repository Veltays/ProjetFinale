using Microsoft.Win32;
using System;
using System.IO;
using System.Windows;
using System.Windows.Controls;

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

                    // TODO: Implémenter la logique d'import CSV
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

                    // TODO: Implémenter la logique d'import JSON
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

                    // TODO: Implémenter la logique d'import Excel
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
            // Ajouter à l'historique
            AddToHistory(format, Path.GetFileName(filePath), fileSize, DateTime.Now);

            // TODO: Ici tu peux ajouter la logique spécifique pour chaque format :
            // - Parser le CSV avec ta logique existante
            // - Traiter le JSON 
            // - Lire l'Excel avec EPPlus ou autre
            // - Sauvegarder dans ta base de données
        }

        private void AddToHistory(string format, string fileName, long fileSize, DateTime importDate)
        {
            // Créer un nouvel élément d'historique
            var historyItem = new Border
            {
                Background = System.Windows.Media.Brushes.Transparent,
                BorderBrush = new System.Windows.Media.SolidColorBrush(System.Windows.Media.Color.FromArgb(255, 175, 102, 255)),
                BorderThickness = new Thickness(1),
                CornerRadius = new CornerRadius(8),

            };

            var stackPanel = new StackPanel { Orientation = Orientation.Horizontal };

            // Icône format
            var formatIcon = new TextBlock
            {
                Text = GetFormatIcon(format),
                FontSize = 16,
                Foreground = new System.Windows.Media.SolidColorBrush(System.Windows.Media.Color.FromArgb(255, 175, 102, 255)),
                VerticalAlignment = VerticalAlignment.Center,
                Margin = new Thickness(0, 0, 15, 0)
            };

            // Info fichier
            var fileInfo = new StackPanel();
            var fileNameText = new TextBlock
            {
                Text = fileName,
                Foreground = System.Windows.Media.Brushes.White,
                FontWeight = FontWeights.Bold,
                FontSize = 14
            };
            var detailsText = new TextBlock
            {
                Text = $"{format} • {FormatFileSize(fileSize)} • {importDate:dd/MM/yyyy HH:mm}",
                Foreground = new System.Windows.Media.SolidColorBrush(System.Windows.Media.Color.FromArgb(255, 204, 204, 204)),
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