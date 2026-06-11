using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Input;
using KargoRaf.Commands;
using KargoRaf.Models;
using KargoRaf.Services;

namespace KargoRaf.ViewModels;

public class HistoryViewModel : ViewModelBase
{
    private readonly PackageService _packageService;
    private HistoryFilter _selectedFilter = HistoryFilter.Today;

    public HistoryViewModel(PackageService packageService)
    {
        _packageService = packageService;
        Items = new ObservableCollection<Package>();
        FilterItems =
        [
            new FilterOption(HistoryFilter.Today, "Bugün"),
            new FilterOption(HistoryFilter.Last7Days, "Son 7 gün"),
            new FilterOption(HistoryFilter.ThisMonth, "Bu ay"),
            new FilterOption(HistoryFilter.All, "Tüm geçmiş")
        ];
        SelectedFilterItem = FilterItems[0];
        RestoreCommand = new RelayCommand<Package>(Restore);
        RefreshCommand = new RelayCommand(Load);
        Load();
    }

    public ObservableCollection<Package> Items { get; }
    public List<FilterOption> FilterItems { get; }

    private FilterOption _selectedFilterItem = null!;

    public FilterOption SelectedFilterItem
    {
        get => _selectedFilterItem;
        set
        {
            if (SetProperty(ref _selectedFilterItem, value) && value is not null)
            {
                _selectedFilter = value.Filter;
                Load();
            }
        }
    }

    public HistoryFilter SelectedFilter
    {
        get => _selectedFilter;
        set
        {
            if (SetProperty(ref _selectedFilter, value))
                Load();
        }
    }

    public ICommand RestoreCommand { get; }
    public ICommand RefreshCommand { get; }

    private void Load()
    {
        try
        {
            Items.Clear();
            foreach (var item in _packageService.GetDeliveredHistory(SelectedFilter))
                Items.Add(item);
        }
        catch (Exception ex)
        {
            LoggingService.Instance.Error("Geçmiş yüklenemedi.", ex);
            MessageBox.Show($"Geçmiş yüklenemedi:\n{ex.Message}", "Hata", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    private void Restore(Package? package)
    {
        if (package is null) return;
        try
        {
            _packageService.RestoreFromHistory(package.Id);
            Load();
            WidgetViewModel.Instance?.Refresh();
        }
        catch (Exception ex)
        {
            LoggingService.Instance.Error("Kayıt geri yüklenemedi.", ex);
            MessageBox.Show($"Kayıt geri yüklenemedi:\n{ex.Message}", "Hata", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }
}

public class FilterOption
{
    public FilterOption(HistoryFilter filter, string label)
    {
        Filter = filter;
        Label = label;
    }

    public HistoryFilter Filter { get; }
    public string Label { get; }
}
