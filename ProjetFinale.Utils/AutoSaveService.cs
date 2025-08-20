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
    public sealed class AutoSaveService
    {
        private const string RegistryPath = @"Software\ProjetFinale";
        private const string RegistryKeySaveAutoEnable = "SauvegardeAuto";
        private const string RegistryKeyFrequency = "FrequenceSauvegarde";

        // 🟣 DEV: Datafile/AutoSave sous le projet (on remonte depuis bin\Debug\...)
        private static readonly string AutoSaveFolder =
            Path.Combine(
                Path.GetFullPath(Path.Combine(AppContext.BaseDirectory, @"..\..\..")),
                "Datafile",
                "AutoSave"
            );

        private static readonly Lazy<AutoSaveService> _lazy = new(() => new AutoSaveService());
        public static AutoSaveService Instance => _lazy.Value;

        private readonly System.Timers.Timer _timer;
        private readonly SemaphoreSlim _snapshotMutex = new(1, 1);
        private bool _isEnabled;
        private TimeSpan _interval = TimeSpan.FromMinutes(5);

        private AutoSaveService()
        {
            _timer = new System.Timers.Timer(_interval.TotalMilliseconds)
            {
                AutoReset = true,
                Enabled = false
            };

            _timer.Elapsed += async (_, __) =>
            {
                await CreateSnapshotAsync().ConfigureAwait(false);
            };
        }

        // -------- API publique --------
        public void SetEnabled(bool enabled)
        {
            _isEnabled = enabled;
            ApplyTimerState();
        }

        public void SetIntervalMinutes(int minutes)
        {
            if (minutes <= 0) minutes = 1;
            _interval = TimeSpan.FromMinutes(minutes);
            _timer.Interval = _interval.TotalMilliseconds;
            ApplyTimerState();
        }

        public void Configure(bool enabled, TimeSpan interval)
        {
            _isEnabled = enabled;
            _interval = interval <= TimeSpan.Zero ? TimeSpan.FromMinutes(1) : interval;
            _timer.Interval = _interval.TotalMilliseconds;
            ApplyTimerState();
        }

        public void ConfigureFromRegistry()
        {
            bool enabled = ReadRegistryBool(RegistryPath, RegistryKeySaveAutoEnable, defaultValue: true);
            string freqLabel = ReadRegistryString(RegistryPath, RegistryKeyFrequency, defaultValue: "5 min");
            int minutes = FrequencyLabelToMinutes(freqLabel);
            Configure(enabled, TimeSpan.FromMinutes(minutes));
        }

        public void StartIfEnabled() => ApplyTimerState();

        public async Task HandleAppClosingAsync()
        {
            ConfigureFromRegistry();
            if (!_isEnabled) return;

            _timer.Stop();
            await CreateSnapshotAsync().ConfigureAwait(false);
        }

        // -------- Snapshots --------
        public async Task<string> CreateSnapshotAsync()
        {
            if (!await _snapshotMutex.WaitAsync(0).ConfigureAwait(false))
                return string.Empty;

            try
            {
                var state = AppStateAggregator.GetAllData();

                var jsonOptions = new JsonSerializerOptions
                {
                    WriteIndented = true,
                    DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
                    ReferenceHandler = ReferenceHandler.IgnoreCycles
                };

                string json = JsonSerializer.Serialize(state, jsonOptions);

                Directory.CreateDirectory(AutoSaveFolder);
                string fileName = $"autosave_{DateTime.Now:yyyy-MM-dd_HH-mm-ss}.json";
                string fullPath = Path.Combine(AutoSaveFolder, fileName);

                await File.WriteAllTextAsync(fullPath, json).ConfigureAwait(false);
                return fullPath;
            }
            catch
            {
                return string.Empty;
            }
            finally
            {
                _snapshotMutex.Release();
            }
        }

        // -------- Helpers --------
        private void ApplyTimerState()
        {
            _timer.Stop();
            if (_isEnabled) _timer.Start();
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
            catch { return defaultValue; }
        }

        private static bool ReadRegistryBool(string path, string name, bool defaultValue)
        {
            try
            {
                using var key = Registry.CurrentUser.OpenSubKey(path);
                var s = key?.GetValue(name)?.ToString();
                return s == null ? defaultValue : s.Equals("true", StringComparison.OrdinalIgnoreCase);
            }
            catch { return defaultValue; }
        }

    }

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