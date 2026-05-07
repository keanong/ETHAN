
using CommunityToolkit.Mvvm.Messaging;
using ETHAN.classes;
using ETHAN.ProgressDialog;
using System.Text.RegularExpressions;
using XDelServiceRef;

namespace ETHAN.Views;
public partial class LoginPwdReset : ContentPage
{

    //private XWSSoapClient xs = new XWSSoapClient(XWSSoapClient.EndpointConfiguration.XWSSoap);
    private XOEWSSoapClient xs = new XOEWSSoapClient(XOEWSSoapClient.EndpointConfiguration.XOEWSSoap);
    //private ProgressDialogService _progressService;
    private readonly IProgressDialogService _progressService;
    private bool _loadedOnce = false;

    public LoginPwdReset(IProgressDialogService progressService)
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

                ValidateSubmitButton();
            }

        }
        catch (Exception e)
        {
            string s = e.Message;
        }
    }

    protected override void OnDisappearing()
    {
        base.OnDisappearing();
        if (_progressService != null && _progressService.IsShowing)
            return;

        WeakReferenceMessenger.Default.Unregister<AppSleepMessage>(this);
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

    void ValidateSubmitButton()
    {
        try
        {
            bool hasUsername = !string.IsNullOrWhiteSpace(txtSUsername.Text);

            bool ok = hasUsername;

            btnSubmit.IsEnabled = ok;

            btnSubmit.Style = ok ? (Style)Application.Current.Resources["bstyleOrange"] : (Style)Application.Current.Resources["bstyleDisabled"];
        }
        catch (Exception e)
        {
            string s = e.Message;
        }
    }

    bool susernameOk = false;

    void onSUsernameFieldChanged(object sender, TextChangedEventArgs e)
    {
        ValidateSUsernameRule();
        ValidateSubmitButton();
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

    private async void btnSubmit_Click(object sender, EventArgs e)
    {
        try
        {
            await showProgress_Dialog("Processing...");
            XWSBase x = await xs.PasswordReset_RequestAsync(txtSUsername.Text.Trim().ToString(), "", "");
            await closeProgress_dialog();

            if (x != null && x.Status == 0)
            {
                await DisplayAlertAsync("", "You request has been sent.\r\nPlease wait for our email to reset your password.", "Ok");
                await BackToHomePage();
            } else if (x != null && x.Status != 0)
            {
                await DisplayAlertAsync("", x.Message, "Ok");
                await BackToHomePage();
            } else
            {
                await DisplayAlertAsync("", "Error processing this request.", "Ok");
                await BackToHomePage();
            }
        }
        catch (Exception ex)
        {
            await closeProgress_dialog();
            string s = ex.Message;
            await DisplayAlertAsync("", "Error processing this request.", "Ok");
            await BackToHomePage();
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