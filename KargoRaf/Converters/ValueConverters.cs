using System.Collections.ObjectModel;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Media;

namespace KargoRaf.Converters;

public class BoolToVisibilityConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        var invert = parameter?.ToString() == "Invert";
        var visible = value is true;
        if (invert) visible = !visible;
        return visible ? System.Windows.Visibility.Visible : System.Windows.Visibility.Collapsed;
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) =>
        throw new NotSupportedException();
}

public class NullOrEmptyToVisibilityConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        var invert = parameter?.ToString() == "Invert";
        var empty = value is null or "" or 0;
        if (invert) empty = !empty;
        return empty ? System.Windows.Visibility.Collapsed : System.Windows.Visibility.Visible;
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) =>
        throw new NotSupportedException();
}

public class SectionAccentConverter : IValueConverter
{
    private static readonly System.Windows.Media.Brush[] Brushes =
    [
        (System.Windows.Media.Brush)new BrushConverter().ConvertFrom("#F97316")!,
        (System.Windows.Media.Brush)new BrushConverter().ConvertFrom("#7C3AED")!,
        (System.Windows.Media.Brush)new BrushConverter().ConvertFrom("#0891B2")!,
        (System.Windows.Media.Brush)new BrushConverter().ConvertFrom("#16A34A")!,
        (System.Windows.Media.Brush)new BrushConverter().ConvertFrom("#EA580C")!,
        (System.Windows.Media.Brush)new BrushConverter().ConvertFrom("#DB2777")!,
        (System.Windows.Media.Brush)new BrushConverter().ConvertFrom("#CA8A04")!,
    ];

    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        var index = value is int i ? Math.Max(0, i - 1) : 0;
        return Brushes[index % Brushes.Length];
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) =>
        throw new NotSupportedException();
}

public class HighlightBackgroundConverter : IMultiValueConverter
{
    public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
    {
        var highlighted = values.Length > 0 && values[0] is true;
        var searchMatch = values.Length > 1 && values[1] is true;
        var selected = values.Length > 2 && values[2] is true;

        if (highlighted) return new SolidColorBrush((Color)ColorConverter.ConvertFromString("#FEF3C7")!);
        if (selected) return new SolidColorBrush((Color)ColorConverter.ConvertFromString("#FFEDD5")!);
        if (searchMatch) return new SolidColorBrush((Color)ColorConverter.ConvertFromString("#ECFDF5")!);
        return System.Windows.Media.Brushes.Transparent;
    }

    public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture) =>
        throw new NotSupportedException();
}

public class SearchHighlightTextConverter : IMultiValueConverter
{
    public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
    {
        if (values.Length < 2 || values[0] is not string text || values[1] is not string query)
            return values.Length > 0 && values[0] is string s ? s : string.Empty;

        if (string.IsNullOrWhiteSpace(query)) return text;

        var index = text.IndexOf(query, StringComparison.CurrentCultureIgnoreCase);
        if (index < 0) return text;
        return text;
    }

    public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture) =>
        throw new NotSupportedException();
}

public class CountToStringConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture) =>
        value?.ToString() ?? "0";

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) =>
        throw new NotSupportedException();
}
