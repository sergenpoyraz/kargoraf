using System.Windows;
using System.Windows.Media.Imaging;
using KargoRaf.Models;
using KargoRaf.Services;

namespace KargoRaf.Views;

public partial class EditPackageDialog : Window
{
    private readonly Package _original;
    private readonly SectionService _sectionService;

    public Package? ResultPackage { get; private set; }

    public EditPackageDialog(Package package, List<int> sectionIds, SectionService sectionService)
    {
        InitializeComponent();
        _original = package;
        _sectionService = sectionService;

        Icon = BitmapFrame.Create(new Uri("pack://application:,,,/Assets/AppIcon.png", UriKind.Absolute));

        NameBox.Text = package.RecipientName;
        NotesBox.Text = package.Notes;

        var sections = _sectionService.GetActiveSections();
        SectionBox.ItemsSource = sections;
        SectionBox.SelectedValuePath = "Id";
        SectionBox.SelectedValue = package.SectionId;

        Loaded += (_, _) => NameBox.Focus();
        KeyDown += (_, e) =>
        {
            if (e.Key == System.Windows.Input.Key.Escape)
            {
                DialogResult = false;
                Close();
            }
        };
    }

    private void Save_Click(object sender, RoutedEventArgs e)
    {
        var name = NameBox.Text.Trim();
        if (string.IsNullOrWhiteSpace(name))
        {
            MessageBox.Show("Alıcı adı boş olamaz.", "Uyarı", MessageBoxButton.OK, MessageBoxImage.Warning);
            return;
        }

        if (SectionBox.SelectedValue is not int sectionId)
        {
            MessageBox.Show("Bölüm seçin.", "Uyarı", MessageBoxButton.OK, MessageBoxImage.Warning);
            return;
        }

        ResultPackage = new Package
        {
            Id = _original.Id,
            RecipientName = name,
            SectionId = sectionId,
            Notes = NotesBox.Text.Trim(),
            CreatedAt = _original.CreatedAt
        };

        DialogResult = true;
        Close();
    }

    private void Cancel_Click(object sender, RoutedEventArgs e)
    {
        DialogResult = false;
        Close();
    }
}
