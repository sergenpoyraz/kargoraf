using System.Windows;
using System.Windows.Forms;
using KargoRaf.Views;

namespace KargoRaf.Services;

public class TrayService : IDisposable
{
    private NotifyIcon? _notifyIcon;
    private MainWindow? _mainWindow;
    private WidgetWindow? _widgetWindow;
    private bool _disposed;

    public event Action? OpenMainRequested;
    public event Action? ToggleWidgetRequested;
    public event Action? BackupRequested;
    public event Action? ExitRequested;

    public void Initialize(MainWindow mainWindow, WidgetWindow widgetWindow)
    {
        _mainWindow = mainWindow;
        _widgetWindow = widgetWindow;

        _notifyIcon = new NotifyIcon
        {
            Text = "Kargo Raf",
            Icon = LoadIcon(),
            Visible = true
        };

        _notifyIcon.DoubleClick += (_, _) => ShowMainWindow();
        RefreshMenu();
    }

    public void RefreshMenu()
    {
        if (_notifyIcon is null) return;

        var widgetText = _widgetWindow?.IsVisible == true ? "Mini Widget Kapat" : "Mini Widget Aç";
        var menu = new ContextMenuStrip();
        menu.Items.Add("Uygulamayı Aç", null, (_, _) => ShowMainWindow());
        menu.Items.Add(widgetText, null, (_, _) => ToggleWidgetRequested?.Invoke());
        menu.Items.Add("Yedek Al", null, (_, _) => BackupRequested?.Invoke());
        menu.Items.Add(new ToolStripSeparator());
        menu.Items.Add("Çıkış", null, (_, _) => ExitRequested?.Invoke());
        _notifyIcon.ContextMenuStrip = menu;
    }

    public void ShowMainWindow()
    {
        OpenMainRequested?.Invoke();
        if (_mainWindow is null) return;
        _mainWindow.Show();
        _mainWindow.WindowState = WindowState.Normal;
        _mainWindow.Activate();
    }

    public void MinimizeToTray()
    {
        _mainWindow?.Hide();
    }

    public void ShowBalloon(string title, string message)
    {
        _notifyIcon?.ShowBalloonTip(3000, title, message, ToolTipIcon.Info);
    }

    private static System.Drawing.Icon LoadIcon()
    {
        var baseDir = AppDomain.CurrentDomain.BaseDirectory;
        var icoPath = Path.Combine(baseDir, "Assets", "AppIcon.ico");
        var pngPath = Path.Combine(baseDir, "Assets", "AppIcon.png");

        try
        {
            if (File.Exists(icoPath))
            {
                using var fs = new FileStream(icoPath, FileMode.Open, FileAccess.Read);
                using var icon = new System.Drawing.Icon(fs);
                return (System.Drawing.Icon)icon.Clone();
            }
        }
        catch (Exception ex)
        {
            LoggingService.Instance.Warning($"ICO ikonu yuklenemedi, PNG denenecek: {ex.Message}");
        }

        try
        {
            if (File.Exists(pngPath))
            {
                using var bmp = new System.Drawing.Bitmap(pngPath);
                using var resized = new System.Drawing.Bitmap(bmp, new System.Drawing.Size(32, 32));
                using var icon = System.Drawing.Icon.FromHandle(resized.GetHicon());
                return (System.Drawing.Icon)icon.Clone();
            }
        }
        catch (Exception ex)
        {
            LoggingService.Instance.Warning($"Tray ikonu yuklenemedi: {ex.Message}");
        }

        return System.Drawing.SystemIcons.Application;
    }

    public void Dispose()
    {
        if (_disposed) return;
        _disposed = true;
        if (_notifyIcon is not null)
        {
            _notifyIcon.Visible = false;
            _notifyIcon.Dispose();
        }
    }
}
