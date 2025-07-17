using System;
using System.Windows;
using System.Windows.Controls;
using ProjetFinale.Services;

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
                // Informations utilisateur dans le profil (nouvelles propriétés XAML)
                // Ces TextBlock remplacent les anciens TextBox
                // Tu peux les récupérer par nom si besoin, ou créer des TextBlock dans le XAML

                // Calcul IMC
                double imc = user.Taille > 0 ? user.Poids / Math.Pow(user.Taille / 100.0, 2) : 0;

                // Calcul années restantes pour objectif
                int anneeObjectif = user.DateObjectif.Year;
                int anneeActuelle = DateTime.Now.Year;
                int difference = anneeObjectif - anneeActuelle;

                // Calcul IMC objectif
                double imcObjectif = user.Taille > 0 ? user.ObjectifPoids / Math.Pow(user.Taille / 100.0, 2) : 0;

                // Mise à jour des données (si tu veux garder des propriétés modifiables)
                // Sinon, tu peux directement mettre les valeurs dans le XAML avec binding

                // Note: Le nouveau XAML utilise des TextBlock statiques, 
                // tu peux soit :
                // 1. Ajouter des x:Name aux TextBlock pour les modifier ici
                // 2. Ou utiliser le binding avec un ViewModel
                // 3. Ou garder les valeurs en dur pour l'instant

                // Pour l'instant, on peut afficher les infos dans des MessageBox pour debug
                // ou tu peux ajouter des x:Name aux TextBlock dans le XAML
            }
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
            MessageBox.Show("Vos progrès sont affichés dans les graphiques ci-dessous !", "Progrès",
                           MessageBoxButton.OK, MessageBoxImage.Information);

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
                double imc = user.Taille > 0 ? user.Poids / Math.Pow(user.Taille / 100.0, 2) : 0;
                double imcObjectif = user.Taille > 0 ? user.ObjectifPoids / Math.Pow(user.Taille / 100.0, 2) : 0;
                int anneesRestantes = user.DateObjectif.Year - DateTime.Now.Year;

                string stats = $"Utilisateur: {user.Pseudo}\n" +
                              $"Poids: {user.Poids} KG\n" +
                              $"Taille: {user.Taille} CM\n" +
                              $"Âge: {user.Age} ANS\n" +
                              $"IMC: {imc:F1}\n" +
                              $"Objectif poids: {user.ObjectifPoids} KG\n" +
                              $"IMC objectif: {imcObjectif:F1}\n" +
                              $"Années restantes: {anneesRestantes}";

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