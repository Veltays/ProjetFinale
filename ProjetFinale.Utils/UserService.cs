using ProjetFinale.Models;
using ProjetFinale.Utils;
using System;
using System.Text.RegularExpressions;

namespace ProjetFinale.Services
{
    public class UserService
    {
        // Champ privé pour stocker l'utilisateur actif
        private static Utilisateur? _utilisateurActif;

        // 🔥 NOUVEAU : Événement déclenché quand l'utilisateur change
        public static event Action<Utilisateur?> UtilisateurActifChanged;

        // Propriété publique avec notification d'événement
        public static Utilisateur? UtilisateurActif
        {
            get => _utilisateurActif;
            set
            {
                if (_utilisateurActif != value)
                {
                    var ancienUtilisateur = _utilisateurActif;
                    _utilisateurActif = value;

                    // 🚀 Déclencher l'événement pour notifier les changements
                    UtilisateurActifChanged?.Invoke(_utilisateurActif);

                    // Logs pour le debug
                    if (_utilisateurActif != null)
                    {
                        Console.WriteLine($"🔄 UtilisateurActif changé : {_utilisateurActif.Pseudo} (Age: {_utilisateurActif.Age})");
                    }
                    else
                    {
                        Console.WriteLine("🔄 UtilisateurActif défini à null");
                    }
                }
            }
        }

        // 🆕 NOUVELLE MÉTHODE : Charger l'utilisateur depuis le fichier
        public static void ChargerUtilisateurDepuisFichier()
        {
            try
            {
                var utilisateur = JsonService.ChargerUtilisateur();
                if (utilisateur != null)
                {
                    UtilisateurActif = utilisateur; // Ceci déclenchera l'événement
                    Console.WriteLine($"✅ Utilisateur chargé depuis le fichier : {utilisateur.Pseudo}");
                }
                else
                {
                    Console.WriteLine("⚠️ Aucun utilisateur trouvé dans le fichier");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Erreur lors du chargement de l'utilisateur : {ex.Message}");
            }
        }

        // 🆕 NOUVELLE MÉTHODE : Forcer le rafraîchissement des interfaces
        public static void NotifierChangementUtilisateur()
        {
            if (_utilisateurActif != null)
            {
                Console.WriteLine($"🔔 Notification forcée pour : {_utilisateurActif.Pseudo}");
                UtilisateurActifChanged?.Invoke(_utilisateurActif);
            }
        }

        // === MÉTHODES DE VALIDATION (inchangées) ===

        public bool VerifierPseudo(string pseudo)
        {
            if (string.IsNullOrWhiteSpace(pseudo) || pseudo.Length < 3 || pseudo.Length > 20)
                return false;

            foreach (char c in pseudo)
            {
                if (!char.IsLetterOrDigit(c) && c != '_')
                    return false;
            }

            return true;
        }

        public bool VerifierNom(string nom)
        {
            return !string.IsNullOrWhiteSpace(nom);
        }

        public bool VerifierPrenom(string prenom)
        {
            return !string.IsNullOrWhiteSpace(prenom);
        }

        public bool VerifierTaille(int taille)
        {
            return taille > 0;
        }

        public bool VerifierPoids(double poids)
        {
            return poids > 0;
        }

        public bool VerifierAge(int age)
        {
            return age > 0;
        }

        public bool VerifierObjectifPoids(double objectif)
        {
            return objectif > 0;
        }

        public bool VerifierDateObjectif(DateTime date)
        {
            return date > DateTime.Now;
        }

        public bool VerifierMotDePasse(string mdp, string confirmation)
        {
            if (string.IsNullOrWhiteSpace(mdp) || mdp.Length < 6)
                return false;

            return mdp == confirmation;
        }

        public bool VerifierEmail(string email)
        {
            if (string.IsNullOrWhiteSpace(email)) return false;

            return Regex.IsMatch(email, @"^[^@\s]+@[^@\s]+\.[^@\s]+$");
        }

        // === CRÉATION D'UTILISATEUR (améliorée) ===

        public Utilisateur? CreerUtilisateur(string pseudo, string nom, string prenom, int taille,
                                             double poids, int age, double objectif, DateTime dateObjectif,
                                             string email, string motDePasse, string confirmation)
        {
            if (!VerifierPseudo(pseudo) || !VerifierNom(nom) || !VerifierPrenom(prenom) ||
                !VerifierTaille(taille) || !VerifierPoids(poids) || !VerifierAge(age) ||
                !VerifierObjectifPoids(objectif) || !VerifierDateObjectif(dateObjectif) ||
                !VerifierEmail(email) || !VerifierMotDePasse(motDePasse, confirmation))
            {
                return null;
            }

            var user = new Utilisateur
            {
                Pseudo = pseudo,
                Nom = nom,
                Prenom = prenom,
                Taille = taille,
                Poids = poids,
                Age = age,
                ObjectifPoids = objectif,
                DateObjectif = dateObjectif,
                DateInscription = DateTime.Now,
                Email = email,
                MotDePasse = motDePasse
            };

            // 🚀 Utiliser la propriété pour déclencher l'événement
            UtilisateurActif = user;

            JsonService.SauvegarderUtilisateur(user);

            Console.WriteLine($"✅ Nouvel utilisateur créé : {user.Pseudo} (Age: {user.Age})");

            return user;
        }

        // 🆕 NOUVELLE MÉTHODE : Mettre à jour l'utilisateur existant
        public static bool MettreAJourUtilisateur(Utilisateur utilisateurMisAJour)
        {
            try
            {
                if (utilisateurMisAJour != null)
                {
                    // Sauvegarder dans le fichier
                    JsonService.SauvegarderUtilisateur(utilisateurMisAJour);

                    // Mettre à jour l'utilisateur actif (déclenche l'événement)
                    UtilisateurActif = utilisateurMisAJour;

                    Console.WriteLine($"✅ Utilisateur mis à jour : {utilisateurMisAJour.Pseudo}");
                    return true;
                }
                return false;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Erreur lors de la mise à jour : {ex.Message}");
                return false;
            }
        }

        // 🆕 NOUVELLE MÉTHODE : Déconnexion
        public static void Deconnecter()
        {
            Console.WriteLine("👋 Déconnexion de l'utilisateur");
            UtilisateurActif = null; // Déclenche l'événement avec null
        }
    }
}