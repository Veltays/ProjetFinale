using System;
using System.IO;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading;
using System.Threading.Tasks;
using System.Timers;
using Microsoft.Win32;
using ProjetFinale.Services;   // JsonService, UserService

// Alias pour éviter l'ambiguïté entre System.Timers.Timer et System.Threading.Timer
using STimer = System.Timers.Timer;

namespace ProjetFinale.Utils
{
    /// <summary>
    /// Service d’auto-sauvegarde JSON (singleton).
    /// Utilise System.Timers.Timer (hors UI) + SemaphoreSlim pour éviter le chevauchement.
    /// Aucune dépendance à WPF.
    /// </summary>
    public sealed class AutoSaveService
    {
        private static readonly Lazy<AutoSaveService> _lazy = new(() => new AutoSaveService());
        public static AutoSaveService Instance => _lazy.Value;

        private readonly STimer _timer;
        private readonly SemaphoreSlim _lock = new(1, 1);

        private bool _isEnabled;
        private TimeSpan _interval = TimeSpan.FromMinutes(5);

        private AutoSaveService()
        {
            _timer = new STimer(_interval.TotalMilliseconds)
            {
                AutoReset = true
            };
            _timer.Elapsed += async (_, __) => await OnTickAsync().ConfigureAwait(false);
        }

        /// <summary>Activer/Désactiver le service.</summary>
        public void Enable(bool enabled)
        {
            _isEnabled = enabled;
            ReapplyTimerState();
        }

        /// <summary>Changer l’intervalle (minutes).</summary>
        public void SetInterval(int minutes)
        {
            if (minutes <= 0) minutes = 1;
            _interval = TimeSpan.FromMinutes(minutes);
            _timer.Interval = _interval.TotalMilliseconds;
            ReapplyTimerState();
        }

        /// <summary>Configuration complète.</summary>
        public void Configure(bool enabled, TimeSpan interval)
        {
            _isEnabled = enabled;
            _interval = interval <= TimeSpan.Zero ? TimeSpan.FromMinutes(1) : interval;
            _timer.Interval = _interval.TotalMilliseconds;
            ReapplyTimerState();
        }

        /// <summary>
        /// (Re)lit la configuration depuis le Registry (sans dépendre de SettingsManager).
        /// </summary>
        public void ConfigureFromSettings()
        {
            bool enabled = GetRegistryBool(@"Software\ProjetFinale", "SauvegardeAuto", true);
            string freq = GetRegistryString(@"Software\ProjetFinale", "FrequenceSauvegarde", "5 min");
            var minutes = MapFrequencyToMinutes(freq);
            Configure(enabled, TimeSpan.FromMinutes(minutes));
        }

        /// <summary>Export complet en JSON (fichier horodaté). Retourne le chemin écrit.</summary>
        public async Task<string> ExportAllAsync()
        {
            // Empêche deux exports en parallèle
            if (!await _lock.WaitAsync(0).ConfigureAwait(false))
                return string.Empty;

            try
            {
                var snapshot = AppStateAggregator.GetAllData();

                var options = new JsonSerializerOptions
                {
                    WriteIndented = true,
                    DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
                    ReferenceHandler = ReferenceHandler.IgnoreCycles
                };

                string json = JsonSerializer.Serialize(snapshot, options);

                string baseDir = Path.Combine(
                    Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments),
                    "ProjetFitness",
                    "AutoSaves");

                Directory.CreateDirectory(baseDir);

                string fileName = $"autosave_{DateTime.Now:yyyy-MM-dd_HH-mm-ss}.json";
                string fullPath = Path.Combine(baseDir, fileName);

                await File.WriteAllTextAsync(fullPath, json).ConfigureAwait(false);
                return fullPath;
            }
            catch
            {
                return string.Empty; // TODO: logger si besoin
            }
            finally
            {
                _lock.Release();
            }
        }

        private async Task OnTickAsync()
        {
            _ = await ExportAllAsync().ConfigureAwait(false);
        }

        private void ReapplyTimerState()
        {
            _timer.Stop();
            if (_isEnabled)
                _timer.Start();
        }

        internal static int MapFrequencyToMinutes(string frequency) => frequency switch
        {
            "1 min" => 1,
            "5 min" => 5,
            "15 min" => 15,
            "30 min" => 30,
            _ => 5
        };

        private static string GetRegistryString(string path, string name, string def)
        {
            try
            {
                using var key = Registry.CurrentUser.OpenSubKey(path);
                return key?.GetValue(name)?.ToString() ?? def;
            }
            catch { return def; }
        }

        private static bool GetRegistryBool(string path, string name, bool def)
        {
            try
            {
                using var key = Registry.CurrentUser.OpenSubKey(path);
                var s = key?.GetValue(name)?.ToString();
                return s == null ? def : s.Equals("true", StringComparison.OrdinalIgnoreCase);
            }
            catch { return def; }
        }
    }

    /// <summary>
    /// Petit agrégateur d’état à étendre selon les besoins.
    /// </summary>
    public static class AppStateAggregator
    {
        public static object GetAllData()
        {
            var user = UserService.UtilisateurActif;

            // TODO: Ajouter ici d’autres données (agendas, exercices, objectifs, etc.)
            return new
            {
                generatedAt = DateTime.Now,
                user
            };
        }
    }
}
