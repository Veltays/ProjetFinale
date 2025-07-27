using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using ProjetFinale.Models;

namespace ProjetFinale.WPF.Pages
{
    public partial class CreateEventDialog : Window
    {
        public Agenda CreatedEvent { get; private set; }
        private string _selectedColor = "#8B5CF6";
        private List<Activite> _availableActivites;

        public CreateEventDialog(DateTime defaultDate, TimeSpan defaultStartTime, List<Activite> activites = null)
        {
            InitializeComponent();

            _availableActivites = activites ?? new List<Activite>();

            InitializeForm(defaultDate, defaultStartTime);
            SetupEventHandlers();
        }

        private void InitializeForm(DateTime defaultDate, TimeSpan defaultStartTime)
        {
            // Date par défaut
            DatePicker.SelectedDate = defaultDate;

            // Heures par défaut
            HeureDebutTextBox.Text = defaultStartTime.ToString(@"hh\:mm");
            HeureFinTextBox.Text = defaultStartTime.Add(TimeSpan.FromHours(1)).ToString(@"hh\:mm");

            // Couleur par défaut (violet)
            Color1.BorderBrush = Brushes.White;

            // Charger les activités
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

            ActiviteComboBox.SelectedIndex = 0;
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

        private void Create_Click(object sender, RoutedEventArgs e)
        {
            if (ValidateForm())
            {
                CreatedEvent = CreateEventFromForm();
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

        private Agenda CreateEventFromForm()
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

            return new Agenda
            {
                Titre = TitreTextBox.Text.Trim(),
                Date = DatePicker.SelectedDate.Value,
                HeureDebut = startTime,
                HeureFin = endTime,
                Couleur = _selectedColor,
                Description = DescriptionTextBox.Text.Trim(),
                Activite = selectedActivite
            };
        }
    }
}