using ProjetFinale.Models.Enums;

namespace ProjetFinale.Models
{
    public class Statistique 
    {
        public TypeStatistique Type { get; set; }
        public float Valeur { get; set; }
        public string Unite { get; set; }
    }
}