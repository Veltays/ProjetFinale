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

        private readonly List<Activite> _activites;
        private string _selectedColor = "#8B5CF6";
        private Button? _selectedColorBtn;

        public CreateEventDialog(DateTime defaultDate, TimeSpan defaultStartTime, List<Activite>? activites = null)
        {
            InitializeComponent();
            _activites = activites ?? new List<Activite>();
            InitForm(defaultDate, defaultStartTime);
            WireEvents();
        }

        // ---- Init ----
        private void InitForm(DateTime date, TimeSpan start)
        {
            DatePicker.SelectedDate = date;
            HeureDebutTextBox.Text = start.ToString(@"hh\:mm");
            HeureFinTextBox.Text = start.Add(TimeSpan.FromHours(1)).ToString(@"hh\:mm");

            // Couleur par défaut (violet)
            if (Color1 is Button b)
            {
                b.BorderBrush = Brushes.White;
                _selectedColorBtn = b;
            }

            // Activités
            ActiviteComboBox.Items.Clear();
            ActiviteComboBox.Items.Add("Aucune activité");
            foreach (var a in _activites) ActiviteComboBox.Items.Add(a);
            ActiviteComboBox.SelectedIndex = 0;

            TitreTextBox.Focus();
        }

        private void WireEvents()
        {
            LierObjectifCheckBox.Checked += LierObjectif_Changed;
            LierObjectifCheckBox.Unchecked += LierObjectif_Changed;
        }

        // ---- Objectifs (checkbox) ----
        private void LierObjectif_Changed(object sender, RoutedEventArgs e)
        {
            bool visible = LierObjectifCheckBox.IsChecked == true;
            ObjectifComboBox.Visibility = visible ? Visibility.Visible : Visibility.Collapsed;
            if (!visible)
            {
                ObjectifComboBox.SelectedIndex = -1;
                return;
            }

            ObjectifComboBox.Items.Clear();
            ObjectifComboBox.Items.Add("Aucun objectif");

            var user = UserService.UtilisateurActif ?? JsonService.ChargerUtilisateur();
            if (user != null) UserService.UtilisateurActif = user;

            if (user?.ListeTaches?.Count > 0)
                foreach (var t in user.ListeTaches) ObjectifComboBox.Items.Add(t);
            else
                ObjectifComboBox.Items.Add("(Aucune tâche disponible)");

            ObjectifComboBox.SelectedIndex = 0;
        }

        // ---- Couleurs ----
        private void ColorButton_Click(object sender, RoutedEventArgs e)
        {
            if (sender is not Button btn) return;

            if (_selectedColorBtn != null) _selectedColorBtn.BorderBrush = Brushes.Transparent;
            btn.BorderBrush = Brushes.White;
            _selectedColorBtn = btn;

            _selectedColor = btn.Tag?.ToString() ?? _selectedColor;
        }

        // ---- Actions ----
        private void Create_Click(object sender, RoutedEventArgs e)
        {
            if (!ValidateForm()) return;
            CreatedEvent = BuildEvent();
            DialogResult = true;
            Close();
        }

        private void Cancel_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }

        // ---- Validation ----
        private bool ValidateForm()
        {
            if (string.IsNullOrWhiteSpace(TitreTextBox.Text))
            {
                MessageBox.Show("Le titre est obligatoire !", "Erreur", MessageBoxButton.OK, MessageBoxImage.Warning);
                TitreTextBox.Focus();
                return false;
            }

            if (!DatePicker.SelectedDate.HasValue)
            {
                MessageBox.Show("La date est obligatoire !", "Erreur", MessageBoxButton.OK, MessageBoxImage.Warning);
                return false;
            }

            if (!TryParseTime(HeureDebutTextBox.Text, out var start))
            {
                MessageBox.Show("Heure de début invalide (utilisez HH:MM).", "Erreur", MessageBoxButton.OK, MessageBoxImage.Warning);
                return false;
            }

            TimeSpan end;
            if (string.IsNullOrWhiteSpace(HeureFinTextBox.Text))
            {
                end = start.Add(TimeSpan.FromHours(1));
                HeureFinTextBox.Text = end.ToString(@"hh\:mm");
            }
            else if (!TryParseTime(HeureFinTextBox.Text, out end))
            {
                MessageBox.Show("Heure de fin invalide (utilisez HH:MM).", "Erreur", MessageBoxButton.OK, MessageBoxImage.Warning);
                return false;
            }

            if (end <= start)
            {
                MessageBox.Show("L'heure de fin doit être postérieure à l'heure de début.", "Erreur", MessageBoxButton.OK, MessageBoxImage.Warning);
                return false;
            }

            return true;
        }

        // ---- Construction modèle ----
        private Agenda BuildEvent()
        {
            TryParseTime(HeureDebutTextBox.Text, out var startTime);
            TryParseTime(HeureFinTextBox.Text, out var endTime);

            var activite = ActiviteComboBox.SelectedItem as Activite; // "Aucune activité" => null

            return new Agenda
            {
                Titre = TitreTextBox.Text.Trim(),
                Date = DatePicker.SelectedDate!.Value,
                HeureDebut = startTime,
                HeureFin = endTime,
                Couleur = _selectedColor,
                Description = DescriptionTextBox.Text.Trim(),
                Activite = activite
            };
        }

        /// <summary>
        /// Tolère "8", "8h", "8:00", "08:00", "8.30".
        /// </summary>
        private static bool TryParseTime(string input, out TimeSpan result)
        {
            result = default;
            if (string.IsNullOrWhiteSpace(input)) return false;

            var t = input.Trim().ToLowerInvariant().Replace("h", ":").Replace(".", ":");
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