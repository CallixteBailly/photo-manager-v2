using Avalonia.Data.Converters;
using Avalonia.Media.Imaging;
using System.Globalization;

namespace PhotoManager.UI.Avalonia.Converters;

public class ImageSourceConverter : IValueConverter
{
    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is byte[] imageData && imageData.Length > 0)
        {
            using MemoryStream stream = new(imageData);
            return Bitmap.DecodeToWidth(stream, 200);
        }

        return null!;
    }

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}
