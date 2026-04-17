using ImageMagick;
using Microsoft.Extensions.Logging;

namespace PhotoManager.Common;

public static class BitmapHelper
{
    // From CatalogAssetsService for CreateAsset() to get the originalImage
    /// <summary>
    /// Mimics WPF DecodePixelWidth/DecodePixelHeight behavior:
    /// when one dimension is 0, it is calculated from the other preserving aspect ratio.
    /// When both are 0, original dimensions are used.
    /// </summary>
    private static (int width, int height) CalculateDimensions(uint imageWidth, uint imageHeight, int width, int height)
    {
        if (width <= 0 && height <= 0)
        {
            return ((int)imageWidth, (int)imageHeight);
        }

        if (width <= 0)
        {
            float percentage = height * 100f / imageHeight;
            width = Convert.ToInt32(percentage * imageWidth / 100);
        }
        else if (height <= 0)
        {
            float percentage = width * 100f / imageWidth;
            height = Convert.ToInt32(percentage * imageHeight / 100);
        }

        return (width, height);
    }

    public static ImageInfo LoadOriginalImage(byte[] buffer, ImageRotation rotation, ILogger logger)
    {
        ArgumentNullException.ThrowIfNull(buffer);

        if (buffer.Length == 0)
        {
            throw new ArgumentException("Value cannot be empty. (Parameter 'stream')");
        }

        try
        {
            MagickReadSettings settings = new() { SyncImageWithExifProfile = false };
            using MagickImage magickImage = new(buffer, settings);
            int originalWidth = (int)magickImage.Width;
            int originalHeight = (int)magickImage.Height;
            MagickImageApplyRotation(magickImage, rotation);
            byte[] imageData = magickImage.ToByteArray(MagickFormat.Bmp);
            (int width, int height) = GetRotatedDimensions(originalWidth, originalHeight, rotation);
            return new ImageInfo(imageData, width, height, rotation);
        }
        catch (Exception ex) when (ex is not ArgumentException and not ArgumentNullException and not OverflowException)
        {
            NotSupportedException exception =
                new("No imaging component suitable to complete this operation was found.");
            logger.LogError(exception, "{ExMessage}", exception.Message);
            throw exception;
        }
    }

    // From CatalogAssetsService for CreateAsset() to get the thumbnailImage
    public static ImageInfo LoadThumbnailImage(byte[] buffer, ImageRotation rotation, int width, int height,
        ILogger logger)
    {
        ArgumentNullException.ThrowIfNull(buffer);

        if (buffer.Length == 0)
        {
            throw new ArgumentException("Value cannot be empty. (Parameter 'stream')");
        }

        try
        {
            MagickReadSettings settings = new() { SyncImageWithExifProfile = false };
            using MagickImage magickImage = new(buffer, settings);
            MagickImageApplyRotation(magickImage, rotation);
            (width, height) = CalculateRotatedDimensions(magickImage.Width, magickImage.Height, width, height);
            magickImage.Resize((uint)width, (uint)height);
            byte[] imageData = magickImage.ToByteArray(MagickFormat.Bmp);
            return new ImageInfo(imageData, (int)magickImage.Width, (int)magickImage.Height, rotation);
        }
        catch (Exception ex) when (ex is not ArgumentException and not ArgumentNullException and not OverflowException)
        {
            NotSupportedException exception =
                new("No imaging component suitable to complete this operation was found.");
            logger.LogError(exception, "{ExMessage}", exception.Message);
            throw exception;
        }
    }

    // From CatalogAssetsService for CreateAsset() to get the originalImage for HEIC
    public static ImageInfo LoadHeicOriginalImage(byte[] buffer, ImageRotation rotation, ILogger logger)
    {
        ArgumentNullException.ThrowIfNull(buffer);

        if (buffer.Length == 0)
        {
            throw new ArgumentException("Value cannot be empty. (Parameter 'stream')");
        }

        try
        {
            MagickReadSettings settings = new() { SyncImageWithExifProfile = false };
            using MagickImage magickImage = new(buffer, settings);
            int originalWidth = (int)magickImage.Width;
            int originalHeight = (int)magickImage.Height;
            MagickImageApplyRotation(magickImage, rotation);
            byte[] imageData = magickImage.ToByteArray(MagickFormat.Bmp);
            (int width, int height) = GetRotatedDimensions(originalWidth, originalHeight, rotation);
            return new ImageInfo(imageData, width, height, rotation);
        }
        catch (ArgumentException)
        {
            throw;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "The image is not valid or in an unsupported format");
            return new ImageInfo(null, 0, 0, ImageRotation.Rotate0);
        }
    }

    // From CatalogAssetsService for CreateAsset() to get the thumbnailImage for HEIC
    public static ImageInfo LoadHeicThumbnailImage(byte[] buffer, ImageRotation rotation, int width, int height,
        ILogger logger)
    {
        ArgumentNullException.ThrowIfNull(buffer);

        if (buffer.Length == 0)
        {
            throw new ArgumentException("Value cannot be empty. (Parameter 'stream')");
        }

        try
        {
            MagickReadSettings settings = new() { SyncImageWithExifProfile = false };
            using MagickImage magickImage = new(buffer, settings);
            MagickImageApplyRotation(magickImage, rotation);
            (width, height) = CalculateRotatedDimensions(magickImage.Width, magickImage.Height, width, height);
            magickImage.Resize((uint)width, (uint)height);
            byte[] imageData = magickImage.ToByteArray(MagickFormat.Bmp);
            return new ImageInfo(imageData, (int)magickImage.Width, (int)magickImage.Height, rotation);
        }
        catch (ArgumentException)
        {
            throw;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "The image is not valid or in an unsupported format");
            return new ImageInfo(null, 0, 0, ImageRotation.Rotate0);
        }
    }

    // From ShowImage() in ViewerUserControl to open the image in fullscreen mode
    public static ImageInfo LoadImageFromPath(string imagePath, ImageRotation rotation)
    {
        if (!File.Exists(imagePath))
        {
            return new ImageInfo([], 0, 0, rotation);
        }

        MagickReadSettings settings = new() { SyncImageWithExifProfile = false };
        using MagickImage magickImage = new(imagePath, settings);
        int originalWidth = (int)magickImage.Width;
        int originalHeight = (int)magickImage.Height;
        MagickImageApplyRotation(magickImage, rotation);
        byte[] imageData = magickImage.ToByteArray(MagickFormat.Bmp);
        (int width, int height) = GetRotatedDimensions(originalWidth, originalHeight, rotation);
        return new ImageInfo(imageData, width, height, rotation);
    }

    // From ShowImage() in ViewerUserControl to open the image in fullscreen mode for Heic
    public static ImageInfo LoadHeicImageFromPath(string imagePath, ImageRotation rotation, ILogger logger)
    {
        if (!File.Exists(imagePath))
        {
            return new ImageInfo([], 0, 0, rotation);
        }

        try
        {
            MagickReadSettings settings = new() { SyncImageWithExifProfile = false };
            using MagickImage magickImage = new(imagePath, settings);
            int originalWidth = (int)magickImage.Width;
            int originalHeight = (int)magickImage.Height;
            MagickImageApplyRotation(magickImage, rotation);
            byte[] imageData = magickImage.ToByteArray(MagickFormat.Jpg);
            (int width, int height) = GetRotatedDimensions(originalWidth, originalHeight, rotation);
            return new ImageInfo(imageData, width, height, rotation);
        }
        catch (MagickException ex)
        {
            logger.LogError(ex, "Failed to load HEIC image from path: {imagePath}.", imagePath);
            return new ImageInfo([], 0, 0, rotation);
        }
    }

    // From AssetRepository
    public static ImageInfo LoadThumbnailImage(byte[] buffer, int width, int height, ILogger logger)
    {
        ArgumentNullException.ThrowIfNull(buffer);

        if (buffer.Length == 0)
        {
            throw new NotSupportedException("No imaging component suitable to complete this operation was found.");
        }

        try
        {
            MagickReadSettings settings = new() { SyncImageWithExifProfile = false };
            using MagickImage magickImage = new(buffer, settings);
            (width, height) = CalculateDimensions(magickImage.Width, magickImage.Height, width, height);
            magickImage.Resize((uint)width, (uint)height);
            byte[] imageData = magickImage.ToByteArray(MagickFormat.Bmp);
            return new ImageInfo(imageData, (int)magickImage.Width, (int)magickImage.Height, ImageRotation.Rotate0);
        }
        catch (Exception ex) when (ex is not ArgumentException and not ArgumentNullException and not OverflowException)
        {
            logger.LogError(ex, "No imaging component suitable to complete this operation was found.");
            throw new NotSupportedException("No imaging component suitable to complete this operation was found.");
        }
    }

    public static byte[]? LoadBitmapFromPath(string imagePath)
    {
        if (!File.Exists(imagePath))
        {
            return null;
        }

        MagickReadSettings settings = new() { SyncImageWithExifProfile = false };
        using MagickImage magickImage = new(imagePath, settings);
        return magickImage.ToByteArray(MagickFormat.Jpg);
    }

    public static byte[] GetJpegBytes(ImageInfo imageInfo)
    {
        return GetImageBytes(imageInfo, MagickFormat.Jpg);
    }

    public static byte[] GetPngBytes(ImageInfo imageInfo)
    {
        return GetImageBytes(imageInfo, MagickFormat.Png);
    }

    public static byte[] GetGifBytes(ImageInfo imageInfo)
    {
        return GetImageBytes(imageInfo, MagickFormat.Gif);
    }

    private static byte[] GetImageBytes(ImageInfo imageInfo, MagickFormat format)
    {
        using MagickImage magickImage = new(imageInfo.Data!);
        return magickImage.ToByteArray(format);
    }

    /// <summary>
    /// Mimics WPF DecodePixelWidth/DecodePixelHeight behavior:
    /// when one dimension is negative or 0, it is calculated from the other preserving aspect ratio.
    /// When both are negative or 0, original dimensions are used.
    /// Uses the already-rotated image dimensions (after MagickImageApplyRotation).
    /// </summary>
    private static (int width, int height) CalculateRotatedDimensions(uint imageWidth, uint imageHeight, int width, int height)
    {
        if (width <= 0 && height <= 0)
        {
            return ((int)imageWidth, (int)imageHeight);
        }

        if (width <= 0)
        {
            float percentage = height * 100f / imageHeight;
            width = Convert.ToInt32(percentage * imageWidth / 100);
        }
        else if (height <= 0)
        {
            float percentage = width * 100f / imageWidth;
            height = Convert.ToInt32(percentage * imageHeight / 100);
        }

        return (width, height);
    }

    private static void MagickImageApplyRotation(MagickImage magickImage, ImageRotation rotation)
    {
        int rotationAngle = rotation switch
        {
            ImageRotation.Rotate0 => 0,
            ImageRotation.Rotate90 => 90,
            ImageRotation.Rotate180 => 180,
            ImageRotation.Rotate270 => 270,
            _ => throw new ArgumentException($"'{rotation}' is not a valid value for property 'Rotation'.")
        };

        if (rotationAngle != 0)
        {
            magickImage.Rotate(rotationAngle);
        }
    }

    private static (int Width, int Height) GetRotatedDimensions(int width, int height, ImageRotation rotation)
    {
        return rotation switch
        {
            ImageRotation.Rotate90 => (height, width),
            ImageRotation.Rotate270 => (height, width),
            _ => (width, height)
        };
    }
}
