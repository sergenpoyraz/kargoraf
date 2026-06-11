using KargoRaf.Models;

namespace KargoRaf.ViewModels;

public class PackageItemViewModel : ViewModelBase
{
    private bool _isHighlighted;
    private bool _isSearchMatch;
    private bool _isSelected;

    public PackageItemViewModel(Package package)
    {
        Id = package.Id;
        RecipientName = package.RecipientName;
        SectionId = package.SectionId;
        SectionName = package.SectionName;
        Notes = package.Notes;
        CreatedAt = package.CreatedAt;
    }

    public int Id { get; }
    public string RecipientName { get; private set; }
    public int SectionId { get; private set; }
    public string SectionName { get; private set; }
    public string Notes { get; private set; }
    public DateTime CreatedAt { get; private set; }

    public string DisplayText => string.IsNullOrWhiteSpace(Notes)
        ? RecipientName
        : $"{RecipientName} — {Notes}";

    public bool IsHighlighted
    {
        get => _isHighlighted;
        set => SetProperty(ref _isHighlighted, value);
    }

    public bool IsSearchMatch
    {
        get => _isSearchMatch;
        set => SetProperty(ref _isSearchMatch, value);
    }

    public bool IsSelected
    {
        get => _isSelected;
        set => SetProperty(ref _isSelected, value);
    }

    public void UpdateFrom(Package package)
    {
        RecipientName = package.RecipientName;
        SectionId = package.SectionId;
        SectionName = package.SectionName;
        Notes = package.Notes;
        CreatedAt = package.CreatedAt;
        OnPropertyChanged(nameof(RecipientName));
        OnPropertyChanged(nameof(SectionId));
        OnPropertyChanged(nameof(SectionName));
        OnPropertyChanged(nameof(Notes));
        OnPropertyChanged(nameof(DisplayText));
    }
}
