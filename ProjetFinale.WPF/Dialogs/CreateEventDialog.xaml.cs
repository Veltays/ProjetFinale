using ProjetFinale.Models;
using ProjetFinale.Services;
using ProjetFinale.Utils;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace ProjetFinale.WPF.Pages
{
    public partial class CreateEventDialog : Window
    {
        public Agenda CreatedEvent { get; private set; }

        private string _selectedColor = "#8B5CF6";
        private readonly List<Activite> _availableActivites;

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
                ActiviteComboBox.Items.Add(activite);

            ActiviteComboBox.SelectedIndex = 0;
        }

        private void SetupEventHandlers()
        {
            LierObjectifCheckBox.Checked += LierObjectifCheckBox_Checked;
            LierObjectifCheckBox.Unchecked += LierObjectifCheckBox_Unchecked;
        }

        private void LierObjectifCheckBox_Checked(object sender, RoutedEventArgs e)
        {
            ObjectifComboBox.Visibility = Visibility.Visible;

            ObjectifComboBox.Items.Clear();
            ObjectifComboBox.Items.Add("Aucun objectif");

            // Récup utilisateur
            var utilisateur = UserService.UtilisateurActif ?? JsonService.ChargerUtilisateur();
            if (utilisateur != null) UserService.UtilisateurActif = utilisateur;

            if (utilisateur?.ListeTaches != null)
            {
                foreach (var tache in utilisateur.ListeTaches)
                    ObjectifComboBox.Items.Add(tache);
            }
            else
            {
                ObjectifComboBox.Items.Add("(Aucune tâche disponible)");
            }

            ObjectifComboBox.SelectedIndex = 0;
        }

        private void LierObjectifCheckBox_Unchecked(object sender, RoutedEventArgs e)
        {
            ObjectifComboBox.Visibility = Visibility.Collapsed;
            ObjectifComboBox.SelectedIndex = -1;
        }

        private void ColorButton_Click(object sender, RoutedEventArgs e)
        {
            // Reset
            foreach (Button colorBtn in ColorPanel.Children.OfType<Button>())
                colorBtn.BorderBrush = Brushes.Transparent;

            if (sender is Button button)
            {
                button.BorderBrush = Brushes.White;
                _selectedColor = button.Tag?.ToString() ?? _selectedColor;
            }
        }

        private void Create_Click(object sender, RoutedEventArgs e)
        {
            if (!ValidateForm()) return;

            CreatedEvent = CreateEventFromForm();
            DialogResult = true;
            Close();
        }

        private void Cancel_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }

        private bool ValidateForm()
        {
            // Titre
            if (string.IsNullOrWhiteSpace(TitreTextBox.Text))
            {
                MessageBox.Show("Le titre est obligatoire !", "Erreur", MessageBoxButton.OK, MessageBoxImage.Warning);
                TitreTextBox.Focus();
                return false;
            }

            // Date
            if (!DatePicker.SelectedDate.HasValue)
            {
                MessageBox.Show("La date est obligatoire !", "Erreur", MessageBoxButton.OK, MessageBoxImage.Warning);
                return false;
            }

            // Heures (sans durée)
            if (!TryParseTime(HeureDebutTextBox.Text, out var start))
            {
                MessageBox.Show("Heure de début invalide (utilisez HH:MM).", "Erreur", MessageBoxButton.OK, MessageBoxImage.Warning);
                return false;
            }

            // Si fin vide → +1h
            TimeSpan end;
            if (string.IsNullOrWhiteSpace(HeureFinTextBox.Text))
            {
                end = start.Add(TimeSpan.FromHours(1));
                HeureFinTextBox.Text = end.ToString(@"hh\:mm");
            }
            else
            {
                if (!TryParseTime(HeureFinTextBox.Text, out end))
                {
                    MessageBox.Show("Heure de fin invalide (utilisez HH:MM).", "Erreur", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return false;
                }
            }

            if (end <= start)
            {
                MessageBox.Show("L'heure de fin doit être postérieure à l'heure de début.", "Erreur", MessageBoxButton.OK, MessageBoxImage.Warning);
                return false;
            }

            return true;
        }

        private Agenda CreateEventFromForm()
        {
            // Parse sûrs (ValidateForm a déjà contrôlé)
            TryParseTime(HeureDebutTextBox.Text, out var startTime);
            TryParseTime(HeureFinTextBox.Text, out var endTime);

            // Activité sélectionnée
            Activite selectedActivite = ActiviteComboBox.SelectedItem as Activite;

            return new Agenda
            {
                Titre = TitreTextBox.Text.Trim(),
                Date = DatePicker.SelectedDate!.Value,
                HeureDebut = startTime,
                HeureFin = endTime,
                Couleur = _selectedColor,
                Description = DescriptionTextBox.Text.Trim(),
                Activite = selectedActivite
            };
        }

        /// <summary>
        /// Tolère "8", "8h", "8:00", "08:00", "8.30" → TimeSpan.
        /// </summary>
        private static bool TryParseTime(string input, out TimeSpan result)
        {
            result = default;
            if (string.IsNullOrWhiteSpace(input)) return false;

            var t = input.Trim()
                         .ToLowerInvariant()
                         .Replace("h", ":")
                         .Replace(".", ":");

            // Ajoute les minutes si juste l'heure
            if (!t.Contains(":")) t += ":00";

            return TimeSpan.TryParseExact(t,
                                          new[] { @"h\:mm", @"hh\:mm" },
                                          CultureInfo.InvariantCulture,
                                          out result);
        }
    }
}
