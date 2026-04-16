using Microsoft.Extensions.Logging;

namespace PhotoManager.Infrastructure;

public class ImageProcessingService(ILogger<ImageProcessingService> logger) : IImageProcessingService
{
    // From CatalogAssetsService for CreateAsset() to get the thumbnailImage
    public ImageInfo LoadThumbnailImage(byte[] buffer, ImageRotation rotation, int width, int height)
    {
        return BitmapHelper.LoadThumbnailImage(buffer, rotation, width, height, logger);
    }

    // From AssetRepository
    public ImageInfo LoadThumbnailImage(byte[] buffer, int width, int height)
    {
        return BitmapHelper.LoadThumbnailImage(buffer, width, height, logger);
    }

    // From CatalogAssetsService for CreateAsset() to get the originalImage
    public ImageInfo LoadOriginalImage(byte[] buffer, ImageRotation rotation)
    {
        return BitmapHelper.LoadOriginalImage(buffer, rotation, logger);
    }

    // From ShowImage() in ViewerUserControl to open the image in fullscreen mode
    public ImageInfo LoadImageFromPath(string imagePath, ImageRotation rotation)
    {
        return BitmapHelper.LoadImageFromPath(imagePath, rotation);
    }

    // From CatalogAssetsService for CreateAsset() to get the originalImage for HEIC
    public ImageInfo LoadHeicOriginalImage(byte[] imageBytes, ImageRotation rotation)
    {
        return BitmapHelper.LoadHeicOriginalImage(imageBytes, rotation, logger);
    }

    // From CatalogAssetsService for CreateAsset() to get the thumbnailImage for HEIC
    public ImageInfo LoadHeicThumbnailImage(byte[] buffer, ImageRotation rotation, int width, int height)
    {
        return BitmapHelper.LoadHeicThumbnailImage(buffer, rotation, width, height, logger);
    }

    // From ShowImage() in ViewerUserControl to open the image in fullscreen mode for Heic
    public ImageInfo LoadHeicImageFromPath(string imagePath, ImageRotation rotation)
    {
        return BitmapHelper.LoadHeicImageFromPath(imagePath, rotation, logger);
    }

    public byte[] GetJpegBytes(ImageInfo imageInfo)
    {
        return BitmapHelper.GetJpegBytes(imageInfo);
    }

    public byte[] GetPngBytes(ImageInfo imageInfo)
    {
        return BitmapHelper.GetPngBytes(imageInfo);
    }

    public byte[] GetGifBytes(ImageInfo imageInfo)
    {
        return BitmapHelper.GetGifBytes(imageInfo);
    }

    public bool IsValidGdiPlusImage(byte[] imageData)
    {
        return ExifHelper.IsValidGdiPlusImage(imageData, logger);
    }

    public bool IsValidHeic(byte[] imageData)
    {
        return ExifHelper.IsValidHeic(imageData, logger);
    }
}
