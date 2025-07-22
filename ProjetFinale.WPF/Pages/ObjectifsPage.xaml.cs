using ProjetFinale.Models;
using ProjetFinale.Utils;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Shapes;

namespace ProjetFinale.WPF
{
    public partial class ObjectifPage : Page
    {
        private Utilisateur? _utilisateur;

        public ObjectifPage()
        {
            InitializeComponent();
            _utilisateur = JsonService.ChargerUtilisateur();
            AfficherTaches();
        }

        private void TextBox_GotFocus(object sender, RoutedEventArgs e)
        {
            if (NouvelleTacheTextBox.Text == "ENTREZ UNE TACHE....")
                NouvelleTacheTextBox.Text = "";
        }

        private void NouvelleTacheTextBox_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
                AjouterTacheDepuisChamp();
        }

        private void AjouterTacheButton_Click(object sender, RoutedEventArgs e)
        {
            AjouterTacheDepuisChamp();
        }

        private void AjouterTacheDepuisChamp()
        {
            string texte = NouvelleTacheTextBox.Text.Trim();

            if (!string.IsNullOrWhiteSpace(texte) && texte != "ENTREZ UNE TACHE....")
            {
                AjouterTache(texte);
                NouvelleTacheTextBox.Text = string.Empty;
            }
        }

        private void AjouterTache(string texte)
        {
            if (_utilisateur == null || string.IsNullOrWhiteSpace(texte)) return;

            var nouvelle = new Tache { Description = texte };
            _utilisateur.ListeTaches.Add(nouvelle);

            JsonService.SauvegarderUtilisateur(_utilisateur);
            AfficherTaches();
        }

        private void SupprimerTache_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button btn && btn.Tag is Tache tache && _utilisateur != null)
            {
                _utilisateur.ListeTaches.Remove(tache);
                JsonService.SauvegarderUtilisateur(_utilisateur);
                AfficherTaches();
            }
        }

        private void ToggleTacheEtat(object sender, RoutedEventArgs e)
        {
            if (sender is CheckBox checkBox && checkBox.Tag is Tache tache && _utilisateur != null)
            {
                tache.EstTerminee = checkBox.IsChecked == true;
                JsonService.SauvegarderUtilisateur(_utilisateur);
                AfficherTaches();
            }
        }

        private void AfficherTaches()
        {
            TaskListContainer.Children.Clear();

            if (this.FindName("EmptyStateMessage") is Border emptyMessage)
                emptyMessage.Visibility = (_utilisateur?.ListeTaches.Count ?? 0) == 0 ? Visibility.Visible : Visibility.Collapsed;

            if (_utilisateur == null) return;

            foreach (var tache in _utilisateur.ListeTaches)
            {
                var item = CreerTacheElement(tache);
                TaskListContainer.Children.Add(item);
            }
        }

        private UIElement CreerTacheElement(Tache tache)
        {
            var border = new Border
            {
                Background = new SolidColorBrush(Color.FromRgb(42, 42, 42)),
                CornerRadius = new CornerRadius(10),
                Padding = new Thickness(20),
                Margin = new Thickness(0, 0, 0, 15),
                Effect = new System.Windows.Media.Effects.DropShadowEffect
                {
                    Color = Colors.Black,
                    BlurRadius = 8,
                    ShadowDepth = 2,
                    Opacity = 0.3
                }
            };

            var grid = new Grid();
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });

            var checkBox = new CheckBox
            {
                IsChecked = tache.EstTerminee,
                Width = 25,
                Height = 25,
                Margin = new Thickness(0, 0, 15, 0),
                Tag = tache,
                VerticalAlignment = VerticalAlignment.Center
            };

            if (this.Resources["CheckboxStyle"] is Style checkboxStyle)
                checkBox.Style = checkboxStyle;

            checkBox.Checked += ToggleTacheEtat;
            checkBox.Unchecked += ToggleTacheEtat;
            Grid.SetColumn(checkBox, 0);

            var textBlock = new TextBlock
            {
                Text = tache.Description,
                FontSize = 16,
                FontWeight = FontWeights.Bold,
                VerticalAlignment = VerticalAlignment.Center,
                Foreground = tache.EstTerminee
                    ? new SolidColorBrush(Color.FromRgb(136, 136, 136))
                    : Brushes.White,
                TextDecorations = tache.EstTerminee ? TextDecorations.Strikethrough : null
            };
            Grid.SetColumn(textBlock, 1);

            var deleteButton = new Button
            {
                Width = 30,
                Height = 30,
                Background = Brushes.Transparent,
                BorderThickness = new Thickness(0),
                Cursor = Cursors.Hand,
                Tag = tache,
                VerticalAlignment = VerticalAlignment.Center
            };

            deleteButton.Click += SupprimerTache_Click;

            var deleteText = new TextBlock
            {
                Text = "✕",
                FontSize = 16,
                FontWeight = FontWeights.Bold,
                Foreground = new SolidColorBrush(Color.FromRgb(255, 68, 68)),
                HorizontalAlignment = HorizontalAlignment.Center,
                VerticalAlignment = VerticalAlignment.Center
            };

            deleteButton.Content = deleteText;

            if (this.Resources["DeleteButtonStyle"] is Style deleteStyle)
                deleteButton.Style = deleteStyle;

            Grid.SetColumn(deleteButton, 2);

            grid.Children.Add(checkBox);
            grid.Children.Add(textBlock);
            grid.Children.Add(deleteButton);

            border.Child = grid;
            return border;
        }
    }
}
