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
    private readonly SectionService _sectionService;
    private readonly DispatcherTimer _refreshTimer;

    private int _totalCount;
    private string _searchText = string.Empty;

    public WidgetViewModel(PackageService packageService, SectionService sectionService)
    {
        _instance = this;
        _packageService = packageService;
        _sectionService = sectionService;

        SectionCounts = new ObservableCollection<WidgetSectionCount>();
        RecentItems = new ObservableCollection<WidgetRecentItem>();

        OpenPackageCommand = new RelayCommand<WidgetRecentItem>(OpenPackage);

        _refreshTimer = new DispatcherTimer { Interval = TimeSpan.FromMilliseconds(50) };
        _refreshTimer.Tick += (_, _) =>
        {
            _refreshTimer.Stop();
            ApplyRefresh();
        };

        _packageService.PackagesChanged += ScheduleRefresh;
        _sectionService.SectionsChanged += ScheduleRefresh;
        ApplyRefresh();
    }

    public ObservableCollection<WidgetSectionCount> SectionCounts { get; }
    public ObservableCollection<WidgetRecentItem> RecentItems { get; }

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

    public ICommand OpenPackageCommand { get; }

    public event Action<int>? PackageOpenRequested;

    public void Refresh() => ScheduleRefresh();

    private void OpenPackage(WidgetRecentItem? item)
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
            var sections = _sectionService.GetActiveSections().OrderBy(s => s.SortOrder).ToList();
            var packages = _packageService.GetActivePackages();
            TotalCount = packages.Count;

            SectionCounts.Clear();
            foreach (var section in sections)
            {
                var count = packages.Count(p => p.SectionId == section.Id);
                SectionCounts.Add(new WidgetSectionCount
                {
                    SortOrder = section.SortOrder,
                    Count = count
                });
            }

            var query = SearchText.Trim();
            var filtered = string.IsNullOrEmpty(query)
                ? packages
                : packages.Where(p =>
                    p.RecipientName.Contains(query, StringComparison.CurrentCultureIgnoreCase) ||
                    p.Notes.Contains(query, StringComparison.CurrentCultureIgnoreCase)).ToList();

            RecentItems.Clear();
            foreach (var package in filtered.OrderByDescending(p => p.CreatedAt).Take(6))
            {
                var section = sections.FirstOrDefault(s => s.Id == package.SectionId);
                RecentItems.Add(new WidgetRecentItem
                {
                    Id = package.Id,
                    Name = package.RecipientName,
                    SectionOrder = section?.SortOrder ?? 0,
                    HasNotes = !string.IsNullOrWhiteSpace(package.Notes),
                    NotePreview = TruncateNote(package.Notes)
                });
            }
        }
        catch (Exception ex)
        {
            LoggingService.Instance.Error("Widget güncellenemedi.", ex);
        }
    }

    private static string TruncateNote(string notes)
    {
        if (string.IsNullOrWhiteSpace(notes)) return string.Empty;
        notes = notes.Trim();
        return notes.Length <= 32 ? notes : notes[..31] + "…";
    }
}

public class WidgetSectionCount
{
    public int SortOrder { get; set; }
    public int Count { get; set; }
    public bool HasItems => Count > 0;
    public string Display => $"Bölüm {SortOrder} · {Count}";
}

public class WidgetRecentItem
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public int SectionOrder { get; set; }
    public bool HasNotes { get; set; }
    public string NotePreview { get; set; } = string.Empty;
    public string SectionLabel => SectionOrder > 0 ? $"Bölüm {SectionOrder}" : string.Empty;
}
