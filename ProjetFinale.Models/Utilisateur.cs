using System;
using System.Collections.Generic;

namespace ProjetFinale.Models
{
    public class Utilisateur
    {
        public string Pseudo { get; set; }
        public string Nom { get; set; }
        public string Prenom { get; set; }
        public string Email { get; set; }

        public int Age { get; set; }
        public string MDPHash { get; set; }

        public double Poids { get; set; }
        public double Taille { get; set; }

        public double IMC => (Taille > 0) ? (Poids / (Taille * Taille)) : 0f; // auto-calculé

        public double ObjectifPoids { get; set; }

        public DateTime DateInscription { get; set; }

        public string MotDePasse
        {
            get => MDPHash;
            set
            {
                // Hashage simple pour l'exemple, à remplacer par un vrai hashage sécurisé
                MDPHash = Convert.ToBase64String(System.Text.Encoding.UTF8.GetBytes(value));
            }
        }

        public DateTime DateObjectif { get; set; }

        public List<Activite> ListeActivites { get; set; } = new();
        public List<Statistique> ListeStatistiques { get; set; } = new();
        public List<Tache> ListeTaches { get; set; } = new();
    }
}
