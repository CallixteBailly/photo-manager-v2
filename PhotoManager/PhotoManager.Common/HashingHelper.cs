using ImageMagick;
using Microsoft.Extensions.Logging;
using System.Numerics.Tensors;
using System.Runtime.CompilerServices;
using System.Security.Cryptography;

namespace PhotoManager.Common;

public static class HashingHelper
{
    public static string CalculateHash(byte[] imageBytes)
    {
        Span<byte> hash = stackalloc byte[SHA512.HashSizeInBytes];
        SHA512.HashData(imageBytes, hash);

        return string.Create(128, hash, static (chars, hashBytes) =>
        {
            for (int i = 0; i < hashBytes.Length; i++)
            {
                byte b = hashBytes[i];
                chars[i * 2] = GetHexChar(b >> 4);
                chars[(i * 2) + 1] = GetHexChar(b & 0xF);
            }
        });

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        static char GetHexChar(int value) => (char)(value < 10 ? '0' + value : 'a' + value - 10);
    }

    // Performances are decreased by 6 times with CalculatePHash
    public static string? CalculatePHash(string filePath, ILogger logger)
    {
        try
        {
            using (MagickImage image = new(filePath))
            {
                // Resize the image
                MagickGeometry geometry = new(32, 32);
                image.Resize(geometry);

                // Convert the image to grayscale
                image.Grayscale(PixelIntensityMethod.Average);

                return image.PerceptualHash()!.ToString(); // We want to log the reason if PerceptualHash returns null
            }
        }
        catch (Exception ex) when (ex is MagickBlobErrorException or MagickMissingDelegateErrorException)
        {
            logger.LogError("MagickImage is unable to open image {filePath}.", filePath);
            return null;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to calculate PHash for: {filePath}. Message: {ex.Message}", filePath,
                ex.Message);
            return null;
        }
    }

    // For GIF or some heic file it returns "00000000000000"
    public static string CalculateDHash(string? filePath)
    {
        if (filePath is null)
        {
            throw new ArgumentNullException("path");
        }

        try
        {
            using (MagickImage image = new(filePath))
            {
                // Resize to 9x8 for DHash calculation
                // Must force exact size (ignore aspect ratio) for pixel access to work correctly
                image.Resize(new MagickGeometry(9, 8) { IgnoreAspectRatio = true });

                // Convert to grayscale
                image.Grayscale(PixelIntensityMethod.Average);

                ulong hash = 0UL;
                ulong mask = 1UL;

                // Get pixel data as 2D array of grayscale values
                // After resize, image is exactly 9x8 pixels
                using (IPixelCollection<ushort> pixels = image.GetPixels())
                {
                    for (int y = 0; y < 8; y++)
                    {
                        for (int x = 0; x < 7; x++)
                        {
                            ushort leftPixel = pixels.GetPixel(x, y)[0];
                            ushort rightPixel = pixels.GetPixel(x + 1, y)[0];
                            if (leftPixel < rightPixel)
                            {
                                hash |= mask;
                            }
                            mask <<= 1;
                        }
                    }
                }

                return hash.ToString("x14"); // Always 14 hex chars (lowercase)
            }
        }
        catch (MagickException)
        {
            throw new ArgumentException("Parameter is not valid.");
        }
    }

    public static string CalculateMD5Hash(byte[] imageBytes)
    {
        Span<byte> hash = stackalloc byte[MD5.HashSizeInBytes];
        MD5.HashData(imageBytes, hash);

        return string.Create(32, hash, static (chars, hashBytes) =>
        {
            for (int i = 0; i < hashBytes.Length; i++)
            {
                byte b = hashBytes[i];
                chars[i * 2] = GetHexChar(b >> 4);
                chars[(i * 2) + 1] = GetHexChar(b & 0xF);
            }
        });

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        static char GetHexChar(int value) => (char)(value < 10 ? '0' + value : 'a' + value - 10);
    }

    // The best use is for PHash method, the most accurate
    public static int CalculateHammingDistance(string hash1, string hash2, ILogger logger)
    {
        if (hash1.Length != hash2.Length)
        {
            ArgumentException exception = new(
                $"Input arguments must all have the same length for hamming distance calculation. hash1: {hash1}, hash2: {hash2}");
            logger.LogError(exception, "{ExMessage}", exception.Message);
            throw exception;
        }

        return TensorPrimitives.HammingDistance(hash1.AsSpan(), hash2.AsSpan());
    }
}
