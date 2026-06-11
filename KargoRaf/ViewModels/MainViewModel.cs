using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using KargoRaf.Commands;
using KargoRaf.Models;
using KargoRaf.Services;
using KargoRaf.Views;

namespace KargoRaf.ViewModels;

public class MainViewModel : ViewModelBase
{
    private readonly PackageService _packageService;
    private readonly SectionService _sectionService;
    private readonly UndoService _undoService;
    private readonly BackupService _backupService;

    private string _quickAddName = string.Empty;
    private string _quickAddNotes = string.Empty;
    private bool _showQuickAddNotes;
    private string _searchText = string.Empty;
    private int _selectedSectionNumber = 1;
    private int _totalActiveCount;
    private int _todayAddedCount;
    private int _notedPackageCount;
    private string _busiestSectionDisplay = "—";
    private string _statusMessage = string.Empty;
    private bool _showUndoBar;
    private bool _showStatusToast;
    private bool _showNoSearchResults;
    private string _undoMessage = string.Empty;
    private string _warningMessage = string.Empty;
    private PackageItemViewModel? _selectedPackage;
    private int? _highlightPackageId;
    private string _selectedSectionDisplay = "Bölüm 1";
    private string _keyboardHint = "Ctrl+1..5 ile hızlı ekle · Enter ile ekle · Ctrl+F ara";
    private int _sectionGridColumns = 2;

    private static readonly Brush[] AccentBrushes =
    [
        (Brush)new BrushConverter().ConvertFrom("#2563EB")!,
        (Brush)new BrushConverter().ConvertFrom("#3B82F6")!,
        (Brush)new BrushConverter().ConvertFrom("#1D4ED8")!,
        (Brush)new BrushConverter().ConvertFrom("#60A5FA")!,
        (Brush)new BrushConverter().ConvertFrom("#93C5FD")!,
    ];

    public MainViewModel(
        PackageService packageService,
        SectionService sectionService,
        UndoService undoService,
        BackupService backupService)
    {
        _packageService = packageService;
        _sectionService = sectionService;
        _undoService = undoService;
        _backupService = backupService;

        Sections = new ObservableCollection<SectionCardViewModel>();
        SectionButtons = new ObservableCollection<int>();

        AddCommand = new RelayCommand(() => AddToSelectedSection());
        DeliverCommand = new RelayCommand<PackageItemViewModel>(DeliverPackage);
        UndoCommand = new RelayCommand(UndoLastDelivery, () => ShowUndoBar);
        OpenHistoryCommand = new RelayCommand(OpenHistory);
        OpenSettingsCommand = new RelayCommand(OpenSettings);
        BackupCommand = new RelayCommand(CreateBackup);
        SelectSectionCommand = new RelayCommand<int>(n => SelectedSectionNumber = n);
        AddToSectionCommand = new RelayCommand(p =>
        {
            var sectionNumber = p switch
            {
                int i => i,
                string s when int.TryParse(s, out var parsed) => parsed,
                _ => SelectedSectionNumber
            };
            AddToSection(sectionNumber);
        });
        EditPackageCommand = new RelayCommand<PackageItemViewModel>(EditPackage);
        ViewNotesCommand = new RelayCommand<PackageItemViewModel>(ViewNotes);
        ToggleQuickAddNotesCommand = new RelayCommand(() => ShowQuickAddNotes = !ShowQuickAddNotes);
        ClearSearchCommand = new RelayCommand(ClearSearch);
        ToggleWidgetCommand = new RelayCommand(() => ToggleWidgetRequested?.Invoke());
        DeliverSelectedCommand = new RelayCommand(() =>
        {
            if (SelectedPackage is not null)
                DeliverPackage(SelectedPackage);
        }, () => SelectedPackage is not null);

        _packageService.PackagesChanged += RefreshAll;
        _sectionService.SectionsChanged += RefreshSections;
        _undoService.UndoAvailable += OnUndoAvailable;
        _undoService.UndoExpired += () =>
        {
            ShowUndoBar = false;
            UndoMessage = string.Empty;
        };

        RefreshSections();
        RefreshAll();
        UpdateSelectedSectionDisplay();
    }

    public ObservableCollection<SectionCardViewModel> Sections { get; }
    public ObservableCollection<int> SectionButtons { get; }

    public string QuickAddName
    {
        get => _quickAddName;
        set
        {
            if (SetProperty(ref _quickAddName, value))
                WarningMessage = string.Empty;
        }
    }

    public string QuickAddNotes
    {
        get => _quickAddNotes;
        set => SetProperty(ref _quickAddNotes, value);
    }

    public bool ShowQuickAddNotes
    {
        get => _showQuickAddNotes;
        set => SetProperty(ref _showQuickAddNotes, value);
    }

    public string SearchText
    {
        get => _searchText;
        set
        {
            if (SetProperty(ref _searchText, value))
                ApplySearchFilter();
        }
    }

    public int SelectedSectionNumber
    {
        get => _selectedSectionNumber;
        set
        {
            if (SetProperty(ref _selectedSectionNumber, value))
                UpdateSelectedSectionDisplay();
        }
    }

    public string SelectedSectionDisplay
    {
        get => _selectedSectionDisplay;
        private set => SetProperty(ref _selectedSectionDisplay, value);
    }

    public string KeyboardHint
    {
        get => _keyboardHint;
        private set => SetProperty(ref _keyboardHint, value);
    }

    public int SectionGridColumns
    {
        get => _sectionGridColumns;
        private set => SetProperty(ref _sectionGridColumns, value);
    }

    public int MaxSectionNumber =>
        Sections.Count > 0 ? Sections.Max(s => s.SortOrder) : 1;

    public int TotalActiveCount
    {
        get => _totalActiveCount;
        set => SetProperty(ref _totalActiveCount, value);
    }

    public int TodayAddedCount
    {
        get => _todayAddedCount;
        set => SetProperty(ref _todayAddedCount, value);
    }

    public int NotedPackageCount
    {
        get => _notedPackageCount;
        set => SetProperty(ref _notedPackageCount, value);
    }

    public string BusiestSectionDisplay
    {
        get => _busiestSectionDisplay;
        set => SetProperty(ref _busiestSectionDisplay, value);
    }

    public bool ShowNoSearchResults
    {
        get => _showNoSearchResults;
        set => SetProperty(ref _showNoSearchResults, value);
    }

    public string StatusMessage
    {
        get => _statusMessage;
        set => SetProperty(ref _statusMessage, value);
    }

    public bool ShowStatusToast
    {
        get => _showStatusToast;
        set => SetProperty(ref _showStatusToast, value);
    }

    public bool ShowUndoBar
    {
        get => _showUndoBar;
        set
        {
            if (SetProperty(ref _showUndoBar, value) && value)
                ShowStatusToast = false;
        }
    }

    public string UndoMessage
    {
        get => _undoMessage;
        set => SetProperty(ref _undoMessage, value);
    }

    public string WarningMessage
    {
        get => _warningMessage;
        set => SetProperty(ref _warningMessage, value);
    }

    public PackageItemViewModel? SelectedPackage
    {
        get => _selectedPackage;
        set
        {
            if (SetProperty(ref _selectedPackage, value))
                (DeliverSelectedCommand as RelayCommand)?.RaiseCanExecuteChanged();
        }
    }

    public ICommand AddCommand { get; }
    public ICommand DeliverCommand { get; }
    public ICommand UndoCommand { get; }
    public ICommand OpenHistoryCommand { get; }
    public ICommand OpenSettingsCommand { get; }
    public ICommand BackupCommand { get; }
    public ICommand SelectSectionCommand { get; }
    public ICommand AddToSectionCommand { get; }
    public ICommand EditPackageCommand { get; }
    public ICommand ViewNotesCommand { get; }
    public ICommand ToggleQuickAddNotesCommand { get; }
    public ICommand ClearSearchCommand { get; }
    public ICommand ToggleWidgetCommand { get; }
    public ICommand DeliverSelectedCommand { get; }

    public event Action? DataRefreshed;
    public event Action? ToggleWidgetRequested;
    public event Action? RequestQuickAddFocus;
    public event Action<int>? HighlightPackageRequested;

    public void AddToSelectedSection() => AddToSection(SelectedSectionNumber);

    public void AddToSection(int sectionNumber)
    {
        try
        {
            var section = Sections.FirstOrDefault(s => s.SortOrder == sectionNumber)
                          ?? Sections.ElementAtOrDefault(sectionNumber - 1);
            if (section is null)
            {
                WarningMessage = "Geçersiz bölüm seçimi.";
                return;
            }

            if (string.IsNullOrWhiteSpace(QuickAddName))
            {
                WarningMessage = "Lütfen alıcı adı yazın.";
                return;
            }

            if (_packageService.ExistsActiveByName(QuickAddName))
                WarningMessage = $"'{QuickAddName.Trim()}' zaten listede — yine de eklenecek.";

            var package = _packageService.Add(QuickAddName, section.Id, QuickAddNotes);
            QuickAddName = string.Empty;
            QuickAddNotes = string.Empty;
            ShowQuickAddNotes = false;
            WarningMessage = string.Empty;
            RefreshAll();
            HighlightNewPackage(package.Id, section.Id);
            RequestQuickAddFocus?.Invoke();
        }
        catch (Exception ex)
        {
            ShowError("Kargo eklenemedi.", ex);
        }
    }

    public void DeliverPackage(PackageItemViewModel? item)
    {
        if (item is null) return;
        try
        {
            var delivered = _packageService.Deliver(item.Id);
            _undoService.RegisterDelivery(delivered);
            RefreshAll();
        }
        catch (Exception ex)
        {
            ShowError("Teslim işlemi başarısız.", ex);
        }
    }

    public void UndoLastDelivery()
    {
        try
        {
            _undoService.TryUndo(_packageService);
            ShowUndoBar = false;
            RefreshAll();
        }
        catch (Exception ex)
        {
            ShowError("Geri alma başarısız.", ex);
        }
    }

    public void ClearSearch()
    {
        SearchText = string.Empty;
        RequestQuickAddFocus?.Invoke();
    }

    public void CreateBackup()
    {
        try
        {
            var path = _backupService.CreateDatabaseBackup();
            ShowToast("Yedek alındı.");
            LoggingService.Instance.Info($"Yedek: {path}");
        }
        catch (Exception ex)
        {
            ShowError("Yedek alınamadı.", ex);
        }
    }

    public void ShowToast(string message, int milliseconds = 3000)
    {
        if (ShowUndoBar) return;
        StatusMessage = message;
        ShowStatusToast = true;
        _ = HideToastAfterDelay(milliseconds);
    }

    private async Task HideToastAfterDelay(int ms)
    {
        await Task.Delay(ms);
        if (!ShowUndoBar)
        {
            ShowStatusToast = false;
            StatusMessage = string.Empty;
        }
    }

    public void HighlightPackageFromWidget(int packageId)
    {
        _highlightPackageId = packageId;
        var pkg = _packageService.GetById(packageId);
        if (pkg is null) return;

        SearchText = pkg.RecipientName;
        HighlightNewPackage(packageId, pkg.SectionId);
    }

    private void ViewNotes(PackageItemViewModel? item)
    {
        if (item is null || !item.HasNotes) return;
        MessageBox.Show(item.Notes, $"{item.RecipientName} — Not", MessageBoxButton.OK, MessageBoxImage.Information);
    }

    private void EditPackage(PackageItemViewModel? item)
    {
        if (item is null) return;
        var pkg = _packageService.GetById(item.Id);
        if (pkg is null) return;

        var dialog = new EditPackageDialog(pkg, _packageService, _sectionService)
        {
            Owner = Application.Current.MainWindow
        };
        dialog.ChangesSaved += RefreshAll;
        dialog.ShowDialog();
    }

    private void OpenHistory()
    {
        var window = new HistoryWindow(_packageService)
        {
            Owner = Application.Current.MainWindow
        };
        window.ShowDialog();
        RefreshAll();
    }

    private void OpenSettings()
    {
        var window = new SettingsWindow(_sectionService, _backupService)
        {
            Owner = Application.Current.MainWindow
        };
        window.ShowDialog();
        RefreshSections();
        RefreshAll();
    }

    private void OnUndoAvailable(Package package)
    {
        UndoMessage = $"{package.RecipientName} teslim edildi.";
        ShowUndoBar = true;
    }

    private void RefreshSections()
    {
        var sections = _sectionService.GetActiveSections().OrderBy(s => s.SortOrder).ToList();

        SectionButtons.Clear();
        foreach (var s in sections)
            SectionButtons.Add(s.SortOrder);

        var existingById = Sections.ToDictionary(s => s.Id);
        var newList = new List<SectionCardViewModel>();

        foreach (var section in sections)
        {
            if (existingById.TryGetValue(section.Id, out var card))
            {
                card.Name = section.Name;
                newList.Add(card);
            }
            else
            {
                var brush = AccentBrushes[(section.SortOrder - 1) % AccentBrushes.Length];
                newList.Add(new SectionCardViewModel(section, brush));
            }
        }

        Sections.Clear();
        foreach (var s in newList)
            Sections.Add(s);

        SectionGridColumns = CalculateGridColumns(Sections.Count);
        UpdateSelectedSectionDisplay();
    }

    private static int CalculateGridColumns(int sectionCount) => sectionCount switch
    {
        <= 1 => 1,
        2 => 2,
        3 => 3,
        4 => 2,
        5 or 6 => 3,
        7 or 8 => 4,
        _ => 4
    };

    private void UpdateSelectedSectionDisplay()
    {
        var section = Sections.FirstOrDefault(s => s.SortOrder == SelectedSectionNumber)
                      ?? Sections.FirstOrDefault();
        SelectedSectionDisplay = section is not null
            ? $"Bölüm {section.SortOrder} · {section.Name}"
            : $"Bölüm {SelectedSectionNumber}";

        if (SelectedSectionNumber > MaxSectionNumber && MaxSectionNumber > 0)
            SelectedSectionNumber = MaxSectionNumber;

        UpdateKeyboardHint();
    }

    private void UpdateKeyboardHint()
    {
        var maxKey = Math.Min(MaxSectionNumber, 5);
        KeyboardHint = maxKey > 1
            ? $"Ctrl+1..{maxKey} ile hızlı ekle · Enter ile ekle · Ctrl+F ara"
            : "Enter ile ekle · Ctrl+F ara";
    }

    private void RefreshAll()
    {
        try
        {
            var packages = string.IsNullOrWhiteSpace(SearchText)
                ? _packageService.GetActivePackages()
                : _packageService.SearchActive(SearchText);

            foreach (var section in Sections)
            {
                section.Packages.Clear();
                foreach (var p in packages.Where(x => x.SectionId == section.Id))
                    section.Packages.Add(new PackageItemViewModel(p));
                section.RefreshCount();
            }

            TotalActiveCount = _packageService.GetActiveCount();
            TodayAddedCount = _packageService.GetTodayAddedCount();
            NotedPackageCount = packages.Count(p => !string.IsNullOrWhiteSpace(p.Notes));
            UpdateBusiestSection();
            ApplySearchFilter();
            DataRefreshed?.Invoke();
        }
        catch (Exception ex)
        {
            ShowError("Veriler yüklenemedi.", ex);
        }
    }

    private void UpdateBusiestSection()
    {
        if (Sections.Count == 0)
        {
            BusiestSectionDisplay = "—";
            return;
        }

        var busiest = Sections.OrderByDescending(s => s.Count).First();
        BusiestSectionDisplay = busiest.Count > 0
            ? $"Bölüm {busiest.SortOrder} ({busiest.Count})"
            : "—";
    }

    private void ApplySearchFilter()
    {
        var query = SearchText.Trim();
        var hasSearch = !string.IsNullOrEmpty(query);
        var matchCount = 0;

        foreach (var section in Sections)
        {
            section.IsSearchHighlighted = false;
            foreach (var pkg in section.Packages)
            {
                pkg.IsSearchMatch = hasSearch &&
                    (pkg.RecipientName.Contains(query, StringComparison.CurrentCultureIgnoreCase) ||
                     pkg.Notes.Contains(query, StringComparison.CurrentCultureIgnoreCase));
                if (pkg.IsSearchMatch) matchCount++;
            }

            if (hasSearch && section.Packages.Any(p => p.IsSearchMatch))
                section.IsSearchHighlighted = true;
        }

        ShowNoSearchResults = hasSearch && matchCount == 0;

        if (_highlightPackageId.HasValue)
        {
            var id = _highlightPackageId.Value;
            foreach (var section in Sections)
            {
                foreach (var pkg in section.Packages.Where(p => p.Id == id))
                {
                    pkg.IsHighlighted = true;
                    section.IsSearchHighlighted = true;
                    HighlightPackageRequested?.Invoke(id);
                }
            }
            _highlightPackageId = null;
        }
    }

    private async void HighlightNewPackage(int packageId, int sectionId)
    {
        PackageItemViewModel? target = null;
        foreach (var section in Sections)
        {
            target = section.Packages.FirstOrDefault(p => p.Id == packageId);
            if (target is not null) break;
        }

        if (target is null) return;
        target.IsHighlighted = true;
        HighlightPackageRequested?.Invoke(packageId);

        await Task.Delay(2500);
        target.IsHighlighted = false;
    }

    private static void ShowError(string message, Exception ex)
    {
        LoggingService.Instance.Error(message, ex);
        MessageBox.Show($"{message}\n\n{ex.Message}", "Hata", MessageBoxButton.OK, MessageBoxImage.Error);
    }
}
