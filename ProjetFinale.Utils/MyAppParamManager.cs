using System;
using Microsoft.Win32;

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

        public string FormatHeure
        {
            get => GetRegistryValue("FormatHeure", "24H");
            set => SetRegistryValue("FormatHeure", value);
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
                FormatHeure = this.FormatHeure,
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

                // Logique métier : convertir les données existantes si nécessaire
                ConvertExistingWeightData(format);
            }
        }

        public void UpdateFormatHeure(string format)
        {
            if (IsValidTimeFormat(format))
            {
                FormatHeure = format;
                UpdateLastModified();

                // Logique métier : appliquer le nouveau format d'heure dans l'app
                ApplyTimeFormatToApp(format);
            }
        }

        public void UpdateTheme(string theme)
        {
            if (IsValidTheme(theme))
            {
                ThemeCouleur = theme;
                UpdateLastModified();

                // Logique métier : appliquer le thème à l'application
                ApplyThemeToApplication(theme);
            }
        }

        public void UpdateSaveFrequency(string frequency)
        {
            if (IsValidSaveFrequency(frequency))
            {
                FrequenceSauvegarde = frequency;
                UpdateLastModified();

                // Logique métier : reconfigurer l'auto-save timer
                ReconfigureAutoSaveTimer(frequency);
            }
        }

        public void UpdateDarkMode(bool isDarkMode)
        {
            ModeSombre = isDarkMode;
            UpdateLastModified();

            // Logique métier : appliquer le mode sombre
            ApplyDarkModeToApplication(isDarkMode);
        }

        public void UpdateWorkoutReminders(bool isEnabled)
        {
            RappelsEntrainement = isEnabled;
            UpdateLastModified();

            // Logique métier : configurer les notifications d'entraînement
            ConfigureWorkoutNotifications(isEnabled);
        }

        public void UpdateGoalReminders(bool isEnabled)
        {
            RappelsObjectifs = isEnabled;
            UpdateLastModified();

            // Logique métier : configurer les notifications d'objectifs
            ConfigureGoalNotifications(isEnabled);
        }

        public void UpdateAutoSave(bool isEnabled)
        {
            SauvegardeAuto = isEnabled;
            UpdateLastModified();

            // Logique métier : activer/désactiver l'auto-save
            ConfigureAutoSave(isEnabled);
        }

        public void DeleteAccount()
        {
            try
            {
                // Logique métier : supprimer toutes les données utilisateur
                DeleteAllUserData();
                DeleteAllSettings();

                // Log de l'action
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
            FormatHeure = "24H";
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
                // Key is created if it doesn't exist
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
                return string.Equals(value, "true", StringComparison.OrdinalIgnoreCase);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Erreur lecture registry bool {valueName} : {ex.Message}");
                return defaultValue;
            }
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
                // Supprimer toutes les valeurs settings mais garder IsLogin
                var settingsToDelete = new[]
                {
                    "FormatPoids", "FormatHeure", "ThemeCouleur", "FrequenceSauvegarde",
                    "ModeSombre", "RappelsEntrainement", "RappelsObjectifs", "SauvegardeAuto",
                    "DerniereModification"
                };

                using var key = Registry.CurrentUser.CreateSubKey(REGISTRY_PATH);
                if (key != null)
                {
                    foreach (var setting in settingsToDelete)
                    {
                        try
                        {
                            key.DeleteValue(setting, false); // false = no exception if not found
                        }
                        catch
                        {
                            // Ignore individual deletion errors
                        }
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

        private bool IsValidWeightFormat(string format)
        {
            return format == "KG" || format == "LBS";
        }

        private bool IsValidTimeFormat(string format)
        {
            return format == "24H" || format == "12H";
        }

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
            // Logique pour convertir toutes les données de poids existantes
            if (newFormat == "LBS")
            {
                // Convertir de KG vers LBS (multiplier par 2.20462)
                // Ici tu appellerais ton service de données pour convertir
                // DataService.ConvertWeightData("KG", "LBS");
            }
            else if (newFormat == "KG")
            {
                // Convertir de LBS vers KG (diviser par 2.20462)
                // DataService.ConvertWeightData("LBS", "KG");
            }
        }

        private void ApplyTimeFormatToApp(string format)
        {
            // Logique pour changer le format d'heure dans toute l'app
            // Par exemple, mettre à jour les culture settings
            // CultureManager.SetTimeFormat(format);
        }

        private void ApplyThemeToApplication(string theme)
        {
            // Logique pour appliquer le thème à toute l'application
            // ThemeManager.ApplyTheme(theme);
        }

        private void ApplyDarkModeToApplication(bool isDarkMode)
        {
            // Logique pour appliquer le mode sombre
            // ThemeManager.SetDarkMode(isDarkMode);
        }

        private void ReconfigureAutoSaveTimer(string frequency)
        {
            // Logique pour reconfigurer le timer d'auto-save
            var intervalMinutes = frequency switch
            {
                "1 min" => 1,
                "5 min" => 5,
                "15 min" => 15,
                "30 min" => 30,
                _ => 5
            };

            // AutoSaveService.SetInterval(intervalMinutes);
        }

        private void ConfigureWorkoutNotifications(bool isEnabled)
        {
            // Logique pour configurer les notifications d'entraînement
            // NotificationService.EnableWorkoutReminders(isEnabled);
        }

        private void ConfigureGoalNotifications(bool isEnabled)
        {
            // Logique pour configurer les notifications d'objectifs
            // NotificationService.EnableGoalReminders(isEnabled);
        }

        private void ConfigureAutoSave(bool isEnabled)
        {
            // Logique pour activer/désactiver l'auto-save
            // AutoSaveService.Enable(isEnabled);
        }

        private void DeleteAllUserData()
        {
            // Logique pour supprimer toutes les données utilisateur
            // Tu peux appeler tes autres services ici
            // UserDataService.DeleteAllData();
            // FileService.DeleteUserFiles();
        }

        private void LogAccountDeletion()
        {
            // Log de la suppression du compte pour audit
            var logEntry = $"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}] Account deleted";
            try
            {
                // Tu peux logger dans le registry ou un fichier
                SetRegistryValue("LastAccountDeletion", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));
            }
            catch
            {
                // Ignore les erreurs de log
            }
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
        public string FormatHeure { get; set; } = "24H";
        public string ThemeCouleur { get; set; } = "Violet";
        public string FrequenceSauvegarde { get; set; } = "5 min";
        public bool ModeSombre { get; set; } = true;
        public bool RappelsEntrainement { get; set; } = true;
        public bool RappelsObjectifs { get; set; } = false;
        public bool SauvegardeAuto { get; set; } = true;

        public AppSettings()
        {
            // Paramètres par défaut
        }
    }
}