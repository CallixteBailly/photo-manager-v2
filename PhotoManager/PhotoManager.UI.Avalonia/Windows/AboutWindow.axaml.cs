namespace PhotoManager.UI.Avalonia.Windows;

public partial class AboutWindow : Window
{
    private readonly ILogger<AboutWindow> _logger;

    public AboutWindow(AboutInformation aboutInformation, ILogger<AboutWindow> logger)
    {
        _logger = logger;

        try
        {
            InitializeComponent();
            DataContext = aboutInformation;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "{ExMessage}", ex.Message);
            throw;
        }
    }
}
