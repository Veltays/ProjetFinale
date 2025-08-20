using Microsoft.Win32;
using System;

namespace ProjetFinale.Utils
{
    /// <summary>
    /// Gestionnaire des paramètres - logique métier uniquement (Registry)
    /// </summary>
    public class SettingsManager
    {
        private const string REGISTRY_PATH = @"Software\ProjetFinale";

        public SettingsManager() => EnsureRegistryKeyExists();

        // ========== Properties (Registry) ==========
        public string FormatPoids
        {
            get => GetRegistryValue("FormatPoids", "KG");
            set => SetRegistryValue("FormatPoids", value);
        }

        public string FormatTaille
        {
            get => GetRegistryValue("FormatTaille", "CM");
            set => SetRegistryValue("FormatTaille", value);
        }

        public string ThemeCouleur
        {
            get => GetRegistryValue("ThemeCouleur", "Violet");
            set => SetRegistryValue("ThemeCouleur", value);
        }

        public string FrequenceSauvegarde
        {
            get => GetRegistryValue("FrequenceSauvegarde", "5 min");
            set => SetRegistryValue("FrequenceSauvegarde", value);
        }

        public bool ModeSombre
        {
            get => GetRegistryBoolValue("ModeSombre", true);
            set => SetRegistryValue("ModeSombre", value.ToString().ToLower());
        }

        public bool RappelsEntrainement
        {
            get => GetRegistryBoolValue("RappelsEntrainement", true);
            set => SetRegistryValue("RappelsEntrainement", value.ToString().ToLower());
        }

        public bool RappelsObjectifs
        {
            get => GetRegistryBoolValue("RappelsObjectifs", false);
            set => SetRegistryValue("RappelsObjectifs", value.ToString().ToLower());
        }

        public bool SauvegardeAuto
        {
            get => GetRegistryBoolValue("SauvegardeAuto", true);
            set => SetRegistryValue("SauvegardeAuto", value.ToString().ToLower());
        }

        // ========== Public ==========
        public AppSettings GetCurrentSettings() => new()
        {
            FormatPoids = FormatPoids,
            FormatTaille = FormatTaille,
            FrequenceSauvegarde = FrequenceSauvegarde,
            ModeSombre = ModeSombre,
            SauvegardeAuto = SauvegardeAuto
        };

        public void UpdateFormatPoids(string format)
        {
            if (!IsValidWeightFormat(format)) return;
            FormatPoids = format;
            UpdateLastModified();
            ConvertExistingWeightData(format);
        }

        public void UpdateFormatTaille(string format) // "CM" ou "INCH"
        {
            if (format != "CM" && format != "INCH") return;
            FormatTaille = format;
            UpdateLastModified();
        }

        public void UpdateTheme(string theme)
        {
            if (!IsValidTheme(theme)) return;
            ThemeCouleur = theme;
            UpdateLastModified();
            ApplyThemeToApplication(theme);
        }

        public void UpdateSaveFrequency(string frequency)
        {
            if (!IsValidSaveFrequency(frequency)) return;
            FrequenceSauvegarde = frequency;
            UpdateLastModified();
            ReconfigureAutoSaveTimer(frequency);
        }

        public void UpdateDarkMode(bool isDarkMode)
        {
            ModeSombre = isDarkMode;
            UpdateLastModified();
            ApplyDarkModeToApplication(isDarkMode);
        }

        public void UpdateWorkoutReminders(bool isEnabled)
        {
            RappelsEntrainement = isEnabled;
            UpdateLastModified();
            ConfigureWorkoutNotifications(isEnabled);
        }

        public void UpdateGoalReminders(bool isEnabled)
        {
            RappelsObjectifs = isEnabled;
            UpdateLastModified();
            ConfigureGoalNotifications(isEnabled);
        }

        public void UpdateAutoSave(bool isEnabled)
        {
            SauvegardeAuto = isEnabled;
            UpdateLastModified();
            ConfigureAutoSave(isEnabled);
        }

        public void DeleteAccount()
        {
            try
            {
                DeleteAllUserData();
                DeleteAllSettings();
                LogAccountDeletion();
            }
            catch (Exception ex)
            {
                throw new Exception($"Erreur lors de la suppression du compte : {ex.Message}");
            }
        }

        public void ResetToDefaults()
        {
            FormatPoids = "KG";
            FormatTaille = "CM";
            ThemeCouleur = "Violet";
            FrequenceSauvegarde = "5 min";
            ModeSombre = true;
            RappelsEntrainement = true;
            RappelsObjectifs = false;
            SauvegardeAuto = true;
            UpdateLastModified();
        }

        // ========== Registry ops ==========
        private void EnsureRegistryKeyExists()
        {
            try { using var _ = Registry.CurrentUser.CreateSubKey(REGISTRY_PATH); }
            catch (Exception ex) { Console.WriteLine($"Erreur création clé registry : {ex.Message}"); }
        }

        private string GetRegistryValue(string name, string def)
        {
            try
            {
                using var key = Registry.CurrentUser.OpenSubKey(REGISTRY_PATH);
                return key?.GetValue(name)?.ToString() ?? def;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Erreur lecture registry {name} : {ex.Message}");
                return def;
            }
        }

        private bool GetRegistryBoolValue(string name, bool def)
        {
            try
            {
                using var key = Registry.CurrentUser.OpenSubKey(REGISTRY_PATH);
                var s = key?.GetValue(name)?.ToString();
                return s == null ? def : s.Equals("true", StringComparison.OrdinalIgnoreCase);
            }
            catch { return def; }
        }

        private void SetRegistryValue(string name, string value)
        {
            try
            {
                using var key = Registry.CurrentUser.CreateSubKey(REGISTRY_PATH);
                key?.SetValue(name, value);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Erreur écriture registry {name} : {ex.Message}");
            }
        }

        private void UpdateLastModified()
            => SetRegistryValue("DerniereModification", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));

        private void DeleteAllSettings()
        {
            try
            {
                var keys = new[]
                {
                    "FormatPoids","FormatTaille","ThemeCouleur","FrequenceSauvegarde",
                    "ModeSombre","RappelsEntrainement","RappelsObjectifs","SauvegardeAuto",
                    "DerniereModification","IsLogin","LastAccountDeletion"
                };
                using var key = Registry.CurrentUser.CreateSubKey(REGISTRY_PATH);
                foreach (var k in keys) try { key?.DeleteValue(k, false); } catch { }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Erreur suppression settings registry : {ex.Message}");
            }
        }

        // ========== Validation ==========
        private bool IsValidWeightFormat(string f) => f == "KG" || f == "LBS";
        private bool IsValidTheme(string t)
        {
            var valid = new[] { "Violet", "Bleu", "Rose", "Vert" };
            foreach (var v in valid) if (v.Equals(t, StringComparison.OrdinalIgnoreCase)) return true;
            return false;
        }
        private bool IsValidSaveFrequency(string f)
            => f == "1 min" || f == "5 min" || f == "15 min" || f == "30 min";

        // ========== Business (stubs/appels services) ==========
        private void ConvertExistingWeightData(string _) { /* TODO */ }
        private void ApplyThemeToApplication(string theme) => ThemeManager.ApplyTheme(theme);
        private void ApplyDarkModeToApplication(bool dark) => ThemeManager.SetDarkMode(dark);
        private void ReconfigureAutoSaveTimer(string label)
        {
            var minutes = AutoSaveService.FrequencyLabelToMinutes(label);
            AutoSaveService.Instance.SetIntervalMinutes(minutes);
        }
        private void ConfigureWorkoutNotifications(bool _) { /* TODO */ }
        private void ConfigureGoalNotifications(bool _) { /* TODO */ }
        private void ConfigureAutoSave(bool enabled) => AutoSaveService.Instance.SetEnabled(enabled);
        private void DeleteAllUserData() { /* TODO */ }
        private void LogAccountDeletion()
            => SetRegistryValue("LastAccountDeletion", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));

        // ========== IsLogin (cohérent avec helpers) ==========
        public bool IsLogin
        {
            get => GetRegistryBoolValue("IsLogin", false);
            set => SetRegistryValue("IsLogin", value.ToString().ToLower());
        }
    }

    public class AppSettings
    {
        public string FormatPoids { get; set; } = "KG";
        public string FormatTaille { get; set; } = "CM";
        public string FrequenceSauvegarde { get; set; } = "5 min";
        public bool ModeSombre { get; set; } = true;
        public bool SauvegardeAuto { get; set; } = true;
    }
}