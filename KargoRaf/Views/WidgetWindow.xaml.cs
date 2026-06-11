using System.Windows;
using System.Windows.Input;
using System.Windows.Threading;
using KargoRaf.Services;
using KargoRaf.ViewModels;

namespace KargoRaf.Views;

public partial class WidgetWindow : Window
{
    private readonly WidgetViewModel _viewModel;
    private readonly DispatcherTimer _tickerTimer;
    private bool _tickerPaused;

    public WidgetWindow(PackageService packageService, SectionService sectionService)
    {
        InitializeComponent();

        _viewModel = new WidgetViewModel(packageService, sectionService);
        DataContext = _viewModel;

        _viewModel.TickerResetRequested += ResetTickerScroll;

        _tickerTimer = new DispatcherTimer { Interval = TimeSpan.FromMilliseconds(35) };
        _tickerTimer.Tick += TickerTimer_Tick;

        Loaded += (_, _) =>
        {
            PositionBottomRight();
            ResetTickerScroll();
            _tickerTimer.Start();
        };

        IsVisibleChanged += (_, e) =>
        {
            if (IsVisible)
            {
                ResetTickerScroll();
                _tickerTimer.Start();
            }
            else
            {
                _tickerTimer.Stop();
            }
        };

        Unloaded += (_, _) => _tickerTimer.Stop();
    }

    private void ResetTickerScroll()
    {
        _tickerPaused = true;
        _tickerTimer.Stop();
        TickerScroll.ScrollToVerticalOffset(0);

        Dispatcher.BeginInvoke(DispatcherPriority.Loaded, () =>
        {
            TickerScroll.ScrollToVerticalOffset(0);
            _tickerPaused = false;
            if (IsVisible && TickerScroll.ScrollableHeight > 1)
                _tickerTimer.Start();
        });
    }

    private void TickerTimer_Tick(object? sender, EventArgs e)
    {
        if (_tickerPaused) return;

        var scrollable = TickerScroll.ScrollableHeight;
        if (scrollable <= 1) return;

        var loopHeight = scrollable / 2.0;
        if (loopHeight <= 1) return;

        var next = TickerScroll.VerticalOffset + 0.7;
        if (next >= loopHeight)
            next = 0;

        TickerScroll.ScrollToVerticalOffset(next);
    }

    private void PositionBottomRight()
    {
        var workArea = SystemParameters.WorkArea;
        Left = workArea.Right - Width - 24;
        Top = workArea.Bottom - Height - 24;
    }

    private void Border_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
    {
        if (e.LeftButton == MouseButtonState.Pressed)
            DragMove();
    }

    private void CloseButton_Click(object sender, RoutedEventArgs e) => Hide();
}
