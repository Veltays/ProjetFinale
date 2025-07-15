using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace ProjetFinale.WPF
{
    /// <summary>
    /// Interaction logic for SettingsPage.xaml
    /// </summary>
    public partial class SettingsPage : Page
    {
        public SettingsPage()
        {
            InitializeComponent();
        }
    


    private void ModifierProfil_Click(object sender, RoutedEventArgs e)
        {
            // Logique pour modifier le profil de l'utilisateur
            MessageBox.Show("Fonctionnalité de modification du profil à implémenter.");
        }



        private void DeleteAccount_Click(object sender, RoutedEventArgs e)
        {
            // Logique pour supprimer le compte de l'utilisateur
            MessageBox.Show("Fonctionnalité de suppression du compte à implémenter.");
        }

    }
}

