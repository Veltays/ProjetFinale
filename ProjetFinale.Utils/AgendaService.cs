using Newtonsoft.Json;
using ProjetFinale.Models;
using ProjetFinale.Utils;           // ✅ pour JsonService
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;

namespace ProjetFinale.Services
{
    /// <summary>
    /// Version refactorée : 
    /// - PLUS AUCUN accès à agenda.json
    /// - TOUT passe par UserService.UtilisateurActif.ListeAgenda
    /// - Sauvegarde via JsonService.SauvegarderUtilisateur(utilisateur)
    /// </summary>
    public static class AgendaService
    {
        // ==========
        // Helpers internes
        // ==========

        private static Utilisateur? CurrentUser => UserService.UtilisateurActif;

        private static ObservableCollection<Agenda> GetAgendaCollection(bool createIfNull = true)
        {
            var user = CurrentUser;
            if (user == null)
            {
                Console.WriteLine("⚠️ Aucun utilisateur actif. ListeAgenda indisponible.");
                return new ObservableCollection<Agenda>();
            }

            if (user.ListeAgenda == null && createIfNull)
            {
                user.ListeAgenda = new ObservableCollection<Agenda>();
                TrySaveUser(user);
            }

            return user.ListeAgenda ?? new ObservableCollection<Agenda>();
        }

        private static bool TrySaveUser(Utilisateur? user = null)
        {
            try
            {
                user ??= CurrentUser;
                if (user == null)
                {
                    Console.WriteLine("⚠️ Sauvegarde ignorée : aucun utilisateur actif.");
                    return false;
                }

                JsonService.SauvegarderUtilisateur(user);
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Erreur lors de la sauvegarde de l'utilisateur : {ex.Message}");
                return false;
            }
        }

        // ==========
        // API publique (signatures conservées quand possible)
        // ==========

        /// <summary>
        /// (Refactor) Sauvegarder les événements = met à jour ListeAgenda de l'utilisateur et sauvegarde l'utilisateur.
        /// </summary>
        public static void SauvegarderAgenda(List<Agenda> evenements)
        {
            var user = CurrentUser;
            if (user == null)
            {
                Console.WriteLine("⚠️ SauvegarderAgenda ignoré : aucun utilisateur actif.");
                return;
            }

            user.ListeAgenda = new ObservableCollection<Agenda>(evenements ?? new List<Agenda>());
            TrySaveUser(user);
        }

        /// <summary>
        /// (Refactor) Charger les événements = lit depuis UtilisateurActif.ListeAgenda
        /// </summary>
        public static List<Agenda> ChargerAgenda()
        {
            var list = GetAgendaCollection(createIfNull: false);
            // On renvoie une copie List pour compatibilité avec l'existant
            return list?.ToList() ?? new List<Agenda>();
        }

        /// <summary>
        /// Ajouter un nouvel événement (dans l'utilisateur), puis sauvegarder l'utilisateur.
        /// </summary>
        public static void AjouterEvenement(Agenda nouvelEvenement)
        {
            if (nouvelEvenement == null) return;

            var coll = GetAgendaCollection();
            coll.Add(nouvelEvenement);
            TrySaveUser();
        }

        /// <summary>
        /// Modifier un événement existant (match naïf : Date + HeureDébut + Titre), puis sauvegarder.
        /// </summary>
        public static void ModifierEvenement(Agenda evenementModifie)
        {
            if (evenementModifie == null) return;

            var coll = GetAgendaCollection();
            var index = coll.ToList().FindIndex(e =>
                e.Date == evenementModifie.Date &&
                e.HeureDebut == evenementModifie.HeureDebut &&
                e.Titre == evenementModifie.Titre);

            if (index >= 0)
            {
                coll[index] = evenementModifie;
                TrySaveUser();
            }
        }

        /// <summary>
        /// Supprimer un événement (match naïf : Date + HeureDébut + Titre), puis sauvegarder.
        /// </summary>
        public static void SupprimerEvenement(Agenda evenementASupprimer)
        {
            if (evenementASupprimer == null) return;

            var coll = GetAgendaCollection();
            // On recherche toutes les correspondances "équivalentes"
            var toRemove = coll.Where(e =>
                e.Date == evenementASupprimer.Date &&
                e.HeureDebut == evenementASupprimer.HeureDebut &&
                e.Titre == evenementASupprimer.Titre).ToList();

            foreach (var e in toRemove)
                coll.Remove(e);

            TrySaveUser();
        }

        /// <summary>
        /// Obtenir les événements d'une période.
        /// </summary>
        public static List<Agenda> ObtenirEvenements(DateTime dateDebut, DateTime dateFin)
        {
            var evenements = ChargerAgenda();
            return evenements
                .Where(e => e.Date >= dateDebut && e.Date <= dateFin)
                .ToList();
        }

        /// <summary>
        /// Obtenir les événements d'une semaine (dateDebutSemaine = lundi).
        /// </summary>
        public static List<Agenda> ObtenirEvenementsSemaine(DateTime dateDebutSemaine)
        {
            var dateFin = dateDebutSemaine.AddDays(6);
            return ObtenirEvenements(dateDebutSemaine, dateFin);
        }

        /// <summary>
        /// Obtenir les événements d'une journée (toute la journée).
        /// </summary>
        public static List<Agenda> ObtenirEvenementsJour(DateTime date)
        {
            var debut = date.Date;
            var fin = date.Date.AddDays(1).AddTicks(-1);
            return ObtenirEvenements(debut, fin);
        }

        /// <summary>
        /// Vérifier s'il y a un conflit horaire.
        /// Par défaut, on vérifie vs la liste de l'utilisateur.
        /// </summary>
        public static bool VerifierConflitHoraire(Agenda nouvelEvenement, List<Agenda> evenementsExistants = null)
        {
            evenementsExistants ??= ChargerAgenda();

            return evenementsExistants.Any(e =>
                e.Date.Date == nouvelEvenement.Date.Date &&
                !(nouvelEvenement.HeureFin <= e.HeureDebut || nouvelEvenement.HeureDebut >= e.HeureFin));
        }

        /// <summary>
        /// Statistiques de la semaine.
        /// </summary>
        public static (int NombreEvenements, double HeuresTotal) ObtenirStatistiquesSemaine(DateTime dateDebutSemaine)
        {
            var evenements = ObtenirEvenementsSemaine(dateDebutSemaine);
            var nombreEvenements = evenements.Count;
            var heuresTotal = evenements.Sum(e => (e.HeureFin - e.HeureDebut).TotalHours);
            return (nombreEvenements, heuresTotal);
        }

        /// <summary>
        /// Export CSV : lit la liste de l'utilisateur (pas de fichier externe).
        /// </summary>
        public static string ExporterCSV()
        {
            var evenements = ChargerAgenda();
            var lignes = new List<string>
            {
                "Date,HeureDebut,HeureFin,Titre,Description,Activite,Couleur"
            };

            foreach (var evt in evenements.OrderBy(e => e.Date).ThenBy(e => e.HeureDebut))
            {
                var ligne = $"{evt.Date:yyyy-MM-dd}," +
                           $"{evt.HeureDebut:hh\\:mm}," +
                           $"{evt.HeureFin:hh\\:mm}," +
                           $"\"{evt.Titre}\"," +
                           $"\"{(evt.Description ?? "").Replace("\"", "\"\"")}\"," +
                           $"\"{(evt.Activite?.Titre ?? "").Replace("\"", "\"\"")}\"," +
                           $"{evt.Couleur}";
                lignes.Add(ligne);
            }

            return string.Join(Environment.NewLine, lignes);
        }

        /// <summary>
        /// Nettoyer les anciens événements (plus de 1 an) dans l'utilisateur, puis sauvegarder.
        /// </summary>
        public static void NettoyerAnciensEvenements()
        {
            var coll = GetAgendaCollection();
            var dateSeuilSuppression = DateTime.Now.AddYears(-1);

            var conserves = coll.Where(e => e.Date >= dateSeuilSuppression).ToList();
            if (conserves.Count != coll.Count)
            {
                // Remplacer la collection (préserve le binding si la prop Notifies)
                var user = CurrentUser;
                if (user != null)
                {
                    user.ListeAgenda = new ObservableCollection<Agenda>(conserves);
                    TrySaveUser(user);
                }
            }
        }
    }
}