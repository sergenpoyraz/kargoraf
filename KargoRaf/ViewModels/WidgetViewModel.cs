using System.Collections.ObjectModel;
using KargoRaf.Services;

namespace KargoRaf.ViewModels;

public class WidgetViewModel : ViewModelBase
{
    private static WidgetViewModel? _instance;
    public static WidgetViewModel? Instance => _instance;

    private readonly PackageService _packageService;
    private int _totalCount;

    public WidgetViewModel(PackageService packageService, SectionService sectionService)
    {
        _instance = this;
        _packageService = packageService;

        TickerItems = new ObservableCollection<WidgetTickerItem>();

        _packageService.PackagesChanged += Refresh;
        Refresh();
    }

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
            TotalCount = _packageService.GetActiveCount();

            TickerItems.Clear();
            var packages = _packageService.GetActivePackages()
                .OrderBy(p => p.RecipientName, StringComparer.CurrentCultureIgnoreCase)
                .ToList();

            foreach (var p in packages)
            {
                TickerItems.Add(new WidgetTickerItem
                {
                    Id = p.Id,
                    Name = p.RecipientName
                });
            }

            foreach (var p in packages)
            {
                TickerItems.Add(new WidgetTickerItem
                {
                    Id = p.Id,
                    Name = p.RecipientName
                });
            }
        }
        catch (Exception ex)
        {
            LoggingService.Instance.Error("Widget güncellenemedi.", ex);
        }
    }
}

public class WidgetTickerItem
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
}
