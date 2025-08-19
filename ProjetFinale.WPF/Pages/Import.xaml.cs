using Microsoft.Win32;
using ProjetFinale.Models;
using ProjetFinale.Services;
using ProjetFinale.Utils;
using System;
using System.Collections.ObjectModel;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.Json;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Xml.Serialization;

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
            Loaded += (_, __) => RenderHistory();   // reconstruit à chaque arrivée sur la page
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
                case "JSON": ImportJsonFile(filePath); break;
                case "CSV": ImportCsvFile(filePath); break; // remplace l’utilisateur
                case "XML": ImportXmlFile(filePath); break;
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

        // === CSV : remplace entièrement l'utilisateur + ses listes (compatible avec TON export) ===
        private void ImportCsvFile(string filePath)
        {
            var lines = File.ReadAllLines(filePath, Encoding.UTF8);
            if (lines.Length == 0) throw new Exception("CSV vide.");

            var u = new Utilisateur
            {
                // on initialise les collections (évite null)
                ListeActivites = new System.Collections.Generic.List<Activite>(),
                ListeAgenda = new ObservableCollection<Agenda>(),
                ListeTaches = new ObservableCollection<Tache>()
            };

            int i = 0;
            while (i < lines.Length)
            {
                var line = (lines[i] ?? "").Trim();
                i++;

                if (string.IsNullOrWhiteSpace(line)) continue;
                if (!line.StartsWith("====", StringComparison.OrdinalIgnoreCase)) continue;

                if (line.Contains("Utilisateur", StringComparison.OrdinalIgnoreCase))
                {
                    while (i < lines.Length && !string.IsNullOrWhiteSpace(lines[i]))
                    {
                        var (key, val) = SplitKV(lines[i]);
                        ApplyUserKV(u, key, val);
                        i++;
                    }
                }
                else if (line.Contains("Tâches", StringComparison.OrdinalIgnoreCase) ||
                         line.Contains("Taches", StringComparison.OrdinalIgnoreCase))
                {
                    if (i < lines.Length && StartsWithIC(lines[i], "Description")) i++;

                    while (i < lines.Length && !string.IsNullOrWhiteSpace(lines[i]))
                    {
                        var cols = SplitCsv(lines[i]);
                        if (cols.Length >= 1)
                        {
                            u.ListeTaches.Add(new Tache
                            {
                                Description = cols[0].Trim(),
                                EstTerminee = cols.Length >= 2 && ParseBool(cols[1]),
                                Date = DateTime.Now
                            });
                        }
                        i++;
                    }
                }
                else if (line.Contains("Activités", StringComparison.OrdinalIgnoreCase) ||
                         line.Contains("Activites", StringComparison.OrdinalIgnoreCase))
                {
                    // saute l'en-tête
                    if (i < lines.Length && StartsWithIC(lines[i], "Titre")) i++;

                    while (i < lines.Length && !string.IsNullOrWhiteSpace(lines[i]))
                    {
                        var cols = SplitCsv(lines[i]);
                        if (cols.Length < 4)
                            throw new Exception("Ligne 'Activités' invalide : 4 colonnes attendues (Titre,Duree,ImagePath,CaloriesBrulees).");

                        u.ListeActivites.Add(new Activite
                        {
                            Titre = cols[0].Trim(),
                            // TimeSpan attendu : accepte "90" (minutes) ou "01:30[:00]"
                            Duree = ParseTimeSpanFlexible(cols[1]),
                            ImagePath = cols[2].Trim(),
                            CaloriesBrulees = ParseInt(cols[3])
                        });

                        i++;
                    }
                }
                else if (line.Contains("Agenda", StringComparison.OrdinalIgnoreCase))
                {
                    // Skip header: ancien ("HeureDebut,...") ou nouveau ("Titre,...")
                    if (i < lines.Length && (StartsWithIC(lines[i], "HeureDebut") || StartsWithIC(lines[i], "Titre")))
                        i++;

                    while (i < lines.Length && !string.IsNullOrWhiteSpace(lines[i]))
                    {
                        var cols = SplitCsv(lines[i]);

                        // Au minimum on veut 3 colonnes
                        if (cols.Length >= 3)
                        {
                            // Nouveau format si >= 6 colonnes : Titre,HeureDebut,HeureFin,Date,ActiviteTitre,Couleur[,Description]
                            bool nouveauFormat = cols.Length >= 6;

                            if (nouveauFormat)
                            {
                                // 0  : Titre (Agenda)
                                // 1-2: HeureDebut / HeureFin
                                // 3  : Date
                                // 4  : ActiviteTitre (optionnel)
                                // 5  : Couleur (optionnel)
                                // 6  : Description (optionnel)
                                var titreAgenda = cols[0].Trim();
                                var heureDebut = ParseTimeSpanFlexible(cols[1]);
                                var heureFin = ParseTimeSpanFlexible(cols[2]);
                                var date = ParseDate(cols[3]);

                                Activite? act = null;
                                if (cols.Length >= 5)
                                {
                                    var actTitre = cols[4].Trim();
                                    if (!string.IsNullOrEmpty(actTitre))
                                        act = u.ListeActivites.FirstOrDefault(a =>
                                            a?.Titre?.Equals(actTitre, StringComparison.OrdinalIgnoreCase) == true);
                                }

                                var agenda = new Agenda
                                {
                                    Titre = titreAgenda,
                                    HeureDebut = heureDebut,
                                    HeureFin = heureFin,
                                    Date = date,
                                    Activite = act
                                };

                                if (cols.Length >= 6) agenda.Couleur = cols[5].Trim();
                                if (cols.Length >= 7) agenda.Description = cols[6]; // garde tel quel

                                u.ListeAgenda.Add(agenda);
                            }
                            else
                            {
                                // Ancien format: HeureDebut,HeureFin,Date,ActiviteTitre
                                Activite? act = null;
                                if (cols.Length >= 4)
                                {
                                    var actTitre = cols[3].Trim();
                                    if (!string.IsNullOrEmpty(actTitre))
                                        act = u.ListeActivites.FirstOrDefault(a =>
                                            a?.Titre?.Equals(actTitre, StringComparison.OrdinalIgnoreCase) == true);
                                }

                                u.ListeAgenda.Add(new Agenda
                                {
                                    HeureDebut = ParseTimeSpanFlexible(cols[0]),
                                    HeureFin = ParseTimeSpanFlexible(cols[1]),
                                    Date = ParseDate(cols[2]),
                                    Activite = act
                                });
                            }
                        }

                        i++;
                    }
                }

                // sauter les lignes vides entre sections
                while (i < lines.Length && string.IsNullOrWhiteSpace(lines[i])) i++;
            }

            PersistAndBroadcast(u);

            MessageBox.Show(
                $"✅ Données utilisateur (CSV) importées et remplacées !\n\n" +
                $"👤 Utilisateur : {u.Pseudo}\n" +
                $"📅 Âge : {u.Age}\n" +
                $"⚖️ Poids : {u.Poids}\n" +
                $"📏 Taille : {u.Taille}",
                "Import CSV", MessageBoxButton.OK, MessageBoxImage.Information);

            RefreshAccueilPage();
        }

        // ===== Helpers minimalistes =====
        private static (string key, string val) SplitKV(string line)
        {
            var parts = line.Split(',', 2);            // coupe en 2 morceaux max
            var key = parts[0].Trim();
            var val = parts.Length > 1 ? parts[1].Trim() : "";
            return (key, val);
        }

        private static string[] SplitCsv(string line)
            => (line ?? "").Split(',').Select(s => s.Trim()).ToArray();

        private static bool StartsWithIC(string s, string prefix)
            => s?.StartsWith(prefix, StringComparison.OrdinalIgnoreCase) == true;

        private static int ParseInt(string s)
            => int.TryParse(s, NumberStyles.Integer, CultureInfo.InvariantCulture, out var n) ? n : 0;

        private static double ParseDouble(string s)
            => double.TryParse(s, NumberStyles.Float, CultureInfo.InvariantCulture, out var d) ? d : 0d;

        private static bool ParseBool(string s)
            => s.Equals("true", StringComparison.OrdinalIgnoreCase) || s == "1";

        private static DateTime ParseDate(string s)
        {
            if (DateTime.TryParseExact(s, "yyyy-MM-dd", CultureInfo.InvariantCulture, DateTimeStyles.None, out var dt))
                return dt;
            return DateTime.TryParse(s, CultureInfo.InvariantCulture, DateTimeStyles.AssumeLocal, out dt) ? dt : DateTime.MinValue;
        }

        // Mappe tous les champs "Utilisateur" exportés
        private static void ApplyUserKV(Utilisateur u, string key, string val)
        {
            switch (key.ToLowerInvariant())
            {
                case "pseudo": u.Pseudo = val; break;
                case "nom": u.Nom = val; break;
                case "prenom": u.Prenom = val; break;
                case "email": u.Email = val; break;
                case "mdphash":
                    if (!TrySetStringProperty(u, "MdpHash", val))
                    {
                        if (!TrySetStringProperty(u, "MDPHash", val))
                        {
                            TrySetStringProperty(u, "MotDePasseHash", val);
                        }
                    }
                    break;

                case "age": u.Age = ParseInt(val); break;
                case "poids": u.Poids = ParseDouble(val); break;
                case "taille": u.Taille = ParseDouble(val); break;
                case "objectifpoids": u.ObjectifPoids = ParseDouble(val); break;

                case "dateinscription": u.DateInscription = ParseDate(val); break;
                case "dateobjectif": u.DateObjectif = ParseDate(val); break;
            }
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
            ImportHistoryService.Add(new ImportHistoryEntry(format, fileName, fileSize, importedAt));
            RenderHistory(); // rafraîchit l’affichage
        }

        private Border BuildHistoryItem(string format, string fileName, long fileSize, DateTime importedAt)
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
            return item;
        }

        // === Utils ===
        private static string GetFormatIcon(string format) => format switch
        {
            "CSV" => "📊",
            "JSON" => "📄",
            "XML" => "📜",
            _ => "📁"
        };
        private void RenderHistory()
        {
            // Vide le conteneur
            HistoryGrid.Children.Clear();

            var list = ImportHistoryService.Load();
            if (list.Count == 0)
            {
                // texte “Aucun import” si vide
                HistoryGrid.Children.Add(new TextBlock
                {
                    Text = "Aucun import effectué pour le moment",
                    Foreground = new SolidColorBrush(Color.FromArgb(255, 204, 204, 204)),
                    HorizontalAlignment = HorizontalAlignment.Center,
                    Margin = new Thickness(0, 8, 0, 8)
                });
                return;
            }

            var stack = new StackPanel();
            foreach (var h in list)
                stack.Children.Add(BuildHistoryItem(h.Format, h.FileName, h.FileSize, h.ImportedAt));

            HistoryGrid.Children.Add(stack);
        }
        private static string FormatFileSize(long bytes)
        {
            if (bytes < 1024) return $"{bytes} B";
            double kb = bytes / 1024d;
            if (kb < 1024) return $"{kb:F1} KB";
            double mb = kb / 1024d;
            return $"{mb:F1} MB";
        }

        private static TimeSpan ParseTimeSpanFlexible(string s)
        {
            if (TimeSpan.TryParse(s, out var ts)) return ts;
            if (int.TryParse(s, NumberStyles.Integer, CultureInfo.InvariantCulture, out var minutes))
                return TimeSpan.FromMinutes(minutes);
            return TimeSpan.Zero;
        }

        private static bool TrySetStringProperty(object target, string propertyName, string value)
        {
            var prop = target.GetType()
                             .GetProperties(BindingFlags.Public | BindingFlags.Instance)
                             .FirstOrDefault(p => p.CanWrite &&
                                                  string.Equals(p.Name, propertyName, StringComparison.OrdinalIgnoreCase) &&
                                                  p.PropertyType == typeof(string));
            if (prop == null) return false;
            prop.SetValue(target, value);
            return true;
        }


    }
}
