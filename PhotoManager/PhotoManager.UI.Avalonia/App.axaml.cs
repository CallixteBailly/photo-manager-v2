using PhotoManager.Infrastructure;
using PhotoManager.UI.Avalonia.Windows;
using Serilog;
using System.Diagnostics.CodeAnalysis;

namespace PhotoManager.UI.Avalonia;

[ExcludeFromCodeCoverage]
public partial class App : global::Avalonia.Application
{
    private static readonly Mutex AppMutex = new(true, "PhotoManagerStartup");

    private IServiceProvider? _serviceProvider;

    /// <summary>
    /// Exposes the DI container for use in XAML-instantiated controls that cannot receive constructor injection
    /// </summary>
    public static IServiceProvider? ServiceProvider { get; private set; }

    public override void Initialize()
    {
        AvaloniaXamlLoader.Load(this);
        base.Initialize();
    }

    public override void OnFrameworkInitializationCompleted()
    {
        if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            try
            {
                ServiceCollection serviceCollection = new();
                ConfigureServices(serviceCollection);
                _serviceProvider = serviceCollection.BuildServiceProvider();
                ServiceProvider = _serviceProvider;

                ILogger<App> logger = _serviceProvider.GetRequiredService<ILogger<App>>();

                if (AppMutex.WaitOne(TimeSpan.Zero, true))
                {
                    desktop.MainWindow = _serviceProvider.GetService<MainWindow>();
                }
                else
                {
                    logger.LogWarning("The application is already running.");
                    desktop.Shutdown();
                }
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine($"Failed to initialize application: {ex.Message}");
                desktop.Shutdown();
            }
        }

        base.OnFrameworkInitializationCompleted();
    }

    private static void ConfigureServices(IServiceCollection services)
    {
        IConfigurationBuilder builder =
            new ConfigurationBuilder().AddJsonFile("appsettings.json", optional: false, reloadOnChange: true);
        IConfigurationRoot configuration = builder.Build();

        Log.Logger = new LoggerConfiguration()
            .MinimumLevel.Information()
            .WriteTo.File(
                "log.txt",
                outputTemplate:
                "{Timestamp:yyyy-MM-dd HH:mm:ss} {Level:u5} {SourceContext} - {Message:lj}{NewLine}{Exception}",
                fileSizeLimitBytes: 10L * 1024 * 1024,
                rollOnFileSizeLimit: true,
                retainedFileCountLimit: 10)
            .CreateLogger();

        services.AddLogging(logging =>
        {
            logging.AddConsole();
            logging.AddSerilog(dispose: true);
            logging.SetMinimumLevel(LogLevel.Information);
        });
        services.AddSingleton(configuration);
        services.AddInfrastructure();
        services.AddDomain();
        services.AddApplication();
        services.AddUiAvalonia();
    }
}
