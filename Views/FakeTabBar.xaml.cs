namespace ETHAN.Views;

public partial class FakeTabBar : ContentView
{
    public event EventHandler<string> TabSelected;

    public Color HomeColor { get; set; } = Colors.Orange;
    public Color ChatColor { get; set; } = Colors.Gray;
    public Color SettingsColor { get; set; } = Colors.Gray;

    private string _homeIcon;
    public string HomeIcon
    {
        get => _homeIcon;
        set
        {
            _homeIcon = value;
            OnPropertyChanged();
        }
    }

    //public string ChatIcon { get; set; } = "chat_24";
    public string ChatIcon { get; set; } = "chat_50";
    //public string SettingsIcon { get; set; } = "setting_24";
    public string SettingsIcon { get; set; } = "gears_50";

    /*public FakeTabBar(bool showChat, bool showSets)
	{
		InitializeComponent();
        BindingContext = this;
        vChat.IsVisible = showChat;
        vSets.IsVisible = showSets;
    }*/

    public FakeTabBar(bool showSets)
    {
        InitializeComponent();
        BindingContext = this;
        vSets.IsVisible = showSets;
    }

    //private void ResetIcons()
    //{
    //    HomeIcon = "house_24";
    //    ChatIcon = "chat_24";
    //    SettingsIcon = "setting_24";
    //}
    private void ResetIcons()
    {
        HomeIcon = "home_50.png";
        ChatIcon = "chat_50.png";
        SettingsIcon = "gears_50.png";
    }

    private void ResetColors()
    {
        HomeColor = ChatColor = SettingsColor = Colors.Gray;
    }

    private void SelectTab(string tab)
    {
        ResetColors();
        ResetIcons();

        switch (tab)
        {
            //case "Home": HomeColor = Colors.Orange; HomeIcon = "house_24_orange"; break;
            //case "Chat": ChatColor = Colors.Orange; ChatIcon = "chat_24_orange"; break;
            //case "Settings": SettingsColor = Colors.Orange; SettingsIcon = "setting_24_orange"; break;
            case "Home": HomeColor = Colors.Orange; HomeIcon = "home_orange_50"; break;
            case "Chat": ChatColor = Colors.Orange; ChatIcon = "chat_orange_50"; break;
            case "Settings": SettingsColor = Colors.Orange; SettingsIcon = "gears_orange_50"; break;
        }
        OnPropertyChanged(nameof(HomeColor));
        OnPropertyChanged(nameof(ChatColor));
        OnPropertyChanged(nameof(SettingsColor));

        OnPropertyChanged(nameof(HomeIcon));
        OnPropertyChanged(nameof(ChatIcon));
        OnPropertyChanged(nameof(SettingsIcon));
    }

    private void Home_Clicked(object sender, EventArgs e) => TabSelected?.Invoke(this, "Home");
    private void Chat_Clicked(object sender, EventArgs e) => TabSelected?.Invoke(this, "Chat");
    private void Settings_Clicked(object sender, EventArgs e) => TabSelected?.Invoke(this, "Settings");

}