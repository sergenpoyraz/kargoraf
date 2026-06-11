using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using KargoRaf.Services;
using KargoRaf.ViewModels;

namespace KargoRaf.Views;

public partial class HistoryWindow : Window
{
    private readonly HistoryViewModel _viewModel;

    public HistoryWindow(PackageService packageService)
    {
        InitializeComponent();
        _viewModel = new HistoryViewModel(packageService);
        DataContext = _viewModel;

        BuildFilterChips();
        _viewModel.PropertyChanged += (_, e) =>
        {
            if (e.PropertyName == nameof(HistoryViewModel.SelectedFilter))
                UpdateFilterSelection();
        };
    }

    private void BuildFilterChips()
    {
        foreach (var option in _viewModel.FilterItems)
        {
            var chip = new ToggleButton
            {
                Content = option.Label,
                Tag = option.Filter,
                Style = (Style)FindResource("FilterChipButton")
            };
            chip.Click += FilterChip_Click;
            FilterPanel.Children.Add(chip);
        }
        UpdateFilterSelection();
    }

    private void FilterChip_Click(object sender, RoutedEventArgs e)
    {
        if (sender is ToggleButton btn && btn.Tag is HistoryFilter filter)
            _viewModel.SelectedFilter = filter;
    }

    private void UpdateFilterSelection()
    {
        foreach (var child in FilterPanel.Children)
        {
            if (child is ToggleButton btn && btn.Tag is HistoryFilter filter)
                btn.IsChecked = filter == _viewModel.SelectedFilter;
        }
    }
}
