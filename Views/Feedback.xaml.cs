using CommunityToolkit.Mvvm.Messaging;
using ETHAN.classes;
using ETHAN.Network;
using ETHAN.ProgressDialog;
using System.Text;
using System.Text.RegularExpressions;
using XDelServiceRef;

namespace ETHAN.Views;

public partial class Feedback : ContentPage, IRecipient<AppSleepMessage>, IRecipient<AppResumeMessage>
{
    private XOEWSSoapClient xs = new XOEWSSoapClient(XOEWSSoapClient.EndpointConfiguration.XOEWSSoap);
    private readonly IProgressDialogService _progressService;
    private const string FeedbackEmail = "feedback@xdel.com";
    private bool _isSubmitting = false;

    private LoginInfo? logininfo;

    // Regex matching emojis / pictographs / symbols that should not be allowed.
    // Covers surrogate pairs (all emoji outside the BMP) plus common BMP emoji blocks.
    private static readonly Regex EmojiRegex = new Regex(
        @"[\uD800-\uDFFF]|[\u2600-\u27BF]|[\u2300-\u23FF]|[\u2B00-\u2BFF]|[\uFE00-\uFE0F]|[\u200D]|[\u20E3]",
        RegexOptions.Compiled);

    public Feedback(IProgressDialogService progressService)
    {
        try
        {
            InitializeComponent();
            _progressService = progressService;
            Shell.SetTabBarIsVisible(this, false);
        }
        catch (Exception ex)
        {
            string s = ex.Message;
        }
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

    protected override void OnAppearing()
    {
        base.OnAppearing();

        try
        {
            logininfo = AppSession.logininfo;
            WeakReferenceMessenger.Default.Register<AppSleepMessage>(this);
            WeakReferenceMessenger.Default.Register<AppResumeMessage>(this);

        }
        catch (Exception ex)
        {
            string s = ex.Message;
        }
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
                
            });
        }
    }

    // Get the logged-in receiver's registered email address for reply-to purposes
    private string GetSenderEmail()
    {
        string email = "";
        try
        {
            logininfo = AppSession.logininfo;

            if (logininfo != null && logininfo.ETHAN_Receiver != null
                && !string.IsNullOrEmpty(logininfo.ETHAN_Receiver.EMAILADDRESS))
            {
                email = logininfo.ETHAN_Receiver.EMAILADDRESS.Trim();
            }
        }
        catch (Exception ex)
        {
            string s = ex.Message;
        }
        return email;
    }

    // Get the logged-in user's display name (optional, helps HQ identify the sender)
    private string GetSenderName()
    {
        string name = "";
        try
        {
            if (logininfo != null && logininfo.ETHAN_Receiver != null)
            {
                XOE_ETHAN_Receiver er = logininfo.ETHAN_Receiver;
                name = ((er.FNAME ?? "") + " " + (er.LNAME ?? "")).Trim();
            }
        }
        catch (Exception ex)
        {
            string s = ex.Message;
        }
        return name;
    }

    // ===== Emoji filtering: strip emojis as the user types / pastes =====
    private void txtFeedback_TextChanged(object sender, TextChangedEventArgs e)
    {
        try
        {
            string newText = e.NewTextValue;
            if (string.IsNullOrEmpty(newText))
                return;

            string cleaned = RemoveEmojis(newText);

            if (!cleaned.Equals(newText))
            {
                int diff = newText.Length - cleaned.Length;
                int cursor = txtFeedback.CursorPosition;

                txtFeedback.Text = cleaned;

                // keep cursor in a sensible position after stripping
                int newCursor = Math.Max(0, Math.Min(cleaned.Length, cursor - diff));
                txtFeedback.CursorPosition = newCursor;
            }
        }
        catch (Exception ex)
        {
            string s = ex.Message;
        }
    }

    private static string RemoveEmojis(string input)
    {
        try
        {
            return EmojiRegex.Replace(input, string.Empty);
        }
        catch
        {
            return input;
        }
    }

    // ===== Submit =====
    void btnSubmit_Click(System.Object sender, System.EventArgs e)
    {
        try
        {
            SubmitClick();
        }
        catch (Exception ex)
        {
            string s = ex.Message;
        }
    }

    async void SubmitClick()
    {
        if (_isSubmitting) return;
        _isSubmitting = true;

        try
        {
            logininfo = AppSession.logininfo;
            string mode = AppSession.LoginMode;
            bool isReceiver = mode.Equals("r");

            if (logininfo?.clientInfo == null ||
            string.IsNullOrEmpty(logininfo.clientInfo.Web_UID) ||
            mode.Equals("s") && logininfo.clientInfo.CAIDX <= 0)
            {
                await DisplayAlertAsync("Session expired", "Please Login again.", "OK");
                await common.BackToLogin();
                return;
            }

            ClientInfo ci = logininfo.clientInfo;

            string feedback = txtFeedback.Text?.Trim() ?? string.Empty;

            if (string.IsNullOrWhiteSpace(feedback))
            {
                await AppShell.Current.DisplayAlertAsync("", "Please enter your feedback before submitting.", "OK");
                return;
            }

            feedback = RemoveEmojis(feedback);

            if (NetworkHelper.IsDisconnected())
            {
                await AppShell.Current.DisplayAlertAsync("No Internet Connection", "Please check your internet connection and try again.", "OK");
                return;
            }
            string WEB_UID = "";
            if (isReceiver && logininfo != null && logininfo.ETHAN_Receiver != null && !string.IsNullOrEmpty(logininfo.ETHAN_Receiver.UID))
                WEB_UID = logininfo.ETHAN_Receiver.UID;
            else
                WEB_UID = logininfo.clientInfo.Web_UID;

            if (string.IsNullOrEmpty(WEB_UID))
            {
                await AppShell.Current.DisplayAlertAsync("", "Session expired. Please Login again.", "OK");
                return;
            }

            await showProgress_Dialog("Processing...");

            var x = await xs.XOE_SendFeedbackAsync(WEB_UID, feedback);
            if (x == null || x.Status != 0)
            {
                await closeProgress_dialog();
                await AppShell.Current.DisplayAlertAsync("", x?.Message ?? "Error sending feedback. Please try again.", "OK");
                return;
            }

            await closeProgress_dialog();

            txtFeedback.Text = string.Empty;
            //await AppShell.Current.DisplayAlertAsync("Thank You", "Thank you for your feedback!", "OK");
            await ShowAlertSafe("Thank You", "Thank you for your feedback!");
            await BackToHomeAsync();
        }
        catch (Exception ex)
        {
            string s = ex.Message;
        }
        finally
        {
            _isSubmitting = false;
        }
    }

    // ===== Back navigation =====
    void BackToHome(System.Object sender, System.EventArgs e)
    {
        try
        {
            MainThread.BeginInvokeOnMainThread(async () =>
            {
                await BackToHomeAsync();
            });
        }
        catch (Exception ex)
        {
            string s = ex.Message;
        }
    }

    async Task BackToHomeAsync()
    {
        try
        {
            await Shell.Current.GoToAsync("..");
        }
        catch (Exception ex)
        {
            string s = ex.Message;
        }
    }

    protected override bool OnBackButtonPressed()
    {
        try
        {
            MainThread.BeginInvokeOnMainThread(async () =>
            {
                await BackToHomeAsync();
            });
        }
        catch (Exception ex)
        {
            string s = ex.Message;
        }
        return true;
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