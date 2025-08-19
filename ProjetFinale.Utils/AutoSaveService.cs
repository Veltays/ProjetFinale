using System;
using System.IO;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Win32;
using ProjetFinale.Services; // UserService

namespace ProjetFinale.Utils
{
    /// <summary>
    /// AutoSaveService = service d’auto-sauvegarde (singleton).
    /// - Lance un minuteur (timer) qui crée périodiquement un "snapshot" JSON.
    /// - À la fermeture de l’app, crée un dernier snapshot si activé.
    /// - Lit sa configuration (activé + fréquence) dans la Registry.
    /// </summary>
    public sealed class AutoSaveService
    {
        // --------- Constantes & chemins ---------
        private const string RegistryPath = @"Software\ProjetFinale";
        private const string RegistryKeyEnabled = "SauvegardeAuto";
        private const string RegistryKeyFrequency = "FrequenceSauvegarde"; // "1 min" | "5 min" | "15 min" | "30 min"

        private static readonly string AutoSaveFolder =
            Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "ProjetFitness");
        // // Chemin du dossier où seront stockés les snapshots JSON
        // --> "Documents\ProjetFitness\AutoSaves"

        // --------- Singleton ---------
        private static readonly Lazy<AutoSaveService> _lazy = new(() => new AutoSaveService());
        public static AutoSaveService Instance => _lazy.Value;

        // --------- État interne ---------
        private readonly System.Timers.Timer _timer;
        private readonly SemaphoreSlim _snapshotMutex = new(1, 1); // empêche 2 sauvegardes en parallèle
        private bool _isEnabled;
        private TimeSpan _interval = TimeSpan.FromMinutes(5);

        // --------- Ctor privé (singleton) ---------
        private AutoSaveService()
        {
            _timer = new System.Timers.Timer(_interval.TotalMilliseconds)
            {
                AutoReset = true, // relance automatiquement après chaque tick
                Enabled = false   // on ne démarre pas tant qu’on n’a pas lu la config
            };

            _timer.Elapsed += async (_, __) =>
            {
                // Un tick = on tente un snapshot
                await CreateSnapshotAsync().ConfigureAwait(false);
            };
        }

        // =========================================================
        // API publique : configuration & cycle de vie
        // =========================================================

        /// <summary>Active ou désactive l’auto-sauvegarde.</summary>
        public void SetEnabled(bool enabled)
        {
            _isEnabled = enabled;
            ApplyTimerState();
        }

        /// <summary>Change la fréquence (en minutes, minimum 1).</summary>
        public void SetIntervalMinutes(int minutes)
        {
            if (minutes <= 0) minutes = 1;
            _interval = TimeSpan.FromMinutes(minutes);
            _timer.Interval = _interval.TotalMilliseconds;
            ApplyTimerState();
        }

        /// <summary>Configure entièrement (activé + intervalle).</summary>
        public void Configure(bool enabled, TimeSpan interval)
        {
            _isEnabled = enabled;
            _interval = interval <= TimeSpan.Zero ? TimeSpan.FromMinutes(1) : interval;
            _timer.Interval = _interval.TotalMilliseconds;
            ApplyTimerState();
        }

        /// <summary>Lit la configuration depuis la Registry et l’applique.</summary>
        public void ConfigureFromRegistry()
        {
            bool enabled = ReadRegistryBool(RegistryPath, RegistryKeyEnabled, defaultValue: true);
            string freqLabel = ReadRegistryString(RegistryPath, RegistryKeyFrequency, defaultValue: "5 min");
            int minutes = FrequencyLabelToMinutes(freqLabel);

            Configure(enabled, TimeSpan.FromMinutes(minutes));
        }

        /// <summary>
        /// À appeler au démarrage (après ConfigureFromRegistry) si tu veux démarrer explicitement.
        /// Utile si tu préfères ne pas démarrer dans Configure().
        /// </summary>
        public void StartIfEnabled() => ApplyTimerState();

        /// <summary>
        /// À appeler à la fermeture de l’application.
        /// Si l’autosave est activée, on arrête le timer et on force un dernier snapshot.
        /// </summary>
        public async Task HandleAppClosingAsync()
        {
            // Recharge la config au cas où
            ConfigureFromRegistry();

            if (!_isEnabled) return;

            _timer.Stop(); // évite un tick concurrent pendant la fermeture
            await CreateSnapshotAsync().ConfigureAwait(false);
        }

        // =========================================================
        // Snapshot : écrit un JSON horodaté sur disque (dossier AutoSaves)
        // =========================================================

        /// <summary>
        /// Crée un snapshot JSON (retourne le chemin du fichier). Retourne "" si déjà en cours ou si erreur.
        /// </summary>
        public async Task<string> CreateSnapshotAsync()
        {
            // Empêche deux snapshots en parallèle
            if (!await _snapshotMutex.WaitAsync(0).ConfigureAwait(false))
                return string.Empty;

            try
            {
                // 1) Récupère l’état courant (ce que tu veux inclure dans l’export)
                var state = AppStateAggregator.GetAllData();

                // 2) Options JSON lisibles + robustes
                var jsonOptions = new JsonSerializerOptions
                {
                    WriteIndented = true,
                    DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
                    ReferenceHandler = ReferenceHandler.IgnoreCycles
                };

                string json = JsonSerializer.Serialize(state, jsonOptions);

                // 3) Prépare le dossier + nom de fichier
                Directory.CreateDirectory(AutoSaveFolder);
                string fileName = $"autosave_{DateTime.Now:yyyy-MM-dd_HH-mm-ss}.json";
                string fullPath = Path.Combine(AutoSaveFolder, fileName);

                // 4) Écrit le fichier
                await File.WriteAllTextAsync(fullPath, json).ConfigureAwait(false);
                return fullPath;
            }
            catch
            {
                // Ici tu peux logguer si tu as un logger central
                return string.Empty;
            }
            finally
            {
                _snapshotMutex.Release();
            }
        }

        // =========================================================
        // Helpers privés
        // =========================================================

        private void ApplyTimerState()
        {
            _timer.Stop();
            if (_isEnabled)
                _timer.Start();
        }

        internal static int FrequencyLabelToMinutes(string label) => label switch
        {
            "1 min" => 1,
            "5 min" => 5,
            "15 min" => 15,
            "30 min" => 30,
            _ => 5
        };

        private static string ReadRegistryString(string path, string name, string defaultValue)
        {
            try
            {
                using var key = Registry.CurrentUser.OpenSubKey(path);
                return key?.GetValue(name)?.ToString() ?? defaultValue;
            }
            catch
            {
                return defaultValue;
            }
        }

        private static bool ReadRegistryBool(string path, string name, bool defaultValue)
        {
            try
            {
                using var key = Registry.CurrentUser.OpenSubKey(path);
                var s = key?.GetValue(name)?.ToString();
                return s == null ? defaultValue : s.Equals("true", StringComparison.OrdinalIgnoreCase);
            }
            catch
            {
                return defaultValue;
            }
        }
    }

    /// <summary>
    /// Point central pour agréger l’état à sauvegarder.
    /// Étends cet objet quand tu veux inclure plus de données.
    /// </summary>
    public static class AppStateAggregator
    {
        public static object GetAllData()
        {
            var user = UserService.UtilisateurActif;

            return new
            {
                generatedAt = DateTime.Now,
                user
            };
        }
    }
}