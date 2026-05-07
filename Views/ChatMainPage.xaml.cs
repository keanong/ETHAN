using ETHAN.classes;
using ETHAN.Network;
using ETHAN.ProgressDialog;
using ETHAN.ViewModel;
using System.Collections.ObjectModel;
using XDelServiceRef;

namespace ETHAN.Views;

//[QueryProperty(nameof(LOGININFO), "LOGININFO")] // Add a QueryProperty to handle the navigation parameter

public partial class ChatMainPage : ContentView
{
    private XOEWSSoapClient xs = new XOEWSSoapClient(XOEWSSoapClient.EndpointConfiguration.XOEWSSoap);
    private readonly IProgressDialogService _progressService;
    private SelectableExpListChatsVM _ExpviewModel;
    string errmsg_relogin = "Session expired. Please Login again.";

    private LoginInfo? logininfo;
    /*public LoginInfo? LOGININFO
    {
        get => logininfo;
        set
        {
            logininfo = value;
            if (logininfo != null)
                _ = getChatHistory(); //_ = fire n forget
        }
    }*/

    public void Refresh()
    {
        if (logininfo != null)
            _ = getChatHistory(); // fire & forget
    }

    public ChatMainPage(IProgressDialogService progressService)
    {
        InitializeComponent();
        _progressService = progressService;
        Shell.SetTabBarIsVisible(this, false);
        try
        {
            _ExpviewModel = new SelectableExpListChatsVM();
            cvChats.BindingContext = _ExpviewModel;
        }
        catch (Exception e)
        {
            string s = e.Message;
        }
    }

    protected override void OnParentSet()
    {
        base.OnParentSet();

        if (Parent != null)
            Reload();
        else
        {
            // Parent is null = view is being removed
            if (_ExpviewModel != null)
                _ExpviewModel.ChildTappedEvent -= OnChildTapped;
        }
    }

    void Reload()
    {
        try
        {
            string mode = AppSession.LoginMode;
            logininfo = AppSession.logininfo;
            if (mode.Equals("r"))
                btnNewChat.IsVisible = false;            

            Refresh();
        }
        catch (Exception e)
        {
            string s = e.Message;
        }
    }

    private bool canProceed()
    {
        try
        {
            logininfo = AppSession.logininfo;
            if (logininfo == null)
                return false;
            if (logininfo.clientInfo == null)
                return false;
            if (string.IsNullOrEmpty(logininfo.clientInfo.Web_UID))
                return false;
            return true;
        }
        catch (Exception e)
        {
            string s = e.Message;
            return false;
        }
    }

    private async Task getChatHistory()
    {
        XDelServiceRef.ChatSession[]? list = null;
        ChatSessions css = null;
        try
        {
            if (NetworkHelper.IsDisconnected())
            {
                await AppShell.Current.DisplayAlertAsync("No Internet Connection", "Please check your internet connection and try again.", "OK");
                return;
            }

            if (canProceed())
            {
                string mode = AppSession.LoginMode;
                logininfo = AppSession.logininfo;
                ClientInfo ci = logininfo.clientInfo;
                await showProgress_Dialog("Processing...");

                //if (list == null)
                //{
                //    if (mode.Equals("r"))
                //        css = await xs.XOE_VCS_Get_ChatsAsync(ci.Web_UID, 0);
                //    else
                //        css = await xs.VCS_Get_ChatsAsync(ci.Web_UID, 0);
                //    if (css != null && css.Status == 0)
                //        list = css.Sessions;
                //}

                //// Unsubscribe old handler before replacing
                //if (_ExpviewModel != null)
                //    _ExpviewModel.ChildTappedEvent -= OnChildTapped;

                //_ExpviewModel = new SelectableExpListChatsVM(list);
                //_ExpviewModel.ChildTappedEvent += OnChildTapped; // named method, not lambda

                //cvChats.BindingContext = _ExpviewModel;
                //await closeProgress_dialog();

                if (mode.Equals("r"))
                    css = await xs.XOE_VCS_Get_ChatsAsync(ci.Web_UID, 0);
                else
                    css = await xs.VCS_Get_ChatsAsync(ci.Web_UID, 0);

                if (css != null && css.Status == 0)
                    list = css.Sessions;

                // Always unsubscribe first to avoid duplicates
                _ExpviewModel.ChildTappedEvent -= OnChildTapped;
                // Always subscribe
                _ExpviewModel.ChildTappedEvent += OnChildTapped;
                // Just update data
                _ExpviewModel.UpdateItems(list);

                // Just update the data inside existing VM
                _ExpviewModel.UpdateItems(list);

                await closeProgress_dialog();
            }
            else
            {
                await closeProgress_dialog();
                await AppShell.Current.DisplayAlertAsync("", errmsg_relogin, "OK");
                await common.BackToLogin();
            }
        }
        catch (Exception e)
        {
            string s = e.Message;
        }
    }

    // Named method instead of lambda — can be unsubscribed
    private async void OnChildTapped(object s, ChatChild child)
    {
        await SetChatClick(child);
    }

    async Task SetChatClick(ChatChild cc)
    {
        string ccref = "";

        try
        {
            if (NetworkHelper.IsDisconnected())
            {
                await AppShell.Current.DisplayAlertAsync("No Internet Connection", "Please check your internet connection and try again.", "OK");
                return;
            }

            if (cc == null)
                return;
            if (cc.Chatsession == null)
                return;
            if (cc.Chatsession.SessionIDX == 0)
                return;

            ccref = !string.IsNullOrEmpty(cc.Reference) ? cc.Reference : "";
            if (!string.IsNullOrEmpty(ccref) && ccref.Substring(ccref.Length - 1).Equals("/"))
                ccref = ccref.Substring(0, ccref.Length - 1);
            if (string.IsNullOrEmpty(ccref))
                ccref = "Chat";

            await Shell.Current.GoToAsync("ChatPage", new Dictionary<string, object>
                    {
                        { "CHATCHILD", cc },
                        { "CHATTITLE", ccref },
                        { "CHATSTATUS", cc.Status }
                    });

        }
        catch (Exception e)
        {
            string s = e.Message;
        }
    }

    void NewChat_Click(System.Object sender, System.EventArgs e)
    {
        NewChat_Click();
    }

    async void NewChat_Click()
    {
        XDelServiceRef.ChatSession[]? list = null;
        ChatSessions css = null;
        ChatSession cs = null;
        ChatChild? cc = null;
        ObservableCollection<long>? sIDXs_final = new();
        try
        {
            if (NetworkHelper.IsDisconnected())
            {
                await AppShell.Current.DisplayAlertAsync("No Internet Connection", "Please check your internet connection and try again.", "OK");
                return;
            }

            if (canProceed())
            {
                logininfo = AppSession.logininfo;
                ClientInfo ci = logininfo.clientInfo;
                await showProgress_Dialog("Processing...");
                css = await Task.Run(() => xs.VCS_CreateAsync(ci.Web_UID, 0));
                if (css != null && css.Status == 0)
                    list = css.Sessions;

                await closeProgress_dialog();

                if (list != null && list.Length > 0)
                {
                    cs = list[0];
                    sIDXs_final.Add(cs.SessionIDX);
                    cc = new ChatChild { Fulltext = "", Reference = "", Chatsession = cs, SessionIDXs = sIDXs_final, isNewChat = true, Status = 1 };

                    await Shell.Current.GoToAsync("ChatPage", new Dictionary<string, object>
                    {
                        { "CHATCHILD", cc },
                        { "CHATTITLE", "Chat" },
                        { "CHATSTATUS", 1 }
                    });
                }
            }
            else
            {
                await closeProgress_dialog();
                await AppShell.Current.DisplayAlertAsync("", errmsg_relogin, "OK");
                await common.BackToLogin();
            }
        }
        catch (Exception ex)
        {
            string s = ex.Message;
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

            /*if (await AppShell.Current.DisplayAlert("Are you sure?", "You will be logged out.", "Yes", "No"))
                await ForceLogoutAsync();*/
            if (await AppShell.Current.DisplayAlertAsync("Are you sure?", "You will be logged out.", "Yes", "No"))
                await ForceLogoutAsync();
        }
        catch (Exception ex)
        {
            string s = ex.Message;
        }
    }

    void Refresh_Click(System.Object sender, System.EventArgs e)
    {
        try
        {
            logininfo = AppSession.logininfo;
            if (logininfo != null)
                _ = getChatHistory(); // fire & forget
        }
        catch (Exception ex)
        {
            string s = ex.Message;
        }
    }

    private async Task showProgress_Dialog(string msg)
    {
        try
        {
            //await MainThread.InvokeOnMainThreadAsync(() =>
            //{
            //    _ = _progressService.ShowAsync(msg);
            //});

            //await Task.Delay(50);

            await _progressService.ShowAsync(msg);
            await Task.Yield();
            await Task.Delay(100);
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine(ex);
        }
    }

    private async Task closeProgress_dialog()
    {
        try
        {
            //await MainThread.InvokeOnMainThreadAsync(async () =>
            //{
            //    await _progressService.DismissAsync();
            //});

            await _progressService.DismissAsync();
            await Task.Yield();
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine(ex);
        }
    }

}