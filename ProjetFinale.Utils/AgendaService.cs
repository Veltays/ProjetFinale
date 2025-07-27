using Newtonsoft.Json;
using ProjetFinale.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;


namespace ProjetFinale.Services
{
    public static class AgendaService
    {
        private static readonly string AgendaFilePath = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
            "ProjetFinale",
            "agenda.json"
        );

        static AgendaService()
        {
            // Créer le répertoire s'il n'existe pas
            var directory = Path.GetDirectoryName(AgendaFilePath);
            if (!Directory.Exists(directory))
            {
                Directory.CreateDirectory(directory);
            }
        }

        /// <summary>
        /// Sauvegarder les événements d'agenda en JSON
        /// </summary>
        public static void SauvegarderAgenda(List<Agenda> evenements)
        {
            try
            {
                var json = JsonConvert.SerializeObject(evenements, Formatting.Indented, new JsonSerializerSettings
                {
                    DateFormatString = "yyyy-MM-dd",
                    NullValueHandling = NullValueHandling.Ignore
                });

                File.WriteAllText(AgendaFilePath, json);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Erreur lors de la sauvegarde de l'agenda : {ex.Message}");
                // TODO: Logger l'erreur
            }
        }

        /// <summary>
        /// Charger les événements d'agenda depuis le JSON
        /// </summary>
        public static List<Agenda> ChargerAgenda()
        {
            try
            {
                if (!File.Exists(AgendaFilePath))
                {
                    return new List<Agenda>();
                }

                var json = File.ReadAllText(AgendaFilePath);
                if (string.IsNullOrWhiteSpace(json))
                {
                    return new List<Agenda>();
                }

                var evenements = JsonConvert.DeserializeObject<List<Agenda>>(json);
                return evenements ?? new List<Agenda>();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Erreur lors du chargement de l'agenda : {ex.Message}");
                // TODO: Logger l'erreur
                return new List<Agenda>();
            }
        }

        /// <summary>
        /// Ajouter un nouvel événement
        /// </summary>
        public static void AjouterEvenement(Agenda nouvelEvenement)
        {
            var evenements = ChargerAgenda();
            evenements.Add(nouvelEvenement);
            SauvegarderAgenda(evenements);
        }

        /// <summary>
        /// Modifier un événement existant
        /// </summary>
        public static void ModifierEvenement(Agenda evenementModifie)
        {
            var evenements = ChargerAgenda();
            var index = evenements.FindIndex(e =>
                e.Date == evenementModifie.Date &&
                e.HeureDebut == evenementModifie.HeureDebut &&
                e.Titre == evenementModifie.Titre);

            if (index >= 0)
            {
                evenements[index] = evenementModifie;
                SauvegarderAgenda(evenements);
            }
        }

        /// <summary>
        /// Supprimer un événement
        /// </summary>
        public static void SupprimerEvenement(Agenda evenementASupprimer)
        {
            var evenements = ChargerAgenda();
            evenements.RemoveAll(e =>
                e.Date == evenementASupprimer.Date &&
                e.HeureDebut == evenementASupprimer.HeureDebut &&
                e.Titre == evenementASupprimer.Titre);

            SauvegarderAgenda(evenements);
        }

        /// <summary>
        /// Obtenir les événements d'une période donnée
        /// </summary>
        public static List<Agenda> ObtenirEvenements(DateTime dateDebut, DateTime dateFin)
        {
            var evenements = ChargerAgenda();
            return evenements.Where(e => e.Date >= dateDebut && e.Date <= dateFin).ToList();
        }

        /// <summary>
        /// Obtenir les événements d'une semaine
        /// </summary>
        public static List<Agenda> ObtenirEvenementsSemaine(DateTime dateDebutSemaine)
        {
            var dateFin = dateDebutSemaine.AddDays(6);
            return ObtenirEvenements(dateDebutSemaine, dateFin);
        }

        /// <summary>
        /// Obtenir les événements d'une journée
        /// </summary>
        public static List<Agenda> ObtenirEvenementsJour(DateTime date)
        {
            return ObtenirEvenements(date.Date, date.Date.AddDays(1).AddTicks(-1));
        }

        /// <summary>
        /// Vérifier s'il y a un conflit horaire
        /// </summary>
        public static bool VerifierConflitHoraire(Agenda nouvelEvenement, List<Agenda> evenementsExistants = null)
        {
            evenementsExistants ??= ChargerAgenda();

            return evenementsExistants.Any(e =>
                e.Date.Date == nouvelEvenement.Date.Date &&
                !(nouvelEvenement.HeureFin <= e.HeureDebut || nouvelEvenement.HeureDebut >= e.HeureFin));
        }

        /// <summary>
        /// Obtenir les statistiques de la semaine
        /// </summary>
        public static (int NombreEvenements, double HeuresTotal) ObtenirStatistiquesSemaine(DateTime dateDebutSemaine)
        {
            var evenements = ObtenirEvenementsSemaine(dateDebutSemaine);
            var nombreEvenements = evenements.Count;
            var heuresTotal = evenements.Sum(e => (e.HeureFin - e.HeureDebut).TotalHours);

            return (nombreEvenements, heuresTotal);
        }

        /// <summary>
        /// Exporter l'agenda au format CSV
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
                           $"\"{evt.Description ?? ""}\"," +
                           $"\"{evt.Activite?.Titre ?? ""}\"," +
                           $"{evt.Couleur}";

                lignes.Add(ligne);
            }

            return string.Join(Environment.NewLine, lignes);
        }

        /// <summary>
        /// Nettoyer les anciens événements (plus de 1 an)
        /// </summary>
        public static void NettoyerAnciensEvenements()
        {
            var evenements = ChargerAgenda();
            var dateSeuilSuppression = DateTime.Now.AddYears(-1);

            var evenementsAConserver = evenements.Where(e => e.Date >= dateSeuilSuppression).ToList();

            if (evenementsAConserver.Count != evenements.Count)
            {
                SauvegarderAgenda(evenementsAConserver);
            }
        }
    }
}