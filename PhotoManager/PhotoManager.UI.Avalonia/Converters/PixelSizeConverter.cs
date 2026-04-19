using Avalonia.Data.Converters;
using System.Globalization;

namespace PhotoManager.UI.Avalonia.Converters;

public class PixelSizeConverter : IValueConverter
{
    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is PixelSize pixelSize)
        {
            return $"{pixelSize.Width}x{pixelSize.Height}";
        }

        return string.Empty;
    }

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}
