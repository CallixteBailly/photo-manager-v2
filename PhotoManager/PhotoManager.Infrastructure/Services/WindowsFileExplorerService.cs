using System.Diagnostics;

namespace PhotoManager.Infrastructure.Services;

public class WindowsFileExplorerService : IFileExplorerService
{
    public void OpenFileInExplorer(string filePath)
    {
        Process.Start(new ProcessStartInfo("explorer.exe", $"/select,\"{filePath}\"") { UseShellExecute = true });
    }

    public void SelectFileInExplorer(string filePath)
    {
        OpenFileInExplorer(filePath);
    }
}
