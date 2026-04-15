namespace PhotoManager.Domain.Interfaces;

public interface IFileExplorerService
{
    void OpenFileInExplorer(string filePath);
    void SelectFileInExplorer(string filePath);
}
