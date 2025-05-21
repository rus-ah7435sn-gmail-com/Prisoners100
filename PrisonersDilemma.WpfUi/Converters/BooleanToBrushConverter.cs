using System;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Media;

namespace PrisonersDilemma.WpfUi.Converters
{
    public class BooleanToBrushConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is bool boolValue)
            {
                return boolValue ? Brushes.LightGreen : Brushes.Transparent;
            }
            return Brushes.Transparent; // Default if not a bool
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
