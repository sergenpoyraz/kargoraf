using System.Windows;
using System.Windows.Input;
using KargoRaf.Services;
using KargoRaf.ViewModels;

namespace KargoRaf.Views;

public partial class WidgetWindow : Window
{
    public WidgetWindow(PackageService packageService, SectionService sectionService)
    {
        InitializeComponent();

        var viewModel = new WidgetViewModel(packageService, sectionService);
        DataContext = viewModel;

        viewModel.PackageOpenRequested += id => PackageOpenRequested?.Invoke(id);

        Loaded += (_, _) =>
        {
            Left = SystemParameters.WorkArea.Right - Width - 16;
            Top = SystemParameters.WorkArea.Bottom - Height - 16;
        };
    }

    public event Action<int>? PackageOpenRequested;

    private void Border_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
    {
        if (e.ButtonState == MouseButtonState.Pressed)
            DragMove();
    }

    private void CloseButton_Click(object sender, RoutedEventArgs e) => Hide();
}
