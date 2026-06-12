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
    private double _rowHeight = WidgetViewModel.EstimatedRowHeight;
    private bool _isTickerPaused;
    private bool _hasPositioned;

    public WidgetWindow(PackageService packageService, SectionService sectionService)
    {
        InitializeComponent();

        _viewModel = new WidgetViewModel(packageService, sectionService);
        DataContext = _viewModel;

        _viewModel.PackageOpenRequested += id => PackageOpenRequested?.Invoke(id);
        _viewModel.TickerResetRequested += ResetTicker;
        _viewModel.PropertyChanged += ViewModel_PropertyChanged;

        Loaded += (_, _) =>
        {
            PositionNearBottomRight(force: true);
            UpdateVisibleCapacity();
        };
        SizeChanged += (_, _) => UpdateVisibleCapacity();
        TickerViewport.SizeChanged += (_, _) => UpdateVisibleCapacity();
        IsVisibleChanged += (_, _) =>
        {
            if (IsVisible)
            {
                _viewModel.Refresh();
                PositionNearBottomRight();
                UpdateVisibleCapacity();
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

        UpdateRowHeightIfNeeded();

        var loopHeight = GetLoopHeight();
        if (loopHeight <= 1)
            return;

        var now = DateTime.UtcNow;
        var elapsedSeconds = Math.Max(0, (now - _lastFrame).TotalSeconds);
        _lastFrame = now;

        const double pixelsPerSecond = 22;
        _scrollOffset += elapsedSeconds * pixelsPerSecond;

        if (_viewModel.UsesRotation)
        {
            while (_scrollOffset >= _rowHeight)
            {
                _scrollOffset -= _rowHeight;
                _viewModel.RotateNext();
            }
        }

        TickerTransform.Y = -_scrollOffset;
    }

    private double GetLoopHeight()
    {
        if (!_viewModel.UsesRotation)
            return _rowHeight;

        return _rowHeight;
    }

    private void UpdateTickerClipHeight()
    {
        if (TickerViewport.ActualHeight <= 0 || _rowHeight <= 1)
            return;

        var visibleRows = Math.Max(1, _viewModel.VisibleRowCount);
        TickerClip.Height = visibleRows * _rowHeight;
    }

    private void UpdateRowHeightIfNeeded()
    {
        if (TickerPrimary.ItemContainerGenerator.Status != System.Windows.Controls.Primitives.GeneratorStatus.ContainersGenerated)
            return;

        if (TickerPrimary.ItemContainerGenerator.ContainerFromIndex(0) is not FrameworkElement firstRow)
            return;

        if (firstRow.ActualHeight > 1)
            _rowHeight = firstRow.ActualHeight;

        UpdateTickerClipHeight();
    }

    private void ViewModel_PropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName is nameof(WidgetViewModel.SearchText) or nameof(WidgetViewModel.UsesRotation))
            ResetTicker();
    }

    private void ResetTicker()
    {
        _scrollOffset = 0;
        TickerTransform.Y = 0;
        _lastFrame = DateTime.UtcNow;
        _rowHeight = WidgetViewModel.EstimatedRowHeight;
        UpdateTickerClipHeight();
    }

    private void UpdateVisibleCapacity()
    {
        if (TickerViewport.ActualHeight <= 0)
            return;

        UpdateRowHeightIfNeeded();
        var capacity = Math.Max(1, (int)Math.Floor(TickerViewport.ActualHeight / _rowHeight));
        _viewModel.SetVisibleCapacity(capacity);
    }

    private void PositionNearBottomRight(bool force = false)
    {
        if (_hasPositioned && !force) return;

        Left = SystemParameters.WorkArea.Right - Width - 24;
        Top = SystemParameters.WorkArea.Bottom - Height - 24;
        _hasPositioned = true;
    }

    private void Border_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
    {
        if (e.ButtonState == MouseButtonState.Pressed)
            DragMove();
    }

    private void Ticker_MouseEnter(object sender, System.Windows.Input.MouseEventArgs e) => _isTickerPaused = true;

    private void Ticker_MouseLeave(object sender, System.Windows.Input.MouseEventArgs e)
    {
        _isTickerPaused = false;
        _lastFrame = DateTime.UtcNow;
    }

    private void CloseButton_Click(object sender, RoutedEventArgs e) => Hide();
}
