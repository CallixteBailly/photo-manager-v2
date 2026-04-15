using System.Diagnostics;
using PhotoManager.Domain.Interfaces;

namespace PhotoManager.Infrastructure.Services;

public class MacOSFileExplorerService : IFileExplorerService
{
    public void OpenFileInExplorer(string filePath)
    {
        string? directory = Path.GetDirectoryName(filePath);
        if (directory != null)
        {
            Process.Start(new ProcessStartInfo("open", directory) { UseShellExecute = true });
        }
    }

    public void SelectFileInExplorer(string filePath)
    {
        Process.Start(new ProcessStartInfo("open", $"-R "{filePath}"") { UseShellExecute = true });
    }
}
