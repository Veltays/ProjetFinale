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
    public partial class EditEventDialog : Window
    {
        public bool IsDeleted { get; private set; }
        private readonly Agenda _event;
        private string _selectedColor;
        private readonly List<Activite> _availableActivites;

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

            // Heures (sans durée)
            HeureDebutTextBox.Text = _event.HeureDebut.ToString(@"hh\:mm");
            HeureFinTextBox.Text = _event.HeureFin.ToString(@"hh\:mm");

            // Couleur
            SetSelectedColor(_event.Couleur);

            // Description
            DescriptionTextBox.Text = _event.Description ?? string.Empty;

            // Activités
            LoadActivites();

            // Focus
            TitreTextBox.Focus();
        }

        private void LoadActivites()
        {
            ActiviteComboBox.Items.Clear();
            ActiviteComboBox.Items.Add("Aucune activité");

            foreach (var activite in _availableActivites)
                ActiviteComboBox.Items.Add(activite);

            // Sélectionner l'activité actuelle
            if (_event.Activite != null)
            {
                var current = _availableActivites.FirstOrDefault(a => a.Titre == _event.Activite.Titre);
                ActiviteComboBox.SelectedItem = current ?? ActiviteComboBox.Items[0];
            }
            else
            {
                ActiviteComboBox.SelectedIndex = 0;
            }
        }

        private void SetSelectedColor(string color)
        {
            foreach (Button colorBtn in ColorPanel.Children.OfType<Button>())
            {
                colorBtn.BorderBrush = Brushes.Transparent;
                if ((colorBtn.Tag?.ToString() ?? "") == (color ?? ""))
                    colorBtn.BorderBrush = Brushes.White;
            }
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

            var utilisateur = UserService.UtilisateurActif ?? JsonService.ChargerUtilisateur();
            if (utilisateur != null) UserService.UtilisateurActif = utilisateur;

            if (utilisateur?.ListeTaches != null)
            {
                foreach (var tache in utilisateur.ListeTaches)
                    ObjectifComboBox.Items.Add(tache);
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
            foreach (Button colorBtn in ColorPanel.Children.OfType<Button>())
                colorBtn.BorderBrush = Brushes.Transparent;

            if (sender is Button b)
            {
                b.BorderBrush = Brushes.White;
                _selectedColor = b.Tag?.ToString() ?? _selectedColor;
            }
        }

        private void Save_Click(object sender, RoutedEventArgs e)
        {
            if (!ValidateForm()) return;

            UpdateEventFromForm();
            DialogResult = true;
            Close();
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

        private void UpdateEventFromForm()
        {
            // Parse sûrs (ValidateForm a déjà checké)
            TryParseTime(HeureDebutTextBox.Text, out var startTime);
            TryParseTime(HeureFinTextBox.Text, out var endTime);

            // Activité
            var selectedActivite = ActiviteComboBox.SelectedItem as Activite;

            // Mise à jour
            _event.Titre = TitreTextBox.Text.Trim();
            _event.Date = DatePicker.SelectedDate!.Value;
            _event.HeureDebut = startTime;
            _event.HeureFin = endTime;
            _event.Couleur = _selectedColor;
            _event.Description = DescriptionTextBox.Text.Trim();
            _event.Activite = selectedActivite;
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

            if (!t.Contains(":")) t += ":00";

            return TimeSpan.TryParseExact(
                t,
                new[] { @"h\:mm", @"hh\:mm" },
                CultureInfo.InvariantCulture,
                out result
            );
        }
    }
}