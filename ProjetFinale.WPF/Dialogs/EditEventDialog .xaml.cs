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
        public Agenda? EditedEvent { get; private set; } // <-- résultat à lire par l'appelant

        private readonly Agenda _original;         // juste pour le titre et référence
        private Agenda _draft;                     // <-- copie de travail
        private readonly List<Activite> _activites;
        private string _selectedColor;
        private Button? _selectedColorBtn;

        public EditEventDialog(Agenda eventToEdit, List<Activite>? activites = null)
        {
            InitializeComponent();

            _original = eventToEdit;
            _draft = CloneAgenda(eventToEdit);         // <-- on clone ici
            _activites = activites ?? new List<Activite>();
            _selectedColor = _draft.Couleur;

            InitForm();
            WireEvents();
        }

        // ---- Init ----
        private void InitForm()
        {
            TitreTextBox.Text = _draft.Titre;
            DatePicker.SelectedDate = _draft.Date;

            HeureDebutTextBox.Text = _draft.HeureDebut.ToString(@"hh\:mm");
            HeureFinTextBox.Text = _draft.HeureFin.ToString(@"hh\:mm");

            DescriptionTextBox.Text = _draft.Description ?? string.Empty;

            SelectColorButton(_selectedColor);

            ActiviteComboBox.Items.Clear();
            ActiviteComboBox.Items.Add("Aucune activité");
            foreach (var a in _activites) ActiviteComboBox.Items.Add(a);

            if (_draft.Activite != null)
            {
                var current = _activites.FirstOrDefault(a => a.Titre == _draft.Activite.Titre);
                ActiviteComboBox.SelectedItem = current ?? ActiviteComboBox.Items[0];
            }
            else
            {
                ActiviteComboBox.SelectedIndex = 0;
            }

            TitreTextBox.Focus();
        }

        private void WireEvents()
        {
            LierObjectifCheckBox.Checked += LierObjectif_Changed;
            LierObjectifCheckBox.Unchecked += LierObjectif_Changed;
        }

        // ---- Objectifs ----
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

        private void SelectColorButton(string colorHex)
        {
            foreach (var b in ColorPanel.Children.OfType<Button>())
            {
                var isMatch = (b.Tag?.ToString() ?? "") == (colorHex ?? "");
                b.BorderBrush = isMatch ? Brushes.White : Brushes.Transparent;
                if (isMatch) _selectedColorBtn = b;
            }
        }

        // ---- Actions ----
        private void Save_Click(object sender, RoutedEventArgs e)
        {
            if (!ValidateForm()) return;

            // Remplit la copie _draft depuis le formulaire
            TryParseTime(HeureDebutTextBox.Text, out var startTime);
            TryParseTime(HeureFinTextBox.Text, out var endTime);

            _draft.Titre = TitreTextBox.Text.Trim();
            _draft.Date = DatePicker.SelectedDate!.Value;
            _draft.HeureDebut = startTime;
            _draft.HeureFin = endTime;
            _draft.Couleur = _selectedColor;
            _draft.Description = DescriptionTextBox.Text.Trim();
            _draft.Activite = ActiviteComboBox.SelectedItem as Activite;

            // Expose le résultat à l'appelant, sans toucher _original
            EditedEvent = _draft;

            DialogResult = true;
            Close();
        }

        private void Delete_Click(object sender, RoutedEventArgs e)
        {
            var confirm = MessageBox.Show(
                $"Êtes-vous sûr de vouloir supprimer l'événement '{_original.Titre}' ?",
                "Confirmer la suppression",
                MessageBoxButton.YesNo,
                MessageBoxImage.Question);

            if (confirm != MessageBoxResult.Yes) return;

            IsDeleted = true;
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


        private static Agenda CloneAgenda(Agenda a) => new Agenda
        {
            Titre = a.Titre,
            Description = a.Description,
            Date = a.Date,
            HeureDebut = a.HeureDebut,
            HeureFin = a.HeureFin,
            Couleur = a.Couleur,
            Activite = a.Activite
        };



    }
}
