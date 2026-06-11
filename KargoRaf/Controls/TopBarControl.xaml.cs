using System.Windows.Controls;

namespace KargoRaf.Controls;

public partial class TopBarControl : System.Windows.Controls.UserControl
{
    public TopBarControl()
    {
        InitializeComponent();
    }

    public SearchBoxControl Search => SearchBox;

    public void FocusSearch() => SearchBox.FocusSearch();

    public void ClearSearch() => SearchBox.Clear();
}
