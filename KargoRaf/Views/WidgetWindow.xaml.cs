using System.Collections.Specialized;
using System.ComponentModel;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using KargoRaf.Services;
using KargoRaf.ViewModels;

namespace KargoRaf.Views;

public partial class WidgetWindow : Window
{
    private readonly WidgetViewModel _viewModel;
    private DateTime _lastFrame = DateTime.UtcNow;
    private double _scrollOffset;
    private bool _isTickerPaused;

    public WidgetWindow(PackageService packageService, SectionService sectionService)
    {
        InitializeComponent();

        _viewModel = new WidgetViewModel(packageService, sectionService);
        DataContext = _viewModel;

        _viewModel.PackageOpenRequested += id => PackageOpenRequested?.Invoke(id);
        _viewModel.PropertyChanged += ViewModel_PropertyChanged;
        _viewModel.TickerItems.CollectionChanged += TickerItems_CollectionChanged;

        Loaded += (_, _) => PositionNearBottomRight();
        IsVisibleChanged += (_, _) =>
        {
            if (IsVisible)
            {
                _viewModel.Refresh();
                PositionNearBottomRight();
                _lastFrame = DateTime.UtcNow;
            }
        };

        CompositionTarget.Rendering += CompositionTarget_Rendering;
    }

    public event Action<int>? PackageOpenRequested;

    private void CompositionTarget_Rendering(object? sender, EventArgs e)
    {
        if (!IsVisible || _isTickerPaused || !_viewModel.ShouldAnimate)
        {
            _lastFrame = DateTime.UtcNow;
            return;
        }

        var loopHeight = TickerPrimary.ActualHeight;
        if (loopHeight <= TickerViewport.ActualHeight || loopHeight <= 0)
        {
            ResetTicker();
            return;
        }

        var now = DateTime.UtcNow;
        var elapsedSeconds = Math.Max(0, (now - _lastFrame).TotalSeconds);
        _lastFrame = now;

        const double pixelsPerSecond = 18;
        _scrollOffset += elapsedSeconds * pixelsPerSecond;
        if (_scrollOffset >= loopHeight)
            _scrollOffset -= loopHeight;

        TickerTransform.Y = -_scrollOffset;
    }

    private void ViewModel_PropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName == nameof(WidgetViewModel.ShouldAnimate) ||
            e.PropertyName == nameof(WidgetViewModel.SearchText))
        {
            ResetTicker();
        }
    }

    private void TickerItems_CollectionChanged(object? sender, NotifyCollectionChangedEventArgs e) => ResetTicker();

    private void ResetTicker()
    {
        _scrollOffset = 0;
        TickerTransform.Y = 0;
        _lastFrame = DateTime.UtcNow;
    }

    private void PositionNearBottomRight()
    {
        Left = SystemParameters.WorkArea.Right - Width - 24;
        Top = SystemParameters.WorkArea.Bottom - Height - 24;
    }

    private void Border_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
    {
        if (e.ButtonState == MouseButtonState.Pressed)
            DragMove();
    }

    private void Ticker_MouseEnter(object sender, MouseEventArgs e) => _isTickerPaused = true;

    private void Ticker_MouseLeave(object sender, MouseEventArgs e)
    {
        _isTickerPaused = false;
        _lastFrame = DateTime.UtcNow;
    }

    private void CloseButton_Click(object sender, RoutedEventArgs e) => Hide();
}
