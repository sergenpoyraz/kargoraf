using System.Windows;
using KargoRaf.Services;
using KargoRaf.ViewModels;

namespace KargoRaf.Views;

public partial class SettingsWindow : Window
{
    public SettingsWindow(SectionService sectionService, BackupService backupService)
    {
        InitializeComponent();
        DataContext = new SettingsViewModel(sectionService, backupService);
    }
}
