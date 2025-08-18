using System;
using System.Globalization;
using System.Windows.Data;
using ProjetFinale.Utils;

namespace ProjetFinale.WPF.Converters
{
    public class HeightConverter : IValueConverter
    {
        private const double CM_PER_INCH = 2.54;
        private const double INCH_PER_CM = 1.0 / CM_PER_INCH;

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is not double cm) return value;

            bool useInch = SettingsContext.Instance.HeightUnit == HeightUnit.INCH;
            double display = useInch ? cm * INCH_PER_CM : cm;
            int decimals = useInch ? 2 : 1;

            return Math.Round(display, decimals).ToString(culture);
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var s = value?.ToString();
            if (string.IsNullOrWhiteSpace(s)) return Binding.DoNothing;

            if (!double.TryParse(s, NumberStyles.Any, culture, out var input))
                return Binding.DoNothing;

            bool useInch = SettingsContext.Instance.HeightUnit == HeightUnit.INCH;
            return useInch ? input * CM_PER_INCH : input; // toujours stocké en cm
        }
    }
}
