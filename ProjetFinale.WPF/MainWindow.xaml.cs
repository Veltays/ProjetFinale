using ProjetFinale.Services;
using ProjetFinale.WPF;
using System;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Interop;

namespace ProjetFinale.Views
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            // Afficher la page d'accueil au démarrage
            NavigateToAccueil();
        }

        // === RESIZE NATIF AVEC WindowStyle="None" ===
        protected override void OnSourceInitialized(EventArgs e)
        {
            base.OnSourceInitialized(e);

            // Permettre le resize natif même avec WindowStyle="None"
            var hwnd = new WindowInteropHelper(this).Handle;
            var source = HwndSource.FromHwnd(hwnd);
            source.AddHook(WndProc);
        }

        private IntPtr WndProc(IntPtr hwnd, int msg, IntPtr wParam, IntPtr lParam, ref bool handled)
        {
            const int WM_NCHITTEST = 0x0084;
            const int HTCLIENT = 1;
            const int HTLEFT = 10;
            const int HTRIGHT = 11;
            const int HTTOP = 12;
            const int HTTOPLEFT = 13;
            const int HTTOPRIGHT = 14;
            const int HTBOTTOM = 15;
            const int HTBOTTOMRIGHT = 16;
            const int HTBOTTOMLEFT = 17;
            const int HTCAPTION = 2;

            if (msg == WM_NCHITTEST)
            {
                var point = PointFromScreen(new Point(
                    (int)(lParam.ToInt64() & 0xFFFF),
                    (int)((lParam.ToInt64() & 0xFFFF0000) >> 16)));

                // Zone de resize (8 pixels depuis le bord)
                int resizeArea = 8;

                // Vérifier les coins et bords pour le resize
                if (point.X <= resizeArea && point.Y <= resizeArea)
                {
                    handled = true;
                    return new IntPtr(HTTOPLEFT);
                }
                else if (point.X >= ActualWidth - resizeArea && point.Y <= resizeArea)
                {
                    handled = true;
                    return new IntPtr(HTTOPRIGHT);
                }
                else if (point.X <= resizeArea && point.Y >= ActualHeight - resizeArea)
                {
                    handled = true;
                    return new IntPtr(HTBOTTOMLEFT);
                }
                else if (point.X >= ActualWidth - resizeArea && point.Y >= ActualHeight - resizeArea)
                {
                    handled = true;
                    return new IntPtr(HTBOTTOMRIGHT);
                }
                else if (point.X <= resizeArea)
                {
                    handled = true;
                    return new IntPtr(HTLEFT);
                }
                else if (point.Y <= resizeArea)
                {
                    handled = true;
                    return new IntPtr(HTTOP);
                }
                else if (point.X >= ActualWidth - resizeArea)
                {
                    handled = true;
                    return new IntPtr(HTRIGHT);
                }
                else if (point.Y >= ActualHeight - resizeArea)
                {
                    handled = true;
                    return new IntPtr(HTBOTTOM);
                }
            }

            return IntPtr.Zero;
        }

        // === CONTRÔLES FENÊTRE PERSONNALISÉS ===

        private void TitleBar_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ButtonState == MouseButtonState.Pressed)
            {
                // Double-clic pour maximiser/restaurer
                if (e.ClickCount == 2)
                {
                    MaximizeRestoreButton_Click(sender, e);
                }
                else
                {
                    // Simple clic pour déplacer
                    this.DragMove();
                }
            }
        }

        private void MinimizeButton_Click(object sender, RoutedEventArgs e)
        {
            this.WindowState = WindowState.Minimized;
        }

        private void MaximizeRestoreButton_Click(object sender, RoutedEventArgs e)
        {
            if (this.WindowState == WindowState.Maximized)
            {
                this.WindowState = WindowState.Normal;
                MaximizeRestoreIcon.Text = "🗖"; // Icône maximiser
                MaximizeRestoreButton.ToolTip = "Agrandir";
            }
            else
            {
                this.WindowState = WindowState.Maximized;
                MaximizeRestoreIcon.Text = "🗗"; // Icône restaurer
                MaximizeRestoreButton.ToolTip = "Restaurer";
            }
        }

        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        // === NAVIGATION SIDEBAR ===

        private void AccueilButton_Click(object sender, RoutedEventArgs e)
        {
            NavigateToAccueil();
        }

        private void ObjectifsButton_Click(object sender, RoutedEventArgs e)
        {
            ContentFrame.Navigate(new ObjectifPage());
            ContentFrame.Visibility = Visibility.Visible;
        }

        private void ExportsButton_Click(object sender, RoutedEventArgs e)
        {
            ContentFrame.Navigate(new ExportsPage());
            ContentFrame.Visibility = Visibility.Visible;
        }

        private void SettingsButton_Click(object sender, RoutedEventArgs e)
        {
            ContentFrame.Navigate(new SettingsPage());
            ContentFrame.Visibility = Visibility.Visible;
        }

        private void ScheduleButton_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("Page Schedule en cours de développement", "Info",
                           MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private void ExercicesButton_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("Page Exercices en cours de développement", "Info",
                           MessageBoxButton.OK, MessageBoxImage.Information);
        }

        // === MÉTHODES DE NAVIGATION PUBLIQUES (pour AccueilPage) ===

        public void NavigateToAccueil()
        {
            ContentFrame.Navigate(new AccueilPage());
            ContentFrame.Visibility = Visibility.Visible;
        }

        public void NavigateToExercices()
        {
            ExercicesButton_Click(null, null);
        }

        public void NavigateToObjectifs()
        {
            ObjectifsButton_Click(null, null);
        }

        public void NavigateToSchedule()
        {
            ScheduleButton_Click(null, null);
        }

        public void NavigateToSettings()
        {
            SettingsButton_Click(null, null);
        }

        public void NavigateToExports()
        {
            ExportsButton_Click(null, null);
        }


        private void ImportButton_Click(object sender, RoutedEventArgs e)
        {
            ContentFrame.Navigate(new ImportPage());
            ContentFrame.Visibility = Visibility.Visible;
        }

        // Et aussi dans les méthodes publiques :
        public void NavigateToImport()
        {
            ImportButton_Click(null, null);
        }
        // === DÉCONNEXION ===

        private void Logout_Click(object sender, RoutedEventArgs e)
        {
            var result = MessageBox.Show("Êtes-vous sûr de vouloir vous déconnecter ?",
                                        "Déconnexion",
                                        MessageBoxButton.YesNo,
                                        MessageBoxImage.Question);

            if (result == MessageBoxResult.Yes)
            {
                var settingsManager = new SettingsManager();

                var login = new LoginWindow();
                login.Show();
                this.Close();
            }
        }
    }
}