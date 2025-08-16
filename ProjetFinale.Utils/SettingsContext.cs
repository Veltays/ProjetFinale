
using System.ComponentModel;

namespace ProjetFinale.Utils
{
    public class SettingsContext : INotifyPropertyChanged
    {
        private static readonly SettingsContext _instance = new SettingsContext();
        public static SettingsContext Instance => _instance;

        private WeightUnit _weightUnit = WeightUnit.KG;
        public WeightUnit WeightUnit
        {
            get => _weightUnit;
            set { if (_weightUnit != value) { _weightUnit = value; OnPropertyChanged(nameof(WeightUnit)); } }
        }

        private HeightUnit _heightUnit = HeightUnit.CM;
        public HeightUnit HeightUnit
        {
            get => _heightUnit;
            set { if (_heightUnit != value) { _heightUnit = value; OnPropertyChanged(nameof(HeightUnit)); } }
        }

        public event PropertyChangedEventHandler PropertyChanged;
        private void OnPropertyChanged(string name) => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    }
}