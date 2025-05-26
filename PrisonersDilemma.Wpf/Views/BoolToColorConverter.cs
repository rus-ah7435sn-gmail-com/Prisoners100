using System;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Media;

namespace PrisonersDilemma.Wpf.Views;

/// <summary>
/// Конвертирует булево значение в цвет.
/// Используется для подсветки коробок.
/// ConverterParameter может содержать два цвета через двоеточие, например, "Yellow:LightGray",
/// где первый цвет для true, второй для false.
/// </summary>
public class BoolToColorConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        bool boolValue = (bool)value;
        string colorParams = parameter as string ?? "Yellow:Transparent"; // По умолчанию: желтый для true, прозрачный для false

        string[] colors = colorParams.Split(':');
        string trueColorString = colors.Length > 0 ? colors[0] : "Yellow";
        string falseColorString = colors.Length > 1 ? colors[1] : "Transparent";

        try
        {
            Color trueColor = (Color)ColorConverter.ConvertFromString(trueColorString);
            Color falseColor = (Color)ColorConverter.ConvertFromString(falseColorString);
            return new SolidColorBrush(boolValue ? trueColor : falseColor);
        }
        catch (FormatException)
        {
            // В случае ошибки формата цвета, возвращаем цвета по умолчанию
            return new SolidColorBrush(boolValue ? Colors.Yellow : Colors.Transparent);
        }
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}
