using ETHAN.classes;
using ETHAN.Network;
using ETHAN.ProgressDialog;
using XDelServiceRef;
using ZXing.QrCode.Internal;

namespace ETHAN.Views;

[QueryProperty(nameof(BARCODE), "BARCODE")] // Add a QueryProperty to handle the navigation parameter

public partial class Home_Page : ContentView
{
    private LoginInfo? logininfo;

    public string barcode;
    public string BARCODE
    {
        set
        {
            barcode = value;
            if (barcode != null && txtRefNo != null)
                txtRefNo.Text = barcode;
            else if ((barcode == null || (barcode != null && string.IsNullOrEmpty(barcode))) && txtRefNo != null)
                txtRefNo.Text = "";
        }
    }

    private XOEWSSoapClient xs = new XOEWSSoapClient(XOEWSSoapClient.EndpointConfiguration.XOEWSSoap);
    private ProgressDialogService _progressService;

    public Home_Page()
	{
        try
        {
            InitializeComponent();
        }
        catch (InvalidOperationException ex)
        {
            Console.WriteLine($"An error occurred: {ex.Message}");
        }
        catch (Exception ex)
        {
            string s = ex.Message;
        }
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
            logininfo = AppSession.logininfo;
            loadLoginInfo();
        } catch (Exception e)
        {
            string s = e.Message;
        }
    }

    private void loadLoginInfo()
    {
        try
        {
            string mode = AppSession.LoginMode;
            if (mode.Equals("s") && logininfo != null && logininfo.clientInfo != null)
            {
                lblName.Text = "Hi,\r\n" + logininfo.clientInfo.LoggedInUserName;
                lblCoyName.Text = logininfo.clientInfo.Company;
                lblAcctNoText.Text = logininfo.clientInfo.Account.ToString();

                if (logininfo.clientInfo.AccountType == TAccountType.atPrePaid && logininfo.PrePaidBalance != null)
                    lblAvailBal.Text = logininfo.PrePaidBalance == null ? "$0.00" : "$" + logininfo.PrePaidBalance.Value.ToString("F2");

                lblPrepaidAcctText.IsVisible = (logininfo.clientInfo.AccountType == TAccountType.atPrePaid);
                lblAvailBal.IsVisible = (logininfo.clientInfo.AccountType == TAccountType.atPrePaid);
                lblAvailBalText.IsVisible = (logininfo.clientInfo.AccountType == TAccountType.atPrePaid);
                btnTopUp.IsVisible = (logininfo.clientInfo.AccountType == TAccountType.atPrePaid);
                bTrack.IsVisible = false;
            }
            else if (mode.Equals("r") && logininfo != null && logininfo.clientInfo != null)
            {
                lblName.Text = "Hi,\r\n" + logininfo.clientInfo.LoggedInUserName;
                lblCoyName.Text = "";
                lblAcctNoText.Text = "";

                lblPrepaidAcctText.IsVisible = false;
                lblAvailBal.IsVisible = false;
                lblAvailBalText.IsVisible = false;

                btnCreateJ.IsVisible = false;
                btnInventory.IsVisible = false;
                btnTopUp.IsVisible = false;
                bCoy.IsVisible = false;
                bTrack.IsVisible = false;
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

            if (await AppShell.Current.DisplayAlertAsync("Are you sure?", "You will be logged out.", "Yes", "No"))
                await ForceLogoutAsync();
        }
        catch (Exception ex)
        {
            string s = ex.Message;
        }
    }

    //void CreateJob_Click(System.Object sender, System.EventArgs e)
    void CreateJob_Click(System.Object sender, TappedEventArgs e)
    {
        try
        {
            CreateJobClick();
        }
        catch (Exception ex)
        {
            string s = ex.Message;
        }
    }

    async void CreateJobClick()
    {
        try
        {
            await Shell.Current.GoToAsync("CreateJob", new Dictionary<string, object>
                    {
                        { "vmm", null }
                    });
        }
        catch (Exception ex)
        {
            string s = ex.Message;
        }
    }

    //void ManageJob_Click(System.Object sender, System.EventArgs e)
    void ManageJob_Click(System.Object sender, TappedEventArgs e)
    {
        try
        {
            ManageJobClick();
        }
        catch (Exception ex)
        {
            string s = ex.Message;
        }
    }

    async void ManageJobClick()
    {
        try
        {
            await Shell.Current.GoToAsync("/ManageJobPage", new Dictionary<string, object>
                    {
                        { "vmm", null }
                    });
        }
        catch (Exception ex)
        {
            string s = ex.Message;
        }
    }

    //void Inventory_Click(System.Object sender, System.EventArgs e)
    void Inventory_Click(System.Object sender, TappedEventArgs e)
    {
        try
        {
            InventoryClick();
        }
        catch (Exception ex)
        {
            string s = ex.Message;
        }
    }

    async void InventoryClick()
    {
        try
        {
            await Shell.Current.GoToAsync("InventoryPage", new Dictionary<string, object>
                    {
                        { "vmm", null }
                    });
        }
        catch (Exception ex)
        {
            string s = ex.Message;
        }
    }

    //void btnTopUp_Click(System.Object sender, System.EventArgs e)
    void btnTopUp_Click(System.Object sender, TappedEventArgs e)
    {
        try
        {
            TopUpClick();
        }
        catch (Exception ex)
        {
            string s = ex.Message;
        }
    }

    async void TopUpClick()
    {
        try
        {
            await Shell.Current.GoToAsync("PrepaidListPage", new Dictionary<string, object>
                    {
                        { "vmm", null }
                    });
        }
        catch (Exception ex)
        {
            string s = ex.Message;
        }
    }

    //void btnChat_Click(System.Object sender, System.EventArgs e)
    void btnChat_Click(System.Object sender, TappedEventArgs e)
    {
        try
        {
            ChatClick();
        }
        catch (Exception ex)
        {
            string s = ex.Message;
        }
    }

    async void ChatClick()
    {
        try
        {
            await Shell.Current.GoToAsync("/ChatSupportPage");
        }
        catch (Exception ex)
        {
            string s = ex.Message;
        }
    }

    void btnScan_Click(System.Object sender, System.EventArgs e)
    {
        try
        {
            ScanClick();
        }
        catch (Exception ex)
        {
            string s = ex.Message;
        }
    }

    async void ScanClick()
    {
        try
        {
            await Shell.Current.GoToAsync("/Barcode");
        }
        catch (Exception ex)
        {
            string s = ex.Message;
        }
    }

}