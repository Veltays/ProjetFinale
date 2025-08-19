using Microsoft.Win32;
using ProjetFinale.Models;
using ProjetFinale.Services;
using ProjetFinale.Utils;
using System;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Xml.Serialization;
using System.Reflection;

namespace ProjetFinale.WPF
{
    public partial class ImportPage : Page
    {
        // Limites (octets)
        private const long MAX_JSON_BYTES = 2L * 1024 * 1024;
        private const long MAX_CSV_BYTES = 5L * 1024 * 1024;
        private const long MAX_XML_BYTES = 5L * 1024 * 1024;

        // Couleurs UI
        private static readonly SolidColorBrush AccentBrush = new(Color.FromArgb(255, 175, 102, 255));
        private static readonly SolidColorBrush MutedTextBrush = new(Color.FromArgb(255, 204, 204, 204));

        public ImportPage()
        {
            InitializeComponent();
        }

        // === Boutons ===
        private void ImportCSV_Click(object sender, RoutedEventArgs e)
            => OpenAndProcess("Sélectionner un fichier CSV",
                              "Fichiers CSV (*.csv)|*.csv|Tous les fichiers (*.*)|*.*",
                              "csv", "CSV", MAX_CSV_BYTES);

        private void ImportJSON_Click(object sender, RoutedEventArgs e)
            => OpenAndProcess("Sélectionner un fichier JSON",
                              "Fichiers JSON (*.json)|*.json|Tous les fichiers (*.*)|*.*",
                              "json", "JSON", MAX_JSON_BYTES);

        private void ImportXML_Click(object sender, RoutedEventArgs e)
            => OpenAndProcess("Sélectionner un fichier XML",
                              "Fichiers XML (*.xml)|*.xml|Tous les fichiers (*.*)|*.*",
                              "xml", "XML", MAX_XML_BYTES);

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
                    ImportCsvFile(filePath);   // ⬅️ maintenant : remplace l’utilisateur
                    break;

                case "XML":
                    ImportXmlFile(filePath);
                    break;
            }
        }

        // === JSON : remplace l'utilisateur ===
        private void ImportJsonFile(string filePath)
        {
            var json = File.ReadAllText(filePath, Encoding.UTF8);
            var utilisateur = JsonSerializer.Deserialize<Utilisateur>(json)
                ?? throw new Exception("Impossible de désérialiser le fichier JSON.");

            PersistAndBroadcast(utilisateur);

            MessageBox.Show(
                $"✅ Données utilisateur (JSON) importées et remplacées !\n\n" +
                $"👤 Utilisateur : {utilisateur.Pseudo}\n" +
                $"📅 Âge : {utilisateur.Age}\n" +
                $"⚖️ Poids : {utilisateur.Poids}\n" +
                $"📏 Taille : {utilisateur.Taille}",
                "Import JSON", MessageBoxButton.OK, MessageBoxImage.Information);

            RefreshAccueilPage();
        }

        // === XML : remplace l'utilisateur ===
        private void ImportXmlFile(string filePath)
        {
            var ser = new XmlSerializer(typeof(Utilisateur));
            using var fs = File.OpenRead(filePath);
            if (ser.Deserialize(fs) is not Utilisateur utilisateur)
                throw new Exception("Impossible de désérialiser le fichier XML.");

            PersistAndBroadcast(utilisateur);

            MessageBox.Show(
                $"✅ Données utilisateur (XML) importées et remplacées !\n\n" +
                $"👤 Utilisateur : {utilisateur.Pseudo}\n" +
                $"📅 Âge : {utilisateur.Age}\n" +
                $"⚖️ Poids : {utilisateur.Poids}\n" +
                $"📏 Taille : {utilisateur.Taille}",
                "Import XML", MessageBoxButton.OK, MessageBoxImage.Information);

            RefreshAccueilPage();
        }

        // === CSV : remplace l'utilisateur ===
        // Attendu : un CSV **une ligne** (ou plusieurs), avec entête colonnes correspondant aux propriétés simples de Utilisateur.
        // Exemple minimal (séparateur ';' ou ','):
        // Pseudo;Age;Poids;Taille
        // Yassine;21;72.5;178
        //
        // Si plusieurs lignes : on prend la **première** ligne de données.
        private void ImportCsvFile(string filePath)
        {
            var lines = File.ReadAllLines(filePath, Encoding.UTF8)
                            .Where(l => !string.IsNullOrWhiteSpace(l))
                            .ToArray();

            if (lines.Length < 2)
                throw new Exception("CSV invalide : entête + au moins une ligne de données sont requis.");

            // Détecte séparateur ; ou , automatiquement
            char sep = DetectCsvSeparator(lines[0]);

            var headers = SplitCsvLine(lines[0], sep);
            var values = SplitCsvLine(lines[1], sep); // 1ère ligne de données

            if (values.Length != headers.Length)
                throw new Exception("CSV invalide : le nombre de valeurs ne correspond pas au nombre de colonnes.");

            var utilisateur = new Utilisateur();
            MapSimplePropertiesFromCsv(utilisateur, headers, values);

            PersistAndBroadcast(utilisateur);

            MessageBox.Show(
                $"✅ Données utilisateur (CSV) importées et remplacées !\n\n" +
                $"👤 Utilisateur : {utilisateur.Pseudo}\n" +
                $"📅 Âge : {utilisateur.Age}\n" +
                $"⚖️ Poids : {utilisateur.Poids}\n" +
                $"📏 Taille : {utilisateur.Taille}",
                "Import CSV", MessageBoxButton.OK, MessageBoxImage.Information);

            RefreshAccueilPage();
        }

        // Mappe seulement les propriétés "simples" usuelles (string, int, double, bool, DateTime) par nom de colonne.
        private static void MapSimplePropertiesFromCsv(object target, string[] headers, string[] values)
        {
            var type = target.GetType();
            var props = type.GetProperties(BindingFlags.Public | BindingFlags.Instance)
                            .Where(p => p.CanWrite);

            // dictionnaire provisoire header->value
            for (int i = 0; i < headers.Length; i++)
            {
                var name = headers[i].Trim();
                var value = values[i].Trim();

                var prop = props.FirstOrDefault(p => p.Name.Equals(name, StringComparison.OrdinalIgnoreCase));
                if (prop == null) continue;

                object? converted = ConvertString(value, prop.PropertyType);
                prop.SetValue(target, converted);
            }
        }

        private static object? ConvertString(string value, Type targetType)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                // valeurs par défaut nullables
                if (!targetType.IsValueType || Nullable.GetUnderlyingType(targetType) != null)
                    return null;
            }

            var t = Nullable.GetUnderlyingType(targetType) ?? targetType;

            if (t == typeof(string)) return value;
            if (t == typeof(int)) return int.TryParse(value, NumberStyles.Any, CultureInfo.InvariantCulture, out var i) ? i : 0;
            if (t == typeof(double)) return double.TryParse(value, NumberStyles.Any, CultureInfo.InvariantCulture, out var d) ? d : 0d;
            if (t == typeof(float)) return float.TryParse(value, NumberStyles.Any, CultureInfo.InvariantCulture, out var f) ? f : 0f;
            if (t == typeof(decimal)) return decimal.TryParse(value, NumberStyles.Any, CultureInfo.InvariantCulture, out var m) ? m : 0m;
            if (t == typeof(bool)) return value.Equals("1") || value.Equals("true", StringComparison.OrdinalIgnoreCase);
            if (t == typeof(DateTime)) return DateTime.TryParse(value, CultureInfo.InvariantCulture, DateTimeStyles.AssumeLocal, out var dt) ? dt : DateTime.MinValue;

            // Types non gérés (collections, objets complexes) : on ignore proprement.
            return null;
        }

        private static char DetectCsvSeparator(string headerLine)
        {
            // simple heuristique
            var comma = headerLine.Count(c => c == ',');
            var semi = headerLine.Count(c => c == ';');
            return semi >= comma ? ';' : ',';
        }

        private static string[] SplitCsvLine(string line, char sep)
        {
            // Split simple (sans guillemets imbriqués). Suffisant pour notre usage.
            // Si besoin de CSV avancé (quotes/escapes), on pourra upgrader.
            return line.Split(sep).Select(s => s.Trim()).ToArray();
        }

        // === Persistance + propagation ===
        private static void PersistAndBroadcast(Utilisateur utilisateur)
        {
            JsonService.SauvegarderUtilisateur(utilisateur);
            UserService.UtilisateurActif = utilisateur;
        }

        // Navigue pour forcer un rafraîchissement visuel (facultatif)
        private void RefreshAccueilPage()
        {
            try
            {
                if (Application.Current.MainWindow is Views.MainWindow main)
                {
                    main.NavigateToAccueil();
                    Console.WriteLine("🔄 Page Accueil rafraîchie");
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
            "XML" => "📜",
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