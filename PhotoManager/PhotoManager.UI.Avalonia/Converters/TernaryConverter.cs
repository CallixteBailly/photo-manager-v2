using Avalonia.Data.Converters;
using System.Globalization;

namespace PhotoManager.UI.Avalonia.Converters;

public class TernaryConverter : IValueConverter
{
    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (parameter is not string param)
        {
            return false;
        }

        string[] parts = param.Split('|');
        if (parts.Length != 2)
        {
            return false;
        }

        string trueValue = parts[0];
        string falseValue = parts[1];

        if (value is not null && value.Equals(trueValue))
        {
            return true;
        }

        if (value is not null && value.Equals(falseValue))
        {
            return false;
        }

        return false;
    }

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}
