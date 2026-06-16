using System.Windows;
using System.Windows.Threading;
using KargoRaf.Helpers;
using KargoRaf.Services;
using KargoRaf.ViewModels;
using KargoRaf.Views;

namespace KargoRaf;

public partial class App : System.Windows.Application
{
    private DatabaseService? _databaseService;
    private PackageService? _packageService;
    private SectionService? _sectionService;
    private SettingsService? _settingsService;
    private BackupService? _backupService;
    private UndoService? _undoService;
    private TrayService? _trayService;
    private WidgetWindow? _widgetWindow;
    private MainWindow? _mainWindow;
    private bool _isExiting;
    private bool _isShowingErrorDialog;

    protected override void OnStartup(StartupEventArgs e)
    {
        base.OnStartup(e);

        DispatcherUnhandledException += OnDispatcherUnhandledException;
        AppDomain.CurrentDomain.UnhandledException += OnUnhandledException;

        try
        {
            AppPaths.EnsureDirectories();
            LoggingService.Instance.Info("Kargo Raf başlatılıyor...");

            _databaseService = new DatabaseService();
            _databaseService.Initialize();

            if (!_databaseService.TestConnection())
            {
                MessageBox.Show(
                    "Veritabanına bağlanılamadı. Lütfen uygulamayı yeniden başlatın veya destek alın.",
                    "Veritabanı Hatası",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
            }

            _packageService = new PackageService();
            _sectionService = new SectionService();
            _settingsService = new SettingsService();
            _undoService = new UndoService();
            _backupService = new BackupService(_packageService);

            _mainWindow = new MainWindow(
                _packageService,
                _sectionService,
                _undoService,
                _backupService,
                _settingsService);

            _widgetWindow = new WidgetWindow(_packageService, _sectionService);
            _widgetWindow.Hide();
            _widgetWindow.PackageOpenRequested += id =>
            {
                _mainWindow.ShowFromTray();
                _mainWindow.HighlightPackage(id);
            };

            _trayService = new TrayService();
            _trayService.Initialize(_mainWindow, _widgetWindow);
            _trayService.OpenMainRequested += () => _mainWindow.ShowFromTray();
            _trayService.ToggleWidgetRequested += ToggleWidget;
            _trayService.BackupRequested += () => (_mainWindow.DataContext as MainViewModel)?.CreateBackup();
            _trayService.ExitRequested += ExitApplication;

            if (_mainWindow.DataContext is MainViewModel mainVm)
                mainVm.ToggleWidgetRequested += ToggleWidget;
            _mainWindow.Closing += MainWindow_Closing;

            MainWindow = _mainWindow;
            _mainWindow.Show();
        }
        catch (Exception ex)
        {
            LoggingService.Instance.Error("Uygulama başlatılamadı.", ex);
            MessageBox.Show($"Uygulama başlatılamadı:\n{ex.Message}", "Kritik Hata", MessageBoxButton.OK, MessageBoxImage.Error);
            Shutdown(1);
        }
    }

    private void MainWindow_Closing(object? sender, System.ComponentModel.CancelEventArgs e)
    {
        if (_isExiting) return;
        e.Cancel = true;
        _trayService?.MinimizeToTray();
    }

    private void ToggleWidget()
    {
        if (_widgetWindow is null) return;

        if (_widgetWindow.IsVisible)
        {
            _widgetWindow.Hide();
        }
        else
        {
            _widgetWindow.Show();
            _widgetWindow.Activate();
        }

        _trayService?.RefreshMenu();
    }

    private void ExitApplication()
    {
        _isExiting = true;
        LoggingService.Instance.Info("Uygulama kapatılıyor.");
        _trayService?.Dispose();
        _widgetWindow?.Close();
        _mainWindow?.ForceClose();
        Shutdown();
    }

    private void OnDispatcherUnhandledException(object sender, DispatcherUnhandledExceptionEventArgs e)
    {
        LoggingService.Instance.Error("Beklenmeyen hata.", e.Exception);
        if (!_isShowingErrorDialog)
        {
            _isShowingErrorDialog = true;
            try
            {
                var detail = e.Exception.InnerException?.Message ?? e.Exception.Message;
                MessageBox.Show(
                    $"Beklenmeyen bir hata oluştu:\n{detail}",
                    "Hata",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
            }
            finally
            {
                _isShowingErrorDialog = false;
            }
        }
        e.Handled = true;
    }

    private void OnUnhandledException(object sender, UnhandledExceptionEventArgs e)
    {
        if (e.ExceptionObject is Exception ex)
            LoggingService.Instance.Error("Kritik hata.", ex);
    }
}
