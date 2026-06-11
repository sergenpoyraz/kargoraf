using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using KargoRaf.ViewModels;

namespace KargoRaf.Controls;

public partial class PackageRowControl : System.Windows.Controls.UserControl
{
    public PackageRowControl()
    {
        InitializeComponent();
    }

    private void PackageItem_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
    {
        if (sender is not FrameworkElement fe || fe.DataContext is not PackageItemViewModel item)
            return;

        if (Window.GetWindow(this)?.DataContext is not MainViewModel vm)
            return;

        vm.SelectedPackage = item;
        foreach (var section in vm.Sections)
            foreach (var p in section.Packages)
                p.IsSelected = p.Id == item.Id;
    }

    private void PackageItem_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
    {
        if (e.ClickCount == 2 && sender is FrameworkElement fe && fe.DataContext is PackageItemViewModel item)
        {
            if (Window.GetWindow(this)?.DataContext is MainViewModel vm)
                vm.EditPackageCommand.Execute(item);
            e.Handled = true;
        }
    }
}
