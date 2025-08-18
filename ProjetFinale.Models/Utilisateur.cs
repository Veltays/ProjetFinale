using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;

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

        // ✅ CORRIGÉ : ObservableCollection pour l'agenda ET les tâches
        private ObservableCollection<Agenda> _listeAgenda = new ObservableCollection<Agenda>();
        private ObservableCollection<Tache> _listeTaches = new ObservableCollection<Tache>();

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


        // ✅ CORRIGÉ : ObservableCollection au lieu de List
        public ObservableCollection<Agenda> ListeAgenda
        {
            get => _listeAgenda;
            set { _listeAgenda = value; OnPropertyChanged(); }
        }

        public ObservableCollection<Tache> ListeTaches
        {
            get => _listeTaches;
            set { _listeTaches = value; OnPropertyChanged(); }
        }

        // Propriétés calculées et formatées pour l'affichage
        public double IMC => (Taille > 0) ? Math.Round(Poids / Math.Pow(Taille / 100, 2), 1) : 0;

        public string AgeFormate => $"{Age} ANS";
        public string PoidsFormate => $"{Poids} KG";
        public string TailleFormatee => $"{Taille} CM";
        public string IMCFormate => $"{IMC:F1}";
        public string ObjectifPoidsFormate => $"{ObjectifPoids} KG";
        public string DateObjectifFormatee => DateObjectif.Year.ToString();
        public string IMCObjectifFormate => (Taille > 0) ? $"{Math.Round(ObjectifPoids / Math.Pow(Taille / 100, 2), 1):F1} IMC" : "0 IMC";
        public string AnneesRestantesFormate
        {
            get
            {
                var annees = Math.Max(0, DateObjectif.Year - DateTime.Now.Year);
                return $"{annees} ANS";
            }
        }

        // Propriété pour la compatibilité JSON (ne pas supprimer)
        public string MotDePasse
        {
            get => MDPHash;
            set => MDPHash = value;
        }

        // Implémentation INotifyPropertyChanged
        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}