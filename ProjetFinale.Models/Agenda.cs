using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace ProjetFinale.Models
{
    public class Agenda : INotifyPropertyChanged
    {
        private string _titre;
        private TimeSpan _heureDebut;
        private TimeSpan _heureFin;
        private DateTime _date;
        private Activite _activite;
        private string _couleur;
        private string _description;

        public string Titre
        {
            get => _titre;
            set { _titre = value; OnPropertyChanged(); }
        }

        public TimeSpan HeureDebut
        {
            get => _heureDebut;
            set { _heureDebut = value; OnPropertyChanged(); }
        }

        public TimeSpan HeureFin
        {
            get => _heureFin;
            set { _heureFin = value; OnPropertyChanged(); }
        }

        public DateTime Date
        {
            get => _date;
            set { _date = value; OnPropertyChanged(); }
        }

        public Activite Activite
        {
            get => _activite;
            set { _activite = value; OnPropertyChanged(); }
        }

        // ✅ AJOUTEZ CES PROPRIÉTÉS MANQUANTES :

        public string Couleur
        {
            get => _couleur ?? "#8B5CF6"; // Violet par défaut
            set { _couleur = value; OnPropertyChanged(); }
        }

        public string Description
        {
            get => _description;
            set { _description = value; OnPropertyChanged(); }
        }

        // ✅ PROPRIÉTÉS CALCULÉES UTILES :

        public TimeSpan Duree => HeureFin - HeureDebut;

        public string DureeFormatee =>
            Duree.TotalMinutes < 60
                ? $"{Duree.TotalMinutes:F0}min"
                : $"{Duree.TotalHours:F1}h";

        public string HeureFormatee => $"{HeureDebut:hh\\:mm} - {HeureFin:hh\\:mm}";

        // ✅ AJOUTEZ L'IMPLÉMENTATION INOTIFYPROPERTYCHANGED :

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        // ✅ MÉTHODE TOSTRING UTILE :

        public override string ToString()
        {
            string activiteInfo = Activite != null ? $" | {Activite.Titre}" : "";
            return $"{Date:dd/MM/yyyy} - {HeureFormatee} - {Titre}{activiteInfo}";
        }
    }
}