using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace KargoRaf.Controls;

public partial class SearchBoxControl : System.Windows.Controls.UserControl
{
    public static readonly DependencyProperty SearchTextProperty =
        DependencyProperty.Register(nameof(SearchText), typeof(string), typeof(SearchBoxControl),
            new FrameworkPropertyMetadata(string.Empty, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault));

    public static readonly DependencyProperty ClearSearchCommandProperty =
        DependencyProperty.Register(nameof(ClearSearchCommand), typeof(ICommand), typeof(SearchBoxControl));

    public SearchBoxControl()
    {
        InitializeComponent();
    }

    public string SearchText
    {
        get => (string)GetValue(SearchTextProperty);
        set => SetValue(SearchTextProperty, value);
    }

    public ICommand? ClearSearchCommand
    {
        get => (ICommand?)GetValue(ClearSearchCommandProperty);
        set => SetValue(ClearSearchCommandProperty, value);
    }

    public void FocusSearch()
    {
        SearchInput.Focus();
        SearchInput.SelectAll();
    }

    public void Clear()
    {
        SearchText = string.Empty;
    }
}
