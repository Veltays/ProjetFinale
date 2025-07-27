using ProjetFinale.Services;
using ProjetFinale.Utils;
using System;
using System.Windows;
using System.Windows.Controls;

namespace ProjetFinale.WPF
{
    public partial class AccueilPage : Page
    {
        public AccueilPage()
        {
            InitializeComponent();
            InitialiserUtilisateurConnecte();
        }

        private void InitialiserUtilisateurConnecte()
        {
            var user = UserService.UtilisateurActif;
            if (user != null)
            {
                // 🎯 DÉFINIR LE DATACONTEXT - C'est tout ce qu'il faut !
                this.DataContext = user;

                Console.WriteLine($"✅ DataContext défini pour l'utilisateur : {user.Pseudo}");
                Console.WriteLine($"   Âge : {user.Age} ans");
                Console.WriteLine($"   Poids : {user.Poids} kg");
                Console.WriteLine($"   Taille : {user.Taille} cm");
            }
            else
            {
                Console.WriteLine("⚠️ Aucun utilisateur actif trouvé !");

                // Optionnel : Charger depuis le fichier JSON si pas d'utilisateur actif
                var utilisateurCharge = JsonService.ChargerUtilisateur();
                if (utilisateurCharge != null)
                {
                    UserService.UtilisateurActif = utilisateurCharge;
                    this.DataContext = utilisateurCharge;
                    Console.WriteLine($"✅ Utilisateur chargé depuis le fichier : {utilisateurCharge.Pseudo}");
                }
            }
        }

        // Méthode publique pour rafraîchir les données (appelée après import)
        public void RafraichirDonnees()
        {
            InitialiserUtilisateurConnecte();
        }

        // === ÉVÉNEMENTS ACTIONS RAPIDES ===

        private void NouvelleSeance_Click(object sender, RoutedEventArgs e)
        {
            // Naviguer vers la page exercices
            var mainWindow = Application.Current.MainWindow as Views.MainWindow;
            mainWindow?.NavigateToExercices();
        }

        private void VoirProgres_Click(object sender, RoutedEventArgs e)
        {
            // Rester sur cette page et montrer les graphiques
            MessageBox.Show("Vos progrès sont affichés dans les graphiques ci-dessous !",
                           "Progrès",
                           MessageBoxButton.OK,
                           MessageBoxImage.Information);

            // Optionnel : scroll automatique vers les graphiques
            // ScrollToGraphiques();
        }

        private void MesObjectifs_Click(object sender, RoutedEventArgs e)
        {
            // Naviguer vers la page Task/Objectifs
            var mainWindow = Application.Current.MainWindow as Views.MainWindow;
            mainWindow?.NavigateToObjectifs();
        }

        private void Planning_Click(object sender, RoutedEventArgs e)
        {
            // Naviguer vers la page Schedule
            var mainWindow = Application.Current.MainWindow as Views.MainWindow;
            mainWindow?.NavigateToSchedule();
        }

        // === MÉTHODES UTILITAIRES ===

        private void AfficherStatistiquesUtilisateur()
        {
            var user = UserService.UtilisateurActif;
            if (user != null)
            {
                // 🔥 Maintenant on utilise les propriétés formatées !
                string stats = $"Utilisateur: {user.Pseudo}\n" +
                              $"Poids: {user.PoidsFormate}\n" +
                              $"Taille: {user.TailleFormatee}\n" +
                              $"Âge: {user.AgeFormate}\n" +
                              $"IMC: {user.IMCFormate}\n" +
                              $"Objectif poids: {user.ObjectifPoidsFormate}\n" +
                              $"IMC objectif: {user.IMCObjectifFormate}\n" +
                              $"Années restantes: {user.AnneesRestantesFormate}";

                MessageBox.Show(stats, "Statistiques Utilisateur", MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }

        // Méthode pour scroll vers les graphiques (optionnel)
        private void ScrollToGraphiques()
        {
            // Si tu veux implémenter un scroll automatique vers les graphiques
            // Tu peux utiliser le ScrollViewer parent
        }
    }
}