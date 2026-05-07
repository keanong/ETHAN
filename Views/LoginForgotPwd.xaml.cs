
using ETHAN.classes;
using XDelServiceRef;
using ETHAN.ProgressDialog;
using System.Security.Cryptography;
using System.Text;
using System;
using System.ComponentModel.DataAnnotations;
using System.Text.RegularExpressions;
using CommunityToolkit.Mvvm.Messaging;

namespace ETHAN.Views;

public partial class LoginForgotPwd : ContentPage, IRecipient<AppSleepMessage>
{
    //private XWSSoapClient xs = new XWSSoapClient(XWSSoapClient.EndpointConfiguration.XWSSoap);
    private XOEWSSoapClient xs = new XOEWSSoapClient(XOEWSSoapClient.EndpointConfiguration.XOEWSSoap);
    //private ProgressDialogService_ _progressService;
    private readonly IProgressDialogService _progressService;
    private bool _loadedOnce = false;

    private CancellationTokenSource? _ctsM;
    private CancellationTokenSource? _ctsE;


    public LoginForgotPwd(IProgressDialogService progressService)
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

                LblMobile.TextColor = (Color)Application.Current.Resources["XDelOrange"];
                IndicatorMobile.IsVisible = true;

                LblEmail.TextColor = (Color)Application.Current.Resources["Gray600"];
                IndicatorEmail.IsVisible = false;

                await MobileTab_ResetFields();

                setMobileRuleStatus(true, true);
                setEmailRuleStatus(true, true);

                SetRuleStatus(false, iconMinEight, lblminEight);
                SetRuleStatus(false, iconLowerC, lblLowerC);
                SetRuleStatus(false, iconUpperC, lblUpperC);
                SetRuleStatus(false, iconNum, lblNum);

                ValidateButton();

                iconPwdMatch.Text = "\u2716";
                iconPwdMatch.Style = (Style)Application.Current!.Resources["textStyle16Red"];
                lblPwdMatch.Text = "Please enter and confirm password";
                lblPwdMatch.Style = (Style)Application.Current!.Resources["textStyle16Red"];

                /*Dispatcher.Dispatch(async () =>
                {
                    try
                    {
                        await StartOtpUiUpdateAsync();
                    }
                    catch (Exception ex)
                    {
                        System.Diagnostics.Debug.WriteLine(ex);
                    }
                });*/

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
        //await Task.Delay(250); // allow Shell/UI to settle
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

    protected override void OnDisappearing()
    {
        base.OnDisappearing();
        if (_progressService != null && _progressService.IsShowing)
            return;

        StopMCountdown();
        StopECountdown();

        WeakReferenceMessenger.Default.Unregister<AppSleepMessage>(this);
    }

    public void Receive(AppSleepMessage message)
    {
        if (message.Value)  // true = app backgrounded
        {
            StopMCountdown();
            StopECountdown();
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
            await Shell.Current.GoToAsync("//Login");
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

    private async void MobileTab_Tapped(object sender, TappedEventArgs e)
    {
        await MobileTab_ResetFields();
    }

    private async Task MobileTab_ResetFields()
    {
        /*SecureStorage.Remove("FORGOT_EUIDX");
        SecureStorage.Remove("FORGOT_EOTP_SESSIONID");
        SecureStorage.Remove("FORGOT_EOTP_VERIFIED");*/
        await AppSession.SetFORGOT_EUIDX("");
        await AppSession.SetFORGOT_EOTP_SESSIONIDAsync("");
        await AppSession.SetFORGOT_EOTP_VERIFIED("");

        lblMobileEmail.Text = "m";

        LblMobile.TextColor = (Color)Application.Current.Resources["XDelOrange"];
        IndicatorMobile.IsVisible = true;

        LblEmail.TextColor = (Color)Application.Current.Resources["Gray600"];
        IndicatorEmail.IsVisible = false;


        gbMobile.IsVisible = true;
        gbEmail.IsVisible = false;

        txtPwd.Text = "";
        txtPwd.IsPassword = true;
        btnTogglePwd.Source = "eye_show_80.png";
        txtCfmPwd.Text = "";
        txtCfmPwd.IsPassword = true;
        btnToggleCfmPwd.Source = "eye_show_80.png";

        lblECountdown.IsVisible = false;
        txtEmail.InputTransparent = false;
        GEOTP.IsVisible = false;
        btnEmailOTP.IsVisible = false;
        iconVerifiedEmail.IsVisible = false;
        iconPendingEmail.IsVisible = false;
        hsEmail.IsVisible = false;
        lblEmailInvalid.IsVisible = false;

        if (_ctsM is { IsCancellationRequested: false })
        {
            lblMCountdown.IsVisible = true;
            txtMobile.InputTransparent = true;
            GMOTP.IsVisible = true;
            btnMobileOTP.IsVisible = false;
            iconVerifiedMobile.IsVisible = false;
            hsMobile.IsVisible = false;
            lblMobileInvalid.IsVisible = false;
        } else
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

    private async void EmailTab_Tapped(object sender, TappedEventArgs e)
    {
        await EmailTab_ResetFields();
    }

    private async Task EmailTab_ResetFields()
    {
        /*SecureStorage.Remove("FORGOT_EUIDX");
        SecureStorage.Remove("FORGOT_MOTP_SESSIONID");
        SecureStorage.Remove("FORGOT_MOTP_VERIFIED");*/
        await AppSession.SetFORGOT_EUIDX("");
        await AppSession.SetFORGOT_MOTP_SESSIONIDAsync("");
        await AppSession.SetFORGOT_MOTP_VERIFIED("");

        lblMobileEmail.Text = "e";

        LblMobile.TextColor = (Color)Application.Current.Resources["Gray600"];
        IndicatorMobile.IsVisible = false;

        LblEmail.TextColor = (Color)Application.Current.Resources["XDelOrange"];
        IndicatorEmail.IsVisible = true;


        gbMobile.IsVisible = false;
        gbEmail.IsVisible = true;

        txtPwd.Text = "";
        txtPwd.IsPassword = true;
        btnTogglePwd.Source = "eye_show_80.png";
        txtCfmPwd.Text = "";
        txtCfmPwd.IsPassword = true;
        btnToggleCfmPwd.Source = "eye_show_80.png";

        lblMCountdown.IsVisible = false;
        txtMobile.InputTransparent = false;
        GMOTP.IsVisible = false;
        btnMobileOTP.IsVisible = false;
        iconVerifiedMobile.IsVisible = false;
        iconPendingMobile.IsVisible = false;
        hsMobile.IsVisible = false;
        lblMobileInvalid.IsVisible = false;

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
    bool minEightOk = false;
    bool lowerOk = false;
    bool upperOk = false;
    bool numOk = false;
    bool matchOk = false;

    void OnPwdFieldChanged(object sender, TextChangedEventArgs e)
    {
        ValidateRules();
        ValidateMatch();
        ValidateButton();
    }

    void ValidateRules()
    {
        try
        {
            string pwd = txtPwd.Text ?? "";

            minEightOk = pwd.Length >= 8;
            lowerOk = pwd.Any(char.IsLower);
            upperOk = pwd.Any(char.IsUpper);
            numOk = pwd.Any(char.IsDigit);

            // Minimum 8 chars
            SetRuleStatus(minEightOk, iconMinEight, lblminEight);

            // Lowercase
            SetRuleStatus(lowerOk, iconLowerC, lblLowerC);

            // Uppercase
            SetRuleStatus(upperOk, iconUpperC, lblUpperC);

            // Number
            SetRuleStatus(numOk, iconNum, lblNum);
        }
        catch (Exception e)
        {
            string s = e.Message;
        }
    }

    void ValidateMatch()
    {
        try
        {
            string Pwd = txtPwd.Text ?? "";
            string cfmPwd = txtCfmPwd.Text ?? "";

            if (string.IsNullOrWhiteSpace(Pwd) &&
                string.IsNullOrWhiteSpace(cfmPwd))
            {
                matchOk = false;

                iconPwdMatch.Text = "\u2716";
                iconPwdMatch.Style = (Style)Application.Current!.Resources["textStyle16Red"];
                lblPwdMatch.Text = "Please enter and confirm password";
                lblPwdMatch.Style = (Style)Application.Current!.Resources["textStyle16Red"];
                return;
            }

            if (Pwd != cfmPwd)
            {
                matchOk = false;

                iconPwdMatch.Text = "\u2716";
                iconPwdMatch.Style = (Style)Application.Current!.Resources["textStyle16Red"];
                lblPwdMatch.Text = "Password does not match.";
                lblPwdMatch.Style = (Style)Application.Current!.Resources["textStyle16Red"];
                return;
            }

            // Match
            matchOk = true;

            iconPwdMatch.Text = "\u2714";
            iconPwdMatch.Style = (Style)Application.Current!.Resources["textStyle16Green"];
            lblPwdMatch.Text = "Password matches.";
            lblPwdMatch.Style = (Style)Application.Current!.Resources["textStyle16Green"];
        }
        catch (Exception e)
        {
            string s = e.Message;
        }
    }

    void SetRuleStatus(bool isValid, Label icon, Label text)
    {
        try
        {
            if (isValid)
            {
                icon.Text = "\u2714";
                icon.Style = (Style)Application.Current!.Resources["textStyle16Green"];
                text.Style = (Style)Application.Current!.Resources["textStyle16Green"];
            }
            else
            {
                icon.Text = "\u2716";
                icon.Style = (Style)Application.Current!.Resources["textStyle16Red"];
                text.Style = (Style)Application.Current!.Resources["textStyle16Red"];
            }
        }
        catch (Exception e)
        {
            string s = e.Message;
        }
    }

    void onMobileFieldChanged(object sender, TextChangedEventArgs e)
    {
        ValidateMobileRule();
        ValidateRules();
        ValidateMatch();
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
            //hsMobile.IsVisible = !isValid;
            lblMobileInvalid.IsVisible = (isOnAppearing && isValid);
            hsMobile.IsVisible = (!isOnAppearing && !isValid);
            btnMobileOTP.IsVisible = (!isOnAppearing && isValid);
            //GMOTP.IsVisible = (!isOnAppearing && isValid);
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
            //var value = await SecureStorage.GetAsync("FORGOT_MOTP_SESSIONID");
            var value = AppSession.FORGOT_MOTP_SESSIONID;

            if (!string.IsNullOrWhiteSpace(value) && Guid.TryParse(value, out var parsed))
            {
                stored_MOTP_SESSIONID = parsed;
            }
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
            //var value = await SecureStorage.GetAsync("FORGOT_EOTP_SESSIONID");
            var value = AppSession.FORGOT_EOTP_SESSIONID;

            if (!string.IsNullOrWhiteSpace(value) && Guid.TryParse(value, out var parsed))
            {
                stored_EOTP_SESSIONID = parsed;
            }
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
        ValidateMatch();
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
            //hsEmail.IsVisible = !isValid;
            lblEmailInvalid.IsVisible = (isOnAppearing && isValid);
            hsEmail.IsVisible = (!isOnAppearing && !isValid);
            btnEmailOTP.IsVisible = (!isOnAppearing && isValid);
            //GEOTP.IsVisible = (!isOnAppearing && isValid);
        }
        catch (Exception e)
        {
            string s = e.Message;
        }
    }

    void ValidateButton()
    {
        string me = lblMobileEmail.Text;
        bool ok = false;
        try
        {
            bool hasMobile = !string.IsNullOrWhiteSpace(txtMobile.Text);
            bool hasEmail = !string.IsNullOrWhiteSpace(txtEmail.Text);
            bool hasPwd = !string.IsNullOrWhiteSpace(txtPwd.Text);
            bool hasCfm = !string.IsNullOrWhiteSpace(txtCfmPwd.Text);
            bool hasVMOTP = iconVerifiedMobile.IsVisible;
            bool hasVEOTP = iconVerifiedEmail.IsVisible;

            
            if (!string.IsNullOrEmpty(me) && me.Equals("m"))
                ok = hasMobile && hasVMOTP && mobileOk && hasPwd && hasCfm && minEightOk && lowerOk && upperOk && numOk && matchOk;
            else
                ok = hasEmail && hasVEOTP && emailOk && hasPwd && hasCfm && minEightOk && lowerOk && upperOk && numOk && matchOk;

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

        //return new EmailAddressAttribute().IsValid(email);

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
        //XDelServiceRef.XWSBase x = new XWSBase();
        XOE_ETHAN_Receiver x = new XOE_ETHAN_Receiver();
        string MOBILE = txtMobile.Text.Trim().ToString();
        try
        {

            await showProgress_Dialog("Processing...");
            //await UiPump.Yield();

            x = await xs.XOE_Get_ETHAN_ReceiverIDX_By_MobileEmailAsync(MOBILE, "");
            if (x == null || x.Status != 0)
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

            if (string.IsNullOrEmpty(TEMP_UID))
            {
                await closeProgress_dialog();
                //await UiPump.Yield();
                await ShowAlertSafe("", "Error requesting One-Time-Pin (OTP).\nTEMP_UID not found.");
                return;
            }

            sessionid = await xs.XOE_Request_2FAAsync(TEMP_UID, 0, eridx, MOBILE, "");
            if (sessionid == Guid.Empty)
            {
                await closeProgress_dialog();
                //await UiPump.Yield();
                await ShowAlertSafe("", "Error requesting One-Time-Pin (OTP). Please try again.");
                return;
            }

            await AppSession.SetTEMP_UID(TEMP_UID);
            await AppSession.SetFORGOT_EUIDX(eridx.ToString());
            await AppSession.SetFORGOT_MOTP_SESSIONIDAsync(sessionid.ToString());
            //await SecureStorage.SetAsync("FORGOT_EUIDX", eridx.ToString());
            //await SecureStorage.SetAsync("FORGOT_MOTP_SESSIONID", sessionid.ToString());

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
            if (x == null || x.Status != 0)
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

            if (string.IsNullOrEmpty(TEMP_UID))
            {
                await closeProgress_dialog();
                //await UiPump.Yield();
                await ShowAlertSafe("", "Error requesting One-Time-Pin (OTP).\nTEMP_UID not found.");
                return;
            }

            sessionid = await xs.XOE_Request_2FAAsync(TEMP_UID, 0, eridx, "", EMAIL);
            if (sessionid == Guid.Empty)
            {
                await closeProgress_dialog();
                //await UiPump.Yield();
                await ShowAlertSafe("", "Error requesting One-Time-Pin (OTP). Please try again.");
                return;
            }

            await AppSession.SetTEMP_UID(TEMP_UID);
            await AppSession.SetFORGOT_EUIDX(eridx.ToString());
            await AppSession.SetFORGOT_EOTP_SESSIONIDAsync(sessionid.ToString());
            //await SecureStorage.SetAsync("FORGOT_EUIDX", eridx.ToString());
            //await SecureStorage.SetAsync("FORGOT_EOTP_SESSIONID", sessionid.ToString());

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
            //var stored = await SecureStorage.GetAsync("FORGOT_MOTP_SESSIONID");
            var stored = AppSession.FORGOT_MOTP_SESSIONID;
            if (!string.IsNullOrWhiteSpace(stored))
                Guid.TryParse(stored, out sessionId);
        }
        catch { }

        if (sessionId == Guid.Empty)
        {
            await ShowAlertSafe("", "Unable to verify One-Time-Pin (OTP). Session not found.");
            return;
        }

        //string TEMP_UID = await SecureStorage.GetAsync("TEMP_UID");
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
            //await UiPump.Yield();

            var result = await xs.XOE_Verify_OTPAsync(TEMP_UID, 0, eridx, sessionId, txtMobileOTP.Text);

            await closeProgress_dialog();
            //await UiPump.Yield();

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

            //SUCCESS
            //await SecureStorage.SetAsync("FORGOT_MOTP_VERIFIED", "t");
            //SecureStorage.Remove("FORGOT_MOTP_SESSIONID");
            await AppSession.SetFORGOT_MOTP_VERIFIED("t");
            await AppSession.SetFORGOT_MOTP_SESSIONIDAsync("");

            StopMCountdown();

            iconVerifiedMobile.IsVisible = true;
            iconPendingMobile.IsVisible = false;
            btnMobileOTP.IsVisible = false;
            txtMobileOTP.Text = "";
            txtMobile.InputTransparent = true;
            hsMobile.IsVisible = false;

            ValidateRules();
            ValidateMatch();
            ValidateButton();

            await StartOtpUiUpdateAsync();

            await ShowAlertSafe("", "Mobile Number Verified.");
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
            //var stored = await SecureStorage.GetAsync("FORGOT_EOTP_SESSIONID");
            var stored = AppSession.FORGOT_EOTP_SESSIONID;
            if (!string.IsNullOrWhiteSpace(stored))
                Guid.TryParse(stored, out sessionId);
        }
        catch { }

        if (sessionId == Guid.Empty)
        {
            await AppSession.SetFORGOT_EUIDX("");
            await AppSession.SetTEMP_UID("");
            await AppSession.SetFORGOT_EOTP_SESSIONIDAsync("");
            await ShowAlertSafe("", "Unable to verify One-Time-Pin (OTP). Session not found.");
            //await SecureStorage.SetAsync("FORGOT_EOTP_SESSIONID", "");
            return;
        }

        //string TEMP_UID = await SecureStorage.GetAsync("TEMP_UID");
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
            //await UiPump.Yield();

            var result = await xs.XOE_Verify_OTPAsync(TEMP_UID, 0, eridx, sessionId, txtEmailOTP.Text);

            await closeProgress_dialog();
            //await UiPump.Yield();

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

            //SUCCESS
            //await SecureStorage.SetAsync("FORGOT_EOTP_VERIFIED", "t");
            //SecureStorage.Remove("FORGOT_EOTP_SESSIONID");
            await AppSession.SetFORGOT_EOTP_VERIFIED("t");
            await AppSession.SetFORGOT_EOTP_SESSIONIDAsync("");

            StopECountdown();

            iconVerifiedEmail.IsVisible = true;
            iconPendingEmail.IsVisible = false;
            btnEmailOTP.IsVisible = false;
            txtEmailOTP.Text = "";
            txtEmail.InputTransparent = true;
            hsEmail.IsVisible = false;

            ValidateRules();
            ValidateMatch();
            ValidateButton();

            await StartOtpUiUpdateAsync();
            await ShowAlertSafe("", "Email Address Verified.");
        }
        catch (Exception e)
        {
            string s = e.Message;
            await closeProgress_dialog();
            //await UiPump.Yield();
            await ShowAlertSafe("", e.Message);
        }
    }

    // Track visibility per field
    bool PwdVisible = false;
    bool cfmPwdVisible = false;

    private void OnTogglePwdClicked(object sender, EventArgs e)
    {
        if (sender is ImageButton btn)
        {
            if (btn == btnTogglePwd)
            {
                PwdVisible = !PwdVisible;
                txtPwd.IsPassword = !PwdVisible;
                btnTogglePwd.Source = PwdVisible ? "eye_hide_80.png" : "eye_show_80.png";
            }
            else if (btn == btnToggleCfmPwd)
            {
                cfmPwdVisible = !cfmPwdVisible;
                txtCfmPwd.IsPassword = !cfmPwdVisible;
                btnToggleCfmPwd.Source = cfmPwdVisible ? "eye_hide_80.png" : "eye_show_80.png";
            }
        }
    }

    async void btnUpdate_Click(System.Object sender, System.EventArgs e)
    {
        await UpdatePwd();
    }

    private async Task UpdatePwd()
    {
        string pwd = txtCfmPwd.Text.Trim().ToString();
        XWSBase x = new XWSBase();
        try
        {
            //var stored = await SecureStorage.GetAsync("FORGOT_EUIDX");
            var stored = AppSession.FORGOT_EUIDX;
            if (string.IsNullOrEmpty(stored) || !long.TryParse(stored, out var eridx) || eridx == 0)
            {
                await ShowAlertSafe("", "Error Processing.\nUnable to retrieve registration record.");
                return;
            }

            //string TEMP_UID = await SecureStorage.GetAsync("TEMP_UID");
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
            //await UiPump.Yield();

            x = await xs.XOE_Update_ETHAN_Receiver_PasswordAsync(TEMP_UID, eridx, pwd);
            if (x == null || x.Status != 0)
            {
                await closeProgress_dialog();
                //await UiPump.Yield();
                await ShowAlertSafe("", x?.Message ?? "Error Processing.");
                return;
            }

            await closeProgress_dialog();
            //await UiPump.Yield();

            await ShowAlertSafe("", "Password update completed.");
            await UiPump.Yield();
            await BackToHomePage();
        }
        catch (Exception e)
        {
            string s = e.Message;
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