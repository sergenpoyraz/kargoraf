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

    public static readonly DependencyProperty ShellBackgroundProperty =
        DependencyProperty.Register(nameof(ShellBackground), typeof(System.Windows.Media.Brush), typeof(SearchBoxControl),
            new PropertyMetadata(System.Windows.Media.Brushes.White));

    public static readonly DependencyProperty ShellBorderBrushProperty =
        DependencyProperty.Register(nameof(ShellBorderBrush), typeof(System.Windows.Media.Brush), typeof(SearchBoxControl),
            new PropertyMetadata(null));

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

    public System.Windows.Media.Brush ShellBackground
    {
        get => (System.Windows.Media.Brush)GetValue(ShellBackgroundProperty);
        set => SetValue(ShellBackgroundProperty, value);
    }

    public System.Windows.Media.Brush ShellBorderBrush
    {
        get => (System.Windows.Media.Brush)GetValue(ShellBorderBrushProperty);
        set => SetValue(ShellBorderBrushProperty, value);
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
