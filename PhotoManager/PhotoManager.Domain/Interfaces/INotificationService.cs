namespace PhotoManager.Domain.Interfaces;

public interface INotificationService
{
    void ShowNotification(string title, string message);
    void ShowProgress(string title, string message, int progress);
}
