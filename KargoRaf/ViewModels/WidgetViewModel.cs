using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Threading;
using KargoRaf.Services;

namespace KargoRaf.ViewModels;

public class WidgetViewModel : ViewModelBase
{
    private static WidgetViewModel? _instance;
    public static WidgetViewModel? Instance => _instance;

    private readonly PackageService _packageService;
    private readonly DispatcherTimer _refreshTimer;
    private int _totalCount;

    public WidgetViewModel(PackageService packageService, SectionService sectionService)
    {
        _instance = this;
        _packageService = packageService;

        TickerItems = new ObservableCollection<WidgetTickerItem>();

        _refreshTimer = new DispatcherTimer { Interval = TimeSpan.FromMilliseconds(80) };
        _refreshTimer.Tick += (_, _) =>
        {
            _refreshTimer.Stop();
            ApplyRefresh();
        };

        _packageService.PackagesChanged += ScheduleRefresh;
        ScheduleRefresh();
    }

    public ObservableCollection<WidgetTickerItem> TickerItems { get; }

    public int TotalCount
    {
        get => _totalCount;
        set => SetProperty(ref _totalCount, value);
    }

    public event Action? TickerResetRequested;

    public void Refresh() => ScheduleRefresh();

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

            if (packages.Count > 0)
            {
                foreach (var p in packages)
                {
                    TickerItems.Add(new WidgetTickerItem
                    {
                        Id = p.Id,
                        Name = p.RecipientName
                    });
                }
            }

            TickerResetRequested?.Invoke();
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
