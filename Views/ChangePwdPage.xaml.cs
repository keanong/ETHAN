
using ETHAN.classes;
using XDelServiceRef;
using ETHAN.ProgressDialog;
using System.Security.Cryptography;
using System.Text;
using System;

namespace ETHAN.Views;

//[QueryProperty(nameof(Vmm), "vmm")] // Add a QueryProperty to handle the navigation parameter
//[QueryProperty(nameof(LOGININFO), "LOGININFO")] // Add a QueryProperty to handle the navigation parameter

public partial class ChangePwdPage : ContentPage
{
    //private XWSSoapClient xs = new XWSSoapClient(XWSSoapClient.EndpointConfiguration.XWSSoap);
    private XOEWSSoapClient xs = new XOEWSSoapClient(XOEWSSoapClient.EndpointConfiguration.XOEWSSoap);
    //private ProgressDialogService _progressService;
    private readonly IProgressDialogService _progressService;

    private LoginInfo? logininfo;
    /*public LoginInfo? LOGININFO
    {
        set
        {
            logininfo = value;
        }
    }*/

    public ChangePwdPage(IProgressDialogService progressService)
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
            SetRuleStatus(false, iconMinEight, lblminEight);
            SetRuleStatus(false, iconLowerC, lblLowerC);
            SetRuleStatus(false, iconUpperC, lblUpperC);
            SetRuleStatus(false, iconNum, lblNum);

            ValidateButton();

            iconPwdMatch.Text = "\u2716";
            iconPwdMatch.Style = (Style)Application.Current!.Resources["textStyle16Red"];
            lblPwdMatch.Text = "Please enter and confirm new password";
            lblPwdMatch.Style = (Style)Application.Current!.Resources["textStyle16Red"];
        } catch (Exception e)
        {
            string s = e.Message;
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

            MainThread.BeginInvokeOnMainThread(async () =>
            {
                BindingContext = null;
                string v = string.Empty;
                /*await Shell.Current.GoToAsync("///CardShellPage", new Dictionary<string, object>
                    {
                        { "LOGININFO", logininfo },
                        { "BARCODE", null },
                        { "DEFAULTTAB", "Settings" },
                    });*/
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
            string pwd = txtNewPwd.Text ?? "";

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
        } catch (Exception e)
        {
            string s = e.Message;
        }
    }

    void ValidateMatch()
    {
        try
        {
            string newPwd = txtNewPwd.Text ?? "";
            string cfmPwd = txtCfmNewPwd.Text ?? "";

            if (string.IsNullOrWhiteSpace(newPwd) &&
                string.IsNullOrWhiteSpace(cfmPwd))
            {
                matchOk = false;

                iconPwdMatch.Text = "\u2716";
                iconPwdMatch.Style = (Style)Application.Current!.Resources["textStyle16Red"];
                lblPwdMatch.Text = "Please enter and confirm new password";
                lblPwdMatch.Style = (Style)Application.Current!.Resources["textStyle16Red"];
                return;
            }

            if (newPwd != cfmPwd)
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
        } catch (Exception e)
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
        } catch (Exception e)
        {
            string s = e.Message;
        }
    }

    void ValidateButton()
    {
        bool ok = false;
        try
        {
            bool hasOld = !string.IsNullOrWhiteSpace(txtOldPwd.Text);
            bool hasNew = !string.IsNullOrWhiteSpace(txtNewPwd.Text);
            bool hasCfm = !string.IsNullOrWhiteSpace(txtCfmNewPwd.Text);

            //btnChangePwd.IsVisible =
            //    hasOld &&
            //    hasNew &&
            //    hasCfm &&
            //    minEightOk &&
            //    lowerOk &&
            //    upperOk &&
            //    numOk &&
            //    matchOk;

            ok = hasOld && hasNew && hasCfm && minEightOk && lowerOk && upperOk && numOk && matchOk;

            btnChangePwd.IsEnabled = ok;
            btnChangePwd.Style = ok ? (Style)Application.Current.Resources["bstyleOrange"] : (Style)Application.Current.Resources["bstyleDisabled"];
        } catch (Exception e)
        {
            string s = e.Message;
        }
    }

    async void btnChangePwd_Click(System.Object sender, System.EventArgs e)
    {
        try
        {
            logininfo = AppSession.logininfo;
            if (logininfo?.clientInfo == null || (logininfo != null && logininfo.clientInfo == null) ||
                (logininfo != null && logininfo.clientInfo != null && string.IsNullOrEmpty(logininfo.clientInfo.Web_UID)))
            {
                await DisplayAlertAsync("Session expired", "Please Login again.", "OK");
                await common.BackToLogin();
                return;
            } else
            {
                await confirmChangePwd();
            }


        } catch (Exception ex)
        {
            string s = ex.Message;
        }
    }

    private async Task confirmChangePwd()
    {
        try
        {
            var currentPage = App.Current?.Windows.FirstOrDefault()?.Page;

            if (currentPage is null)
                return; // or handle gracefully if app not ready
            bool answer = await currentPage.DisplayAlertAsync(
            "Confirmation",
            "Confirm to change password?",
            "OK",
            "Cancel");

            if (answer)
            {
                // User clicked OK
                await ChangePwd();
            }
            else
            {
                // User clicked Cancel — do nothing or close dialog
            }
        }
        catch (Exception e)
        {
            string s = e.Message;
        }
    }

    private async Task pwdChangedSucc()
    {
        try
        {
            var currentPage = App.Current?.Windows.FirstOrDefault()?.Page;

            if (currentPage is null)
                return; // handle gracefully if app not ready

            await currentPage.DisplayAlertAsync(
                "Success",
                "Password changed successfully.",
                "OK" // only one button
            );

            await BackToHomePage();
        }
        catch (Exception e)
        {
            string s = e.Message;
        }
    }

    private async Task ChangePwd()
    {
        try
        {
            logininfo = AppSession.logininfo;
            if (logininfo?.clientInfo == null || (logininfo != null && logininfo.clientInfo == null) ||
                (logininfo != null && logininfo.clientInfo != null && string.IsNullOrEmpty(logininfo.clientInfo.Web_UID)))
            {
                await DisplayAlertAsync("Session expired", "Please Login again.", "OK");
                await common.BackToLogin();
                return;
            }

            string op = txtOldPwd.Text;
            string np = txtNewPwd.Text;


            await showProgress_Dialog("Processing...");

            int c = common.getPwdRestrictLvl(np);
            if (c < 3)
            {
                await Task.Delay(100);
                await closeProgress_dialog();
                await DisplayAlertAsync("", "Password must be at least 8 characters long, contains at least 1 upper case, 1 lower case and a number.", "OK");
                return;
            }

            ClientInfo ci = logininfo!.clientInfo!; //Use the null-forgiving operator (!) since you already checked

            string oldhash = getHash(op);

            XDelServiceRef.XWSBase xb = await Task.Run(() => xs.ChangePasswordAsync(ci.Web_UID, oldhash, np));

            await Task.Delay(100);
            await closeProgress_dialog();

            if (xb != null && xb.Status == 0)
                await pwdChangedSucc();
            else
                await DisplayAlertAsync("", "Old Password is incorrect. Please try again", "OK");
        } catch (Exception e)
        {
            string s = e.Message;
            await Task.Delay(100);
            await closeProgress_dialog();
        }
    }


    private string getHash(string pwd)
    {
        HashAlgorithm hasher = SHA1.Create();
        hasher.Initialize();
        byte[] abyte = ASCIIEncoding.Default.GetBytes(pwd);
        hasher.TransformBlock(abyte, 0, abyte.Length, abyte, 0);
        abyte = ASCIIEncoding.Default.GetBytes("jnksntr*(YjhUB7uygH%Fg8*j(jOIPJuHG^GFBuijniojKJHBYG6FGh^&%^$6598)(");
        hasher.TransformFinalBlock(abyte, 0, abyte.Length);
        return HexEncoding.ToString(hasher.Hash);

    }

    // Track visibility per field
    bool oldPwdVisible = false;
    bool newPwdVisible = false;
    bool cfmPwdVisible = false;

    private void OnTogglePwdClicked(object sender, EventArgs e)
    {
        if (sender is ImageButton btn)
        {
            if (btn == btnToggleOldPwd)
            {
                oldPwdVisible = !oldPwdVisible;
                txtOldPwd.IsPassword = !oldPwdVisible;
                btnToggleOldPwd.Source = oldPwdVisible ? "eye_hide_80.png" : "eye_show_80.png";
            }
            else if (btn == btnToggleNewPwd)
            {
                newPwdVisible = !newPwdVisible;
                txtNewPwd.IsPassword = !newPwdVisible;
                btnToggleNewPwd.Source = newPwdVisible ? "eye_hide_80.png" : "eye_show_80.png";
            }
            else if (btn == btnToggleCfmNewPwd)
            {
                cfmPwdVisible = !cfmPwdVisible;
                txtCfmNewPwd.IsPassword = !cfmPwdVisible;
                btnToggleCfmNewPwd.Source = cfmPwdVisible ? "eye_hide_80.png" : "eye_show_80.png";
            }
        }
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

}