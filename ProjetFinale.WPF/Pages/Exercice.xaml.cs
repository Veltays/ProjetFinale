using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using Microsoft.Win32;
using ProjetFinale.Models;    // ✅ AJOUTER CETTE LIGNE
using ProjetFinale.Services;  // ✅ AJOUTER CETTE LIGNE (UserService est ici)
using ProjetFinale.Utils;     // ✅ AJOUTER CETTE LIGNE

namespace ProjetFinale.Views
{
    public partial class ExercicesPage : Page
    {
        // ✅ REMPLACER Exercice par Activite
        private List<Activite> _exercices;
        private List<Activite> _exercicesFiltres;

        // ✅ REMPLACER Exercice par Activite
        private Activite _exerciceEnEdition;
        private bool _estEnModeEdition = false;

        // ✅ AJOUTER : Référence à l'utilisateur actuel
        private Utilisateur _utilisateur;

        public ExercicesPage()
        {
            InitializeComponent();

            // ✅ CHARGER L'UTILISATEUR AU DÉMARRAGE (comme pour les tâches)
            ChargerUtilisateurEtDonnees();

            InitialiserDonnees();
            FiltrerExercices();
            MettreAJourAffichage();

            // ✅ AJOUTER : S'abonner aux changements d'utilisateur
            UserService.UtilisateurActifChanged += OnUtilisateurChanged;
        }

        // ✅ NOUVELLE MÉTHODE : Charger l'utilisateur comme dans ObjectifsPage
        private void ChargerUtilisateurEtDonnees()
        {
            // Essayer d'abord l'utilisateur actif
            _utilisateur = UserService.UtilisateurActif;
            Console.WriteLine($"🔍 UtilisateurActif : {(_utilisateur != null ? _utilisateur.Pseudo : "NULL")}");

            // Si pas d'utilisateur actif, charger depuis le fichier JSON
            if (_utilisateur == null)
            {
                _utilisateur = JsonService.ChargerUtilisateur();
                Console.WriteLine($"🔍 ChargerUtilisateur : {(_utilisateur != null ? _utilisateur.Pseudo : "NULL")}");

                if (_utilisateur != null)
                {
                    UserService.UtilisateurActif = _utilisateur;
                }
            }

            if (_utilisateur != null)
            {
                Console.WriteLine($"✅ Utilisateur chargé pour ExercicesPage : {_utilisateur.Pseudo}");
                Console.WriteLine($"   Nombre d'activités : {_utilisateur.ListeActivites.Count}");
            }
            else
            {
                Console.WriteLine("⚠️ Aucun utilisateur trouvé pour ExercicesPage");
            }
        }

        // ✅ MODIFIER : Gérer les changements d'utilisateur avec rechargement complet
        private void OnUtilisateurChanged(Utilisateur? nouvelUtilisateur)
        {
            Console.WriteLine("🔄 Changement d'utilisateur détecté dans ExercicesPage");
            _utilisateur = nouvelUtilisateur;

            if (_utilisateur != null)
            {
                Console.WriteLine($"   Nouvel utilisateur : {_utilisateur.Pseudo}");
                Console.WriteLine($"   Activités disponibles : {_utilisateur.ListeActivites.Count}");
            }

            // Recharger toutes les données
            InitialiserDonnees();
            FiltrerExercices();
            MettreAJourAffichage();
        }

        private void InitialiserDonnees()
        {
            // ✅ MODIFIER : Ne charger depuis l'utilisateur QUE s'il a des activités
            if (_utilisateur?.ListeActivites != null && _utilisateur.ListeActivites.Count > 0)
            {
                // Charger les activités sauvegardées de l'utilisateur
                _exercices = new List<Activite>(_utilisateur.ListeActivites);
                Console.WriteLine($"✅ {_exercices.Count} activités chargées depuis l'utilisateur");
            }
            else
            {
                // ✅ CRÉER des données par défaut SEULEMENT si l'utilisateur n'a aucune activité
                Console.WriteLine("ℹ️ Aucune activité trouvée, création des exercices par défaut");
                _exercices = new List<Activite>
                {
                    new Activite
                    {
                        Titre = "Développé couché",
                        CaloriesBrulees = 180,
                        Duree = TimeSpan.FromMinutes(45),
                        ImagePath = "/Images/developpe_couche.png"
                    },
                    new Activite
                    {
                        Titre = "Course à pied",
                        CaloriesBrulees = 300,
                        Duree = TimeSpan.FromMinutes(30),
                        ImagePath = "/Images/course.png"
                    },
                    new Activite
                    {
                        Titre = "Squats",
                        CaloriesBrulees = 120,
                        Duree = TimeSpan.FromMinutes(20),
                        ImagePath = "/Images/squats.png"
                    }
                };

                // ✅ AJOUTER : Sauvegarder les données par défaut dans l'utilisateur
                if (_utilisateur != null)
                {
                    foreach (var exercice in _exercices)
                    {
                        _utilisateur.ListeActivites.Add(exercice);
                    }
                    UserService.MettreAJourUtilisateur(_utilisateur);
                    Console.WriteLine("✅ Exercices par défaut sauvegardés dans l'utilisateur");
                }
            }

            _exercicesFiltres = new List<Activite>(_exercices);
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

                // ✅ MODIFIER : Gérer la durée selon le type
                TimeSpan duree;
                if (type == "repetitions")
                {
                    // Pour les répétitions, estimer une durée basée sur le nombre
                    string repsValue = RepetitionsBox.Text.Trim();
                    // Estimation : 1 minute par série
                    duree = TimeSpan.FromMinutes(EstimerDureeRepetitions(repsValue));
                }
                else
                {
                    // Pour la durée, parser directement
                    duree = ParserDuree(DurationBox.Text.Trim());
                }

                float calories = 0;
                float.TryParse(CaloriesBox.Text.Trim(), out calories);

                if (_estEnModeEdition)
                {
                    // ✅ MODIFIER : Modifier l'activité existante
                    _exerciceEnEdition.Titre = nom;
                    _exerciceEnEdition.CaloriesBrulees = calories;
                    _exerciceEnEdition.Duree = duree;

                    // ✅ AJOUTER : Sauvegarder les modifications
                    if (_utilisateur != null)
                    {
                        UserService.MettreAJourUtilisateur(_utilisateur);
                    }

                    MessageBox.Show("Exercice modifié avec succès!", "Succès",
                                   MessageBoxButton.OK, MessageBoxImage.Information);
                }
                else
                {
                    // ✅ MODIFIER : Créer une nouvelle Activite
                    Activite nouvelleActivite = new Activite
                    {
                        Titre = nom,
                        CaloriesBrulees = calories,
                        Duree = duree,
                        ImagePath = "/Images/default_exercise.png"
                    };

                    _exercices.Add(nouvelleActivite);

                    // ✅ AJOUTER : Ajouter à l'utilisateur et sauvegarder
                    if (_utilisateur != null)
                    {
                        _utilisateur.ListeActivites.Add(nouvelleActivite);
                        UserService.MettreAJourUtilisateur(_utilisateur);
                    }

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

        // ✅ AJOUTER : Méthodes utilitaires pour gérer les durées
        private int EstimerDureeRepetitions(string repsValue)
        {
            // Estimer la durée basée sur les répétitions (ex: "12 x 3" = 3 séries = 3 minutes)
            try
            {
                if (repsValue.Contains("x"))
                {
                    var parts = repsValue.Split('x');
                    if (parts.Length == 2 && int.TryParse(parts[1].Trim(), out int series))
                    {
                        return series; // 1 minute par série
                    }
                }
                return 15; // Durée par défaut
            }
            catch
            {
                return 15;
            }
        }

        private TimeSpan ParserDuree(string dureeStr)
        {
            try
            {
                // Supporter différents formats : "30 minutes", "1h30", "45min", etc.
                dureeStr = dureeStr.ToLower().Replace(" ", "");

                if (dureeStr.Contains("minute"))
                {
                    string numStr = dureeStr.Replace("minutes", "").Replace("minute", "");
                    if (int.TryParse(numStr, out int minutes))
                        return TimeSpan.FromMinutes(minutes);
                }
                else if (dureeStr.Contains("min"))
                {
                    string numStr = dureeStr.Replace("min", "");
                    if (int.TryParse(numStr, out int minutes))
                        return TimeSpan.FromMinutes(minutes);
                }
                else if (dureeStr.Contains("h"))
                {
                    // Format "1h30" ou "1h"
                    var parts = dureeStr.Split('h');
                    int heures = int.Parse(parts[0]);
                    int minutes = parts.Length > 1 && !string.IsNullOrEmpty(parts[1]) ? int.Parse(parts[1]) : 0;
                    return TimeSpan.FromHours(heures).Add(TimeSpan.FromMinutes(minutes));
                }
                else
                {
                    // Supposer que c'est en minutes
                    if (int.TryParse(dureeStr, out int minutes))
                        return TimeSpan.FromMinutes(minutes);
                }

                return TimeSpan.FromMinutes(30); // Défaut
            }
            catch
            {
                return TimeSpan.FromMinutes(30);
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
                _exercicesFiltres = new List<Activite>(_exercices);
            }
            else
            {
                // ✅ MODIFIER : Adapter pour la classe Activite
                _exercicesFiltres = _exercices.Where(ex =>
                    !string.IsNullOrEmpty(ex.Titre) && ex.Titre.ToLower().Contains(searchTerm)).ToList();
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

        // ✅ MODIFIER : Adapter pour la classe Activite
        private Border CreerCarteExercice(Activite exercice)
        {
            var border = new Border
            {
                Style = (Style)FindResource("ExercicesCardStyle")  // ✅ NOUVEAU NOM
            };

            var grid = new Grid();
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(2, GridUnitType.Star) });
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });

            // ✅ MODIFIER : Utiliser Titre au lieu de Name
            var nomText = new TextBlock
            {
                Text = exercice.Titre,
                FontWeight = FontWeights.Bold,
                Foreground = Brushes.White,
                VerticalAlignment = VerticalAlignment.Center,
                FontSize = 14
            };
            Grid.SetColumn(nomText, 0);

            // ✅ MODIFIER : Afficher la durée
            var dureeText = new TextBlock
            {
                Text = $"⏱️ {exercice.Duree.TotalMinutes:F0} min",
                Foreground = new SolidColorBrush(Color.FromRgb(204, 204, 204)),
                VerticalAlignment = VerticalAlignment.Center,
                FontSize = 12
            };
            Grid.SetColumn(dureeText, 1);

            // ✅ MODIFIER : Utiliser CaloriesBrulees
            var caloriesText = new TextBlock
            {
                Text = $"🔥 {exercice.CaloriesBrulees:F0} cal",
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
                Style = (Style)FindResource("ExercicesPrimaryButtonStyle"),  // ✅ NOUVEAU NOM
                Margin = new Thickness(0, 0, 10, 0),
                Tag = exercice
            };
            editButton.Click += EditExercise_Click;

            var deleteButton = new Button
            {
                Content = "SUPPRIMER",
                Style = (Style)FindResource("ExercicesDeleteButtonStyle"),  // ✅ PROPRE
                Tag = exercice
            };

            deleteButton.Click += DeleteExercise_Click;
            actionsPanel.Children.Add(editButton);
            actionsPanel.Children.Add(deleteButton);
            Grid.SetColumn(actionsPanel, 3);

            grid.Children.Add(nomText);
            grid.Children.Add(dureeText);
            grid.Children.Add(caloriesText);
            grid.Children.Add(actionsPanel);

            border.Child = grid;
            return border;
        }

        private void EditExercise_Click(object sender, RoutedEventArgs e)
        {
            Button button = sender as Button;
            Activite exercice = button.Tag as Activite; // ✅ MODIFIER : Activite au lieu d'Exercice

            if (exercice != null)
            {
                ChargerExercicePourEdition(exercice);
            }
        }

        private void DeleteExercise_Click(object sender, RoutedEventArgs e)
        {
            Button button = sender as Button;
            Activite exercice = button.Tag as Activite; // ✅ MODIFIER : Activite au lieu d'Exercice

            if (exercice != null)
            {
                MessageBoxResult result = MessageBox.Show(
                    $"Êtes-vous sûr de vouloir supprimer l'exercice '{exercice.Titre}' ?", // ✅ MODIFIER : Titre
                    "Confirmation de suppression",
                    MessageBoxButton.YesNo,
                    MessageBoxImage.Question);

                if (result == MessageBoxResult.Yes)
                {
                    _exercices.Remove(exercice);

                    // ✅ AJOUTER : Supprimer de l'utilisateur
                    if (_utilisateur != null)
                    {
                        _utilisateur.ListeActivites.Remove(exercice);
                        UserService.MettreAJourUtilisateur(_utilisateur);
                    }

                    FiltrerExercices();
                    MettreAJourAffichage();
                    MessageBox.Show("Exercice supprimé avec succès!", "Succès",
                                   MessageBoxButton.OK, MessageBoxImage.Information);
                }
            }
        }

        // ✅ MODIFIER : Adapter pour la classe Activite
        private void ChargerExercicePourEdition(Activite exercice)
        {
            _estEnModeEdition = true;
            _exerciceEnEdition = exercice;

            // ✅ MODIFIER : Utiliser les propriétés de Activite
            ExerciseNameBox.Text = exercice.Titre;
            CaloriesBox.Text = exercice.CaloriesBrulees.ToString();

            // Estimer le type basé sur la durée (vous pouvez ajuster cette logique)
            if (exercice.Duree.TotalMinutes <= 10)
            {
                // Probablement des répétitions
                foreach (ComboBoxItem item in TypeComboBox.Items)
                {
                    if (item.Tag.ToString() == "repetitions")
                    {
                        TypeComboBox.SelectedItem = item;
                        break;
                    }
                }
                TypeComboBox_SelectionChanged(TypeComboBox, null);
                RepetitionsBox.Text = $"10 x {exercice.Duree.TotalMinutes:F0}"; // Estimation
            }
            else
            {
                // Probablement une durée
                foreach (ComboBoxItem item in TypeComboBox.Items)
                {
                    if (item.Tag.ToString() == "duration")
                    {
                        TypeComboBox.SelectedItem = item;
                        break;
                    }
                }
                TypeComboBox_SelectionChanged(TypeComboBox, null);
                DurationBox.Text = $"{exercice.Duree.TotalMinutes:F0} minutes";
            }

            AddButton.Content = "MODIFIER";
        }

        #endregion

        // ✅ AJOUTER : Nettoyage lors de la fermeture
        ~ExercicesPage()
        {
            UserService.UtilisateurActifChanged -= OnUtilisateurChanged;
        }
    }
}