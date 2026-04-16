using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using PhotoManager.UI.ViewModels;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using PhotoManager.Common;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media.Imaging;

namespace PhotoManager.UI.Controls;

/// <summary>
/// Interaction logic for ViewerUserControl.xaml
/// </summary>
[ExcludeFromCodeCoverage]
public partial class ViewerUserControl
{
    private readonly ILogger<ViewerUserControl> _logger;

    public ViewerUserControl()
    {
        _logger = App.ServiceProvider?.GetService<ILogger<ViewerUserControl>>()
                  ?? NullLogger<ViewerUserControl>.Instance;
        try
        {
            InitializeComponent();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "{ExMessage}", ex.Message);
        }
    }

    public event EventHandler? ThumbnailSelected;

    private ApplicationViewModel ViewModel => (ApplicationViewModel)DataContext;

    private void NextButton_MouseLeftButtonDown(object sender, RoutedEventArgs e)
    {
        try
        {
            ViewModel.GoToNextAsset();
            ShowImage();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "{ExMessage}", ex.Message);
        }
    }

    private void PreviousButton_MouseLeftButtonDown(object sender, RoutedEventArgs e)
    {
        try
        {
            ViewModel.GoToPreviousAsset();
            ShowImage();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "{ExMessage}", ex.Message);
        }
    }

    private void ContentControl_MouseDoubleClick(object sender, MouseButtonEventArgs e)
    {
        try
        {
            if (ViewModel.CurrentAsset != null)
            {
                ThumbnailSelected?.Invoke(this, EventArgs.Empty);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "{ExMessage}", ex.Message);
        }
    }

    private void CloseButton_Click(object sender, RoutedEventArgs e)
    {
        ViewModel.ChangeAppMode();
    }

    // Triggered when double-clicked on the thumbnail image from ThumbnailsUserControl to pass into fullscreen mode
    public void ShowImage()
    {
        if (ViewModel is { CurrentAsset: not null })
        {
            bool isHeic = ViewModel.CurrentAsset.FileName.EndsWith(".heic", StringComparison.OrdinalIgnoreCase);

            ImageInfo imageInfo = isHeic ? ViewModel.LoadHeicImageFromPath() : ViewModel.LoadImageFromPath();

            BitmapImage bitmapImage = new();
            bitmapImage.BeginInit();
            bitmapImage.StreamSource = new MemoryStream(imageInfo.Data);
            bitmapImage.Rotation = imageInfo.Rotation.ToRotation();
            bitmapImage.EndInit();

            Image.Source = bitmapImage;
            BackgroundImage.Source = bitmapImage;
        }
        else
        {
            Image.Source = null;
            BackgroundImage.Source = null;
        }
    }
}
