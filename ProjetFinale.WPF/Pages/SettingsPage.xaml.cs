using System.Windows;
using System.Windows.Controls;
using ProjetFinale.Utils;

namespace ProjetFinale.WPF
{
    public partial class SettingsPage : Page
    {
        private readonly SettingsManager _settings;

        public SettingsPage()
        {
            InitializeComponent();
            _settings = new SettingsManager();

            AppliquerParametresAuxUI();
            BrancherEvenements();
        }

        // ---- INIT ----
        private void AppliquerParametresAuxUI()
        {
            var s = _settings.GetCurrentSettings();

            // Contexte global (unités)
            SettingsContext.Instance.WeightUnit = s.FormatPoids == "LBS" ? WeightUnit.LBS : WeightUnit.KG;
            SettingsContext.Instance.HeightUnit = s.FormatTaille == "INCH" ? HeightUnit.INCH : HeightUnit.CM;

            // UI
            FormatPoidsComboBox.SelectedIndex = (s.FormatPoids == "LBS") ? 1 : 0;
            FormatTailleComboBox.SelectedIndex = (s.FormatTaille == "INCH") ? 1 : 0;
            SaveFrequencyComboBox.SelectedIndex = IndexFrequence(s.FrequenceSauvegarde);

            DarkModeToggle.IsChecked = s.ModeSombre;
            AutoSaveToggle.IsChecked = s.SauvegardeAuto;
        }

        private void BrancherEvenements()
        {
            // Une seule méthode pour les 3 ComboBox  // c'est le WPF qui fait l'invoke
            FormatPoidsComboBox.SelectionChanged += Combo_SelectionChanged;
            FormatTailleComboBox.SelectionChanged += Combo_SelectionChanged;
            SaveFrequencyComboBox.SelectionChanged += Combo_SelectionChanged;

            // Une seule méthode pour les 2 toggles
            DarkModeToggle.Checked += Toggle_Changed;
            DarkModeToggle.Unchecked += Toggle_Changed;
            AutoSaveToggle.Checked += Toggle_Changed;
            AutoSaveToggle.Unchecked += Toggle_Changed;
        }

        // ---- HANDLERS ----
        private void Combo_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (sender == null || e.AddedItems.Count == 0) return;
            var value = ContentAsString(e.AddedItems[0]);
            if (string.IsNullOrWhiteSpace(value)) return;

            if (sender == FormatPoidsComboBox)
            {
                _settings.UpdateFormatPoids(value);
                SettingsContext.Instance.WeightUnit = (value == "LBS") ? WeightUnit.LBS : WeightUnit.KG;
                return;
            }

            if (sender == FormatTailleComboBox)
            {
                _settings.UpdateFormatTaille(value);
                SettingsContext.Instance.HeightUnit = (value == "INCH") ? HeightUnit.INCH : HeightUnit.CM;
                return;
            }

            if (sender == SaveFrequencyComboBox)
            {
                _settings.UpdateSaveFrequency(value);
            }
        }

        private void Toggle_Changed(object sender, RoutedEventArgs e)
        {
            if (sender == DarkModeToggle)
            {
                _settings.UpdateDarkMode(DarkModeToggle.IsChecked == true);
                return;
            }

            if (sender == AutoSaveToggle)
            {
                bool active = AutoSaveToggle.IsChecked == true;
                _settings.UpdateAutoSave(active);
            }
        }

        // ---- HELPERS ----
        private static int IndexFrequence(string f) => f switch
        {
            "1 min" => 0,
            "5 min" => 1,
            "15 min" => 2,
            "30 min" => 3,
            _ => 1
        };

        private static string ContentAsString(object? item) =>
            item is ComboBoxItem { Content: not null } c ? c.Content.ToString()! : item?.ToString() ?? string.Empty;


    }
}
