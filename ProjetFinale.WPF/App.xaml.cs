using System.Windows;
using ProjetFinale.Views; // important

namespace ProjetFinale
{
    public partial class App : Application
    {
        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            var paramManager = new MyAppParamManager();
            bool isLoggedIn = paramManager.IsLogin;

            MessageBox.Show("IsLogin: " + isLoggedIn); // debug visuel

            Window windowToShow = isLoggedIn ? new MainWindow() : new LoginWindow();
            windowToShow.Show();
        }
    }
}
