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
        private static readonly string ProjectRootPath = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
        private static readonly string DataFolderPath = Path.Combine(Path.GetFullPath(Path.Combine(ProjectRootPath, @"..\..\..")), "Datafile");

        private static readonly string UserFilePath = Path.Combine(DataFolderPath, "utilisateur.json");
        private static readonly string TachesFilePath = Path.Combine(DataFolderPath, "taches.json");
        private static readonly string ActivitesFilePath = Path.Combine(DataFolderPath, "activites.json");

        static JsonService()
        {
            // Créer le dossier Datafile s'il n'existe pas
            if (!Directory.Exists(DataFolderPath))
            {
                Directory.CreateDirectory(DataFolderPath);
                Console.WriteLine($"📁 Dossier Datafile créé à : {DataFolderPath}");
            }
        }

        // === GESTION UTILISATEUR ===
        public static void SauvegarderUtilisateur(Utilisateur utilisateur)
        {
            try
            {
                var json = JsonSerializer.Serialize(utilisateur, new JsonSerializerOptions { WriteIndented = true });
                File.WriteAllText(UserFilePath, json);
                Console.WriteLine($"✅ Utilisateur {utilisateur.Pseudo} sauvegardé.");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Erreur sauvegarde utilisateur : {ex.Message}");
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

                // ✅ IMPORTANT : S'assurer que les ObservableCollection sont initialisées
                if (utilisateur != null)
                {
                    if (utilisateur.ListeTaches == null)
                        utilisateur.ListeTaches = new ObservableCollection<Tache>();

                    if (utilisateur.ListeAgenda == null)
                        utilisateur.ListeAgenda = new ObservableCollection<Agenda>();

                    if (utilisateur.ListeActivites == null)
                        utilisateur.ListeActivites = new List<Activite>();

                    if (utilisateur.ListeStatistiques == null)
                        utilisateur.ListeStatistiques = new List<Statistique>();
                }

                Console.WriteLine(utilisateur != null ? $"✅ Utilisateur chargé : {utilisateur.Pseudo}" : "⚠️ Utilisateur null.");
                return utilisateur;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Erreur chargement utilisateur : {ex.Message}");
                return null;
            }
        }

        // === GESTION TÂCHES (maintenant avec ObservableCollection) ===
        public static void SauvegarderTaches(ObservableCollection<Tache> taches)
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

        public static ObservableCollection<Tache> ChargerTaches()
        {
            try
            {
                if (!File.Exists(TachesFilePath))
                    return new ObservableCollection<Tache>();

                var json = File.ReadAllText(TachesFilePath);

                // Désérialiser en List d'abord (format JSON standard)
                var tachesList = JsonSerializer.Deserialize<List<Tache>>(json);

                // Convertir en ObservableCollection
                return new ObservableCollection<Tache>(tachesList ?? new List<Tache>());
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Erreur chargement des tâches : {ex.Message}");
                return new ObservableCollection<Tache>();
            }
        }

        // === GESTION ACTIVITÉS (inchangé) ===
        public static void SauvegarderActivites(List<Activite> activites)
        {
            try
            {
                var json = JsonSerializer.Serialize(activites, new JsonSerializerOptions { WriteIndented = true });
                File.WriteAllText(ActivitesFilePath, json);
                Console.WriteLine("✅ Activités sauvegardées dans Datafile/activites.json");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Erreur sauvegarde des activités : {ex.Message}");
            }
        }

        public static List<Activite> ChargerActivites()
        {
            try
            {
                if (!File.Exists(ActivitesFilePath))
                    return new List<Activite>();

                var json = File.ReadAllText(ActivitesFilePath);
                var activites = JsonSerializer.Deserialize<List<Activite>>(json);
                return activites ?? new List<Activite>();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Erreur chargement des activités : {ex.Message}");
                return new List<Activite>();
            }
        }

        // === MÉTHODES UTILITAIRES ===
        public static bool FichierUtilisateurExiste()
        {
            return File.Exists(UserFilePath);
        }

        public static void SupprimerFichierUtilisateur()
        {
            try
            {
                if (File.Exists(UserFilePath))
                {
                    File.Delete(UserFilePath);
                    Console.WriteLine("🗑️ Fichier utilisateur supprimé.");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Erreur suppression fichier utilisateur : {ex.Message}");
            }
        }
    }
}