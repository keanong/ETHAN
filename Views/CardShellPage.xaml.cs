using ZXing.QrCode.Internal;
using ETHAN.classes;
using Microsoft.Maui.Controls;
using System.Reflection.Metadata.Ecma335;
using XDelServiceRef;

namespace ETHAN.Views;

/*[QueryProperty(nameof(LOGININFO), "LOGININFO")] // Add a QueryProperty to handle the navigation parameter
[QueryProperty(nameof(BARCODE), "BARCODE")] // Add a QueryProperty to handle the navigation parameter*/


public partial class CardShellPage : ContentPage, IQueryAttributable
{
    private readonly Dictionary<string, ContentView> _pages = new();

    private LoginInfo? _loginInfo;

    private string? _barcode;

    private string? _defaultTab = "Home";

    private string? _LOGIN = "";

    private readonly IServiceProvider _services;

    public CardShellPage(IServiceProvider services)
	{
		InitializeComponent();
        _services = services;
        string mode = AppSession.LoginMode;

        // Add TabBar control
        //var tabBar = new Views.FakeTabBar(mode.Equals("s"), mode.Equals("s"));
        var tabBar = new Views.FakeTabBar(mode.Equals("s"));
        tabBar.TabSelected += OnTabSelected;
        TabBarHost.Content = tabBar;

        // Prepare default tab
        LoadTab(_defaultTab);
    }

    // This method is called by Shell when you navigate with a Dictionary<string, object>
    public void ApplyQueryAttributes(IDictionary<string, object> query)
    {
        try
        {
            // Ignore if another page is currently shown
            if (Shell.Current?.CurrentPage is CardShellPage || Shell.Current?.CurrentPage is Login)
            {
                //do nothing
            } else if (Shell.Current?.CurrentPage is not CardShellPage)
                return;

            // Defensive: only set if present and not null
            if (query == null) return;

            /*if (query.TryGetValue("LOGININFO", out var loginObj) && loginObj is LoginInfo li)
                _loginInfo = li;*/
            _loginInfo = AppSession.logininfo;

            if (query.TryGetValue("BARCODE", out var bcObj) && bcObj is string s)
                _barcode = s;
            else
                _barcode = null;

            if (query.TryGetValue("DEFAULTTAB", out var dt) && dt is string tabStr)
                _defaultTab = tabStr;
            else
                _defaultTab = "Home";

            if (query.TryGetValue("LOGIN", out var lg) && lg is string lgn)
                _LOGIN = lgn;
            else
                _LOGIN = null;

            

            // If page wasn't loaded yet, ensure the tab content is created with the correct data.
            // If PageHost already has content, update it if needed.
            if (PageHost?.Content == null)
            {
                LoadTab(_defaultTab);
            }
            else
            {
                // If current content is Home_Page, update its properties (if you expose them)
                if (PageHost.Content is Home_Page hp)
                {
                    //// Either expose a method/property to update logininfo after creation:
                    //hp.LOGININFO = _loginInfo;
                    hp.BARCODE = _barcode;
                    LoadTab("Home");
                }
                else if (PageHost.Content is ChatMainPage cm)
                {
                    //if (!Equals(cm.LOGININFO, _loginInfo))
                        //cm.LOGININFO = _loginInfo;
                    //// If DEFAULTTAB or LOGIN instruct to switch, honor it
                    if (!string.IsNullOrEmpty(_defaultTab) && _defaultTab.Equals("Chat", StringComparison.OrdinalIgnoreCase))
                        LoadTab("Chat");
                }
                else if (_LOGIN != null && _LOGIN.Equals("Y"))
                    LoadTab(_defaultTab);
                else
                    LoadTab(_defaultTab);

            }
        } catch (Exception e)
        {
            string s = e.Message;
        }
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();

        // If ApplyQueryAttributes already ran and PageHost is empty, LoadTab will use _loginInfo/_barcode.
        if (PageHost.Content == null)
            LoadTab(_defaultTab);
    }

    protected override void OnSizeAllocated(double width, double height)
    {
        base.OnSizeAllocated(width, height);

        if (DeviceInfo.Idiom == DeviceIdiom.Tablet)
        {
            AppCard.WidthRequest = 500;
            AppCard.HorizontalOptions = LayoutOptions.Center;
            AppCard.HeightRequest = -1; // ← let Fill handle it, never force this
        }
        else
        {
            AppCard.WidthRequest = -1; // fill phone screen
            AppCard.HorizontalOptions = LayoutOptions.Fill;
            AppCard.HeightRequest = -1;
        }
    }

    private void OnTabSelected(object sender, string tab)
    {
        LoadTab(tab);
    }

    private void LoadTab(string tab)
    {
        if (string.IsNullOrEmpty(tab)) tab = "Home";

        if (!_pages.TryGetValue(tab, out var page))
        {

            /*page = tab switch
            {
                "Home" => new Home_Page(),
                "Chat" => new ChatMainPage(),
                "Settings" => new SettingsPage(),
                _ => new Home_Page(),
            };*/
            page = tab switch
            {
                "Home" => _services.GetRequiredService<Home_Page>(),
                "Chat" => _services.GetRequiredService<ChatMainPage>(),
                "Settings" => _services.GetRequiredService<SettingsPage>(),
                _ => _services.GetRequiredService<Home_Page>(),
            };

            _pages[tab] = page;
        }

        PageHost.Content = page;

        if (page is Home_Page hp)
        {
            //hp.LOGININFO = _loginInfo;
            hp.BARCODE = _barcode;
        }
            
        if (page is ChatMainPage cm)
        {
            //if (!Equals(cm.LOGININFO, _loginInfo))
                //cm.LOGININFO = _loginInfo;
        }

        //if (page is SettingsPage sp)
            //sp.LOGININFO = _loginInfo;

        /*//Call Refresh() whenever switching to Chat
        if (page is ChatMainPage chatPage)
            chatPage.Refresh();*/

        (TabBarHost.Content as Views.FakeTabBar)?.GetType().GetMethod("SelectTab",
            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)?.Invoke(TabBarHost.Content, new object[] { tab });
    }

}