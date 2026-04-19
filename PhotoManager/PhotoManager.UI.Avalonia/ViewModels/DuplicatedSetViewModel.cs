using System.ComponentModel;

namespace PhotoManager.UI.Avalonia.ViewModels;

public class DuplicatedSetViewModel : List<DuplicatedAssetViewModel>, INotifyPropertyChanged
{
    public event PropertyChangedEventHandler? PropertyChanged;

    protected void NotifyPropertyChanged(params string[] propertyNames)
    {
        foreach (string propertyName in propertyNames)
        {
            PropertyChanged?.Invoke(this, new(propertyName));
        }
    }

    public string FileName => this[0].Asset.FileName;

    public int DuplicatesCount => GetVisibleDuplicates();

    public bool IsVisible => GetVisibleDuplicates() > 1;

    public void NotifyAssetChanged()
    {
        NotifyPropertyChanged(nameof(DuplicatesCount), nameof(IsVisible));
    }

    private int GetVisibleDuplicates()
    {
        return this.Count(a => a.IsVisible);
    }
}
