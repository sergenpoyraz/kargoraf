using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Media;
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

        TopBar.DataContext = _viewModel;
        QuickAdd.DataContext = _viewModel;

        _viewModel.RequestQuickAddFocus += FocusQuickAdd;
        _viewModel.PropertyChanged += ViewModel_PropertyChanged;
        _viewModel.DataRefreshed += () =>
            Dispatcher.BeginInvoke(() => UpdateSectionSelection(_viewModel.SelectedSectionNumber));

        Loaded += (_, _) =>
        {
            UpdateSectionSelection(_viewModel.SelectedSectionNumber);
            FocusQuickAdd();
        };
        Activated += (_, _) => FocusQuickAdd();
    }

    private void ViewModel_PropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName == nameof(MainViewModel.SelectedSectionNumber))
            UpdateSectionSelection(_viewModel.SelectedSectionNumber);
    }

    public void UpdateSectionSelection(int selected)
    {
        foreach (var btn in FindVisualChildren<ToggleButton>(QuickAdd))
        {
            if (btn.Content is int n)
                btn.IsChecked = n == selected;
        }
    }

    private static IEnumerable<T> FindVisualChildren<T>(DependencyObject parent) where T : DependencyObject
    {
        for (var i = 0; i < VisualTreeHelper.GetChildrenCount(parent); i++)
        {
            var child = VisualTreeHelper.GetChild(parent, i);
            if (child is T match)
                yield return match;

            foreach (var nested in FindVisualChildren<T>(child))
                yield return nested;
        }
    }

    private void SetWindowIcon()
    {
        try
        {
            var icoPath = System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Assets", "AppIcon.ico");
            if (System.IO.File.Exists(icoPath))
                Icon = BitmapFrame.Create(new Uri(icoPath, UriKind.Absolute));
            else
                Icon = BitmapFrame.Create(new Uri("pack://application:,,,/Assets/AppIcon.png", UriKind.Absolute));
        }
        catch
        {
            // Varsayılan ikon kalır
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

    public void FocusQuickAdd() => QuickAdd.FocusQuickAdd();

    private void MainWindow_PreviewKeyDown(object sender, KeyEventArgs e)
    {
        if (e.Key == Key.Escape)
        {
            _viewModel.ClearSearch();
            TopBar.ClearSearch();
            FocusQuickAdd();
            e.Handled = true;
            return;
        }

        if (e.Key == Key.F && Keyboard.Modifiers == ModifierKeys.Control)
        {
            TopBar.FocusSearch();
            e.Handled = true;
            return;
        }

        if (Keyboard.Modifiers == ModifierKeys.Control)
        {
            var section = ResolveSectionKey(e.Key);
            if (section > 0 && !string.IsNullOrWhiteSpace(_viewModel.QuickAddName))
            {
                _viewModel.AddToSection(section);
                FocusQuickAdd();
                e.Handled = true;
            }
        }
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
}
