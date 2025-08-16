using System;
using System.Globalization;
using System.Windows.Data;
using ProjetFinale.Utils;

namespace ProjetFinale.WPF.Converters
{
    public class WeightConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is double kg)
                return SettingsContext.Instance.WeightUnit == WeightUnit.LBS
                    ? Math.Round(kg * 2.20462, 2).ToString(culture)
                    : Math.Round(kg, 1).ToString(culture);
            return value;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (double.TryParse(value?.ToString(), NumberStyles.Any, culture, out var input))
                return SettingsContext.Instance.WeightUnit == WeightUnit.LBS
                    ? input / 2.20462
                    : input;
            return 0d;
        }
    }
}