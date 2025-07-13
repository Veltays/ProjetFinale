using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProjetFinale.Models
{
    class Activite : ElementSuivi, IStatGenerable
    {




        public List<Statistique> GenererStatistiques()
        {
            return new List<Statistique>(); // ou du vrai code plus tard
        }
    }

}
