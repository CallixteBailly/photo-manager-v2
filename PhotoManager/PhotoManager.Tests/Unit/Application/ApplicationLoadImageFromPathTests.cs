using Directories = PhotoManager.Tests.Unit.Constants.Directories;
using FileNames = PhotoManager.Tests.Unit.Constants.FileNames;
using PixelHeightAsset = PhotoManager.Tests.Unit.Constants.PixelHeightAsset;
using PixelWidthAsset = PhotoManager.Tests.Unit.Constants.PixelWidthAsset;
using PhotoManager.Domain;

namespace PhotoManager.Tests.Unit.Application;

[TestFixture]
public class ApplicationLoadImageFromPathTests
{
    private string? _dataDirectory;
    private string? _databaseDirectory;
    private string? _databasePath;

    private PhotoManager.Application.Application? _application;

    [OneTimeSetUp]
    public void OneTimeSetUp()
    {
        _dataDirectory = Path.Combine(TestContext.CurrentContext.TestDirectory, Directories.TEST_FILES);
        _databaseDirectory = Path.Combine(_dataDirectory, Directories.DATABASE_TESTS);
        _databasePath = Path.Combine(_databaseDirectory, Constants.DATABASE_END_PATH);
    }

    private void ConfigureApplication(int catalogBatchSize, string assetsDirectory, int thumbnailMaxWidth,
        int thumbnailMaxHeight, bool usingDHash, bool usingMD5Hash, bool usingPHash, bool analyseVideos)
    {
        IConfigurationRoot configurationRootMock = Substitute.For<IConfigurationRoot>();
        configurationRootMock.GetDefaultMockConfig();
        configurationRootMock.MockGetValue(UserConfigurationKeys.CATALOG_BATCH_SIZE, catalogBatchSize.ToString());
        configurationRootMock.MockGetValue(UserConfigurationKeys.ASSETS_DIRECTORY, assetsDirectory);
        configurationRootMock.MockGetValue(UserConfigurationKeys.THUMBNAIL_MAX_WIDTH, thumbnailMaxWidth.ToString());
        configurationRootMock.MockGetValue(UserConfigurationKeys.THUMBNAIL_MAX_HEIGHT, thumbnailMaxHeight.ToString());
        configurationRootMock.MockGetValue(UserConfigurationKeys.USING_DHASH, usingDHash.ToString());
        configurationRootMock.MockGetValue(UserConfigurationKeys.USING_MD5_HASH, usingMD5Hash.ToString());
        configurationRootMock.MockGetValue(UserConfigurationKeys.USING_PHASH, usingPHash.ToString());
        configurationRootMock.MockGetValue(UserConfigurationKeys.ANALYSE_VIDEOS, analyseVideos.ToString());

        UserConfigurationService userConfigurationService = new(configurationRootMock);

        IPathProviderService pathProviderServiceMock = Substitute.For<IPathProviderService>();
        pathProviderServiceMock.ResolveDataDirectory().Returns(_databasePath);

        Database database = new(new ObjectListStorage(), new BlobStorage(), new BackupStorage(),
            new TestLogger<Database>());
        ImageProcessingService imageProcessingService = new(new TestLogger<ImageProcessingService>());
        FileOperationsService fileOperationsService = new(userConfigurationService,
            new TestLogger<FileOperationsService>());
        ImageMetadataService imageMetadataService = new(fileOperationsService, new TestLogger<ImageMetadataService>());
        AssetRepository assetRepository = new(database, pathProviderServiceMock, imageProcessingService,
            imageMetadataService, userConfigurationService, new TestLogger<AssetRepository>());
        AssetHashCalculatorService assetHashCalculatorService = new(userConfigurationService,
            new TestLogger<AssetHashCalculatorService>());
        AssetCreationService assetCreationService = new(assetRepository, fileOperationsService, imageProcessingService,
            imageMetadataService, assetHashCalculatorService, userConfigurationService,
            new TestLogger<AssetCreationService>());
        AssetsComparator assetsComparator = new();
        CatalogAssetsService catalogAssetsService = new(assetRepository, fileOperationsService, imageMetadataService,
            assetCreationService, userConfigurationService, assetsComparator, new TestLogger<CatalogAssetsService>());
        MoveAssetsService moveAssetsService = new(assetRepository, fileOperationsService, assetCreationService,
            new TestLogger<MoveAssetsService>());
        SyncAssetsService syncAssetsService = new(assetRepository, fileOperationsService, assetsComparator,
            moveAssetsService);
        FindDuplicatedAssetsService findDuplicatedAssetsService = new(assetRepository, fileOperationsService,
            userConfigurationService, new TestLogger<FindDuplicatedAssetsService>());
        _application = new(assetRepository, syncAssetsService, catalogAssetsService, moveAssetsService,
            findDuplicatedAssetsService, userConfigurationService, fileOperationsService, imageProcessingService);
    }

    [Test]
    [TestCase(Enums.ImageRotation.Rotate0, PixelWidthAsset.IMAGE_1_JPG, PixelHeightAsset.IMAGE_1_JPG)]
    [TestCase(Enums.ImageRotation.Rotate90, PixelHeightAsset.IMAGE_1_JPG, PixelWidthAsset.IMAGE_1_JPG)]
    [TestCase(Enums.ImageRotation.Rotate180, PixelWidthAsset.IMAGE_1_JPG, PixelHeightAsset.IMAGE_1_JPG)]
    [TestCase(Enums.ImageRotation.Rotate270, PixelHeightAsset.IMAGE_1_JPG, PixelWidthAsset.IMAGE_1_JPG)]
    public void LoadImageFromPath_ValidRotationAndPath_ReturnsImageInfo(Enums.ImageRotation rotation, int expectedWith,
        int expectedHeight)
    {
        ConfigureApplication(100, _dataDirectory!, 200, 150, false, false, false, false);

        try
        {
            string filePath = Path.Combine(_dataDirectory!, FileNames.IMAGE_1_JPG);

            ImageInfo image = _application!.LoadImageFromPath(filePath, rotation);

            Assert.That(image, Is.Not.Null);
            Assert.That(image.Data, Is.Not.Empty);
            Assert.That(image.Rotation, Is.EqualTo(rotation));
            Assert.That(image.Width, Is.EqualTo(expectedWith));
            Assert.That(image.Height, Is.EqualTo(expectedHeight));
        }
        finally
        {
            Directory.Delete(_databaseDirectory!, true);
        }
    }

    [Test]
    public void LoadImageFromPath_ImageDoesNotExist_ReturnsEmptyImageInfo()
    {
        ConfigureApplication(100, _dataDirectory!, 200, 150, false, false, false, false);

        try
        {
            string filePath = Path.Combine(_dataDirectory!, FileNames.NON_EXISTENT_IMAGE_JPG);
            const Enums.ImageRotation rotation = Enums.ImageRotation.Rotate90;

            ImageInfo image = _application!.LoadImageFromPath(filePath, rotation);

            Assert.That(image, Is.Not.Null);
            Assert.That(image.Data, Is.Empty);
            Assert.That(image.Rotation, Is.EqualTo(Enums.ImageRotation.Rotate90));
            Assert.That(image.Width, Is.Zero);
            Assert.That(image.Height, Is.Zero);
        }
        finally
        {
            Directory.Delete(_databaseDirectory!, true);
        }
    }

    [Test]
    public void LoadImageFromPath_FilePathIsNull_ReturnsEmptyImageInfo()
    {
        ConfigureApplication(100, _dataDirectory!, 200, 150, false, false, false, false);

        try
        {
            string? filePath = null;
            const Enums.ImageRotation rotation = Enums.ImageRotation.Rotate90;

            ImageInfo image = _application!.LoadImageFromPath(filePath!, rotation);

            Assert.That(image, Is.Not.Null);
            Assert.That(image.Data, Is.Empty);
            Assert.That(image.Rotation, Is.EqualTo(Enums.ImageRotation.Rotate90));
            Assert.That(image.Width, Is.Zero);
            Assert.That(image.Height, Is.Zero);
        }
        finally
        {
            Directory.Delete(_databaseDirectory!, true);
        }
    }

    [Test]
    public void LoadImageFromPath_InvalidRotation_ThrowsArgumentException()
    {
        ConfigureApplication(100, _dataDirectory!, 200, 150, false, false, false, false);

        try
        {
            string filePath = Path.Combine(_dataDirectory!, FileNames.IMAGE_1_JPG);
            const Enums.ImageRotation rotation = (Enums.ImageRotation)999;

            ArgumentException? exception =
                Assert.Throws<ArgumentException>(() => _application!.LoadImageFromPath(filePath, rotation));

            Assert.That(exception?.Message, Is.EqualTo($"'{rotation}' is not a valid value for property 'Rotation'."));
        }
        finally
        {
            Directory.Delete(_databaseDirectory!, true);
        }
    }

    // TODO: Migrate from MagickImage to ImageInfo ?
    [Test]
    public void LoadImageFromPath_HeicImageFormat_ReturnsImageInfo()
    {
        ConfigureApplication(100, _dataDirectory!, 200, 150, false, false, false, false);

        try
        {
            string filePath = Path.Combine(_dataDirectory!, FileNames.IMAGE_11_HEIC);
            const Enums.ImageRotation rotation = Enums.ImageRotation.Rotate0;

            ImageInfo image = _application!.LoadImageFromPath(filePath, rotation);

            using (Assert.EnterMultipleScope())
            {
                Assert.That(image, Is.Not.Null);
                Assert.That(image.Data, Is.Not.Empty);
                Assert.That(image.Rotation, Is.EqualTo(rotation));
                Assert.That(image.Width,
                    Is.EqualTo(PixelHeightAsset.IMAGE_11_HEIC)); // Wrong width (getting the height value instead)
                Assert.That(image.Height, Is.EqualTo(5376)); // Wrong height
            }
        }
        finally
        {
            Directory.Delete(_databaseDirectory!, true);
        }
    }
}
