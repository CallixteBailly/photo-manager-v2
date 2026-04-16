
namespace PhotoManager.Infrastructure.Services;

// TODO: Full implementation in Phase 4 when Avalonia UI is available
// This service uses Avalonia's MessageBox/DialogManager for cross-platform dialogs
public class AvaloniaDialogService : IDialogService
{
    public Task<bool> ShowConfirmAsync(string title, string message)
    {
        // TODO: Implement with Avalonia MessageBox in Phase 4
        return Task.FromResult(true);
    }

    public Task ShowInfoAsync(string title, string message)
    {
        // TODO: Implement with Avalonia MessageBox in Phase 4
        return Task.CompletedTask;
    }
}
