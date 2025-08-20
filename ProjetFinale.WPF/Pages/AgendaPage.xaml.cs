using Microsoft.Win32;
using ProjetFinale.Models;
using ProjetFinale.Services;
using System;
using System.Collections.Generic;
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
        private const int START_HOUR = 8;
        private const int END_HOUR = 22;
        private const int ROW_HEIGHT_PX = 60;

        private DateTime _currentWeekStart;
        private Utilisateur _utilisateur = new();

        public AgendaPage()
        {
            InitializeComponent();

            _currentWeekStart = GetStartOfWeek(DateTime.Now);
            _utilisateur = UserService.UtilisateurActif ?? new Utilisateur();



            // ✅ Abonnement automatique
            if (_utilisateur.ListeAgenda != null)
                _utilisateur.ListeAgenda.CollectionChanged += (s, e) => RefreshCalendar();


            InitializeCalendar();
            UpdateWeekDisplay();

            RefreshCalendar();
        }

        // =========================
        // Construction de la grille
        // =========================
        private void InitializeCalendar()
        {
            TimeGrid.Children.Clear();
            TimeGrid.RowDefinitions.Clear();
            TimeGrid.ColumnDefinitions.Clear();

            TimeGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(80) });
            for (int i = 0; i < 7; i++)
                TimeGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });

            for (int hour = START_HOUR; hour <= END_HOUR; hour++)
                TimeGrid.RowDefinitions.Add(new RowDefinition { Height = new GridLength(ROW_HEIGHT_PX) });

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
                    var slot = new Border
                    {
                        Style = (Style)FindResource("AgendaTimeSlotStyle"),
                        Tag = $"{hour},{day}"
                    };
                    slot.MouseLeftButtonDown += TimeSlot_MouseLeftButtonDown;

                    Grid.SetRow(slot, row);
                    Grid.SetColumn(slot, day + 1);
                    TimeGrid.Children.Add(slot);
                }
            }
        }

        // ======================
        // Création / Édition
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
                if (AgendaService.VerifierConflitHoraire(newEvent, AgendaService.ChargerAgenda()))
                {
                    MessageBox.Show("Conflit horaire détecté !", "Erreur", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                AgendaService.AjouterEvenement(newEvent);
                
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
                return;
            }

            var draft = dialog.EditedEvent ?? original;

            var autres = AgendaService.ChargerAgenda().Where(e => !ReferenceEquals(e, original)).ToList();
            if (AgendaService.VerifierConflitHoraire(draft, autres))
            {
                MessageBox.Show("Conflit horaire détecté !", "Erreur", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            ApplyAgendaChanges(original, draft);
            AgendaService.ModifierEvenement(original);
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
            var toRemove = TimeGrid.Children.OfType<Border>().Where(b => b.Tag is Agenda).ToList();
            foreach (var b in toRemove) TimeGrid.Children.Remove(b);

            foreach (var evt in GetWeekEvents())
                DisplayEvent(evt);

            UpdateWeekStats();

        }

        private IEnumerable<Agenda> GetWeekEvents()
        {
            var weekEnd = _currentWeekStart.AddDays(7);
            return AgendaService.ChargerAgenda()
                .Where(e => e.Date >= _currentWeekStart && e.Date < weekEnd)
                .OrderBy(e => e.Date)
                .ThenBy(e => e.HeureDebut);
        }

        private void DisplayEvent(Agenda evt)
        {
            int day = (int)evt.Date.DayOfWeek;
            if (day == 0) day = 7;
            day--;
            int col = day + 1;

            var visibleStart = new TimeSpan(START_HOUR, 0, 0);
            var visibleEnd = new TimeSpan(END_HOUR, 0, 0);

            if (evt.HeureFin <= visibleStart || evt.HeureDebut >= visibleEnd) return;

            var start = evt.HeureDebut < visibleStart ? visibleStart : evt.HeureDebut;
            var end = evt.HeureFin > visibleEnd ? visibleEnd : evt.HeureFin;

            int row = start.Hours - START_HOUR;
            if (row < 0 || row > (END_HOUR - START_HOUR)) return;

            int minuteOffset = start.Minutes;
            double durationMinutes = (end - start).TotalMinutes;
            if (durationMinutes <= 0) return;

            int rowSpan = Math.Max(1, (int)Math.Ceiling((minuteOffset + durationMinutes) / 60.0));

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
                Height = durationMinutes,
                VerticalAlignment = VerticalAlignment.Top,
                Margin = new Thickness(2, minuteOffset, 2, 0),
                Tag = evt,
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
        // En-têtes & Stats
        // ====================
        private void UpdateWeekDisplay()
        {
            CurrentWeekLabel.Text =
                $"{_currentWeekStart:dd}-{_currentWeekStart.AddDays(6):dd} {_currentWeekStart:MMMM yyyy}";

            WeekTitleLabel.Text =
                $"Semaine du {_currentWeekStart:dd} au {_currentWeekStart.AddDays(6):dd} {_currentWeekStart:MMMM yyyy}";

            MondayDate.Text = _currentWeekStart.Day.ToString();
            TuesdayDate.Text = _currentWeekStart.AddDays(1).Day.ToString();
            WednesdayDate.Text = _currentWeekStart.AddDays(2).Day.ToString();
            ThursdayDate.Text = _currentWeekStart.AddDays(3).Day.ToString();
            FridayDate.Text = _currentWeekStart.AddDays(4).Day.ToString();
            SaturdayDate.Text = _currentWeekStart.AddDays(5).Day.ToString();
            SundayDate.Text = _currentWeekStart.AddDays(6).Day.ToString();
        }

        private void UpdateWeekStats()
        {
            var stats = AgendaService.ObtenirStatistiquesSemaine(_currentWeekStart);
            EventCountLabel.Text = stats.NombreEvenements.ToString();
            PlannedHoursLabel.Text = $"{stats.HeuresTotal:F1}h";
        }

        // ====================
        // Navigation
        // ====================
        private void PreviousWeek_Click(object sender, RoutedEventArgs e)
        {
            _currentWeekStart = _currentWeekStart.AddDays(-7);
            UpdateWeekDisplay();
            RefreshCalendar();
        }

        private void NextWeek_Click(object sender, RoutedEventArgs e)
        {
            _currentWeekStart = _currentWeekStart.AddDays(7);
            UpdateWeekDisplay();
            RefreshCalendar();
        }

        private void GoToToday_Click(object sender, RoutedEventArgs e)
        {
            _currentWeekStart = GetStartOfWeek(DateTime.Now);
            UpdateWeekDisplay();
            RefreshCalendar();
        }

        // ====================
        // Actions rapides
        // ====================
        private void CreateEvent_Click(object sender, RoutedEventArgs e)
        {
            int nextHour = DateTime.Now.Hour + 1;
            if (nextHour > END_HOUR) nextHour = START_HOUR;
            ShowCreateEventDialog(DateTime.Today, new TimeSpan(nextHour, 0, 0));
        }

        private void ExportIcs_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var events = AgendaService.ChargerAgenda();
                if (events == null || events.Count == 0)
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

                var content = GenerateIcsContent(events);
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

        private static DateTime GetStartOfWeek(DateTime date)
        {
            int diff = (7 + (date.DayOfWeek - DayOfWeek.Monday)) % 7;
            return date.AddDays(-diff).Date;
        }
    }
}
