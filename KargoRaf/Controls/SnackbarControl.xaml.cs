using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Animation;
using KargoRaf.ViewModels;

namespace KargoRaf.Controls;

public partial class SnackbarControl : System.Windows.Controls.UserControl
{
    public SnackbarControl()
    {
        InitializeComponent();
        DataContextChanged += (_, _) => HookViewModel();
    }

    private void HookViewModel()
    {
        if (DataContext is MainViewModel vm)
        {
            vm.PropertyChanged -= ViewModel_PropertyChanged;
            vm.PropertyChanged += ViewModel_PropertyChanged;
        }
    }

    private void ViewModel_PropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName == nameof(MainViewModel.ShowStatusToast) && DataContext is MainViewModel vm && vm.ShowStatusToast)
            AnimateIn(StatusTransform);

        if (e.PropertyName == nameof(MainViewModel.ShowUndoBar) && DataContext is MainViewModel vm2 && vm2.ShowUndoBar)
            AnimateIn(UndoTransform);
    }

    private static void AnimateIn(System.Windows.Media.TranslateTransform transform)
    {
        var slide = new DoubleAnimation(20, 0, TimeSpan.FromMilliseconds(280))
        {
            EasingFunction = new QuadraticEase { EasingMode = EasingMode.EaseOut }
        };
        transform.BeginAnimation(System.Windows.Media.TranslateTransform.YProperty, slide);
    }
}
