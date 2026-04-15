using Directories = PhotoManager.Tests.Unit.Constants.Directories;
using FileNames = PhotoManager.Tests.Unit.Constants.FileNames;
using PhotoManager.Domain.Enums;

namespace PhotoManager.Tests.Integration.Infrastructure;

[TestFixture]
public class ImageProcessingServiceTests
{
    private string? _dataDirectory;

    private ImageProcessingService? _imageProcessingService;
    private TestLogger<ImageProcessingService>? _testLogger;

    [OneTimeSetUp]
    public void OneTimeSetUp()
    {
        _dataDirectory = Path.Combine(TestContext.CurrentContext.TestDirectory, Directories.TEST_FILES);
    }

    [SetUp]
    public void SetUp()
    {
        _testLogger = new();
        _imageProcessingService = new(_testLogger);
    }

    [TearDown]
    public void TearDown()
    {
        _testLogger!.LoggingAssertTearDown();
    }

    [Test]
    [TestCase(FileNames.IMAGE_8_JPEG)]
    [TestCase(FileNames.IMAGE_1_JPG)]
    public void GetJpegBytes_ValidImage_ReturnsJpegByteArray(string fileName)
    {
        string filePath = Path.Combine(_dataDirectory!, fileName);
        ImageInfo imageInfo = _imageProcessingService!.LoadImageFromPath(filePath, ImageRotation.Rotate0);

        byte[] imageBuffer = _imageProcessingService!.GetJpegBytes(imageInfo);

        Assert.That(imageBuffer, Is.Not.Null);
        Assert.That(imageBuffer, Is.Not.Empty);

        string destinationNewFileDirectory = Path.Combine(_dataDirectory!, Directories.IMAGE_CONVERTED);

        try
        {
            Assert.That(_imageProcessingService.IsValidGdiPlusImage(imageBuffer), Is.True);
            Directory.CreateDirectory(destinationNewFileDirectory);
            string destinationNewFilePath = Path.Combine(destinationNewFileDirectory, FileNames.IMAGE_CONVERTED_JPEG);
            File.WriteAllBytes(destinationNewFilePath, imageBuffer);
            Assert.That(IsValidImage(destinationNewFilePath), Is.True);

            _testLogger!.AssertLogExceptions([], typeof(ImageMetadataService));
        }
        finally
        {
            Directory.Delete(destinationNewFileDirectory, true);
        }
    }

    [Test]
    public void GetJpegBytes_HeicValidImage_ReturnsJpegByteArray()
    {
        string filePath = Path.Combine(_dataDirectory!, FileNames.IMAGE_11_HEIC);
        byte[] buffer = File.ReadAllBytes(filePath);

        ImageInfo imageInfo = _imageProcessingService!.LoadHeicThumbnailImage(buffer, ImageRotation.Rotate0, 100, 100);

        byte[] imageBuffer = _imageProcessingService!.GetJpegBytes(imageInfo);

        Assert.That(imageBuffer, Is.Not.Null);
        Assert.That(imageBuffer, Is.Not.Empty);

        string destinationNewFileDirectory = Path.Combine(_dataDirectory!, Directories.IMAGE_CONVERTED);

        try
        {
            Assert.That(_imageProcessingService.IsValidGdiPlusImage(imageBuffer), Is.True);
            Directory.CreateDirectory(destinationNewFileDirectory);
            string destinationNewFilePath = Path.Combine(destinationNewFileDirectory, FileNames.IMAGE_CONVERTED_JPEG);
            File.WriteAllBytes(destinationNewFilePath, imageBuffer);
            Assert.That(IsValidImage(destinationNewFilePath), Is.True);

            _testLogger!.AssertLogExceptions([], typeof(ImageMetadataService));
        }
        finally
        {
            Directory.Delete(destinationNewFileDirectory, true);
        }
    }

    [Test]
    public void GetJpegBytes_InvalidImage_ThrowsInvalidOperationException()
    {
        ImageInfo imageInfo = new([], 0, 0, ImageRotation.Rotate0);

        InvalidOperationException? exception = Assert.Throws<InvalidOperationException>(() =>
            _imageProcessingService!.GetJpegBytes(imageInfo));

        Assert.That(exception?.Message, Is.EqualTo("Operation is not valid due to the current state of the object."));

        _testLogger!.AssertLogExceptions([], typeof(ImageProcessingService));
    }

    [Test]
    public void GetJpegBytes_NullImage_ThrowsArgumentNullException()
    {
        ImageInfo? invalidImageInfo = null;

        ArgumentNullException? exception = Assert.Throws<ArgumentNullException>(() =>
            _imageProcessingService!.GetJpegBytes(invalidImageInfo!));

        Assert.That(exception?.Message, Is.EqualTo("Value cannot be null. (Parameter 'source')"));

        _testLogger!.AssertLogExceptions([], typeof(ImageProcessingService));
    }

    [Test]
    [TestCase(FileNames.IMAGE_8_JPEG)]
    [TestCase(FileNames.IMAGE_1_JPG)]
    public void GetPngBytes_ValidImage_ReturnsPngByteArray(string fileName)
    {
        string filePath = Path.Combine(_dataDirectory!, fileName);
        ImageInfo imageInfo = _imageProcessingService!.LoadImageFromPath(filePath, ImageRotation.Rotate0);

        byte[] imageBuffer = _imageProcessingService!.GetPngBytes(imageInfo);

        Assert.That(imageBuffer, Is.Not.Null);
        Assert.That(imageBuffer, Is.Not.Empty);

        string destinationNewFileDirectory = Path.Combine(_dataDirectory!, Directories.IMAGE_CONVERTED);

        try
        {
            Assert.That(_imageProcessingService.IsValidGdiPlusImage(imageBuffer), Is.True);
            Directory.CreateDirectory(destinationNewFileDirectory);
            string destinationNewFilePath = Path.Combine(destinationNewFileDirectory, FileNames.IMAGE_CONVERTED_PNG);
            File.WriteAllBytes(destinationNewFilePath, imageBuffer);
            Assert.That(IsValidImage(destinationNewFilePath), Is.True);

            _testLogger!.AssertLogExceptions([], typeof(ImageMetadataService));
        }
        finally
        {
            Directory.Delete(destinationNewFileDirectory, true);
        }
    }

    [Test]
    public void GetPngBytes_HeicValidImage_ReturnsPngByteArray()
    {
        string filePath = Path.Combine(_dataDirectory!, FileNames.IMAGE_11_HEIC);
        byte[] buffer = File.ReadAllBytes(filePath);

        ImageInfo imageInfo = _imageProcessingService!.LoadHeicThumbnailImage(buffer, ImageRotation.Rotate0, 100, 100);

        byte[] imageBuffer = _imageProcessingService!.GetPngBytes(imageInfo);

        Assert.That(imageBuffer, Is.Not.Null);
        Assert.That(imageBuffer, Is.Not.Empty);

        string destinationNewFileDirectory = Path.Combine(_dataDirectory!, Directories.IMAGE_CONVERTED);

        try
        {
            Assert.That(_imageProcessingService.IsValidGdiPlusImage(imageBuffer), Is.True);
            Directory.CreateDirectory(destinationNewFileDirectory);
            string destinationNewFilePath = Path.Combine(destinationNewFileDirectory, FileNames.IMAGE_CONVERTED_PNG);
            File.WriteAllBytes(destinationNewFilePath, imageBuffer);
            Assert.That(IsValidImage(destinationNewFilePath), Is.True);

            _testLogger!.AssertLogExceptions([], typeof(ImageMetadataService));
        }
        finally
        {
            Directory.Delete(destinationNewFileDirectory, true);
        }
    }

    [Test]
    public void GetPngBytes_InvalidImage_ThrowsInvalidOperationException()
    {
        ImageInfo imageInfo = new([], 0, 0, ImageRotation.Rotate0);

        InvalidOperationException? exception =
            Assert.Throws<InvalidOperationException>(() => _imageProcessingService!.GetPngBytes(imageInfo));

        Assert.That(exception?.Message, Is.EqualTo("Operation is not valid due to the current state of the object."));

        _testLogger!.AssertLogExceptions([], typeof(ImageProcessingService));
    }

    [Test]
    public void GetPngBytes_NullImage_ThrowsArgumentNullException()
    {
        ImageInfo? invalidImageInfo = null;

        ArgumentNullException? exception =
            Assert.Throws<ArgumentNullException>(() => _imageProcessingService!.GetPngBytes(invalidImageInfo!));

        Assert.That(exception?.Message, Is.EqualTo("Value cannot be null. (Parameter 'source')"));

        _testLogger!.AssertLogExceptions([], typeof(ImageProcessingService));
    }

    [Test]
    [TestCase(FileNames.IMAGE_8_JPEG)]
    [TestCase(FileNames.IMAGE_1_JPG)]
    public void GetGifBytes_ValidImage_ReturnsGifByteArray(string fileName)
    {
        string filePath = Path.Combine(_dataDirectory!, fileName);
        ImageInfo imageInfo = _imageProcessingService!.LoadImageFromPath(filePath, ImageRotation.Rotate0);

        byte[] imageBuffer = _imageProcessingService!.GetGifBytes(imageInfo);

        Assert.That(imageBuffer, Is.Not.Null);
        Assert.That(imageBuffer, Is.Not.Empty);

        string destinationNewFileDirectory = Path.Combine(_dataDirectory!, Directories.IMAGE_CONVERTED);

        try
        {
            Assert.That(_imageProcessingService.IsValidGdiPlusImage(imageBuffer), Is.True);
            Directory.CreateDirectory(destinationNewFileDirectory);
            string destinationNewFilePath = Path.Combine(destinationNewFileDirectory, FileNames.IMAGE_CONVERTED_GIF);
            File.WriteAllBytes(destinationNewFilePath, imageBuffer);
            Assert.That(IsValidImage(destinationNewFilePath), Is.True);

            _testLogger!.AssertLogExceptions([], typeof(ImageMetadataService));
        }
        finally
        {
            Directory.Delete(destinationNewFileDirectory, true);
        }
    }

    [Test]
    public void GetGifBytes_HeicValidImage_ReturnsGifByteArray()
    {
        string filePath = Path.Combine(_dataDirectory!, FileNames.IMAGE_11_HEIC);
        byte[] buffer = File.ReadAllBytes(filePath);

        ImageInfo imageInfo = _imageProcessingService!.LoadHeicThumbnailImage(buffer, ImageRotation.Rotate0, 100, 100);

        byte[] imageBuffer = _imageProcessingService!.GetGifBytes(imageInfo);

        Assert.That(imageBuffer, Is.Not.Null);
        Assert.That(imageBuffer, Is.Not.Empty);

        string destinationNewFileDirectory = Path.Combine(_dataDirectory!, Directories.IMAGE_CONVERTED);

        try
        {
            Assert.That(_imageProcessingService.IsValidGdiPlusImage(imageBuffer), Is.True);
            Directory.CreateDirectory(destinationNewFileDirectory);
            string destinationNewFilePath = Path.Combine(destinationNewFileDirectory, FileNames.IMAGE_CONVERTED_GIF);
            File.WriteAllBytes(destinationNewFilePath, imageBuffer);
            Assert.That(IsValidImage(destinationNewFilePath), Is.True);

            _testLogger!.AssertLogExceptions([], typeof(ImageMetadataService));
        }
        finally
        {
            Directory.Delete(destinationNewFileDirectory, true);
        }
    }

    [Test]
    public void GetGifBytes_InvalidImage_ThrowsInvalidOperationException()
    {
        ImageInfo imageInfo = new([], 0, 0, ImageRotation.Rotate0);

        InvalidOperationException? exception =
            Assert.Throws<InvalidOperationException>(() => _imageProcessingService!.GetGifBytes(imageInfo));

        Assert.That(exception?.Message, Is.EqualTo("Operation is not valid due to the current state of the object."));

        _testLogger!.AssertLogExceptions([], typeof(ImageProcessingService));
    }

    [Test]
    public void GetGifBytes_NullImage_ThrowsArgumentException()
    {
        ImageInfo? invalidImageInfo = null;

        ArgumentNullException? exception =
            Assert.Throws<ArgumentNullException>(() => _imageProcessingService!.GetGifBytes(invalidImageInfo!));

        Assert.That(exception?.Message, Is.EqualTo("Value cannot be null. (Parameter 'source')"));

        _testLogger!.AssertLogExceptions([], typeof(ImageProcessingService));
    }

    [Test]
    [TestCase(FileNames.IMAGE_1_JPG)]
    [TestCase(FileNames.IMAGE_8_JPEG)]
    [TestCase(FileNames.IMAGE_10_PORTRAIT_PNG)]
    [TestCase(FileNames.HOMER_GIF)]
    [TestCase(FileNames.IMAGE_11_HEIC)]
    public void IsValidGdiPlusImage_ValidImageData_ReturnsTrue(string fileName)
    {
        string filePath = Path.Combine(_dataDirectory!, fileName);
        byte[] validImageData = File.ReadAllBytes(filePath);

        bool result = _imageProcessingService!.IsValidGdiPlusImage(validImageData);

        Assert.That(result, Is.True);

        _testLogger!.AssertLogExceptions([], typeof(ImageProcessingService));
    }

    [Test]
    public void IsValidGdiPlusImage_EmptyImageData_ReturnsFalse()
    {
        byte[] emptyHeicData = [];

        bool result = _imageProcessingService!.IsValidGdiPlusImage(emptyHeicData);

        Assert.That(result, Is.False);

        _testLogger!.AssertLogExceptions(
            [new Exception("No imaging component suitable to complete this operation was found.")],
            typeof(ImageProcessingService));
    }

    [Test]
    public void IsValidHeic_ValidImageData_ReturnsTrue()
    {
        string filePath = Path.Combine(_dataDirectory!, FileNames.IMAGE_11_HEIC);
        byte[] validHeicData = File.ReadAllBytes(filePath);

        bool result = _imageProcessingService!.IsValidHeic(validHeicData);

        Assert.That(result, Is.True);

        _testLogger!.AssertLogExceptions([], typeof(ImageProcessingService));
    }

    [Test]
    public void IsValidHeic_InvalidImageData_ReturnsFalse()
    {
        byte[] invalidHeicData = [0x00, 0x01, 0x02, 0x03];

        bool result = _imageProcessingService!.IsValidHeic(invalidHeicData);

        Assert.That(result, Is.False);

        _testLogger!.AssertLogExceptions([new Exception("The image is not valid or in an unsupported format")],
            typeof(ImageProcessingService));
    }

    [Test]
    public void IsValidHeic_EmptyImageData_ThrowsArgumentException()
    {
        byte[] emptyHeicData = [];

        ArgumentException? exception =
            Assert.Throws<ArgumentException>(() => _imageProcessingService!.IsValidHeic(emptyHeicData));

        Assert.That(exception?.Message, Is.EqualTo("Value cannot be empty. (Parameter 'stream')"));

        _testLogger!.AssertLogExceptions([], typeof(ImageProcessingService));
    }

    private static bool IsValidImage(string filePath)
    {
        try
        {
            using (Image.FromFile(filePath))
            {
                // The image is successfully loaded; consider it valid
                return true;
            }
        }
        catch (Exception)
        {
            // An exception occurred while loading the image; consider it invalid
            return false;
        }
    }
}
