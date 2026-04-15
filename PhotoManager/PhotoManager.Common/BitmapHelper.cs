using ImageMagick;
using Microsoft.Extensions.Logging;
using PhotoManager.Domain;

namespace PhotoManager.Common;

public static class BitmapHelper
{
    // From CatalogAssetsService for CreateAsset() to get the originalImage
    public static ImageInfo LoadOriginalImage(byte[] buffer, Enums.ImageRotation rotation, ILogger logger)
    {
        try
        {
            using MagickImage magickImage = new(buffer);
            MagickImageApplyRotation(magickImage, rotation);
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

    // From CatalogAssetsService for CreateAsset() to get the thumbnailImage
    public static ImageInfo LoadThumbnailImage(byte[] buffer, Enums.ImageRotation rotation, int width, int height,
        ILogger logger)
    {
        try
        {
            using MagickImage magickImage = new(buffer);
            MagickImageApplyRotation(magickImage, rotation);
            magickImage.Resize((uint)width, (uint)height);
            byte[] imageData = magickImage.ToByteArray(MagickFormat.Bmp);
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

    // From CatalogAssetsService for CreateAsset() to get the originalImage for HEIC
    public static ImageInfo LoadHeicOriginalImage(byte[] buffer, Enums.ImageRotation rotation, ILogger logger)
    {
        try
        {
            using MagickImage magickImage = new(buffer);
            MagickImageApplyRotation(magickImage, rotation);
            byte[] imageData = magickImage.ToByteArray(MagickFormat.Bmp);
            return new ImageInfo(imageData, (int)magickImage.Width, (int)magickImage.Height, rotation);
        }
        catch (MagickException ex)
        {
            logger.LogError(ex, "The image is not valid or in an unsupported format");
            throw;
        }
    }

    // From CatalogAssetsService for CreateAsset() to get the thumbnailImage for HEIC
    public static ImageInfo LoadHeicThumbnailImage(byte[] buffer, Enums.ImageRotation rotation, int width, int height,
        ILogger logger)
    {
        try
        {
            using MagickImage magickImage = new(buffer);
            MagickImageApplyRotation(magickImage, rotation);
            magickImage.Resize((uint)width, (uint)height);
            byte[] imageData = magickImage.ToByteArray(MagickFormat.Bmp);
            return new ImageInfo(imageData, width, height, rotation);
        }
        catch (MagickException ex)
        {
            logger.LogError(ex, "The image is not valid or in an unsupported format");
            throw;
        }
    }

    // From ShowImage() in ViewerUserControl to open the image in fullscreen mode
    public static ImageInfo LoadImageFromPath(string imagePath, Enums.ImageRotation rotation)
    {
        if (!File.Exists(imagePath))
        {
            return new ImageInfo([], 0, 0, rotation);
        }

        using MagickImage magickImage = new(imagePath);
        MagickImageApplyRotation(magickImage, rotation);
        byte[] imageData = magickImage.ToByteArray(MagickFormat.Bmp);
        return new ImageInfo(imageData, (int)magickImage.Width, (int)magickImage.Height, rotation);
    }

    // From ShowImage() in ViewerUserControl to open the image in fullscreen mode for Heic
    public static ImageInfo LoadHeicImageFromPath(string imagePath, Enums.ImageRotation rotation, ILogger logger)
    {
        if (!File.Exists(imagePath))
        {
            return new ImageInfo([], 0, 0, rotation);
        }

        try
        {
            using MagickImage magickImage = new(imagePath);
            MagickImageApplyRotation(magickImage, rotation);
            byte[] imageData = magickImage.ToByteArray(MagickFormat.Jpg);
            return new ImageInfo(imageData, (int)magickImage.Width, (int)magickImage.Height, rotation);
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
        try
        {
            using MagickImage magickImage = new(buffer);
            magickImage.Resize((uint)width, (uint)height);
            byte[] imageData = magickImage.ToByteArray(MagickFormat.Bmp);
            return new ImageInfo(imageData, width, height, Enums.ImageRotation.Rotate0);
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

        using MagickImage magickImage = new(imagePath);
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
        using MagickImage magickImage = new(imageInfo.Data);
        MagickImageApplyRotation(magickImage, imageInfo.Rotation);
        return magickImage.ToByteArray(format);
    }

    private static void MagickImageApplyRotation(MagickImage magickImage, Enums.ImageRotation rotation)
    {
        int rotationAngle = rotation switch
        {
            Enums.ImageRotation.Rotate90 => 90,
            Enums.ImageRotation.Rotate180 => 180,
            Enums.ImageRotation.Rotate270 => 270,
            _ => 0
        };

        if (rotationAngle != 0)
        {
            magickImage.Rotate(rotationAngle);
        }
    }
}
