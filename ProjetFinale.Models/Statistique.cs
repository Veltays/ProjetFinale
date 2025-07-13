using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ProjetFinale.Models.Enums;

namespace ProjetFinale.Models
{
    class Statistique : ElementSuivi
    {
        public TypeStatistique Type { get; set; }
        public float Valeur { get; set; }
        public string Unite { get; set; }
    }
}
