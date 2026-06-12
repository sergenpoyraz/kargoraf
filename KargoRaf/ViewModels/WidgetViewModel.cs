using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Input;
using System.Windows.Threading;
using KargoRaf.Commands;
using KargoRaf.Services;

namespace KargoRaf.ViewModels;

public class WidgetViewModel : ViewModelBase
{
    private static WidgetViewModel? _instance;
    public static WidgetViewModel? Instance => _instance;

    private readonly PackageService _packageService;
    private readonly DispatcherTimer _refreshTimer;

    private int _totalCount;
    private string _searchText = string.Empty;
    private bool _isEmpty = true;
    private bool _shouldAnimate;

    public WidgetViewModel(PackageService packageService, SectionService sectionService)
    {
        _instance = this;
        _packageService = packageService;

        TickerItems = new ObservableCollection<WidgetTickerItem>();
        LoopItems = new ObservableCollection<WidgetTickerItem>();
        OpenPackageCommand = new RelayCommand<WidgetTickerItem>(OpenPackage);
        ClearSearchCommand = new RelayCommand(() => SearchText = string.Empty);

        _refreshTimer = new DispatcherTimer { Interval = TimeSpan.FromMilliseconds(50) };
        _refreshTimer.Tick += (_, _) =>
        {
            _refreshTimer.Stop();
            ApplyRefresh();
        };

        _packageService.PackagesChanged += ScheduleRefresh;
        sectionService.SectionsChanged += ScheduleRefresh;
        ApplyRefresh();
    }

    public ObservableCollection<WidgetTickerItem> TickerItems { get; }
    public ObservableCollection<WidgetTickerItem> LoopItems { get; }

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

    public ICommand OpenPackageCommand { get; }
    public ICommand ClearSearchCommand { get; }

    public event Action<int>? PackageOpenRequested;

    public void Refresh() => ScheduleRefresh();

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
            dispatcher.BeginInvoke(ScheduleRefresh);
            return;
        }

        _refreshTimer.Stop();
        _refreshTimer.Start();
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

            TickerItems.Clear();
            foreach (var package in filtered)
            {
                TickerItems.Add(new WidgetTickerItem
                {
                    Id = package.Id,
                    Name = package.RecipientName,
                    HasNotes = !string.IsNullOrWhiteSpace(package.Notes),
                    NotePreview = TruncateNote(package.Notes)
                });
            }

            RebuildLoopItems();
            IsEmpty = TickerItems.Count == 0;
            ShouldAnimate = TickerItems.Count > 0;
        }
        catch (Exception ex)
        {
            LoggingService.Instance.Error("Widget güncellenemedi.", ex);
        }
    }

    private void RebuildLoopItems()
    {
        LoopItems.Clear();
        if (TickerItems.Count == 0) return;

        const int repeatedSets = 10;
        for (var set = 0; set < repeatedSets; set++)
        {
            foreach (var item in TickerItems)
                LoopItems.Add(item);
        }
    }

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
