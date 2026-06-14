using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using KargoRaf.ViewModels;

namespace KargoRaf.Controls;

public partial class QuickAddPanel : System.Windows.Controls.UserControl
{
    public QuickAddPanel()
    {
        InitializeComponent();
    }

    public System.Windows.Controls.TextBox QuickAddInput => QuickAddBox;

    public void FocusQuickAdd()
    {
        QuickAddBox.Focus();
        QuickAddBox.CaretIndex = QuickAddBox.Text.Length;
    }

    private void QuickAddBox_PreviewKeyDown(object sender, KeyEventArgs e)
    {
        if (e.Key == Key.Enter && DataContext is MainViewModel vm)
        {
            vm.AddToSelectedSection();
            e.Handled = true;
        }
    }

    private void SectionButton_Click(object sender, RoutedEventArgs e)
    {
        if (sender is not ToggleButton btn)
            return;

        var number = btn.Tag is int tagNumber ? tagNumber
            : btn.DataContext is SectionCardViewModel section ? section.SortOrder
            : 0;

        if (number <= 0)
            return;

        if (DataContext is MainViewModel vm)
            vm.SelectedSectionNumber = number;

        if (Window.GetWindow(this) is MainWindow main)
            main.UpdateSectionSelection(number);
    }
}
