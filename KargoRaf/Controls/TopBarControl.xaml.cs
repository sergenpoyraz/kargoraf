using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Media;

namespace KargoRaf.Controls;

public partial class TopBarControl : System.Windows.Controls.UserControl
{
    public TopBarControl()
    {
        InitializeComponent();
        Loaded += (_, _) => UpdateMaximizeIcon();
    }

    public SearchBoxControl Search => SearchBox;

    public void FocusSearch() => SearchBox.FocusSearch();

    public void ClearSearch() => SearchBox.Clear();

    public void UpdateMaximizeIcon()
    {
        if (MaximizeButton is null)
            return;

        var window = Window.GetWindow(this);
        MaximizeButton.Content = window?.WindowState == WindowState.Maximized ? "❐" : "□";
        MaximizeButton.ToolTip = window?.WindowState == WindowState.Maximized
            ? "Pencereyi geri al"
            : "Tam ekran";
    }

    private void BrandPanel_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
    {
        if (e.ClickCount == 2)
        {
            ToggleMaximize();
            e.Handled = true;
            return;
        }

        BeginWindowDrag(e);
    }

    private void DragRegion_MouseLeftButtonDown(object sender, MouseButtonEventArgs e) =>
        BeginWindowDrag(e);

    private void ToolbarGrid_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
    {
        if (IsInteractiveElement(e.OriginalSource) || IsInsideSearchBox(e.OriginalSource))
            return;

        BeginWindowDrag(e);
    }

    private void BeginWindowDrag(MouseButtonEventArgs e)
    {
        if (e.ButtonState != MouseButtonState.Pressed)
            return;

        var window = Window.GetWindow(this);
        if (window is null)
            return;

        try
        {
            window.DragMove();
        }
        catch (InvalidOperationException)
        {
            // Fare düğmesi bırakıldıysa sessizce geç
        }
    }

    private void MinimizeWindow_Click(object sender, RoutedEventArgs e)
    {
        if (Window.GetWindow(this) is Window window)
            window.WindowState = WindowState.Minimized;
    }

    private void MaximizeWindow_Click(object sender, RoutedEventArgs e) => ToggleMaximize();

    private void ToggleMaximize()
    {
        if (Window.GetWindow(this) is not Window window)
            return;

        window.WindowState = window.WindowState == WindowState.Maximized
            ? WindowState.Normal
            : WindowState.Maximized;
        UpdateMaximizeIcon();
    }

    private void CloseWindow_Click(object sender, RoutedEventArgs e) =>
        Window.GetWindow(this)?.Close();

    private bool IsInsideSearchBox(object? source)
    {
        for (var current = source as DependencyObject; current != null; current = VisualTreeHelper.GetParent(current))
        {
            if (ReferenceEquals(current, SearchBox))
                return true;
        }

        return false;
    }

    private static bool IsInteractiveElement(object? source)
    {
        for (var current = source as DependencyObject; current != null; current = VisualTreeHelper.GetParent(current))
        {
            if (current is System.Windows.Controls.Button
                or System.Windows.Controls.TextBox
                or System.Windows.Controls.ComboBox
                or ToggleButton
                or System.Windows.Controls.Primitives.ScrollBar)
                return true;
        }

        return false;
    }
}
