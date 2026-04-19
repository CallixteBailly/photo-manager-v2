namespace PhotoManager.UI.Avalonia.Models;

public delegate void FolderRemovedEventHandler(object sender, FolderRemovedEventArgs e);

public class FolderRemovedEventArgs
{
    public required Folder Folder { get; init; }
}
