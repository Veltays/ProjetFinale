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
                PseudoLabel.Text = user.Pseudo;
                DateInscriptionLabel.Text = $"INSCRIT LE {user.DateInscription:dd/MM/yyyy}";

                PoidLabel.Text = user.Poids.ToString();
                TailleLabel.Text = user.Taille.ToString();
                AnsLabel.Text = user.Age.ToString();

                double imc = user.Taille > 0 ? user.Poids / Math.Pow(user.Taille / 100.0, 2) : 0;
                ImcLabel.Text = imc.ToString("F1");

                PoidObjectifLabel.Text = user.ObjectifPoids.ToString();
                int anneeObjectif = user.DateObjectif.Year;
                int anneeActuelle = DateTime.Now.Year;
                int difference = anneeObjectif - anneeActuelle;
                DateObjectifLabel.Text = difference.ToString();

                double imcObjectif = user.Taille > 0 ? user.ObjectifPoids / Math.Pow(user.Taille / 100.0, 2) : 0;
                ImcObjectifLabel.Text = imcObjectif.ToString("F1");
            }
        }
    }
}