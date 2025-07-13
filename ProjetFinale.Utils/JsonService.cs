using ProjetFinale.Models;
using System;
using System.IO;
using System.Text.Json;

namespace ProjetFinale.Utils
{
    public static class JsonService
    {
        private const string UserFilePath = "utilisateur.json";

        public static void SauvegarderUtilisateur(Utilisateur utilisateur)
        {
            try
            {
                var json = JsonSerializer.Serialize(utilisateur, new JsonSerializerOptions { WriteIndented = true });
                File.WriteAllText(UserFilePath, json);
                Console.WriteLine("Utilisateur sauvegardé avec succès dans le fichier JSON.");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Erreur lors de la sauvegarde de l'utilisateur : {ex.Message}");
            }
        }

        public static Utilisateur? ChargerUtilisateur()
        {
            try
            {
                if (!File.Exists(UserFilePath))
                {
                    Console.WriteLine("Fichier JSON introuvable.");
                    return null;
                }

                string json = File.ReadAllText(UserFilePath);
                Console.WriteLine("Lecture JSON réussie. Contenu :\n" + json);

                var utilisateur = JsonSerializer.Deserialize<Utilisateur>(json);
                if (utilisateur == null)
                {
                    Console.WriteLine("La désérialisation a retourné null.");
                }
                else
                {
                    Console.WriteLine($"Utilisateur chargé : {utilisateur.Pseudo}");
                }

                return utilisateur;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Erreur lors du chargement de l'utilisateur : {ex.Message}");
                return null;
            }
        }
    }
}
