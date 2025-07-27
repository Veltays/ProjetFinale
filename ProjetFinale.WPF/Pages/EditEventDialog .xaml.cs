using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using ProjetFinale.Models;

namespace ProjetFinale.WPF.Pages
{
    public partial class EditEventDialog : Window
    {
        public bool IsDeleted { get; private set; }
        private Agenda _event;
        private string _selectedColor;
        private List<Activite> _availableActivites;

        public EditEventDialog(Agenda eventToEdit, List<Activite> activites = null)
        {
            InitializeComponent();

            _event = eventToEdit;
            _availableActivites = activites ?? new List<Activite>();
            _selectedColor = _event.Couleur;

            LoadEventData();
            SetupEventHandlers();
        }

        private void LoadEventData()
        {
            // Titre
            TitreTextBox.Text = _event.Titre;

            // Date
            DatePicker.SelectedDate = _event.Date;

            // Heures
            HeureDebutTextBox.Text = _event.HeureDebut.ToString(@"hh\:mm");
            HeureFinTextBox.Text = _event.HeureFin.ToString(@"hh\:mm");

            // Durée
            TimeSpan duration = _event.HeureFin - _event.HeureDebut;
            SetDurationComboBox(duration);

            // Couleur
            SetSelectedColor(_event.Couleur);

            // Description
            DescriptionTextBox.Text = _event.Description ?? "";

            // Activités
            LoadActivites();

            // Focus sur le titre
            TitreTextBox.Focus();
        }

        private void LoadActivites()
        {
            ActiviteComboBox.Items.Clear();
            ActiviteComboBox.Items.Add("Aucune activité");

            foreach (var activite in _availableActivites)
            {
                ActiviteComboBox.Items.Add(activite);
            }

            // Sélectionner l'activité actuelle
            if (_event.Activite != null)
            {
                var currentActivite = _availableActivites.FirstOrDefault(a => a.Titre == _event.Activite.Titre);
                if (currentActivite != null)
                {
                    ActiviteComboBox.SelectedItem = currentActivite;
                }
                else
                {
                    ActiviteComboBox.SelectedIndex = 0;
                }
            }
            else
            {
                ActiviteComboBox.SelectedIndex = 0;
            }
        }

        private void SetDurationComboBox(TimeSpan duration)
        {
            string durationText = duration.TotalMinutes switch
            {
                30 => "30 min",
                60 => "1 heure",
                90 => "1h30",
                120 => "2 heures",
                150 => "2h30",
                180 => "3 heures",
                _ => "Personnalisé"
            };

            foreach (ComboBoxItem item in DureeComboBox.Items)
            {
                if (item.Content.ToString() == durationText)
                {
                    DureeComboBox.SelectedItem = item;
                    break;
                }
            }

            if (durationText == "Personnalisé")
            {
                CustomTimePanel.Visibility = Visibility.Visible;
            }
        }

        private void SetSelectedColor(string color)
        {
            // Réinitialiser toutes les bordures
            foreach (Button colorBtn in ColorPanel.Children.OfType<Button>())
            {
                colorBtn.BorderBrush = Brushes.Transparent;

                if (colorBtn.Tag.ToString() == color)
                {
                    colorBtn.BorderBrush = Brushes.White;
                }
            }
        }

        private void SetupEventHandlers()
        {
            DureeComboBox.SelectionChanged += DureeComboBox_SelectionChanged;
            LierObjectifCheckBox.Checked += LierObjectifCheckBox_Checked;
            LierObjectifCheckBox.Unchecked += LierObjectifCheckBox_Unchecked;
        }

        private void DureeComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (DureeComboBox.SelectedItem is ComboBoxItem item)
            {
                string content = item.Content.ToString();

                if (content == "Personnalisé")
                {
                    CustomTimePanel.Visibility = Visibility.Visible;
                }
                else
                {
                    CustomTimePanel.Visibility = Visibility.Collapsed;
                    UpdateEndTimeFromDuration(content);
                }
            }
        }

        private void UpdateEndTimeFromDuration(string durationText)
        {
            if (!TimeSpan.TryParse(HeureDebutTextBox.Text, out TimeSpan startTime))
                return;

            TimeSpan duration = durationText switch
            {
                "30 min" => TimeSpan.FromMinutes(30),
                "1 heure" => TimeSpan.FromHours(1),
                "1h30" => TimeSpan.FromHours(1.5),
                "2 heures" => TimeSpan.FromHours(2),
                "2h30" => TimeSpan.FromHours(2.5),
                "3 heures" => TimeSpan.FromHours(3),
                _ => TimeSpan.FromHours(1)
            };

            TimeSpan endTime = startTime.Add(duration);
            HeureFinTextBox.Text = endTime.ToString(@"hh\:mm");
        }

        private void LierObjectifCheckBox_Checked(object sender, RoutedEventArgs e)
        {
            ObjectifComboBox.Visibility = Visibility.Visible;
            // TODO: Charger les objectifs de l'utilisateur
            ObjectifComboBox.Items.Add("Objectif 1: 100 pompes d'affilée");
            ObjectifComboBox.Items.Add("Objectif 2: Courir 10km");
            ObjectifComboBox.Items.Add("Objectif 3: Perdre 5kg");
        }

        private void LierObjectifCheckBox_Unchecked(object sender, RoutedEventArgs e)
        {
            ObjectifComboBox.Visibility = Visibility.Collapsed;
            ObjectifComboBox.SelectedIndex = -1;
        }

        private void ColorButton_Click(object sender, RoutedEventArgs e)
        {
            // Réinitialiser toutes les bordures
            foreach (Button colorBtn in ColorPanel.Children.OfType<Button>())
            {
                colorBtn.BorderBrush = Brushes.Transparent;
            }

            // Sélectionner la nouvelle couleur
            if (sender is Button button)
            {
                button.BorderBrush = Brushes.White;
                _selectedColor = button.Tag.ToString();
            }
        }

        private void Save_Click(object sender, RoutedEventArgs e)
        {
            if (ValidateForm())
            {
                UpdateEventFromForm();
                DialogResult = true;
                Close();
            }
        }

        private void Delete_Click(object sender, RoutedEventArgs e)
        {
            var result = MessageBox.Show(
                $"Êtes-vous sûr de vouloir supprimer l'événement '{_event.Titre}' ?",
                "Confirmer la suppression",
                MessageBoxButton.YesNo,
                MessageBoxImage.Question);

            if (result == MessageBoxResult.Yes)
            {
                IsDeleted = true;
                DialogResult = true;
                Close();
            }
        }

        private void Cancel_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }

        private bool ValidateForm()
        {
            // Vérifier le titre
            if (string.IsNullOrWhiteSpace(TitreTextBox.Text))
            {
                MessageBox.Show("Le titre est obligatoire !", "Erreur", MessageBoxButton.OK, MessageBoxImage.Warning);
                TitreTextBox.Focus();
                return false;
            }

            // Vérifier la date
            if (!DatePicker.SelectedDate.HasValue)
            {
                MessageBox.Show("La date est obligatoire !", "Erreur", MessageBoxButton.OK, MessageBoxImage.Warning);
                return false;
            }

            // Vérifier les heures
            if (CustomTimePanel.Visibility == Visibility.Visible)
            {
                if (!TimeSpan.TryParse(HeureDebutTextBox.Text, out TimeSpan startTime) ||
                    !TimeSpan.TryParse(HeureFinTextBox.Text, out TimeSpan endTime))
                {
                    MessageBox.Show("Format d'heure invalide ! Utilisez le format HH:MM", "Erreur", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return false;
                }

                if (endTime <= startTime)
                {
                    MessageBox.Show("L'heure de fin doit être postérieure à l'heure de début !", "Erreur", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return false;
                }
            }

            return true;
        }

        private void UpdateEventFromForm()
        {
            TimeSpan startTime, endTime;

            if (CustomTimePanel.Visibility == Visibility.Visible)
            {
                TimeSpan.TryParse(HeureDebutTextBox.Text, out startTime);
                TimeSpan.TryParse(HeureFinTextBox.Text, out endTime);
            }
            else
            {
                TimeSpan.TryParse(HeureDebutTextBox.Text, out startTime);
                string durationText = ((ComboBoxItem)DureeComboBox.SelectedItem).Content.ToString();

                TimeSpan duration = durationText switch
                {
                    "30 min" => TimeSpan.FromMinutes(30),
                    "1 heure" => TimeSpan.FromHours(1),
                    "1h30" => TimeSpan.FromHours(1.5),
                    "2 heures" => TimeSpan.FromHours(2),
                    "2h30" => TimeSpan.FromHours(2.5),
                    "3 heures" => TimeSpan.FromHours(3),
                    _ => TimeSpan.FromHours(1)
                };

                endTime = startTime.Add(duration);
            }

            // Activité sélectionnée
            Activite selectedActivite = null;
            if (ActiviteComboBox.SelectedItem is Activite activite)
            {
                selectedActivite = activite;
            }

            // Mettre à jour l'événement
            _event.Titre = TitreTextBox.Text.Trim();
            _event.Date = DatePicker.SelectedDate.Value;
            _event.HeureDebut = startTime;
            _event.HeureFin = endTime;
            _event.Couleur = _selectedColor;
            _event.Description = DescriptionTextBox.Text.Trim();
            _event.Activite = selectedActivite;
        }
    }
}