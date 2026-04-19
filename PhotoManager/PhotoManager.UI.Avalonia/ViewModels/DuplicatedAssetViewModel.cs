namespace PhotoManager.UI.Avalonia.ViewModels;

public class DuplicatedAssetViewModel : BaseViewModel
{
    public required Asset Asset { get; init; }

    public bool IsVisible
    {
        get;
        set
        {
            field = value;
            NotifyPropertyChanged(nameof(IsVisible));
            ParentViewModel.NotifyAssetChanged();
        }
    }

    public DuplicatedSetViewModel ParentViewModel { get; init; } = [];
}
