using System.Windows;
using KargoRaf.Services;
using KargoRaf.ViewModels;

namespace KargoRaf.Views;

public partial class HistoryWindow : Window
{
    public HistoryWindow(PackageService packageService)
    {
        InitializeComponent();
        DataContext = new HistoryViewModel(packageService);
    }
}
