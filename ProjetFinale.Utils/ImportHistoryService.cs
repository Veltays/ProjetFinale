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
        private static readonly string FilePath =
            Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
                         "ProjetFinale", "import_history.json");


        public static List<ImportHistoryEntry> Load()
        {
            try
            {
                if (!File.Exists(FilePath)) return new();
                var json = File.ReadAllText(FilePath);
                return JsonSerializer.Deserialize<List<ImportHistoryEntry>>(json) ?? new();
            }
            catch { return new(); }
        }

        public static void Add(ImportHistoryEntry item)
        {
            var list = Load();
            list.Insert(0, item);
            if (list.Count > MaxItems) list.RemoveRange(MaxItems, list.Count - MaxItems);

            try
            {
                Directory.CreateDirectory(Path.GetDirectoryName(FilePath)!);
                var json = JsonSerializer.Serialize(list, new JsonSerializerOptions { WriteIndented = true });
                File.WriteAllText(FilePath, json);
            }
            catch { /* noop/log si tu veux */ }
        }
    }
}