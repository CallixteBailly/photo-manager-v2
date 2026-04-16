using System.Runtime.InteropServices;
using Microsoft.Extensions.Logging;

namespace PhotoManager.Infrastructure.Services;

/// <summary>
/// Windows-specific implementation of INotificationService using Windows Runtime APIs.
/// </summary>
public class NotificationService(ILogger<NotificationService> logger) : INotificationService
{

    /// <inheritdoc />
    public void ShowNotification(string title, string message)
    {
        try
        {
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                // Use Windows Runtime APIs via Microsoft.Toolkit.Uwp.Notifications
                // This will be implemented in the UI layer due to WPF/Avalonia dependencies
                logger.LogWarning("Notification functionality will be implemented in the UI layer");
            }
            else
            {
                // Linux and macOS implementations would use different notification systems
                logger.LogInformation("Notification: {Title} - {Message}", title, message);
            }
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to show notification: {Title} - {Message}", title, message);
        }
    }

    /// <inheritdoc />
    public void ShowProgress(string title, string message, int progress)
    {
        try
        {
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                // Use Windows Runtime APIs via Microsoft.Toolkit.Uwp.Notifications
                logger.LogWarning("Progress notification functionality will be implemented in the UI layer");
            }
            else
            {
                logger.LogInformation("Progress notification: {Title} - {Message} ({Progress}%)", title, message,
                    progress);
            }
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to show progress notification: {Title} - {Message} ({Progress}%)", title,
                message, progress);
        }
    }
}
