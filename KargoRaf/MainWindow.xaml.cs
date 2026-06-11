using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media.Imaging;
using KargoRaf.Services;
using KargoRaf.ViewModels;

namespace KargoRaf;

public partial class MainWindow : Window
{
    private readonly MainViewModel _viewModel;

    public MainWindow(
        PackageService packageService,
        SectionService sectionService,
        UndoService undoService,
        BackupService backupService)
    {
        InitializeComponent();
        SetWindowIcon();

        _viewModel = new MainViewModel(packageService, sectionService, undoService, backupService);
        DataContext = _viewModel;

        _viewModel.RequestQuickAddFocus += FocusQuickAdd;

        Loaded += (_, _) => FocusQuickAdd();
        Activated += (_, _) => FocusQuickAdd();
    }

    private void SetWindowIcon()
    {
        try
        {
            Icon = BitmapFrame.Create(new Uri("pack://application:,,,/Assets/AppIcon.png", UriKind.Absolute));
        }
        catch
        {
            // PNG yoksa varsayılan ikon kalır
        }
    }

    public void ShowFromTray()
    {
        Show();
        WindowState = WindowState.Normal;
        Activate();
        FocusQuickAdd();
    }

    public void HighlightPackage(int packageId) => _viewModel.HighlightPackageFromWidget(packageId);

    public void ForceClose() => Close();

    private void FocusQuickAdd()
    {
        QuickAddBox.Focus();
        QuickAddBox.CaretIndex = QuickAddBox.Text.Length;
    }

    private void MainWindow_PreviewKeyDown(object sender, KeyEventArgs e)
    {
        if (e.Key == Key.Escape)
        {
            _viewModel.ClearSearch();
            SearchBox.Clear();
            FocusQuickAdd();
            e.Handled = true;
            return;
        }

        if (e.Key == Key.F && Keyboard.Modifiers == ModifierKeys.Control)
        {
            SearchBox.Focus();
            SearchBox.SelectAll();
            e.Handled = true;
            return;
        }

        // Ctrl+1..5 — odak nerede olursa olsun direkt ekle
        if (Keyboard.Modifiers == ModifierKeys.Control)
        {
            var section = ResolveSectionKey(e.Key);
            if (section > 0)
            {
                _viewModel.AddToSection(section);
                FocusQuickAdd();
                e.Handled = true;
            }
        }
    }

    private void QuickAddBox_PreviewKeyDown(object sender, KeyEventArgs e)
    {
        if (e.Key == Key.Enter)
        {
            _viewModel.AddToSelectedSection();
            e.Handled = true;
            return;
        }

        if (Keyboard.Modifiers == ModifierKeys.Control)
            return; // Ctrl kombinasyonları pencere seviyesinde

        var section = ResolveSectionKey(e.Key);
        if (section <= 0) return;

        // İsim yazılıysa 1-5 tuşu direkt o bölüme ekler
        if (!string.IsNullOrWhiteSpace(_viewModel.QuickAddName))
        {
            _viewModel.AddToSection(section);
            e.Handled = true;
            return;
        }

        // İsim yoksa sadece bölüm seç (Enter ile eklenecek)
        _viewModel.SelectedSectionNumber = section;
        e.Handled = true;
    }

    private int ResolveSectionKey(Key key)
    {
        var num = key switch
        {
            Key.D1 or Key.NumPad1 => 1,
            Key.D2 or Key.NumPad2 => 2,
            Key.D3 or Key.NumPad3 => 3,
            Key.D4 or Key.NumPad4 => 4,
            Key.D5 or Key.NumPad5 => 5,
            Key.D6 or Key.NumPad6 => 6,
            Key.D7 or Key.NumPad7 => 7,
            Key.D8 or Key.NumPad8 => 8,
            Key.D9 or Key.NumPad9 => 9,
            _ => 0
        };

        if (num <= 0 || num > _viewModel.MaxSectionNumber)
            return 0;

        return num;
    }

    private void SectionQuickAdd_Click(object sender, RoutedEventArgs e)
    {
        if (sender is not Button btn || btn.Tag is not int sortOrder) return;

        _viewModel.SelectedSectionNumber = sortOrder;
        if (!string.IsNullOrWhiteSpace(_viewModel.QuickAddName))
            _viewModel.AddToSection(sortOrder);
        else
            FocusQuickAdd();
    }

    private void PackageItem_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
    {
        if (sender is not FrameworkElement fe || fe.DataContext is not PackageItemViewModel item) return;

        _viewModel.SelectedPackage = item;
        foreach (var section in _viewModel.Sections)
            foreach (var p in section.Packages)
                p.IsSelected = p.Id == item.Id;
    }

    private void PackageItem_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
    {
        if (e.ClickCount == 2 && sender is FrameworkElement fe && fe.DataContext is PackageItemViewModel item)
        {
            _viewModel.EditPackageCommand.Execute(item);
            e.Handled = true;
            FocusQuickAdd();
        }
    }
}
