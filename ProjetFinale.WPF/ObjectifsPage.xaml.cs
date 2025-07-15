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
        private List<Tache> _taches = new();

        public ObjectifPage()
        {
            InitializeComponent();
            _taches = JsonService.ChargerTaches();
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
            {
                AjouterTacheDepuisChamp();
            }
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

        private void AfficherTaches()
        {
            TaskListContainer.Children.Clear();

            // Supprimer les exemples statiques du XAML s'ils existent
            foreach (UIElement child in TaskListContainer.Children.OfType<Border>().ToList())
            {
                TaskListContainer.Children.Remove(child);
            }

            // Afficher le message vide si aucune tâche
            if (_taches.Count == 0)
            {
                if (this.FindName("EmptyStateMessage") is Border emptyMessage)
                {
                    emptyMessage.Visibility = Visibility.Visible;
                }
                return;
            }
            else
            {
                if (this.FindName("EmptyStateMessage") is Border emptyMessage)
                {
                    emptyMessage.Visibility = Visibility.Collapsed;
                }
            }

            foreach (var tache in _taches)
            {
                var item = CreerTacheElement(tache);
                TaskListContainer.Children.Add(item);
            }
        }

        private void ToggleTacheEtat(object sender, RoutedEventArgs e)
        {
            if (sender is CheckBox checkBox && checkBox.Tag is Tache tache)
            {
                tache.EstTerminee = checkBox.IsChecked == true;
                JsonService.SauvegarderTaches(_taches);
                AfficherTaches();
            }
        }

        private UIElement CreerTacheElement(Tache tache)
        {
            // Border principal avec le style de la nouvelle interface
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

            // Grid pour organiser les éléments
            var grid = new Grid();
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });

            // CheckBox personnalisée
            var checkBox = new CheckBox
            {
                IsChecked = tache.EstTerminee,
                Width = 25,
                Height = 25,
                Margin = new Thickness(0, 0, 15, 0),
                Tag = tache,
                VerticalAlignment = VerticalAlignment.Center
            };

            // Appliquer le style personnalisé de la checkbox
            if (this.Resources["CheckboxStyle"] is Style checkboxStyle)
            {
                checkBox.Style = checkboxStyle;
            }

            checkBox.Checked += ToggleTacheEtat;
            checkBox.Unchecked += ToggleTacheEtat;
            Grid.SetColumn(checkBox, 0);

            // TextBlock pour le texte de la tâche
            var textBlock = new TextBlock
            {
                Text = tache.Description,
                FontSize = 16,
                FontWeight = FontWeights.Bold,
                VerticalAlignment = VerticalAlignment.Center
            };

            // Appliquer le style selon l'état de la tâche
            if (tache.EstTerminee)
            {
                textBlock.Foreground = new SolidColorBrush(Color.FromRgb(136, 136, 136));
                textBlock.TextDecorations = TextDecorations.Strikethrough;
            }
            else
            {
                textBlock.Foreground = Brushes.White;
            }

            Grid.SetColumn(textBlock, 1);

            // Bouton de suppression
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

            // Contenu du bouton de suppression
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
            deleteButton.Click += SupprimerTache_Click;

            // Appliquer le style du bouton de suppression
            if (this.Resources["DeleteButtonStyle"] is Style deleteStyle)
            {
                deleteButton.Style = deleteStyle;
            }

            Grid.SetColumn(deleteButton, 2);

            // Ajouter tous les éléments au grid
            grid.Children.Add(checkBox);
            grid.Children.Add(textBlock);
            grid.Children.Add(deleteButton);

            border.Child = grid;
            return border;
        }

        private void SupprimerTache_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button btn && btn.Tag is Tache tache)
            {
                _taches.Remove(tache);
                JsonService.SauvegarderTaches(_taches);
                AfficherTaches();
            }
        }

        private void AjouterTache(string texte)
        {
            if (string.IsNullOrWhiteSpace(texte)) return;
            var nouvelle = new Tache { Description = texte };
            _taches.Add(nouvelle);
            JsonService.SauvegarderTaches(_taches);
            AfficherTaches();
        }
    }
}