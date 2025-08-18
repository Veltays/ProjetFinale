using System;
using Microsoft.Win32;
using ProjetFinale.Utils;

namespace ProjetFinale.WPF
{
    /// <summary>
    /// Gestionnaire des paramètres - UNIQUEMENT logique métier
    /// Utilise le Registry comme MyAppParamManager
    /// </summary>
    public class SettingsManager
    {
        private const string REGISTRY_PATH = @"Software\ProjetFinale";

        public SettingsManager()
        {
            // Ensure registry key exists
            EnsureRegistryKeyExists();
        }

        #region Registry Properties

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

        #endregion

        #region Public Methods

        public AppSettings GetCurrentSettings()
        {
            return new AppSettings
            {
                FormatPoids = this.FormatPoids,
                FormatTaille = this.FormatTaille,
                ThemeCouleur = this.ThemeCouleur,
                FrequenceSauvegarde = this.FrequenceSauvegarde,
                ModeSombre = this.ModeSombre,
                RappelsEntrainement = this.RappelsEntrainement,
                RappelsObjectifs = this.RappelsObjectifs,
                SauvegardeAuto = this.SauvegardeAuto
            };
        }

        public void UpdateFormatPoids(string format)
        {
            if (IsValidWeightFormat(format))
            {
                FormatPoids = format;
                UpdateLastModified();

                ConvertExistingWeightData(format);
            }
        }

        public void UpdateFormatTaille(string format)  // "CM" ou "INCH"
        {
            if (format == "CM" || format == "INCH")
            {
                FormatTaille = format;
                UpdateLastModified();

                // TODO conversions éventuelles
            }
        }

        public void UpdateTheme(string theme)
        {
            if (IsValidTheme(theme))
            {
                ThemeCouleur = theme;
                UpdateLastModified();
                ApplyThemeToApplication(theme);
            }
        }

        public void UpdateSaveFrequency(string frequency)
        {
            if (IsValidSaveFrequency(frequency))
            {
                FrequenceSauvegarde = frequency;
                UpdateLastModified();

                // Reconfigurer le timer d'auto-save
                ReconfigureAutoSaveTimer(frequency);
            }
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

            // Activer/Désactiver l'auto-save
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
            ThemeCouleur = "Violet";
            FrequenceSauvegarde = "5 min";
            ModeSombre = true;
            RappelsEntrainement = true;
            RappelsObjectifs = false;
            SauvegardeAuto = true;
            UpdateLastModified();
        }

        #endregion

        #region Private Methods - Registry Operations

        private void EnsureRegistryKeyExists()
        {
            try
            {
                using var key = Registry.CurrentUser.CreateSubKey(REGISTRY_PATH);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Erreur création clé registry : {ex.Message}");
            }
        }

        private string GetRegistryValue(string valueName, string defaultValue)
        {
            try
            {
                using var key = Registry.CurrentUser.OpenSubKey(REGISTRY_PATH);
                return key?.GetValue(valueName)?.ToString() ?? defaultValue;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Erreur lecture registry {valueName} : {ex.Message}");
                return defaultValue;
            }
        }

        private bool GetRegistryBoolValue(string valueName, bool defaultValue)
        {
            try
            {
                using var key = Registry.CurrentUser.OpenSubKey(REGISTRY_PATH);
                string value = key?.GetValue(valueName)?.ToString();
                return value == null ? defaultValue
                                     : string.Equals(value, "true", StringComparison.OrdinalIgnoreCase);
            }
            catch { return defaultValue; }
        }

        private void SetRegistryValue(string valueName, string value)
        {
            try
            {
                using var key = Registry.CurrentUser.CreateSubKey(REGISTRY_PATH);
                key?.SetValue(valueName, value);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Erreur écriture registry {valueName} : {ex.Message}");
            }
        }

        private void UpdateLastModified()
        {
            SetRegistryValue("DerniereModification", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));
        }

        private void DeleteAllSettings()
        {
            try
            {
                var settingsToDelete = new[]
                {
                    "FormatPoids","FormatTaille", "ThemeCouleur", "FrequenceSauvegarde",
                    "ModeSombre", "RappelsEntrainement", "RappelsObjectifs", "SauvegardeAuto",
                    "DerniereModification"
                };

                using var key = Registry.CurrentUser.CreateSubKey(REGISTRY_PATH);
                if (key != null)
                {
                    foreach (var setting in settingsToDelete)
                    {
                        try { key.DeleteValue(setting, false); }
                        catch { /* ignore */ }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Erreur suppression settings registry : {ex.Message}");
            }
        }

        #endregion

        #region Private Methods - Validation

        private bool IsValidWeightFormat(string format) => format == "KG" || format == "LBS";

        private bool IsValidTheme(string theme)
        {
            var validThemes = new[] { "Violet", "Bleu", "Rose", "Vert" };
            return Array.Exists(validThemes, t => t.Equals(theme, StringComparison.OrdinalIgnoreCase));
        }

        private bool IsValidSaveFrequency(string frequency)
        {
            var validFrequencies = new[] { "1 min", "5 min", "15 min", "30 min" };
            return Array.Exists(validFrequencies, f => f == frequency);
        }

        #endregion

        #region Private Methods - Business Logic

        private void ConvertExistingWeightData(string newFormat)
        {
            // TODO: Conversion de données existantes si nécessaire
        }

        private void ApplyThemeToApplication(string theme) => ThemeManager.ApplyTheme(theme);

        private void ApplyDarkModeToApplication(bool isDarkMode) => ThemeManager.SetDarkMode(isDarkMode);

        private void ReconfigureAutoSaveTimer(string frequency)
        {
            var minutes = Utils.AutoSaveService.MapFrequencyToMinutes(frequency);
            AutoSaveService.Instance.SetInterval(minutes);
        }

        private void ConfigureWorkoutNotifications(bool isEnabled)
        {
            // TODO notifications entraînement
        }

        private void ConfigureGoalNotifications(bool isEnabled)
        {
            // TODO notifications objectifs
        }

        private void ConfigureAutoSave(bool isEnabled)
        {
            AutoSaveService.Instance.Enable(isEnabled);
        }

        private void DeleteAllUserData()
        {
            // TODO suppression données utilisateur
        }

        private void LogAccountDeletion()
        {
            var logEntry = $"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}] Account deleted";
            try
            {
                SetRegistryValue("LastAccountDeletion", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));
            }
            catch { }
        }

        public bool IsLogin
        {
            get
            {
                using var key = Registry.CurrentUser.OpenSubKey(@"Software\ProjetFinale");
                string value = key?.GetValue("IsLogin")?.ToString();
                return string.Equals(value, "true", StringComparison.OrdinalIgnoreCase);
            }
            set
            {
                using var key = Registry.CurrentUser.CreateSubKey(@"Software\ProjetFinale");
                key?.SetValue("IsLogin", value.ToString().ToLower());
            }
        }

        #endregion
    }

    /// <summary>
    /// Modèle des paramètres de l'application (pour la compatibilité)
    /// </summary>
    public class AppSettings
    {
        public string FormatPoids { get; set; } = "KG";
        public string FormatTaille { get; set; } = "CM";
        public string ThemeCouleur { get; set; } = "Violet";
        public string FrequenceSauvegarde { get; set; } = "5 min";
        public bool ModeSombre { get; set; } = true;
        public bool RappelsEntrainement { get; set; } = true;
        public bool RappelsObjectifs { get; set; } = false;
        public bool SauvegardeAuto { get; set; } = true;
    }
}
