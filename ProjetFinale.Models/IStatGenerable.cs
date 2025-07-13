using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProjetFinale.Models
{
    interface IStatGenerable
    {
        public List<Statistique> GenererStatistiques();
    }
}
