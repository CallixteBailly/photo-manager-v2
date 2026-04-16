using System.Diagnostics;

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
        string uri = "file://" + filePath;
        string args =
            "--session --dest=org.freedesktop.FileManager1 --type=method_call " +
            "/org/freedesktop/FileManager1 org.freedesktop.FileManager1.ShowItems " +
            "array:string:" + uri + " string:";

        Process.Start(new ProcessStartInfo("dbus-send", args) { UseShellExecute = true });
    }
}
