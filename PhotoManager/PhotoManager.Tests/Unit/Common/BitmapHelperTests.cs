using Directories = PhotoManager.Tests.Unit.Constants.Directories;
using FileNames = PhotoManager.Tests.Unit.Constants.FileNames;
using PixelHeightAsset = PhotoManager.Tests.Unit.Constants.PixelHeightAsset;
using PixelWidthAsset = PhotoManager.Tests.Unit.Constants.PixelWidthAsset;

namespace PhotoManager.Tests.Unit.Common;

[TestFixture]
public class BitmapHelperTests
{
    private string? _dataDirectory;
    private TestLogger<BitmapHelperTests>? _testLogger;

    [OneTimeSetUp]
    public void OneTimeSetUp()
    {
        _dataDirectory = Path.Combine(TestContext.CurrentContext.TestDirectory, Directories.TEST_FILES);
    }

    [SetUp]
    public void SetUp()
    {
        _testLogger = new();
    }

    [TearDown]
    public void TearDown()
    {
        _testLogger!.LoggingAssertTearDown();
    }

    [Test]
    [Category("From CatalogAssetsService for CreateAsset() to get the originalImage")]
    [TestCase(ImageRotation.Rotate0, PixelWidthAsset.IMAGE_1_JPG, PixelHeightAsset.IMAGE_1_JPG)]
    [TestCase(ImageRotation.Rotate90, PixelHeightAsset.IMAGE_1_JPG, PixelWidthAsset.IMAGE_1_JPG)]
    [TestCase(ImageRotation.Rotate180, PixelWidthAsset.IMAGE_1_JPG, PixelHeightAsset.IMAGE_1_JPG)]
    [TestCase(ImageRotation.Rotate270, PixelHeightAsset.IMAGE_1_JPG, PixelWidthAsset.IMAGE_1_JPG)]
    // [TestCase(null, PixelWidthAsset.IMAGE_1_JPG, PixelHeightAsset.IMAGE_1_JPG)]
    public void LoadBitmapOriginalImage_ValidBufferAndRotation_ReturnsImageInfo(ImageRotation rotation,
        int expectedPixelWidth, int expectedPixelHeight)
    {
        string filePath = Path.Combine(_dataDirectory!, FileNames.IMAGE_1_JPG);
        byte[] buffer = File.ReadAllBytes(filePath);

        ImageInfo image = BitmapHelper.LoadBitmapOriginalImage(buffer, rotation, _testLogger!);

        Assert.That(image, Is.Not.Null);
        Assert.That(image.Data, Is.Not.Null);
        Assert.That(image.Data, Is.Not.Empty);
        Assert.That(image.Rotation, Is.EqualTo(rotation));
        Assert.That(image.Width, Is.EqualTo(expectedPixelWidth));
        Assert.That(image.Height, Is.EqualTo(expectedPixelHeight));

        _testLogger!.AssertLogExceptions([], typeof(BitmapHelperTests));
    }

    [Test]
    [Category("From CatalogAssetsService for CreateAsset() to get the originalImage")]
    public void LoadBitmapOriginalImage_NullBuffer_ThrowsArgumentNullException()
    {
        byte[]? buffer = null;
        const ImageRotation rotation = ImageRotation.Rotate90;

        ArgumentNullException? exception =
            Assert.Throws<ArgumentNullException>(() =>
                BitmapHelper.LoadBitmapOriginalImage(buffer!, rotation, _testLogger!));

        Assert.That(exception?.Message, Is.EqualTo("Value cannot be null. (Parameter 'buffer')"));

        _testLogger!.AssertLogExceptions([], typeof(BitmapHelperTests));
    }

    [Test]
    [Category("From CatalogAssetsService for CreateAsset() to get the originalImage")]
    public void LoadBitmapOriginalImage_EmptyBuffer_ThrowsNotSupportedException()
    {
        byte[] buffer = [];
        const ImageRotation rotation = ImageRotation.Rotate90;

        NotSupportedException? exception =
            Assert.Throws<NotSupportedException>(() =>
                BitmapHelper.LoadBitmapOriginalImage(buffer, rotation, _testLogger!));

        Assert.That(exception?.Message,
            Is.EqualTo("No imaging component suitable to complete this operation was found."));

        _testLogger!.AssertLogExceptions(
            [new NotSupportedException("No imaging component suitable to complete this operation was found.")],
            typeof(BitmapHelperTests));
    }

    [Test]
    [Category("From CatalogAssetsService for CreateAsset() to get the originalImage")]
    public void LoadBitmapOriginalImage_InvalidBuffer_ThrowsNotSupportedException()
    {
        byte[] buffer = [0x00, 0x01, 0x02, 0x03];
        const ImageRotation rotation = ImageRotation.Rotate90;

        NotSupportedException? exception =
            Assert.Throws<NotSupportedException>(() =>
                BitmapHelper.LoadBitmapOriginalImage(buffer, rotation, _testLogger!));

        Assert.That(exception?.Message,
            Is.EqualTo("No imaging component suitable to complete this operation was found."));

        _testLogger!.AssertLogExceptions(
            [new NotSupportedException("No imaging component suitable to complete this operation was found.")],
            typeof(BitmapHelperTests));
    }

    [Test]
    [Category("From CatalogAssetsService for CreateAsset() to get the originalImage")]
    public void LoadBitmapOriginalImage_InvalidRotation_ThrowsArgumentException()
    {
        string filePath = Path.Combine(_dataDirectory!, FileNames.IMAGE_1_JPG);
        byte[] buffer = File.ReadAllBytes(filePath);
        const ImageRotation rotation = (ImageRotation)999;

        ArgumentException? exception =
            Assert.Throws<ArgumentException>(() =>
                BitmapHelper.LoadBitmapOriginalImage(buffer, rotation, _testLogger!));

        Assert.That(exception?.Message, Is.EqualTo($"'{rotation}' is not a valid value for property 'Rotation'."));

        _testLogger!.AssertLogExceptions([], typeof(BitmapHelperTests));
    }

    // TODO: Migrate from MagickImage to BitmapImage ?
    [Test]
    [Category("From CatalogAssetsService for CreateAsset() to get the originalImage")]
    public void LoadBitmapOriginalImage_HeicImageFormat_ReturnsImageInfoWithIncorrectData()
    {
        string filePath = Path.Combine(_dataDirectory!, FileNames.IMAGE_11_HEIC);
        byte[] buffer = File.ReadAllBytes(filePath);
        const ImageRotation rotation = ImageRotation.Rotate0;

        ImageInfo image = BitmapHelper.LoadBitmapOriginalImage(buffer, rotation, _testLogger!);

        using (Assert.EnterMultipleScope())
        {
            Assert.That(image, Is.Not.Null);
            Assert.That(image.Data, Is.Not.Null);
            Assert.That(image.Data, Is.Not.Empty);
            Assert.That(image.Rotation, Is.EqualTo(rotation));
            Assert.That(image.Width,
                Is.EqualTo(PixelHeightAsset.IMAGE_11_HEIC)); // Wrong width (getting the height value instead)
            Assert.That(image.Height, Is.EqualTo(5376)); // Wrong height
        }

        _testLogger!.AssertLogExceptions([], typeof(BitmapHelperTests));
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
    public void LoadBitmapThumbnailImage_ValidBufferAndRotationAndWidthAndHeight_ReturnsImageInfo(ImageRotation rotation,
        int width, int height)
    {
        string filePath = Path.Combine(_dataDirectory!, FileNames.IMAGE_1_JPG);
        byte[] buffer = File.ReadAllBytes(filePath);

        ImageInfo image = BitmapHelper.LoadBitmapThumbnailImage(buffer, rotation, width, height, _testLogger!);

        Assert.That(image, Is.Not.Null);
        Assert.That(image.Data, Is.Not.Null);
        Assert.That(image.Data, Is.Not.Empty);
        Assert.That(image.Rotation, Is.EqualTo(rotation));

        _testLogger!.AssertLogExceptions([], typeof(BitmapHelperTests));
    }

    [Test]
    [Category("From CatalogAssetsService for CreateAsset() to get the thumbnailImage")]
    public void LoadBitmapThumbnailImage_LargeWidthAndHeight_ThrowsOverflowException()
    {
        string filePath = Path.Combine(_dataDirectory!, FileNames.IMAGE_1_JPG);
        byte[] buffer = File.ReadAllBytes(filePath);

        OverflowException? exception = Assert.Throws<OverflowException>(() =>
            BitmapHelper.LoadBitmapThumbnailImage(buffer, ImageRotation.Rotate0, 1000000, 1000000, _testLogger!));

        Assert.That(exception?.Message, Is.EqualTo("The image data generated an overflow during processing."));

        _testLogger!.AssertLogExceptions([], typeof(BitmapHelperTests));
    }

    [Test]
    [Category("From CatalogAssetsService for CreateAsset() to get the thumbnailImage")]
    public void LoadBitmapThumbnailImage_NullBuffer_ThrowsArgumentNullException()
    {
        byte[]? buffer = null;
        const ImageRotation rotation = ImageRotation.Rotate90;

        ArgumentNullException? exception = Assert.Throws<ArgumentNullException>(() =>
            BitmapHelper.LoadBitmapThumbnailImage(buffer!, rotation, 100, 100, _testLogger!));

        Assert.That(exception?.Message, Is.EqualTo("Value cannot be null. (Parameter 'buffer')"));

        _testLogger!.AssertLogExceptions([], typeof(BitmapHelperTests));
    }

    [Test]
    [Category("From CatalogAssetsService for CreateAsset() to get the thumbnailImage")]
    public void LoadBitmapThumbnailImage_EmptyBuffer_ThrowsNotSupportedException()
    {
        byte[] buffer = [];
        const ImageRotation rotation = ImageRotation.Rotate90;

        NotSupportedException? exception =
            Assert.Throws<NotSupportedException>(() =>
                BitmapHelper.LoadBitmapThumbnailImage(buffer, rotation, 100, 100, _testLogger!));

        Assert.That(exception?.Message,
            Is.EqualTo("No imaging component suitable to complete this operation was found."));

        _testLogger!.AssertLogExceptions(
            [new NotSupportedException("No imaging component suitable to complete this operation was found.")],
            typeof(BitmapHelperTests));
    }

    [Test]
    [Category("From CatalogAssetsService for CreateAsset() to get the thumbnailImage")]
    public void LoadBitmapThumbnailImage_InvalidBuffer_ThrowsNotSupportedException()
    {
        byte[] buffer = [0x00, 0x01, 0x02, 0x03];
        const ImageRotation rotation = ImageRotation.Rotate90;

        NotSupportedException? exception =
            Assert.Throws<NotSupportedException>(() =>
                BitmapHelper.LoadBitmapThumbnailImage(buffer, rotation, 100, 100, _testLogger!));

        Assert.That(exception?.Message,
            Is.EqualTo("No imaging component suitable to complete this operation was found."));

        _testLogger!.AssertLogExceptions(
            [new NotSupportedException("No imaging component suitable to complete this operation was found.")],
            typeof(BitmapHelperTests));
    }

    [Test]
    [Category("From CatalogAssetsService for CreateAsset() to get the thumbnailImage")]
    public void LoadBitmapThumbnailImage_InvalidRotation_ThrowsArgumentException()
    {
        string filePath = Path.Combine(_dataDirectory!, FileNames.IMAGE_1_JPG);
        byte[] buffer = File.ReadAllBytes(filePath);
        const ImageRotation rotation = (ImageRotation)999;

        ArgumentException? exception =
            Assert.Throws<ArgumentException>(() =>
                BitmapHelper.LoadBitmapThumbnailImage(buffer, rotation, 100, 100, _testLogger!));

        Assert.That(exception?.Message, Is.EqualTo($"'{rotation}' is not a valid value for property 'Rotation'."));

        _testLogger!.AssertLogExceptions([], typeof(BitmapHelperTests));
    }

    // TODO: Migrate from MagickImage to BitmapImage ?
    [Test]
    [Category("From CatalogAssetsService for CreateAsset() to get the thumbnailImage")]
    public void LoadBitmapThumbnailImage_HeicImageFormat_ReturnsImageInfo()
    {
        string filePath = Path.Combine(_dataDirectory!, FileNames.IMAGE_11_HEIC);
        byte[] buffer = File.ReadAllBytes(filePath);
        const ImageRotation rotation = ImageRotation.Rotate0;
        const int width = 100;
        const int height = 100;

        ImageInfo image = BitmapHelper.LoadBitmapThumbnailImage(buffer, rotation, width, height, _testLogger!);

        using (Assert.EnterMultipleScope())
        {
            Assert.That(image, Is.Not.Null);
            Assert.That(image.Data, Is.Not.Null);
            Assert.That(image.Data, Is.Not.Empty);
            Assert.That(image.Rotation, Is.EqualTo(rotation));
        }

        _testLogger!.AssertLogExceptions([], typeof(BitmapHelperTests));
    }

    [Test]
    [Category("From CatalogAssetsService for CreateAsset() to get the originalImage for HEIC")]
    [TestCase(FileNames.IMAGE_11_HEIC, ImageRotation.Rotate0, ImageRotation.Rotate0, PixelWidthAsset.IMAGE_11_HEIC,
        PixelHeightAsset.IMAGE_11_HEIC)]
    [TestCase(FileNames.IMAGE_11_90_DEG_HEIC, ImageRotation.Rotate90, ImageRotation.Rotate90,
        PixelWidthAsset.IMAGE_11_90_DEG_HEIC, PixelHeightAsset.IMAGE_11_90_DEG_HEIC)]
    [TestCase(FileNames.IMAGE_11_180_DEG_HEIC, ImageRotation.Rotate180, ImageRotation.Rotate180,
        PixelWidthAsset.IMAGE_11_180_DEG_HEIC, PixelHeightAsset.IMAGE_11_180_DEG_HEIC)]
    [TestCase(FileNames.IMAGE_11_270_DEG_HEIC, ImageRotation.Rotate270, ImageRotation.Rotate270,
        PixelWidthAsset.IMAGE_11_270_DEG_HEIC, PixelHeightAsset.IMAGE_11_270_DEG_HEIC)]
    // [TestCase("FileNames.IMAGE_11_HEIC", null, ImageRotation.Rotate0, PixelWidthAsset.IMAGE_11_HEIC, PixelHeightAsset.IMAGE_11_HEIC)]
    public void LoadBitmapHeicOriginalImage_ValidBufferAndRotation_ReturnsImageInfo(string fileName,
        ImageRotation rotation, ImageRotation expectedRotation, int expectedPixelWidth, int expectedPixelHeight)
    {
        string filePath = Path.Combine(_dataDirectory!, fileName);
        byte[] buffer = File.ReadAllBytes(filePath);

        ImageInfo image = BitmapHelper.LoadBitmapHeicOriginalImage(buffer, rotation,
            new TestLogger<BitmapHelperTests>());

        Assert.That(image, Is.Not.Null);
        Assert.That(image.Data, Is.Not.Null);
        Assert.That(image.Data, Is.Not.Empty);
        Assert.That(image.Rotation, Is.EqualTo(expectedRotation));
        Assert.That(image.Width, Is.EqualTo(expectedPixelWidth));
        Assert.That(image.Height, Is.EqualTo(expectedPixelHeight));

        _testLogger!.AssertLogExceptions([], typeof(BitmapHelperTests));
    }

    [Test]
    [Category("From CatalogAssetsService for CreateAsset() to get the originalImage for HEIC")]
    public void LoadBitmapHeicOriginalImage_NullBuffer_ThrowsArgumentNullException()
    {
        byte[]? buffer = null;
        const ImageRotation rotation = ImageRotation.Rotate90;

        ArgumentNullException? exception =
            Assert.Throws<ArgumentNullException>(() =>
                BitmapHelper.LoadBitmapHeicOriginalImage(buffer!, rotation, new TestLogger<BitmapHelperTests>()));

        Assert.That(exception?.Message, Is.EqualTo("Value cannot be null. (Parameter 'buffer')"));

        _testLogger!.AssertLogExceptions([], typeof(BitmapHelperTests));
    }

    [Test]
    [Category("From CatalogAssetsService for CreateAsset() to get the originalImage for HEIC")]
    public void LoadBitmapHeicOriginalImage_EmptyBuffer_ThrowsArgumentException()
    {
        byte[] buffer = [];
        const ImageRotation rotation = ImageRotation.Rotate90;

        ArgumentException? exception =
            Assert.Throws<ArgumentException>(() =>
                BitmapHelper.LoadBitmapHeicOriginalImage(buffer, rotation, new TestLogger<BitmapHelperTests>()));

        Assert.That(exception?.Message, Is.EqualTo("Value cannot be empty. (Parameter 'stream')"));

        _testLogger!.AssertLogExceptions([], typeof(BitmapHelperTests));
    }

    [Test]
    [Category("From CatalogAssetsService for CreateAsset() to get the originalImage for HEIC")]
    public void LoadBitmapHeicOriginalImage_InvalidBuffer_ReturnsDefaultImageInfo()
    {
        byte[] buffer = [0x00, 0x01, 0x02, 0x03];
        const ImageRotation rotation = ImageRotation.Rotate90;

        ImageInfo image = BitmapHelper.LoadBitmapHeicOriginalImage(buffer, rotation,
            new TestLogger<BitmapHelperTests>());

        Assert.That(image, Is.Not.Null);
        Assert.That(image.Data, Is.Null);
        Assert.That(image.Rotation, Is.EqualTo(ImageRotation.Rotate0));

        _testLogger!.AssertLogExceptions([], typeof(BitmapHelperTests));
    }

    [Test]
    [Category("From CatalogAssetsService for CreateAsset() to get the originalImage for HEIC")]
    public void LoadBitmapHeicOriginalImage_InvalidRotation_ThrowsArgumentException()
    {
        string filePath = Path.Combine(_dataDirectory!, FileNames.IMAGE_11_HEIC);
        byte[] buffer = File.ReadAllBytes(filePath);
        const ImageRotation rotation = (ImageRotation)999;

        ArgumentException? exception =
            Assert.Throws<ArgumentException>(() =>
                BitmapHelper.LoadBitmapHeicOriginalImage(buffer, rotation, new TestLogger<BitmapHelperTests>()));

        Assert.That(exception?.Message, Is.EqualTo($"'{rotation}' is not a valid value for property 'Rotation'."));

        _testLogger!.AssertLogExceptions([], typeof(BitmapHelperTests));
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
    public void LoadBitmapHeicThumbnailImage_ValidBufferAndRotationAndNotRotatedImage_ReturnsImageInfo(
        ImageRotation rotation, int width, int height, ImageRotation expectedRotation, int expectedWidth, int expectedHeight)
    {
        string filePath = Path.Combine(_dataDirectory!, FileNames.IMAGE_11_HEIC);
        byte[] buffer = File.ReadAllBytes(filePath);

        ImageInfo image = BitmapHelper.LoadBitmapHeicThumbnailImage(buffer, rotation, width, height,
            new TestLogger<BitmapHelperTests>());

        Assert.That(image, Is.Not.Null);
        Assert.That(image.Data, Is.Not.Null);
        Assert.That(image.Data, Is.Not.Empty);
        Assert.That(image.Rotation, Is.EqualTo(expectedRotation));
        Assert.That(image.Width, Is.EqualTo(expectedWidth));
        Assert.That(image.Height, Is.EqualTo(expectedHeight));

        _testLogger!.AssertLogExceptions([], typeof(BitmapHelperTests));
    }

    [Test]
    [Category("From CatalogAssetsService for CreateAsset() to get the thumbnailImage for HEIC")]
    [TestCase(FileNames.IMAGE_11_90_DEG_HEIC, ImageRotation.Rotate90, 100, 100, ImageRotation.Rotate90, 100, 75)]
    [TestCase(FileNames.IMAGE_11_180_DEG_HEIC, ImageRotation.Rotate180, 100, 100, ImageRotation.Rotate180, 75, 100)]
    [TestCase(FileNames.IMAGE_11_270_DEG_HEIC, ImageRotation.Rotate270, 100, 100, ImageRotation.Rotate270, 100, 75)]
    public void LoadBitmapHeicThumbnailImage_ValidBufferAndRotationAndRotatedImage_ReturnsImageInfo(string fileName,
        ImageRotation rotation, int width, int height, ImageRotation expectedRotation, int expectedWidth, int expectedHeight)
    {
        string filePath = Path.Combine(_dataDirectory!, fileName);
        byte[] buffer = File.ReadAllBytes(filePath);

        ImageInfo image = BitmapHelper.LoadBitmapHeicThumbnailImage(buffer, rotation, width, height,
            new TestLogger<BitmapHelperTests>());

        Assert.That(image, Is.Not.Null);
        Assert.That(image.Data, Is.Not.Null);
        Assert.That(image.Data, Is.Not.Empty);
        Assert.That(image.Rotation, Is.EqualTo(expectedRotation));
        Assert.That(image.Width, Is.EqualTo(expectedWidth));
        Assert.That(image.Height, Is.EqualTo(expectedHeight));

        _testLogger!.AssertLogExceptions([], typeof(BitmapHelperTests));
    }

    [Test]
    [Category("From CatalogAssetsService for CreateAsset() to get the thumbnailImage for HEIC")]
    [TestCase(-100, 100, 100, 133)]
    [TestCase(100, -100, 75, 100)]
    public void LoadBitmapHeicThumbnailImage_NegativeWidthOrHeight_ReturnsImageInfo(int width, int height,
        int expectedWidth, int expectedHeight)
    {
        string filePath = Path.Combine(_dataDirectory!, FileNames.IMAGE_11_HEIC);
        byte[] buffer = File.ReadAllBytes(filePath);
        const ImageRotation rotation = ImageRotation.Rotate90;

        ImageInfo image = BitmapHelper.LoadBitmapHeicThumbnailImage(buffer, rotation, width, height,
            new TestLogger<BitmapHelperTests>());

        Assert.That(image, Is.Not.Null);
        Assert.That(image.Data, Is.Not.Null);
        Assert.That(image.Data, Is.Not.Empty);
        Assert.That(image.Rotation, Is.EqualTo(rotation));
        Assert.That(image.Width, Is.EqualTo(expectedWidth));
        Assert.That(image.Height, Is.EqualTo(expectedHeight));

        _testLogger!.AssertLogExceptions([], typeof(BitmapHelperTests));
    }

    [Test]
    [Category("From CatalogAssetsService for CreateAsset() to get the thumbnailImage for HEIC")]
    public void LoadBitmapHeicThumbnailImage_NegativeWidthAndHeight_ReturnsDefaultImageInfo()
    {
        string filePath = Path.Combine(_dataDirectory!, FileNames.IMAGE_11_HEIC);
        byte[] buffer = File.ReadAllBytes(filePath);

        ImageInfo image = BitmapHelper.LoadBitmapHeicThumbnailImage(buffer, ImageRotation.Rotate90, -100, -100,
            new TestLogger<BitmapHelperTests>());

        Assert.That(image, Is.Not.Null);
        Assert.That(image.Data, Is.Null);
        Assert.That(image.Rotation, Is.EqualTo(ImageRotation.Rotate0));

        _testLogger!.AssertLogExceptions([], typeof(BitmapHelperTests));
    }

    [Test]
    [Category("From CatalogAssetsService for CreateAsset() to get the thumbnailImage for HEIC")]
    public void LoadBitmapHeicThumbnailImage_LargeWidthAndHeight_ReturnsDefaultImageInfo()
    {
        string filePath = Path.Combine(_dataDirectory!, FileNames.IMAGE_11_HEIC);
        byte[] buffer = File.ReadAllBytes(filePath);
        const ImageRotation rotation = ImageRotation.Rotate90;

        ImageInfo image = BitmapHelper.LoadBitmapHeicThumbnailImage(buffer, rotation, 1000000, 1000000,
            new TestLogger<BitmapHelperTests>());

        Assert.That(image, Is.Not.Null);
        Assert.That(image.Data, Is.Null);
        Assert.That(image.Rotation, Is.EqualTo(ImageRotation.Rotate0));

        _testLogger!.AssertLogExceptions([], typeof(BitmapHelperTests));
    }

    [Test]
    [Category("From CatalogAssetsService for CreateAsset() to get the thumbnailImage for HEIC")]
    public void LoadBitmapHeicThumbnailImage_NullBuffer_ThrowsArgumentNullException()
    {
        byte[]? buffer = null;
        const ImageRotation rotation = ImageRotation.Rotate90;

        ArgumentNullException? exception = Assert.Throws<ArgumentNullException>(() =>
            BitmapHelper.LoadBitmapHeicThumbnailImage(buffer!, rotation, 100, 100,
                new TestLogger<BitmapHelperTests>()));

        Assert.That(exception?.Message, Is.EqualTo("Value cannot be null. (Parameter 'buffer')"));

        _testLogger!.AssertLogExceptions([], typeof(BitmapHelperTests));
    }

    [Test]
    [Category("From CatalogAssetsService for CreateAsset() to get the thumbnailImage for HEIC")]
    public void LoadBitmapHeicThumbnailImage_EmptyBuffer_ThrowsArgumentException()
    {
        byte[] buffer = [];
        const ImageRotation rotation = ImageRotation.Rotate90;

        ArgumentException? exception = Assert.Throws<ArgumentException>(() =>
            BitmapHelper.LoadBitmapHeicThumbnailImage(buffer, rotation, 100, 100,
                new TestLogger<BitmapHelperTests>()));

        Assert.That(exception?.Message, Is.EqualTo("Value cannot be empty. (Parameter 'stream')"));

        _testLogger!.AssertLogExceptions([], typeof(BitmapHelperTests));
    }

    [Test]
    [Category("From CatalogAssetsService for CreateAsset() to get the thumbnailImage for HEIC")]
    public void LoadBitmapHeicThumbnailImage_InvalidBuffer_ReturnsDefaultImageInfo()
    {
        byte[] buffer = [0x00, 0x01, 0x02, 0x03];
        const ImageRotation rotation = ImageRotation.Rotate90;

        ImageInfo image = BitmapHelper.LoadBitmapHeicThumbnailImage(buffer, rotation, 100, 100,
            new TestLogger<BitmapHelperTests>());

        Assert.That(image, Is.Not.Null);
        Assert.That(image.Data, Is.Null);
        Assert.That(image.Rotation, Is.EqualTo(ImageRotation.Rotate0));

        _testLogger!.AssertLogExceptions([], typeof(BitmapHelperTests));
    }

    [Test]
    [Category("From CatalogAssetsService for CreateAsset() to get the thumbnailImage for HEIC")]
    public void LoadBitmapHeicThumbnailImage_InvalidRotation_ThrowsArgumentException()
    {
        string filePath = Path.Combine(_dataDirectory!, FileNames.IMAGE_11_HEIC);
        byte[] buffer = File.ReadAllBytes(filePath);
        const ImageRotation rotation = (ImageRotation)999;

        ArgumentException? exception = Assert.Throws<ArgumentException>(() =>
            BitmapHelper.LoadBitmapHeicThumbnailImage(buffer, rotation, 100, 100,
                new TestLogger<BitmapHelperTests>()));

        Assert.That(exception?.Message, Is.EqualTo($"'{rotation}' is not a valid value for property 'Rotation'."));

        _testLogger!.AssertLogExceptions([], typeof(BitmapHelperTests));
    }

    [Test]
    [Category("From ShowImage() in ViewerUserControl to open the image in fullscreen mode")]
    [TestCase(ImageRotation.Rotate0, PixelWidthAsset.IMAGE_1_JPG, PixelHeightAsset.IMAGE_1_JPG)]
    [TestCase(ImageRotation.Rotate90, PixelHeightAsset.IMAGE_1_JPG, PixelWidthAsset.IMAGE_1_JPG)]
    [TestCase(ImageRotation.Rotate180, PixelWidthAsset.IMAGE_1_JPG, PixelHeightAsset.IMAGE_1_JPG)]
    [TestCase(ImageRotation.Rotate270, PixelHeightAsset.IMAGE_1_JPG, PixelWidthAsset.IMAGE_1_JPG)]
    // [TestCase(null, PixelWidthAsset.IMAGE_1_JPG, PixelHeightAsset.IMAGE_1_JPG)]
    public void LoadBitmapImageFromPath_ValidRotationAndPath_ReturnsImageInfo(ImageRotation rotation, int expectedWith,
        int expectedHeight)
    {
        string filePath = Path.Combine(_dataDirectory!, FileNames.IMAGE_1_JPG);

        ImageInfo image = BitmapHelper.LoadBitmapImageFromPath(filePath, rotation);

        Assert.That(image, Is.Not.Null);
        Assert.That(image.Data, Is.Not.Null);
        Assert.That(image.Data, Is.Not.Empty);
        Assert.That(image.Rotation, Is.EqualTo(rotation));
        Assert.That(image.Width, Is.EqualTo(expectedWith));
        Assert.That(image.Height, Is.EqualTo(expectedHeight));

        _testLogger!.AssertLogExceptions([], typeof(BitmapHelperTests));
    }

    [Test]
    [Category("From ShowImage() in ViewerUserControl to open the image in fullscreen mode")]
    public void LoadBitmapImageFromPath_ImageDoesNotExist_ReturnsDefaultImageInfo()
    {
        string filePath = Path.Combine(_dataDirectory!, FileNames.NON_EXISTENT_IMAGE_JPG);
        const ImageRotation rotation = ImageRotation.Rotate90;

        ImageInfo image = BitmapHelper.LoadBitmapImageFromPath(filePath, rotation);

        Assert.That(image, Is.Not.Null);
        Assert.That(image.Data, Is.Null);
        Assert.That(image.Rotation, Is.EqualTo(ImageRotation.Rotate0));

        _testLogger!.AssertLogExceptions([], typeof(BitmapHelperTests));
    }

    [Test]
    [Category("From ShowImage() in ViewerUserControl to open the image in fullscreen mode")]
    public void LoadBitmapImageFromPath_FilePathIsNull_ReturnsDefaultImageInfo()
    {
        string? filePath = null;
        const ImageRotation rotation = ImageRotation.Rotate90;

        ImageInfo image = BitmapHelper.LoadBitmapImageFromPath(filePath!, rotation);

        Assert.That(image, Is.Not.Null);
        Assert.That(image.Data, Is.Null);
        Assert.That(image.Rotation, Is.EqualTo(ImageRotation.Rotate0));

        _testLogger!.AssertLogExceptions([], typeof(BitmapHelperTests));
    }

    [Test]
    [Category("From ShowImage() in ViewerUserControl to open the image in fullscreen mode")]
    public void LoadBitmapImageFromPath_InvalidRotation_ThrowsArgumentException()
    {
        string filePath = Path.Combine(_dataDirectory!, FileNames.IMAGE_1_JPG);
        const ImageRotation rotation = (ImageRotation)999;

        ArgumentException? exception =
            Assert.Throws<ArgumentException>(() => BitmapHelper.LoadBitmapImageFromPath(filePath, rotation));

        Assert.That(exception?.Message, Is.EqualTo($"'{rotation}' is not a valid value for property 'Rotation'."));

        _testLogger!.AssertLogExceptions([], typeof(BitmapHelperTests));
    }

    // TODO: Migrate from MagickImage to BitmapImage ?
    [Test]
    [Category("From ShowImage() in ViewerUserControl to open the image in fullscreen mode")]
    public void LoadBitmapImageFromPath_HeicImageFormat_ReturnsImageInfo()
    {
        string filePath = Path.Combine(_dataDirectory!, FileNames.IMAGE_11_HEIC);
        const ImageRotation rotation = ImageRotation.Rotate0;

        ImageInfo image = BitmapHelper.LoadBitmapImageFromPath(filePath, rotation);

        using (Assert.EnterMultipleScope())
        {
            Assert.That(image, Is.Not.Null);
            Assert.That(image.Data, Is.Not.Null);
            Assert.That(image.Data, Is.Not.Empty);
            Assert.That(image.Rotation, Is.EqualTo(rotation));
            Assert.That(image.Width,
                Is.EqualTo(PixelHeightAsset.IMAGE_11_HEIC)); // Wrong width (getting the height value instead)
            Assert.That(image.Height, Is.EqualTo(5376)); // Wrong height
        }

        _testLogger!.AssertLogExceptions([], typeof(BitmapHelperTests));
    }

    [Test]
    [Category("From ShowImage() in ViewerUserControl to open the image in fullscreen mode for Heic")]
    [TestCase(ImageRotation.Rotate0, PixelWidthAsset.IMAGE_11_HEIC, PixelHeightAsset.IMAGE_11_HEIC)]
    [TestCase(ImageRotation.Rotate90, PixelWidthAsset.IMAGE_11_HEIC, PixelHeightAsset.IMAGE_11_HEIC)]
    [TestCase(ImageRotation.Rotate180, PixelWidthAsset.IMAGE_11_HEIC, PixelHeightAsset.IMAGE_11_HEIC)]
    [TestCase(ImageRotation.Rotate270, PixelWidthAsset.IMAGE_11_HEIC, PixelHeightAsset.IMAGE_11_HEIC)]
    // [TestCase(null, PixelWidthAsset.IMAGE_11_HEIC, PixelHeightAsset.IMAGE_11_HEIC)]
    public void LoadBitmapHeicImageFromPathViewerUserControl_ValidPathAndRotationAndNotRotatedImage_ReturnsImageInfo(
        ImageRotation rotation, int expectedWidth, int expectedHeight)
    {
        string filePath = Path.Combine(_dataDirectory!, FileNames.IMAGE_11_HEIC);

        ImageInfo image = BitmapHelper.LoadBitmapHeicImageFromPath(filePath, rotation, _testLogger!);

        using (Assert.EnterMultipleScope())
        {
            Assert.That(image, Is.Not.Null);
            Assert.That(image.Data, Is.Not.Null);
            Assert.That(image.Data, Is.Not.Empty);
            Assert.That(image.Rotation, Is.EqualTo(rotation));
            Assert.That(image.Width, Is.EqualTo(expectedWidth));
            Assert.That(image.Height, Is.EqualTo(expectedHeight));
        }

        _testLogger!.AssertLogExceptions([], typeof(BitmapHelperTests));
    }

    [Test]
    [Category("From ShowImage() in ViewerUserControl to open the image in fullscreen mode for Heic")]
    [TestCase(FileNames.IMAGE_11_90_DEG_HEIC, ImageRotation.Rotate90, PixelWidthAsset.IMAGE_11_90_DEG_HEIC,
        PixelHeightAsset.IMAGE_11_90_DEG_HEIC)]
    [TestCase(FileNames.IMAGE_11_180_DEG_HEIC, ImageRotation.Rotate180, PixelWidthAsset.IMAGE_11_180_DEG_HEIC,
        PixelHeightAsset.IMAGE_11_180_DEG_HEIC)]
    [TestCase(FileNames.IMAGE_11_270_DEG_HEIC, ImageRotation.Rotate270, PixelWidthAsset.IMAGE_11_270_DEG_HEIC,
        PixelHeightAsset.IMAGE_11_270_DEG_HEIC)]
    public void LoadBitmapHeicImageFromPathViewerUserControl_ValidPathAndRotationAndRotatedImage_ReturnsImageInfo(
        string fileName, ImageRotation rotation, int expectedWidth, int expectedHeight)
    {
        string filePath = Path.Combine(_dataDirectory!, fileName);

        ImageInfo image = BitmapHelper.LoadBitmapHeicImageFromPath(filePath, rotation, _testLogger!);

        using (Assert.EnterMultipleScope())
        {
            Assert.That(image, Is.Not.Null);
            Assert.That(image.Data, Is.Not.Null);
            Assert.That(image.Data, Is.Not.Empty);
            Assert.That(image.Rotation, Is.EqualTo(rotation));
            Assert.That(image.Width, Is.EqualTo(expectedWidth));
            Assert.That(image.Height, Is.EqualTo(expectedHeight));
        }

        _testLogger!.AssertLogExceptions([], typeof(BitmapHelperTests));
    }

    [Test]
    [Category("From ShowImage() in ViewerUserControl to open the image in fullscreen mode for Heic")]
    public void LoadBitmapHeicImageFromPathViewerUserControl_FilePathIsNull_ReturnsImageInfo()
    {
        string? filePath = null;
        const ImageRotation rotation = ImageRotation.Rotate90;

        ImageInfo image = BitmapHelper.LoadBitmapHeicImageFromPath(filePath!, rotation, _testLogger!);

        using (Assert.EnterMultipleScope())
        {
            Assert.That(image, Is.Not.Null);
            Assert.That(image.Data, Is.Null);
            Assert.That(image.Rotation, Is.EqualTo(ImageRotation.Rotate0));
        }

        _testLogger!.AssertLogExceptions([], typeof(BitmapHelperTests));
    }

    [Test]
    [Category("From ShowImage() in ViewerUserControl to open the image in fullscreen mode for Heic")]
    public void LoadBitmapHeicImageFromPathViewerUserControl_ImageDoesNotExist_ReturnsDefaultImageInfo()
    {
        string filePath = Path.Combine(_dataDirectory!, FileNames.NON_EXISTENT_IMAGE_HEIC);
        const ImageRotation rotation = ImageRotation.Rotate90;

        ImageInfo image = BitmapHelper.LoadBitmapHeicImageFromPath(filePath, rotation, _testLogger!);

        using (Assert.EnterMultipleScope())
        {
            Assert.That(image, Is.Not.Null);
            Assert.That(image.Data, Is.Null);
            Assert.That(image.Rotation, Is.EqualTo(ImageRotation.Rotate0));
        }

        _testLogger!.AssertLogExceptions([], typeof(BitmapHelperTests));
    }

    [Test]
    [Category("From ShowImage() in ViewerUserControl to open the image in fullscreen mode for Heic")]
    public void LoadBitmapHeicImageFromPathViewerUserControl_CorruptedHeicFile_ReturnsDefaultImageInfo()
    {
        string validFilePath = Path.Combine(_dataDirectory!, FileNames.IMAGE_11_HEIC);
        string tempDirectory = Path.Combine(_dataDirectory!, "Temp");
        Directory.CreateDirectory(tempDirectory);

        try
        {
            const string invalidHeicFileName = "Invalid_Corrupted.heic";
            string filePath = Path.Combine(tempDirectory, invalidHeicFileName);
            const ImageRotation rotation = ImageRotation.Rotate90;

            ImageHelper.CreateInvalidImage(validFilePath, filePath);

            ImageInfo image = BitmapHelper.LoadBitmapHeicImageFromPath(filePath, rotation, _testLogger!);

            using (Assert.EnterMultipleScope())
            {
                Assert.That(image, Is.Not.Null);
                Assert.That(image.Data, Is.Null);
                Assert.That(image.Rotation, Is.EqualTo(ImageRotation.Rotate0));

                string expectedErrorMessage = $"Failed to load HEIC image from path: {filePath}.";
                _testLogger!.AssertLogExceptions([new Exception(expectedErrorMessage)], typeof(BitmapHelperTests));
            }
        }
        finally
        {
            Directory.Delete(tempDirectory, true);
        }
    }

    [Test]
    [Category("From ShowImage() in ViewerUserControl to open the image in fullscreen mode for Heic")]
    public void LoadBitmapHeicImageFromPathViewerUserControl_InvalidRotation_ThrowsArgumentException()
    {
        string filePath = Path.Combine(_dataDirectory!, FileNames.IMAGE_11_HEIC);
        const ImageRotation rotation = (ImageRotation)999;

        ArgumentException? exception =
            Assert.Throws<ArgumentException>(() =>
                BitmapHelper.LoadBitmapHeicImageFromPath(filePath, rotation, _testLogger!));

        Assert.That(exception?.Message, Is.EqualTo($"'{rotation}' is not a valid value for property 'Rotation'."));

        _testLogger!.AssertLogExceptions([], typeof(BitmapHelperTests));
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
    public void LoadBitmapThumbnailImageAssetRepository_ValidBufferAndWidthAndHeight_ReturnsImageInfo(int width,
        int height, int expectedWidth, int expectedHeight)
    {
        string filePath = Path.Combine(_dataDirectory!, FileNames.IMAGE_1_JPG);
        byte[] buffer = File.ReadAllBytes(filePath);

        ImageInfo image = BitmapHelper.LoadBitmapThumbnailImage(buffer, width, height, _testLogger!);

        Assert.That(image, Is.Not.Null);
        Assert.That(image.Data, Is.Not.Null);
        Assert.That(image.Data, Is.Not.Empty);
        Assert.That(image.Rotation, Is.EqualTo(ImageRotation.Rotate0));
        Assert.That(image.Width, Is.EqualTo(expectedWidth));
        Assert.That(image.Height, Is.EqualTo(expectedHeight));

        _testLogger!.AssertLogExceptions([], typeof(BitmapHelperTests));
    }

    [Test]
    [Category("From AssetRepository")]
    public void LoadBitmapThumbnailImageAssetRepository_LargeWidthAndHeight_ThrowsOverflowException()
    {
        string filePath = Path.Combine(_dataDirectory!, FileNames.IMAGE_1_JPG);
        byte[] buffer = File.ReadAllBytes(filePath);

        OverflowException? exception =
            Assert.Throws<OverflowException>(() =>
                BitmapHelper.LoadBitmapThumbnailImage(buffer, 1000000, 1000000, _testLogger!));

        Assert.That(exception?.Message, Is.EqualTo("The image data generated an overflow during processing."));

        _testLogger!.AssertLogExceptions([], typeof(BitmapHelperTests));
    }

    [Test]
    [Category("From AssetRepository")]
    public void LoadBitmapThumbnailImageAssetRepository_NullBuffer_ThrowsArgumentNullException()
    {
        byte[]? buffer = null;

        ArgumentNullException? exception =
            Assert.Throws<ArgumentNullException>(() =>
                BitmapHelper.LoadBitmapThumbnailImage(buffer!, 100, 100, _testLogger!));

        Assert.That(exception?.Message, Is.EqualTo("Value cannot be null. (Parameter 'buffer')"));

        _testLogger!.AssertLogExceptions([], typeof(BitmapHelperTests));
    }

    [Test]
    [Category("From AssetRepository")]
    public void LoadBitmapThumbnailImageAssetRepository_EmptyBuffer_ThrowsNotSupportedException()
    {
        byte[] buffer = [];

        NotSupportedException? exception =
            Assert.Throws<NotSupportedException>(() =>
                BitmapHelper.LoadBitmapThumbnailImage(buffer, 100, 100, _testLogger!));

        Assert.That(exception?.Message,
            Is.EqualTo("No imaging component suitable to complete this operation was found."));

        _testLogger!.AssertLogExceptions(
            [new NotSupportedException("No imaging component suitable to complete this operation was found.")],
            typeof(BitmapHelperTests));
    }

    [Test]
    [Category("From AssetRepository")]
    public void LoadBitmapThumbnailImageAssetRepository_InvalidBuffer_ThrowsNotSupportedException()
    {
        byte[] buffer = [];

        NotSupportedException? exception =
            Assert.Throws<NotSupportedException>(() =>
                BitmapHelper.LoadBitmapThumbnailImage(buffer, 100, 100, _testLogger!));

        Assert.That(exception?.Message,
            Is.EqualTo("No imaging component suitable to complete this operation was found."));

        _testLogger!.AssertLogExceptions(
            [new NotSupportedException("No imaging component suitable to complete this operation was found.")],
            typeof(BitmapHelperTests));
    }

    // TODO: Migrate from MagickImage to BitmapImage ?
    [Test]
    [Category("From AssetRepository")]
    public void LoadBitmapThumbnailImageAssetRepository_HeicImageFormat_ReturnsImageInfo()
    {
        string filePath = Path.Combine(_dataDirectory!, FileNames.IMAGE_11_HEIC);
        byte[] buffer = File.ReadAllBytes(filePath);
        const int width = 100;
        const int height = 100;

        ImageInfo image = BitmapHelper.LoadBitmapThumbnailImage(buffer, width, height, _testLogger!);

        using (Assert.EnterMultipleScope())
        {
            Assert.That(image, Is.Not.Null);
            Assert.That(image.Data, Is.Not.Null);
            Assert.That(image.Data, Is.Not.Empty);
            Assert.That(image.Rotation, Is.EqualTo(ImageRotation.Rotate0));
        }

        _testLogger!.AssertLogExceptions([], typeof(BitmapHelperTests));
    }

    [Test]
    [TestCase(FileNames.IMAGE_1_JPG)]
    [TestCase(FileNames.IMAGE_8_JPEG)]
    [TestCase(FileNames.IMAGE_10_PORTRAIT_PNG)]
    [TestCase(FileNames.HOMER_GIF)]
    [TestCase(FileNames.IMAGE_11_HEIC)]
    [TestCase(FileNames.IMAGE_11_90_DEG_HEIC)]
    public void LoadBitmapFromPath_ValidImagePath_ReturnsNonNullByteArray(string fileName)
    {
        string filePath = Path.Combine(_dataDirectory!, fileName);
        byte[]? imageBytes = BitmapHelper.LoadBitmapFromPath(filePath);

        Assert.That(imageBytes, Is.Not.Null);
        Assert.That(imageBytes, Is.Not.Empty);

        _testLogger!.AssertLogExceptions([], typeof(BitmapHelperTests));
    }

    [Test]
    public void LoadBitmapFromPath_ImageDoesNotExist_ReturnsNull()
    {
        string filePath = Path.Combine(_dataDirectory!, FileNames.NON_EXISTENT_IMAGE_PNG);

        byte[]? imageBytes = BitmapHelper.LoadBitmapFromPath(filePath);

        Assert.That(imageBytes, Is.Null);

        _testLogger!.AssertLogExceptions([], typeof(BitmapHelperTests));
    }

    [Test]
    public void LoadBitmapFromPath_ImagePathIsInvalid_ReturnsNull()
    {
        byte[]? imageBytes = BitmapHelper.LoadBitmapFromPath(_dataDirectory!);

        Assert.That(imageBytes, Is.Null);

        _testLogger!.AssertLogExceptions([], typeof(BitmapHelperTests));
    }

    [Test]
    public void LoadBitmapFromPath_ImagePathIsNull_ReturnsNull()
    {
        string? imagePath = null;

        byte[]? imageBytes = BitmapHelper.LoadBitmapFromPath(imagePath!);

        Assert.That(imageBytes, Is.Null);

        _testLogger!.AssertLogExceptions([], typeof(BitmapHelperTests));
    }

    [Test]
    [TestCase(FileNames.IMAGE_8_JPEG)]
    [TestCase(FileNames.IMAGE_1_JPG)]
    public void GetJpegBytes_ValidImage_ReturnsJpegByteArray(string fileName)
    {
        string filePath = Path.Combine(_dataDirectory!, fileName);
        ImageInfo image = BitmapHelper.LoadBitmapImageFromPath(filePath, ImageRotation.Rotate0);

        byte[] imageBuffer = BitmapHelper.GetJpegBytes(image);

        Assert.That(imageBuffer, Is.Not.Null);
        Assert.That(imageBuffer, Is.Not.Empty);

        string destinationNewFileDirectory = Path.Combine(_dataDirectory!, Directories.IMAGE_CONVERTED);

        try
        {
            Assert.That(ExifHelper.IsValidGdiPlusImage(imageBuffer, new TestLogger<BitmapHelperTests>()),
                Is.True);
            Directory.CreateDirectory(destinationNewFileDirectory);
            string destinationNewFilePath = Path.Combine(destinationNewFileDirectory, FileNames.IMAGE_CONVERTED_JPEG);
            File.WriteAllBytes(destinationNewFilePath, imageBuffer);
            Assert.That(IsValidImage(destinationNewFilePath), Is.True);
        }
        finally
        {
            Directory.Delete(destinationNewFileDirectory, true);
        }

        _testLogger!.AssertLogExceptions([], typeof(BitmapHelperTests));
    }

    [Test]
    public void GetJpegBytes_HeicValidImage_ReturnsJpegByteArray()
    {
        string filePath = Path.Combine(_dataDirectory!, FileNames.IMAGE_11_HEIC);
        byte[] buffer = File.ReadAllBytes(filePath);

        ImageInfo image = BitmapHelper.LoadBitmapHeicThumbnailImage(buffer, ImageRotation.Rotate0, 100, 100,
            new TestLogger<BitmapHelperTests>());

        byte[] imageBuffer = BitmapHelper.GetJpegBytes(image);

        Assert.That(imageBuffer, Is.Not.Null);
        Assert.That(imageBuffer, Is.Not.Empty);

        string destinationNewFileDirectory = Path.Combine(_dataDirectory!, Directories.IMAGE_CONVERTED);

        try
        {
            Assert.That(ExifHelper.IsValidGdiPlusImage(imageBuffer, new TestLogger<BitmapHelperTests>()),
                Is.True);
            Directory.CreateDirectory(destinationNewFileDirectory);
            string destinationNewFilePath = Path.Combine(destinationNewFileDirectory, FileNames.IMAGE_CONVERTED_JPEG);
            File.WriteAllBytes(destinationNewFilePath, imageBuffer);
            Assert.That(IsValidImage(destinationNewFilePath), Is.True);
        }
        finally
        {
            Directory.Delete(destinationNewFileDirectory, true);
        }

        _testLogger!.AssertLogExceptions([], typeof(BitmapHelperTests));
    }

    [Test]
    public void GetJpegBytes_InvalidImage_ThrowsInvalidOperationException()
    {
        ImageInfo image = new ImageInfo(null, 0, 0, ImageRotation.Rotate0);

        InvalidOperationException? exception =
            Assert.Throws<InvalidOperationException>(() => BitmapHelper.GetJpegBytes(image));

        Assert.That(exception?.Message, Is.EqualTo("Operation is not valid due to the current state of the object."));

        _testLogger!.AssertLogExceptions([], typeof(BitmapHelperTests));
    }

    [Test]
    public void GetJpegBytes_NullImage_ThrowsArgumentNullException()
    {
        ImageInfo? invalidImage = null;

        ArgumentNullException? exception =
            Assert.Throws<ArgumentNullException>(() => BitmapHelper.GetJpegBytes(invalidImage!));

        Assert.That(exception?.Message, Is.EqualTo("Value cannot be null. (Parameter 'source')"));

        _testLogger!.AssertLogExceptions([], typeof(BitmapHelperTests));
    }

    [Test]
    [TestCase(FileNames.IMAGE_8_JPEG)]
    [TestCase(FileNames.IMAGE_1_JPG)]
    public void GetPngBytes_ValidImage_ReturnsPngByteArray(string fileName)
    {
        string filePath = Path.Combine(_dataDirectory!, fileName);
        ImageInfo image = BitmapHelper.LoadBitmapImageFromPath(filePath, ImageRotation.Rotate0);

        byte[] imageBuffer = BitmapHelper.GetPngBytes(image);

        Assert.That(imageBuffer, Is.Not.Null);
        Assert.That(imageBuffer, Is.Not.Empty);

        string destinationNewFileDirectory = Path.Combine(_dataDirectory!, Directories.IMAGE_CONVERTED);

        try
        {
            Assert.That(ExifHelper.IsValidGdiPlusImage(imageBuffer, new TestLogger<BitmapHelperTests>()),
                Is.True);
            Directory.CreateDirectory(destinationNewFileDirectory);
            string destinationNewFilePath = Path.Combine(destinationNewFileDirectory, FileNames.IMAGE_CONVERTED_PNG);
            File.WriteAllBytes(destinationNewFilePath, imageBuffer);
            Assert.That(IsValidImage(destinationNewFilePath), Is.True);
        }
        finally
        {
            Directory.Delete(destinationNewFileDirectory, true);
        }

        _testLogger!.AssertLogExceptions([], typeof(BitmapHelperTests));
    }

    [Test]
    public void GetPngBytes_HeicValidImage_ReturnsPngByteArray()
    {
        string filePath = Path.Combine(_dataDirectory!, FileNames.IMAGE_11_HEIC);
        byte[] buffer = File.ReadAllBytes(filePath);

        ImageInfo image = BitmapHelper.LoadBitmapHeicThumbnailImage(buffer, ImageRotation.Rotate0, 100, 100,
            new TestLogger<BitmapHelperTests>());

        byte[] imageBuffer = BitmapHelper.GetPngBytes(image);

        Assert.That(imageBuffer, Is.Not.Null);
        Assert.That(imageBuffer, Is.Not.Empty);

        string destinationNewFileDirectory = Path.Combine(_dataDirectory!, Directories.IMAGE_CONVERTED);

        try
        {
            Assert.That(ExifHelper.IsValidGdiPlusImage(imageBuffer, new TestLogger<BitmapHelperTests>()),
                Is.True);
            Directory.CreateDirectory(destinationNewFileDirectory);
            string destinationNewFilePath = Path.Combine(destinationNewFileDirectory, FileNames.IMAGE_CONVERTED_PNG);
            File.WriteAllBytes(destinationNewFilePath, imageBuffer);
            Assert.That(IsValidImage(destinationNewFilePath), Is.True);
        }
        finally
        {
            Directory.Delete(destinationNewFileDirectory, true);
        }

        _testLogger!.AssertLogExceptions([], typeof(BitmapHelperTests));
    }

    [Test]
    public void GetPngBytes_InvalidImage_ThrowsInvalidOperationException()
    {
        ImageInfo image = new ImageInfo(null, 0, 0, ImageRotation.Rotate0);

        InvalidOperationException? exception =
            Assert.Throws<InvalidOperationException>(() => BitmapHelper.GetPngBytes(image));

        Assert.That(exception?.Message, Is.EqualTo("Operation is not valid due to the current state of the object."));

        _testLogger!.AssertLogExceptions([], typeof(BitmapHelperTests));
    }

    [Test]
    public void GetPngBytes_NullImage_ThrowsArgumentNullException()
    {
        ImageInfo? invalidImage = null;

        ArgumentNullException? exception =
            Assert.Throws<ArgumentNullException>(() => BitmapHelper.GetPngBytes(invalidImage!));

        Assert.That(exception?.Message, Is.EqualTo("Value cannot be null. (Parameter 'source')"));

        _testLogger!.AssertLogExceptions([], typeof(BitmapHelperTests));
    }

    [Test]
    [TestCase(FileNames.IMAGE_8_JPEG)]
    [TestCase(FileNames.IMAGE_1_JPG)]
    public void GetGifBytes_ValidImage_ReturnsGifByteArray(string fileName)
    {
        string filePath = Path.Combine(_dataDirectory!, fileName);
        ImageInfo image = BitmapHelper.LoadBitmapImageFromPath(filePath, ImageRotation.Rotate0);

        byte[] imageBuffer = BitmapHelper.GetGifBytes(image);

        Assert.That(imageBuffer, Is.Not.Null);
        Assert.That(imageBuffer, Is.Not.Empty);

        string destinationNewFileDirectory = Path.Combine(_dataDirectory!, Directories.IMAGE_CONVERTED);

        try
        {
            Assert.That(ExifHelper.IsValidGdiPlusImage(imageBuffer, new TestLogger<BitmapHelperTests>()),
                Is.True);
            Directory.CreateDirectory(destinationNewFileDirectory);
            string destinationNewFilePath = Path.Combine(destinationNewFileDirectory, FileNames.IMAGE_CONVERTED_GIF);
            File.WriteAllBytes(destinationNewFilePath, imageBuffer);
            Assert.That(IsValidImage(destinationNewFilePath), Is.True);
        }
        finally
        {
            Directory.Delete(destinationNewFileDirectory, true);
        }

        _testLogger!.AssertLogExceptions([], typeof(BitmapHelperTests));
    }

    [Test]
    public void GetGifBytes_HeicValidImage_ReturnsGifByteArray()
    {
        string filePath = Path.Combine(_dataDirectory!, FileNames.IMAGE_11_HEIC);
        byte[] buffer = File.ReadAllBytes(filePath);

        ImageInfo image = BitmapHelper.LoadBitmapHeicThumbnailImage(buffer, ImageRotation.Rotate0, 100, 100,
            new TestLogger<BitmapHelperTests>());

        byte[] imageBuffer = BitmapHelper.GetGifBytes(image);

        Assert.That(imageBuffer, Is.Not.Null);
        Assert.That(imageBuffer, Is.Not.Empty);

        string destinationNewFileDirectory = Path.Combine(_dataDirectory!, Directories.IMAGE_CONVERTED);

        try
        {
            Assert.That(ExifHelper.IsValidGdiPlusImage(imageBuffer, new TestLogger<BitmapHelperTests>()),
                Is.True);
            Directory.CreateDirectory(destinationNewFileDirectory);
            string destinationNewFilePath = Path.Combine(destinationNewFileDirectory, FileNames.IMAGE_CONVERTED_GIF);
            File.WriteAllBytes(destinationNewFilePath, imageBuffer);
            Assert.That(IsValidImage(destinationNewFilePath), Is.True);
        }
        finally
        {
            Directory.Delete(destinationNewFileDirectory, true);
        }

        _testLogger!.AssertLogExceptions([], typeof(BitmapHelperTests));
    }

    [Test]
    public void GetGifBytes_InvalidImage_ThrowsInvalidOperationException()
    {
        ImageInfo image = new ImageInfo(null, 0, 0, ImageRotation.Rotate0);

        InvalidOperationException? exception =
            Assert.Throws<InvalidOperationException>(() => BitmapHelper.GetGifBytes(image));

        Assert.That(exception?.Message, Is.EqualTo("Operation is not valid due to the current state of the object."));

        _testLogger!.AssertLogExceptions([], typeof(BitmapHelperTests));
    }

    [Test]
    public void GetGifBytes_NullImage_ThrowsArgumentException()
    {
        ImageInfo? invalidImage = null;

        ArgumentNullException? exception =
            Assert.Throws<ArgumentNullException>(() => BitmapHelper.GetGifBytes(invalidImage!));

        Assert.That(exception?.Message, Is.EqualTo("Value cannot be null. (Parameter 'source')"));

        _testLogger!.AssertLogExceptions([], typeof(BitmapHelperTests));
    }

    private static void AssertBrightnessValues(Bitmap bitmap, int x, int y)
    {
        Color pixelColor = bitmap.GetPixel(x, y);
        float brightness = pixelColor.GetBrightness();

        // Assert.That(brightness, Is.Not.Null);
        Assert.That(brightness, Is.GreaterThan(0));
        Assert.That(brightness, Is.LessThan(1));
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
