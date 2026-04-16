namespace PhotoManager.Domain.Interfaces;

public interface IImageProcessingService
{
    ImageInfo LoadThumbnailImage(byte[] buffer, ImageRotation rotation, int width, int height);
    ImageInfo LoadThumbnailImage(byte[] buffer, int width, int height);
    ImageInfo LoadOriginalImage(byte[] buffer, ImageRotation rotation);
    ImageInfo LoadImageFromPath(string imagePath, ImageRotation rotation);
    ImageInfo LoadHeicOriginalImage(byte[] imageBytes, ImageRotation rotation);
    ImageInfo LoadHeicThumbnailImage(byte[] buffer, ImageRotation rotation, int width, int height);
    ImageInfo LoadHeicImageFromPath(string imagePath, ImageRotation rotation);
    byte[] GetJpegBytes(ImageInfo imageInfo);
    byte[] GetPngBytes(ImageInfo imageInfo);
    byte[] GetGifBytes(ImageInfo imageInfo);
    bool IsValidGdiPlusImage(byte[] imageData);
    bool IsValidHeic(byte[] imageData);
}
