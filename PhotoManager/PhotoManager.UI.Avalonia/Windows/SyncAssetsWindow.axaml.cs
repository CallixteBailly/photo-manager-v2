using PhotoManager.UI.Avalonia.ViewModels;

namespace PhotoManager.UI.Avalonia.Windows;

public partial class SyncAssetsWindow : Window
{
    private readonly ILogger<SyncAssetsWindow> _logger;

    public SyncAssetsWindow(
        SyncAssetsViewModel viewModel,
        ILogger<SyncAssetsWindow> logger)
    {
        _logger = logger;

        try
        {
            InitializeComponent();
            DataContext = viewModel;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "{ExMessage}", ex.Message);
            throw;
        }
    }
}
