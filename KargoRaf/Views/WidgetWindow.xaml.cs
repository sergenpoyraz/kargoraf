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

    public WidgetWindow(PackageService packageService, SectionService sectionService)
    {
        InitializeComponent();

        _viewModel = new WidgetViewModel(packageService, sectionService);
        DataContext = _viewModel;

        _tickerTimer = new DispatcherTimer { Interval = TimeSpan.FromMilliseconds(35) };
        _tickerTimer.Tick += TickerTimer_Tick;

        Loaded += (_, _) =>
        {
            PositionBottomRight();
            _tickerTimer.Start();
        };

        Unloaded += (_, _) => _tickerTimer.Stop();
    }

    private void TickerTimer_Tick(object? sender, EventArgs e)
    {
        if (TickerScroll.ScrollableHeight <= 0) return;

        var next = TickerScroll.VerticalOffset + 0.7;
        if (next >= TickerScroll.ScrollableHeight)
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
