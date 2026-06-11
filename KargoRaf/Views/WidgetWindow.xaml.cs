using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Threading;
using KargoRaf.Services;
using KargoRaf.ViewModels;

namespace KargoRaf.Views;

public partial class WidgetWindow : Window
{
    private const double ScrollSpeedPxPerSec = 24;

    private readonly WidgetViewModel _viewModel;
    private double _scrollOffset;
    private double _cycleHeight;
    private bool _tickerActive;
    private DateTime _lastFrame = DateTime.UtcNow;

    public WidgetWindow(PackageService packageService, SectionService sectionService)
    {
        InitializeComponent();

        _viewModel = new WidgetViewModel(packageService, sectionService);
        DataContext = _viewModel;

        _viewModel.TickerResetRequested += ResetTicker;

        Loaded += (_, _) =>
        {
            PositionBottomRight();
            ResetTicker();
            CompositionTarget.Rendering += OnRendering;
        };

        Unloaded += (_, _) => CompositionTarget.Rendering -= OnRendering;

        IsVisibleChanged += (_, _) =>
        {
            if (IsVisible)
            {
                _viewModel.Refresh();
                ResetTicker();
            }
            else
            {
                _tickerActive = false;
            }
        };
    }

    private void ResetTicker()
    {
        _scrollOffset = 0;
        _cycleHeight = 0;
        _tickerActive = false;
        _lastFrame = DateTime.UtcNow;
        TickerTransform.Y = 0;

        Dispatcher.BeginInvoke(DispatcherPriority.Loaded, TryStartTicker);
        Dispatcher.BeginInvoke(DispatcherPriority.ApplicationIdle, TryStartTicker);
    }

    private void TryStartTicker()
    {
        if (_viewModel.UniqueNameCount <= 0)
        {
            _tickerActive = false;
            return;
        }

        TickerPanel.UpdateLayout();
        var total = TickerPanel.ActualHeight;
        if (total <= 1) return;

        _cycleHeight = total / 2.0;
        if (_scrollOffset >= _cycleHeight)
            _scrollOffset %= _cycleHeight;

        TickerTransform.Y = -_scrollOffset;
        _tickerActive = IsVisible && _cycleHeight > 1;
        _lastFrame = DateTime.UtcNow;
    }

    private void OnRendering(object? sender, EventArgs e)
    {
        if (!_tickerActive || _cycleHeight <= 1)
            return;

        var now = DateTime.UtcNow;
        var delta = (now - _lastFrame).TotalSeconds;
        _lastFrame = now;

        if (delta <= 0 || delta > 0.5) return;

        _scrollOffset += ScrollSpeedPxPerSec * delta;

        while (_scrollOffset >= _cycleHeight)
            _scrollOffset -= _cycleHeight;

        TickerTransform.Y = -_scrollOffset;
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
