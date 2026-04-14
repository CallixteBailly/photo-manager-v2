namespace PhotoManager.Domain;

public record ImageInfo(byte[] Data, int Width, int Height, Enums.ImageRotation Rotation);
