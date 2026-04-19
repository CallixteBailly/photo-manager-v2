using System.Collections.ObjectModel;

namespace PhotoManager.UI.Avalonia.ViewModels;

public class SortableObservableCollection<T>(IEnumerable<T> items) : ObservableCollection<T>(items)
{
    public SortableObservableCollection() : this([])
    {
    }

    public void Sort<TKey>(Func<T, TKey> keySelector, bool ascending = true)
    {
        List<T> sortedList = [.. ascending ? this.OrderBy(keySelector) : this.OrderByDescending(keySelector)];
        Clear();
        foreach (T item in sortedList)
        {
            Add(item);
        }
    }

    public void Sort(IComparer<T> comparer)
    {
        List<T> sortedList = [.. this.OrderBy(x => x, comparer)];
        Clear();
        foreach (T item in sortedList)
        {
            Add(item);
        }
    }
}
