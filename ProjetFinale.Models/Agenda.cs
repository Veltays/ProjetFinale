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