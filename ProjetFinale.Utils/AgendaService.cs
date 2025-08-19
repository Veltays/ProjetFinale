using ProjetFinale.Models;
using ProjetFinale.Utils; // JsonService
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace ProjetFinale.Services
{
    public static class AgendaService
    {
        // --- Accès rapide à l'utilisateur actif
        private static Utilisateur? CurrentUser => UserService.UtilisateurActif;

        // Retourne la collection d'agenda de l'utilisateur (crée si null)
        private static ObservableCollection<Agenda> GetAgendaCollection(bool createIfMissing = true)
        {
            var user = CurrentUser;
            if (user == null)
            {
                Console.WriteLine("⚠️ Aucun utilisateur actif.");
                return new ObservableCollection<Agenda>();
            }

            if (user.ListeAgenda == null && createIfMissing)
            {
                user.ListeAgenda = new ObservableCollection<Agenda>();
                SaveUser(user);
            }

            return user.ListeAgenda ?? new ObservableCollection<Agenda>();
        }

        // Sauvegarde l'utilisateur actif (évite les try/catch répétitifs)
        private static bool SaveUser(Utilisateur? user = null)
        {
            user ??= CurrentUser;
            if (user == null)
            {
                Console.WriteLine("⚠️ Sauvegarde ignorée (aucun utilisateur).");
                return false;
            }

            try
            {
                JsonService.SauvegarderUtilisateur(user);
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Erreur sauvegarde utilisateur : {ex.Message}");
                return false;
            }
        }

        // --- API publique (noms conservés)

        // Remplace toute la liste par 'evenements' et sauvegarde
        public static void SauvegarderAgenda(List<Agenda> evenements)
        {
            var user = CurrentUser;
            if (user == null)
            {
                Console.WriteLine("⚠️ SauvegarderAgenda ignoré (aucun utilisateur).");
                return;
            }

            user.ListeAgenda = new ObservableCollection<Agenda>(evenements ?? new List<Agenda>());
            SaveUser(user);
        }

        // Renvoie une copie List<> pour compatibilité
        public static List<Agenda> ChargerAgenda()
        {
            var list = GetAgendaCollection(createIfMissing: false);
            return list?.ToList() ?? new List<Agenda>();
        }

        // Ajoute un événement et sauvegarde
        public static void AjouterEvenement(Agenda nouvelEvenement)
        {
            if (nouvelEvenement == null) return;

            var agenda = GetAgendaCollection();
            agenda.Add(nouvelEvenement);
            SaveUser();
        }

        // Modifie (match simple : Date + HeureDébut + Titre) puis sauvegarde
        public static void ModifierEvenement(Agenda evenementModifie)
        {
            if (evenementModifie == null) return;

            var agenda = GetAgendaCollection();
            var index = agenda.ToList().FindIndex(e => IsSameIdentity(e, evenementModifie));
            if (index >= 0)
            {
                agenda[index] = evenementModifie;
                SaveUser();
            }
        }

        // Supprime (match simple : Date + HeureDébut + Titre) puis sauvegarde
        public static void SupprimerEvenement(Agenda evenementASupprimer)
        {
            if (evenementASupprimer == null) return;

            var agenda = GetAgendaCollection();
            var toRemove = agenda.Where(e => IsSameIdentity(e, evenementASupprimer)).ToList();
            foreach (var e in toRemove) agenda.Remove(e);

            if (toRemove.Count > 0) SaveUser();
        }

        // Renvoie les événements dans [dateDebut, dateFin]
        public static List<Agenda> ObtenirEvenements(DateTime dateDebut, DateTime dateFin)
        {
            return ChargerAgenda()
                .Where(e => e.Date >= dateDebut && e.Date <= dateFin)
                .ToList();
        }

        // Renvoie les événements d'une semaine (lundi -> dimanche)
        public static List<Agenda> ObtenirEvenementsSemaine(DateTime dateDebutSemaine)
        {
            var dateFin = dateDebutSemaine.AddDays(6);
            return ObtenirEvenements(dateDebutSemaine, dateFin);
        }

        // Renvoie les événements d'un jour (toute la journée)
        public static List<Agenda> ObtenirEvenementsJour(DateTime date)
        {
            var debut = date.Date;
            var fin = date.Date.AddDays(1).AddTicks(-1);
            return ObtenirEvenements(debut, fin);
        }

        // Détecte un chevauchement horaire le même jour
        public static bool VerifierConflitHoraire(Agenda nouvelEvenement, List<Agenda> existants = null)
        {
            if (nouvelEvenement == null) return false;

            existants ??= ChargerAgenda();

            return existants.Any(e =>
                e.Date.Date == nouvelEvenement.Date.Date &&
                !(nouvelEvenement.HeureFin <= e.HeureDebut || nouvelEvenement.HeureDebut >= e.HeureFin));
        }

        // Stats semaine : nombre d'événements + total heures
        public static (int NombreEvenements, double HeuresTotal) ObtenirStatistiquesSemaine(DateTime dateDebutSemaine)
        {
            var semaine = ObtenirEvenementsSemaine(dateDebutSemaine);
            var count = semaine.Count;
            var heures = semaine.Sum(e => (e.HeureFin - e.HeureDebut).TotalHours);
            return (count, heures);
        }

   

        // Supprime les événements de plus d'un an et sauvegarde si changement
        public static void NettoyerAnciensEvenements()
        {
            var agenda = GetAgendaCollection();
            var seuil = DateTime.Now.AddYears(-1);

            var conserves = agenda.Where(e => e.Date >= seuil).ToList();
            if (conserves.Count == agenda.Count) return;

            var user = CurrentUser;
            if (user == null) return;

            user.ListeAgenda = new ObservableCollection<Agenda>(conserves);
            SaveUser(user);
        }

        // --- Petits helpers privés

        // Identité “simple” d’un événement
        private static bool IsSameIdentity(Agenda a, Agenda b) =>
            a.Date == b.Date && a.HeureDebut == b.HeureDebut && a.Titre == b.Titre;

    }
}
