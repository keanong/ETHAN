using ETHAN.classes;
using ETHAN.ProgressDialog;
using Microsoft.Maui.ApplicationModel; // for AppInfo
using System.Text.RegularExpressions;
using XDelServiceRef;

namespace ETHAN.Views;

public partial class Login : ContentPage
{
    private XOEWSSoapClient xs = new XOEWSSoapClient(XOEWSSoapClient.EndpointConfiguration.XOEWSSoap);
    //private ProgressDialogService_ _progressService;
    private readonly IProgressDialogService _progressService;
    private bool _loadedOnce = false;

    private CancellationTokenSource? _ctsM;
    private CancellationTokenSource? _ctsE;

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
            lblMobileEmail.Text = "e";

            LblSender.TextColor = (Color)Application.Current.Resources["XDelOrange"];
            IndicatorSender.IsVisible = true;

            LblReceiver.TextColor = (Color)Application.Current.Resources["Gray600"];
            IndicatorReceiver.IsVisible = false;

            SenderPanel.IsVisible = true;
            ReceiverPanel.IsVisible = false;

            setSUsernameRuleStatus(true, true);
            setSPwdRuleStatus(true, true);
            EmailMobile_ResetFields();
            ValidateSenderLoginButton();
            ValidateReceiverLoginBtnNew();

            txtSUsername.Text = "";
            txtSPassword.Text = "";
            txtSPassword.IsPassword = true;
            btnSTogglePwd.Source = "eye_show_80.png";

            txtEmailMobile.Text = "";
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

    /*private async Task StartOtpUiUpdateAsync()
    {
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
    }*/

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
        try
        {
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
        //Application.Current.Quit();
        _ = HideOtpOrQuit();
        return true;
    }

    private void DisableTabs(bool enable)
    {
        try
        {
            LblSender.InputTransparent = enable;
            LblReceiver.InputTransparent = enable;
        } catch (Exception e)
        {
            string s = e.Message;
        }
    }

    private async Task HideOtpOrQuit()
    {
        bool GSv = GSOTP.IsVisible;
        /*bool GMv = GMOTP.IsVisible;
        bool GEv = GEOTP.IsVisible;*/
        bool GOv = GOTP.IsVisible;
        try
        {
            //if (GSv || GMv || GEv || GOv)
            if (GSv || GOv)
            {
                if (GSv)
                {
                    await AppSession.SetLOGIN_SUIDX("");
                    await AppSession.SetAPP_UID("");
                    await AppSession.SetLOGIN_SOTP_SESSIONIDAsync("");
                    ShowSenderLogin(true);
                } else
                {
                    await AppSession.SetLOGIN_EOTP_SESSIONIDAsync("");
                    await AppSession.SetLOGIN_MOTP_SESSIONIDAsync("");

                    SecureStorage.Remove("LOGIN_EUIDX");
                    SecureStorage.Remove("LOGIN_MOTP_SESSIONID");
                    SecureStorage.Remove("LOGIN_MOTP_VERIFIED");
                    SecureStorage.Remove("LOGIN_EOTP_SESSIONID");
                    SecureStorage.Remove("LOGIN_EOTP_VERIFIED");

                    await AppSession.SetLOGIN_EUIDX("");
                    await AppSession.SetTEMP_UID("");
                    await AppSession.SetLOGIN_MOTP_SESSIONIDAsync("");
                    ShowReceiverLoginNew(true);
                }
            } else
            {
                Application.Current.Quit();
            }
        } catch (Exception e)
        {
            string s = e.Message;
        }
    }

    private async void CXOtp_Click(object sender, EventArgs e)
    {
        try
        {
            DisableTabs(false);
            HideOtpOrQuit();
        } catch (Exception ex)
        {
            string s = ex.Message;
        }
        
    }

    private async void ReceiverLoginBtn_Click(object sender, EventArgs e)
    {
        string me = lblMobileEmail.Text;
        try
        {
            await EUAuthNew();
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
            SenderLoginBtn.InputTransparent = false;

            if (ci != null && ci.Status == 0)
            {
                txtSUsername.Text = "";
                txtSPassword.Text = "";
                GSOTP.IsVisible = true;
                DisableTabs(true);
                ShowSenderLogin(false);
                AppSession.SetLoginInfo(null);
            }
            else if ((ci != null && ci.Status != 0) || ci == null)
            {
                AppSession.SetLoginInfo(null);
                txtSUsername.Text = "";
                txtSPassword.Text = "";
                GSOTP.IsVisible = false;
                DisableTabs(false);
                ValidateSenderLoginButton();
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
        }
    }

    private async Task EUAuthNew()
    {
        ClientInfo? ci = null;
        XDelServiceRef.XWSBase xb = new XDelServiceRef.XWSBase();
        string me = lblMobileEmail.Text;
        string txt = txtEmailMobile.Text;
        try
        {
            ReceiverLoginBtn.InputTransparent = true;
            await showProgress_Dialog("Processing...");

            ci = await xs.XOE_Pre_AuthenticateXOEAsync(
                me.Equals("m") ? txt : "",
                me.Equals("e") ? txt : "",
                txtRPassword.Text.Trim().ToString());

            if (ci != null && ci.Status == 0 && ci.CNIDX > 0)
            {
                Guid sessionid = Guid.Empty;
                Guid pending_sessionid = Guid.Empty;

                pending_sessionid = await xs.XOE_Get_Pending_2FAAsync(ci.Web_UID, 0, ci.CNIDX, "", "");
                if (pending_sessionid != Guid.Empty)
                    xb = await xs.XOE_Update_2FA_Status_By_SessionIDAsync(ci.Web_UID, 0, ci.CNIDX, pending_sessionid, 1);

                if (xb.Status == 0)
                {

                    if (!String.IsNullOrEmpty(me))
                    {
                        sessionid = await xs.XOE_Request_2FAAsync(ci.Web_UID, 0, ci.CNIDX,
                            me.Equals("m") ? txt.Trim().ToString() : "",
                            me.Equals("e") ? txt.Trim().ToString() : "");

                        if (sessionid != Guid.Empty)
                        {
                            await AppSession.SetLOGIN_EUIDX(ci.CNIDX.ToString());
                            await AppSession.SetTEMP_UID(ci.Web_UID);
                            await AppSession.SetLOGIN_SOTP_SESSIONIDAsync(sessionid.ToString());
                        } else
                        {
                            ci.Status = -11;
                            ci.Message = "Unable to request One-Time-Pin (OTP). Please login again.";
                        }
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
            ReceiverLoginBtn.InputTransparent = false;

            if (ci != null && ci.Status == 0)
            {
                txtSUsername.Text = "";
                txtSPassword.Text = "";
                txtEmailMobile.Text = "";
                GSOTP.IsVisible = false;
                GOTP.IsVisible = true;
                txtOTP.Placeholder = me.Equals("m") ? "Enter OTP sent via SMS" : me.Equals("e") ? "Enter OTP sent via Email" : "";
                DisableTabs(true);
                ShowReceiverLoginNew(false);

                AppSession.SetLoginInfo(null);
            }
            else if ((ci != null && ci.Status != 0) || ci == null)
            {
                AppSession.SetLoginInfo(null);
                txtSUsername.Text = "";
                txtSPassword.Text = "";
                txtRPassword.Text = "";
                GSOTP.IsVisible = false;
                GOTP.IsVisible = false;
                DisableTabs(false);
                ValidateReceiverLoginBtnNew();
                hsSPwd.IsVisible = false;
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
            await ShowAlertSafe("", e.Message);
        }
    }

    //// Track visibility per field
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
        lblMobileEmail.Text = "e";

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

        //lblSUsernameInvalid.IsVisible = true;
        hsSUsername.IsVisible = false;
        //lblSPasswordInvalid.IsVisible = true;
        hsSPwd.IsVisible = false;
        SenderLoginBtn.InputTransparent = false;

        //MobileTab_ResetFields();
        EmailMobile_ResetFields();
    }

    private async void ReceiverTab_Tapped(object sender, TappedEventArgs e)
    {
        string SenderReceiver = lblSenderReceiver.Text;
        if (!string.IsNullOrEmpty(SenderReceiver) && SenderReceiver.Equals("r"))
            return;

        lblSenderReceiver.Text = "r";
        lblMobileEmail.Text = "e";

        txtSUsername.Text = "";
        txtSPassword.Text = "";
        txtSPassword.IsPassword = true;
        btnSTogglePwd.Source = "eye_show_80.png";
        ReceiverLoginBtn.InputTransparent = false;

        EmailMobile_ResetFields();

        LblSender.TextColor = (Color)Application.Current.Resources["Gray600"];
        IndicatorSender.IsVisible = false;

        LblReceiver.TextColor = (Color)Application.Current.Resources["XDelOrange"];
        IndicatorReceiver.IsVisible = true;

        SenderPanel.IsVisible = false;
        ReceiverPanel.IsVisible = true;
    }

    private async void EmailMobile_ResetFields()
    {
        await AppSession.SetLOGIN_MOTP_SESSIONIDAsync("");
        await AppSession.SetLOGIN_EOTP_SESSIONIDAsync("");
        SecureStorage.Remove("LOGIN_EUIDX");
        SecureStorage.Remove("LOGIN_MOTP_SESSIONID");
        SecureStorage.Remove("LOGIN_MOTP_VERIFIED");
        SecureStorage.Remove("LOGIN_EOTP_SESSIONID");
        SecureStorage.Remove("LOGIN_EOTP_VERIFIED");

        //iconPendingEmailMobile.IsVisible = false;
        txtEmailMobile.InputTransparent = false;

        ShowReceiverLoginNew(true);
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
            lblRRegisterS.IsVisible = show;
            SenderLoginBtn.IsVisible = show;
            SenderLoginBtn.InputTransparent = !show;

            if (show)
            {
                //lblSUsernameInvalid.IsVisible = true;
                hsSUsername.IsVisible = false;
                //lblSPasswordInvalid.IsVisible = true;
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

    void ShowReceiverLoginNew(bool show)
    {
        try
        {
            if (show)
                lblMobileEmail.Text = "e";
            txtRPassword.Text = "";
            txtRPassword.IsPassword = true;

            btnRTogglePwd.Source = "eye_show_80.png";

            ReceiverLoginBtn.IsEnabled = false;
            ReceiverLoginBtn.Style = (Style)Application.Current.Resources["bstyleDisabled"];

            gbEmailMobile.IsVisible = show;
            txtEmailMobile.Text = "";
            txtOTP.Text = "";
            GRPassword.IsVisible = show;

            lblRForgotPassword.IsVisible = show;
            lblRRegister.IsVisible = show;
            ReceiverLoginBtn.IsVisible = show;
            ReceiverLoginBtn.InputTransparent = !show;

            if (show)
            {
                //lblEmailMobileInvalid.IsVisible = true;
                hsEmailMobile.IsVisible = false;
                lblRForgotPassword.IsVisible = true;
                hsPwd.IsVisible = false;
                lblRPasswordInvalid.IsVisible = true;
                GOTP.IsVisible = false;
            }
        }
        catch (Exception e)
        {
            ReceiverLoginBtn.InputTransparent = false;
            string s = e.Message;
        }
    }

    void ValidateReceiverLoginBtnNew()
    {
        string me = lblMobileEmail.Text;
        string txt = txtEmailMobile.Text;
        bool ok = false;
        try
        {

            if (!string.IsNullOrEmpty(me) && me.Equals("m"))
            {
                bool hasMobile = !string.IsNullOrWhiteSpace(txt) && IsDigitOnly(txt) && IsValidMobile(txt);
                bool hasSPassword = !string.IsNullOrWhiteSpace(txtRPassword.Text);
                var stored = AppSession.LOGIN_MOTP_SESSIONID;
                bool hasPMOTP = !string.IsNullOrWhiteSpace(stored);

                ok = !hasPMOTP && hasMobile && hasSPassword && mobileOk && pwdOk && minEightOk && lowerOk && upperOk && numOk;

                ReceiverLoginBtn.IsEnabled = ok;

                ReceiverLoginBtn.Style = ok ? (Style)Application.Current.Resources["bstyleOrange"] : (Style)Application.Current.Resources["bstyleDisabled"];
            }
            if (!string.IsNullOrEmpty(me) && me.Equals("e"))
            {
                bool hasEmail = !string.IsNullOrWhiteSpace(txt) && !IsDigitOnly(txt) && IsValidEmail(txt);
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
            hsSPwd.IsVisible = (!isOnAppearing && !isValid);
        }
        catch (Exception e)
        {
            string s = e.Message;
        }
    }

    void onEmailMobileFieldChanged(object sender, TextChangedEventArgs e)
    {
        try
        {
            var entry = (Entry)sender;
            string txt = txtEmailMobile.Text;
            if (String.IsNullOrEmpty(txt))
            {
                mobileOk = false;
                emailOk = false;
                lblMobileEmail.Text = "e";
                entry.MaxLength = int.MaxValue;
                setEmailMobileRuleStatus(false, false);
            }
            else
            {
                bool digitsOnly = IsDigitOnly(txt);
                entry.MaxLength = digitsOnly ? 8 : int.MaxValue;
                lblMobileEmail.Text = digitsOnly ? "m" : "e";
                bool valid = digitsOnly ? IsValidMobile(txt) : IsValidEmail(txt);
                if (digitsOnly)
                    mobileOk = IsValidMobile(txt);
                else
                    emailOk = IsValidEmail(txt);

                setEmailMobileRuleStatus(valid, false);
                ValidateReceiverLoginBtnNew();
            }
        } catch (Exception ex)
        {
            string s = ex.Message;
        }
    }

    void setEmailMobileRuleStatus(bool isValid, bool isOnAppearing)
    {
        try
        {
            hsEmailMobile.IsVisible = (!isOnAppearing && !isValid);
        }
        catch (Exception e)
        {
            string s = e.Message;
        }
    }

    bool IsDigitOnly(string mobile)
    {
        if (string.IsNullOrWhiteSpace(mobile))
            return false;

        // must be 8 digits starting with 8 or 9
        return mobile.All(char.IsDigit);
    }

    bool IsValidMobile(string mobile)
    {
        if (string.IsNullOrWhiteSpace(mobile))
            return false;

        // must be 8 digits starting with 8 or 9
        return Regex.IsMatch(mobile, @"^[89][0-9]{7}$");
    }

    bool IsValidEmail(string email)
    {
        if (string.IsNullOrEmpty(email))
            return false;

        return Regex.IsMatch(email,
                @"^[A-Za-z0-9._%+-]+@[A-Za-z0-9.-]+\.[A-Za-z]{2,}$");
    }

    void OnPwdFieldChanged(object sender, TextChangedEventArgs e)
    {
        ValidateRules();
        ValidateReceiverLoginBtnNew();
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

    async void IsValidROTP(object sender, EventArgs e)
    {
        try
        {
            string me = lblMobileEmail.Text;
            if (String.IsNullOrEmpty(me))
            {
                await AppSession.SetLOGIN_EUIDX("");
                await AppSession.SetTEMP_UID("");
                await AppSession.SetLOGIN_SOTP_SESSIONIDAsync("");
                await ShowAlertSafe("", "Unable to verify One-Time-Pin (OTP). Please login again.");
                ShowReceiverLoginNew(true);
                return;
            } else  if (string.IsNullOrEmpty(txtOTP.Text))
                await DisplayAlertAsync("",
                    me.Equals("m") ? "Please enter the One-Time-Pin (OTP) sent to your Mobile." : "Please enter the One-Time-Pin (OTP) sent to your Email Address.", 
                    "Ok");
            else
                await ValidateROTP(me);
        }
        catch (Exception ex)
        {
            string s = ex.Message;
        }
    }

    private async Task ValidateROTP(string me)
    {
        Guid sessionId = Guid.Empty;

        try
        {
            var stored = AppSession.LOGIN_SOTP_SESSIONID;
            if (!string.IsNullOrWhiteSpace(stored))
                Guid.TryParse(stored, out sessionId);
        }
        catch { }

        if (sessionId == Guid.Empty)
        {
            await AppSession.SetLOGIN_EUIDX("");
            await AppSession.SetTEMP_UID("");
            await AppSession.SetLOGIN_SOTP_SESSIONIDAsync("");
            await ShowAlertSafe("", "Unable to verify One-Time-Pin (OTP). Session not found.");
            ShowReceiverLoginNew(true);
            return;
        }

        string TEMP_UID = AppSession.TEMP_UID;
        if (string.IsNullOrEmpty(TEMP_UID))
        {
            await AppSession.SetLOGIN_EUIDX("");
            await AppSession.SetTEMP_UID("");
            await AppSession.SetLOGIN_SOTP_SESSIONIDAsync("");
            await ShowAlertSafe("", "Unable to verify One-Time-Pin (OTP). TEMP_UID not found.");
            ShowReceiverLoginNew(true);
            return;
        }

        string _eridx = AppSession.LOGIN_EUIDX;
        if (string.IsNullOrEmpty(_eridx))
        {
            await AppSession.SetLOGIN_EUIDX("");
            await AppSession.SetTEMP_UID("");
            await AppSession.SetLOGIN_SOTP_SESSIONIDAsync("");
            await ShowAlertSafe("", "Unable to verify One-Time-Pin (OTP). _eridx not found.");
            ShowReceiverLoginNew(true);
            return;
        }
        long eridx = long.Parse(_eridx);

        ClientInfo? ci = null;
        DecimalReturn? PrePaidBalance = null;
        XDelServiceRef.AddressStructure? defAddress = null;
        XDelServiceRef.SettingsInfo csi = null;
        XDelServiceRef.SettingsInfo CNSettingsInfo = null;
        LoginInfo loginInfo = new LoginInfo();
        XOE_ETHAN_Receiver ETHAN_Receiver = null;

        try
        {
            await showProgress_Dialog("Verifying...");

            ci = await xs.XOE_Verify_OTP_Async(TEMP_UID, 0, eridx, sessionId, txtOTP.Text);

            await closeProgress_dialog();

            if (ci == null)
            {
                await AppSession.SetLOGIN_EUIDX("");
                await AppSession.SetTEMP_UID("");
                await AppSession.SetLOGIN_SOTP_SESSIONIDAsync("");
                await ShowAlertSafe("", "Error verifying One-Time-Pin (OTP).");
                ShowReceiverLoginNew(true);
                return;
            }

            if (ci.Status != 0)
            {
                await ShowAlertSafe("", ci.Message);
                if (ci.Status == -2) ////expired
                {
                    await AppSession.SetLOGIN_EUIDX("");
                    await AppSession.SetTEMP_UID("");
                    await AppSession.SetLOGIN_SOTP_SESSIONIDAsync("");
                    ShowReceiverLoginNew(true);
                }
                return;
            }

            ////SUCCESS
            await SecureStorage.SetAsync("LOGIN_EOTP_VERIFIED", "t");
            await AppSession.SetLOGIN_EUIDX("");
            await AppSession.SetTEMP_UID("");
            await AppSession.SetLOGIN_SOTP_SESSIONIDAsync("");

            ETHAN_Receiver = await xs.XOE_Get_ETHAN_ReceiverAsync(ci.Web_UID);

            loginInfo.clientInfo = ci;
            loginInfo.ETHAN_Receiver = ETHAN_Receiver;

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

            ci = await xs.XOE_Verify_OTP_Async(APP_UID, eridx, 0, sessionId, txtSenderOTP.Text);

            await closeProgress_dialog();

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
                if (ci.Status == -2) ////expired
                {
                    await AppSession.SetLOGIN_SUIDX("");
                    await AppSession.SetAPP_UID("");
                    await AppSession.SetLOGIN_SOTP_SESSIONIDAsync("");
                    SecureStorage.Remove("LOGIN_SOTP_SESSIONID");
                    ShowSenderLogin(true);
                }
                return;
            }

            ////SUCCESS
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
