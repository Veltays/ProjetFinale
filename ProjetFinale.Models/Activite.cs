using System;
using System.Collections.Generic;

namespace ProjetFinale.Models
{
    public class Activite : ElementSuivi, IStatGenerable
    {
        public TimeSpan Duree { get; set; }
        public float CaloriesBrulees { get; set; }
        public string ImagePath { get; set; }

        public List<Statistique> GenererStatistiques()
        {
            return new List<Statistique>();
        }
    }
}
