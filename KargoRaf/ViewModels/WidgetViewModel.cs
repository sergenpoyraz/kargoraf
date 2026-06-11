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

        _refreshTimer = new DispatcherTimer { Interval = TimeSpan.FromMilliseconds(50) };
        _refreshTimer.Tick += (_, _) =>
        {
            _refreshTimer.Stop();
            ApplyRefresh();
        };

        _packageService.PackagesChanged += ScheduleRefresh;
        ApplyRefresh();
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
            var packages = _packageService.GetActivePackages()
                .OrderByDescending(p => p.CreatedAt)
                .ToList();

            TotalCount = packages.Count;

            var names = packages
                .Select(p => p.RecipientName)
                .Where(n => !string.IsNullOrWhiteSpace(n))
                .ToList();

            TickerItems.Clear();

            if (names.Count == 0)
            {
                TickerResetRequested?.Invoke();
                return;
            }

            foreach (var name in names)
                TickerItems.Add(new WidgetTickerItem { Name = name });

            foreach (var name in names)
                TickerItems.Add(new WidgetTickerItem { Name = name });

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
    public string Name { get; set; } = string.Empty;
}
