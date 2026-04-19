using PhotoManager.UI.Avalonia.ViewModels;

namespace PhotoManager.UI.Avalonia.Windows;

public partial class MainWindow : Window
{
    private readonly ILogger<MainWindow> _logger;

    public MainWindow(ApplicationViewModel viewModel, IApplication _, ILoggerFactory loggerFactory)
    {
        _logger = loggerFactory.CreateLogger<MainWindow>();

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
