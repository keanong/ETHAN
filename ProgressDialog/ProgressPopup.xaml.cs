using CommunityToolkit.Maui.Views;

namespace ETHAN.ProgressDialog;

public partial class ProgressPopup : Popup
{
    public ProgressPopup(string message)
    {
        InitializeComponent();
        MessageLabel.Text = message;
    }
}