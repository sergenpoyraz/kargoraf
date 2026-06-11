using System.Collections.ObjectModel;
using System.Windows.Input;
using KargoRaf.Commands;
using KargoRaf.Services;

namespace KargoRaf.ViewModels;

public class WidgetViewModel : ViewModelBase
{
    private static WidgetViewModel? _instance;
    public static WidgetViewModel? Instance => _instance;

    private readonly PackageService _packageService;
    private readonly SectionService _sectionService;

    private string _searchText = string.Empty;
    private int _totalCount;

    public WidgetViewModel(PackageService packageService, SectionService sectionService)
    {
        _instance = this;
        _packageService = packageService;
        _sectionService = sectionService;

        SectionCounts = new ObservableCollection<WidgetSectionCount>();
        RecentNames = new ObservableCollection<WidgetRecentItem>();

        SearchCommand = new RelayCommand<string>(OnRecentClicked);
        ClearSearchCommand = new RelayCommand(() => SearchText = string.Empty);

        _packageService.PackagesChanged += Refresh;
        Refresh();
    }

    public ObservableCollection<WidgetSectionCount> SectionCounts { get; }
    public ObservableCollection<WidgetRecentItem> RecentNames { get; }

    public string SearchText
    {
        get => _searchText;
        set
        {
            if (SetProperty(ref _searchText, value))
                ApplySearch();
        }
    }

    public int TotalCount
    {
        get => _totalCount;
        set => SetProperty(ref _totalCount, value);
    }

    public ICommand SearchCommand { get; }
    public ICommand ClearSearchCommand { get; }

    public event Action<int>? OpenMainWithPackage;

    public void Refresh()
    {
        try
        {
            var sections = _sectionService.GetActiveSections();
            var counts = _packageService.GetCountsBySection(sections.Select(s => s.Id));

            SectionCounts.Clear();
            foreach (var section in sections.OrderBy(s => s.SortOrder))
            {
                counts.TryGetValue(section.Id, out var count);
                SectionCounts.Add(new WidgetSectionCount
                {
                    Label = section.SortOrder.ToString(),
                    Name = section.Name,
                    Count = count
                });
            }

            TotalCount = _packageService.GetActiveCount();
            ApplySearch();
        }
        catch (Exception ex)
        {
            LoggingService.Instance.Error("Widget güncellenemedi.", ex);
        }
    }

    private void ApplySearch()
    {
        RecentNames.Clear();
        var query = SearchText.Trim();
        var packages = string.IsNullOrEmpty(query)
            ? _packageService.GetRecentActive(5)
            : _packageService.SearchActive(query).Take(8);

        foreach (var p in packages)
        {
            RecentNames.Add(new WidgetRecentItem
            {
                Id = p.Id,
                Name = p.RecipientName,
                SectionLabel = p.SectionName
            });
        }
    }

    private void OnRecentClicked(string? param)
    {
        if (param is null || !int.TryParse(param, out var id)) return;
        OpenMainWithPackage?.Invoke(id);
    }

    public void NotifyPackageClicked(int id) => OpenMainWithPackage?.Invoke(id);
}

public class WidgetSectionCount
{
    public string Label { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public int Count { get; set; }
    public string Display => $"{Label}: {Count}";
}

public class WidgetRecentItem
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string SectionLabel { get; set; } = string.Empty;
    public string Display => $"{Name} ({SectionLabel})";
}
