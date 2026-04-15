using System.Diagnostics;
using PhotoManager.Domain.Interfaces;

namespace PhotoManager.Infrastructure.Services;

public class LinuxNotificationService : INotificationService
{
    public void ShowNotification(string title, string message)
    {
        try
        {
            Process.Start(new ProcessStartInfo("notify-send",
                $""{title}" "{message}"") { UseShellExecute = true });
        }
        catch
        {
            // notify-send may not be available on all Linux distros
        }
    }

    public void ShowProgress(string title, string message, int progress)
    {
        // TODO: Implement Linux progress notification via libnotify
    }
}
