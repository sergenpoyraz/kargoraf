using System.Windows;
using System.Windows.Media.Imaging;

namespace KargoRaf.Views;

public partial class NoteDialog : Window
{
    public NoteDialog(string recipientName, string note)
    {
        InitializeComponent();
        Title = $"{recipientName} — Not";
        TitleText.Text = recipientName;
        NoteText.Text = note;

        try
        {
            Icon = BitmapFrame.Create(new Uri("pack://application:,,,/Assets/AppIcon.png", UriKind.Absolute));
        }
        catch
        {
            // Varsayılan ikon kalır.
        }
    }

    private void Close_Click(object sender, RoutedEventArgs e) => Close();
}
