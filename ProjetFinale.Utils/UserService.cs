using ProjetFinale.Models;
using ProjetFinale.Utils;
using System;
using System.Text.RegularExpressions;

namespace ProjetFinale.Services
{
    public class UserService
    {
        public static Utilisateur? UtilisateurActif { get; set; }
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


            UtilisateurActif = user;

            JsonService.SauvegarderUtilisateur(user);

            return user;
        }
    }
}