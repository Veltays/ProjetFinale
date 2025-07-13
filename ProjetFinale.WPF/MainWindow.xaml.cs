using ProjetFinale.Services;
using ProjetFinale.Views;
using System.Windows;

namespace ProjetFinale.Views
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            InitialiserUtilisateurConnecte();
        }


        private void InitialiserUtilisateurConnecte()
        {
            var user = UserService.UtilisateurActif;

            if (user != null)
            {
                // 🧠 Infos principales
                PseudoLabel.Text = user.Pseudo;
                DateInscriptionLabel.Text = $"INSCRIT LE {user.DateInscription:dd/MM/yyyy}";

                // 💪 Valeurs utilisateur
                PoidLabel.Text = user.Poids.ToString();
                TailleLabel.Text = user.Taille.ToString();
                AnsLabel.Text = user.Age.ToString();

                // 📊 IMC calculé dynamiquement
                double imc = user.Taille > 0 ? user.Poids / Math.Pow(user.Taille / 100.0, 2) : 0;
                ImcLabel.Text = imc.ToString("F1");

                // 🎯 Objectif
                PoidObjectifLabel.Text = user.ObjectifPoids.ToString();

                // Date objectif en années (si logique basée sur durée)
                int anneeObjectif = user.DateObjectif.Year;
                int anneeActuelle = DateTime.Now.Year;
                int difference = anneeObjectif - anneeActuelle;
                DateObjectifLabel.Text = difference.ToString();

                double imcObjectif = user.Taille > 0 ? user.ObjectifPoids / Math.Pow(user.Taille / 100.0, 2) : 0;
                ImcObjectifLabel.Text = imcObjectif.ToString("F1");
            }
        }


        // clique sur le bouton de logout
        private void Logout_Click(object sender, RoutedEventArgs e)
        {
            // Réinitialiser la valeur dans le registre
            var paramManager = new MyAppParamManager();
            paramManager.IsLogin = false;

            // Ouvrir LoginWindow
            var login = new LoginWindow();
            login.Show();

            // Fermer la fenêtre actuelle
            this.Close();
        }
    }
}