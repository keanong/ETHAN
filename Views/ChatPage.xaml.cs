using CommunityToolkit.Maui.Extensions;
using CommunityToolkit.Maui.Views;
using ETHAN.classes;
using ETHAN.Network;
using ETHAN.ProgressDialog;
using ETHAN.ViewModel;
//using System.Text.Json;
using Newtonsoft.Json;
using System.Collections.ObjectModel;
using System.ComponentModel;
using XDelServiceRef;

namespace ETHAN.Views;

//[QueryProperty(nameof(LOGININFO), "LOGININFO")] // Add a QueryProperty to handle the navigation parameter
[QueryProperty(nameof(CHATCHILD), "CHATCHILD")] // Add a QueryProperty to handle the navigation parameter
[QueryProperty(nameof(CHATTITLE), "CHATTITLE")] // Add a QueryProperty to handle the navigation parameter
[QueryProperty(nameof(CHATSTATUS), "CHATSTATUS")] // Add a QueryProperty to handle the navigation parameter
[QueryProperty(nameof(LTR), "LTR")] // Add a QueryProperty to handle the navigation parameter



public partial class ChatPage : ContentPage
{
    private XOEWSSoapClient xs = new XOEWSSoapClient(XOEWSSoapClient.EndpointConfiguration.XOEWSSoap);
    private readonly IProgressDialogService _progressService;
    string errmsg_relogin = "Session expired. Please Login again.";
    private ChatViewModel vm;
    private bool _loadedOnce = false;

    // --- Raw navigation strings ---
    private LoginInfo? logininfoJson;
    private ChatChild? chatchildJson;
    private string? chattitleJson;
    private int chatstatusJson;

    private LoginInfo? logininfo;
    private ChatChild? chatchild;
    private string? chattitle;
    private int chatstatus;

    /*public LoginInfo? LOGININFO
    {
        set => logininfoJson = value;
    }*/

    public ChatChild? CHATCHILD
    {
        set => chatchildJson = value;
    }

    public string? CHATTITLE
    {
        set => chattitleJson = value;
    }

    public int CHATSTATUS
    {
        set => chatstatusJson = value;
    }

    private ManageJobPageVM.LoadTabsRequest? ltr;

    public ManageJobPageVM.LoadTabsRequest? LTR
    {
        set
        {
            ltr = value;
        }
    }

    public ChatPage(ChatViewModel viewModel, IProgressDialogService progressService)
    {
        InitializeComponent();
        _progressService = progressService;
        Shell.SetTabBarIsVisible(this, false);

        this.Padding = DeviceInfo.Platform == DevicePlatform.iOS
               ? new Thickness(0)
               : new Thickness(0);

        vm = viewModel;
        BindingContext = vm;
        vm.PropertyChanged += Vm_PropertyChanged;
    }

    private async void OnNewMessagesFetched(object sender, EventArgs e)
    {
        // Scroll to bottom after UI layout
        //await Task.Delay(10);
        if (vm.Messages.Count > 0)
            ChatListView.ScrollTo(vm.Messages.Last(), position: ScrollToPosition.End, animate: false);

        // Disable entry if chat closed
        gridEntry.IsVisible = vm.IsChatOpen;
    }

    private void Vm_PropertyChanged(object sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName == nameof(vm.IsChatOpen) || e.PropertyName == nameof(vm.ChatStatus))
        {
            if (!vm.IsChatOpen || vm.ChatStatus == 0)
            disableEntryButton();
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

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        try
        {
            if (_loadedOnce)
                return;

            _loadedOnce = true;

            // Deserialize login info
            /*if (logininfoJson != null)
                logininfo = logininfoJson;*/

            logininfo = AppSession.logininfo;

            // Deserialize chat child
            if (chatchildJson != null)
                chatchild = chatchildJson;

            // Title
            chattitle = chattitleJson;

            // Status
            chatstatus = chatstatusJson;        

            if (logininfo == null || chatchild == null)
                return;

            vm.WebUid = logininfo.clientInfo.Web_UID;
            vm.ChatStatus = chatstatus;
            vm.IsChatOpen = chatstatus == 1;
            btnTopNBack.Text = chattitle;
            lblChatStatus.Text = chatstatus.ToString();

            await getChatHistory();

            //// Subscribe to event to scroll UI when messages update
            vm.NewMessagesFetched += OnNewMessagesFetched;

            //// Start polling only if active
            string mode = AppSession.LoginMode;
            vm.StartPolling(mode);

        }
        catch (Exception e)
        {
            string s = e.Message;
        }
    }

    protected override void OnDisappearing()
    {
        //// UnSubscribe to event to scroll UI when messages update
        vm.NewMessagesFetched -= OnNewMessagesFetched;
        vm.StopPolling();
        base.OnDisappearing();
    }

    async void Back(System.Object sender, System.EventArgs e)
    {
        try
        {
            BackToChatMainPage();
        }
        catch (Exception ex)
        {
            string s = ex.Message;
        }
    }

    async void BackToChatMainPage()
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
                        { "DEFAULTTAB", "Chat" },
                    });*/

                if (ltr != null)
                {

                    var stack = Shell.Current.Navigation.NavigationStack.ToList();
                    var currentPage = stack.Last();

                    foreach (var page in stack)
                    {
                        // Skip current page — only remove pages BELOW current
                        if (page == currentPage) continue;

                        if (page is ChatPage)
                            Shell.Current.Navigation.RemovePage(page);
                    }

                    await Shell.Current.GoToAsync("..", new Dictionary<string, object>
                    {
                        { "BARCODE", null },
                        { "LTR", ltr },
                        { "vmm", null }
                    });

                    /*await Shell.Current.GoToAsync("//ManageJobPage", new Dictionary<string, object>
                        {
                            { "BARCODE", null },
                            { "LTR", ltr },
                            { "vmm", null }
                        });*/
                } else
                {
                    await Shell.Current.GoToAsync("///CardShellPage", new Dictionary<string, object>
                    {
                        { "BARCODE", null },
                        { "DEFAULTTAB", "Chat" },
                    });
                }

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
        BackToChatMainPage();
        return true;
    }

    private async void OnSendClicked(object sender, EventArgs e)
    {
        if (string.IsNullOrEmpty(MessageEntry.Text.Trim()) || string.IsNullOrEmpty(lblSessionIDX.Text.Trim()))
            return;


        await sendMsg();
    }

    private void disableEntryButton()
    {
        gridEntry.IsVisible = false;
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
        List<XDelServiceRef.ChatSession> lists = new List<XDelServiceRef.ChatSession>();
        XDelServiceRef.ChatSession list = null;
        ChatSessions css = null;
        string errmsg = "Session expired. Please Login again.";
        ObservableCollection<ChatMessage> cms = new();
        XDelServiceRef.ChatConversation convo = null;
        ChatMessage cm = null;
        string staffname = "";
        string text = "";
        string timing = "";
        bool left;
        string newDateStr = "";
        string prevDateStr = "";

        try
        {
            if (NetworkHelper.IsDisconnected())
            {
                await DisplayAlertAsync("No Internet Connection", "Please check your internet connection and try again.", "OK");
                return;
            }

            if (canProceed())
            {
                string mode = AppSession.LoginMode;
                logininfo = AppSession.logininfo;

                if (chatchild == null || chatchild.SessionIDXs == null || chatchild.SessionIDXs.Count == 0)
                {
                    disableEntryButton();
                    return;
                }

                var vm = BindingContext as ChatViewModel;                
                if (vm == null)
                {
                    disableEntryButton();
                    return;
                }

                lblSessionIDX.Text = "";

                ClientInfo ci = logininfo!.clientInfo;
                await showProgress_Dialog("Processing...");

                for (int i = chatchild.SessionIDXs.Count - 1; i >= 0; i--)
                {
                    list = null;
                    if (mode.Equals("r"))
                        css = await xs.XOE_VCS_Get_ChatsAsync(ci.Web_UID, chatchild.SessionIDXs[i]);
                    else
                        css = await xs.VCS_Get_ChatsAsync(ci.Web_UID, chatchild.SessionIDXs[i]);

                    if (css != null && css.Status == 0 && css.Sessions != null && css.Sessions.Length > 0)
                        list = css.Sessions[0];
                    if (list != null)
                        lists.Add(list);
                    if (list != null && i == 0)
                    {
                        chatstatus = list.Chat_Status == Status.csPrivate || list.Chat_Status == Status.csOpen ? 1 : 0;
                    }

                    if (i == 0)
                    {
                        lblSessionIDX.Text = chatchild.SessionIDXs[0].ToString();
                        vm.SessionIdx = chatchild.SessionIDXs[0];
                    }

                }

                if (lists != null && lists.Count > 0)
                {
                    foreach (XDelServiceRef.ChatSession l in lists)
                    {
                        newDateStr = l.TimeStamp.ToString("dd/MM/yyyy");
                        //add date
                        if (!string.Equals(newDateStr, prevDateStr))
                        {
                            cm = new ChatMessage
                            {
                                StaffName = "",
                                Text = l.TimeStamp.ToString("dd/MM/yyyy"),
                                Time = "",
                                BubblePosition = LayoutOptions.Center,
                                BubbleColor = (Color)Application.Current.Resources["Gray600"],
                                MessageTextColor = Colors.White,
                                ShowName = false,
                                ShowTime = false,
                                MinWidthRequest = 100,
                                Fontsize = 13,
                                TextPosition = LayoutOptions.Center
                            };

                            if (cm != null)
                                cms.Add(cm);
                        }
                        //add date
                        prevDateStr = newDateStr;

                        if (l.Conversation != null && l.Conversation.Length > 0)
                        {
                            for (int i = l.Conversation.Length - 1; i >= 0; i--)
                            {
                                cm = null;
                                convo = l.Conversation[i];
                                if (convo == null)
                                    continue;

                                staffname = string.IsNullOrEmpty(convo.StaffName) ? "" : convo.StaffName;
                                text = string.IsNullOrEmpty(convo.Message) ? "" : convo.Message;
                                timing = convo.TimeStamp != DateTime.MinValue ? convo.TimeStamp.ToString("hh:mm:tt") : "-";
                                left = !string.IsNullOrEmpty(staffname);

                                cm = new ChatMessage
                                {
                                    StaffName = staffname,
                                    Text = text,
                                    Time = timing,
                                    BubblePosition = left ? LayoutOptions.Start : LayoutOptions.End,
                                    BubbleColor = left ? (Color)Application.Current.Resources["Gray600"] : (Color)Application.Current.Resources["XDelOrange"],
                                    MessageTextColor = Colors.White,
                                    ShowName = left,
                                    ShowTime = true,
                                    MinWidthRequest = 150
                                };

                                if (cm != null)
                                    cms.Add(cm);
                            }
                        }

                    }
                }

                if (chatstatus == 0)
                {
                    cm = new ChatMessage
                    {
                        StaffName = "",
                        Text = "Session Closed",
                        Time = "",
                        BubblePosition = LayoutOptions.Center,
                        BubbleColor = (Color)Application.Current.Resources["Gray600"],
                        MessageTextColor = Colors.White,
                        ShowName = false,
                        ShowTime = false,
                        MinWidthRequest = 100,
                        Fontsize = 13,
                        TextPosition = LayoutOptions.Center
                    };
                    cms.Add(cm);
                }

                if (chatchild.isNewChat)
                {
                    cm = new ChatMessage
                    {
                        StaffName = "XDel",
                        Text = "Hi,\r\nThank you for using XDel.\r\nPlease enter your enquiry to start the conversation.\r\n\r\nOur Customer Service Officer will attend to you shortly once available.\r\nPlease wait patiently.",
                        Time = "",
                        BubblePosition = LayoutOptions.Start,
                        BubbleColor = (Color)Application.Current.Resources["Gray600"],
                        MessageTextColor = Colors.White,
                        ShowName = true,
                        ShowTime = false,
                        MinWidthRequest = 150
                    };

                    cms.Add(cm);
                }

                if (cms != null && cms.Count > 0)
                {
                    vm.ReplaceMessages(cms);
                    await Task.Delay(50);   // allow UI to layout
                    if (vm.Messages.Count > 0)
                        ChatListView.ScrollTo(vm.Messages.Last(), position: ScrollToPosition.End, animate: false);
                }
                else
                    vm.Messages.Clear();

                await closeProgress_dialog();

                if (chatchild.isNewChat)
                {
                    lblSessionIDX.Text = chatchild.SessionIDXs[0].ToString();
                    vm.SessionIdx = chatchild.SessionIDXs[0];
                    lblChatStatus.Text = "1";
                    vm.ChatStatus = 1;
                    vm.IsChatOpen = true;
                } else
                {
                    lblChatStatus.Text = chatstatus.ToString();
                    if (chatstatus == 0)
                        disableEntryButton();
                    vm.ChatStatus = chatstatus;
                    vm.IsChatOpen = chatstatus == 1;
                }
            }
            else
            {
                await AppShell.Current.DisplayAlertAsync("", errmsg, "OK");
                await common.BackToLogin();
            }
        } catch (Exception e)
        {
            string s = e.Message;
        }
    }

    private async Task sendMsg()
    {
        XWSBase xb = null;
        try
        {
            if (canProceed())
            {
                string mode = AppSession.LoginMode;
                string txt = MessageEntry.Text;
                ClientInfo ci = logininfo.clientInfo;
                
                if (mode.Equals("r"))
                    xb = await Task.Run(() => xs.XOE_VCS_Post_MessageAsync(ci.Web_UID, long.Parse(lblSessionIDX.Text), txt));
                else
                    xb = await Task.Run(() => xs.VCS_Post_MessageAsync(ci.Web_UID, long.Parse(lblSessionIDX.Text), txt));
                if (xb != null && xb.Status == 0)
                {
                    var vm = BindingContext as ChatViewModel;
                    if (vm == null) return;

                    if (!string.IsNullOrEmpty(MessageEntry.Text))
                    {
                        vm.AddMessage(txt, "", DateTime.Now.ToString("hh:mm:tt"));

                        MessageEntry.Text = string.Empty;

                        // Scroll to bottom
                        if (vm.Messages.Count > 0)
                            ChatListView.ScrollTo(vm.Messages.Last(), position: ScrollToPosition.End);
                    }
                }
            } else
            {
                await AppShell.Current.DisplayAlertAsync("", errmsg_relogin, "OK");
                await common.BackToLogin();
            }
        } catch (Exception e)
        {
            string s = e.Message;
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