using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace PrisonersDilemma.WpfUi.Converters
{
    public class BooleanToVisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            bool boolValue = false;
            if (value is bool b)
            {
                boolValue = b;
            }

            // Allow inversion of the result if "Invert" or "Inverted" is passed as parameter
            if (parameter is string paramString && (paramString.Equals("Invert", StringComparison.OrdinalIgnoreCase) || paramString.Equals("Inverted", StringComparison.OrdinalIgnoreCase)))
            {
                boolValue = !boolValue;
            }

            return boolValue ? Visibility.Visible : Visibility.Collapsed;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is Visibility visibility)
            {
                bool boolValue = visibility == Visibility.Visible;
                if (parameter is string paramString && (paramString.Equals("Invert", StringComparison.OrdinalIgnoreCase) || paramString.Equals("Inverted", StringComparison.OrdinalIgnoreCase)))
                {
                    boolValue = !boolValue;
                }
                return boolValue;
            }
            return false;
        }
    }
}
