using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Threading;
using KargoRaf.Services;
using KargoRaf.ViewModels;

namespace KargoRaf.Views;

public partial class WidgetWindow : Window
{
    private readonly WidgetViewModel _viewModel;
    private double _scrollOffset;
    private double _loopHeight;
    private bool _tickerActive;

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
        _loopHeight = 0;
        _tickerActive = false;
        TickerTransform.Y = 0;

        Dispatcher.BeginInvoke(DispatcherPriority.Loaded, () =>
        {
            TickerPanel.UpdateLayout();
            var totalHeight = TickerPanel.ActualHeight;
            _loopHeight = totalHeight / 2.0;
            _scrollOffset = 0;
            TickerTransform.Y = 0;
            _tickerActive = IsVisible && _loopHeight > 1;
        });
    }

    private void OnRendering(object? sender, EventArgs e)
    {
        if (!_tickerActive || _loopHeight <= 1)
            return;

        _scrollOffset += 0.75;
        if (_scrollOffset >= _loopHeight)
            _scrollOffset = 0;

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
