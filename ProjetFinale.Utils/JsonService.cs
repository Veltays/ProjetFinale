using ProjetFinale.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;

namespace ProjetFinale.Utils
{
    public static class JsonService
    {
        private static readonly string BaseDirectory = Path.GetFullPath(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, @"..\..\..\Datafile"));
        private static readonly string UserFilePath = Path.Combine(BaseDirectory, "utilisateur.json");
        private static readonly string TachesFilePath = Path.Combine(BaseDirectory, "taches.json");

        static JsonService()
        {
            // Créer le dossier s’il n’existe pas
            if (!Directory.Exists(BaseDirectory))
            {
                Directory.CreateDirectory(BaseDirectory);
            }
        }

        public static void SauvegarderUtilisateur(Utilisateur utilisateur)
        {
            try
            {
                var json = JsonSerializer.Serialize(utilisateur, new JsonSerializerOptions { WriteIndented = true });
                File.WriteAllText(UserFilePath, json);
                Console.WriteLine("✅ Utilisateur sauvegardé dans Datafile/utilisateur.json");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Erreur lors de la sauvegarde de l'utilisateur : {ex.Message}");
            }
        }

        public static Utilisateur? ChargerUtilisateur()
        {
            try
            {
                if (!File.Exists(UserFilePath))
                {
                    Console.WriteLine("ℹ️ Fichier utilisateur introuvable.");
                    return null;
                }

                var json = File.ReadAllText(UserFilePath);
                var utilisateur = JsonSerializer.Deserialize<Utilisateur>(json);
                Console.WriteLine(utilisateur != null ? $"✅ Utilisateur chargé : {utilisateur.Pseudo}" : "⚠️ Utilisateur null.");
                return utilisateur;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Erreur chargement utilisateur : {ex.Message}");
                return null;
            }
        }

        public static void SauvegarderTaches(List<Tache> taches)
        {
            try
            {
                var json = JsonSerializer.Serialize(taches, new JsonSerializerOptions { WriteIndented = true });
                File.WriteAllText(TachesFilePath, json);
                Console.WriteLine("✅ Tâches sauvegardées dans Datafile/taches.json");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Erreur sauvegarde des tâches : {ex.Message}");
            }
        }

        public static List<Tache> ChargerTaches()
        {
            try
            {
                if (!File.Exists(TachesFilePath)) return new List<Tache>();
                var json = File.ReadAllText(TachesFilePath);
                var taches = JsonSerializer.Deserialize<List<Tache>>(json);
                return taches ?? new List<Tache>();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Erreur chargement des tâches : {ex.Message}");
                return new List<Tache>();
            }
        }
    }
}
