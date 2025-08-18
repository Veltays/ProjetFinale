using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using ProjetFinale.Utils;

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

        private void ChargerParametres()
        {
            var settings = settingsManager.GetCurrentSettings();

            SettingsContext.Instance.WeightUnit = settings.FormatPoids == "LBS" ? WeightUnit.LBS : WeightUnit.KG;
            SettingsContext.Instance.HeightUnit = settings.FormatTaille == "INCH" ? HeightUnit.INCH : HeightUnit.CM;

            FormatPoidsComboBox.SelectedIndex = settings.FormatPoids == "LBS" ? 1 : 0;
            FormatTailleComboBox.SelectedIndex = settings.FormatTaille == "INCH" ? 1 : 0;
            SaveFrequencyComboBox.SelectedIndex = ObtenirIndexFrequence(settings.FrequenceSauvegarde);

            DarkModeToggle.IsChecked = settings.ModeSombre;
            AutoSaveToggle.IsChecked = settings.SauvegardeAuto;
        }

        private void ConfigurerEvenements()
        {
            FormatPoidsComboBox.SelectionChanged += FormatPoids_Change;
            FormatTailleComboBox.SelectionChanged += FormatTaille_Change;
            SaveFrequencyComboBox.SelectionChanged += FrequenceSauvegarde_Change;

            DarkModeToggle.Checked += ModeSombre_Change;
            DarkModeToggle.Unchecked += ModeSombre_Change;
            AutoSaveToggle.Checked += SauvegardeAuto_Change;
            AutoSaveToggle.Unchecked += SauvegardeAuto_Change;


        }

        private void FormatTaille_Change(object sender, SelectionChangedEventArgs e)
        {
            var item = SaveAsString(FormatTailleComboBox.SelectedItem);
            if (string.IsNullOrEmpty(item)) return;

            settingsManager.UpdateFormatTaille(item);
            SettingsContext.Instance.HeightUnit = (item == "INCH") ? HeightUnit.INCH : HeightUnit.CM;
            AfficherMessage($"Format taille changé: {item}");
        }

        private void FormatPoids_Change(object sender, SelectionChangedEventArgs e)
        {
            var format = SaveAsString(FormatPoidsComboBox.SelectedItem);
            if (string.IsNullOrEmpty(format)) return;

            settingsManager.UpdateFormatPoids(format);
            SettingsContext.Instance.WeightUnit = (format == "LBS") ? WeightUnit.LBS : WeightUnit.KG;
        }

        private void FrequenceSauvegarde_Change(object sender, SelectionChangedEventArgs e)
        {
            var frequence = SaveAsString(SaveFrequencyComboBox.SelectedItem);
            if (!string.IsNullOrEmpty(frequence))
            {
                settingsManager.UpdateSaveFrequency(frequence);
                AfficherMessage($"Fréquence sauvegarde: {frequence}");
            }
        }

        private void ModeSombre_Change(object sender, RoutedEventArgs e)
        {
            bool active = DarkModeToggle.IsChecked == true;
            settingsManager.UpdateDarkMode(active);
        }

        private void SauvegardeAuto_Change(object sender, RoutedEventArgs e)
        {
            bool active = AutoSaveToggle.IsChecked == true;
            settingsManager.UpdateAutoSave(active);
            AfficherMessage($"Sauvegarde auto: {(active ? "Activée" : "Désactivée")}");
        }

        private void ModifierProfil_Click(object sender, RoutedEventArgs e)
        {
            AfficherMessage("Redirection vers modification profil...");
        }

        private void SupprimerCompte_Click(object sender, RoutedEventArgs e)
        {
            if (DemanderConfirmationSuppression())
            {
                settingsManager.DeleteAccount();
                AfficherMessage("Compte supprimé !");
            }
        }

        
        private int ObtenirIndexFrequence(string frequence) => frequence switch
        {
            "1 min" => 0,
            "5 min" => 1,
            "15 min" => 2,
            "30 min" => 3,
            _ => 1
        };

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

        private void AfficherMessage(string message)
        {
            MessageBox.Show(message, "Paramètres", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private static string SaveAsString(object comboItem)
        {
            if (comboItem is ComboBoxItem cbi)
                return cbi.Content?.ToString() ?? string.Empty;
            return comboItem?.ToString() ?? string.Empty;
        }
    }
}
