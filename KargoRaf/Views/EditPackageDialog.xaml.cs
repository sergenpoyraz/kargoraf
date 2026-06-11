using System.Windows;
using System.Windows.Media.Imaging;
using System.Windows.Threading;
using KargoRaf.Models;
using KargoRaf.Services;

namespace KargoRaf.Views;

public partial class EditPackageDialog : Window
{
    private readonly Package _original;
    private readonly PackageService _packageService;
    private readonly DispatcherTimer _saveTimer;
    private bool _isLoading;

    public event Action? ChangesSaved;

    public EditPackageDialog(Package package, PackageService packageService, SectionService sectionService)
    {
        InitializeComponent();
        _original = package;
        _packageService = packageService;

        try
        {
            Icon = BitmapFrame.Create(new Uri("pack://application:,,,/Assets/AppIcon.png", UriKind.Absolute));
        }
        catch
        {
            // ignore
        }

        _saveTimer = new DispatcherTimer { Interval = TimeSpan.FromMilliseconds(350) };
        _saveTimer.Tick += (_, _) =>
        {
            _saveTimer.Stop();
            SaveChanges();
        };

        _isLoading = true;
        NameBox.Text = package.RecipientName;
        NotesBox.Text = package.Notes;
        UpdateNotesCounter();

        var sections = sectionService.GetActiveSections();
        SectionBox.ItemsSource = sections;
        SectionBox.SelectedValuePath = "Id";
        SectionBox.SelectedValue = package.SectionId;
        _isLoading = false;

        NameBox.TextChanged += (_, _) => ScheduleSave();
        NotesBox.TextChanged += (_, _) =>
        {
            UpdateNotesCounter();
            ScheduleSave();
        };
        SectionBox.SelectionChanged += (_, _) => SaveChanges();

        Loaded += (_, _) => NameBox.Focus();
        KeyDown += (_, e) =>
        {
            if (e.Key == System.Windows.Input.Key.Escape)
                Close();
        };
    }

    private void UpdateNotesCounter() =>
        NotesCounter.Text = $"{NotesBox.Text.Length} / 500";

    private void ScheduleSave()
    {
        if (_isLoading) return;
        _saveTimer.Stop();
        _saveTimer.Start();
    }

    private void SaveChanges()
    {
        if (_isLoading) return;

        var name = NameBox.Text.Trim();
        if (string.IsNullOrWhiteSpace(name))
        {
            StatusText.Text = "Alıcı adı boş olamaz";
            StatusText.Foreground = (System.Windows.Media.Brush)FindResource("WarningBrush");
            return;
        }

        if (SectionBox.SelectedValue is not int sectionId)
            return;

        try
        {
            _packageService.Update(_original.Id, name, sectionId, NotesBox.Text.Trim());
            StatusText.Text = "Kaydedildi";
            StatusText.Foreground = (System.Windows.Media.Brush)FindResource("SuccessBrush");
            ChangesSaved?.Invoke();
        }
        catch (Exception ex)
        {
            StatusText.Text = "Kayıt hatası";
            StatusText.Foreground = (System.Windows.Media.Brush)FindResource("DangerBrush");
            LoggingService.Instance.Error("Kargo güncellenemedi.", ex);
        }
    }

    private void Save_Click(object sender, RoutedEventArgs e)
    {
        SaveChanges();
        if (StatusText.Text == "Kaydedildi")
            Close();
    }

    private void Cancel_Click(object sender, RoutedEventArgs e) => Close();
}
