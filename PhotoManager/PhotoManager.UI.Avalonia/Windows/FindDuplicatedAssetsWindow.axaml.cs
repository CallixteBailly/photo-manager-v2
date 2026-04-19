using PhotoManager.UI.Avalonia.ViewModels;

namespace PhotoManager.UI.Avalonia.Windows;

public partial class FindDuplicatedAssetsWindow : Window
{
    private readonly ILogger<FindDuplicatedAssetsWindow> _logger;

    public FindDuplicatedAssetsWindow(
        FindDuplicatedAssetsViewModel viewModel,
        ILogger<FindDuplicatedAssetsWindow> logger)
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
