using PhotoManager.UI.Avalonia.ViewModels;

namespace PhotoManager.UI.Avalonia.Windows;

public partial class FolderNavigationWindow : Window
{
    private readonly ILogger<FolderNavigationWindow> _logger;

    public FolderNavigationViewModel? ViewModel { get; private set; }

    public FolderNavigationWindow(
        FolderNavigationViewModel viewModel,
        ILogger<FolderNavigationWindow> logger)
    {
        _logger = logger;
        ViewModel = viewModel;

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
