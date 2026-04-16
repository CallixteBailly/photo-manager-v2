using Microsoft.Extensions.Logging;

namespace PhotoManager.Infrastructure.Services;

/// <summary>
/// Implementation of IDialogService.
/// Note: This is a placeholder implementation. The actual implementation will be provided by the UI layer
/// (Avalonia) since dialog functionality requires UI framework integration.
/// </summary>
public class DialogService(ILogger<DialogService> logger) : IDialogService
{

    /// <inheritdoc />
    public Task<bool> ShowConfirmAsync(string title, string message)
    {
        // Placeholder implementation - actual implementation will be in the UI layer
        logger.LogInformation("Confirm dialog: {Title} - {Message}", title, message);
        return Task.FromResult(true);
    }

    /// <inheritdoc />
    public Task ShowInfoAsync(string title, string message)
    {
        // Placeholder implementation - actual implementation will be in the UI layer
        logger.LogInformation("Info dialog: {Title} - {Message}", title, message);
        return Task.CompletedTask;
    }
}
