using ImageMagick;
using Microsoft.Extensions.Logging;

namespace PhotoManager.Common;

public static class ExifHelper
{
    // 1: Normal (0 deg rotation)
    // 3: Upside-down (180 deg rotation)
    // 6: Rotated 90 deg clockwise (270 deg counterclockwise)
    // 8: Rotated 90 deg counterclockwise (270 deg clockwise)
    public static ushort GetExifOrientation(byte[] buffer, ushort defaultExifOrientation,
        ushort corruptedImageOrientation, ILogger logger)
    {
        try
        {
            using (MemoryStream stream = new(buffer))
            {
                using (MagickImage image = new(stream))
                {
                    // image.Orientation contains the value from the exif data
                    return GetMagickOrientation(image.Orientation, defaultExifOrientation, corruptedImageOrientation);
                }
            }
        }
        catch (Exception ex)
        {
            // MagickException for corrupted images
            if (ex is MagickException)
            {
                logger.LogError("The image is corrupted");
            }
            else
            {
                logger.LogError(ex, "{ExMessage}", ex.Message);
            }
        }

        return corruptedImageOrientation;
    }

    // 1: Normal (0 deg rotation)
    // 3: Upside-down (180 deg rotation)
    // 6: Rotated 90 deg clockwise (270 deg counterclockwise)
    // 8: Rotated 90 deg counterclockwise (270 deg clockwise)
    public static ushort GetHeicExifOrientation(byte[] buffer, ushort corruptedImageOrientation,
        ILogger logger)
    {
        try
        {
            using (MemoryStream stream = new(buffer))
            {
                MagickReadSettings settings = new();
                settings.SetDefine(MagickFormat.Heic, "preserve-orientation", true);

                using (MagickImage image = new(stream, settings))
                {
                    // image.Orientation contain the value from the exif data -> image.GetAttribute("exif:Orientation")
                    return GetMagickHeicOrientation(image.Orientation, corruptedImageOrientation);
                }
            }
        }
        catch (MagickException)
        {
            logger.LogError("The image is not valid or in an unsupported format");
        }

        return corruptedImageOrientation;
    }

    // (ushort)1 <=> "Horizontal (normal)"
    // (ushort)2 <=> "Mirror horizontal"
    // (ushort)3 <=> "Rotate 180"
    // (ushort)4 <=> "Mirror vertical"
    // (ushort)5 <=> "Mirror horizontal and rotate 270 CW"
    // (ushort)6 <=> "Rotate 90 CW"
    // (ushort)7 <=> "Mirror horizontal and rotate 90 CW"
    // (ushort)8 <=> "Rotate 270 CW"
    public static ImageRotation GetImageRotation(ushort exifOrientation)
    {
        ImageRotation rotation = exifOrientation switch
        {
            1 => ImageRotation.Rotate0,
            2 => ImageRotation.Rotate0, // FlipX
            3 => ImageRotation.Rotate180,
            4 => ImageRotation.Rotate180, // FlipX
            5 => ImageRotation.Rotate90, // FlipX
            6 => ImageRotation.Rotate90,
            7 => ImageRotation.Rotate270, // FlipX
            8 => ImageRotation.Rotate270,
            _ => ImageRotation.Rotate0,
        };

        return rotation;
    }

    public static bool IsValidGdiPlusImage(byte[] imageData, ILogger logger)
    {
        try
        {
            using (MemoryStream ms = new(imageData))
            {
                using (new MagickImage(ms))
                {
                    // Image is valid
                }
            }

            return true;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "{ExMessage}", ex.Message);
            return false;
        }
    }

    public static bool IsValidHeic(byte[] imageData, ILogger logger)
    {
        try
        {
            using (MemoryStream ms = new(imageData))
            {
                using (new MagickImage(ms))
                {
                    // Image is valid
                }
            }

            return true;
        }
        catch (MagickException)
        {
            logger.LogError("The image is not valid or in an unsupported format");
            return false;
        }
    }

    // 1: Normal (0 deg rotation)
    // 3: Upside-down (180 deg rotation)
    // 6: Rotated 90 deg clockwise (270 deg counterclockwise)
    // 8: Rotated 90 deg counterclockwise (270 deg clockwise)
    private static ushort GetMagickOrientation(OrientationType orientationType, ushort defaultExifOrientation,
        ushort corruptedImageOrientation)
    {
        return orientationType switch
        {
            OrientationType.Undefined => defaultExifOrientation,
            (OrientationType.TopLeft or OrientationType.LeftTop) => 1,
            (OrientationType.BottomLeft or OrientationType.LeftBottom) => 8,
            (OrientationType.BottomRight or OrientationType.RightBottom) => 3,
            (OrientationType.TopRight or OrientationType.RightTop) => 6,
            _ => corruptedImageOrientation
        };
    }

    // 1: Normal (0 deg rotation)
    // 3: Upside-down (180 deg rotation)
    // 6: Rotated 90 deg clockwise (270 deg counterclockwise)
    // 8: Rotated 90 deg counterclockwise (270 deg clockwise)
    private static ushort GetMagickHeicOrientation(OrientationType orientationType, ushort corruptedImageOrientation)
    {
        ushort result = orientationType switch
        {
            (OrientationType.TopLeft or OrientationType.LeftTop) => 1,
            (OrientationType.BottomLeft or OrientationType.LeftBottom) => 8,
            (OrientationType.BottomRight or OrientationType.RightBottom) => 3,
            (OrientationType.TopRight or OrientationType.RightTop) => 6,
            _ => corruptedImageOrientation
        };

        return result;
    }
}
