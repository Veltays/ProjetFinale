using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProjetFinale.Models
{
    public abstract class ElementSuivi
    {
        public int Id { get; set; }
        public string Nom { get; set; }
        public DateTime Date { get; set; }
    }
}
