using System;
using System.Collections.Generic;

namespace ProjetFinale.Models
{
    public class Activite : ElementSuivi
    {
        public string Titre { get; set; }
        public TimeSpan Duree { get; set; }
        public float CaloriesBrulees { get; set; }
        public string ImagePath { get; set; }

    }
}
