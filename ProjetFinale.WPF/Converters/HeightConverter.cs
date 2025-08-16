using System;
using System.Globalization;
using System.Windows.Data;
using ProjetFinale.Utils;

namespace ProjetFinale.WPF.Converters
{
    public class HeightConverter : IValueConverter
    {
        private const double CmPerInch = 2.54;          // 1 in = 2.54 cm
        private const double InchPerCm = 1.0 / 2.54;    // 1 cm = 0.3937 in

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null) return "";
            double cm;
            try { cm = System.Convert.ToDouble(value, CultureInfo.InvariantCulture); }
            catch { return ""; }

            return SettingsContext.Instance.HeightUnit == HeightUnit.INCH
                ? (cm * InchPerCm).ToString("0.##", CultureInfo.InvariantCulture)
                : cm.ToString("0.##", CultureInfo.InvariantCulture); // CM
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var s = value?.ToString();
            if (string.IsNullOrWhiteSpace(s)) return 0d;

            double entered;
            try { entered = System.Convert.ToDouble(s, CultureInfo.InvariantCulture); }
            catch { return 0d; }

            return SettingsContext.Instance.HeightUnit == HeightUnit.INCH
                ? entered * CmPerInch        // inches -> cm (stockage)
                : entered;                    // déjà en cm
        }
    }
}
