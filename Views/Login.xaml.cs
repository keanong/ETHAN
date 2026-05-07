using Microsoft.Maui.ApplicationModel; // for AppInfo
using ETHAN.classes;
using ETHAN.ProgressDialog;
using System.Text.RegularExpressions;
using XDelServiceRef;

namespace ETHAN.Views;

public partial class Login : ContentPage
{

    //private XWSXSoapClient xs = new XWSSoapClient(XWSSoapClient.EndpointConfiguration.XWSSoap);
    private XOEWSSoapClient xs = new XOEWSSoapClient(XOEWSSoapClient.EndpointConfiguration.XOEWSSoap);
    //private ProgressDialogService_ _progressService;
    private readonly IProgressDialogService _progressService;
    private bool _loadedOnce = false;

    private CancellationTokenSource? _ctsM;
    private CancellationTokenSource? _ctsE;

    /*public Login()
	{
		InitializeComponent();
	}*/
    public Login(IProgressDialogService progressService)
    {
        InitializeComponent();
        _progressService = progressService;
    }

    protected override void OnSizeAllocated(double width, double height)
    {
        base.OnSizeAllocated(width, height);

        if (DeviceInfo.Idiom == DeviceIdiom.Tablet)
        {
            AppContainer.WidthRequest = 500;
            AppContainer.HeightRequest = height;
        }
        else
        {
            AppContainer.WidthRequest = width; // fill phone screen
            AppContainer.HeightRequest = height;
        }
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();
        try
        {
#if IOS
this.HideSoftInputOnTapped = false;
#endif
            // Retrieve version and build number
            var version = AppInfo.VersionString;   // e.g., "1.0.2"
            //var build = AppInfo.BuildString;       // e.g., "12"

            // Display version in the label
            //lblVersion.Text = $"Version {version} (Build {build})";
            lblVersion.Text = $"Version {version}";

            if (_loadedOnce)
                return;

            _loadedOnce = true;

            lblSenderReceiver.Text = "s";
            lblMobileEmail.Text = "m";

            LblSender.TextColor = (Color)Application.Current.Resources["XDelOrange"];
            IndicatorSender.IsVisible = true;

            LblReceiver.TextColor = (Color)Application.Current.Resources["Gray600"];
            IndicatorReceiver.IsVisible = false;

            SenderPanel.IsVisible = true;
            ReceiverPanel.IsVisible = false;

            LblMobile.TextColor = (Color)Application.Current.Resources["XDelOrange"];
            IndicatorMobile.IsVisible = true;

            LblEmail.TextColor = (Color)Application.Current.Resources["Gray600"];
            IndicatorEmail.IsVisible = false;

            setSUsernameRuleStatus(true, true);
            setSPwdRuleStatus(true, true);
            setMobileRuleStatus(true, true);
            setEmailRuleStatus(true, true);

            MobileTab_ResetFields();

            ValidateSenderLoginButton();
            ValidateReceiverLoginBtn();

            txtSUsername.Text = "";
            txtSPassword.Text = "";
            txtSPassword.IsPassword = true;
            btnSTogglePwd.Source = "eye_show_80.png";

            txtMobile.Text = "";
            txtEmail.Text = "";
            txtRPassword.IsPassword = true;
            btnRTogglePwd.Source = "eye_show_80.png";

            Dispatcher.Dispatch(async () =>
            {
                try
                {
                    await clearAlLStoredValues();
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine(ex);
                }
            });

        }
        catch (Exception e)
        {
            string s = e.Message;
        }
    }

    private async Task StartOtpUiUpdateAsync()
    {
        //await Task.Delay(250); // allow Shell/UI to settle
        try
        {
            Guid motp = Guid.Empty;
            Guid eotp = Guid.Empty;

            await Task.Run(async () =>
            {
                motp = await GetStoredGuidAsync("LOGIN_MOTP_SESSIONID");
                eotp = await GetStoredGuidAsync("LOGIN_EOTP_SESSIONID");
            });

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

    private async Task<Guid> GetStoredGuidAsync(string key)
    {
        try
        {
            var value = await SecureStorage.GetAsync(key);

            if (!string.IsNullOrWhiteSpace(value) && Guid.TryParse(value, out var g))
                return g;
        }
        catch { }

        return Guid.Empty;
    }

    async Task clearAlLStoredValues()
    {
        //await Task.Delay(250); // allow Shell/UI to settle
        try
        {
            /*await Task.Run(() =>
            {
                SecureStorage.Remove("MOTP_SESSIONID");
                SecureStorage.Remove("EOTP_SESSIONID");
                SecureStorage.Remove("MOTP_VERIFIED");
                SecureStorage.Remove("EOTP_VERIFIED");
                SecureStorage.Remove("PENDING_EUIDX");
                SecureStorage.Remove("FORGOT_EUIDX");
                SecureStorage.Remove("FORGOT_MOTP_SESSIONID");
                SecureStorage.Remove("FORGOT_EOTP_SESSIONID");
                SecureStorage.Remove("FORGOT_MOTP_VERIFIED");
                SecureStorage.Remove("FORGOT_EOTP_VERIFIED");
                SecureStorage.Remove("LOGIN_MOTP_SESSIONID");
                SecureStorage.Remove("LOGIN_EOTP_SESSIONID");
                SecureStorage.Remove("LOGIN_MOTP_VERIFIED");
                SecureStorage.Remove("LOGIN_EOTP_VERIFIED");
            });*/

            await Task.Run(async () =>
            {
                await AppSession.SetREG_MOTP_SESSIONIDAsync("");
                await AppSession.SetREG_EOTP_SESSIONIDAsync("");
                await AppSession.SetMOTP_VERIFIEDAsync("");
                await AppSession.SetEOTP_VERIFIEDAsync("");
                await AppSession.SetPENDING_EUIDXAsync("");
                await AppSession.SetFORGOT_EUIDX("");
                await AppSession.SetFORGOT_MOTP_SESSIONIDAsync("");
                await AppSession.SetFORGOT_EOTP_SESSIONIDAsync("");
                await AppSession.SetFORGOT_MOTP_VERIFIED("");
                await AppSession.SetFORGOT_EOTP_VERIFIED("");
                await AppSession.SetLOGIN_MOTP_SESSIONIDAsync("");
                await AppSession.SetLOGIN_EOTP_SESSIONIDAsync("");
                await AppSession.SetLOGIN_MOTP_VERIFIED("");
                await AppSession.SetLOGIN_EOTP_VERIFIED("");
            });

        }
        catch (Exception e)
        {
            string s = e.Message;
        }
    }

    protected override bool OnBackButtonPressed()
    {
        Application.Current.Quit();
        return true;
    }

    private async void ReceiverLoginBtn_Click(object sender, EventArgs e)
    {
        string me = lblMobileEmail.Text;
        try
        {
            await EUAuth();
        } catch (Exception ex)
        {
            string s = ex.Message;
        }
    }

    private async void SenderLoginBtn_Click(object sender, EventArgs e)
    {
        
        try
        {
            await Auth(txtSUsername.Text.Trim().ToString(), txtSPassword.Text.Trim().ToString());
        }
        catch (Exception ex)
        {
            string s = ex.Message;
        }
    }

    private async Task Auth(string u, string p)
    {
        ClientInfo? ci = null;
        XDelServiceRef.XWSBase xb = new XDelServiceRef.XWSBase();

        try
        {
            SenderLoginBtn.InputTransparent = true;
            await showProgress_Dialog("Processing...");
            //await UiPump.Yield();

            ci = await xs.XOE_Pre_AuthenticateXOAsync(u, p);

            if (ci != null && ci.Status == 0 && ci.CNIDX > 0)
            {
                Guid sessionid = Guid.Empty;
                Guid pending_sessionid = Guid.Empty;
                pending_sessionid = await xs.XOE_Get_Pending_2FAAsync(ci.Web_UID, ci.CNIDX, 0, "", "");
                if (pending_sessionid != Guid.Empty)
                    xb = await xs.XOE_Update_2FA_Status_By_SessionIDAsync(ci.Web_UID, ci.CNIDX, 0, pending_sessionid, 1);

                if (xb.Status == 0)
                {
                    sessionid = await xs.XOE_Request_2FAAsync(ci.Web_UID, ci.CNIDX, 0, "", "");
                    if (sessionid != Guid.Empty)
                    {
                        await AppSession.SetLOGIN_SUIDX(ci.CNIDX.ToString());
                        await AppSession.SetAPP_UID(ci.Web_UID);
                        await AppSession.SetLOGIN_SOTP_SESSIONIDAsync(sessionid.ToString());
                    }
                    else
                    {
                        ci.Status = -11;
                        ci.Message = "Unable to request One-Time-Pin (OTP). Please login again.";
                    }
                }
                else
                {
                    ci.Status = -11;
                    ci.Message = "There has been an internal error.";
                }
            }

            await closeProgress_dialog();
            //await UiPump.Yield();
            SenderLoginBtn.InputTransparent = false;

            if (ci != null && ci.Status == 0)
            {
                txtSUsername.Text = "";
                txtSPassword.Text = "";
                GSOTP.IsVisible = true;
                ShowSenderLogin(false);
                AppSession.SetLoginInfo(null);
            }
            else if ((ci != null && ci.Status != 0) || ci == null)
            {
                AppSession.SetLoginInfo(null);
                txtSUsername.Text = "";
                txtSPassword.Text = "";
                GSOTP.IsVisible = false;
                ValidateSenderLoginButton();
                //hsSPwd.IsVisible = false;
                await UiPump.Yield();

                if (ci != null && ci.Status != 0)
                    await DisplayAlertAsync("", ci.Message, "Ok");
                else
                    await DisplayAlertAsync("", "Please enter your Username or Password", "Ok");
            }
        }
        catch (Exception e)
        {
            SenderLoginBtn.InputTransparent = false;
            string s = e.Message;
            await UiPump.Yield();
            await closeProgress_dialog();
            //await DisplayAlertAsync("Error", s + "" + e.StackTrace.ToString(), "Ok");
        }
    }

    private async Task EUAuth()
    {
        ClientInfo? ci = null;
        XDelServiceRef.XWSBase xb = new XDelServiceRef.XWSBase();
        string me = lblMobileEmail.Text;
        try
        {
            ReceiverLoginBtn.InputTransparent = true;
            await showProgress_Dialog("Processing...");
            //await UiPump.Yield();

            ci = await xs.XOE_Pre_AuthenticateXOEAsync(
                me.Equals("m") ? txtMobile.Text.Trim().ToString() : "",
                me.Equals("e") ? txtEmail.Text.Trim().ToString() : "",
                txtRPassword.Text.Trim().ToString());

            if(ci != null && ci.Status == 0 && ci.CNIDX > 0)
            {
                Guid sessionid = Guid.Empty;
                Guid pending_sessionid = Guid.Empty;

                pending_sessionid = await xs.XOE_Get_Pending_2FAAsync(ci.Web_UID, 0, ci.CNIDX, "", "");
                if (pending_sessionid != Guid.Empty)
                    xb = await xs.XOE_Update_2FA_Status_By_SessionIDAsync(ci.Web_UID, 0, ci.CNIDX, pending_sessionid, 1);

                if (xb.Status == 0)
                {
                    if (me.Equals("m"))
                    {
                        sessionid = await xs.XOE_Request_2FAAsync(ci.Web_UID, 0, ci.CNIDX, txtMobile.Text.Trim().ToString(), "");
                        await AppSession.SetLOGIN_EUIDX(ci.CNIDX.ToString());
                        await AppSession.SetTEMP_UID(ci.Web_UID);
                        await AppSession.SetLOGIN_MOTP_SESSIONIDAsync(sessionid.ToString());
                    }
                    else if (me.Equals("e"))
                    {
                        sessionid = await xs.XOE_Request_2FAAsync(ci.Web_UID, 0, ci.CNIDX, "", txtEmail.Text.Trim().ToString());
                        await AppSession.SetLOGIN_EUIDX(ci.CNIDX.ToString());
                        await AppSession.SetTEMP_UID(ci.Web_UID);
                        await AppSession.SetLOGIN_EOTP_SESSIONIDAsync(sessionid.ToString());
                    }
                    else
                    {
                        ci.Status = -11;
                        ci.Message = "Unable to request One-Time-Pin (OTP). Please login again.";
                    }                    
                }
                else
                {
                    ci.Status = -11;
                    ci.Message = "There has been an internal error.";
                }
            }

            await closeProgress_dialog();
            //await UiPump.Yield();
            ReceiverLoginBtn.InputTransparent = false;

            if (ci != null && ci.Status == 0)
            {
                txtSUsername.Text = "";
                txtSPassword.Text = "";
                txtMobile.Text = "";
                txtEmail.Text = "";
                GSOTP.IsVisible = false;
                GMOTP.IsVisible = me.Equals("m");
                GEOTP.IsVisible = me.Equals("e");
                ShowReceiverLogin(false);

                AppSession.SetLoginInfo(null);
            }
            else if ((ci != null && ci.Status != 0) || ci == null)
            {
                AppSession.SetLoginInfo(null);
                txtSUsername.Text = "";
                txtSPassword.Text = "";
                txtRPassword.Text = "";
                GSOTP.IsVisible = false;
                GMOTP.IsVisible = false;
                GEOTP.IsVisible = false;
                ValidateReceiverLoginBtn();
                hsSPwd.IsVisible = false;
                hsMobile.IsVisible = false;
                hsEmail.IsVisible = false;
                await UiPump.Yield();

                if (ci != null && ci.Status != 0)
                    await DisplayAlertAsync("", ci.Message, "Ok");
                else
                    await DisplayAlertAsync("", "Authentication failed or insufficient rights to perform action.\nPlease call +65 6376 1838 to verify your account details.", "Ok");
            }
        }
        catch (Exception e)
        {
            ReceiverLoginBtn.InputTransparent = false;
            string s = e.Message;
            await UiPump.Yield();
            await closeProgress_dialog();
            //await UiPump.Yield();
            await ShowAlertSafe("", e.Message);
        }
    }

    // Track visibility per field
    bool PwdVisible = false;
    bool PwdVisibleS = false;
    bool PwdVisibleR = false;
    bool PwdVisibleFP = false;
    bool PwdVisibleFPC = false;

    private void OnTogglePwdClicked(object sender, EventArgs e)
    {
        if (sender is ImageButton btn)
        {
            if (btn == btnSTogglePwd)
            {
                PwdVisibleS = !PwdVisibleS;
                txtSPassword.IsPassword = !PwdVisibleS;
                btnSTogglePwd.Source = PwdVisibleS ? "eye_hide_80.png" : "eye_show_80.png";
            }

            if (btn == btnRTogglePwd)
            {
                PwdVisibleR = !PwdVisibleR;
                txtRPassword.IsPassword = !PwdVisibleR;
                btnRTogglePwd.Source = PwdVisibleR ? "eye_hide_80.png" : "eye_show_80.png";
            }            
        }
    }

    private async void RegisterClicked(object sender, TappedEventArgs e)
    {
        try
        {
            _loadedOnce = false;
            await Shell.Current.GoToAsync("LoginReg");
        } catch (Exception ex) { 
            string s = ex.Message;
        }
    }

    private async void SenderTab_Tapped(object sender, TappedEventArgs e)
    {
        string SenderReceiver = lblSenderReceiver.Text;
        if (!string.IsNullOrEmpty(SenderReceiver) && SenderReceiver.Equals("s"))
            return;

        await AppSession.SetLOGIN_SOTP_SESSIONIDAsync("");

        lblSenderReceiver.Text = "s";
        lblMobileEmail.Text = "m";

        LblSender.TextColor = (Color)Application.Current.Resources["XDelOrange"];
        IndicatorSender.IsVisible = true;

        LblReceiver.TextColor = (Color)Application.Current.Resources["Gray600"];
        IndicatorReceiver.IsVisible = false;

        SenderPanel.IsVisible = true;
        ReceiverPanel.IsVisible = false;

        txtSUsername.Text = "";
        txtSPassword.Text = "";
        txtSPassword.IsPassword = true;
        btnSTogglePwd.Source = "eye_show_80.png";

        lblSUsernameInvalid.IsVisible = true;
        hsSUsername.IsVisible = false;
        lblSPasswordInvalid.IsVisible = true;
        hsSPwd.IsVisible = false;
        SenderLoginBtn.InputTransparent = false;

        MobileTab_ResetFields();
    }

    private async void ReceiverTab_Tapped(object sender, TappedEventArgs e)
    {
        string SenderReceiver = lblSenderReceiver.Text;
        if (!string.IsNullOrEmpty(SenderReceiver) && SenderReceiver.Equals("r"))
            return;

        lblSenderReceiver.Text = "r";
        lblMobileEmail.Text = "m";

        LblSender.TextColor = (Color)Application.Current.Resources["Gray600"];
        IndicatorSender.IsVisible = false;

        LblReceiver.TextColor = (Color)Application.Current.Resources["XDelOrange"];
        IndicatorReceiver.IsVisible = true;

        SenderPanel.IsVisible = false;
        ReceiverPanel.IsVisible = true;

        txtSUsername.Text = "";
        txtSPassword.Text = "";
        txtSPassword.IsPassword = true;
        btnSTogglePwd.Source = "eye_show_80.png";
        ReceiverLoginBtn.InputTransparent = false;

        MobileTab_ResetFields();
    }

    private void MobileTab_Tapped(object sender, TappedEventArgs e)
    {
        lblSenderReceiver.Text = "r";
        string mobileemail = lblMobileEmail.Text;
        if (!string.IsNullOrEmpty(mobileemail) && mobileemail.Equals("m"))
            return;

        MobileTab_ResetFields();
    }

    private async void MobileTab_ResetFields()
    {
        await AppSession.SetLOGIN_MOTP_SESSIONIDAsync("");
        SecureStorage.Remove("LOGIN_EUIDX");
        SecureStorage.Remove("LOGIN_MOTP_SESSIONID");
        SecureStorage.Remove("LOGIN_MOTP_VERIFIED");
        SecureStorage.Remove("LOGIN_EOTP_SESSIONID");
        SecureStorage.Remove("LOGIN_EOTP_VERIFIED");
        lblMobileEmail.Text = "m";

        LblMobile.TextColor = (Color)Application.Current.Resources["XDelOrange"];
        IndicatorMobile.IsVisible = true;

        LblEmail.TextColor = (Color)Application.Current.Resources["Gray600"];
        IndicatorEmail.IsVisible = false;

        ShowReceiverLogin(true);

        iconVerifiedEmail.IsVisible = false;
        iconPendingEmail.IsVisible = false;
        txtEmail.InputTransparent = false;
        lblECountdown.IsVisible = false;
        btnEmailOTP.IsVisible = false;

        if (_ctsM is { IsCancellationRequested: false })
        {
            lblMCountdown.IsVisible = true;
            txtMobile.InputTransparent = true;
            GMOTP.IsVisible = true;
            btnMobileOTP.IsVisible = false;
            iconVerifiedMobile.IsVisible = false;
            hsMobile.IsVisible = false;
            lblMobileInvalid.IsVisible = false;
        }
        else
        {
            txtMobile.Text = "";
            txtMobileOTP.Text = "";
            lblMCountdown.IsVisible = false;
            txtMobile.InputTransparent = false;
            GMOTP.IsVisible = false;
            btnMobileOTP.IsVisible = false;
            iconVerifiedMobile.IsVisible = false;
            hsMobile.IsVisible = false;
            lblMobileInvalid.IsVisible = true;
        }
    }

    private void EmailTab_Tapped(object sender, TappedEventArgs e)
    {
        lblSenderReceiver.Text = "r";
        string mobileemail = lblMobileEmail.Text;
        if (!string.IsNullOrEmpty(mobileemail) && mobileemail.Equals("e"))
            return;
        EmailTab_ResetFields();
    }

    private async void EmailTab_ResetFields()
    {
        await AppSession.SetLOGIN_EOTP_SESSIONIDAsync("");
        SecureStorage.Remove("LOGIN_EUIDX");
        SecureStorage.Remove("LOGIN_MOTP_SESSIONID");
        SecureStorage.Remove("LOGIN_MOTP_VERIFIED");
        SecureStorage.Remove("LOGIN_EOTP_SESSIONID");
        SecureStorage.Remove("LOGIN_EOTP_VERIFIED");
        lblMobileEmail.Text = "e";

        LblMobile.TextColor = (Color)Application.Current.Resources["Gray600"];
        IndicatorMobile.IsVisible = false;

        LblEmail.TextColor = (Color)Application.Current.Resources["XDelOrange"];
        IndicatorEmail.IsVisible = true;

        ShowReceiverLogin(true);

        iconVerifiedMobile.IsVisible = false;
        iconPendingMobile.IsVisible = false;
        txtMobile.InputTransparent = false;
        lblMCountdown.IsVisible = false;
        btnMobileOTP.IsVisible = false;

        if (_ctsE is { IsCancellationRequested: false })
        {
            lblECountdown.IsVisible = true;
            txtEmail.InputTransparent = true;
            GEOTP.IsVisible = true;
            btnEmailOTP.IsVisible = false;
            iconVerifiedEmail.IsVisible = false;
            hsEmail.IsVisible = false;
            lblEmailInvalid.IsVisible = false;
        }
        else
        {
            txtEmail.Text = "";
            txtEmailOTP.Text = "";
            lblECountdown.IsVisible = false;
            txtEmail.InputTransparent = false;
            GEOTP.IsVisible = false;
            btnEmailOTP.IsVisible = false;
            iconVerifiedEmail.IsVisible = false;
            hsEmail.IsVisible = false;
            lblEmailInvalid.IsVisible = true;
        }
    }

    bool emailOk = false;
    bool mobileOk = false;
    bool pwdOk = false;
    bool minEightOk = false;
    bool lowerOk = false;
    bool upperOk = false;
    bool numOk = false;
    bool susernameOk = false;
    bool spwdOk = false;
    bool sminEightOk = false;
    bool slowerOk = false;
    bool supperOk = false;
    bool snumOk = false;

    void ValidateSenderLoginButton()
    {
        try
        {
            bool hasUsername = !string.IsNullOrWhiteSpace(txtSUsername.Text);
            bool hasSPassword = !string.IsNullOrWhiteSpace(txtSPassword.Text);
            var stored = AppSession.LOGIN_SOTP_SESSIONID;
            bool hasPSOTP = !string.IsNullOrWhiteSpace(stored);

            bool ok = !hasPSOTP && hasUsername && hasSPassword && susernameOk && spwdOk && sminEightOk && slowerOk && supperOk && snumOk;

            SenderLoginBtn.IsEnabled = ok;

            SenderLoginBtn.Style = ok ? (Style)Application.Current.Resources["bstyleOrange"] : (Style)Application.Current.Resources["bstyleDisabled"];
        }
        catch (Exception e)
        {
            string s = e.Message;
        }
    }

    void ShowSenderLogin(bool show)
    {
        try
        {
            txtSUsername.Text = "";
            txtSPassword.Text = "";
            txtSenderOTP.Text = "";
            SenderLoginBtn.IsEnabled = false;
            SenderLoginBtn.Style = (Style)Application.Current.Resources["bstyleDisabled"];
            GSUsername.IsVisible = show;
            GSPassword.IsVisible = show;
            lblSForgotPassword.IsVisible = show;
            SenderLoginBtn.IsVisible = show;
            SenderLoginBtn.InputTransparent = !show;

            if (show)
            {
                lblSUsernameInvalid.IsVisible = true;
                hsSUsername.IsVisible = false;
                lblSPasswordInvalid.IsVisible = true;
                hsSPwd.IsVisible = false;
                GSOTP.IsVisible = false;
            }
        }
        catch (Exception e)
        {
            SenderLoginBtn.InputTransparent = false;
            string s = e.Message;
        }
    }

    void ShowReceiverLogin(bool show)
    {
        try
        {
            string mobileemail = lblMobileEmail.Text;
            txtRPassword.Text = "";
            txtRPassword.IsPassword = true;
            
            btnRTogglePwd.Source = "eye_show_80.png";

            ReceiverLoginBtn.IsEnabled = false;
            ReceiverLoginBtn.Style = (Style)Application.Current.Resources["bstyleDisabled"];
            if (mobileemail.Equals("m"))
            {
                gbMobile.IsVisible = show;
                gbEmail.IsVisible = false;
                txtMobile.Text = "";
                txtMobileOTP.Text = "";
            }
                
            if (mobileemail.Equals("e"))
            {
                gbMobile.IsVisible = false;
                gbEmail.IsVisible = show;
                txtEmail.Text = "";
                txtEmailOTP.Text = "";
            }

            GRPassword.IsVisible = show;

            lblRForgotPassword.IsVisible = show;
            lblRRegister.IsVisible = show;
            ReceiverLoginBtn.IsVisible = show;
            ReceiverLoginBtn.InputTransparent = !show;

            if (show)
            {
                if (mobileemail.Equals("m"))
                {
                    lblMobileInvalid.IsVisible = true;
                    hsMobile.IsVisible = false;
                    lblRForgotPassword.IsVisible = true;
                    hsPwd.IsVisible = false;
                    lblRPasswordInvalid.IsVisible = true;
                    GMOTP.IsVisible = false;
                    GEOTP.IsVisible = false;
                }
                    

                if (mobileemail.Equals("e"))
                {
                    lblEmailInvalid.IsVisible = true;
                    hsEmail.IsVisible = false;
                    lblRForgotPassword.IsVisible = true;
                    hsPwd.IsVisible = false;
                    lblRPasswordInvalid.IsVisible = true;
                    GMOTP.IsVisible = false;
                    GEOTP.IsVisible = false;
                }
            }
        }
        catch (Exception e)
        {
            ReceiverLoginBtn.InputTransparent = false;
            string s = e.Message;
        }
    }

    void ValidateReceiverLoginBtn()
    {
        string me = lblMobileEmail.Text;
        bool ok = false;
        try
        {

            if (!string.IsNullOrEmpty(me) && me.Equals("m"))
            {
                bool hasMobile = !string.IsNullOrWhiteSpace(txtMobile.Text);
                bool hasSPassword = !string.IsNullOrWhiteSpace(txtRPassword.Text);
                var stored = AppSession.LOGIN_MOTP_SESSIONID;
                bool hasPMOTP = !string.IsNullOrWhiteSpace(stored);

                ok = !hasPMOTP && hasMobile && hasSPassword && mobileOk && pwdOk && minEightOk && lowerOk && upperOk && numOk;

                ReceiverLoginBtn.IsEnabled = ok;

                ReceiverLoginBtn.Style = ok ? (Style)Application.Current.Resources["bstyleOrange"] : (Style)Application.Current.Resources["bstyleDisabled"];
            }
            if (!string.IsNullOrEmpty(me) && me.Equals("e"))
            {
                bool hasEmail = !string.IsNullOrWhiteSpace(txtEmail.Text);
                bool hasSPassword = !string.IsNullOrWhiteSpace(txtRPassword.Text);
                var stored = AppSession.LOGIN_EOTP_SESSIONID;
                bool hasPEOTP = !string.IsNullOrWhiteSpace(stored);

                ok = !hasPEOTP && hasEmail && hasSPassword && emailOk && pwdOk && minEightOk && lowerOk && upperOk && numOk;

                ReceiverLoginBtn.IsEnabled = ok;

                ReceiverLoginBtn.Style = ok ? (Style)Application.Current.Resources["bstyleOrange"] : (Style)Application.Current.Resources["bstyleDisabled"];
            }
        }
        catch (Exception e)
        {
            string s = e.Message;
        }
    }

    void onSUsernameFieldChanged(object sender, TextChangedEventArgs e)
    {
        ValidateSUsernameRule();
        ValidateSenderLoginButton();
    }

    void ValidateSUsernameRule()
    {
        try
        {
            string txt = txtSUsername.Text;
            susernameOk = IsValidSUsername(txt);
            setSUsernameRuleStatus(susernameOk, false);
        }
        catch (Exception e)
        {
            string s = e.Message;
        }
    }

    bool IsValidSUsername(string txt)
    {
        return !string.IsNullOrWhiteSpace(txt);
    }

    void setSUsernameRuleStatus(bool isValid, bool isOnAppearing)
    {
        lblSUsernameInvalid.IsVisible = isValid;
        hsSUsername.IsVisible = (!isOnAppearing && !isValid);
    }

    void OnSPwdFieldChanged(object sender, TextChangedEventArgs e)
    {
        ValidateSRules();
        ValidateSenderLoginButton();
    }

    void ValidateSRules()
    {
        try
        {
            spwdOk = IsValidSPwd();
            setSPwdRuleStatus(spwdOk, false);
        }
        catch (Exception e)
        {
            string s = e.Message;
        }
    }

    bool IsValidSPwd()
    {
        string pwd = txtSPassword.Text ?? "";

        sminEightOk = pwd.Length >= 8;
        slowerOk = pwd.Any(char.IsLower);
        supperOk = pwd.Any(char.IsUpper);
        snumOk = pwd.Any(char.IsDigit);

        return (sminEightOk && slowerOk && supperOk && snumOk);
    }

    void setSPwdRuleStatus(bool isValid, bool isOnAppearing)
    {
        try
        {
            lblSPasswordInvalid.IsVisible = isValid;
            hsSPwd.IsVisible = (!isOnAppearing && !isValid);
        }
        catch (Exception e)
        {
            string s = e.Message;
        }
    }


    void onMobileFieldChanged(object sender, TextChangedEventArgs e)
    {
        ValidateMobileRule();
        ValidateReceiverLoginBtn();
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

    bool IsValidMobile(string mobile)
    {
        if (string.IsNullOrWhiteSpace(mobile))
            return false;

        // must be 8 digits starting with 8 or 9
        return Regex.IsMatch(mobile, @"^[89][0-9]{7}$");
    }

    void setMobileRuleStatus(bool isValid, bool isOnAppearing)
    {
        try
        {
            lblMobileInvalid.IsVisible = (isValid);
            hsMobile.IsVisible = (!isOnAppearing && !isValid);
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
            //var value = await SecureStorage.GetAsync("LOGIN_MOTP_SESSIONID");
            var value = AppSession.LOGIN_MOTP_SESSIONID;

            if (!string.IsNullOrWhiteSpace(value) && Guid.TryParse(value, out var parsed))
                stored_MOTP_SESSIONID = parsed;
        }
        catch (Exception ex)
        {
            // iOS can throw if keychain access fails
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

    async void showGEOTP()
    {
        Guid stored_EOTP_SESSIONID = Guid.Empty;
        try
        {
            //var value = await SecureStorage.GetAsync("LOGIN_EOTP_SESSIONID");
            var value = AppSession.LOGIN_EOTP_SESSIONID;

            if (!string.IsNullOrWhiteSpace(value) && Guid.TryParse(value, out var parsed))
                stored_EOTP_SESSIONID = parsed;
        }
        catch (Exception ex)
        {
            // iOS can throw if keychain access fails
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
        ValidateRules();
        ValidateReceiverLoginBtn();
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

    bool IsValidEmail(string email)
    {
        if (string.IsNullOrEmpty(email))
            return false;

        //return new EmailAddressAttribute().IsValid(email);

        ////IsRealWorldEmail
        return Regex.IsMatch(email,
                @"^[A-Za-z0-9._%+-]+@[A-Za-z0-9.-]+\.[A-Za-z]{2,}$");
    }

    void setEmailRuleStatus(bool isValid, bool isOnAppearing)
    {
        try
        {
            lblEmailInvalid.IsVisible = isValid;
            hsEmail.IsVisible = (!isOnAppearing && !isValid);
        }
        catch (Exception e)
        {
            string s = e.Message;
        }
    }

    void OnPwdFieldChanged(object sender, TextChangedEventArgs e)
    {
        ValidateRules();
        ValidateReceiverLoginBtn();
    }

    void ValidateRules()
    {
        try
        {
            pwdOk = IsValidPwd();
            setPwdRuleStatus(pwdOk, false);
        } catch (Exception e)
        {
            string s = e.Message;
        }
    }

    bool IsValidPwd()
    {
        string pwd = txtRPassword.Text ?? "";

        minEightOk = pwd.Length >= 8;
        lowerOk = pwd.Any(char.IsLower);
        upperOk = pwd.Any(char.IsUpper);
        numOk = pwd.Any(char.IsDigit);

        return (minEightOk && lowerOk && upperOk && numOk);
    }
    
    void setPwdRuleStatus(bool isValid, bool isOnAppearing)
    {
        try
        {
            lblRPasswordInvalid.IsVisible = isValid;
            hsPwd.IsVisible = (!isOnAppearing && !isValid);
        }
        catch (Exception e)
        {
            string s = e.Message;
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
        Guid sessionid = Guid.Empty;
        long eridx = 0;
        string TEMP_UID = "";
        XOE_ETHAN_Receiver x = new XOE_ETHAN_Receiver();
        string MOBILE = txtMobile.Text.Trim().ToString();
        try
        {

            await showProgress_Dialog("Processing...");
            //await UiPump.Yield();

            x = await xs.XOE_Get_ETHAN_ReceiverIDX_By_MobileEmailAsync(MOBILE, "");
            if (x == null || x.Status == -1)
            {
                await closeProgress_dialog();
                //await UiPump.Yield();
                await ShowAlertSafe("", x?.Message ?? "Error Processing.");
                return;
            }

            /*if (!long.TryParse(x.state?.ToString(), out eridx))
            {
                // handle error: invalid IDX returned
                eridx = 0;
            }*/
            eridx = x.IDX;
            TEMP_UID = x.TEMP_UID;
            if (eridx == 0)
            {
                await closeProgress_dialog();
                //await UiPump.Yield();
                await ShowAlertSafe("", "Error requesting One-Time-Pin (OTP).\neridx not found.");
                return;
            }

            sessionid = await xs.XOE_Request_2FAAsync(TEMP_UID, 0, eridx, MOBILE, "");
            if (sessionid == Guid.Empty)
            {
                await closeProgress_dialog();
                //await UiPump.Yield();
                await ShowAlertSafe("", "Error requesting One-Time-Pin (OTP).\nTEMP_UID not found.");
                return;
            }

            await AppSession.SetTEMP_UID(TEMP_UID);
            await AppSession.SetLOGIN_EUIDX(eridx.ToString());
            //await SecureStorage.SetAsync("LOGIN_EUIDX", eridx.ToString());
            await SecureStorage.SetAsync("LOGIN_MOTP_SESSIONID", sessionid.ToString());

            await closeProgress_dialog();
            //await UiPump.Yield();
            await ShowAlertSafe("", "Your One-Time-Pin (OTP) SMS sent!");

            setMobileRuleStatus(true, false);
            showGMOTP();
            btnMobileOTP.IsVisible = false;
            iconPendingMobile.IsVisible = true;
            txtMobile.InputTransparent = true;
            txtMobileOTP.Focus();
        }
        catch (Exception e)
        {
            string s = e.Message;
            await closeProgress_dialog();
            //await UiPump.Yield();
            await ShowAlertSafe("", e.Message);
        }
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
        long eridx = 0;
        string TEMP_UID = "";
        //XDelServiceRef.XWSBase x = new XWSBase();
        XOE_ETHAN_Receiver x = new XOE_ETHAN_Receiver();
        string EMAIL = txtEmail.Text.Trim().ToString();
        try
        {

            await showProgress_Dialog("Processing...");
            //await UiPump.Yield();

            x = await xs.XOE_Get_ETHAN_ReceiverIDX_By_MobileEmailAsync("", EMAIL);
            if (x == null || x.Status == -1)
            {
                await closeProgress_dialog();
                //await UiPump.Yield();
                await ShowAlertSafe("", x?.Message ?? "Error requesting One-Time-Pin (OTP).");
                return;
            }

            /*if (!long.TryParse(x.state?.ToString(), out eridx))
            {
                // handle error: invalid IDX returned
                eridx = 0;
            }*/
            eridx = x.IDX;
            TEMP_UID = x.TEMP_UID;
            if (eridx == 0)
            {
                await closeProgress_dialog();
                //await UiPump.Yield();
                await ShowAlertSafe("", "Error requesting One-Time-Pin (OTP).\neridx not found.");
                return;
            }

            sessionid = await xs.XOE_Request_2FAAsync(x.TEMP_UID, 0, eridx, "", EMAIL);
            if (sessionid == Guid.Empty)
            {
                await closeProgress_dialog();
                //await UiPump.Yield();
                await ShowAlertSafe("", "Error requesting One-Time-Pin (OTP). Please try again.");
                return;
            }

            await AppSession.SetTEMP_UID(TEMP_UID);
            await AppSession.SetLOGIN_EUIDX(eridx.ToString());
            //await SecureStorage.SetAsync("LOGIN_EUIDX", eridx.ToString());
            await SecureStorage.SetAsync("LOGIN_EOTP_SESSIONID", sessionid.ToString());

            await closeProgress_dialog();
            //await UiPump.Yield();
            await ShowAlertSafe("", "Your One-Time-Pin (OTP) Email sent!");

            setEmailRuleStatus(true, false);
            showGEOTP();
            btnEmailOTP.IsVisible = false;
            iconPendingEmail.IsVisible = true;
            txtEmail.InputTransparent = true;
            txtEmailOTP.Focus();
        }
        catch (Exception e)
        {
            string s = e.Message;
            await closeProgress_dialog();
            //await UiPump.Yield();
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
            //var stored = await SecureStorage.GetAsync("LOGIN_MOTP_SESSIONID");
            var stored = AppSession.LOGIN_MOTP_SESSIONID;
            if (!string.IsNullOrWhiteSpace(stored))
                Guid.TryParse(stored, out sessionId);
        }
        catch { }

        if (sessionId == Guid.Empty)
        {
            await AppSession.SetLOGIN_EUIDX("");
            await AppSession.SetTEMP_UID("");
            await AppSession.SetLOGIN_MOTP_SESSIONIDAsync("");
            await ShowAlertSafe("", "Unable to verify One-Time-Pin (OTP). Session not found.");
            ShowReceiverLogin(true);
            return;
        }

        string TEMP_UID = AppSession.TEMP_UID;
        if (string.IsNullOrEmpty(TEMP_UID))
        {
            await AppSession.SetLOGIN_EUIDX("");
            await AppSession.SetTEMP_UID("");
            await AppSession.SetLOGIN_MOTP_SESSIONIDAsync("");
            await ShowAlertSafe("", "Unable to verify One-Time-Pin (OTP). TEMP_UID not found.");
            ShowReceiverLogin(true);
            return;
        }

        string _eridx = AppSession.LOGIN_EUIDX;
        if (string.IsNullOrEmpty(_eridx))
        {
            await AppSession.SetLOGIN_EUIDX("");
            await AppSession.SetTEMP_UID("");
            await AppSession.SetLOGIN_EOTP_SESSIONIDAsync("");
            await ShowAlertSafe("", "Unable to verify One-Time-Pin (OTP). _eridx not found.");
            ShowReceiverLogin(true);
            return;
        }
        long eridx = long.Parse(_eridx);

        ClientInfo? ci = null;
        DecimalReturn? PrePaidBalance = null;
        XDelServiceRef.AddressStructure? defAddress = null;
        XDelServiceRef.SettingsInfo csi = null;
        XDelServiceRef.SettingsInfo CNSettingsInfo = null;
        LoginInfo loginInfo = new LoginInfo();

        try
        {
            await showProgress_Dialog("Verifying...");
            //await UiPump.Yield();
            ci = await xs.XOE_Verify_OTP_Async(TEMP_UID, 0, eridx, sessionId, txtMobileOTP.Text);

            await closeProgress_dialog();
            //await UiPump.Yield();

            if (ci == null)
            {
                await AppSession.SetTEMP_UID("");
                await AppSession.SetLOGIN_MOTP_SESSIONIDAsync("");
                //SecureStorage.Remove("LOGIN_MOTP_SESSIONID");
                await ShowAlertSafe("", "Error verifying One-Time-Pin (OTP).");
                ShowReceiverLogin(true);
                return;
            }

            if (ci.Status != 0)
            {
                await ShowAlertSafe("", ci.Message);
                if (ci.Status == -2) //expired
                {
                    await AppSession.SetLOGIN_EUIDX("");
                    await AppSession.SetTEMP_UID("");
                    await AppSession.SetLOGIN_MOTP_SESSIONIDAsync("");
                    //SecureStorage.Remove("LOGIN_MOTP_SESSIONID");
                    ShowReceiverLogin(true);
                }
                return;
            }

            //SUCCESS
            await SecureStorage.SetAsync("LOGIN_MOTP_VERIFIED", "t");
            await AppSession.SetLOGIN_EUIDX("");
            await AppSession.SetTEMP_UID("");
            await AppSession.SetLOGIN_MOTP_SESSIONIDAsync("");
            //SecureStorage.Remove("LOGIN_EOTP_SESSIONID");

            loginInfo.clientInfo = ci;

            AppSession.SetLoginInfo(loginInfo);

            await AppSession.SetLoginModeAsync("r");
            await Shell.Current.GoToAsync("///CardShellPage", new Dictionary<string, object>
                {
                    { "BARCODE", null },
                    { "LOGIN", "Y" },
                    { "DEFAULTTAB", "Home" },
                });
        }
        catch (Exception e)
        {
            string s = e.Message;
            await closeProgress_dialog();
            //await UiPump.Yield();
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
            //var stored = await SecureStorage.GetAsync("LOGIN_EOTP_SESSIONID");
            var stored = AppSession.LOGIN_EOTP_SESSIONID;
            if (!string.IsNullOrWhiteSpace(stored))
                Guid.TryParse(stored, out sessionId);
        }
        catch { }

        if (sessionId == Guid.Empty)
        {
            await AppSession.SetLOGIN_EUIDX("");
            await AppSession.SetTEMP_UID("");
            await AppSession.SetLOGIN_EOTP_SESSIONIDAsync("");
            await ShowAlertSafe("", "Unable to verify One-Time-Pin (OTP). Session not found.");
            ShowReceiverLogin(true);
            return;
        }

        //string TEMP_UID = await SecureStorage.GetAsync("TEMP_UID");
        string TEMP_UID = AppSession.TEMP_UID;
        if (string.IsNullOrEmpty(TEMP_UID))
        {
            await AppSession.SetLOGIN_EUIDX("");
            await AppSession.SetTEMP_UID("");
            await AppSession.SetLOGIN_EOTP_SESSIONIDAsync("");
            await ShowAlertSafe("", "Unable to verify One-Time-Pin (OTP). TEMP_UID not found.");
            ShowReceiverLogin(true);
            return;
        }

        string _eridx = AppSession.LOGIN_EUIDX;
        if (string.IsNullOrEmpty(_eridx))
        {
            await AppSession.SetLOGIN_EUIDX("");
            await AppSession.SetTEMP_UID("");
            await AppSession.SetLOGIN_EOTP_SESSIONIDAsync("");
            await ShowAlertSafe("", "Unable to verify One-Time-Pin (OTP). _eridx not found.");
            ShowReceiverLogin(true);
            return;
        }
        long eridx = long.Parse(_eridx);

        ClientInfo? ci = null;
        DecimalReturn? PrePaidBalance = null;
        XDelServiceRef.AddressStructure? defAddress = null;
        XDelServiceRef.SettingsInfo csi = null;
        XDelServiceRef.SettingsInfo CNSettingsInfo = null;
        LoginInfo loginInfo = new LoginInfo();

        try
        {
            await showProgress_Dialog("Verifying...");
            //await UiPump.Yield();

            ci = await xs.XOE_Verify_OTP_Async(TEMP_UID, 0, eridx, sessionId, txtEmailOTP.Text);

            await closeProgress_dialog();
            //await UiPump.Yield();

            if (ci == null)
            {
                await AppSession.SetLOGIN_EUIDX("");
                await AppSession.SetTEMP_UID("");
                await AppSession.SetLOGIN_EOTP_SESSIONIDAsync("");
                //SecureStorage.Remove("LOGIN_EOTP_SESSIONID");
                await ShowAlertSafe("", "Error verifying One-Time-Pin (OTP).");
                ShowReceiverLogin(true);
                return;
            }

            if (ci.Status != 0)
            {
                await ShowAlertSafe("", ci.Message);
                if (ci.Status == -2) //expired
                {
                    await AppSession.SetLOGIN_EUIDX("");
                    await AppSession.SetTEMP_UID("");
                    await AppSession.SetLOGIN_EOTP_SESSIONIDAsync("");
                    //SecureStorage.Remove("LOGIN_EOTP_SESSIONID");
                    ShowReceiverLogin(true);
                }
                return;
            }

            //SUCCESS
            await SecureStorage.SetAsync("LOGIN_EOTP_VERIFIED", "t");
            await AppSession.SetLOGIN_EUIDX("");
            await AppSession.SetTEMP_UID("");
            await AppSession.SetLOGIN_EOTP_SESSIONIDAsync("");
            //SecureStorage.Remove("LOGIN_EOTP_SESSIONID");

            loginInfo.clientInfo = ci;

            AppSession.SetLoginInfo(loginInfo);

            await AppSession.SetLoginModeAsync("r");
            await Shell.Current.GoToAsync("///CardShellPage", new Dictionary<string, object>
                {
                    { "BARCODE", null },
                    { "LOGIN", "Y" },
                    { "DEFAULTTAB", "Home" },
                });
        }
        catch (Exception e)
        {
            string s = e.Message;
            await closeProgress_dialog();
            //await UiPump.Yield();
            await ShowAlertSafe("", e.Message);
        }
    }

    async void IsValidSenderOTP(object sender, EventArgs e)
    {
        try
        {
            if (string.IsNullOrEmpty(txtSenderOTP.Text))
                await DisplayAlertAsync("", "Please enter the One-Time-Pin (OTP) sent to your Email Address.", "Ok");
            else
                await ValidateSenderOTP();
        }
        catch (Exception ex)
        {
            string s = ex.Message;
        }
    }

    private async Task ValidateSenderOTP()
    {
        Guid sessionId = Guid.Empty;
        try
        {
            //var stored = await SecureStorage.GetAsync("LOGIN_SOTP_SESSIONID");
            var stored = AppSession.LOGIN_SOTP_SESSIONID;
            if (!string.IsNullOrWhiteSpace(stored))
                Guid.TryParse(stored, out sessionId);
        }
        catch { }

        if (sessionId == Guid.Empty)
        {
            await ShowAlertSafe("", "Unable to verify One-Time-Pin (OTP). Session not found.");
            await AppSession.SetLOGIN_SUIDX("");
            await AppSession.SetAPP_UID("");
            await AppSession.SetLOGIN_SOTP_SESSIONIDAsync("");
            ShowSenderLogin(true);
            return;
        }

        //string APP_UID = await SecureStorage.GetAsync("APP_UID");
        string APP_UID = AppSession.APP_UID;
        if (string.IsNullOrEmpty(APP_UID))
        {
            await ShowAlertSafe("", "Unable to verify One-Time-Pin (OTP). APP_UID not found.");
            await AppSession.SetLOGIN_SUIDX("");
            await AppSession.SetAPP_UID("");
            await AppSession.SetLOGIN_SOTP_SESSIONIDAsync("");
            ShowSenderLogin(true);
            return;
        }

        string _eridx = AppSession.LOGIN_SUIDX;
        if (string.IsNullOrEmpty(_eridx))
        {
            await ShowAlertSafe("", "Unable to verify One-Time-Pin (OTP). _eridx not found.");
            await AppSession.SetLOGIN_SUIDX("");
            await AppSession.SetAPP_UID("");
            await AppSession.SetLOGIN_SOTP_SESSIONIDAsync("");
            ShowSenderLogin(true);
            return;
        }
        long eridx = long.Parse(_eridx);

        ClientInfo? ci = null;
        DecimalReturn? PrePaidBalance = null;
        XDelServiceRef.AddressStructure? defAddress = null;
        XDelServiceRef.SettingsInfo csi = null;
        XDelServiceRef.SettingsInfo CNSettingsInfo = null;
        LoginInfo loginInfo = new LoginInfo();

        try
        {
            await showProgress_Dialog("Verifying...");
            //await UiPump.Yield();

            ci = await xs.XOE_Verify_OTP_Async(APP_UID, eridx, 0, sessionId, txtSenderOTP.Text);

            await closeProgress_dialog();
            //await UiPump.Yield();

            if (ci == null)
            {
                await ShowAlertSafe("", "Error verifying One-Time-Pin (OTP).");
                await AppSession.SetLOGIN_SUIDX("");
                await AppSession.SetAPP_UID("");
                await AppSession.SetLOGIN_SOTP_SESSIONIDAsync("");
                SecureStorage.Remove("LOGIN_SOTP_SESSIONID");
                ShowSenderLogin(true);
                return;
            }

            if (ci.Status != 0)
            {
                await ShowAlertSafe("", ci.Message);
                if (ci.Status == -2) //expired
                {
                    await AppSession.SetLOGIN_SUIDX("");
                    await AppSession.SetAPP_UID("");
                    await AppSession.SetLOGIN_SOTP_SESSIONIDAsync("");
                    SecureStorage.Remove("LOGIN_SOTP_SESSIONID");
                    ShowSenderLogin(true);
                }
                return;
            }

            //SUCCESS
            await AppSession.SetLOGIN_SUIDX("");
            await AppSession.SetAPP_UID("");
            await AppSession.SetLOGIN_SOTP_SESSIONIDAsync("");
            SecureStorage.Remove("LOGIN_SOTP_SESSIONID");

            if (ci.AccountType == TAccountType.atPrePaid)
            {
                PrePaidBalance = await Task.Run(async () =>
                {
                    return await xs.GetPrePaidBalanceAsync(ci.Web_UID);
                });
            }

            if (ci.CAIDX > 0)
            {
                XDelServiceRef.AddressBook searchedAddress = await Task.Run(async () =>
                {
                    return await xs.GetAddressesAsync(ci.Web_UID, ci.CAIDX, "");
                });
                if (searchedAddress != null && searchedAddress.AddressList != null && searchedAddress.AddressList.Length > 0)
                {
                    for (int i = 0; i <= searchedAddress.AddressList.Length - 1; i++)
                    {
                        if (searchedAddress.AddressList[i].IDX == ci.CAIDX)
                        {
                            defAddress = searchedAddress.AddressList[i];
                            break;
                        }
                    }
                }
            }

            csi = await xs.GetClientSettingsAsync(ci.Web_UID);
            CNSettingsInfo = await xs.GetContactSettingsAsync(ci.Web_UID, ci.CNIDX);

            loginInfo.clientInfo = ci;
            loginInfo.PrePaidBalance = PrePaidBalance;
            loginInfo.defAddress = defAddress;
            loginInfo.ClientXDelOnlineSettings = csi?.XDelOnlineSettings;
            loginInfo.ContactLvlSettingsInfo = CNSettingsInfo;
            loginInfo.xdelOnlineSettings = loginInfo.ContactLvlSettingsInfo != null ? loginInfo.ContactLvlSettingsInfo.XDelOnlineSettings : null;

            AppSession.SetLoginInfo(loginInfo);

            await AppSession.SetLoginModeAsync("s");
            await Shell.Current.GoToAsync("///CardShellPage", new Dictionary<string, object>
                {
                    { "BARCODE", null },
                    { "LOGIN", "Y" },
                    { "DEFAULTTAB", "Home" },
                });

        }
        catch (Exception e)
        {
            string s = e.Message;
            await closeProgress_dialog();
            //await UiPump.Yield();
            await ShowAlertSafe("", e.Message);
        }
    }

    async void SForgotPwd(object sender, TappedEventArgs e)
    {
        try
        {
            _loadedOnce = false;
            await Shell.Current.GoToAsync("LoginPwdReset");
        }
        catch (Exception ex)
        {
            string s = ex.Message;
        }
    }

    async void RForgotPwd(object sender, TappedEventArgs e)
    {
        try
        {
            _loadedOnce = false;
            await Shell.Current.GoToAsync("LoginForgotPwd");
        }
        catch (Exception ex)
        {
            string s = ex.Message;
        }
    }


    private async Task StartCountdownAsync(
    Label countdownLabel,
    View inputBlockView,
    View showAgainButton,
    View txtOTP,
    CancellationTokenSource cts)
    {
        inputBlockView.InputTransparent = true;
        showAgainButton.IsVisible = false;
        countdownLabel.IsVisible = true;

        await Task.Delay(30);
        await MainThread.InvokeOnMainThreadAsync(() =>
        {
            txtOTP.Focus();
        });

        int seconds = 30;

        try
        {
            while (seconds >= 0 && !cts.Token.IsCancellationRequested)
            {
                countdownLabel.Text = $"{seconds}s";
                await Task.Delay(1000, cts.Token);
                seconds--;
            }
        }
        catch (TaskCanceledException)
        {
            // expected when stopping timer
        }

        // finished normally
        if (!cts.Token.IsCancellationRequested)
        {
            showAgainButton.IsVisible = true;
            countdownLabel.IsVisible = false;
            inputBlockView.InputTransparent = false;
            cts.Cancel();
        }
    }

    private void StopMCountdown()
    {
        if (_ctsM is { IsCancellationRequested: false })
            _ctsM.Cancel();

        btnMobileOTP.IsVisible = true;
        lblMCountdown.IsVisible = false;
        txtMobile.InputTransparent = false;
    }

    private void StopECountdown()
    {
        if (_ctsE is { IsCancellationRequested: false })
            _ctsE.Cancel();

        btnEmailOTP.IsVisible = true;
        lblECountdown.IsVisible = false;
        txtEmail.InputTransparent = false;
    }


    private async Task showProgress_Dialog(string msg)
    {
        try
        {
            //await MainThread.InvokeOnMainThreadAsync(async () =>
            //{
            //    await _progressService.ShowAsync(msg);
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

    private Task ShowAlertSafe(string title, string message, string button = "OK")
    {
        if (MainThread.IsMainThread)
            return DisplayAlertAsync(title, message, button);

        return MainThread.InvokeOnMainThreadAsync(() =>
            DisplayAlertAsync(title, message, button));
    }

}
