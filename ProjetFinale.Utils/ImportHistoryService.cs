using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;

namespace ProjetFinale.Utils
{
    public record ImportHistoryEntry(string Format, string FileName, long FileSize, DateTime ImportedAt);

    public static class ImportHistoryService
    {
        private const int MaxItems = 5;

        // 🔹 Dossier Datafile dans ton projet (pendant le dev)
        private static readonly string DataFolder =
            Path.Combine(Path.GetFullPath(Path.Combine(AppContext.BaseDirectory, @"..\..\..")), "Datafile");

        private static readonly string FilePath =
            Path.Combine(DataFolder, "import_history.json");

        public static List<ImportHistoryEntry> Load()
        {
            try
            {
                if (!File.Exists(FilePath)) return new();
                var json = File.ReadAllText(FilePath);
                return JsonSerializer.Deserialize<List<ImportHistoryEntry>>(json) ?? new();
            }
            catch
            {
                return new();
            }
        }

        public static void Add(ImportHistoryEntry item)
        {
            var list = Load();
            list.Insert(0, item);
            if (list.Count > MaxItems)
                list.RemoveRange(MaxItems, list.Count - MaxItems);

            try
            {
                Directory.CreateDirectory(DataFolder);
                var json = JsonSerializer.Serialize(list, new JsonSerializerOptions { WriteIndented = true });
                File.WriteAllText(FilePath, json);
            }
            catch
            {
                // tu peux logguer ici si besoin
            }
        }
    }
}