using Microsoft.Win32;
using ProjetFinale.Models;
using ProjetFinale.Services;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace ProjetFinale.WPF.Pages
{
    public partial class AgendaPage : Page
    {
        // ====== Constantes ======
        private const int START_HOUR = 8;     // heure visible de début (incluse)
        private const int END_HOUR = 22;      // heure visible de fin (exclue pour l'affichage d'événements)
        private const int ROW_HEIGHT_PX = 60; // 60 px = 1 heure => 1 minute = 1 px

        // ====== État ======
        private DateTime _currentWeekStart;
        private ObservableCollection<Agenda> _events = new();
        private Utilisateur _utilisateur = new();

        public AgendaPage()
        {
            InitializeComponent();

            _currentWeekStart = GetStartOfWeek(DateTime.Now);

            // Sécurise l'accès utilisateur/agenda (source de vérité = UserService)
            _utilisateur = UserService.UtilisateurActif ?? new Utilisateur();
            _events = _utilisateur.ListeAgenda ?? new ObservableCollection<Agenda>();

            InitializeCalendar();
            UpdateWeekDisplay();
            UpdateWeekStats();
        }

        // =========================
        // Construction de la grille
        // =========================
        private void InitializeCalendar()
        {
            TimeGrid.Children.Clear();
            TimeGrid.RowDefinitions.Clear();
            TimeGrid.ColumnDefinitions.Clear();

            // Col 0 = libellés heures, puis 7 colonnes pour les jours
            TimeGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(80) });
            for (int i = 0; i < 7; i++)
                TimeGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });

            // Lignes horaires : de 8H à 22H (libellés inclus)
            for (int hour = START_HOUR; hour <= END_HOUR; hour++)
                TimeGrid.RowDefinitions.Add(new RowDefinition { Height = new GridLength(ROW_HEIGHT_PX) });

            // Libellés d'heures + slots cliquables
            for (int hour = START_HOUR; hour <= END_HOUR; hour++)
            {
                int row = hour - START_HOUR;

                var timeLabel = new TextBlock
                {
                    Text = $"{hour:00}H",
                    Style = (Style)FindResource("AgendaTimeTextStyle")
                };
                Grid.SetRow(timeLabel, row);
                Grid.SetColumn(timeLabel, 0);
                TimeGrid.Children.Add(timeLabel);

                for (int day = 0; day < 7; day++)
                {
                    // Tag = "hour,day" pour reconstituer la date/heure au clic
                    var slot = new Border
                    {
                        Style = (Style)FindResource("AgendaTimeSlotStyle"),
                        Tag = $"{hour},{day}"
                    };
                    slot.MouseLeftButtonDown += TimeSlot_MouseLeftButtonDown;

                    Grid.SetRow(slot, row);
                    Grid.SetColumn(slot, day + 1); // +1 car col 0 = heures
                    TimeGrid.Children.Add(slot);
                }
            }
        }

        // ======================
        // Création / Édition UI
        // ======================
        private void TimeSlot_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (sender is not Border { Tag: string tag }) return;

            var parts = tag.Split(',');
            if (parts.Length != 2) return;

            if (!int.TryParse(parts[0], out int hour) || !int.TryParse(parts[1], out int dayOffset)) return;

            var date = _currentWeekStart.AddDays(dayOffset);
            var time = new TimeSpan(hour, 0, 0);
            ShowCreateEventDialog(date, time);
        }

        private void ShowCreateEventDialog(DateTime date, TimeSpan startTime)
        {
            var dialog = new CreateEventDialog(date, startTime, _utilisateur?.ListeActivites)
            {
                Owner = Window.GetWindow(this)
            };

            if (dialog.ShowDialog() == true && dialog.CreatedEvent is Agenda newEvent)
            {
                // Refus si conflit
                if (AgendaService.VerifierConflitHoraire(newEvent, _events.ToList()))
                {
                    MessageBox.Show("Conflit horaire détecté !", "Erreur", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                // Ajout + persistance (source de vérité = AgendaService -> Utilisateur)
                AgendaService.AjouterEvenement(newEvent);
                RefreshCalendar();
            }
        }

        private void Event_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (sender is Border { Tag: Agenda evt })
                ShowEditEventDialog(evt);
        }

        private void ShowEditEventDialog(Agenda original)
        {
            var dialog = new EditEventDialog(original, _utilisateur?.ListeActivites)
            {
                Owner = Window.GetWindow(this)
            };

            if (dialog.ShowDialog() != true) return;

            if (dialog.IsDeleted)
            {
                AgendaService.SupprimerEvenement(original);
                RefreshCalendar();
                return;
            }

            var draft = dialog.EditedEvent ?? original;

            // Exclure l'original pour la détection
            var autres = _events.Where(e => !ReferenceEquals(e, original)).ToList();
            if (AgendaService.VerifierConflitHoraire(draft, autres))
            {
                MessageBox.Show(
                    "Conflit horaire détecté avec un autre événement.",
                    "Conflit d'horaire",
                    MessageBoxButton.OK,
                    MessageBoxImage.Warning
                );
                RefreshCalendar(); // rien n'a été recopié → affichage inchangé
                return;
            }

            // Appliquer la copie dans l'original puis persister
            ApplyAgendaChanges(original, draft);
            AgendaService.ModifierEvenement(original);
            RefreshCalendar();
        }

        private static void ApplyAgendaChanges(Agenda target, Agenda source)
        {
            target.Titre = source.Titre;
            target.Description = source.Description;
            target.Date = source.Date;
            target.HeureDebut = source.HeureDebut;
            target.HeureFin = source.HeureFin;
            target.Couleur = source.Couleur;
            target.Activite = source.Activite;
        }

        // =========================
        // Affichage des événements
        // =========================
        private void RefreshCalendar()
        {
            // Retirer uniquement les "event borders" (Tag = Agenda)
            var toRemove = TimeGrid.Children
                .OfType<Border>()
                .Where(b => b.Tag is Agenda)
                .ToList();

            foreach (var b in toRemove)
                TimeGrid.Children.Remove(b);

            // Afficher les events de la semaine courante
            foreach (var evt in GetWeekEvents())
                DisplayEvent(evt);

            UpdateWeekStats();
        }

        private IEnumerable<Agenda> GetWeekEvents()
        {
            var weekEnd = _currentWeekStart.AddDays(7);
            return _events
                .Where(e => e.Date >= _currentWeekStart && e.Date < weekEnd)
                .OrderBy(e => e.Date)
                .ThenBy(e => e.HeureDebut);
        }

        private void DisplayEvent(Agenda evt)
        {
            // Colonne du jour (Lundi=0..Dimanche=6)
            int day = (int)evt.Date.DayOfWeek;
            if (day == 0) day = 7; // Sunday => 7
            day--;
            int col = day + 1;     // +1 car col 0 = heures

            // Fenêtre visible [START_HOUR, END_HOUR]
            var visibleStart = new TimeSpan(START_HOUR, 0, 0);
            var visibleEnd = new TimeSpan(END_HOUR, 0, 0);

            // Entièrement en dehors -> on n'affiche pas
            if (evt.HeureFin <= visibleStart || evt.HeureDebut >= visibleEnd) return;

            // Clamp pour rester dans la fenêtre visible
            var start = evt.HeureDebut < visibleStart ? visibleStart : evt.HeureDebut;
            var end = evt.HeureFin > visibleEnd ? visibleEnd : evt.HeureFin;

            // Ligne de départ (08:00 => 0)
            int row = start.Hours - START_HOUR;
            if (row < 0 || row > (END_HOUR - START_HOUR)) return; // garde-fou

            // Décalage vertical (1 px = 1 min, car 60 px = 60 min)
            int minuteOffset = start.Minutes;

            // Durée visible (en minutes)
            double durationMinutes = (end - start).TotalMinutes;
            if (durationMinutes <= 0) return;

            // Nombre de lignes "spannées" (pour couvrir plusieurs heures à l'affichage)
            int rowSpan = Math.Max(1, (int)Math.Ceiling((minuteOffset + durationMinutes) / 60.0));

            // Contenu visuel
            var content = new StackPanel();
            content.Children.Add(new TextBlock
            {
                Text = evt.Titre ?? "Sans titre",
                Foreground = Brushes.White,
                FontSize = 12,
                FontWeight = FontWeights.Bold,
                TextWrapping = TextWrapping.Wrap
            });
            content.Children.Add(new TextBlock
            {
                Text = $"{evt.HeureDebut:hh\\:mm} - {evt.HeureFin:hh\\:mm}",
                Foreground = Brushes.White,
                FontSize = 10,
                Opacity = 0.8
            });
            if (evt.Activite != null)
            {
                content.Children.Add(new TextBlock
                {
                    Text = evt.Activite.Titre,
                    Foreground = Brushes.White,
                    FontSize = 10,
                    Opacity = 0.7,
                    TextWrapping = TextWrapping.Wrap
                });
            }

            var border = new Border
            {
                Style = (Style)FindResource("AgendaEventStyle"),
                Height = durationMinutes,                       // 1 px = 1 min
                VerticalAlignment = VerticalAlignment.Top,
                Margin = new Thickness(2, minuteOffset, 2, 0),  // décalage dans la 1re case horaire
                Tag = evt,                                      // pour cleanup + click
                Child = content,
                Background = (SolidColorBrush)(new BrushConverter().ConvertFromString(evt.Couleur ?? "#AF66FF"))
        };
            border.MouseLeftButtonDown += Event_MouseLeftButtonDown;

            Grid.SetRow(border, row);
            Grid.SetColumn(border, col);
            Grid.SetRowSpan(border, rowSpan);

            TimeGrid.Children.Add(border);
        }

        // ====================
        // En-têtes & Statistiques
        // ====================
        private void UpdateWeekDisplay()
        {
            // Ex: "12-18 août 2025"
            CurrentWeekLabel.Text =
                $"{_currentWeekStart:dd}-{_currentWeekStart.AddDays(6):dd} {_currentWeekStart:MMMM yyyy}";

            WeekTitleLabel.Text =
                $"Semaine du {_currentWeekStart:dd} au {_currentWeekStart.AddDays(6):dd} {_currentWeekStart:MMMM yyyy}";

            // Jours affichés sous l'entête
            MondayDate.Text = _currentWeekStart.Day.ToString();
            TuesdayDate.Text = _currentWeekStart.AddDays(1).Day.ToString();
            WednesdayDate.Text = _currentWeekStart.AddDays(2).Day.ToString();
            ThursdayDate.Text = _currentWeekStart.AddDays(3).Day.ToString();
            FridayDate.Text = _currentWeekStart.AddDays(4).Day.ToString();
            SaturdayDate.Text = _currentWeekStart.AddDays(5).Day.ToString();
            SundayDate.Text = _currentWeekStart.AddDays(6).Day.ToString();

            RefreshCalendar();
        }

        private void UpdateWeekStats()
        {
            var weekEvents = GetWeekEvents().ToList();
            EventCountLabel.Text = weekEvents.Count.ToString();

            double totalHours = weekEvents.Sum(e => (e.HeureFin - e.HeureDebut).TotalHours);
            PlannedHoursLabel.Text = $"{totalHours:F1}h";
        }

        // ====================
        // Navigation semaine
        // ====================
        private void PreviousWeek_Click(object sender, RoutedEventArgs e)
        {
            _currentWeekStart = _currentWeekStart.AddDays(-7);
            UpdateWeekDisplay();
        }

        private void NextWeek_Click(object sender, RoutedEventArgs e)
        {
            _currentWeekStart = _currentWeekStart.AddDays(7);
            UpdateWeekDisplay();
        }

        private void GoToToday_Click(object sender, RoutedEventArgs e)
        {
            _currentWeekStart = GetStartOfWeek(DateTime.Now);
            UpdateWeekDisplay();
        }

        // ====================
        // Actions rapides
        // ====================
        private void CreateEvent_Click(object sender, RoutedEventArgs e)
        {
            // Slot par défaut = prochaine heure "ronde" (ou START_HOUR si on dépasse la plage visible)
            int nextHour = DateTime.Now.Hour + 1;
            if (nextHour > END_HOUR) nextHour = START_HOUR;
            ShowCreateEventDialog(DateTime.Today, new TimeSpan(nextHour, 0, 0));
        }

        // ====================
        // Export .ics
        // ====================
        private void ExportIcs_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (_events == null || _events.Count == 0)
                {
                    MessageBox.Show("Aucun événement à exporter.");
                    return;
                }

                var dialog = new SaveFileDialog
                {
                    Title = "Exporter l'agenda",
                    Filter = "Fichier iCalendar (*.ics)|*.ics",
                    DefaultExt = "ics",
                    FileName = $"MonAgenda_{DateTime.Now:yyyy-MM-dd}.ics"
                };

                if (dialog.ShowDialog() != true) return;

                var content = GenerateIcsContent(_events);
                File.WriteAllText(dialog.FileName, content, Encoding.UTF8);
                MessageBox.Show("Export réussi.");
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Erreur : {ex.Message}");
            }
        }

        private string GenerateIcsContent(IEnumerable<Agenda> events)
        {
            var ics = new StringBuilder();
            ics.AppendLine("BEGIN:VCALENDAR");
            ics.AppendLine("VERSION:2.0");
            ics.AppendLine("PRODID:-//ProjetFinale//Agenda Export//FR");

            foreach (var evt in events.OrderBy(e => e.Date).ThenBy(e => e.HeureDebut))
            {
                ics.AppendLine("BEGIN:VEVENT");
                ics.AppendLine($"UID:{GenerateEventUid(evt)}");
                ics.AppendLine($"DTSTAMP:{DateTime.UtcNow:yyyyMMddTHHmmssZ}");
                ics.AppendLine($"DTSTART:{evt.Date.Add(evt.HeureDebut):yyyyMMddTHHmmss}");
                ics.AppendLine($"DTEND:{evt.Date.Add(evt.HeureFin):yyyyMMddTHHmmss}");
                ics.AppendLine($"SUMMARY:{EscapeIcsText(evt.Titre ?? "Sans titre")}");

                if (!string.IsNullOrWhiteSpace(evt.Description))
                    ics.AppendLine($"DESCRIPTION:{EscapeIcsText(evt.Description)}");

                if (evt.Activite != null)
                    ics.AppendLine($"CATEGORIES:{EscapeIcsText(evt.Activite.Titre)}");

                ics.AppendLine("STATUS:CONFIRMED");
                ics.AppendLine("TRANSP:OPAQUE");
                ics.AppendLine("END:VEVENT");
            }

            ics.AppendLine("END:VCALENDAR");
            return ics.ToString();
        }

        private string GenerateEventUid(Agenda evt)
        {
            // Simple et lisible : basé sur date/heure/titre (suffit pour un export local)
            var baseString = $"{evt.Date:yyyyMMdd}-{evt.HeureDebut:hhmm}-{evt.Titre ?? "event"}";
            int hash = baseString.GetHashCode();
            return $"{Math.Abs(hash)}@projetfinale.com";
        }

        private string EscapeIcsText(string text) =>
            text?
                .Replace("\\", "\\\\")
                .Replace(",", "\\,")
                .Replace(";", "\\;")
                .Replace("\n", "\\n")
                .Replace("\r", "")
            ?? "";

        // ====================
        // Utilitaires
        // ====================
        private static DateTime GetStartOfWeek(DateTime date)
        {
            // Lundi = début de semaine
            int diff = (7 + (date.DayOfWeek - DayOfWeek.Monday)) % 7;
            return date.AddDays(-diff).Date;
        }
    }
}


