using System;
using System.Globalization;
using System.Windows.Data;
using ProjetFinale.Utils;

namespace ProjetFinale.WPF.Converters
{
    public class WeightConverter : IValueConverter
    {
        private const double LBS_PER_KG = 2.20462;

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is not double kg) return value;

            bool inLbs = SettingsContext.Instance.WeightUnit == WeightUnit.LBS;
            double display = inLbs ? kg * LBS_PER_KG : kg;
            int decimals = inLbs ? 2 : 1;

            return Math.Round(display, decimals).ToString(culture);
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var s = value?.ToString();
            if (string.IsNullOrWhiteSpace(s)) return Binding.DoNothing;

            if (!double.TryParse(s, NumberStyles.Any, culture, out var input))
                return Binding.DoNothing;

            bool inLbs = SettingsContext.Instance.WeightUnit == WeightUnit.LBS;
            return inLbs ? input / LBS_PER_KG : input;
        }
    }
}
