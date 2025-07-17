using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace ProjetFinale.WPF
{
    public partial class SettingsPage : Page
    {
        private SettingsManager settingsManager;

        public SettingsPage()
        {
            InitializeComponent();
            settingsManager = new SettingsManager();

            ChargerParametres();
            ConfigurerEvenements();
        }

        // Charger les paramètres depuis le registry et les afficher
        private void ChargerParametres()
        {
            var settings = settingsManager.GetCurrentSettings();

            // ComboBox
            FormatPoidsComboBox.SelectedIndex = settings.FormatPoids == "LBS" ? 1 : 0;
            FormatHeureComboBox.SelectedIndex = settings.FormatHeure == "12H" ? 1 : 0;
            ThemeColorComboBox.SelectedIndex = ObtenirIndexTheme(settings.ThemeCouleur);
            SaveFrequencyComboBox.SelectedIndex = ObtenirIndexFrequence(settings.FrequenceSauvegarde);

            // Toggles
            DarkModeToggle.IsChecked = settings.ModeSombre;
            WorkoutRemindersToggle.IsChecked = settings.RappelsEntrainement;
            GoalRemindersToggle.IsChecked = settings.RappelsObjectifs;
            AutoSaveToggle.IsChecked = settings.SauvegardeAuto;
        }

        // Connecter tous les événements aux méthodes
        private void ConfigurerEvenements()
        {
            // ComboBox
            FormatPoidsComboBox.SelectionChanged += FormatPoids_Change;
            FormatHeureComboBox.SelectionChanged += FormatHeure_Change;
            ThemeColorComboBox.SelectionChanged += ThemeCouleur_Change;
            SaveFrequencyComboBox.SelectionChanged += FrequenceSauvegarde_Change;

            // Toggles
            DarkModeToggle.Checked += ModeSombre_Change;
            DarkModeToggle.Unchecked += ModeSombre_Change;
            WorkoutRemindersToggle.Checked += RappelsEntrainement_Change;
            WorkoutRemindersToggle.Unchecked += RappelsEntrainement_Change;
            GoalRemindersToggle.Checked += RappelsObjectifs_Change;
            GoalRemindersToggle.Unchecked += RappelsObjectifs_Change;
            AutoSaveToggle.Checked += SauvegardeAuto_Change;
            AutoSaveToggle.Unchecked += SauvegardeAuto_Change;

            // Boutons
            ModifierProfilButton.Click += ModifierProfil_Click;
            DeleteAccountButton.Click += SupprimerCompte_Click;
        }

        // === ÉVÉNEMENTS COMBOBOX ===

        private void FormatPoids_Change(object sender, SelectionChangedEventArgs e)
        {
            var item = FormatPoidsComboBox.SelectedItem as ComboBoxItem;
            var format = item?.Content.ToString();

            if (!string.IsNullOrEmpty(format))
            {
                settingsManager.UpdateFormatPoids(format);
                AfficherMessage($"Format poids changé: {format}");
            }
        }

        private void FormatHeure_Change(object sender, SelectionChangedEventArgs e)
        {
            var item = FormatHeureComboBox.SelectedItem as ComboBoxItem;
            var format = item?.Content.ToString();

            if (!string.IsNullOrEmpty(format))
            {
                settingsManager.UpdateFormatHeure(format);
                AfficherMessage($"Format heure changé: {format}");
            }
        }

        private void ThemeCouleur_Change(object sender, SelectionChangedEventArgs e)
        {
            var item = ThemeColorComboBox.SelectedItem as ComboBoxItem;
            var theme = item?.Content.ToString();

            if (!string.IsNullOrEmpty(theme))
            {
                settingsManager.UpdateTheme(theme);
                ChangerCouleurTheme(theme); // Appliquer le nouveau thème visuel
                AfficherMessage($"Thème changé: {theme}");
            }
        }

        private void FrequenceSauvegarde_Change(object sender, SelectionChangedEventArgs e)
        {
            var item = SaveFrequencyComboBox.SelectedItem as ComboBoxItem;
            var frequence = item?.Content.ToString();

            if (!string.IsNullOrEmpty(frequence))
            {
                settingsManager.UpdateSaveFrequency(frequence);
                AfficherMessage($"Fréquence sauvegarde: {frequence}");
            }
        }

        // === ÉVÉNEMENTS TOGGLES ===

        private void ModeSombre_Change(object sender, RoutedEventArgs e)
        {
            bool active = DarkModeToggle.IsChecked == true;
            settingsManager.UpdateDarkMode(active);
            AfficherMessage($"Mode sombre: {(active ? "Activé" : "Désactivé")}");
        }

        private void RappelsEntrainement_Change(object sender, RoutedEventArgs e)
        {
            bool active = WorkoutRemindersToggle.IsChecked == true;
            settingsManager.UpdateWorkoutReminders(active);
            AfficherMessage($"Rappels entraînement: {(active ? "Activés" : "Désactivés")}");
        }

        private void RappelsObjectifs_Change(object sender, RoutedEventArgs e)
        {
            bool active = GoalRemindersToggle.IsChecked == true;
            settingsManager.UpdateGoalReminders(active);
            AfficherMessage($"Rappels objectifs: {(active ? "Activés" : "Désactivés")}");
        }

        private void SauvegardeAuto_Change(object sender, RoutedEventArgs e)
        {
            bool active = AutoSaveToggle.IsChecked == true;
            settingsManager.UpdateAutoSave(active);
            AfficherMessage($"Sauvegarde auto: {(active ? "Activée" : "Désactivée")}");
        }

        // === ÉVÉNEMENTS BOUTONS ===

        private void ModifierProfil_Click(object sender, RoutedEventArgs e)
        {
            // TODO: Naviguer vers la page de profil
            AfficherMessage("Redirection vers modification profil...");
        }

        private void SupprimerCompte_Click(object sender, RoutedEventArgs e)
        {
            // Demander confirmation 2 fois (c'est dangereux !)
            if (DemanderConfirmationSuppression())
            {
                settingsManager.DeleteAccount();
                AfficherMessage("Compte supprimé !");

                // TODO: Retourner à l'écran de connexion
                // Application.Current.Shutdown();
            }
        }

        // === MÉTHODES UTILES ===

        // Changer la couleur des bordures selon le thème choisi
        private void ChangerCouleurTheme(string theme)
        {
            SolidColorBrush couleur;

            switch (theme.ToLower())
            {
                case "violet":
                    couleur = new SolidColorBrush(Color.FromRgb(175, 102, 255));
                    break;
                case "bleu":
                    couleur = new SolidColorBrush(Color.FromRgb(59, 130, 246));
                    break;
                case "rose":
                    couleur = new SolidColorBrush(Color.FromRgb(244, 114, 182));
                    break;
                case "vert":
                    couleur = new SolidColorBrush(Color.FromRgb(34, 197, 94));
                    break;
                default:
                    couleur = new SolidColorBrush(Color.FromRgb(175, 102, 255));
                    break;
            }

            // Appliquer la nouvelle couleur aux ComboBox
            FormatPoidsComboBox.BorderBrush = couleur;
            FormatHeureComboBox.BorderBrush = couleur;
            ThemeColorComboBox.BorderBrush = couleur;
            SaveFrequencyComboBox.BorderBrush = couleur;
        }

        // Convertir le nom du thème en index pour la ComboBox
        private int ObtenirIndexTheme(string theme)
        {
            return theme.ToLower() switch
            {
                "violet" => 0,
                "bleu" => 1,
                "rose" => 2,
                "vert" => 3,
                _ => 0 // Par défaut violet
            };
        }

        // Convertir la fréquence en index pour la ComboBox
        private int ObtenirIndexFrequence(string frequence)
        {
            return frequence switch
            {
                "1 min" => 0,
                "5 min" => 1,
                "15 min" => 2,
                "30 min" => 3,
                _ => 1 // Par défaut 5 min
            };
        }

        // Demander confirmation pour la suppression du compte
        private bool DemanderConfirmationSuppression()
        {
            var result1 = MessageBox.Show(
                "ATTENTION ! Voulez-vous vraiment supprimer votre compte ?\n\nToutes vos données seront perdues !",
                "⚠️ Suppression du compte",
                MessageBoxButton.YesNo,
                MessageBoxImage.Warning);

            if (result1 == MessageBoxResult.Yes)
            {
                var result2 = MessageBox.Show(
                    "DERNIÈRE CHANCE !\n\nÊtes-vous VRAIMENT sûr de vouloir supprimer définitivement votre compte ?",
                    "⚠️ Confirmation finale",
                    MessageBoxButton.YesNo,
                    MessageBoxImage.Stop);

                return result2 == MessageBoxResult.Yes;
            }

            return false;
        }

        // Afficher un message à l'utilisateur
        private void AfficherMessage(string message)
        {
            MessageBox.Show(message, "Paramètres", MessageBoxButton.OK, MessageBoxImage.Information);
        }
    }
}