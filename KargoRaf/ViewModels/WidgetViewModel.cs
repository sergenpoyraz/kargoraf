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

    private int _totalCount;

    public WidgetViewModel(PackageService packageService, SectionService sectionService)
    {
        _instance = this;
        _packageService = packageService;
        _sectionService = sectionService;

        SectionCounts = new ObservableCollection<WidgetSectionCount>();
        TickerItems = new ObservableCollection<WidgetTickerItem>();

        _packageService.PackagesChanged += Refresh;
        Refresh();
    }

    public ObservableCollection<WidgetSectionCount> SectionCounts { get; }
    public ObservableCollection<WidgetTickerItem> TickerItems { get; }

    public int TotalCount
    {
        get => _totalCount;
        set => SetProperty(ref _totalCount, value);
    }

    public void Refresh()
    {
        try
        {
            var sections = _sectionService.GetActiveSections();
            var counts = _packageService.GetCountsBySection(sections.Select(s => s.Id));
            var sectionById = sections.ToDictionary(s => s.Id);

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

            TickerItems.Clear();
            var packages = _packageService.GetActivePackages()
                .OrderBy(p => sectionById.GetValueOrDefault(p.SectionId)?.SortOrder ?? 99)
                .ThenBy(p => p.RecipientName, StringComparer.CurrentCultureIgnoreCase)
                .ToList();

            foreach (var p in packages)
            {
                sectionById.TryGetValue(p.SectionId, out var section);
                TickerItems.Add(new WidgetTickerItem
                {
                    Id = p.Id,
                    Name = p.RecipientName,
                    SectionOrder = section?.SortOrder.ToString() ?? "?"
                });
            }

            // Kesintisiz kaydırma için listeyi iki kez göster
            foreach (var p in packages)
            {
                sectionById.TryGetValue(p.SectionId, out var section);
                TickerItems.Add(new WidgetTickerItem
                {
                    Id = p.Id,
                    Name = p.RecipientName,
                    SectionOrder = section?.SortOrder.ToString() ?? "?"
                });
            }
        }
        catch (Exception ex)
        {
            LoggingService.Instance.Error("Widget güncellenemedi.", ex);
        }
    }
}

public class WidgetSectionCount
{
    public string Label { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public int Count { get; set; }
}

public class WidgetTickerItem
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string SectionOrder { get; set; } = string.Empty;
}
