using CommunityToolkit.Mvvm.Messaging;
using ETHAN.classes;
using ETHAN.ProgressDialog;
using System;
using System.ComponentModel.DataAnnotations;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using XDelServiceRef;

namespace ETHAN.Views;

public partial class ChangeMobilePage : ContentPage, IRecipient<AppSleepMessage>, IRecipient<AppResumeMessage>
{
    private XOEWSSoapClient xs = new XOEWSSoapClient(XOEWSSoapClient.EndpointConfiguration.XOEWSSoap);
    private readonly IProgressDialogService _progressService;
    private bool _loadedOnce = false;
    private LoginInfo? logininfo;

    public ChangeMobilePage(IProgressDialogService progressService)
    {
        InitializeComponent();
        _progressService = progressService;
        Shell.SetTabBarIsVisible(this, false);
    }

    protected override void OnSizeAllocated(double width, double height)
    {
        base.OnSizeAllocated(width, height);

        if (DeviceInfo.Idiom == DeviceIdiom.Tablet)
        {
            //AppContainer.WidthRequest = Math.Min(width * 0.32, 400); // 40% of screen width, max 800
            //AppContainer.HeightRequest = Math.Min(height * 0.85, 1000);
            AppContainer.WidthRequest = 500; // 40% of screen width, max 800
            AppContainer.HeightRequest = height;
        }
        else
        {
            AppContainer.WidthRequest = width; // fill phone screen
            AppContainer.HeightRequest = height;
        }
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        try
        {
            if (!_loadedOnce)
            {
                _loadedOnce = true;

                WeakReferenceMessenger.Default.Register<AppSleepMessage>(this);
                WeakReferenceMessenger.Default.Register<AppResumeMessage>(this);

                setMobileRuleStatus(true, true);
                setEmailRuleStatus(true, true);
                ValidateButton();

                Dispatcher.Dispatch(() =>
                {
                    _ = StartOtpUiUpdateAsync();
                });
            }
        }
        catch (Exception e)
        {
            string s = e.Message;
        }
    }

    private async Task StartOtpUiUpdateAsync()
    {
        try
        {
            Guid motp = Guid.Empty;
            Guid eotp = Guid.Empty;

            /*await Task.Run(async () =>
            {
                motp = await GetStoredGuidAsync("FORGOT_MOTP_SESSIONID");
                eotp = await GetStoredGuidAsync("FORGOT_EOTP_SESSIONID");
            });*/

            motp = await GetStoredGuid(AppSession.FORGOT_MOTP_SESSIONID);
            eotp = await GetStoredGuid(AppSession.FORGOT_EOTP_SESSIONID);

            await MainThread.InvokeOnMainThreadAsync(() =>
            {
                GMOTP.IsVisible = motp != Guid.Empty;
                GEOTP.IsVisible = eotp != Guid.Empty;
            });
        }
        catch (Exception e)
        {
            string s = e.Message;
        }
    }

    private async Task<Guid> GetStoredGuid(string k)
    {
        try
        {
            if (!string.IsNullOrWhiteSpace(k) && Guid.TryParse(k, out var g))
                return g;
        }
        catch (Exception e)
        {

        }
        return Guid.Empty;
    }

    protected override void OnDisappearing()
    {
        base.OnDisappearing();
        if (_progressService != null && _progressService.IsShowing)
            return;

        WeakReferenceMessenger.Default.Unregister<AppSleepMessage>(this);
        WeakReferenceMessenger.Default.Unregister<AppResumeMessage>(this);
    }

    public void Receive(AppSleepMessage message)
    {
        if (message.Value)  // true = app backgrounded
        {
            
        }
    }

    public void Receive(AppResumeMessage message)
    {
        if (message.Value)
        {
            // App came back to foreground — put your logic here
            // Example: restart OTP countdown UI update
            MainThread.BeginInvokeOnMainThread(() =>
            {
                RestoreEmailOtpState();
                RestoreMobileOtpState();
            });
        }
    }

    private void RestoreEmailOtpState()
    {
        try
        {
            bool otpWasSent = !string.IsNullOrWhiteSpace(AppSession.FORGOT_EOTP_SESSIONID)
                              && Guid.TryParse(AppSession.FORGOT_EOTP_SESSIONID, out var g)
                              && g != Guid.Empty;

            bool otpVerified = !string.IsNullOrWhiteSpace(AppSession.FORGOT_EOTP_VERIFIED);

            if (otpVerified)
            {
                btnEmailOTP.IsVisible = false;
                txtEmail.InputTransparent = true;
                GMobile.IsVisible = true;
            }

            if (!otpVerified && !otpWasSent)
                ValidateEmailRule();

            if (!otpWasSent)
                return; // OTP was never sent or already cleared — nothing to restore

            // Email tab is active — restore its locked state
            // txtEmail keeps its text already (page is still alive, not re-created)

            GMobile.IsVisible = false;
            GMOTP.IsVisible = false;
            txtEmail.InputTransparent = true;     // Email field stays locked
            btnEmailOTP.IsVisible = false;         // "Get OTP" stays hidden
            hsEmail.IsVisible = false;

            iconVerifiedEmail.IsVisible = otpVerified;

            GEOTP.IsVisible = true;                 // OTP entry row stays visible

            // Refocus OTP entry so keyboard comes up ready
            txtEmailOTP.Focus();
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"RestoreEmailOtpState error: {ex.Message}");
        }
    }

    private void RestoreMobileOtpState()
    {
        try
        {
            bool otpWasSent = !string.IsNullOrWhiteSpace(AppSession.FORGOT_MOTP_SESSIONID)
                              && Guid.TryParse(AppSession.FORGOT_MOTP_SESSIONID, out var g)
                              && g != Guid.Empty;

            bool otpVerified = !string.IsNullOrWhiteSpace(AppSession.FORGOT_MOTP_VERIFIED);

            if (otpVerified)
            {
                btnMobileOTP.IsVisible = false;
                txtMobile.InputTransparent = true;
            }

            if (!otpVerified && !otpWasSent)
                ValidateMobileRule();

            if (!otpWasSent)
                return; // OTP was never sent or already cleared — nothing to restore

            // Mobile tab is active — restore its locked state
            // txtMobile keeps its text already (page is still alive, not re-created)

            txtMobile.InputTransparent = true;     // mobile field stays locked
            btnMobileOTP.IsVisible = false;         // "Get OTP" stays hidden
            hsMobile.IsVisible = false;

            iconVerifiedMobile.IsVisible = otpVerified;

            GMOTP.IsVisible = true;                 // OTP entry row stays visible

            // Refocus OTP entry so keyboard comes up ready
            txtMobileOTP.Focus();
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"RestoreMobileOtpState error: {ex.Message}");
        }
    }

    void BackToHome(System.Object sender, System.EventArgs e)
    {
        try
        {
            BackToHomePage();
        }
        catch (Exception ex)
        {
            string s = ex.Message;
        }
    }

    async Task BackToHomePage()
    {
        try
        {
            await AppSession.SetFORGOT_EUIDX("");
            await AppSession.SetTEMP_UID("");
            await AppSession.SetFORGOT_MOTP_SESSIONIDAsync("");
            await AppSession.SetFORGOT_EOTP_SESSIONIDAsync("");
            await AppSession.SetFORGOT_MOTP_VERIFIED("");
            await AppSession.SetFORGOT_EOTP_VERIFIED("");

            MainThread.BeginInvokeOnMainThread(async () =>
            {
                BindingContext = null;
                string v = string.Empty;
                await Shell.Current.GoToAsync("///CardShellPage", new Dictionary<string, object>
                    {
                        { "BARCODE", null },
                        { "DEFAULTTAB", "Settings" },
                    });
            }
            );
        }
        catch (Exception e)
        {
            string s = e.Message;
        }
    }

    protected override bool OnBackButtonPressed()
    {
        try
        {
            _ = BackToHomePage(); //_ = is Fire-and-forget async call safely
        }
        catch (Exception e)
        {
            string s = e.Message;
        }
        return true;
    }

    bool emailOk = false;
    bool mobileOk = false;

    void onMobileFieldChanged(object sender, TextChangedEventArgs e)
    {
        ValidateMobileRule();
        ValidateButton();
    }

    void ValidateMobileRule()
    {
        try
        {
            string txt = txtMobile.Text;
            mobileOk = IsValidMobile(txt);
            setMobileRuleStatus(mobileOk, false);
        }
        catch (Exception e)
        {
            string s = e.Message;
        }
    }

    void setMobileRuleStatus(bool isValid, bool isOnAppearing)
    {
        try
        {
            bool otpVerified = !string.IsNullOrWhiteSpace(AppSession.FORGOT_MOTP_VERIFIED);
            hsMobile.IsVisible = otpVerified ? false : (!isOnAppearing && !isValid);
            btnMobileOTP.IsVisible = otpVerified ? false : (!isOnAppearing && isValid);
        }
        catch (Exception e)
        {
            string s = e.Message;
        }
    }

    async void showGMOTP()
    {
        Guid stored_MOTP_SESSIONID = Guid.Empty;
        try
        {
            var value = AppSession.FORGOT_MOTP_SESSIONID;

            if (!string.IsNullOrWhiteSpace(value) && Guid.TryParse(value, out var parsed))
            {
                stored_MOTP_SESSIONID = parsed;
            }
        }
        catch (Exception ex)
        {
            //// iOS can throw if keychain access fails
            System.Diagnostics.Debug.WriteLine($"SecureStorage error: {ex.Message}");
        }

        try
        {
            GMOTP.IsVisible = !(stored_MOTP_SESSIONID == Guid.Empty);
        }
        catch (Exception e)
        {
            string s = e.Message;
        }
    }

    async void showGMobile()
    {
        string? stored_EOTP_VERIFIED = null;
        try
        {
            stored_EOTP_VERIFIED = AppSession.FORGOT_EOTP_VERIFIED;
        }
        catch (Exception ex)
        {
            //// iOS can throw if keychain access fails
            System.Diagnostics.Debug.WriteLine($"SecureStorage error: {ex.Message}");
        }

        bool eotpVerified = stored_EOTP_VERIFIED != null && stored_EOTP_VERIFIED == "t";

        try
        {
            lblEnterMobileText.IsVisible = eotpVerified;
            GMobile.IsVisible = eotpVerified;
            if (eotpVerified)
                ValidateMobileRule();
        }
        catch (Exception e)
        {
            string s = e.Message;
        }
    }

    async void showGEOTP()
    {
        Guid stored_EOTP_SESSIONID = Guid.Empty;
        try
        {
            var value = AppSession.FORGOT_EOTP_SESSIONID;

            if (!string.IsNullOrWhiteSpace(value) && Guid.TryParse(value, out var parsed))
            {
                stored_EOTP_SESSIONID = parsed;
            }
        }
        catch (Exception ex)
        {
            //// iOS can throw if keychain access fails
            System.Diagnostics.Debug.WriteLine($"SecureStorage error: {ex.Message}");
        }

        try
        {
            GEOTP.IsVisible = !(stored_EOTP_SESSIONID == Guid.Empty);
        }
        catch (Exception e)
        {
            string s = e.Message;
        }
    }

    void onEmailFieldChanged(object sender, TextChangedEventArgs e)
    {
        ValidateEmailRule();
        ValidateButton();
    }

    void ValidateEmailRule()
    {
        try
        {
            string txt = txtEmail.Text;
            emailOk = IsValidEmail(txt);
            setEmailRuleStatus(emailOk, false);
        }
        catch (Exception e)
        {
            string s = e.Message;
        }
    }

    void setEmailRuleStatus(bool isValid, bool isOnAppearing)
    {
        try
        {
            bool otpVerified = !string.IsNullOrWhiteSpace(AppSession.FORGOT_EOTP_VERIFIED);
            hsEmail.IsVisible = otpVerified ? false : (!isOnAppearing && !isValid);
            btnEmailOTP.IsVisible = otpVerified ? false : (!isOnAppearing && isValid);
        }
        catch (Exception e)
        {
            string s = e.Message;
        }
    }

    void ValidateButton()
    {
        bool ok = false;

        try
        {
            bool hasMobile = !string.IsNullOrWhiteSpace(txtMobile.Text);
            bool hasEmail = !string.IsNullOrWhiteSpace(txtEmail.Text);
            bool hasVMOTP = iconVerifiedMobile.IsVisible;
            bool hasVEOTP = iconVerifiedEmail.IsVisible;

            ok = hasMobile && hasVMOTP && hasEmail && hasVEOTP && mobileOk && emailOk;

            btnUpdate.IsEnabled = ok;

            btnUpdate.Style = ok ? (Style)Application.Current.Resources["bstyleOrange"] : (Style)Application.Current.Resources["bstyleDisabled"];
        }
        catch (Exception e)
        {
            string s = e.Message;
        }
    }

    bool IsValidEmail(string email)
    {
        if (string.IsNullOrEmpty(email))
            return false;

        ////IsRealWorldEmail
        return Regex.IsMatch(email,
                @"^[A-Za-z0-9._%+-]+@[A-Za-z0-9.-]+\.[A-Za-z]{2,}$");
    }

    bool IsValidMobile(string mobile)
    {
        if (string.IsNullOrWhiteSpace(mobile))
            return false;

        // must be 8 digits starting with 8 or 9
        return Regex.IsMatch(mobile, @"^[89][0-9]{7}$");
    }

    async void RequestEmailOTP(object sender, EventArgs e)
    {
        try
        {
            await GetEmailOTP();
        }
        catch (Exception ex)
        {
            string s = ex.Message;
        }
    }

    private async Task GetEmailOTP()
    {
        Guid sessionid = Guid.Empty;
        long EIDX = 0;
        long eridx = 0;
        string TEMP_UID = "";
        string WEB_UID = "";
        XOE_ETHAN_Receiver x = new XOE_ETHAN_Receiver();
        string EMAIL = txtEmail.Text.Trim().ToString();
        try
        {
            logininfo = AppSession.logininfo;

            if (logininfo == null
                || (logininfo.ETHAN_Receiver == null)
                || (logininfo != null && logininfo.ETHAN_Receiver != null && logininfo.ETHAN_Receiver.IDX == 0)
                || (logininfo != null && logininfo.ETHAN_Receiver != null && string.IsNullOrEmpty(logininfo.ETHAN_Receiver.UID)))
            {
                await ShowAlertSafe("", "Unable to get OTP. Please log out and log in again.");
                return;
            }

            x = logininfo!.ETHAN_Receiver!;
            WEB_UID = x!.UID;
            EIDX = x.IDX;

            await showProgress_Dialog("Processing...");

            //pass webuid, eridx, mobile or email to check if mobile or email exists n belong to eridx,
            //n return XOE_Get_ETHAN_ReceiverIDX_By_MobileEmailAsync
            x = await xs.XOE_MobileEmailUserMatchAsync(WEB_UID, EIDX, EMAIL, "");

            if (x == null || x.Status != 0)
            {
                await closeProgress_dialog();
                await ShowAlertSafe("", x?.Message ?? "Error Processing.");
                return;
            }

            eridx = x.IDX;
            TEMP_UID = x.TEMP_UID;
            if (eridx == 0)
            {
                await closeProgress_dialog();
                await ShowAlertSafe("", "Error requesting One-Time-Pin (OTP).\neridx not found.");
                return;
            }

            if (eridx != EIDX)
            {
                await closeProgress_dialog();
                await ShowAlertSafe("", "Error requesting One-Time-Pin (OTP).\nVerification fails.");
                return;
            }

            if (string.IsNullOrEmpty(TEMP_UID))
            {
                await closeProgress_dialog();
                await ShowAlertSafe("", "Error requesting One-Time-Pin (OTP).\nTEMP_UID not found.");
                return;
            }

            sessionid = await xs.XOE_Request_2FAAsync(TEMP_UID, 0, eridx, "", EMAIL);
            if (sessionid == Guid.Empty)
            {
                await closeProgress_dialog();
                await ShowAlertSafe("", "Error requesting One-Time-Pin (OTP). Please try again.");
                return;
            }

            await AppSession.SetTEMP_UID(TEMP_UID);
            await AppSession.SetFORGOT_EUIDX(eridx.ToString());
            await AppSession.SetFORGOT_EOTP_SESSIONIDAsync(sessionid.ToString());

            await closeProgress_dialog();
            await ShowAlertSafe("", "Your One-Time-Pin (OTP) Email sent!");

            setEmailRuleStatus(true, false);
            showGEOTP();
            btnEmailOTP.IsVisible = false;
            txtEmail.InputTransparent = true;
            txtEmailOTP.Focus();
        }
        catch (Exception e)
        {
            string s = e.Message;
            await closeProgress_dialog();
            await ShowAlertSafe("", e.Message);
        }
    }

    async void IsValidEmailOTP(object sender, EventArgs e)
    {
        try
        {
            if (string.IsNullOrEmpty(txtEmailOTP.Text))
                await DisplayAlertAsync("", "Please enter the One-Time-Pin (OTP) sent to your Email Address.", "Ok");
            else
                await ValidateEmailOTP();
        }
        catch (Exception ex)
        {
            string s = ex.Message;
        }
    }

    private async Task ValidateEmailOTP()
    {
        Guid sessionId = Guid.Empty;

        try
        {
            var stored = AppSession.FORGOT_EOTP_SESSIONID;
            if (!string.IsNullOrWhiteSpace(stored))
                Guid.TryParse(stored, out sessionId);
        }
        catch { }

        if (sessionId == Guid.Empty)
        {
            await ShowAlertSafe("", "Unable to verify One-Time-Pin (OTP). Session not found.");
            return;
        }

        string TEMP_UID = AppSession.TEMP_UID;
        if (string.IsNullOrEmpty(TEMP_UID))
        {
            await AppSession.SetFORGOT_EUIDX("");
            await AppSession.SetTEMP_UID("");
            await AppSession.SetFORGOT_EOTP_SESSIONIDAsync("");
            await ShowAlertSafe("", "Unable to verify One-Time-Pin (OTP). TEMP_UID not found.");
            return;
        }

        string _eridx = AppSession.FORGOT_EUIDX;
        if (string.IsNullOrEmpty(_eridx))
        {
            await AppSession.SetFORGOT_EUIDX("");
            await AppSession.SetTEMP_UID("");
            await AppSession.SetFORGOT_EOTP_SESSIONIDAsync("");
            await ShowAlertSafe("", "Unable to verify One-Time-Pin (OTP). _eridx not found.");
            return;
        }
        long eridx = long.Parse(_eridx);

        try
        {
            await showProgress_Dialog("Verifying...");

            var result = await xs.XOE_Verify_OTPAsync(TEMP_UID, 0, eridx, sessionId, txtEmailOTP.Text);

            await closeProgress_dialog();

            if (result == null)
            {
                await ShowAlertSafe("", "Error verifying One-Time-Pin (OTP).");
                return;
            }

            if (result.Status != 0)
            {
                await ShowAlertSafe("", result.Message);
                return;
            }

            ////SUCCESS
            await AppSession.SetFORGOT_EOTP_VERIFIED("t");
            await AppSession.SetFORGOT_EOTP_SESSIONIDAsync("");

            iconVerifiedEmail.IsVisible = true;
            btnEmailOTP.IsVisible = false;
            txtEmailOTP.Text = "";
            txtEmail.InputTransparent = true;
            hsEmail.IsVisible = false;

            ValidateButton();
            showGMobile();
            await StartOtpUiUpdateAsync();
            await ShowAlertSafe("", "Email Address Verified.");
        }
        catch (Exception e)
        {
            string s = e.Message;
            await closeProgress_dialog();
            await ShowAlertSafe("", e.Message);
        }
    }

    async void RequestMobileOTP(object sender, EventArgs e)
    {
        try
        {
            await GetMobileOTP();
        }
        catch (Exception ex)
        {
            string s = ex.Message;
        }
    }

    private async Task GetMobileOTP()
    {
        long EIDX = 0;
        long eridx = 0;
        string TEMP_UID = "";
        string WEB_UID = "";
        XOE_ETHAN_Receiver x = new XOE_ETHAN_Receiver();
        try
        {
            string MOBILE = txtMobile.Text.Trim().ToString();
            logininfo = AppSession.logininfo;

            if (logininfo == null
                || (logininfo.ETHAN_Receiver == null)
                || (logininfo != null && logininfo.ETHAN_Receiver != null && logininfo.ETHAN_Receiver.IDX == 0)
                || (logininfo != null && logininfo.ETHAN_Receiver != null && string.IsNullOrEmpty(logininfo.ETHAN_Receiver.UID)))
            {
                await ShowAlertSafe("", "Unable to get OTP. Please log out and log in again.");
                return;
            }

            x = logininfo!.ETHAN_Receiver!;
            WEB_UID = x!.UID;
            EIDX = x.IDX;

            await showProgress_Dialog("Processing...");

            var xbReg = await xs.XOE_HasRegistered_MobileEmailAsync(MOBILE, "");
            if (xbReg == null || (xbReg != null && xbReg.Status != 0))
            {
                await closeProgress_dialog();
                await ShowAlertSafe("", xbReg?.Message ?? "Error Processing.");
                return;
            }

            var xbPend = await xs.XOE_HasPending_Registration_2FAAsync(MOBILE, "");
            if (xbPend == null || (xbPend != null && xbPend.Status != 0))
            {
                await closeProgress_dialog();
                await ShowAlertSafe("", xbPend?.Message ?? "Error Processing.");
                return;
            }

            TEMP_UID = AppSession.TEMP_UID;
            var sessionid = await xs.XOE_Request_2FAAsync(TEMP_UID, 0, EIDX, MOBILE, "");
            if (sessionid == Guid.Empty)
            {
                await closeProgress_dialog();
                await ShowAlertSafe("", "Error requesting One-Time-Pin (OTP). Please try again.");
                return;
            }

            await AppSession.SetFORGOT_MOTP_SESSIONIDAsync(sessionid.ToString());

            await closeProgress_dialog();
            await ShowAlertSafe("", "Your One-Time-Pin (OTP) Email sent!");
            await UiPump.Yield();

            setMobileRuleStatus(true, false);
            showGMOTP();
            btnMobileOTP.IsVisible = false;
            txtMobile.InputTransparent = true;
            txtMobileOTP.Focus();
        }
        catch (Exception e)
        {
            string s = e.Message;
            await closeProgress_dialog();
            await ShowAlertSafe("", e.Message);
        }
    }

    async void IsValidMobileOTP(object sender, EventArgs e)
    {
        try
        {
            if (string.IsNullOrEmpty(txtMobileOTP.Text))
                await DisplayAlertAsync("", "Please enter the One-Time-Pin (OTP) sent to your Mobile.", "Ok");
            else
                await ValidateMobileOTP();
        }
        catch (Exception ex)
        {
            string s = ex.Message;
        }
    }

    private async Task ValidateMobileOTP()
    {
        Guid sessionId = Guid.Empty;

        try
        {
            var stored = AppSession.FORGOT_MOTP_SESSIONID;
            if (!string.IsNullOrWhiteSpace(stored))
                Guid.TryParse(stored, out sessionId);
        }
        catch { }

        if (sessionId == Guid.Empty)
        {
            await AppSession.SetFORGOT_EUIDX("");
            await AppSession.SetTEMP_UID("");
            await AppSession.SetFORGOT_MOTP_SESSIONIDAsync("");
            await ShowAlertSafe("", "Unable to verify One-Time-Pin (OTP). Session not found.");
            return;
        }

        string TEMP_UID = AppSession.TEMP_UID;
        if (string.IsNullOrEmpty(TEMP_UID))
        {
            await AppSession.SetFORGOT_EUIDX("");
            await AppSession.SetTEMP_UID("");
            await AppSession.SetFORGOT_MOTP_SESSIONIDAsync("");
            await ShowAlertSafe("", "Unable to verify One-Time-Pin (OTP). TEMP_UID not found.");
            return;
        }

        string _eridx = AppSession.FORGOT_EUIDX;
        if (string.IsNullOrEmpty(_eridx))
        {
            await AppSession.SetFORGOT_EUIDX("");
            await AppSession.SetTEMP_UID("");
            await AppSession.SetFORGOT_MOTP_SESSIONIDAsync("");
            await ShowAlertSafe("", "Unable to verify One-Time-Pin (OTP). _eridx not found.");
            return;
        }
        long eridx = long.Parse(_eridx);

        try
        {
            await showProgress_Dialog("Verifying...");

            var result = await xs.XOE_Verify_OTPAsync(TEMP_UID, 0, eridx, sessionId, txtMobileOTP.Text);

            await closeProgress_dialog();

            if (result == null)
            {
                await ShowAlertSafe("", "Error verifying One-Time-Pin (OTP).");
                return;
            }

            if (result.Status != 0)
            {
                await ShowAlertSafe("", result.Message);
                return;
            }

            ////SUCCESS
            await AppSession.SetFORGOT_MOTP_VERIFIED("t");
            await AppSession.SetFORGOT_MOTP_SESSIONIDAsync("");

            iconVerifiedMobile.IsVisible = true;
            btnMobileOTP.IsVisible = false;
            txtMobileOTP.Text = "";
            txtMobile.InputTransparent = true;
            hsMobile.IsVisible = false;

            ValidateButton();
            showGMobile();
            await StartOtpUiUpdateAsync();
            await ShowAlertSafe("", "Mobile Number Verified.");
        }
        catch (Exception e)
        {
            string s = e.Message;
            await closeProgress_dialog();
            await ShowAlertSafe("", e.Message);
        }
    }

    async void btnUpdate_Click(System.Object sender, System.EventArgs e)
    {
        await UpdateMobile();
    }

    private async Task UpdateMobile()
    {
        string mobile = txtMobile.Text.Trim().ToString();
        XWSBase x = new XWSBase();
        XOE_ETHAN_Receiver xx = new XOE_ETHAN_Receiver();
        try
        {
            logininfo = AppSession.logininfo;

            if (logininfo == null || (logininfo.ETHAN_Receiver == null))
            {
                await ShowAlertSafe("", "Unable to get OTP. Please log out and log in again.");
                return;
            }

            xx = logininfo!.ETHAN_Receiver!;

            var stored = AppSession.FORGOT_EUIDX;
            if (string.IsNullOrEmpty(stored) || !long.TryParse(stored, out var eridx) || eridx == 0)
            {
                await ShowAlertSafe("", "Error Processing.\nUnable to retrieve registration record.");
                return;
            }

            string TEMP_UID = AppSession.TEMP_UID;
            if (string.IsNullOrEmpty(TEMP_UID))
            {
                await AppSession.SetFORGOT_EUIDX("");
                await AppSession.SetTEMP_UID("");
                await AppSession.SetFORGOT_MOTP_SESSIONIDAsync("");
                await AppSession.SetFORGOT_EOTP_SESSIONIDAsync("");
                await AppSession.SetFORGOT_MOTP_VERIFIED("");
                await AppSession.SetFORGOT_EOTP_VERIFIED("");
                await ShowAlertSafe("", "Error Processing. TEMP_UID not found.");
                return;
            }

            await showProgress_Dialog("Processing...");

            x = await xs.XOE_Update_ETHAN_Receiver_Mobile_EmailAsync(TEMP_UID, eridx, "", mobile);
            if (x == null || x.Status != 0)
            {
                await closeProgress_dialog();
                await ShowAlertSafe("", x?.Message ?? "Error Processing.");
                return;
            }

            xx.MOBILE = mobile;
            logininfo.ETHAN_Receiver = xx;
            AppSession.SetLoginInfo(logininfo);

            await closeProgress_dialog();

            await ShowAlertSafe("", "Mobile number updated.");
            await UiPump.Yield();
            await BackToHomePage();
        }
        catch (Exception e)
        {
            string s = e.Message;
        }
    }

    private async Task showProgress_Dialog(string msg)
    {
        try
        {
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
            await _progressService.DismissAsync();
            await Task.Yield();
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine(ex);
        }
    }

    private Task ShowAlertSafe(string title, string message, string button = "OK")
    {
        if (MainThread.IsMainThread)
            return DisplayAlertAsync(title, message, button);

        return MainThread.InvokeOnMainThreadAsync(() =>
            DisplayAlertAsync(title, message, button));
    }

}