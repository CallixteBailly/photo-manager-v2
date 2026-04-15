using System.Diagnostics;
using PhotoManager.Domain.Interfaces;

namespace PhotoManager.Infrastructure.Services;

public class LinuxFileExplorerService : IFileExplorerService
{
    public void OpenFileInExplorer(string filePath)
    {
        string? directory = Path.GetDirectoryName(filePath);
        if (directory != null)
        {
            Process.Start(new ProcessStartInfo("xdg-open", directory) { UseShellExecute = true });
        }
    }

    public void SelectFileInExplorer(string filePath)
    {
        // dbus-send can highlight a specific file in Nautilus
        Process.Start(new ProcessStartInfo("dbus-send",
            "--session --dest=org.freedesktop.FileManager1 " +
            "--type=method_call /org/freedesktop/FileManager1 " +
            "org.freedesktop.FileManager1.ShowItems " +
            $"array:string:file://{filePath} string:""") { UseShellExecute = true });
    }
}
