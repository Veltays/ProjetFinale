using System;

namespace ProjetFinale.Models
{
    public class Agenda
    {
        public TimeSpan HeureDebut { get; set; }
        public TimeSpan HeureFin { get; set; }
        public DateTime Date { get; set; }
        public Activite? Activite { get; set; }

        public override string ToString()
        {
            return $"{Date:dd/MM/yyyy} - {HeureDebut:hh\\:mm} → {HeureFin:hh\\:mm} | {Activite?.Titre}";
        }
    }
}
