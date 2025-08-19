using ProjetFinale.Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Reflection;
using System.Text.Json;

namespace ProjetFinale.Utils
{
    public static class JsonService
    {
        // === Chemins ===
        private static readonly string ProjectRoot = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location)!;
        private static readonly string DataFolder = Path.Combine(Path.GetFullPath(Path.Combine(ProjectRoot, @"..\..\..")), "Datafile");

        private static readonly string UserFile = Path.Combine(DataFolder, "utilisateur.json");
        private static readonly string TasksFile = Path.Combine(DataFolder, "taches.json");
        private static readonly string ActivitiesFile = Path.Combine(DataFolder, "activites.json");

        // === Options JSON ===
        private static readonly JsonSerializerOptions JsonOpts = new() { WriteIndented = true };

        static JsonService()
        {
            if (!Directory.Exists(DataFolder)) Directory.CreateDirectory(DataFolder);
        }

        // === Helpers génériques ===
        private static void WriteJson<T>(string path, T data)
        {
            try
            {
                Directory.CreateDirectory(Path.GetDirectoryName(path)!);
                File.WriteAllText(path, JsonSerializer.Serialize(data, JsonOpts));
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Écriture JSON ({Path.GetFileName(path)}): {ex.Message}");
            }
        }

        private static T? ReadJson<T>(string path)
        {
            try
            {
                if (!File.Exists(path)) return default;
                return JsonSerializer.Deserialize<T>(File.ReadAllText(path));
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Lecture JSON ({Path.GetFileName(path)}): {ex.Message}");
                return default;
            }
        }

        // === Utilisateur ===
        public static void SauvegarderUtilisateur(Utilisateur utilisateur)
            => WriteJson(UserFile, utilisateur);

        public static Utilisateur? ChargerUtilisateur()
        {
            var u = ReadJson<Utilisateur>(UserFile);
            if (u == null) return null;

            // Assurer des collections non null
            u.ListeTaches ??= new ObservableCollection<Tache>();
            u.ListeAgenda ??= new ObservableCollection<Agenda>();
            u.ListeActivites ??= new List<Activite>();
            return u;
        }

        // === Tâches ===
        public static void SauvegarderTaches(ObservableCollection<Tache> taches)
            => WriteJson(TasksFile, taches);

        public static ObservableCollection<Tache> ChargerTaches()
        {
            var list = ReadJson<List<Tache>>(TasksFile) ?? new List<Tache>();
            return new ObservableCollection<Tache>(list);
        }

        // === Activités ===
        public static void SauvegarderActivites(List<Activite> activites)
            => WriteJson(ActivitiesFile, activites);

        public static List<Activite> ChargerActivites()
            => ReadJson<List<Activite>>(ActivitiesFile) ?? new List<Activite>();

        // === Utilitaires ===
        public static bool FichierUtilisateurExiste() => File.Exists(UserFile);

        public static void SupprimerFichierUtilisateur()
        {
            try
            {
                if (File.Exists(UserFile)) File.Delete(UserFile);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Suppression utilisateur.json : {ex.Message}");
            }
        }
    }
}
