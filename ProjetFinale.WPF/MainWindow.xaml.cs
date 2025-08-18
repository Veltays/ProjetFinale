using ProjetFinale.Services;
using ProjetFinale.Utils;   // SettingsContext si besoin ailleurs
using ProjetFinale.WPF;     // pages
using ProjetFinale.WPF.Pages;
using ProjetFinale.Views;   // LoginWindow
using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Interop;

namespace ProjetFinale.Views
{
    public partial class MainWindow : Window
    {
        private const int ResizeBorder = 8;

        public MainWindow()
        {
            InitializeComponent();
            Loaded += (_, __) => NavigateToAccueil();
            StateChanged += (_, __) => UpdateWindowStateUI();
        }

        // === Redimensionnement natif (WindowStyle=None) ===
        protected override void OnSourceInitialized(EventArgs e)
        {
            base.OnSourceInitialized(e);
            var hwnd = new WindowInteropHelper(this).Handle;
            HwndSource.FromHwnd(hwnd)?.AddHook(WndProc);
        }

        private IntPtr WndProc(IntPtr hwnd, int msg, IntPtr wParam, IntPtr lParam, ref bool handled)
        {
            const int WM_NCHITTEST = 0x0084;
            const int HTLEFT = 10, HTRIGHT = 11, HTTOP = 12, HTTOPLEFT = 13, HTTOPRIGHT = 14, HTBOTTOM = 15, HTBOTTOMRIGHT = 16, HTBOTTOMLEFT = 17;

            if (msg == WM_NCHITTEST)
            {
                var screenX = (short)(lParam.ToInt64() & 0xFFFF);
                var screenY = (short)((lParam.ToInt64() >> 16) & 0xFFFF);
                var p = PointFromScreen(new Point(screenX, screenY));

                bool left = p.X <= ResizeBorder;
                bool right = p.X >= ActualWidth - ResizeBorder;
                bool top = p.Y <= ResizeBorder;
                bool bottom = p.Y >= ActualHeight - ResizeBorder;

                int hit =
                    top && left ? HTTOPLEFT :
                    top && right ? HTTOPRIGHT :
                    bottom && left ? HTBOTTOMLEFT :
                    bottom && right ? HTBOTTOMRIGHT :
                    left ? HTLEFT :
                    right ? HTRIGHT :
                    top ? HTTOP :
                    bottom ? HTBOTTOM : 0;

                if (hit != 0)
                {
                    handled = true;
                    return new IntPtr(hit);
                }
            }
            return IntPtr.Zero;
        }

        // === Barre de titre personnalisée ===
        private void TitleBar_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ButtonState != MouseButtonState.Pressed) return;

            if (e.ClickCount == 2)
                ToggleMaximizeRestore();
            else
                DragMove();
        }

        private void MinimizeButton_Click(object sender, RoutedEventArgs e) => WindowState = WindowState.Minimized;

        private void MaximizeRestoreButton_Click(object sender, RoutedEventArgs e) => ToggleMaximizeRestore();

        private void ToggleMaximizeRestore()
        {
            WindowState = WindowState == WindowState.Maximized ? WindowState.Normal : WindowState.Maximized;
            UpdateWindowStateUI();
        }

        private void UpdateWindowStateUI()
        {
            // Ces éléments viennent du XAML (TextBlock + Button)
            if (MaximizeRestoreIcon != null) MaximizeRestoreIcon.Text = WindowState == WindowState.Maximized ? "🗗" : "🗖";
            if (MaximizeRestoreButton != null) MaximizeRestoreButton.ToolTip = WindowState == WindowState.Maximized ? "Restaurer" : "Agrandir";
        }

        private void CloseButton_Click(object sender, RoutedEventArgs e) => Close();

        // === Navigation (centralisée) ===
        private void ShowPage(Page page)
        {
            ContentFrame.Navigate(page);
            ContentFrame.Visibility = Visibility.Visible;
        }

        private void NavigateTo<T>() where T : Page, new() => ShowPage(new T());

        // Sidebar
        private void AccueilButton_Click(object sender, RoutedEventArgs e) => NavigateToAccueil();
        private void ObjectifsButton_Click(object sender, RoutedEventArgs e) => NavigateTo<ObjectifPage>();
        private void ExportsButton_Click(object sender, RoutedEventArgs e) => NavigateTo<ExportsPage>();
        private void SettingsButton_Click(object sender, RoutedEventArgs e) => NavigateTo<SettingsPage>();
        private void ScheduleButton_Click(object sender, RoutedEventArgs e) => NavigateTo<AgendaPage>();
        private void ExercicesButton_Click(object sender, RoutedEventArgs e) => NavigateTo<ExercicesPage>();
        private void ImportButton_Click(object sender, RoutedEventArgs e) => NavigateTo<ImportPage>();

        // APIs publiques utilisées par d’autres pages
        public void NavigateToAccueil() => NavigateTo<AccueilPage>();
        public void NavigateToExercices() => NavigateTo<ExercicesPage>();
        public void NavigateToObjectifs() => NavigateTo<ObjectifPage>();
        public void NavigateToSchedule() => NavigateTo<AgendaPage>();
        public void NavigateToSettings() => NavigateTo<SettingsPage>();
        public void NavigateToExports() => NavigateTo<ExportsPage>();
        public void NavigateToImport() => NavigateTo<ImportPage>();

        // === Déconnexion ===
        private void Logout_Click(object sender, RoutedEventArgs e)
        {
            var ok = MessageBox.Show(
                "Êtes-vous sûr de vouloir vous déconnecter ?",
                "Déconnexion",
                MessageBoxButton.YesNo,
                MessageBoxImage.Question) == MessageBoxResult.Yes;

            if (!ok) return;

            var login = new LoginWindow();
            login.Show();
            Close();
        }
    }
}
