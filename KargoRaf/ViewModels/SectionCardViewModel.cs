using System.Collections.ObjectModel;
using System.Windows.Media;
using KargoRaf.Models;

namespace KargoRaf.ViewModels;

public class SectionCardViewModel : ViewModelBase
{
    private bool _isSearchHighlighted;
    private string _name = string.Empty;

    public SectionCardViewModel(Section section, System.Windows.Media.Brush accentBrush)
    {
        Id = section.Id;
        SortOrder = section.SortOrder;
        Name = section.Name;
        AccentBrush = accentBrush;
        Packages = new ObservableCollection<PackageItemViewModel>();
    }

    public int Id { get; }
    public int SortOrder { get; }
    public System.Windows.Media.Brush AccentBrush { get; }

    public string Name
    {
        get => _name;
        set => SetProperty(ref _name, value);
    }

    public ObservableCollection<PackageItemViewModel> Packages { get; }

    public int Count => Packages.Count;

    public bool IsEmpty => Count == 0;

    public bool IsSearchHighlighted
    {
        get => _isSearchHighlighted;
        set => SetProperty(ref _isSearchHighlighted, value);
    }

    public void RefreshCount()
    {
        OnPropertyChanged(nameof(Count));
        OnPropertyChanged(nameof(IsEmpty));
    }
}
