using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Collections.ObjectModel;

namespace ProjetFinale.Models
{
    public class Utilisateur : INotifyPropertyChanged
    {
        // Champs privés pour stocker les valeurs
        private string _pseudo;
        private string _nom;
        private string _prenom;
        private string _email;
        private int _age;
        private string _mdpHash;
        private double _poids;
        private double _taille;
        private double _objectifPoids;
        private DateTime _dateInscription;
        private DateTime _dateObjectif;
        private List<Activite> _listeActivites = new();
        private List<Statistique> _listeStatistiques = new();
        
        
        private ObservableCollection<Agenda> _listeAgenda = new ObservableCollection<Agenda>();
        public ObservableCollection<Agenda> ListeAgenda
        {
            get => _listeAgenda;
            set { _listeAgenda = value; OnPropertyChanged(); }
        }


        private List<Tache> _listeTaches = new();

        // Propriétés publiques avec notification
        public string Pseudo
        {
            get => _pseudo;
            set { _pseudo = value; OnPropertyChanged(); }
        }

        public string Nom
        {
            get => _nom;
            set { _nom = value; OnPropertyChanged(); }
        }

        public string Prenom
        {
            get => _prenom;
            set { _prenom = value; OnPropertyChanged(); }
        }

        public string Email
        {
            get => _email;
            set { _email = value; OnPropertyChanged(); }
        }

        public int Age
        {
            get => _age;
            set
            {
                _age = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(AgeFormate));
            }
        }

        public string MDPHash
        {
            get => _mdpHash;
            set { _mdpHash = value; OnPropertyChanged(); }
        }

        public double Poids
        {
            get => _poids;
            set
            {
                _poids = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(PoidsFormate));
                OnPropertyChanged(nameof(IMC));
                OnPropertyChanged(nameof(IMCFormate));
            }
        }

        public double Taille
        {
            get => _taille;
            set
            {
                _taille = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(TailleFormatee));
                OnPropertyChanged(nameof(IMC));
                OnPropertyChanged(nameof(IMCFormate));
                OnPropertyChanged(nameof(IMCObjectifFormate));
            }
        }

        public double ObjectifPoids
        {
            get => _objectifPoids;
            set
            {
                _objectifPoids = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(ObjectifPoidsFormate));
                OnPropertyChanged(nameof(IMCObjectifFormate));
            }
        }

        public DateTime DateInscription
        {
            get => _dateInscription;
            set { _dateInscription = value; OnPropertyChanged(); }
        }

        public DateTime DateObjectif
        {
            get => _dateObjectif;
            set
            {
                _dateObjectif = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(DateObjectifFormatee));
                OnPropertyChanged(nameof(AnneesRestantesFormate));
            }
        }

        public List<Activite> ListeActivites
        {
            get => _listeActivites;
            set { _listeActivites = value; OnPropertyChanged(); }
        }

        public List<Statistique> ListeStatistiques
        {
            get => _listeStatistiques;
            set { _listeStatistiques = value; OnPropertyChanged(); }
        }

        public List<Agenda> ListeAgenda
        {
            get => _listeAgenda;
            set { _listeAgenda = value; OnPropertyChanged(); }
        }

        public List<Tache> ListeTaches
        {
            get => _listeTaches;
            set { _listeTaches = value; OnPropertyChanged(); }
        }

        // Propriétés calculées et formatées pour l'affichage
        public double IMC => (Taille > 0) ? (Poids / Math.Pow(Taille / 100.0, 2)) : 0;

        public string AgeFormate => $"{Age} ANS";
        public string PoidsFormate => $"{Poids} KG";
        public string TailleFormatee => $"{Taille} CM";
        public string IMCFormate => $"{IMC:F1}";
        public string ObjectifPoidsFormate => $"{ObjectifPoids} KG";
        public string DateObjectifFormatee => DateObjectif.Year.ToString();

        public string IMCObjectifFormate
        {
            get
            {
                if (Taille > 0)
                {
                    double imcObjectif = ObjectifPoids / Math.Pow(Taille / 100.0, 2);
                    return $"{imcObjectif:F1} IMC";
                }
                return "0.0 IMC";
            }
        }

        public string AnneesRestantesFormate
        {
            get
            {
                int anneesRestantes = DateObjectif.Year - DateTime.Now.Year;
                return $"{Math.Max(0, anneesRestantes)} ANS";
            }
        }

        // Propriété MotDePasse (conservée pour compatibilité)
        public string MotDePasse
        {
            get => MDPHash;
            set
            {
                // Hashage simple pour l'exemple, à remplacer par un vrai hashage sécurisé
                MDPHash = Convert.ToBase64String(System.Text.Encoding.UTF8.GetBytes(value));
            }
        }

        // Implémentation INotifyPropertyChanged
        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}