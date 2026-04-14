namespace PhotoManager.Domain.Interfaces;

public interface IImageProcessingService
{
    ImageInfo LoadThumbnailImage(byte[] buffer, Enums.ImageRotation rotation, int width, int height);
    ImageInfo LoadThumbnailImage(byte[] buffer, int width, int height);
    ImageInfo LoadOriginalImage(byte[] buffer, Enums.ImageRotation rotation);
    ImageInfo LoadImageFromPath(string imagePath, Enums.ImageRotation rotation);
    ImageInfo LoadHeicOriginalImage(byte[] imageBytes, Enums.ImageRotation rotation);
    ImageInfo LoadHeicThumbnailImage(byte[] buffer, Enums.ImageRotation rotation, int width, int height);
    ImageInfo LoadHeicImageFromPath(string imagePath, Enums.ImageRotation rotation);
    byte[] GetJpegBytes(ImageInfo imageInfo);
    byte[] GetPngBytes(ImageInfo imageInfo);
    byte[] GetGifBytes(ImageInfo imageInfo);
    bool IsValidGdiPlusImage(byte[] imageData);
    bool IsValidHeic(byte[] imageData);
}
