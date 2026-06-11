using System.Windows;
using System.Windows.Controls;
using KargoRaf.ViewModels;

namespace KargoRaf.Controls;

public partial class SectionCardControl : System.Windows.Controls.UserControl
{
    public SectionCardControl()
    {
        InitializeComponent();
    }

    private void SectionQuickAdd_Click(object sender, RoutedEventArgs e)
    {
        if (sender is not Button btn || btn.Tag is not int sortOrder)
            return;

        if (Window.GetWindow(this)?.DataContext is not MainViewModel vm)
            return;

        vm.SelectedSectionNumber = sortOrder;
        if (Window.GetWindow(this) is MainWindow main)
            main.UpdateSectionSelection(sortOrder);

        if (!string.IsNullOrWhiteSpace(vm.QuickAddName))
            vm.AddToSection(sortOrder);
        else if (Window.GetWindow(this) is MainWindow mw)
            mw.FocusQuickAdd();
    }
}
