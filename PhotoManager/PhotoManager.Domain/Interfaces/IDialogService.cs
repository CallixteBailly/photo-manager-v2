namespace PhotoManager.Domain.Interfaces;

public interface IDialogService
{
    Task<bool> ShowConfirmAsync(string title, string message);
    Task ShowInfoAsync(string title, string message);
}
