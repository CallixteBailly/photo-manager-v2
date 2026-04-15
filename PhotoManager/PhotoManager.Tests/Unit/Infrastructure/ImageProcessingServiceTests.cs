using Directories = PhotoManager.Tests.Unit.Constants.Directories;
using FileNames = PhotoManager.Tests.Unit.Constants.FileNames;
using ImageRotation = PhotoManager.Domain.Enums.ImageRotation;
using PixelHeightAsset = PhotoManager.Tests.Unit.Constants.PixelHeightAsset;
using PixelWidthAsset = PhotoManager.Tests.Unit.Constants.PixelWidthAsset;

namespace PhotoManager.Tests.Unit.Infrastructure;

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
    [Category("From CatalogAssetsService for CreateAsset() to get the thumbnailImage")]
    [TestCase(ImageRotation.Rotate0, 100, 100)]
    [TestCase(ImageRotation.Rotate90, 100, 100)]
    [TestCase(ImageRotation.Rotate180, 100, 100)]
    [TestCase(ImageRotation.Rotate270, 100, 100)]
    [TestCase(ImageRotation.Rotate90, 10000, 100)]
    [TestCase(ImageRotation.Rotate90, 100, 10000)]
    [TestCase(ImageRotation.Rotate90, 0, 10000)]
    [TestCase(ImageRotation.Rotate90, 100, 0)]
    [TestCase(ImageRotation.Rotate90, 0, 0)]
    // [TestCase(null, 100, 100)]
    // [TestCase(ImageRotation.Rotate90, null, 100)]
    // [TestCase(ImageRotation.Rotate90, 100, null)]
    // [TestCase(ImageRotation.Rotate90, null, null)]
    [TestCase(ImageRotation.Rotate90, -100, 100)]
    [TestCase(ImageRotation.Rotate90, 100, -100)]
    [TestCase(ImageRotation.Rotate90, -100, -100)]
    [TestCase(ImageRotation.Rotate0, 1000000, 100)]
    [TestCase(ImageRotation.Rotate0, 100, 1000000)]
    // [TestCase(null, 100, null)]
    // [TestCase(null, null, 100)]
    // [TestCase(null, null, null)]
    public void LoadThumbnailImage_ValidBufferAndRotationAndWidthAndHeight_ReturnsImageInfo(ImageRotation rotation,
        int width, int height)
    {
        string filePath = Path.Combine(_dataDirectory!, FileNames.IMAGE_1_JPG);
        byte[] buffer = File.ReadAllBytes(filePath);

        ImageInfo image = _imageProcessingService!.LoadThumbnailImage(buffer, rotation, width, height);

        Assert.That(image, Is.Not.Null);
        Assert.That(image.Data, Is.Not.Null);
        Assert.That(image.Rotation, Is.EqualTo(rotation));

        _testLogger!.AssertLogExceptions([], typeof(ImageProcessingService));
    }

    [Test]
    [Category("From CatalogAssetsService for CreateAsset() to get the thumbnailImage")]
    public void LoadThumbnailImage_LargeWidthAndHeight_ThrowsOverflowException()
    {
        string filePath = Path.Combine(_dataDirectory!, FileNames.IMAGE_1_JPG);
        byte[] buffer = File.ReadAllBytes(filePath);

        OverflowException? exception = Assert.Throws<OverflowException>(() =>
            _imageProcessingService!.LoadThumbnailImage(buffer, ImageRotation.Rotate0, 1000000, 1000000));

        Assert.That(exception?.Message, Is.EqualTo("The image data generated an overflow during processing."));

        _testLogger!.AssertLogExceptions([], typeof(ImageProcessingService));
    }

    [Test]
    [Category("From CatalogAssetsService for CreateAsset() to get the thumbnailImage")]
    public void LoadThumbnailImage_NullBuffer_ThrowsArgumentNullException()
    {
        byte[]? buffer = null;
        const ImageRotation rotation = ImageRotation.Rotate90;

        ArgumentNullException? exception = Assert.Throws<ArgumentNullException>(() =>
            _imageProcessingService!.LoadThumbnailImage(buffer!, rotation, 100, 100));

        Assert.That(exception?.Message, Is.EqualTo("Value cannot be null. (Parameter 'buffer')"));

        _testLogger!.AssertLogExceptions([], typeof(ImageProcessingService));
    }

    [Test]
    [Category("From CatalogAssetsService for CreateAsset() to get the thumbnailImage")]
    public void LoadThumbnailImage_EmptyBuffer_ThrowsNotSupportedException()
    {
        byte[] buffer = [];
        const ImageRotation rotation = ImageRotation.Rotate90;

        NotSupportedException? exception = Assert.Throws<NotSupportedException>(() =>
            _imageProcessingService!.LoadThumbnailImage(buffer, rotation, 100, 100));

        Assert.That(exception?.Message,
            Is.EqualTo("No imaging component suitable to complete this operation was found."));

        _testLogger!.AssertLogExceptions([exception!], typeof(ImageProcessingService));
    }

    [Test]
    [Category("From CatalogAssetsService for CreateAsset() to get the thumbnailImage")]
    public void LoadThumbnailImage_InvalidBuffer_ThrowsNotSupportedException()
    {
        byte[] buffer = [0x00, 0x01, 0x02, 0x03];
        const ImageRotation rotation = ImageRotation.Rotate90;

        NotSupportedException? exception = Assert.Throws<NotSupportedException>(() =>
            _imageProcessingService!.LoadThumbnailImage(buffer, rotation, 100, 100));

        Assert.That(exception?.Message,
            Is.EqualTo("No imaging component suitable to complete this operation was found."));

        _testLogger!.AssertLogExceptions([exception!], typeof(ImageProcessingService));
    }

    [Test]
    [Category("From CatalogAssetsService for CreateAsset() to get the thumbnailImage")]
    public void LoadThumbnailImage_InvalidRotation_ThrowsArgumentException()
    {
        string filePath = Path.Combine(_dataDirectory!, FileNames.IMAGE_1_JPG);
        byte[] buffer = File.ReadAllBytes(filePath);
        const ImageRotation rotation = (ImageRotation)999;

        ArgumentException? exception = Assert.Throws<ArgumentException>(() =>
            _imageProcessingService!.LoadThumbnailImage(buffer, rotation, 100, 100));

        Assert.That(exception?.Message, Is.EqualTo($"'{rotation}' is not a valid value for property 'Rotation'."));

        _testLogger!.AssertLogExceptions([], typeof(ImageProcessingService));
    }

    // TODO: Migrate from MagickImage to ImageInfo ?
    [Test]
    [Category("From CatalogAssetsService for CreateAsset() to get the thumbnailImage")]
    public void LoadThumbnailImage_HeicImageFormat_ReturnsImageInfo()
    {
        string filePath = Path.Combine(_dataDirectory!, FileNames.IMAGE_11_HEIC);
        byte[] buffer = File.ReadAllBytes(filePath);
        const ImageRotation rotation = ImageRotation.Rotate0;
        const int width = 100;
        const int height = 100;

        ImageInfo image = _imageProcessingService!.LoadThumbnailImage(buffer, rotation, width, height);

        Assert.That(image, Is.Not.Null);
        Assert.That(image.Data, Is.Not.Null);
        Assert.That(image.Rotation, Is.EqualTo(rotation));

        _testLogger!.AssertLogExceptions([], typeof(ImageProcessingService));
    }

    [Test]
    [Category("From AssetRepository")]
    [TestCase(100, 100, 100, 100)]
    [TestCase(10000, 100, 10000, 100)]
    [TestCase(100, 10000, 100, 10000)]
    [TestCase(0, 10000, 17777, 10000)]
    [TestCase(100, 0, 100, 56)]
    [TestCase(0, 0, PixelWidthAsset.IMAGE_1_JPG, PixelHeightAsset.IMAGE_1_JPG)]
    [TestCase(-100, 100, 100, 100)]
    [TestCase(100, -100, 100, 100)]
    [TestCase(-100, -100, 100, 100)]
    [TestCase(1000000, 100, 1000000, 100)]
    [TestCase(100, 1000000, 100, 1000000)]
    // [TestCase(100, null, 100, 56)]
    // [TestCase(null, 100, 177, 100)]
    // [TestCase(null, null, PixelWidthAsset.IMAGE_1_JPG, PixelHeightAsset.IMAGE_1_JPG)]
    public void LoadThumbnailImageAssetRepository_ValidBufferAndWidthAndHeight_ReturnsImageInfo(int width,
        int height, int expectedWidth, int expectedHeight)
    {
        string filePath = Path.Combine(_dataDirectory!, FileNames.IMAGE_1_JPG);
        byte[] buffer = File.ReadAllBytes(filePath);

        ImageInfo image = _imageProcessingService!.LoadThumbnailImage(buffer, width, height);

        Assert.That(image, Is.Not.Null);
        Assert.That(image.Data, Is.Not.Null);
        Assert.That(image.Rotation, Is.EqualTo(ImageRotation.Rotate0));
        Assert.That(image.Width, Is.EqualTo(expectedWidth));
        Assert.That(image.Height, Is.EqualTo(expectedHeight));

        _testLogger!.AssertLogExceptions([], typeof(ImageProcessingService));
    }

    [Test]
    [Category("From AssetRepository")]
    public void LoadThumbnailImageAssetRepository_LargeWidthAndHeight_ThrowsOverflowException()
    {
        string filePath = Path.Combine(_dataDirectory!, FileNames.IMAGE_1_JPG);
        byte[] buffer = File.ReadAllBytes(filePath);

        OverflowException? exception =
            Assert.Throws<OverflowException>(() =>
                _imageProcessingService!.LoadThumbnailImage(buffer, 1000000, 1000000));

        Assert.That(exception?.Message, Is.EqualTo("The image data generated an overflow during processing."));

        _testLogger!.AssertLogExceptions([], typeof(ImageProcessingService));
    }

    [Test]
    [Category("From AssetRepository")]
    public void LoadThumbnailImageAssetRepository_NullBuffer_ThrowsArgumentNullException()
    {
        byte[]? buffer = null;

        ArgumentNullException? exception =
            Assert.Throws<ArgumentNullException>(() =>
                _imageProcessingService!.LoadThumbnailImage(buffer!, 100, 100));

        Assert.That(exception?.Message, Is.EqualTo("Value cannot be null. (Parameter 'buffer')"));

        _testLogger!.AssertLogExceptions([], typeof(ImageProcessingService));
    }

    [Test]
    [Category("From AssetRepository")]
    public void LoadThumbnailImageAssetRepository_EmptyBuffer_ThrowsNotSupportedException()
    {
        byte[] buffer = [];
        const string expectedExceptionMessage = "No imaging component suitable to complete this operation was found.";

        NotSupportedException? exception =
            Assert.Throws<NotSupportedException>(() =>
                _imageProcessingService!.LoadThumbnailImage(buffer, 100, 100));

        Assert.That(exception?.Message, Is.EqualTo(expectedExceptionMessage));

        _testLogger!.AssertLogExceptions([new NotSupportedException(expectedExceptionMessage)],
            typeof(ImageProcessingService));
    }

    [Test]
    [Category("From AssetRepository")]
    public void LoadThumbnailImageAssetRepository_InvalidBuffer_ThrowsNotSupportedException()
    {
        byte[] buffer = [];
        const string expectedExceptionMessage = "No imaging component suitable to complete this operation was found.";

        NotSupportedException? exception =
            Assert.Throws<NotSupportedException>(() =>
                _imageProcessingService!.LoadThumbnailImage(buffer, 100, 100));

        Assert.That(exception?.Message, Is.EqualTo(expectedExceptionMessage));

        _testLogger!.AssertLogExceptions([new NotSupportedException(expectedExceptionMessage)],
            typeof(ImageProcessingService));
    }

    // TODO: Migrate from MagickImage to ImageInfo ?
    [Test]
    [Category("From AssetRepository")]
    public void LoadThumbnailImageAssetRepository_HeicImageFormat_ReturnsImageInfo()
    {
        string filePath = Path.Combine(_dataDirectory!, FileNames.IMAGE_11_HEIC);
        byte[] buffer = File.ReadAllBytes(filePath);
        const int width = 100;
        const int height = 100;

        ImageInfo image = _imageProcessingService!.LoadThumbnailImage(buffer, width, height);

        Assert.That(image, Is.Not.Null);
        Assert.That(image.Data, Is.Not.Null);
        Assert.That(image.Rotation, Is.EqualTo(ImageRotation.Rotate0));

        _testLogger!.AssertLogExceptions([], typeof(ImageProcessingService));
    }

    [Test]
    [Category("From CatalogAssetsService for CreateAsset() to get the originalImage")]
    [TestCase(ImageRotation.Rotate0, PixelWidthAsset.IMAGE_1_JPG, PixelHeightAsset.IMAGE_1_JPG)]
    [TestCase(ImageRotation.Rotate90, PixelHeightAsset.IMAGE_1_JPG, PixelWidthAsset.IMAGE_1_JPG)]
    [TestCase(ImageRotation.Rotate180, PixelWidthAsset.IMAGE_1_JPG, PixelHeightAsset.IMAGE_1_JPG)]
    [TestCase(ImageRotation.Rotate270, PixelHeightAsset.IMAGE_1_JPG, PixelWidthAsset.IMAGE_1_JPG)]
    // [TestCase(null, PixelWidthAsset.IMAGE_1_JPG, PixelHeightAsset.IMAGE_1_JPG)]
    public void LoadOriginalImage_ValidBufferAndRotation_ReturnsImageInfo(ImageRotation rotation,
        int expectedWidth, int expectedHeight)
    {
        string filePath = Path.Combine(_dataDirectory!, FileNames.IMAGE_1_JPG);
        byte[] buffer = File.ReadAllBytes(filePath);

        ImageInfo image = _imageProcessingService!.LoadOriginalImage(buffer, rotation);

        Assert.That(image, Is.Not.Null);
        Assert.That(image.Data, Is.Not.Null);
        Assert.That(image.Rotation, Is.EqualTo(rotation));
        Assert.That(image.Width, Is.EqualTo(expectedWidth));
        Assert.That(image.Height, Is.EqualTo(expectedHeight));

        _testLogger!.AssertLogExceptions([], typeof(ImageProcessingService));
    }

    [Test]
    [Category("From CatalogAssetsService for CreateAsset() to get the originalImage")]
    public void LoadOriginalImage_NullBuffer_ThrowsArgumentNullException()
    {
        byte[]? buffer = null;
        const ImageRotation rotation = ImageRotation.Rotate90;

        ArgumentNullException? exception =
            Assert.Throws<ArgumentNullException>(() =>
                _imageProcessingService!.LoadOriginalImage(buffer!, rotation));

        Assert.That(exception?.Message, Is.EqualTo("Value cannot be null. (Parameter 'buffer')"));

        _testLogger!.AssertLogExceptions([], typeof(ImageProcessingService));
    }

    [Test]
    [Category("From CatalogAssetsService for CreateAsset() to get the originalImage")]
    public void LoadOriginalImage_EmptyBuffer_ThrowsNotSupportedException()
    {
        byte[] buffer = [];
        const ImageRotation rotation = ImageRotation.Rotate90;
        const string expectedExceptionMessage = "No imaging component suitable to complete this operation was found.";

        NotSupportedException? exception =
            Assert.Throws<NotSupportedException>(() =>
                _imageProcessingService!.LoadOriginalImage(buffer, rotation));

        Assert.That(exception?.Message, Is.EqualTo(expectedExceptionMessage));

        _testLogger!.AssertLogExceptions([new NotSupportedException(expectedExceptionMessage)],
            typeof(ImageProcessingService));
    }

    [Test]
    [Category("From CatalogAssetsService for CreateAsset() to get the originalImage")]
    public void LoadOriginalImage_InvalidBuffer_ThrowsNotSupportedException()
    {
        byte[] buffer = [0x00, 0x01, 0x02, 0x03];
        const ImageRotation rotation = ImageRotation.Rotate90;
        const string expectedExceptionMessage = "No imaging component suitable to complete this operation was found.";

        NotSupportedException? exception =
            Assert.Throws<NotSupportedException>(() =>
                _imageProcessingService!.LoadOriginalImage(buffer, rotation));

        Assert.That(exception?.Message, Is.EqualTo(expectedExceptionMessage));

        _testLogger!.AssertLogExceptions([new NotSupportedException(expectedExceptionMessage)],
            typeof(ImageProcessingService));
    }

    [Test]
    [Category("From CatalogAssetsService for CreateAsset() to get the originalImage")]
    public void LoadOriginalImage_InvalidRotation_ThrowsArgumentException()
    {
        string filePath = Path.Combine(_dataDirectory!, FileNames.IMAGE_1_JPG);
        byte[] buffer = File.ReadAllBytes(filePath);
        const ImageRotation rotation = (ImageRotation)999;

        ArgumentException? exception =
            Assert.Throws<ArgumentException>(() => _imageProcessingService!.LoadOriginalImage(buffer, rotation));

        Assert.That(exception?.Message, Is.EqualTo($"'{rotation}' is not a valid value for property 'Rotation'."));

        _testLogger!.AssertLogExceptions([], typeof(ImageProcessingService));
    }

    // TODO: Migrate from MagickImage to ImageInfo ?
    [Test]
    [Category("From CatalogAssetsService for CreateAsset() to get the originalImage")]
    public void LoadOriginalImage_HeicImageFormat_ReturnsImageInfoWithIncorrectData()
    {
        string filePath = Path.Combine(_dataDirectory!, FileNames.IMAGE_11_HEIC);
        byte[] buffer = File.ReadAllBytes(filePath);
        const ImageRotation rotation = ImageRotation.Rotate0;

        ImageInfo image = _imageProcessingService!.LoadOriginalImage(buffer, rotation);

        Assert.That(image, Is.Not.Null);
        Assert.That(image.Data, Is.Not.Null);
        Assert.That(image.Rotation, Is.EqualTo(rotation));
        Assert.That(image.Width,
            Is.EqualTo(PixelHeightAsset.IMAGE_11_HEIC)); // Wrong width (getting the height value instead)
        Assert.That(image.Height, Is.EqualTo(5376)); // Wrong height

        _testLogger!.AssertLogExceptions([], typeof(ImageProcessingService));
    }

    [Test]
    [Category("From ShowImage() in ViewerUserControl to open the image in fullscreen mode")]
    [TestCase(ImageRotation.Rotate0, PixelWidthAsset.IMAGE_1_JPG, PixelHeightAsset.IMAGE_1_JPG)]
    [TestCase(ImageRotation.Rotate90, PixelHeightAsset.IMAGE_1_JPG, PixelWidthAsset.IMAGE_1_JPG)]
    [TestCase(ImageRotation.Rotate180, PixelWidthAsset.IMAGE_1_JPG, PixelHeightAsset.IMAGE_1_JPG)]
    [TestCase(ImageRotation.Rotate270, PixelHeightAsset.IMAGE_1_JPG, PixelWidthAsset.IMAGE_1_JPG)]
    // [TestCase(null, PixelWidthAsset.IMAGE_1_JPG, PixelHeightAsset.IMAGE_1_JPG)]
    public void LoadImageFromPath_ValidRotationAndPath_ReturnsImageInfo(ImageRotation rotation, int expectedWith,
        int expectedHeight)
    {
        string filePath = Path.Combine(_dataDirectory!, FileNames.IMAGE_1_JPG);

        ImageInfo image = _imageProcessingService!.LoadImageFromPath(filePath, rotation);

        Assert.That(image, Is.Not.Null);
        Assert.That(image.Data, Is.Not.Null);
        Assert.That(image.Rotation, Is.EqualTo(rotation));
        Assert.That(image.Width, Is.EqualTo(expectedWith));
        Assert.That(image.Height, Is.EqualTo(expectedHeight));

        _testLogger!.AssertLogExceptions([], typeof(ImageProcessingService));
    }

    [Test]
    [Category("From ShowImage() in ViewerUserControl to open the image in fullscreen mode")]
    public void LoadImageFromPath_ImageDoesNotExist_ReturnsDefaultImageInfo()
    {
        string filePath = Path.Combine(_dataDirectory!, FileNames.NON_EXISTENT_IMAGE_JPG);
        const ImageRotation rotation = ImageRotation.Rotate90;

        ImageInfo image = _imageProcessingService!.LoadImageFromPath(filePath, rotation);

        Assert.That(image, Is.Not.Null);
        Assert.That(image.Data, Is.Not.Null);
        Assert.That(image.Rotation, Is.EqualTo(ImageRotation.Rotate0));
        Assert.That(image.Width, Is.Zero);
        Assert.That(image.Height, Is.Zero);

        _testLogger!.AssertLogExceptions([], typeof(ImageProcessingService));
    }

    [Test]
    [Category("From ShowImage() in ViewerUserControl to open the image in fullscreen mode")]
    public void LoadImageFromPath_FilePathIsNull_ReturnsDefaultImageInfo()
    {
        string? filePath = null;
        const ImageRotation rotation = ImageRotation.Rotate90;

        ImageInfo image = _imageProcessingService!.LoadImageFromPath(filePath!, rotation);

        Assert.That(image, Is.Not.Null);
        Assert.That(image.Data, Is.Not.Null);
        Assert.That(image.Rotation, Is.EqualTo(ImageRotation.Rotate0));
        Assert.That(image.Width, Is.Zero);
        Assert.That(image.Height, Is.Zero);

        _testLogger!.AssertLogExceptions([], typeof(ImageProcessingService));
    }

    [Test]
    [Category("From ShowImage() in ViewerUserControl to open the image in fullscreen mode")]
    public void LoadImageFromPath_InvalidRotation_ThrowsArgumentException()
    {
        string filePath = Path.Combine(_dataDirectory!, FileNames.IMAGE_1_JPG);
        const ImageRotation rotation = (ImageRotation)999;

        ArgumentException? exception =
            Assert.Throws<ArgumentException>(() =>
                _imageProcessingService!.LoadImageFromPath(filePath, rotation));

        Assert.That(exception?.Message, Is.EqualTo($"'{rotation}' is not a valid value for property 'Rotation'."));

        _testLogger!.AssertLogExceptions([], typeof(ImageProcessingService));
    }

    // TODO: Migrate from MagickImage to ImageInfo ?
    [Test]
    [Category("From ShowImage() in ViewerUserControl to open the image in fullscreen mode")]
    public void LoadImageFromPath_HeicImageFormat_ReturnsImageInfo()
    {
        string filePath = Path.Combine(_dataDirectory!, FileNames.IMAGE_11_HEIC);
        const ImageRotation rotation = ImageRotation.Rotate0;

        ImageInfo image = _imageProcessingService!.LoadImageFromPath(filePath, rotation);

        Assert.That(image, Is.Not.Null);
        Assert.That(image.Data, Is.Not.Null);
        Assert.That(image.Rotation, Is.EqualTo(rotation));
        Assert.That(image.Width,
            Is.EqualTo(PixelHeightAsset.IMAGE_11_HEIC)); // Wrong width (getting the height value instead)
        Assert.That(image.Height, Is.EqualTo(5376)); // Wrong height

        _testLogger!.AssertLogExceptions([], typeof(ImageProcessingService));
    }

    [Test]
    [Category("From CatalogAssetsService for CreateAsset() to get the originalImage for HEIC")]
    [TestCase(FileNames.IMAGE_11_HEIC, ImageRotation.Rotate0, ImageRotation.Rotate0,
        PixelWidthAsset.IMAGE_11_HEIC, PixelHeightAsset.IMAGE_11_HEIC)]
    [TestCase(FileNames.IMAGE_11_90_DEG_HEIC, ImageRotation.Rotate90, ImageRotation.Rotate90,
        PixelHeightAsset.IMAGE_11_HEIC, PixelWidthAsset.IMAGE_11_HEIC)]
    [TestCase(FileNames.IMAGE_11_180_DEG_HEIC, ImageRotation.Rotate180, ImageRotation.Rotate180,
        PixelWidthAsset.IMAGE_11_HEIC, PixelHeightAsset.IMAGE_11_HEIC)]
    [TestCase(FileNames.IMAGE_11_270_DEG_HEIC, ImageRotation.Rotate270, ImageRotation.Rotate270,
        PixelHeightAsset.IMAGE_11_HEIC, PixelWidthAsset.IMAGE_11_HEIC)]
    // [TestCase(FileNames.IMAGE_11_HEIC, null, ImageRotation.Rotate0, PixelWidthAsset.IMAGE_11_HEIC,
    //  PixelHeightAsset.IMAGE_11_HEIC)]
    public void LoadHeicOriginalImage_ValidBufferAndRotation_ReturnsImageInfo(string fileName,
        ImageRotation rotation, ImageRotation expectedRotation, int expectedWidth, int expectedHeight)
    {
        string filePath = Path.Combine(_dataDirectory!, fileName);
        byte[] buffer = File.ReadAllBytes(filePath);

        ImageInfo image = _imageProcessingService!.LoadHeicOriginalImage(buffer, rotation);

        Assert.That(image, Is.Not.Null);
        Assert.That(image.Data, Is.Not.Null);
        Assert.That(image.Rotation, Is.EqualTo(expectedRotation));
        Assert.That(image.Width, Is.EqualTo(expectedWidth));
        Assert.That(image.Height, Is.EqualTo(expectedHeight));

        _testLogger!.AssertLogExceptions([], typeof(ImageProcessingService));
    }

    [Test]
    [Category("From CatalogAssetsService for CreateAsset() to get the originalImage for HEIC")]
    public void LoadHeicOriginalImage_NullBuffer_ThrowsArgumentNullException()
    {
        byte[]? buffer = null;
        const ImageRotation rotation = ImageRotation.Rotate90;

        ArgumentNullException? exception =
            Assert.Throws<ArgumentNullException>(() =>
                _imageProcessingService!.LoadHeicOriginalImage(buffer!, rotation));

        Assert.That(exception?.Message, Is.EqualTo("Value cannot be null. (Parameter 'buffer')"));

        _testLogger!.AssertLogExceptions([], typeof(ImageProcessingService));
    }

    [Test]
    [Category("From CatalogAssetsService for CreateAsset() to get the originalImage for HEIC")]
    public void LoadHeicOriginalImage_EmptyBuffer_ThrowsArgumentException()
    {
        byte[] buffer = [];
        const ImageRotation rotation = ImageRotation.Rotate90;

        ArgumentException? exception =
            Assert.Throws<ArgumentException>(() =>
                _imageProcessingService!.LoadHeicOriginalImage(buffer, rotation));

        Assert.That(exception?.Message, Is.EqualTo("Value cannot be empty. (Parameter 'stream')"));

        _testLogger!.AssertLogExceptions([], typeof(ImageProcessingService));
    }

    [Test]
    [Category("From CatalogAssetsService for CreateAsset() to get the originalImage for HEIC")]
    public void LoadHeicOriginalImage_InvalidBuffer_ReturnsDefaultImageInfo()
    {
        byte[] buffer = [0x00, 0x01, 0x02, 0x03];
        const ImageRotation rotation = ImageRotation.Rotate90;

        ImageInfo image = _imageProcessingService!.LoadHeicOriginalImage(buffer, rotation);

        Assert.That(image, Is.Not.Null);
        Assert.That(image.Data, Is.Not.Null);
        Assert.That(image.Rotation, Is.EqualTo(ImageRotation.Rotate0));
        Assert.That(image.Width, Is.Zero);
        Assert.That(image.Height, Is.Zero);

        _testLogger!.AssertLogExceptions([new Exception("The image is not valid or in an unsupported format")],
            typeof(ImageProcessingService));
    }

    [Test]
    [Category("From CatalogAssetsService for CreateAsset() to get the originalImage for HEIC")]
    public void LoadHeicOriginalImage_InvalidRotation_ThrowsArgumentException()
    {
        string filePath = Path.Combine(_dataDirectory!, FileNames.IMAGE_11_HEIC);
        byte[] buffer = File.ReadAllBytes(filePath);
        const ImageRotation rotation = (ImageRotation)999;

        ArgumentException? exception =
            Assert.Throws<ArgumentException>(() =>
                _imageProcessingService!.LoadHeicOriginalImage(buffer, rotation));

        Assert.That(exception?.Message, Is.EqualTo($"'{rotation}' is not a valid value for property 'Rotation'."));

        _testLogger!.AssertLogExceptions([], typeof(ImageProcessingService));
    }

    [Test]
    [Category("From CatalogAssetsService for CreateAsset() to get the thumbnailImage for HEIC")]
    [TestCase(ImageRotation.Rotate0, 100, 100, ImageRotation.Rotate0, 75, 100)]
    [TestCase(ImageRotation.Rotate90, 100, 100, ImageRotation.Rotate90, 75, 100)]
    [TestCase(ImageRotation.Rotate180, 100, 100, ImageRotation.Rotate180, 75, 100)]
    [TestCase(ImageRotation.Rotate270, 100, 100, ImageRotation.Rotate270, 75, 100)]
    [TestCase(ImageRotation.Rotate90, 10000, 100, ImageRotation.Rotate90, 100, 133)]
    [TestCase(ImageRotation.Rotate90, 100, 10000, ImageRotation.Rotate90, 75, 100)]
    [TestCase(ImageRotation.Rotate90, 0, 10000, ImageRotation.Rotate90, 10000, 13333)]
    [TestCase(ImageRotation.Rotate90, 100, 0, ImageRotation.Rotate90, 75, 100)]
    [TestCase(ImageRotation.Rotate90, 0, 0, ImageRotation.Rotate90, 1, 1)]
    // [TestCase(null, 100, 100, ImageRotation.Rotate0, 75, 100)]
    // [TestCase(ImageRotation.Rotate90, null, 100, ImageRotation.Rotate90, 100, 133)]
    // [TestCase(ImageRotation.Rotate90, 100, null, ImageRotation.Rotate90, 75, 100)]
    // [TestCase(ImageRotation.Rotate90, null, null, ImageRotation.Rotate90, 1, 1)]
    [TestCase(ImageRotation.Rotate0, 1000000, 100, ImageRotation.Rotate0, 75, 100)]
    [TestCase(ImageRotation.Rotate0, 100, 1000000, ImageRotation.Rotate0, 100, 133)]
    // [TestCase(null, 100, null, ImageRotation.Rotate0, 100, 133)]
    // [TestCase(null, null, 100, ImageRotation.Rotate0, 75, 100)]
    // [TestCase(null, null, null, ImageRotation.Rotate0, 1, 1)]
    public void LoadHeicThumbnailImage_ValidBufferAndRotation_ReturnsImageInfo(ImageRotation rotation, int width,
        int height, ImageRotation expectedRotation, int expectedWidth, int expectedHeight)
    {
        string filePath = Path.Combine(_dataDirectory!, FileNames.IMAGE_11_HEIC);
        byte[] buffer = File.ReadAllBytes(filePath);

        ImageInfo image = _imageProcessingService!.LoadHeicThumbnailImage(buffer, rotation, width, height);

        Assert.That(image, Is.Not.Null);
        Assert.That(image.Data, Is.Not.Null);
        Assert.That(image.Rotation, Is.EqualTo(expectedRotation));
        Assert.That(image.Width, Is.EqualTo(expectedWidth));
        Assert.That(image.Height, Is.EqualTo(expectedHeight));

        _testLogger!.AssertLogExceptions([], typeof(ImageProcessingService));
    }

    [Test]
    [Category("From CatalogAssetsService for CreateAsset() to get the thumbnailImage for HEIC")]
    [TestCase(FileNames.IMAGE_11_90_DEG_HEIC, ImageRotation.Rotate90, 100, 100, ImageRotation.Rotate90, 100, 75)]
    [TestCase(FileNames.IMAGE_11_180_DEG_HEIC, ImageRotation.Rotate180, 100, 100, ImageRotation.Rotate180, 75, 100)]
    [TestCase(FileNames.IMAGE_11_270_DEG_HEIC, ImageRotation.Rotate270, 100, 100, ImageRotation.Rotate270, 100, 75)]
    public void LoadHeicThumbnailImage_ValidBufferAndRotationAndRotatedImage_ReturnsImageInfo(string fileName,
        ImageRotation rotation, int width, int height, ImageRotation expectedRotation, int expectedWidth,
        int expectedHeight)
    {
        string filePath = Path.Combine(_dataDirectory!, fileName);
        byte[] buffer = File.ReadAllBytes(filePath);

        ImageInfo image = _imageProcessingService!.LoadHeicThumbnailImage(buffer, rotation, width, height);

        Assert.That(image, Is.Not.Null);
        Assert.That(image.Data, Is.Not.Null);
        Assert.That(image.Rotation, Is.EqualTo(expectedRotation));
        Assert.That(image.Width, Is.EqualTo(expectedWidth));
        Assert.That(image.Height, Is.EqualTo(expectedHeight));

        _testLogger!.AssertLogExceptions([], typeof(ImageProcessingService));
    }

    [Test]
    [Category("From CatalogAssetsService for CreateAsset() to get the thumbnailImage for HEIC")]
    [TestCase(-100, 100, 100, 133)]
    [TestCase(100, -100, 75, 100)]
    public void LoadHeicThumbnailImage_InvalidWidthOrHeightOrBoth_ThrowsArgumentException(int width, int height,
        int expectedWidth, int expectedHeight)
    {
        string filePath = Path.Combine(_dataDirectory!, FileNames.IMAGE_11_HEIC);
        byte[] buffer = File.ReadAllBytes(filePath);
        const ImageRotation rotation = ImageRotation.Rotate90;

        ImageInfo image = _imageProcessingService!.LoadHeicThumbnailImage(buffer, rotation, width, height);

        Assert.That(image, Is.Not.Null);
        Assert.That(image.Data, Is.Not.Null);
        Assert.That(image.Rotation, Is.EqualTo(rotation));
        Assert.That(image.Width, Is.EqualTo(expectedWidth));
        Assert.That(image.Height, Is.EqualTo(expectedHeight));

        _testLogger!.AssertLogExceptions([], typeof(ImageProcessingService));
    }

    [Test]
    [Category("From CatalogAssetsService for CreateAsset() to get the thumbnailImage for HEIC")]
    public void LoadHeicThumbnailImage_NegativeWidthAndHeight_ReturnsDefaultImageInfo()
    {
        string filePath = Path.Combine(_dataDirectory!, FileNames.IMAGE_11_HEIC);
        byte[] buffer = File.ReadAllBytes(filePath);

        ImageInfo image =
            _imageProcessingService!.LoadHeicThumbnailImage(buffer, ImageRotation.Rotate90, -100, -100);

        Assert.That(image, Is.Not.Null);
        Assert.That(image.Data, Is.Not.Null);
        Assert.That(image.Rotation, Is.EqualTo(ImageRotation.Rotate0));
        Assert.That(image.Width, Is.Zero);
        Assert.That(image.Height, Is.Zero);

        _testLogger!.AssertLogExceptions([new Exception("The image is not valid or in an unsupported format")],
            typeof(ImageProcessingService));
    }

    [Test]
    [Category("From CatalogAssetsService for CreateAsset() to get the thumbnailImage for HEIC")]
    public void LoadHeicThumbnailImage_LargeWidthAndHeight_ReturnsDefaultImageInfo()
    {
        string filePath = Path.Combine(_dataDirectory!, FileNames.IMAGE_11_HEIC);
        byte[] buffer = File.ReadAllBytes(filePath);
        const ImageRotation rotation = ImageRotation.Rotate90;

        ImageInfo image = _imageProcessingService!.LoadHeicThumbnailImage(buffer, rotation, 1000000, 1000000);

        Assert.That(image, Is.Not.Null);
        Assert.That(image.Data, Is.Not.Null);
        Assert.That(image.Rotation, Is.EqualTo(ImageRotation.Rotate0));
        Assert.That(image.Width, Is.Zero);
        Assert.That(image.Height, Is.Zero);

        _testLogger!.AssertLogExceptions([new Exception("The image is not valid or in an unsupported format")],
            typeof(ImageProcessingService));
    }

    [Test]
    [Category("From CatalogAssetsService for CreateAsset() to get the thumbnailImage for HEIC")]
    public void LoadHeicThumbnailImage_NullBuffer_ThrowsArgumentNullException()
    {
        byte[]? buffer = null;
        const ImageRotation rotation = ImageRotation.Rotate90;

        ArgumentNullException? exception = Assert.Throws<ArgumentNullException>(() =>
            _imageProcessingService!.LoadHeicThumbnailImage(buffer!, rotation, 100, 100));

        Assert.That(exception?.Message, Is.EqualTo("Value cannot be null. (Parameter 'buffer')"));

        _testLogger!.AssertLogExceptions([], typeof(ImageProcessingService));
    }

    [Test]
    [Category("From CatalogAssetsService for CreateAsset() to get the thumbnailImage for HEIC")]
    public void LoadHeicThumbnailImage_EmptyBuffer_ThrowsArgumentException()
    {
        byte[] buffer = [];
        const ImageRotation rotation = ImageRotation.Rotate90;

        ArgumentException? exception = Assert.Throws<ArgumentException>(() =>
            _imageProcessingService!.LoadHeicThumbnailImage(buffer, rotation, 100, 100));

        Assert.That(exception?.Message, Is.EqualTo("Value cannot be empty. (Parameter 'stream')"));

        _testLogger!.AssertLogExceptions([], typeof(ImageProcessingService));
    }

    [Test]
    [Category("From CatalogAssetsService for CreateAsset() to get the thumbnailImage for HEIC")]
    public void LoadHeicThumbnailImage_InvalidBuffer_ReturnsDefaultImageInfo()
    {
        byte[] buffer = [0x00, 0x01, 0x02, 0x03];
        const ImageRotation rotation = ImageRotation.Rotate90;

        ImageInfo image = _imageProcessingService!.LoadHeicThumbnailImage(buffer, rotation, 100, 100);

        Assert.That(image, Is.Not.Null);
        Assert.That(image.Data, Is.Not.Null);
        Assert.That(image.Rotation, Is.EqualTo(ImageRotation.Rotate0));
        Assert.That(image.Width, Is.Zero);
        Assert.That(image.Height, Is.Zero);

        _testLogger!.AssertLogExceptions([new Exception("The image is not valid or in an unsupported format")],
            typeof(ImageProcessingService));
    }

    [Test]
    [Category("From CatalogAssetsService for CreateAsset() to get the thumbnailImage for HEIC")]
    public void LoadHeicThumbnailImage_InvalidRotation_ThrowsArgumentException()
    {
        string filePath = Path.Combine(_dataDirectory!, FileNames.IMAGE_11_HEIC);
        byte[] buffer = File.ReadAllBytes(filePath);
        const ImageRotation rotation = (ImageRotation)999;

        ArgumentException? exception = Assert.Throws<ArgumentException>(() =>
            _imageProcessingService!.LoadHeicThumbnailImage(buffer, rotation, 100, 100));

        Assert.That(exception?.Message, Is.EqualTo($"'{rotation}' is not a valid value for property 'Rotation'."));

        _testLogger!.AssertLogExceptions([], typeof(ImageProcessingService));
    }

    [Test]
    [Category("From ShowImage() in ViewerUserControl to open the image in fullscreen mode for Heic")]
    [TestCase(ImageRotation.Rotate0, PixelWidthAsset.IMAGE_11_HEIC, PixelHeightAsset.IMAGE_11_HEIC)]
    [TestCase(ImageRotation.Rotate90, PixelWidthAsset.IMAGE_11_HEIC, PixelHeightAsset.IMAGE_11_HEIC)]
    [TestCase(ImageRotation.Rotate180, PixelWidthAsset.IMAGE_11_HEIC, PixelHeightAsset.IMAGE_11_HEIC)]
    [TestCase(ImageRotation.Rotate270, PixelWidthAsset.IMAGE_11_HEIC, PixelHeightAsset.IMAGE_11_HEIC)]
    // [TestCase(null, PixelWidthAsset.IMAGE_11_HEIC, PixelHeightAsset.IMAGE_11_HEIC)]
    public void LoadHeicImageFromPathViewerUserControl_ValidPathAndRotationAndNotRotatedImage_ReturnsImageInfo(
        ImageRotation rotation, int expectedWidth, int expectedHeight)
    {
        string filePath = Path.Combine(_dataDirectory!, FileNames.IMAGE_11_HEIC);

        ImageInfo image = _imageProcessingService!.LoadHeicImageFromPath(filePath, rotation);

        Assert.That(image, Is.Not.Null);
        Assert.That(image.Data, Is.Not.Null);
        Assert.That(image.Rotation, Is.EqualTo(rotation));
        Assert.That(image.Width, Is.EqualTo(expectedWidth));
        Assert.That(image.Height, Is.EqualTo(expectedHeight));

        _testLogger!.AssertLogExceptions([], typeof(ImageProcessingService));
    }

    [Test]
    [Category("From ShowImage() in ViewerUserControl to open the image in fullscreen mode for Heic")]
    [TestCase(FileNames.IMAGE_11_90_DEG_HEIC, ImageRotation.Rotate90, PixelWidthAsset.IMAGE_11_90_DEG_HEIC,
        PixelHeightAsset.IMAGE_11_90_DEG_HEIC)]
    [TestCase(FileNames.IMAGE_11_180_DEG_HEIC, ImageRotation.Rotate180, PixelWidthAsset.IMAGE_11_180_DEG_HEIC,
        PixelHeightAsset.IMAGE_11_180_DEG_HEIC)]
    [TestCase(FileNames.IMAGE_11_270_DEG_HEIC, ImageRotation.Rotate270, PixelWidthAsset.IMAGE_11_270_DEG_HEIC,
        PixelHeightAsset.IMAGE_11_270_DEG_HEIC)]
    public void LoadHeicImageFromPathViewerUserControl_ValidPathAndRotationAndRotatedImage_ReturnsImageInfo(
        string fileName, ImageRotation rotation, int expectedWidth, int expectedHeight)
    {
        string filePath = Path.Combine(_dataDirectory!, fileName);

        ImageInfo image = _imageProcessingService!.LoadHeicImageFromPath(filePath, rotation);

        Assert.That(image, Is.Not.Null);
        Assert.That(image.Data, Is.Not.Null);
        Assert.That(image.Rotation, Is.EqualTo(rotation));
        Assert.That(image.Width, Is.EqualTo(expectedWidth));
        Assert.That(image.Height, Is.EqualTo(expectedHeight));

        _testLogger!.AssertLogExceptions([], typeof(ImageProcessingService));
    }

    [Test]
    [Category("From ShowImage() in ViewerUserControl to open the image in fullscreen mode for Heic")]
    public void LoadHeicImageFromPathViewerUserControl_FilePathIsNull_ReturnsImageInfo()
    {
        string? filePath = null;
        const ImageRotation rotation = ImageRotation.Rotate90;

        ImageInfo image = _imageProcessingService!.LoadHeicImageFromPath(filePath!, rotation);

        Assert.That(image, Is.Not.Null);
        Assert.That(image.Data, Is.Not.Null);
        Assert.That(image.Rotation, Is.EqualTo(ImageRotation.Rotate0));
        Assert.That(image.Width, Is.Zero);
        Assert.That(image.Height, Is.Zero);

        _testLogger!.AssertLogExceptions([], typeof(ImageProcessingService));
    }

    [Test]
    [Category("From ShowImage() in ViewerUserControl to open the image in fullscreen mode for Heic")]
    public void LoadHeicImageFromPathViewerUserControl_ImageDoesNotExist_ReturnsDefaultImageInfo()
    {
        string filePath = Path.Combine(_dataDirectory!, FileNames.NON_EXISTENT_IMAGE_HEIC);
        const ImageRotation rotation = ImageRotation.Rotate90;

        ImageInfo image = _imageProcessingService!.LoadHeicImageFromPath(filePath, rotation);

        Assert.That(image, Is.Not.Null);
        Assert.That(image.Data, Is.Not.Null);
        Assert.That(image.Rotation, Is.EqualTo(ImageRotation.Rotate0));
        Assert.That(image.Width, Is.Zero);
        Assert.That(image.Height, Is.Zero);

        _testLogger!.AssertLogExceptions([], typeof(ImageProcessingService));
    }

    [Test]
    [Category("From ShowImage() in ViewerUserControl to open the image in fullscreen mode for Heic")]
    public void LoadHeicImageFromPathViewerUserControl_InvalidRotation_ThrowsArgumentException()
    {
        string filePath = Path.Combine(_dataDirectory!, FileNames.IMAGE_11_HEIC);
        const ImageRotation rotation = (ImageRotation)999;

        ArgumentException? exception =
            Assert.Throws<ArgumentException>(() =>
                _imageProcessingService!.LoadHeicImageFromPath(filePath, rotation));

        Assert.That(exception?.Message, Is.EqualTo($"'{rotation}' is not a valid value for property 'Rotation'."));

        _testLogger!.AssertLogExceptions([], typeof(ImageProcessingService));
    }
}
