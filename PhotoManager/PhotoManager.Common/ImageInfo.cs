namespace PhotoManager.Common;

public record ImageInfo(byte[]? Data, int Width, int Height, ImageRotation Rotation);
