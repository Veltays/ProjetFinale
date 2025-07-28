using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using ProjetFinale.Models;
using ProjetFinale.Services;

namespace ProjetFinale.WPF.Pages
{
    public partial class AgendaPage : Page
    {
        private DateTime _currentWeekStart;
        private ObservableCollection<Agenda> _events;
        private Utilisateur _utilisateur;

        public AgendaPage()
        {
            InitializeComponent();
            _currentWeekStart = GetStartOfWeek(DateTime.Now);
            _events = new ObservableCollection<Agenda>();

            // S'abonner aux changements d'utilisateur
            UserService.UtilisateurActifChanged += OnUtilisateurChanged;
            LoadCurrentUser();

            InitializeCalendar();
            UpdateWeekDisplay();

            var allEvents = AgendaService.ChargerAgenda();
            _events.Clear();

            foreach (var evt in allEvents)
            {
                _events.Add(evt);
            }

            RefreshCalendar();
            UpdateWeekStats();

        }

        private void LoadCurrentUser()
        {
            _utilisateur = UserService.UtilisateurActif;
            if (_utilisateur != null)
            {
                _events = _utilisateur.ListeAgenda ?? new ObservableCollection<Agenda>();
                RefreshCalendar();
            }
        }

        private void OnUtilisateurChanged(Utilisateur? user)
        {
            LoadCurrentUser(); // Ou tu peux aussi passer "user" si besoin
        }
        private void InitializeCalendar()
        {
            CreateTimeGrid();
        }

        private void CreateTimeGrid()
        {
            TimeGrid.Children.Clear();
            TimeGrid.RowDefinitions.Clear();
            TimeGrid.ColumnDefinitions.Clear();

            // Colonnes : Heures + 7 jours
            TimeGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(80) }); // Heures
            for (int i = 0; i < 7; i++)
            {
                TimeGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
            }

            // Lignes : 24 heures (8h à 22h = 15 heures)
            for (int hour = 8; hour <= 22; hour++)
            {
                TimeGrid.RowDefinitions.Add(new RowDefinition { Height = new GridLength(60) });
            }

            // Créer les créneaux horaires
            for (int hour = 8; hour <= 22; hour++)
            {
                int row = hour - 8;

                // Label d'heure
                var timeLabel = new TextBlock
                {
                    Text = $"{hour:00}H",
                    Style = (Style)FindResource("TimeTextStyle")
                };
                Grid.SetRow(timeLabel, row);
                Grid.SetColumn(timeLabel, 0);
                TimeGrid.Children.Add(timeLabel);

                // Créneaux pour chaque jour
                for (int day = 0; day < 7; day++)
                {
                    var timeSlot = new Border
                    {
                        Style = (Style)FindResource("TimeSlotStyle"),
                        Tag = $"{hour},{day}" // Format simple "heure,jour"
                    };

                    timeSlot.MouseLeftButtonDown += TimeSlot_MouseLeftButtonDown;

                    Grid.SetRow(timeSlot, row);
                    Grid.SetColumn(timeSlot, day + 1);
                    TimeGrid.Children.Add(timeSlot);
                }
            }

            RefreshCalendar();
        }

        private void TimeSlot_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (sender is Border border && border.Tag is string tagData)
            {
                var parts = tagData.Split(',');
                if (parts.Length == 2 &&
                    int.TryParse(parts[0], out int hour) &&
                    int.TryParse(parts[1], out int dayOffset))
                {
                    DateTime selectedDate = _currentWeekStart.AddDays(dayOffset);
                    TimeSpan selectedTime = new TimeSpan(hour, 0, 0);

                    ShowCreateEventDialog(selectedDate, selectedTime);
                }
            }
        }

        private void ShowCreateEventDialog(DateTime date, TimeSpan startTime)
        {
            var dialog = new CreateEventDialog(date, startTime, _utilisateur?.ListeActivites);
            dialog.Owner = Window.GetWindow(this);

            if (dialog.ShowDialog() == true)
            {
                var newEvent = dialog.CreatedEvent;
                if (newEvent != null)
                {
                    // Vérifier les conflits horaires
                    if (!AgendaService.VerifierConflitHoraire(newEvent))
                    {
                        // Ajouter à la liste locale
                        _events.Add(newEvent);

                        // Ajouter à l'utilisateur
                        if (_utilisateur != null)
                        {
                            _utilisateur.ListeAgenda.Add(newEvent);
                        }

                        // Sauvegarder avec AgendaService
                        AgendaService.AjouterEvenement(newEvent);

                        RefreshCalendar();
                        UpdateWeekStats();
                    }
                    else
                    {
                        MessageBox.Show("Conflit horaire détecté ! Un autre événement existe déjà à cette heure.",
                                      "Erreur", MessageBoxButton.OK, MessageBoxImage.Warning);
                    }
                }
            }
        }

        private void RefreshCalendar()
        {
            // Effacer les événements existants
            var eventsToRemove = TimeGrid.Children.OfType<Border>()
                .Where(b => b.Style == (Style)FindResource("EventStyle"))
                .ToList();

            foreach (var eventBorder in eventsToRemove)
            {
                TimeGrid.Children.Remove(eventBorder);
            }

            // Afficher les événements de la semaine courante
            var weekEvents = _events.Where(e =>
                e.Date >= _currentWeekStart &&
                e.Date < _currentWeekStart.AddDays(7)).ToList();

            foreach (var evt in weekEvents)
            {
                DisplayEvent(evt);
            }

            UpdateWeekStats();
        }

        private void DisplayEvent(Agenda evt)
        {
            int dayOfWeek = (int)evt.Date.DayOfWeek;
            if (dayOfWeek == 0) dayOfWeek = 7; // Dimanche = 7
            dayOfWeek--; // Ajuster pour l'index (Lundi = 0)

            int startHour = evt.HeureDebut.Hours;
            int endHour = evt.HeureFin.Hours;

            // Vérifier si l'événement est dans la plage horaire affichée (8h-22h)
            if (startHour < 8 || startHour > 22) return;

            int row = startHour - 8;
            int column = dayOfWeek + 1;

            // Calculer la hauteur en fonction de la durée
            double duration = (evt.HeureFin - evt.HeureDebut).TotalHours;
            double height = duration * 60; // 60px par heure

            var eventBorder = new Border
            {
                Style = (Style)FindResource("EventStyle"),
                Height = height,
                VerticalAlignment = VerticalAlignment.Top,
                Tag = evt
            };

            // Contenu de l'événement
            var eventContent = new StackPanel();

            var titleText = new TextBlock
            {
                Text = evt.Titre ?? "Sans titre",
                Foreground = Brushes.White,
                FontFamily = new FontFamily("Kameron"),
                FontSize = 12,
                FontWeight = FontWeights.Bold,
                TextWrapping = TextWrapping.Wrap
            };
            eventContent.Children.Add(titleText);

            var timeText = new TextBlock
            {
                Text = $"{evt.HeureDebut:hh\\:mm} - {evt.HeureFin:hh\\:mm}",
                Foreground = Brushes.White,
                FontFamily = new FontFamily("Kameron"),
                FontSize = 10,
                Opacity = 0.8
            };
            eventContent.Children.Add(timeText);

            if (evt.Activite != null)
            {
                var activityText = new TextBlock
                {
                    Text = evt.Activite.Titre,
                    Foreground = Brushes.White,
                    FontFamily = new FontFamily("Kameron"),
                    FontSize = 10,
                    Opacity = 0.7,
                    TextWrapping = TextWrapping.Wrap
                };
                eventContent.Children.Add(activityText);
            }

            eventBorder.Child = eventContent;
            eventBorder.MouseLeftButtonDown += Event_MouseLeftButtonDown;

            Grid.SetRow(eventBorder, row);
            Grid.SetColumn(eventBorder, column);
            TimeGrid.Children.Add(eventBorder);
        }

        private void Event_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (sender is Border border && border.Tag is Agenda evt)
            {
                ShowEditEventDialog(evt);
            }
        }

        private void ShowEditEventDialog(Agenda evt)
        {
            var dialog = new EditEventDialog(evt, _utilisateur?.ListeActivites);
            dialog.Owner = Window.GetWindow(this);

            if (dialog.ShowDialog() == true)
            {
                if (dialog.IsDeleted)
                {
                    // Supprimer de la liste locale et de l'utilisateur
                    _events.Remove(evt);
                    _utilisateur?.ListeAgenda.Remove(evt);

                    // Sauvegarder avec AgendaService
                    AgendaService.SupprimerEvenement(evt);
                }
                else
                {
                    // L'événement a été modifié, sauvegarder
                    AgendaService.ModifierEvenement(evt);
                }

                RefreshCalendar();
                UpdateWeekStats();
            }
        }

        private void UpdateWeekDisplay()
        {
            // Mettre à jour les labels de dates
            var culture = new CultureInfo("fr-FR");

            CurrentWeekLabel.Text = $"{_currentWeekStart:dd}-{_currentWeekStart.AddDays(6):dd} {_currentWeekStart:MMMM yyyy}";
            WeekTitleLabel.Text = $"Semaine du {_currentWeekStart:dd} au {_currentWeekStart.AddDays(6):dd} {_currentWeekStart:MMMM yyyy}";

            // Mettre à jour les dates des jours
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
            var weekEvents = _events.Where(e =>
                e.Date >= _currentWeekStart &&
                e.Date < _currentWeekStart.AddDays(7)).ToList();

            EventCountLabel.Text = weekEvents.Count.ToString();

            double totalHours = weekEvents.Sum(e => (e.HeureFin - e.HeureDebut).TotalHours);
            PlannedHoursLabel.Text = $"{totalHours:F1}h";
        }

        private DateTime GetStartOfWeek(DateTime date)
        {
            int diff = (7 + (date.DayOfWeek - DayOfWeek.Monday)) % 7;
            return date.AddDays(-1 * diff).Date;
        }

        // Event handlers pour les boutons
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

        private void CreateEvent_Click(object sender, RoutedEventArgs e)
        {
            // Créer un événement pour aujourd'hui à l'heure actuelle
            DateTime today = DateTime.Today;
            TimeSpan currentTime = DateTime.Now.TimeOfDay;

            // Arrondir à l'heure suivante
            int nextHour = currentTime.Hours + 1;
            if (nextHour > 22) nextHour = 9; // Si trop tard, proposer 9h

            ShowCreateEventDialog(today, new TimeSpan(nextHour, 0, 0));
        }

        private void GoToToday_Click(object sender, RoutedEventArgs e)
        {
            _currentWeekStart = GetStartOfWeek(DateTime.Now);
            UpdateWeekDisplay();
        }

        // Nettoyer les événements
        private void Page_Unloaded(object sender, RoutedEventArgs e)
        {
            UserService.UtilisateurActifChanged -= OnUtilisateurChanged;
        }

    }
}