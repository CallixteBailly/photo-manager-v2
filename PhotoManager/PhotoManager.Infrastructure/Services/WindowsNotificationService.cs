using PhotoManager.Domain.Interfaces;

namespace PhotoManager.Infrastructure.Services;

public class WindowsNotificationService : INotificationService
{
    public void ShowNotification(string title, string message)
    {
        // TODO: Implement Windows notification (Microsoft.Toolkit.Uwp.Notifications or Windows API)
    }

    public void ShowProgress(string title, string message, int progress)
    {
        // TODO: Implement Windows progress notification
    }
}
