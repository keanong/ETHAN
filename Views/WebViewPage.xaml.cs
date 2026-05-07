namespace ETHAN.Views;

public partial class WebViewPage : ContentPage
{
	public WebViewPage(string url)
    {
        InitializeComponent();
        cnWebView.Source = url;
    }
}