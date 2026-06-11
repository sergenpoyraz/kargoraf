using KargoRaf.Models;

namespace KargoRaf.ViewModels;

public class PackageItemViewModel : ViewModelBase
{
    private bool _isHighlighted;
    private bool _isSearchMatch;
    private bool _isSelected;
    private string _recipientName;
    private string _notes;
    private DateTime _createdAt;

    public PackageItemViewModel(Package package)
    {
        Id = package.Id;
        _recipientName = package.RecipientName;
        SectionId = package.SectionId;
        SectionName = package.SectionName;
        _notes = package.Notes;
        _createdAt = package.CreatedAt;
    }

    public int Id { get; }
    public int SectionId { get; private set; }
    public string SectionName { get; private set; }

    public string RecipientName
    {
        get => _recipientName;
        private set => SetProperty(ref _recipientName, value);
    }

    public string Notes
    {
        get => _notes;
        private set
        {
            if (SetProperty(ref _notes, value))
            {
                OnPropertyChanged(nameof(HasNotes));
                OnPropertyChanged(nameof(NotePreview));
                OnPropertyChanged(nameof(NotePreviewShort));
                OnPropertyChanged(nameof(NotesTooltip));
                OnPropertyChanged(nameof(DisplayText));
            }
        }
    }

    public DateTime CreatedAt
    {
        get => _createdAt;
        private set
        {
            if (SetProperty(ref _createdAt, value))
                OnPropertyChanged(nameof(CreatedAtDisplay));
        }
    }

    public bool HasNotes => !string.IsNullOrWhiteSpace(Notes);

    public string NotePreview => HasNotes ? $"Not: {Truncate(Notes, 40)}" : string.Empty;

    public string NotePreviewShort => HasNotes ? Truncate(Notes, 28) : string.Empty;

    public string NotesTooltip => HasNotes
        ? $"{Notes}\n\nÇift tıkla veya Düzenle ile değiştir."
        : string.Empty;

    public string CreatedAtDisplay => CreatedAt.ToString("dd.MM HH:mm");

    public string DisplayText => HasNotes ? $"{RecipientName} — {Notes}" : RecipientName;

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
    }

    private static string Truncate(string text, int maxLength)
    {
        if (string.IsNullOrEmpty(text) || text.Length <= maxLength)
            return text;
        return text[..(maxLength - 1)] + "…";
    }
}
