using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using Microsoft.Win32;

namespace ProjetFinale.Views
{
    public partial class ExercicesPage : Page
    {
        // Liste des exercices
        private List<Exercice> _exercices;
        private List<Exercice> _exercicesFiltres;

        // Exercice en cours d'édition
        private Exercice _exerciceEnEdition;
        private bool _estEnModeEdition = false;

        public ExercicesPage()
        {
            InitializeComponent();
            InitialiserDonnees();
            FiltrerExercices();
            MettreAJourAffichage();
        }

        private void InitialiserDonnees()
        {
            // Initialisation avec des données d'exemple
            _exercices = new List<Exercice>
            {
                new Exercice
                {
                    Name = "Développé couché",
                    Type = "repetitions",
                    Value = "12 x 3",
                    Calories = 180,
                    Description = "Exercice de base pour les pectoraux"
                },
                new Exercice
                {
                    Name = "Course à pied",
                    Type = "duration",
                    Value = "30 minutes",
                    Calories = 300,
                    Description = "Cardio pour l'endurance"
                },
                new Exercice
                {
                    Name = "Squats",
                    Type = "repetitions",
                    Value = "15 x 4",
                    Calories = 120,
                    Description = "Renforcement des jambes"
                }
            };

            _exercicesFiltres = new List<Exercice>(_exercices);
        }

        #region Gestion du formulaire

        private void TypeComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (TypeComboBox.SelectedItem is ComboBoxItem item)
            {
                string type = item.Tag.ToString();

                if (type == "repetitions")
                {
                    RepetitionsGroup.Visibility = Visibility.Visible;
                    DurationGroup.Visibility = Visibility.Collapsed;
                }
                else
                {
                    RepetitionsGroup.Visibility = Visibility.Collapsed;
                    DurationGroup.Visibility = Visibility.Visible;
                }
            }
        }

        private void AddButton_Click(object sender, RoutedEventArgs e)
        {
            if (!ValiderFormulaire())
                return;

            try
            {
                // Récupérer les données du formulaire
                string nom = ExerciseNameBox.Text.Trim();
                ComboBoxItem selectedItem = TypeComboBox.SelectedItem as ComboBoxItem;
                string type = selectedItem?.Tag.ToString();
                string valeur = type == "repetitions" ? RepetitionsBox.Text.Trim() : DurationBox.Text.Trim();
                int calories = 0;
                int.TryParse(CaloriesBox.Text.Trim(), out calories);
                string description = DescriptionBox.Text.Trim();

                if (_estEnModeEdition)
                {
                    // Modifier l'exercice existant
                    _exerciceEnEdition.Name = nom;
                    _exerciceEnEdition.Type = type;
                    _exerciceEnEdition.Value = valeur;
                    _exerciceEnEdition.Calories = calories;
                    _exerciceEnEdition.Description = description;

                    MessageBox.Show("Exercice modifié avec succès!", "Succès",
                                   MessageBoxButton.OK, MessageBoxImage.Information);
                }
                else
                {
                    // Créer un nouveau exercice
                    Exercice nouvelExercice = new Exercice
                    {
                        Name = nom,
                        Type = type,
                        Value = valeur,
                        Calories = calories,
                        Description = description
                    };

                    _exercices.Add(nouvelExercice);

                    MessageBox.Show("Exercice créé avec succès!", "Succès",
                                   MessageBoxButton.OK, MessageBoxImage.Information);
                }

                // Réinitialiser le formulaire et mettre à jour l'affichage
                ReinitialiserFormulaire();
                FiltrerExercices();
                MettreAJourAffichage();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Erreur lors de la sauvegarde : {ex.Message}", "Erreur",
                               MessageBoxButton.OK, MessageBoxImage.Error);
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

            ComboBoxItem selectedItem = TypeComboBox.SelectedItem as ComboBoxItem;
            string type = selectedItem?.Tag.ToString();

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

        private void ReinitialiserFormulaire()
        {
            ExerciseNameBox.Text = "";
            RepetitionsBox.Text = "";
            DurationBox.Text = "";
            CaloriesBox.Text = "";
            DescriptionBox.Text = "";

            TypeComboBox.SelectedItem = null;
            RepetitionsGroup.Visibility = Visibility.Collapsed;
            DurationGroup.Visibility = Visibility.Collapsed;

            // Reset du mode édition
            _estEnModeEdition = false;
            _exerciceEnEdition = null;
            AddButton.Content = "AJOUTER";
        }

        #endregion

        #region Gestion de la recherche

        private void SearchBox_GotFocus(object sender, RoutedEventArgs e)
        {
            TextBox searchBox = sender as TextBox;
            if (searchBox.Text == "Rechercher un exercice...")
            {
                searchBox.Text = "";
                searchBox.Foreground = Brushes.White;
            }
        }

        private void SearchBox_LostFocus(object sender, RoutedEventArgs e)
        {
            TextBox searchBox = sender as TextBox;
            if (string.IsNullOrWhiteSpace(searchBox.Text))
            {
                searchBox.Text = "Rechercher un exercice...";
                searchBox.Foreground = new SolidColorBrush(Color.FromRgb(170, 170, 170));
            }
        }

        private void SearchBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            FiltrerExercices();
            MettreAJourAffichage();
        }

        private void FiltrerExercices()
        {
            // Vérifier que les données sont initialisées
            if (_exercices == null || SearchBox == null)
                return;

            string searchTerm = SearchBox.Text.ToLower();

            if (searchTerm == "rechercher un exercice..." || string.IsNullOrWhiteSpace(searchTerm))
            {
                _exercicesFiltres = new List<Exercice>(_exercices);
            }
            else
            {
                _exercicesFiltres = _exercices.Where(ex =>
                    !string.IsNullOrEmpty(ex.Name) && ex.Name.ToLower().Contains(searchTerm) ||
                    !string.IsNullOrEmpty(ex.Description) && ex.Description.ToLower().Contains(searchTerm)).ToList();
            }
        }

        #endregion

        #region Gestion de l'affichage

        private void MettreAJourAffichage()
        {
            // Vérifier que les composants sont initialisés
            if (ExercisesListPanel == null || NoExercisesMessage == null || _exercicesFiltres == null)
                return;

            ExercisesListPanel.Children.Clear();

            if (_exercicesFiltres.Count == 0)
            {
                NoExercisesMessage.Visibility = Visibility.Visible;
            }
            else
            {
                NoExercisesMessage.Visibility = Visibility.Collapsed;

                foreach (var exercice in _exercicesFiltres)
                {
                    var carte = CreerCarteExercice(exercice);
                    ExercisesListPanel.Children.Add(carte);
                }
            }
        }

        private Border CreerCarteExercice(Exercice exercice)
        {
            var border = new Border
            {
                Style = (Style)FindResource("ExerciseCardStyle")
            };

            var grid = new Grid();
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(2, GridUnitType.Star) });
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });

            // Nom
            var nomText = new TextBlock
            {
                Text = exercice.Name,
                FontWeight = FontWeights.Bold,
                Foreground = Brushes.White,
                VerticalAlignment = VerticalAlignment.Center,
                FontSize = 14
            };
            Grid.SetColumn(nomText, 0);

            // Type et valeur
            string typeIcon = exercice.Type == "repetitions" ? "🏋️" : "⏱️";
            var typeText = new TextBlock
            {
                Text = $"{typeIcon} {exercice.Value}",
                Foreground = new SolidColorBrush(Color.FromRgb(204, 204, 204)),
                VerticalAlignment = VerticalAlignment.Center,
                FontSize = 12
            };
            Grid.SetColumn(typeText, 1);

            // Calories
            var caloriesText = new TextBlock
            {
                Text = $"🔥 {exercice.Calories} cal",
                Foreground = new SolidColorBrush(Color.FromRgb(204, 204, 204)),
                VerticalAlignment = VerticalAlignment.Center,
                FontSize = 12
            };
            Grid.SetColumn(caloriesText, 2);

            // Boutons d'actions
            var actionsPanel = new StackPanel
            {
                Orientation = Orientation.Horizontal,
                HorizontalAlignment = HorizontalAlignment.Right
            };

            var editButton = new Button
            {
                Content = "MODIFIER",
                Style = (Style)FindResource("PrimaryButtonStyle"),
                Margin = new Thickness(0, 0, 10, 0),
                Tag = exercice
            };
            editButton.Click += EditExercise_Click;

            var deleteButton = new Button
            {
                Content = "SUPPRIMER",
                Background = new SolidColorBrush(Color.FromRgb(231, 76, 60)),
                Foreground = Brushes.White,
                BorderThickness = new Thickness(0),
                FontFamily = (FontFamily)FindResource("MainFont"),
                FontSize = 12,
                FontWeight = FontWeights.Bold,
                Padding = new Thickness(15, 8, 15, 8),
                Cursor = Cursors.Hand,
                Tag = exercice
            };

            deleteButton.Click += DeleteExercise_Click;
            actionsPanel.Children.Add(editButton);
            actionsPanel.Children.Add(deleteButton);
            Grid.SetColumn(actionsPanel, 3);

            grid.Children.Add(nomText);
            grid.Children.Add(typeText);
            grid.Children.Add(caloriesText);
            grid.Children.Add(actionsPanel);

            border.Child = grid;
            return border;
        }

        private void EditExercise_Click(object sender, RoutedEventArgs e)
        {
            Button button = sender as Button;
            Exercice exercice = button.Tag as Exercice;

            if (exercice != null)
            {
                ChargerExercicePourEdition(exercice);
            }
        }

        private void DeleteExercise_Click(object sender, RoutedEventArgs e)
        {
            Button button = sender as Button;
            Exercice exercice = button.Tag as Exercice;

            if (exercice != null)
            {
                MessageBoxResult result = MessageBox.Show(
                    $"Êtes-vous sûr de vouloir supprimer l'exercice '{exercice.Name}' ?",
                    "Confirmation de suppression",
                    MessageBoxButton.YesNo,
                    MessageBoxImage.Question);

                if (result == MessageBoxResult.Yes)
                {
                    _exercices.Remove(exercice);
                    FiltrerExercices();
                    MettreAJourAffichage();
                    MessageBox.Show("Exercice supprimé avec succès!", "Succès",
                                   MessageBoxButton.OK, MessageBoxImage.Information);
                }
            }
        }

        private void ChargerExercicePourEdition(Exercice exercice)
        {
            _estEnModeEdition = true;
            _exerciceEnEdition = exercice;

            // Remplir le formulaire avec les données de l'exercice
            ExerciseNameBox.Text = exercice.Name;
            CaloriesBox.Text = exercice.Calories.ToString();
            DescriptionBox.Text = exercice.Description;

            // Sélectionner le bon type
            foreach (ComboBoxItem item in TypeComboBox.Items)
            {
                if (item.Tag.ToString() == exercice.Type)
                {
                    TypeComboBox.SelectedItem = item;
                    break;
                }
            }

            // Déclencher l'événement de changement de sélection
            TypeComboBox_SelectionChanged(TypeComboBox, null);

            // Set value
            if (exercice.Type == "repetitions")
            {
                RepetitionsBox.Text = exercice.Value;
            }
            else
            {
                DurationBox.Text = exercice.Value;
            }

            AddButton.Content = "MODIFIER";
        }

        #endregion
    }

    #region Classes de modèle

    public class Exercice
    {
        public string Name { get; set; }
        public string Type { get; set; }
        public string Value { get; set; }
        public int Calories { get; set; }
        public string Description { get; set; }
        public string ImagePath { get; set; }

        // Propriété calculée pour l'icône selon le type
        public string TypeIcon
        {
            get
            {
                return Type == "repetitions" ? "🏋️" : "⏱️";
            }
        }
    }

    #endregion
}