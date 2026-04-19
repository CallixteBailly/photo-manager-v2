using Avalonia.Data.Converters;
using System.Globalization;

namespace PhotoManager.UI.Avalonia.Converters;

public class FileNameConverter : IValueConverter
{
    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is string fileName && !string.IsNullOrWhiteSpace(fileName))
        {
            return Path.GetFileNameWithoutExtension(fileName);
        }

        return string.Empty;
    }

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}
