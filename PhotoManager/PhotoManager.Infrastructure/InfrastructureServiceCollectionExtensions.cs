using Microsoft.Extensions.DependencyInjection;
using PhotoManager.Domain.Interfaces;
using PhotoManager.Infrastructure.Services;
using System.Runtime.InteropServices;

namespace PhotoManager.Infrastructure;

public static class InfrastructureServiceCollectionExtensions
{
    extension(IServiceCollection services)
    {
        public void AddInfrastructure()
        {
            services.AddDatabase();
            services.AddSingleton<IUserConfigurationService, UserConfigurationService>();
            services.AddSingleton<IPathProviderService, PathProviderService>();
            services.AddSingleton<IFileOperationsService, FileOperationsService>();
            services.AddSingleton<IImageProcessingService, ImageProcessingService>();
            services.AddSingleton<IImageMetadataService, ImageMetadataService>();
            services.AddSingleton<IAssetRepository, AssetRepository>();
            services.AddSingleton<IAssetHashCalculatorService, AssetHashCalculatorService>();
            RegisterOsServices(services);
        }

        // Known issue, see: https://github.com/dotnet/roslyn/issues/82691
#pragma warning disable IDE0051
        private void AddDatabase()
#pragma warning restore IDE0051
        {
            services.AddSingleton<IObjectListStorage, ObjectListStorage>();
            services.AddSingleton<IBlobStorage, BlobStorage>();
            services.AddSingleton<IBackupStorage, BackupStorage>();
            services.AddSingleton<IDatabase, Database.Database>();
        }

        private static void RegisterOsServices(IServiceCollection services)
        {
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                services.AddSingleton<IFileExplorerService, WindowsFileExplorerService>();
                services.AddSingleton<INotificationService, WindowsNotificationService>();
            }
            else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
            {
                services.AddSingleton<IFileExplorerService, LinuxFileExplorerService>();
                services.AddSingleton<INotificationService, LinuxNotificationService>();
            }
            else if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
            {
                services.AddSingleton<IFileExplorerService, MacOSFileExplorerService>();
                services.AddSingleton<INotificationService, MacOSNotificationService>();
            }

            services.AddSingleton<IDialogService, AvaloniaDialogService>();
        }
    }
}
