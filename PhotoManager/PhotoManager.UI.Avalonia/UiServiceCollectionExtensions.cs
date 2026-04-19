using PhotoManager.UI.Avalonia.ViewModels;
using PhotoManager.UI.Avalonia.Windows;

namespace PhotoManager.UI.Avalonia;

public static class UiServiceCollectionExtensions
{
    public static IServiceCollection AddUiAvalonia(this IServiceCollection services)
    {
        services.AddTransient<ApplicationViewModel>();
        services.AddTransient<MainWindow>();
        return services;
    }
}
