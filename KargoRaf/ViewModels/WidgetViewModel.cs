using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Input;
using KargoRaf.Commands;
using KargoRaf.Services;

namespace KargoRaf.ViewModels;

public class WidgetViewModel : ViewModelBase
{
    public const int MaxVisibleRows = 8;
    public const double EstimatedRowHeight = 52;

    private static WidgetViewModel? _instance;
    public static WidgetViewModel? Instance => _instance;

    private readonly PackageService _packageService;
    private readonly List<WidgetTickerItem> _sourceItems = [];
    private string _searchText = string.Empty;
    private int _totalCount;
    private bool _isEmpty = true;
    private bool _shouldAnimate;
    private bool _usesRotation;
    private string _snapshot = string.Empty;
    private int _rotationIndex;
    private int _visibleCapacity = 4;

    public WidgetViewModel(PackageService packageService, SectionService sectionService)
    {
        _instance = this;
        _packageService = packageService;

        TickerItems = new ObservableCollection<WidgetTickerItem>();
        OpenPackageCommand = new RelayCommand<WidgetTickerItem>(OpenPackage);
        ClearSearchCommand = new RelayCommand(() => SearchText = string.Empty);

        _packageService.PackagesChanged += ScheduleRefresh;
        sectionService.SectionsChanged += ScheduleRefresh;
        ApplyRefresh();
    }

    public ObservableCollection<WidgetTickerItem> TickerItems { get; }

    public int TotalCount
    {
        get => _totalCount;
        set => SetProperty(ref _totalCount, value);
    }

    public string SearchText
    {
        get => _searchText;
        set
        {
            if (SetProperty(ref _searchText, value))
                ApplyRefresh();
        }
    }

    public bool IsEmpty
    {
        get => _isEmpty;
        private set => SetProperty(ref _isEmpty, value);
    }

    public bool ShouldAnimate
    {
        get => _shouldAnimate;
        private set => SetProperty(ref _shouldAnimate, value);
    }

    public bool UsesRotation
    {
        get => _usesRotation;
        private set => SetProperty(ref _usesRotation, value);
    }

    public int SourceItemCount => _sourceItems.Count;

    public double LoopHeight =>
        UsesRotation
            ? MaxVisibleRows * EstimatedRowHeight
            : Math.Max(EstimatedRowHeight, _sourceItems.Count * EstimatedRowHeight);

    public ICommand OpenPackageCommand { get; }
    public ICommand ClearSearchCommand { get; }

    public event Action<int>? PackageOpenRequested;
    public event Action? TickerResetRequested;

    public void Refresh() => ScheduleRefresh();

    public void SetVisibleCapacity(int capacity)
    {
        capacity = Math.Max(1, capacity);
        if (_visibleCapacity == capacity)
            return;

        _visibleCapacity = capacity;
        UpdateAnimationState();
        RebuildTickerDisplay();
        TickerResetRequested?.Invoke();
    }

    public void RotateNext()
    {
        if (!UsesRotation || _sourceItems.Count == 0)
            return;

        _rotationIndex = (_rotationIndex + 1) % _sourceItems.Count;
        TickerItems.RemoveAt(0);
        var nextIndex = (_rotationIndex + MaxVisibleRows - 1) % _sourceItems.Count;
        TickerItems.Add(_sourceItems[nextIndex]);
    }

    private void OpenPackage(WidgetTickerItem? item)
    {
        if (item is null) return;
        PackageOpenRequested?.Invoke(item.Id);
    }

    private void ScheduleRefresh()
    {
        var dispatcher = Application.Current?.Dispatcher;
        if (dispatcher is not null && !dispatcher.CheckAccess())
        {
            dispatcher.BeginInvoke(ApplyRefresh);
            return;
        }

        ApplyRefresh();
    }

    private void ApplyRefresh()
    {
        try
        {
            var packages = _packageService.GetActivePackages()
                .OrderByDescending(p => p.CreatedAt)
                .ToList();

            TotalCount = packages.Count;

            var query = SearchText.Trim();
            var filtered = string.IsNullOrEmpty(query)
                ? packages
                : packages.Where(p =>
                    p.RecipientName.Contains(query, StringComparison.CurrentCultureIgnoreCase) ||
                    p.Notes.Contains(query, StringComparison.CurrentCultureIgnoreCase)).ToList();

            var snapshot = BuildSnapshot(filtered);
            if (snapshot == _snapshot)
                return;

            _snapshot = snapshot;
            _sourceItems.Clear();
            _sourceItems.AddRange(filtered.Select(package => new WidgetTickerItem
            {
                Id = package.Id,
                Name = package.RecipientName,
                HasNotes = !string.IsNullOrWhiteSpace(package.Notes),
                NotePreview = TruncateNote(package.Notes)
            }));

            IsEmpty = _sourceItems.Count == 0;
            _rotationIndex = 0;
            UpdateAnimationState();
            RebuildTickerDisplay();
            TickerResetRequested?.Invoke();
        }
        catch (Exception ex)
        {
            LoggingService.Instance.Error("Widget güncellenemedi.", ex);
        }
    }

    private void UpdateAnimationState()
    {
        UsesRotation = _sourceItems.Count > MaxVisibleRows;
        ShouldAnimate = _sourceItems.Count > _visibleCapacity;
    }

    private void RebuildTickerDisplay()
    {
        TickerItems.Clear();
        if (_sourceItems.Count == 0)
            return;

        if (UsesRotation)
        {
            for (var i = 0; i < MaxVisibleRows; i++)
                TickerItems.Add(_sourceItems[(_rotationIndex + i) % _sourceItems.Count]);
            return;
        }

        foreach (var item in _sourceItems)
            TickerItems.Add(item);

        if (ShouldAnimate && _sourceItems.Count >= 2)
        {
            foreach (var item in _sourceItems)
                TickerItems.Add(item);
        }
    }

    private static string BuildSnapshot(IEnumerable<Models.Package> packages) =>
        string.Join('|', packages.Select(p =>
            $"{p.Id}:{p.RecipientName}:{p.Notes?.Trim()}"));

    private static string TruncateNote(string notes)
    {
        if (string.IsNullOrWhiteSpace(notes)) return string.Empty;
        notes = notes.Trim();
        return notes.Length <= 38 ? notes : notes[..37] + "…";
    }
}

public class WidgetTickerItem
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public bool HasNotes { get; set; }
    public string NotePreview { get; set; } = string.Empty;
}
