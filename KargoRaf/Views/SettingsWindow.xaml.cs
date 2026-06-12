using System.Diagnostics;
using System.Windows;
using KargoRaf.Services;
using KargoRaf.ViewModels;

namespace KargoRaf.Views;

public partial class SettingsWindow : Window
{
    private const string WhatsAppUrl = "https://wa.me/905060588060";

    public SettingsWindow(SectionService sectionService, BackupService backupService, PackageService packageService)
    {
        InitializeComponent();
        DataContext = new SettingsViewModel(sectionService, backupService, packageService);
    }

    private void OpenWhatsApp_Click(object sender, RoutedEventArgs e)
    {
        Process.Start(new ProcessStartInfo(WhatsAppUrl) { UseShellExecute = true });
    }
}
