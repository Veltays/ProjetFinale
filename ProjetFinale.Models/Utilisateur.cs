using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace ProjetFinale.Models
{
    public class Utilisateur : INotifyPropertyChanged
    {
        private string _pseudo = string.Empty;
        private string _nom = string.Empty;
        private string _prenom = string.Empty;
        private string _email = string.Empty;
        private int _age;
        private string _mdpHash = string.Empty;
        private double _poids;
        private double _taille;
        private double _objectifPoids;
        private DateTime _dateInscription;
        private DateTime _dateObjectif = DateTime.Now;
        private List<Activite> _listeActivites = new();

        private ObservableCollection<Agenda> _listeAgenda = new();
        private ObservableCollection<Tache> _listeTaches = new();

        // --- Propriétés ---

        public string Pseudo
        {
            get => _pseudo;
            set => Set(ref _pseudo, value);
        }

        public string Nom
        {
            get => _nom;
            set => Set(ref _nom, value);
        }

        public string Prenom
        {
            get => _prenom;
            set => Set(ref _prenom, value);
        }

        public string Email
        {
            get => _email;
            set => Set(ref _email, value);
        }

        public int Age
        {
            get => _age;
            set => Set(ref _age, value, nameof(AgeFormate));
        }

        public string MDPHash
        {
            get => _mdpHash;
            set => Set(ref _mdpHash, value);
        }

        public double Poids
        {
            get => _poids;
            set => Set(ref _poids, value,
                nameof(PoidsFormate), nameof(IMC), nameof(IMCFormate), nameof(IMCObjectifFormate));
        }

        public double Taille
        {
            get => _taille;
            set => Set(ref _taille, value,
                nameof(TailleFormatee), nameof(IMC), nameof(IMCFormate), nameof(IMCObjectifFormate));
        }

        public double ObjectifPoids
        {
            get => _objectifPoids;
            set => Set(ref _objectifPoids, value, nameof(ObjectifPoidsFormate), nameof(IMCObjectifFormate));
        }

        public DateTime DateInscription
        {
            get => _dateInscription;
            set => Set(ref _dateInscription, value);
        }

        public DateTime DateObjectif
        {
            get => _dateObjectif;
            set => Set(ref _dateObjectif, value, nameof(DateObjectifFormatee), nameof(AnneesRestantesFormate));
        }

        public List<Activite> ListeActivites
        {
            get => _listeActivites;
            set => Set(ref _listeActivites, value);
        }

        public ObservableCollection<Agenda> ListeAgenda
        {
            get => _listeAgenda;
            set => Set(ref _listeAgenda, value);
        }

        public ObservableCollection<Tache> ListeTaches
        {
            get => _listeTaches;
            set => Set(ref _listeTaches, value);
        }

        // --- Propriétés calculées / formatées ---

        public double IMC => (Taille > 0) ? Math.Round(Poids / Math.Pow(Taille / 100.0, 2), 1) : 0;

        public string AgeFormate => $"{Age} ANS";
        public string PoidsFormate => $"{Poids} KG";
        public string TailleFormatee => $"{Taille} CM";
        public string IMCFormate => $"{IMC:F1}";
        public string ObjectifPoidsFormate => $"{ObjectifPoids} KG";
        public string DateObjectifFormatee => DateObjectif.Year.ToString();
        public string IMCObjectifFormate => (Taille > 0)
            ? $"{Math.Round(ObjectifPoids / Math.Pow(Taille / 100.0, 2), 1):F1} IMC"
            : "0 IMC";

        public string AnneesRestantesFormate
        {
            get
            {
                var annees = Math.Max(0, DateObjectif.Year - DateTime.Now.Year);
                return $"{annees} ANS";
            }
        }

        // Compat JSON
        public string MotDePasse
        {
            get => MDPHash;
            set => MDPHash = value;
        }

        // --- INotifyPropertyChanged ---

        public event PropertyChangedEventHandler? PropertyChanged;

        protected void OnPropertyChanged([CallerMemberName] string? name = null) =>
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));

        protected bool Set<T>(ref T field, T value, params string[] alsoNotify)
        {
            if (EqualityComparer<T>.Default.Equals(field, value)) return false;
            field = value;
            OnPropertyChanged();
            if (alsoNotify != null)
                foreach (var n in alsoNotify) OnPropertyChanged(n);
            return true;
        }
    }
}