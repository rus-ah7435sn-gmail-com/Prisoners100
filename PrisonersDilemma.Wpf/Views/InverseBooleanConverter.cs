using System;
using System.Globalization;
using System.Windows.Data;

namespace PrisonersDilemma.Wpf.Views;

/// <summary>
/// Конвертирует булево значение в противоположное.
/// True -> False, False -> True.
/// </summary>
[ValueConversion(typeof(bool), typeof(bool))]
public class InverseBooleanConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is bool boolValue)
        {
            return !boolValue;
        }
        return false; // Или можно выбросить исключение, если значение не булево
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is bool boolValue)
        {
            return !boolValue;
        }
        return false; // Или можно выбросить исключение
    }
}
