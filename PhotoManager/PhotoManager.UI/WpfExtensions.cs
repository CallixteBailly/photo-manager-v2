using PhotoManager.Domain.Enums;
using System.Windows.Media.Imaging;

namespace PhotoManager.UI;

public static class WpfExtensions
{
    public static Rotation ToRotation(this ImageRotation imageRotation) =>
        imageRotation switch
        {
            ImageRotation.Rotate0 => Rotation.Rotate0,
            ImageRotation.Rotate90 => Rotation.Rotate90,
            ImageRotation.Rotate180 => Rotation.Rotate180,
            ImageRotation.Rotate270 => Rotation.Rotate270,
            _ => throw new ArgumentOutOfRangeException(nameof(imageRotation), $"Unexpected image rotation value: {imageRotation}")
        };
}
