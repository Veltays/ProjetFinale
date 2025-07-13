using Microsoft.Win32;

namespace ProjetFinale
{
    public class MyAppParamManager
    {
        private const string RegistryPath = @"Software\ProjetFinale";

        public bool IsLogin
        {
            get
            {
                using var key = Registry.CurrentUser.OpenSubKey(@"Software\ProjetFinale");
                string value = key?.GetValue("IsLogin")?.ToString();
                return string.Equals(value, "true", StringComparison.OrdinalIgnoreCase);
            }
            set
            {
                using var key = Registry.CurrentUser.CreateSubKey(@"Software\ProjetFinale");
                key?.SetValue("IsLogin", value.ToString().ToLower());
            }
        }
    }
}
