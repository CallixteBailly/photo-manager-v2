using System.Diagnostics;
using PhotoManager.Domain.Interfaces;

namespace PhotoManager.Infrastructure.Services;

public class MacOSNotificationService : INotificationService
{
    public void ShowNotification(string title, string message)
    {
        string script = $"display notification "{message}" with title "{title}"";
        Process.Start(new ProcessStartInfo("osascript", $"-e '{script}'") { UseShellExecute = true });
    }

    public void ShowProgress(string title, string message, int progress)
    {
        // TODO: Implement macOS progress notification
    }
}
