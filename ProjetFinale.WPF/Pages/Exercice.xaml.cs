using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using Microsoft.Win32;
using ProjetFinale.Models;
using ProjetFinale.Services;
using ProjetFinale.Utils;

namespace ProjetFinale.Views
{
    public partial class ExercicesPage : Page
    {
        // ===== Données =====
        private List<Activite> _exercices = new();
        private List<Activite> _exercicesFiltres = new();
        private Activite? _exerciceEnEdition;
        private bool _estEnModeEdition;

        // ===== Utilisateur =====
        private Utilisateur? _utilisateur;

        // ===== Images (formulaire) =====
        private string? _imagePathForm; // chemin fichier de l'image choisie (en création/édition)
        private readonly string _imagesDir = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
            "ProjetFinale", "Images"
        );

        // Si pas d’image fournie, on utilise un visuel de secours (présent dans /Images)
        private const string DEFAULT_RELATIVE_PLACEHOLDER = "/Images/default_exercise.png";

        public ExercicesPage()
        {
            InitializeComponent();

            Directory.CreateDirectory(_imagesDir);

            ChargerUtilisateur();     // 1) charge l’utilisateur
            ChargerActivites();       // 2) charge la liste d’activités (utilisateur ou défaut)
            AppliquerFiltre();        // 3) filtre courant (barre de recherche)
            RafraichirListe(); 
            BinderDataGrid();        // 4) reconstruit l’UI liste


            // Désabonnement propre
            Loaded += OnLoaded;                     // ⬅️ faire l’init quand la page est prête
            Unloaded += (_, __) => UserService.UtilisateurActifChanged -= OnUtilisateurChanged;
        }

        // ------------------------------------------------------------
        // Cycle de vie / chargement
        // ------------------------------------------------------------
        private void OnLoaded(object sender, RoutedEventArgs e)
        {
            // éviter double init si la page est rechargée
            Loaded -= OnLoaded;

            Directory.CreateDirectory(_imagesDir);

            ChargerUtilisateur();     // charge l’utilisateur
            ChargerActivites();       // charge les activités
            AppliquerFiltre();        // applique le filtre (barre de recherche)
            RafraichirListe(); 
            BinderDataGrid();        // reconstruit la liste (le XAML est garanti non-null ici)

            UserService.UtilisateurActifChanged += OnUtilisateurChanged;
        }

        private void ChargerUtilisateur()
        {
            _utilisateur = UserService.UtilisateurActif ?? JsonService.ChargerUtilisateur();
            if (UserService.UtilisateurActif == null && _utilisateur != null)
                UserService.UtilisateurActif = _utilisateur;
        }

        private void OnUtilisateurChanged(Utilisateur? nouvelUtilisateur)
        {
            _utilisateur = nouvelUtilisateur;
            ChargerActivites();
            AppliquerFiltre();
            RafraichirListe(); 
            BinderDataGrid();
        }

        private void ChargerActivites()
        {
            // Source de vérité : ListeActivites de l’utilisateur si dispo
            if (_utilisateur?.ListeActivites is { Count: > 0 } list)
            {
                _exercices = new List<Activite>(list);
            }
            else
            {
                // Plus d'exercices par défaut → juste une liste vide
                _exercices = new List<Activite>();
                _utilisateur?.ListeActivites.Clear();
                UserService.MettreAJourUtilisateur(_utilisateur);
            }

            _exercicesFiltres = new List<Activite>(_exercices);

            // Reset UI image formulaire
            _imagePathForm = null;
            RafraichirPreview(null);
            BinderDataGrid();
        }

        // ------------------------------------------------------------
        // Formulaire (type, image, ajout/modif)
        // ------------------------------------------------------------

        private void TypeComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            // Affiche le bon groupe selon le type sélectionné
            var type = GetSelectedType();
            RepetitionsGroup.Visibility = type == "repetitions" ? Visibility.Visible : Visibility.Collapsed;
            DurationGroup.Visibility = type == "duration" ? Visibility.Visible : Visibility.Collapsed;
        }

        private void BtnPickImage_Click(object sender, RoutedEventArgs e)
        {
            var ofd = new OpenFileDialog
            {
                Title = "Choisir une image",
                Filter = "Images|*.png;*.jpg;*.jpeg;*.webp;*.gif;*.bmp",
                Multiselect = false
            };
            if (ofd.ShowDialog() != true) return;

            try
            {
                var dest = Path.Combine(_imagesDir, $"{Guid.NewGuid():N}{Path.GetExtension(ofd.FileName)}");
                File.Copy(ofd.FileName, dest, overwrite: true);

                _imagePathForm = dest;
                TxtImageName.Text = Path.GetFileName(ofd.FileName);
                RafraichirPreview(_imagePathForm);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Impossible de copier l'image.\n{ex.Message}",
                                "Erreur", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void AddButton_Click(object sender, RoutedEventArgs e)
        {
            if (!ValiderFormulaire()) return;

            var nom = ExerciseNameBox.Text.Trim();
            var type = GetSelectedType();
            var duree = type == "repetitions"
                ? TimeSpan.FromMinutes(EstimerDureeRepetitions(RepetitionsBox.Text.Trim()))
                : ParserDuree(DurationBox.Text.Trim());

            float.TryParse(CaloriesBox.Text.Trim(), out var calories);

            // Image à stocker (fichier copié ou placeholder)
            var imagePathFinal = string.IsNullOrWhiteSpace(_imagePathForm)
                ? DEFAULT_RELATIVE_PLACEHOLDER
                : _imagePathForm;

            if (_estEnModeEdition && _exerciceEnEdition != null)
            {
                _exerciceEnEdition.Titre = nom;
                _exerciceEnEdition.CaloriesBrulees = calories;
                _exerciceEnEdition.Duree = duree;
                if (!string.IsNullOrWhiteSpace(_imagePathForm))
                    _exerciceEnEdition.ImagePath = imagePathFinal;

                PersisterUtilisateur();
                MessageBox.Show("Exercice modifié avec succès!", "Succès", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            else
            {
                var nouvelle = new Activite
                {
                    Titre = nom,
                    CaloriesBrulees = calories,
                    Duree = duree,
                    ImagePath = imagePathFinal
                };

                _exercices.Add(nouvelle);
                _utilisateur?.ListeActivites.Add(nouvelle);
                PersisterUtilisateur();
                MessageBox.Show("Exercice créé avec succès!", "Succès", MessageBoxButton.OK, MessageBoxImage.Information);
            }

            ResetFormulaire();
            AppliquerFiltre();
            RafraichirListe(); 
            BinderDataGrid();
        }

        // ------------------------------------------------------------
        // Recherche / filtre
        // ------------------------------------------------------------

        private void SearchBox_GotFocus(object sender, RoutedEventArgs e)
        {
            if (SearchBox.Text == "Rechercher un exercice...")
            {
                SearchBox.Text = "";
                SearchBox.Foreground = (Brush)FindResource("App.Foreground");
            }
        }

        private void SearchBox_LostFocus(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(SearchBox.Text))
            {
                SearchBox.Text = "Rechercher un exercice...";
                SearchBox.Foreground = new SolidColorBrush(System.Windows.Media.Color.FromRgb(170, 170, 170));
            }
        }

        private void SearchBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            AppliquerFiltre();
            RafraichirListe(); BinderDataGrid();
        }

        private void AppliquerFiltre()
        {
            var term = (SearchBox.Text ?? "").ToLower();
            if (string.IsNullOrWhiteSpace(term) || term == "rechercher un exercice...")
            {
                _exercicesFiltres = new List<Activite>(_exercices);
                return;
            }

            _exercicesFiltres = _exercices
                .Where(ex => !string.IsNullOrWhiteSpace(ex.Titre) &&
                             ex.Titre.ToLower().Contains(term))
                .ToList();
        }

        // ------------------------------------------------------------
        // Liste (cartes) + actions
        // ------------------------------------------------------------

        private void RafraichirListe()
        {
            if (ExercisesListPanel == null) return;           // ⬅️ garde
            if (NoExercisesMessage == null) return;           // ⬅️ garde

            ExercisesListPanel.Children.Clear();

            if (_exercicesFiltres.Count == 0)
            {
                NoExercisesMessage.Visibility = Visibility.Visible;
                return;
            }

            NoExercisesMessage.Visibility = Visibility.Collapsed;
            foreach (var ex in _exercicesFiltres)
                ExercisesListPanel.Children.Add(CreerCarte(ex));
        }

        private Border CreerCarte(Activite exercice)
        {
            var card = new Border { Style = (Style)FindResource("ExercicesCardStyle") };

            var grid = new Grid();
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });                     // image
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(2, GridUnitType.Star) }); // nom
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) }); // durée
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) }); // calories
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });                      // actions

            // Miniature
            var image = new Image
            {
                Width = 42,
                Height = 42,
                Margin = new Thickness(10, 4, 14, 4),
                Stretch = Stretch.UniformToFill,
                SnapsToDevicePixels = true
            };
            SetImageSource(image, exercice.ImagePath);
            Grid.SetColumn(image, 0);

            // Titre
            var titre = new TextBlock
            {
                Text = exercice.Titre,
                FontWeight = FontWeights.Bold,
                Foreground = (Brush)FindResource("App.Foreground"),
                VerticalAlignment = VerticalAlignment.Center,
                FontSize = 14
            };
            Grid.SetColumn(titre, 1);

            // Durée
            var duree = new TextBlock
            {
                Text = $"⏱️ {exercice.Duree.TotalMinutes:F0} min",
                Foreground = (Brush)FindResource("Text.Muted"),
                VerticalAlignment = VerticalAlignment.Center,
                FontSize = 12
            };
            Grid.SetColumn(duree, 2);

            // Calories
            var calories = new TextBlock
            {
                Text = $"🔥 {exercice.CaloriesBrulees:F0} cal",
                Foreground = (Brush)FindResource("Text.Muted"),
                VerticalAlignment = VerticalAlignment.Center,
                FontSize = 12
            };
            Grid.SetColumn(calories, 3);

            // Actions
            var actions = new StackPanel { Orientation = Orientation.Horizontal, HorizontalAlignment = HorizontalAlignment.Right };

            var edit = new Button
            {
                Content = "MODIFIER",
                Style = (Style)FindResource("ExercicesPrimaryButtonStyle"),
                Margin = new Thickness(0, 0, 12, 0),
                MinWidth = 90,
                Tag = exercice
            };
            edit.Click += Edit_Click;

            var del = new Button
            {
                Content = "SUPPRIMER",
                Style = (Style)FindResource("ExercicesDeleteButtonStyle"),
                MinWidth = 90,
                Tag = exercice
            };
            del.Click += Delete_Click;

            actions.Children.Add(edit);
            actions.Children.Add(del);
            Grid.SetColumn(actions, 4);

            grid.Children.Add(image);
            grid.Children.Add(titre);
            grid.Children.Add(duree);
            grid.Children.Add(calories);
            grid.Children.Add(actions);

            card.Child = grid;
            return card;
        }

        private void Edit_Click(object sender, RoutedEventArgs e)
        {
            if ((sender as Button)?.Tag is not Activite ex) return;

            _estEnModeEdition = true;
            _exerciceEnEdition = ex;

            ExerciseNameBox.Text = ex.Titre;
            CaloriesBox.Text = ex.CaloriesBrulees.ToString();

            // Déduire un type simple selon la durée (pas critique, juste pour préremplir)
            if (ex.Duree.TotalMinutes <= 10)
                SetSelectedType("repetitions", () => RepetitionsBox.Text = $"10 x {ex.Duree.TotalMinutes:F0}");
            else
                SetSelectedType("duration", () => DurationBox.Text = $"{ex.Duree.TotalMinutes:F0} minutes");

            // Preview image si fichier absolu dispo
            _imagePathForm = (Path.IsPathRooted(ex.ImagePath) && File.Exists(ex.ImagePath)) ? ex.ImagePath : null;
            TxtImageName.Text = _imagePathForm != null ? Path.GetFileName(_imagePathForm) : "Aucune image sélectionnée";
            RafraichirPreview(_imagePathForm);

            AddButton.Content = "MODIFIER";
        }

        private void Delete_Click(object sender, RoutedEventArgs e)
        {
            if ((sender as Button)?.Tag is not Activite ex) return;

            var confirm = MessageBox.Show(
                $"Supprimer l'exercice '{ex.Titre}' ?",
                "Confirmation", MessageBoxButton.YesNo, MessageBoxImage.Question);

            if (confirm != MessageBoxResult.Yes) return;

            _exercices.Remove(ex);
            _utilisateur?.ListeActivites.Remove(ex);
            PersisterUtilisateur();

            AppliquerFiltre();
            RafraichirListe(); BinderDataGrid();

            MessageBox.Show("Exercice supprimé avec succès!", "Succès",
                MessageBoxButton.OK, MessageBoxImage.Information);
        }

        // ------------------------------------------------------------
        // Helpers (brefs et réutilisables)
        // ------------------------------------------------------------

        private void PersisterUtilisateur()
        {
            if (_utilisateur != null)
                UserService.MettreAJourUtilisateur(_utilisateur);
        }

        private string GetSelectedType()
        {
            return (TypeComboBox.SelectedItem as ComboBoxItem)?.Tag?.ToString() ?? "";
        }

        private void SetSelectedType(string tag, Action afterSelect)
        {
            foreach (ComboBoxItem item in TypeComboBox.Items)
                if (item.Tag?.ToString() == tag) { TypeComboBox.SelectedItem = item; break; }

            TypeComboBox_SelectionChanged(TypeComboBox, null);
            afterSelect?.Invoke();
        }

        private void SetImageSource(Image img, string? path)
        {
            try
            {
                if (!string.IsNullOrWhiteSpace(path) && Path.IsPathRooted(path) && File.Exists(path))
                {
                    img.Source = new BitmapImage(new Uri(path));
                }
                else
                {
                    // pack URI relatif vers une ressource incluse (si elle existe)
                    img.Source = new BitmapImage(new Uri(DEFAULT_RELATIVE_PLACEHOLDER, UriKind.Relative));
                }
            }
            catch
            {
                img.Source = null;
            }
        }

        private void RafraichirPreview(string? path)
        {
            try
            {
                if (!string.IsNullOrWhiteSpace(path) && File.Exists(path))
                {
                    PreviewImage.Source = new BitmapImage(new Uri(path));
                    PreviewImage.Visibility = Visibility.Visible;
                }
                else
                {
                    PreviewImage.Source = null;
                    PreviewImage.Visibility = Visibility.Collapsed;
                }
            }
            catch
            {
                PreviewImage.Source = null;
                PreviewImage.Visibility = Visibility.Collapsed;
            }
        }

        private bool ValiderFormulaire()
        {
            if (string.IsNullOrWhiteSpace(ExerciseNameBox.Text))
            {
                MessageBox.Show("Veuillez entrer un nom pour l'exercice.", "Validation",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                ExerciseNameBox.Focus();
                return false;
            }

            if (TypeComboBox.SelectedItem == null)
            {
                MessageBox.Show("Veuillez sélectionner un type d'exercice.", "Validation",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return false;
            }

            var type = GetSelectedType();
            if (type == "repetitions" && string.IsNullOrWhiteSpace(RepetitionsBox.Text))
            {
                MessageBox.Show("Veuillez entrer le nombre de répétitions.", "Validation",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                RepetitionsBox.Focus();
                return false;
            }

            if (type == "duration" && string.IsNullOrWhiteSpace(DurationBox.Text))
            {
                MessageBox.Show("Veuillez entrer la durée de l'exercice.", "Validation",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                DurationBox.Focus();
                return false;
            }

            return true;
        }

        private void ResetFormulaire()
        {
            ExerciseNameBox.Text = "";
            RepetitionsBox.Text = "";
            DurationBox.Text = "";
            CaloriesBox.Text = "";
            DescriptionBox.Text = "";

            TypeComboBox.SelectedItem = null;
            RepetitionsGroup.Visibility = Visibility.Collapsed;
            DurationGroup.Visibility = Visibility.Collapsed;

            _imagePathForm = null;
            TxtImageName.Text = "Aucune image sélectionnée";
            RafraichirPreview(null);

            _estEnModeEdition = false;
            _exerciceEnEdition = null;
            AddButton.Content = "AJOUTER";
        }

        private int EstimerDureeRepetitions(string repsValue)
        {
            // Heuristique simple : "12 x 3" -> 3 minutes (1 min/série)
            try
            {
                var parts = (repsValue ?? "").ToLower().Split('x');
                return parts.Length == 2 && int.TryParse(parts[1].Trim(), out var series) ? series : 15;
            }
            catch { return 15; }
        }

        private TimeSpan ParserDuree(string dureeStr)
        {
            // Accepte "30", "30min", "30 minutes", "1h30"
            try
            {
                var s = (dureeStr ?? "").ToLower().Replace(" ", "");
                if (s.Contains("minute")) s = s.Replace("minutes", "").Replace("minute", "");
                if (s.EndsWith("min")) s = s.Replace("min", "");

                if (s.Contains('h'))
                {
                    var p = s.Split('h');
                    var h = int.Parse(p[0]);
                    var m = (p.Length > 1 && !string.IsNullOrEmpty(p[1])) ? int.Parse(p[1]) : 0;
                    return TimeSpan.FromHours(h) + TimeSpan.FromMinutes(m);
                }

                if (int.TryParse(s, out var onlyMinutes))
                    return TimeSpan.FromMinutes(onlyMinutes);

                return TimeSpan.FromMinutes(30);
            }
            catch { return TimeSpan.FromMinutes(30); }
        }


        private void BinderDataGrid()
        {
            if (ExercisesGrid == null) return;
            ExercisesGrid.ItemsSource = _exercicesFiltres;
            ExercisesGrid.Items.Refresh();
        }



    }
}
