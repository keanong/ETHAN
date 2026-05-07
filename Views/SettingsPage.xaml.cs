using ETHAN.classes;
using ETHAN.Network;
using XDelServiceRef;

namespace ETHAN.Views;

//[QueryProperty(nameof(LOGININFO), "LOGININFO")] // Add a QueryProperty to handle the navigation parameter

public partial class SettingsPage : ContentView
{
    private LoginInfo? logininfo;
    /*public LoginInfo? LOGININFO
    {
        get => logininfo;
        set
        {
            logininfo = value;
            if (logininfo != null)
            {
                loadLoginInfo();
            }
        }
    }*/

    //private XWSSoapClient xs = new XWSSoapClient(XWSSoapClient.EndpointConfiguration.XWSSoap);
    private XOEWSSoapClient xs = new XOEWSSoapClient(XOEWSSoapClient.EndpointConfiguration.XOEWSSoap);

    public SettingsPage()
	{
		InitializeComponent();
        Shell.SetTabBarIsVisible(this, false);
    }

    protected override void OnParentSet()
    {
        base.OnParentSet();

        if (Parent != null)
            Reload();
    }

    void Reload()
    {
        try
        {
            string mode = AppSession.LoginMode;
            logininfo = AppSession.logininfo;
            loadLoginInfo();
        }
        catch (Exception e)
        {
            string s = e.Message;
        }
    }

    private void loadLoginInfo()
    {
        try
        {
            if (logininfo != null && logininfo.clientInfo != null)
            {
                lblName.Text = logininfo.clientInfo.LoggedInUserName;
                lblCoyName.Text = logininfo.clientInfo.Company;
                lblAcctNoText.Text = logininfo.clientInfo.Account.ToString();
            } else
            {
                vsCoy.IsVisible = false;
            }

        }
        catch (InvalidOperationException ex)
        {
            Console.WriteLine($"An error occurred: {ex.Message}");
        }
        catch (Exception e)
        {
            string s = e.Message;
        }
    }

    void Logout_Click(System.Object sender, System.EventArgs e)
    {
        try
        {
            LogoutClick();
        }
        catch (Exception ex)
        {
            string s = ex.Message;
        }
    }

    bool _isLoggingOut;
    async Task ForceLogoutAsync()
    {
        if (_isLoggingOut) return;
        _isLoggingOut = true;

        try
        {
            try
            {
                string mode = AppSession.LoginMode;
                logininfo = AppSession.logininfo;

                if (mode == "r" && logininfo?.clientInfo?.Web_UID is string ruid && !string.IsNullOrEmpty(ruid))
                    await xs.XOE_LogOutAsync(ruid);
                else if (mode == "s" && logininfo?.clientInfo?.Web_UID is string suid && !string.IsNullOrEmpty(suid))
                    await xs.LogOutAsync(suid);
            }
            catch { } // never block logout on server failure

            await AppSession.ClearAsync();
            SecureStorage.RemoveAll();

            MainThread.BeginInvokeOnMainThread(() =>
            {
                Application.Current.MainPage = new AppShell(); // destroy all cached pages
            });

            //await Shell.Current.GoToAsync("///Login");
        }
        catch (Exception e)
        {
            string s = e.Message;
        }
        finally
        {
            _isLoggingOut = false;
        }
    }

    async void LogoutClick()
    {
        try
        {
            if (NetworkHelper.IsDisconnected())
            {
                await AppShell.Current.DisplayAlertAsync("No Internet Connection", "Please check your internet connection and try again.", "OK");
                return;
            }

            /*if (await AppShell.Current.DisplayAlert("Are you sure?", "You will be logged out.", "Yes", "No"))
            {
                SecureStorage.RemoveAll();
                await Shell.Current.GoToAsync("///Login");
            }*/
            if (await AppShell.Current.DisplayAlertAsync("Are you sure?", "You will be logged out.", "Yes", "No"))
                await ForceLogoutAsync();
        }
        catch (Exception ex)
        {
            string s = ex.Message;
        }
    }

    async void ChangePwdClick(object sender, TappedEventArgs e)
    {
        try
        {
            /*await Shell.Current.GoToAsync("ChangePwdPage", new Dictionary<string, object>
                    {
                        { "vmm", null },
                        {"LOGININFO",  logininfo}
                    });*/
            await Shell.Current.GoToAsync("ChangePwdPage", new Dictionary<string, object>
                    {
                        { "vmm", null }
                    });
        } catch (Exception ex)
        {
            string s = ex.Message;
        }
    }

}