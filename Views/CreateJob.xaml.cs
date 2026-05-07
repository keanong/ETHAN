using ETHAN.classes;
using ETHAN.ViewModel;
using System.Collections.ObjectModel;
using XDelServiceRef;
using ETHAN.ProgressDialog;
using System.Globalization;
using ZXing;
using Microsoft.IdentityModel.Protocols.WsTrust;
using ETHAN.Network;

namespace ETHAN.Views;

//#if ANDROID
//using BottomSheetView = Google.Android.Material.BottomSheet.BottomSheetDialog;
//#elif IOS || MACCATALYST
//using BottomSheetView = UIKit.UIViewController;
//#elif TIZEN
//using BottomSheetView = Tizen.UIExtensions.NUI.Popup;
//#else
//using BottomSheetView = Microsoft.UI.Xaml.Controls.Primitives.Popup;
//#endif


[QueryProperty(nameof(Vmm), "vmm")] // Add a QueryProperty to handle the navigation parameter
//[QueryProperty(nameof(LOGININFO), "LOGININFO")] // Add a QueryProperty to handle the navigation parameter
[QueryProperty(nameof(LTR), "LTR")] // Add a QueryProperty to handle the navigation parameter


public partial class CreateJob : ContentPage
{
    //public CreateJobVM? vm { get; set; }
    private CreateJobVM? vm;
    public CreateJobVM Vmm
    {
        set
        {
            /*vm = value ?? new CreateJobVM();
            BindingContext = vm; // If null, create a new instance*/
            if (value != null)
            {
                vm = value;
                BindingContext = vm;
            }
            else if (vm == null)
            {
                // Only create new if we genuinely have no vm yet
                // This handles the first launch from Home_Page with vmm=null
                vm = new CreateJobVM();
                BindingContext = vm;
            }
        }
    }

    private LoginInfo? logininfo;
    public LoginInfo? LOGININFO
    {
        set
        {
            /*logininfo = value ?? null;*/

            // Only update if value is not null
            // Returning via ".." from AddressBookPage will pass null
            if (value != null)
                logininfo = value;
        }
    }

    private ManageJobPageVM.LoadTabsRequest? ltr;

    public ManageJobPageVM.LoadTabsRequest? LTR
    {
        set
        {
            /*ltr = value;*/

            // Only update if value is not null
            // Returning via ".." from AddressBookPage will pass null
            if (value != null)
                ltr = value;
        }
    }

    //BottomSheetView? bottomSheet;    

    //private XWSSoapClient xs = new XWSSoapClient(XWSSoapClient.EndpointConfiguration.XWSSoap);
    private XOEWSSoapClient xs = new XOEWSSoapClient(XOEWSSoapClient.EndpointConfiguration.XOEWSSoap);
    //private ProgressDialogService _progressService;
    private readonly IProgressDialogService _progressService;

    string[] times = new string[] { "09:00", "09:30", "10:00", "10:30", "11:00", "11:30", "12:00", "12:30", "13:00", "13:30",
        "14:00", "14:30", "15:00", "15:30", "16:00", "16:30", "17:00", "17:30" };

    string[] dateformats = { "dd/MM/yyyy HH:mm", "dd/MM/yyyy", "dd/MM/yyyy HH:mm tt" };

    //List<common.eExpressType>? fakeAllowedExpTypes { get; set; }
    List<XDelServiceRef.eExpressType>? fakeAllowedExpTypes { get; set; }

    public CreateJob(IProgressDialogService progressService)
    {
        try
        {
            InitializeComponent();
            _progressService = progressService;

            //regWeakReferenceMsger();

            BindingContext = vm;
            //loadValue(); ////shifted to OnAppearing
            //addTapGestureRecognizer(); ////shifted to OnAppearing
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
            //AppContainer.WidthRequest = Math.Min(width * 0.32, 600); // 40% of screen width, max 800
            //AppContainer.HeightRequest = Math.Min(height * 0.85, 1000);
            AppContainer.WidthRequest = 500;
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
#if IOS
this.HideSoftInputOnTapped = false;
#endif

            // Handles the case where we return via ".." without params
            // Vmm setter won't have fired in this case
            if (vm != null)
                BindingContext = vm;

            removeTapGestureRecognizer();
            if (!await loadValue())
                return;
            addTapGestureRecognizer();
        }
        catch (Exception e)
        {
            string s = e.Message;
        }
    }

    protected override void OnDisappearing()
    {
        base.OnDisappearing();

        // Remove all gesture recognizers so lambdas/handlers don't hold page alive
        removeTapGestureRecognizer();
        removeInvBSHeaderLabelGestureRecog();

        // Force-close any open sheets and hide them
        // (fire-and-forget is acceptable here since page is leaving)
        _ = CloseBottomSheetSafely(BSContentType);
        _ = CloseBottomSheetSafely(BSInv);
        _ = CloseBottomSheetSafely(BSCollectionTime);
        _ = CloseBottomSheetSafely(BSDeliveryTime);
        _ = CloseBottomSheetSafely(BSRtnTime);

        // Clear CollectionView item sources to release list references
        cvContentType.ItemsSource = null;
        cvInvP1.ItemsSource = null;
        cvDate.ItemsSource = null;
        cvColLunch.ItemsSource = null;
        cvTime.ItemsSource = null;
        cvDFDate.ItemsSource = null;
        cvDFLunch.ItemsSource = null;
        cvDFTime.ItemsSource = null;
        cvDBTime.ItemsSource = null;
        cvRFDate.ItemsSource = null;
        cvRFLunch.ItemsSource = null;
        cvRFTime.ItemsSource = null;
        cvRBTime.ItemsSource = null;
    }

    async Task CloseBottomSheetSafely(ETHAN.BS.BottomSheet bs)
    {
        try
        {
            if (bs != null && bs.isShowing)
            {
                await bs.CloseBottomSheet();
            }
        }
        catch { }
        finally
        {
            if (bs != null)
            {
                bs.isShowing = false;
                bs.IsVisible = false; // releases visual tree from layout pass
            }
        }
    }

    async void showmsg(string title, string msg, bool addGestures = false)
    {
        try
        {
            if (addGestures)
                addTapGestureRecognizer();

            //await Shell.Current.DisplayAlert(String.IsNullOrEmpty(title) ? "Alert" : title, msg, "OK");
            await DisplayAlertAsync(String.IsNullOrEmpty(title) ? "" : title, msg, "OK");
        }
        catch (Exception ex)
        {
            string s = ex.Message;
        }
    }

    void regWeakReferenceMsger()
    {
        try
        {


            //WeakReferenceMessenger.Default.Register<Msg_CollectionTime>(this, (r, m) =>
            //{
            //    // Handle the message
            //    lblColTime.Text = m.Content;
            //    lblColDateTimeSelected.Text = m.Content;

            //    lblColTime.Text = m.lblColTime;
            //    lblColDateTimeSelected.Text = m.lblColDateTimeSelected;
            //    lblColDateSelected.Text = m.lblColDateSelected;
            //    lblColTimeSelected.Text = m.lblColTimeSelected;
            //    lblColLunchTimeSelected.Text = m.lblColLunchTimeSelected;
            //});

            //WeakReferenceMessenger.Default.Register<Msg_DeliveryTime>(this, (r, m) =>
            //{
            //    //// Handle the message
            //    lblDelTime.Text = m.fromdate + " - " + m.todate;
            //    lblDelFromTimeSelected.Text = m.fromdate;
            //    lblDelByTimeSelected.Text = m.todate;
            //});

            //WeakReferenceMessenger.Default.Register<Msg_ContentType>(this, (r, m) =>
            //{
            //    //// Handle the message
            //    lblContent.Text = m.Content;
            //    lblContentSelected.Text = m.Content;
            //});
        }
        catch (Exception ex)
        {
            string s = ex.Message;
        }
    }

    void BackToHome(System.Object sender, System.EventArgs e)
    {
        try
        {
            //string v = string.Empty;
            //await Shell.Current.GoToAsync($"..?bcval={v}", true);
            //await Shell.Current.GoToAsync($"///Homepage?bcval={v}", true);

            BackToHomePage();
        }
        catch (Exception ex)
        {
            string s = ex.Message;
        }
    }

    async void BackToHomePage()
    {
        try
        {
            bool AllBsClosed = false;
            bool bscolnotopen = ((BSCollectionTime == null) || (BSCollectionTime != null && !BSCollectionTime.isShowing));
            bool bsdelnotopen = ((BSDeliveryTime == null) || (BSDeliveryTime != null && !BSDeliveryTime.isShowing));
            bool bscontenttypenotopen = ((BSContentType == null) || (BSContentType != null && !BSContentType.isShowing));
            bool bsinvp1notopen = ((BSInv == null) || (BSInv != null && !BSInv.isShowing));
            bool bsrtnnotopen = ((BSRtnTime == null) || (BSRtnTime != null && !BSRtnTime.isShowing));

            AllBsClosed = (bscolnotopen && bsdelnotopen && bscontenttypenotopen && bsinvp1notopen && bsrtnnotopen);
            if (AllBsClosed)
            {
                MainThread.BeginInvokeOnMainThread(async () =>
                {
                    logininfo = AppSession.logininfo;
                    BindingContext = null;
                    await Shell.Current.GoToAsync("///CardShellPage", new Dictionary<string, object>
                        {
                            { "BARCODE", null }
                        });
                }
                                    );

            }
        }
        catch (Exception e)
        {
            string s = e.Message;
        }
    }

    void BackToSummary(System.Object sender, System.EventArgs e)
    {
        try
        {
            BackToSummaryPage();
        }
        catch (Exception ex)
        {
            string s = ex.Message;
        }
    }

    async void BackToSummaryPage()
    {
        try
        {
            bool AllBsClosed = false;
            bool bscolnotopen = ((BSCollectionTime == null) || (BSCollectionTime != null && !BSCollectionTime.isShowing));
            bool bsdelnotopen = ((BSDeliveryTime == null) || (BSDeliveryTime != null && !BSDeliveryTime.isShowing));
            bool bscontenttypenotopen = ((BSContentType == null) || (BSContentType != null && !BSContentType.isShowing));
            bool bsinvp1notopen = ((BSInv == null) || (BSInv != null && !BSInv.isShowing));
            bool bsrtnnotopen = ((BSRtnTime == null) || (BSRtnTime != null && !BSRtnTime.isShowing));

            AllBsClosed = (bscolnotopen && bsdelnotopen && bscontenttypenotopen && bsinvp1notopen && bsrtnnotopen);
            if (AllBsClosed)
            {
                MainThread.BeginInvokeOnMainThread(async () =>
                {
                    logininfo = AppSession.logininfo;
                    BindingContext = null;
                    await Shell.Current.GoToAsync("JobSummary", new Dictionary<string, object>
                        {
                            { "vmm", vm },
                            { "LTR", ltr },
                            { "Source", "CreateJobPage" }
                        });
                }
                                    );

                /*bool rtnToMJP = (vm != null && vm.job1 != null && vm.job1.JobsIDX != null && vm.job1.JobsIDX > 0);

                MainThread.BeginInvokeOnMainThread(async () =>
                {
                    string v = string.Empty;
                    if (rtnToMJP)
                        await Shell.Current.GoToAsync("ManageJobPage", new Dictionary<string, object>
                        {
                            { "LOGININFO", logininfo },
                            { "BARCODE", null },
                            { "vmm", null }
                        });
                    else
                    {
                        BindingContext = null;
                        await Shell.Current.GoToAsync("///CardShellPage", new Dictionary<string, object>
                        {
                            { "LOGININFO", logininfo },
                            { "BARCODE", null }
                        });
                    }
                }
                                    );*/

            }
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
            if (vm != null && vm.FromSummary)
                BackToSummaryPage();
            else
                BackToHomePage();
        }
        catch (Exception ex)
        {
            string s = ex.Message;
        }
        return true;
    }

    async void btnProceed_Clicked(object sender, EventArgs e)
    {
        JobStructure js = new JobStructure();
        string errmsg = "";
        try
        {
            if (NetworkHelper.IsDisconnected()) 
            {
                await DisplayAlertAsync("No Internet Connection", "Please check your internet connection and try again.", "OK");
                return;
            }

            logininfo = AppSession.logininfo;
            if (logininfo?.clientInfo == null || string.IsNullOrEmpty(logininfo.clientInfo.Web_UID))
            {
                await DisplayAlertAsync("Session expired", "Please Login again.", "OK");
                await common.BackToLogin();
                return;
            }

            if (vm == null)
            {
                await DisplayAlertAsync("", "Unexpected error occurred. Please try again.", "OK");
                BackToHomePage();
                return;
            }

            if (vm.job1 != null)
                js = await xs.SaveTempJobAsync(logininfo.clientInfo.Web_UID, vm.job1);

            if (js == null)
            {
                await DisplayAlertAsync("", "Unable to proceed. Please try again.", "OK");
                BackToHomePage();
                return;
            }
            if (js != null && js.Status != 0)
            {
                await DisplayAlertAsync("", "Unable to proceed. Please try again.", "OK");
                BackToHomePage();
                return;
            }

            vm.JobsIDX = js!.JobList[0].JobsIDX ?? 0;

            if (vm.JobsIDX2_ > 0 && !vm.TwoWay)
            {
                await xs.DiscardUnPostedJobAsync(logininfo.clientInfo.Web_UID, vm.JobsIDX2_);
            }

            if (vm.job2 != null && vm.TwoWay)
            {
                if (vm.JobsIDX2_ > 0)
                    vm.job2.JobsIDX = vm.JobsIDX2_;
                vm.job2.ParentJobsIDX = vm.JobsIDX;
                js = await xs.SaveTempJobAsync(logininfo.clientInfo.Web_UID, vm.job2);
            }

            /*await Shell.Current.GoToAsync("JobSummary", new Dictionary<string, object>
                        {
                            { "vmm", vm },
                            { "LTR", ltr },
                            {"LOGININFO",  logininfo}
                        });*/
            await Shell.Current.GoToAsync("JobSummary", new Dictionary<string, object>
                        {
                            { "vmm", vm },
                            { "LTR", ltr },
                            { "Source", "CreateJobPage" }
                        });


            /* if (js == null)
             {

             } else if (js.Status == 0 && js.JobList != null && js.JobList.Length > 0)
             {
                 vm.JobsIDX = js.JobList[0].JobsIDX ?? 0;

                 await Shell.Current.GoToAsync("JobSummary", new Dictionary<string, object>
                         {
                             { "vmm", vm },
                             {"LOGININFO",  logininfo}
                         });
             }*/

            //vm.JobsIDX = 9362019;

            //await Shell.Current.GoToAsync("JobSummary", new Dictionary<string, object>
            //        {
            //            { "vmm", vm },
            //            {"LOGININFO",  logininfo}
            //        });
        } catch (Exception ex)
        {
            string s = ex.Message;
        }
    }

    //private void loadValue()
    private async Task<bool> loadValue()
    {
        string ca = "Tap to input details.";
        string da = "Tap to input details.";
        string ra = "Tap to input details.";
        string content1 = "Select";
        string content2 = "Select";
        string ctime = "Select";
        string dtime = "Select";
        string rtime = "Select";
        string expDisp1 = "TBA";
        string expDisp2 = "TBA";
        string do1 = "Add/Remove";
        string do2 = "Add/Remove";
        bool TwoWay = false;

        try
        {
            logininfo = AppSession.logininfo;
            if (logininfo == null)
            {
                await DisplayAlertAsync("Session expired", "Please Login again.", "OK");
                await common.BackToLogin();
                return false;
            }

            await addContentTypes();

            if (vm != null)
            {
                TwoWay = vm.TwoWay;
                is2way = TwoWay;

                ca = getSavedAddressDetail(vm.ColAddressFinal);
                vm.job1.PULocation = vm.ColAddressFinal;

                da = getSavedAddressDetail(vm.DelAddressFinal);
                vm.job1.DLLocation = vm.DelAddressFinal;

                ra = getSavedAddressDetail(vm.RtnAddressFinal);
                if (vm.job2 != null)
                {
                    vm.job2.PULocation = vm.DelAddressFinal;
                    vm.job2.DLLocation = vm.RtnAddressFinal;
                }

                content1 = !string.IsNullOrEmpty(vm.ContentType1) ? vm.ContentType1 : "Select";

                content2 = !string.IsNullOrEmpty(vm.ContentType2) ? vm.ContentType2 : "Select";

                ctime = vm.colDateTimeFinalDispText;

                dtime = vm.dfbDateTimeFinalDispText;

                rtime = vm.rtndfbDateTimeFinalDispText;

                expDisp1 = getExpTypeStr2(vm.exp1);

                expDisp2 = getExpTypeStr2(vm.exp2);

                ObservableCollection<invoice> invs1 = vm.getInvoicesP1();
                ObservableCollection<invoice> invs2 = vm.getInvoicesP2();

                do1 = "Add/Remove" + ((invs1 != null && invs1.Count > 0) ? " (" + invs1.Count.ToString() + ")" : "");
                do2 = "Add/Remove" + ((invs2 != null && invs2.Count > 0) ? " (" + invs2.Count.ToString() + ")" : "");
            }

            swOneTwoWay.Toggled -= swOneTwoWayonToggle;
            swOneTwoWay.IsToggled = TwoWay;
            gridRtnAddress.IsVisible = TwoWay;
            gridRtnPortion.IsVisible = TwoWay;
            bvRtnAddress.IsVisible = TwoWay;
            bvRtnSection.IsVisible = TwoWay;

            swOneTwoWay.Toggled += swOneTwoWayonToggle;

            lblPUAdd.Text = ca;
            lblDLAdd.Text = da;
            lblRTNAdd.Text = ra;
            lblContent.Text = content1;
            lblRtnContent.Text = content2;
            lblColTime.Text = ctime;
            lblDelTime.Text = dtime;
            lblRtnTime.Text = rtime;
            lblExpType.Text = expDisp1;
            lblExpType2.Text = expDisp2;
            lblColInv.Text = do1;
            lblRtnInv.Text = do2;

            if (logininfo != null && logininfo.clientInfo != null && logininfo.clientInfo.AccountType == TAccountType.atPrePaid)
            {
                lblPaymentType.Text = "Prepaid";

                lblTotaltxt.IsVisible = false;
                lblTotalValueCurr.IsVisible = false;
                lblTotalValue.IsVisible = false;
                lblwgst.IsVisible = false;
                lblBalancetxt.IsVisible = true;
                lblCurrencytxt.IsVisible = true;
                lblBalance.IsVisible = true;
                lblBalance.Text = logininfo.PrePaidBalance == null ? "$0.00" : "$" + logininfo.PrePaidBalance.Value.ToString("F2");
                okPrepaidBal = !(logininfo.PrePaidBalance == null || (logininfo.PrePaidBalance != null && logininfo.PrePaidBalance.Value < 20));
                lblBalancetxt.Style = okPrepaidBal ? (Style)Application.Current.Resources["textStyleSmallBlackBold"] : (Style)Application.Current.Resources["textStyleSmallRedBold"];
                lblCurrencytxt.Style = okPrepaidBal ? (Style)Application.Current.Resources["textStyleSmallBlackBold"] : (Style)Application.Current.Resources["textStyleSmallRedBold"];
                lblBalance.Style = okPrepaidBal ? (Style)Application.Current.Resources["textStyleSmallBlackBold"] : (Style)Application.Current.Resources["textStyleSmallRedBold"];
            }
            else
            {
                lblPaymentType.Text = "Credit Terms";

                lblTotaltxt.IsVisible = false;
                lblTotalValueCurr.IsVisible = false;
                lblTotalValue.IsVisible = false;
                lblwgst.IsVisible = false;
                lblBalancetxt.IsVisible = false;
                lblCurrencytxt.IsVisible = false;
                lblBalance.IsVisible = false;
                okPrepaidBal = true;
            }

            btnTitle.IsVisible = true;
            btnTitle2.IsVisible = false;

            if (vm != null && vm.FromSummary)
            {
                btnTitle.IsVisible = ltr == null;
                btnTitle2.IsVisible = ltr != null;

                int cvDateCount = cvDate.ItemsSource is List<date> collection ? collection.Count : 0;
                if (cvDateCount == 0)
                    addColDateList();
                preSelectColDate2();

                string ColDateTimeSelected = vm.colDateTimeFinalValue;
                if (!String.IsNullOrEmpty(ColDateTimeSelected))
                {
                    if (fakeAllowedExpTypes == null || (fakeAllowedExpTypes != null && fakeAllowedExpTypes.Count == 0))
                    {
                        if (!await addExpTypes())
                            return false;
                    }
                        

                    int cvDFTimeCount = cvDFTime.ItemsSource is List<time> collection1 ? collection1.Count : 0;
                    int cvDBTimeCount = cvDBTime.ItemsSource is List<delByDateTime> collection2 ? collection2.Count : 0;

                    int cvDFDateCount = cvDFDate.ItemsSource is List<date> collection3 ? collection3.Count : 0;
                    if (cvDFDateCount == 0)
                        addDelFromDateList();
                    preSelectDelDate();
                }

                if (vm.TwoWay)
                {
                    string dbDateTimeSelected = !String.IsNullOrEmpty(vm.dbExtByDateTimeValue) ? vm.dbExtByDateTimeValue : vm.dbDateTimeFinalValue;
                    bool dbDateTimeFinalized = !String.IsNullOrEmpty(dbDateTimeSelected) && !String.IsNullOrEmpty(lblDelTime.Text) && !lblDelTime.Text.ToLower().Equals("select");


                    if (dbDateTimeFinalized)
                    {
                        if (fakeAllowedExpTypes == null || (fakeAllowedExpTypes != null && fakeAllowedExpTypes.Count == 0))
                        {
                            if (!await addExpTypes())
                                return false;
                        }

                        int cvRFDateCount = cvRFDate.ItemsSource is List<date> collection4 ? collection4.Count : 0;
                        if (cvRFDateCount == 0)
                            addRtnFromDateList();
                        preSelectRtnDate();
                    }
                }

            }

            validateButton();
        }
        catch (Exception e)
        {
            string s = e.Message;
            await DisplayAlertAsync("Exception", s, "OK");
            await common.BackToLogin();
            return false;
        }
        return true;
    }

    bool is2way = false;
    bool okColAdd = false;
    bool okDelAdd = false;
    bool okRtnAdd = false;
    bool okContent1 = false;
    bool okContent2 = false;
    bool okColTime = false;
    bool okDelTime = false;
    bool okRtnTime = false;
    bool okExp1 = false;
    bool okExp2 = false;
    bool okPrepaidBal = false;

    void validateButton()
    {
        string ca = "Tap to input details.";
        string da = "Tap to input details.";
        string ra = "Tap to input details.";
        string content1 = "Select";
        string content2 = "Select";
        string ctime = "Select";
        string dtime = "Select";
        string rtime = "Select";
        string expDisp1 = "TBA";
        string expDisp2 = "TBA";
        string do1 = "Add/Remove";
        string do2 = "Add/Remove";
        bool ok = false;
        try
        {
            if (vm != null && vm.job1 != null)
            {
                ca = getSavedAddressDetail(vm.ColAddressFinal);
                okColAdd = vm.job1!.PULocation != null && !string.IsNullOrEmpty(ca) && !ca.Equals("Tap to input details.", StringComparison.OrdinalIgnoreCase);

                da = getSavedAddressDetail(vm.DelAddressFinal);
                okDelAdd = vm.job1!.DLLocation != null && !string.IsNullOrEmpty(da) && !da.Equals("Tap to input details.", StringComparison.OrdinalIgnoreCase);

                ra = getSavedAddressDetail(vm.RtnAddressFinal);
                okRtnAdd = !vm.TwoWay || (vm.TwoWay && vm.job2 != null && vm.job2.DLLocation != null && !string.IsNullOrEmpty(ra) && !ra.Equals("Tap to input details.", StringComparison.OrdinalIgnoreCase));

                content1 = vm.ContentType1;
                okContent1 = !string.IsNullOrEmpty(vm.job1.DeliveryContents) && !string.IsNullOrEmpty(content1) && !content1.Equals("Select", StringComparison.OrdinalIgnoreCase);

                content2 = vm.ContentType2;
                okContent2 = !vm.TwoWay || 
                    (vm.job2 != null && !string.IsNullOrEmpty(vm.job2.DeliveryContents) && vm.TwoWay && !string.IsNullOrEmpty(content2) && !content2.Equals("Select", StringComparison.OrdinalIgnoreCase));

                ctime = vm.colDateTimeFinalDispText;
                okColTime = vm.job1!.ReadyDateTime != DateTime.MinValue && 
                    !string.IsNullOrEmpty(ctime) && !ctime.Equals("Select", StringComparison.OrdinalIgnoreCase);

                dtime = vm.dfbDateTimeFinalDispText;
                okDelTime = vm.job1!.FromDateTime != DateTime.MinValue && vm.job1!.ToDateTime != DateTime.MinValue && 
                    !string.IsNullOrEmpty(dtime) && !dtime.Equals("Select", StringComparison.OrdinalIgnoreCase);

                rtime = vm.rtndfbDateTimeFinalDispText;
                okRtnTime = !vm.TwoWay || 
                    (vm.job2 != null && vm.job2!.ReadyDateTime != DateTime.MinValue && vm.job2!.FromDateTime != DateTime.MinValue && vm.job2!.ToDateTime != DateTime.MinValue
                    && vm.TwoWay && !string.IsNullOrEmpty(rtime) && !rtime.Equals("Select", StringComparison.OrdinalIgnoreCase));

                expDisp1 = getExpTypeStr2(vm.exp1);
                okExp1 = !string.IsNullOrEmpty(expDisp1) && !expDisp1.Equals("TBA", StringComparison.OrdinalIgnoreCase);

                expDisp2 = getExpTypeStr2(vm.exp2);
                okExp2 = !vm.TwoWay || (vm.TwoWay && !string.IsNullOrEmpty(expDisp2) && !expDisp2.Equals("TBA", StringComparison.OrdinalIgnoreCase));

                if (!vm.TwoWay)
                {
                    okRtnAdd = true;
                    okContent2 = true;
                    okRtnTime = true;
                    okExp2 = true;
                }
            }

            ok = okColAdd && okDelAdd && okRtnAdd && okContent1 && okContent2 && okColTime && okDelTime && okRtnTime && okExp1 && okExp2 && okPrepaidBal;

            btnProceed.IsEnabled = ok;
            btnProceed.Style = ok ? (Style)Application.Current.Resources["bstyleOrangeSmall"] : (Style)Application.Current.Resources["bstyleDisabledSmall"];
        }
        catch (Exception e)
        {
            string s = e.Message;
        }
    }

    private string getSavedAddressDetail(XDelServiceRef.AddressStructure ads)
    {
        XDelServiceRef.ContactStructure? cts = null;
        String addv = "Tap to input details.";
        string postal = "";
        string blk = "";
        string unit = "";
        string street = "";
        string bldg = "";
        string company = "";
        string name = "";
        string tel = "";
        string mobile = "";

        string address0 = "";
        string address1 = "";
        string address2 = "";
        string address3 = "";
        string address4 = "";
        string address5 = "";

        try
        {
            if (ads != null && ads.Contacts != null && ads.Contacts.Length > 0)
                cts = ads.Contacts[0];

            if (ads != null && cts != null)
            {
                postal = !string.IsNullOrEmpty(ads.POSTALCODE) ? ads.POSTALCODE : "";
                blk = !string.IsNullOrEmpty(ads.BLOCK) ? ads.BLOCK + " " : "";
                unit = !string.IsNullOrEmpty(ads.UNIT) ? ads.UNIT + " " : "";
                street = !string.IsNullOrEmpty(ads.STREET) ? ads.STREET + " " : "";
                bldg = !string.IsNullOrEmpty(ads.BUILDING) ? ads.BUILDING : "";
                company = !string.IsNullOrEmpty(ads.COMPANY) ? ads.COMPANY : "";

                name = !string.IsNullOrEmpty(cts.NAME) ? cts.NAME : "";
                tel = !string.IsNullOrEmpty(cts.TEL) ? "Tel: " + cts.TEL : "";
                mobile = !string.IsNullOrEmpty(cts.MOBILE) ? "Mobile: " + cts.MOBILE : "";

                address0 = !string.IsNullOrEmpty(company) ? company + "\r\n" : "";
                address1 = blk + street + unit + bldg + (!string.IsNullOrEmpty(postal) ? ", " + postal : "") + "\r\n";
                address2 = !string.IsNullOrEmpty(name) ? name + ((!string.IsNullOrEmpty(tel) || !string.IsNullOrEmpty(mobile)) ? "\r\n" : "") : "";
                address3 = !string.IsNullOrEmpty(tel) ? tel : "";
                address3 += !string.IsNullOrEmpty(address3) && !string.IsNullOrEmpty(mobile) ? ", " + mobile : string.IsNullOrEmpty(address3) && !string.IsNullOrEmpty(mobile) ? mobile : "";

                addv = address0 + address1 + address2 + address3;
            }
        }
        catch (Exception e)
        {
            string s = e.Message;
        }
        return addv;
    }

    private void swOneTwoWayonToggle(object? sender, ToggledEventArgs e)
    {
        try
        {
            gridRtnAddress.IsVisible = e.Value;
            gridRtnPortion.IsVisible = e.Value;
            bvRtnAddress.IsVisible = e.Value;
            bvRtnSection.IsVisible = e.Value;

            if (vm == null)
                vm = (CreateJobVM)BindingContext;

            vm.TwoWay = e.Value;
            vm.ContentType2 = "";
            vm.exp2 = "";
            vm.expStr2 = "";

            vm.clearInvP2();
            vm.job2 = e.Value ? new JobInfo() : null;

            vm.pcs2 = 0;
            vm.kg2 = 0;
            vm.rtndfDateSelectedValue = "";
            vm.rtndfDateTimeSelectedValue = "";
            vm.rtndfTimeSelectedValue = "";
            vm.rtndfLunchPrevSelectedIDX = "";
            vm.rtndfLunchSelectedIDX = "";
            vm.rtndfDateLunchTimeValue = "";
            vm.rtndfDateTimeFinalValue = "";
            vm.rtndbTimeSelectedValue = "";
            vm.rtndbTimeSelectedActualValue = "";
            vm.rtndbDateTimeFinalValue = "";
            vm.rtndbExtByDateTimeValue = "";
            cvRFDate.ItemsSource = null;
            cvRFLunch.ItemsSource = null;
            cvRFTime.ItemsSource = null;
            cvRBTime.ItemsSource = null;

            lblRtnContent.Text = "Select";
            lblRtnInv.Text = "Add/Remove";
            lblRtnTime.Text = "Select";
            lblExpType2.Text = "TBA";

            validateButton();
        }
        catch (Exception ex)
        {
            string s = ex.Message;
        }
    }

    async Task<bool> addContentTypes()
    {
        try
        {
            int cvContentTypeCount = cvContentType.ItemsSource is List<contenttype> collection ? collection.Count : 0;

            if (cvContentTypeCount == 0)
            {
                /*List<contenttype> list = new List<contenttype>() {
                new() { dispText = "Document", value = "Document" },
                new() { dispText = "Dunning Document", value = "Dunning Document" },
                new() { dispText = "Parcel", value = "Parcel" },
                new() { dispText = "Light Parcel", value = "Light Parcel" },
                new() { dispText = "Medication", value = "Medication" },
                new() { dispText = "Sim Card", value = "Sim Card" }
            };

                cvContentType.ItemsSource = null;
                cvContentType.ItemsSource = list;*/

                XDelServiceRef.SettingsInfo def = await xs.GetDeliveryContentsDefinitionsAsync(logininfo!.clientInfo.Web_UID);
                if (def.Status == -1)
                {
                    await DisplayAlertAsync("Session expired", "Your Session Has Expired. Please login again.", "OK");
                    await common.BackToLogin();
                    return false;
                }
                else if (def.Status != 0 && def.Status != -1)
                {
                    await DisplayAlertAsync("Session expired", def.Message, "OK");
                    await common.BackToLogin();
                    return false;
                }

                string[] dc;
                string dcstr = "";
                List<contenttype> list = new List<contenttype>();

                if (def.DeliveryContents.Length > 0)
                {
                    dc = def.DeliveryContents;
                    for (int i = 0; i < dc.Length; i++)
                    {
                        dcstr = dc[i].ToString();
                        if (dcstr.ToLower() == "processing (visa/work permit)" || dcstr.ToLower() == "inbound parcel"
                        || dcstr.ToLower() == "quick chex" || dcstr.ToLower() == "processing (visa/work permit)" ||
                        dcstr.ToLower() == "clearance/transfer" || dcstr.ToLower() == "security bag")
                        {
                            continue;
                        }

                        list.Add(new() { dispText = dcstr, value = dcstr });
                    }

                    if (list.Count > 0)
                    {
                        cvContentType.ItemsSource = null;
                        cvContentType.ItemsSource = list;
                    } else
                    {
                        cvContentType.ItemsSource = null;
                        await DisplayAlertAsync("", "No valid Delivery Contents available.", "OK");
                        await common.BackToLogin();
                        return false;
                    }
                } else
                {
                    cvContentType.ItemsSource = null;
                    await DisplayAlertAsync("", "No valid Delivery Contents available.", "OK");
                    await common.BackToLogin();
                    return false;
                }
            }
        }
        catch (Exception ex)
        {
            cvContentType.ItemsSource = null;
            string s = ex.Message;
            await DisplayAlertAsync("Exception", s, "OK");
            BackToHomePage();
            return false;
        }
        return true;
    }

    void addContentTypes_()
    {
        try
        {
            int cvContentTypeCount = cvContentType.ItemsSource is List<contenttype> collection ? collection.Count : 0;

            if (cvContentTypeCount == 0)
            {
                List<contenttype> list = new List<contenttype>() {
                new() { dispText = "Document", value = "Document" },
                new() { dispText = "Dunning Document", value = "Dunning Document" },
                new() { dispText = "Parcel", value = "Parcel" },
                new() { dispText = "Light Parcel", value = "Light Parcel" },
                new() { dispText = "Medication", value = "Medication" },
                new() { dispText = "Sim Card", value = "Sim Card" }
            };

                cvContentType.ItemsSource = null;
                cvContentType.ItemsSource = list;
            }
        }
        catch (Exception ex)
        {
            string s = ex.Message;
        }
    }

    //void addExpTypes()
    async Task<bool> addExpTypes()
    {
        ////refers to XDelOnline loadExpressTypeDDL2
        /*fakeAllowedExpTypes = new List<common.eExpressType> {
            common.eExpressType.etThreeHour,
            common.eExpressType.etPriority,
            common.eExpressType.etGuaranteed,
            common.eExpressType.etNormal
        };*/

        XDelServiceRef.SettingsInfo CNSettingsInfo = null;
        XDelServiceRef.eExpressType[] allowedExp = null;
        XDelServiceRef.eExpressType[] availableExp = null;
        try
        {
            logininfo = AppSession.logininfo;
            if (logininfo!.ContactLvlSettingsInfo != null)
                CNSettingsInfo = logininfo!.ContactLvlSettingsInfo;
            if (CNSettingsInfo == null)
                return false;

            if (CNSettingsInfo.XDelOnlineSettings.AvailableExpressTypes.Length == 0)
            {
                availableExp = CNSettingsInfo.XDelOnlineSettings.AllowedExpressTypes;
                allowedExp = CNSettingsInfo.XDelOnlineSettings.AllowedExpressTypes;
            } else
            {
                availableExp = CNSettingsInfo.XDelOnlineSettings.AvailableExpressTypes;
                allowedExp = CNSettingsInfo.XDelOnlineSettings.AllowedExpressTypes;
            }

            if (allowedExp == null || allowedExp.Count() == 0)
            {
                if (CNSettingsInfo.XDelOnlineSettings.CanEnterJobs)
                {
                    if (logininfo!.ClientXDelOnlineSettings != null)
                    {
                        if (logininfo.ClientXDelOnlineSettings.AvailableExpressTypes.Length == 0)
                        {
                            availableExp = logininfo.ClientXDelOnlineSettings.AllowedExpressTypes;
                            allowedExp = logininfo.ClientXDelOnlineSettings.AllowedExpressTypes;
                        }
                        else
                        {
                            availableExp = logininfo.ClientXDelOnlineSettings.AvailableExpressTypes;
                            allowedExp = logininfo.ClientXDelOnlineSettings.AllowedExpressTypes;
                        }

                        if (allowedExp == null || allowedExp.Count() == 0)
                        {
                            await DisplayAlertAsync("", "Your account is not set up for creating jobs through the portal.\nPlease contact your account manager for more information.", "OK");
                            BackToHomePage();
                            return false;
                        }
                    }
                    else
                    {
                        await DisplayAlertAsync("", "Your account is not set up for creating jobs through the portal.\nPlease contact your account manager for more information.", "OK");
                        BackToHomePage();
                        return false;
                    }
                } else
                {
                    await DisplayAlertAsync("", "Your account is not set up for creating jobs through the portal.\nPlease contact your account manager for more information.", "OK");
                    BackToHomePage();
                    return false;
                }
            }

            List<string> sortedExp = new List<string>();
            int FiveThirtyPlusIndex = -1;
            fakeAllowedExpTypes = new List<XDelServiceRef.eExpressType>();

            for (int i = 0; i <= allowedExp.Length - 1; i++)
            {
                int expAllowedisInsideAvailable = Array.IndexOf(availableExp, allowedExp[i]);

                //Add all allowed EXP TYPE to sortedExp. (Including 1.5 and 2.5) This is needed to show PU and DEL timing later on if they are allowed for 1.5 and 2.5 as per requirement set by management.
                if (expAllowedisInsideAvailable > -1)
                {
                    fakeAllowedExpTypes.Add(allowedExp[i]);
                    sortedExp.Add(common.convertExpressType(allowedExp[i]));
                    if (allowedExp[i] == eExpressType.etNormal)
                    {
                        FiveThirtyPlusIndex = i;
                    }
                }
            }

            //SORT EXPRESS TYPES BY THIS ORDER: 1.5, 2.5, 3.5, 4:30, 5:30, 5:30+ (Move 5:30+ to the end of array.)
            if (sortedExp.Contains("Five Thirty Plus") && FiveThirtyPlusIndex != -1)
            {
                fakeAllowedExpTypes.Add(fakeAllowedExpTypes[FiveThirtyPlusIndex]);
                fakeAllowedExpTypes.RemoveAt(FiveThirtyPlusIndex);
                sortedExp.RemoveAt(FiveThirtyPlusIndex);
                sortedExp.Add("Five Thirty Plus");
            }
        } catch (Exception e)
        {
            string s = e.Message;
            await DisplayAlertAsync("Exception", s, "OK");
            BackToHomePage();
            return false;
        }
        return true;
    }

    void addExpTypes_()
    {
        ////refers to XDelOnline loadExpressTypeDDL2
        fakeAllowedExpTypes = new List<XDelServiceRef.eExpressType> {
            //XDelServiceRef.eExpressType.etOneHour,
            //XDelServiceRef.eExpressType.etTwoHour,
            XDelServiceRef.eExpressType.etThreeHour,
            XDelServiceRef.eExpressType.etPriority,
            XDelServiceRef.eExpressType.etGuaranteed,
            XDelServiceRef.eExpressType.etNormal
        };
    }


    //==================================================================================================================================

    #region Addresses

    async void show_Address_BS(int type)
    {
        try
        {
            vm.AddressMode = type;
            vm.selectedLocationType = 0;
            string v = type == 1 ? "Collection Address" : type == 2 ? "Delivery Address" : "Return Address";
            if (type == 1)
            {
                vm.ColAddress = vm.ColAddressFinal;
            } else if (type == 2)
            {
                vm.DelAddress = vm.DelAddressFinal;
            } else
            {
                vm.RtnAddress = vm.RtnAddressFinal;
            }

            await Shell.Current.GoToAsync("AddressPage", new Dictionary<string, object>
                    {
                        { "vmm", vm }, 
                        { "titleName", v },
                        {"LOGININFO",  logininfo},
                        { "LTR", ltr }
                    });
        }
        catch (Exception ex)
        {
            string s = ex.Message;
        }
    }

    #endregion

    //==================================================================================================================================

    #region ContentType

    async void show_ContentType_BS(System.Object sender, System.EventArgs e)
    {
        try
        {
            if (vm == null)
                vm = (CreateJobVM)BindingContext;

            BSContentType.IsVisible = true;
            vm.BSContentType2opened = false;
            removeTapGestureRecognizer();
            addContentTypes();

            preSelectContentType();
            // Scroll to the top of the ScrollView
            await svLWH.ScrollToAsync(0, 0, false);

            await BSContentType.OpenBottomSheet(false, false);
            BSContentType.isShowing = true;

        }
        catch (Exception ex)
        {
            string s = ex.Message;
        }
    }

    async void show_ContentType_BS2(System.Object sender, System.EventArgs e)
    {
        try
        {
            if (vm == null)
                vm = (CreateJobVM)BindingContext;

            BSContentType.IsVisible = true;
            vm.BSContentType2opened = true;
            removeTapGestureRecognizer();
            addContentTypes();

            preSelectContentType();
            // Scroll to the top of the ScrollView
            await svLWH.ScrollToAsync(0, 0, false);

            await BSContentType.OpenBottomSheet(false, false);
            BSContentType.isShowing = true;

        }
        catch (Exception ex)
        {
            string s = ex.Message;
        }
    }

    private void cvContentTypeOnItemSelected(object sender, SelectionChangedEventArgs e)
    {
        try
        {
            // Clear previous
            if (cvContentType.ItemsSource is List<contenttype> all)
                foreach (var d in all) d.IsSelected = false;

            var selectedItem = e.CurrentSelection.FirstOrDefault() as contenttype;
            if (selectedItem != null)
            {
                selectedItem.IsSelected = true;

                string v = selectedItem.value;
                if (!vm.BSContentType2opened)
                    vm.ContentType1 = v;
                    
                if (vm.BSContentType2opened)
                    vm.ContentType2 = v;
                
                gridLWH.IsVisible = (v.Equals("Parcel") || v.Equals("Light Parcel"));
                lblWeightRemarksParcel.IsVisible = (v.Equals("Parcel") || v.Equals("Light Parcel"));
                lblWeightRemarksDoc.IsVisible = (!v.Equals("Parcel") && !v.Equals("Light Parcel"));
                if (v.Equals("Parcel") || v.Equals("Light Parcel"))
                {
                    txtLength.Text = "1";
                    txtWidth.Text = "1";
                    txtHeight.Text = "1";
                }
            }
        }
        catch (Exception ex)
        {
            string s = ex.Message;
        }
    }

    void swColdonToggle(object sender, ToggledEventArgs e)
    {
        try
        {
            if (!vm.BSContentType2opened)
                vm.reqCold1 = e.Value;
            if (vm.BSContentType2opened)
                vm.reqCold2 = e.Value;
        }
        catch (Exception ex)
        {
            string s = ex.Message;
        }
    }

    void ContentTypeSaveUponClose()
    {
        try
        {
            string L = txtLength.Text;
            string W = txtWidth.Text;
            string H = txtHeight.Text;
            string PCS = txtPcs.Text;
            string KG = txtWt.Text;

            if (!vm.BSContentType2opened)
            {
                lblContent.Text = vm.ContentType1;
                vm.reqCold1 = swCold.IsToggled;
                vm.pcs1 = String.IsNullOrEmpty(PCS) ? 1 : int.Parse(PCS);
                vm.kg1 = String.IsNullOrEmpty(KG) ? 1 : int.Parse(KG);
                vm.length1 = String.IsNullOrEmpty(L) ? 1 : int.Parse(L);
                vm.width1 = String.IsNullOrEmpty(W) ? 1 : int.Parse(W);
                vm.height1 = String.IsNullOrEmpty(H) ? 1 : int.Parse(H);

                vm.job1.DeliveryContents = vm.ContentType1;
                vm.job1.Pieces = vm.pcs1;
                vm.job1.Weight = vm.kg1;                
                vm.job1.SpecialInstructions = vm.ContentType1.ToLower().Contains("parcel") ? 
                    "L" + vm.length1 + " X W" + vm.width1 + " X H" + vm.height1 + " CM" : "";
                int fv = vm.job1.FLAG2;
                int colditemsuppflag = vm.reqCold1 ? 64 : 0;
                fv = fv | colditemsuppflag;
                vm.job1.FLAG2 = fv;
            }
            else
            {
                lblRtnContent.Text = vm.ContentType2;
                vm.reqCold2 = swCold.IsToggled;
                vm.pcs2 = String.IsNullOrEmpty(PCS) ? 1 : int.Parse(PCS);
                vm.kg2 = String.IsNullOrEmpty(KG) ? 1 : int.Parse(KG);
                vm.length2 = String.IsNullOrEmpty(L) ? 1 : int.Parse(L);
                vm.width2 = String.IsNullOrEmpty(W) ? 1 : int.Parse(W);
                vm.height2 = String.IsNullOrEmpty(H) ? 1 : int.Parse(H);

                vm.job2.DeliveryContents = vm.ContentType2;
                vm.job2.Pieces = vm.pcs2;
                vm.job2.Weight = vm.kg2;
                vm.job2.SpecialInstructions = vm.ContentType2.ToLower().Contains("parcel") ?
                    "L" + vm.length2 + " X W" + vm.width2 + " X H" + vm.height2 + " CM" : "";
                int fv = vm.job2.FLAG2;
                int colditemsuppflag = vm.reqCold2 ? 64 : 0;
                fv = fv | colditemsuppflag;
                vm.job2.FLAG2 = fv;
            }
        }
        catch (Exception ex)
        {
            string s = ex.Message;
        }
    }

    void preSelectContentType()
    {
        try
        {
            string selectedvalue = "";
            int itemCount = cvContentType.ItemsSource is List<contenttype> collection ? collection.Count : 0;
            selectedvalue = !vm.BSContentType2opened ? vm.job1.DeliveryContents : vm.job2.DeliveryContents;

            if (itemCount > 0 && (cvContentType.ItemsSource is List<contenttype> items))
            {
                //selectedvalue = !vm.BSContentType2opened ? vm.ContentType1 : vm.ContentType2;
                
                if (string.IsNullOrEmpty(selectedvalue) || selectedvalue.Equals("Select"))
                    selectedvalue = "";
                if (!String.IsNullOrEmpty(selectedvalue))
                {
                    contenttype item;
                    for (int i = 0; i <= items.Count - 1; i++)
                    {
                        item = items[i];
                        if (item.value.Equals(selectedvalue))
                        {
                            cvContentType.SelectedItem = items[i];
                            cvContentType.ScrollTo(item, position: ScrollToPosition.MakeVisible, animate: false);
                            break;
                        }
                    }
                }
                else
                {
                    cvContentType.SelectedItem = items[0];
                    cvContentType.ScrollTo(items[0], position: ScrollToPosition.MakeVisible, animate: false);
                    if (!vm.BSContentType2opened)
                    {
                        vm.ContentType1 = items[0].value;
                    }
                        

                    if (vm.BSContentType2opened)
                    {
                        vm.ContentType2 = items[0].value;
                    }
                }
            }

            /*string L = (!vm.BSContentType2opened) ? vm.length1.ToString() : vm.length2.ToString();
            string W = (!vm.BSContentType2opened) ? vm.width1.ToString() : vm.width2.ToString();
            string H = (!vm.BSContentType2opened) ? vm.height1.ToString() : vm.height2.ToString();
            string PCS = (!vm.BSContentType2opened) ? vm.pcs1.ToString() : vm.pcs2.ToString();
            string KG = (!vm.BSContentType2opened) ? vm.kg1.ToString() : vm.kg2.ToString();
            bool reqcold = (!vm.BSContentType2opened) ? vm.reqCold1 : vm.reqCold2;*/

            if (!string.IsNullOrEmpty(selectedvalue) && selectedvalue.ToLower().Contains("parcel"))
            {
                string L = (!vm.BSContentType2opened) ? vm.length1.ToString() : vm.length2.ToString();
                string W = (!vm.BSContentType2opened) ? vm.width1.ToString() : vm.width2.ToString();
                string H = (!vm.BSContentType2opened) ? vm.height1.ToString() : vm.height2.ToString();

                string SpecialInstruction = (!vm.BSContentType2opened) ? vm.job1.SpecialInstructions : vm.job2.SpecialInstructions;
                if (SpecialInstruction.Trim() != "")
                {
                    string input = SpecialInstruction.Substring(0, SpecialInstruction.Length - 2);
                    string[] stringsplit = input.Split('X');
                    
                    string result = stringsplit[0].Trim();
                    result = result.ToLower().Replace("l", "").Trim();

                    string result2 = stringsplit[1].Trim();
                    result2 = result2.ToLower().Replace("w", "").Trim();

                    string result3 = stringsplit[2].Trim();
                    result3 = result3.ToLower().Replace("h", "").Trim();

                    L = result;
                    W = result2;
                    H = result3;
                }

                txtLength.Text = !String.IsNullOrEmpty(L) ? L : "1";
                txtWidth.Text = !String.IsNullOrEmpty(W) ? W : "1";
                txtHeight.Text = !String.IsNullOrEmpty(H) ? H : "1";
            }

            string PCS = (!vm.BSContentType2opened) ? vm.job1.Pieces.ToString() : vm.job2.Pieces.ToString();
            string KG = (!vm.BSContentType2opened) ? vm.job1.Weight.ToString() : vm.job2.Weight.ToString();
            bool reqcold = (!vm.BSContentType2opened) ? (vm.job1.FLAG2 & 64) == 64 : (vm.job2.FLAG2 & 64) == 64;

            txtPcs.Text = !String.IsNullOrEmpty(PCS) ? PCS : "1";
            txtWt.Text = !String.IsNullOrEmpty(KG) ? KG : "1";
            swCold.IsToggled = reqcold;
        }
        catch (Exception ex)
        {
            string s = ex.Message;
        }
    }

    async void CloseBSContentType_Clicked(object sender, EventArgs e)
    {
        try
        {
            addTapGestureRecognizer();

            //await BSContentType.CloseBottomSheet();
            //BSContentType.isShowing = false;
            await CloseBottomSheetSafely(BSContentType);
        }
        catch (Exception ex)
        {
            string s = ex.Message;
        }
    }

    async void CloseBSContentTypeSave_Clicked(System.Object sender, System.EventArgs e)
    {
        try
        {
            addTapGestureRecognizer();
            ContentTypeSaveUponClose();

            //await BSContentType.CloseBottomSheet();
            //BSContentType.isShowing = false;
            await CloseBottomSheetSafely(BSContentType);
            validateButton();
        }
        catch (Exception ex)
        {
            string s = ex.Message;
        }
    }

    #endregion

    string getExpTypeStr2(string et)
    {
        string ets = "TBA";
        try
        {
            //string et = vm.BSDeliveryTimeopened ? vm.exp1 : vm.exp2; ;

            switch (et)
            {
                case "etNormal":
                    return "Five Thirty Plus";
                case "etHalfHour":
                    return "1 Hour";
                case "etOneHour":
                    return "1.5 Hours";
                case "etTwoHour":
                    return "2.5 Hours";
                case "etThreeHour":
                    return "3.5 Hours";
                case "etPriority":
                    return "Four Thirty";
                case "etGuaranteed":
                    return "Five Thirty";
                case "etAOH":
                    return "After Office Hours";
                case "etWeekend":
                    return "Weekend";
                case "etTwoDays":
                    return "Two Days";
                case "etThreeDays":
                    return "Three Days";
                default:
                    return "TBA";
            }
        }
        catch (Exception ex)
        {
            string s = ex.Message;
        }
        return ets;
    }

    string getExpTypeStr()
    {
        string ets = "TBA";
        try
        {
            string et = vm.BSDeliveryTimeopened ? vm.exp1 : vm.exp2; ;
            //ets = et.Equals(common.eExpressType.etNormal.ToString()) ? "FIVE THIRTY PLUS" :

            switch (et)
            {
                case "etNormal":
                    return "Five Thirty Plus";
                case "etHalfHour":
                    return "1 Hour";
                case "etOneHour":
                    return "1.5 Hours";
                case "etTwoHour":
                    return "2.5 Hours";
                case "etThreeHour":
                    return "3.5 Hours";
                case "etPriority":
                    return "Four Thirty";
                case "etGuaranteed":
                    return "Five Thirty";
                case "etAOH":
                    return "After Office Hours";
                case "etWeekend":
                    return "Weekend";
                case "etTwoDays":
                    return "Two Days";
                case "etThreeDays":
                    return "Three Days";
                default:
                    return "TBA";
            }
        }
        catch (Exception ex)
        {
            string s = ex.Message;
        }
        return ets;
    }

    List<string> getLunchTimeTimesListNew(string value)
    {
        List<string> list = new List<string>();
        string selectedvalue = value;
        try
        {
            if (!String.IsNullOrEmpty(selectedvalue))
            {
                if (selectedvalue.Equals("0"))
                {

                }
                else if (selectedvalue.Equals("4"))
                {
                    list = new List<string>() { "11:00", "11:30", "12:00" };
                }
                else if (selectedvalue.Equals("3"))
                {
                    list = new List<string>() { "11:30", "12:00", "12:30" };
                }
                else if (selectedvalue.Equals("2"))
                {
                    list = new List<string>() { "11:30", "12:00", "12:30", "13:00", "13:30" };
                }
                else
                {
                    list = new List<string>() { "11:30", "12:00", "12:30", "13:00", "13:30" };
                }
            }

        }
        catch (Exception e)
        {
            string s = e.Message;
        }
        return list;
    }

    void addLunchTime(CollectionView view, int type)
    {
        try
        {
            List<lunchtime> list = new List<lunchtime>() {
                new() { dispText = "None", value = "0" },
                new() { dispText = "11:30 AM to\r\n12:30 PM", value = "4" },
                new() { dispText = "12:00 PM to\r\n1:00 PM", value = "3" },
                new() { dispText = "12:00 PM to\r\n2:00 PM", value = "2" },
                new() { dispText = "1:00 PM to\r\n2:00 PM", value = "1" },
            };

            view.ItemsSource = null;
            view.ItemsSource = list;
            //view.SelectedItem = list[0];            
        }
        catch (Exception ex)
        {
            string s = ex.Message;
        }
    }

    TimeSpan convertStringToTimeSpan(string v)
    {
        TimeSpan ts = new TimeSpan(0, 0, 0);
        try
        {
            int sHr = Int32.Parse(v.Substring(0, 2));
            int sMin = Int32.Parse(v.Substring(3, 2));
            ts = new TimeSpan(sHr, sMin, 0);
        }
        catch (Exception ex)
        {
            string s = ex.Message;
        }
        return ts;
    }

    //==================================================================================================================================

    #region CollectionTime

    async void show_CollectionTime_BS(System.Object sender, System.EventArgs e)
    {
        try
        {
            if (vm == null)
                vm = (CreateJobVM)BindingContext;

            int cvDateCount = cvDate.ItemsSource is List<date> collection ? collection.Count : 0;
            if (cvDateCount == 0)
            {
                addColDateList();
            }

            preSelectColDate2();
            // Scroll to the top of the ScrollView
            await svColTime.ScrollToAsync(0, 0, false);

            BSCollectionTime.IsVisible = true;
            removeTapGestureRecognizer();

            await BSCollectionTime.OpenBottomSheet(false);
            BSCollectionTime.isShowing = true;

        }
        catch (Exception ex)
        {
            string s = ex.Message;
        }
    }

    void addColDateList()
    {
        try
        {
            cvDate.ItemsSource = null;
            date d;
            List<date> list = new List<date>();
            DateTime nw = DateTime.Now;
            string dateddmm = "";
            String dow = nw.ToString("ddd");
            string formattedDate = nw.ToString("dd/MM/yyyy");
            TimeSpan ts = new TimeSpan(17, 0, 0);

            for (int i = 1; i <= 14; i++)
            {
                if (i > 1)
                {
                    //nw = nw.AddDays(1);
                    do
                    {
                        nw = nw.AddDays(1);
                    } /*while (v.vIsHoliday(nw.Date) || (nw.DayOfWeek == DayOfWeek.Saturday || nw.DayOfWeek == DayOfWeek.Sunday));*/
                    while ((nw.DayOfWeek == DayOfWeek.Saturday || nw.DayOfWeek == DayOfWeek.Sunday));
                }

                dow = nw.ToString("ddd");
                //dateV = nw.Day;
                dateddmm = nw.ToString("dd/MM");
                formattedDate = nw.ToString("dd/MM/yyyy");


                if (i == 1 && nw.TimeOfDay > ts)
                {
                    //now time already passed 1700, add 1 day, set ts to 9am
                    i--;
                    //nw = nw.AddDays(1);
                    //nw = nw.Date + new TimeSpan(9, 0, 0);

                    do
                    {
                        nw = nw.AddDays(1);
                    } /*while (v.vIsHoliday(nw.Date) || (nw.DayOfWeek == DayOfWeek.Saturday || nw.DayOfWeek == DayOfWeek.Sunday));*/
                    while ((nw.DayOfWeek == DayOfWeek.Saturday || nw.DayOfWeek == DayOfWeek.Sunday));

                    nw = nw.Date + new TimeSpan(9, 0, 0);
                }
                else
                {
                    d = new date() { dispText = dow + "\n" + dateddmm, formattedDate = formattedDate };
                    list.Add(d);
                }
            }

            cvDate.ItemsSource = list;

            int itemCount = cvDate.ItemsSource is List<date> collection ? collection.Count : 0;

            if (String.IsNullOrEmpty(vm.colDateSelectedValue) && itemCount > 0)
            {
                if (cvDate.ItemsSource is List<date> items)
                {
                    // Select the first item (index 0)
                    cvDate.SelectedItem = items[0];
                }
            }
        }
        catch (Exception ex)
        {
            string s = ex.Message;
        }
    }

    private void cvDateOnItemSelected(object sender, SelectionChangedEventArgs e)
    {
        try
        {
            // Clear previous
            if (cvDate.ItemsSource is List<date> all)
                foreach (var d in all) d.IsSelected = false;

            var selectedItem = e.CurrentSelection.FirstOrDefault() as date;
            if (selectedItem != null)
            {
                selectedItem.IsSelected = true;

                vm.colDateSelectedValue = selectedItem.formattedDate;

                if (!String.IsNullOrEmpty(vm.colDatePrevSelectedValue) && !vm.colDatePrevSelectedValue.Equals(vm.colDateSelectedValue))
                {
                    vm.colLunchPrevSelectedIDX = "";
                    vm.colLunchSelectedIDX = "";
                }

                addLunchTime(cvColLunch, 1);
                if (cvColLunch.ItemsSource != null)
                {
                    int idx = !String.IsNullOrEmpty(vm.colLunchSelectedIDX) ? int.Parse(vm.colLunchSelectedIDX) : 0;
                    cvColLunch.SelectedItem = ((List<lunchtime>)cvColLunch.ItemsSource)[idx];
                    cvColLunch.ScrollTo(idx, position: ScrollToPosition.MakeVisible, animate: false);

                }
            }
        }
        catch (Exception ex)
        {
            string s = ex.Message;
        }
    }

    private void cvColLunchOnItemSelected(object sender, SelectionChangedEventArgs e)
    {
        try
        {
            // Clear previous
            if (cvColLunch.ItemsSource is List<lunchtime> all)
                foreach (var d in all) d.IsSelected = false;

            string selectedDateStr = vm.colDateSelectedValue;
            var selectedItem = e.CurrentSelection.FirstOrDefault() as lunchtime;
            if (selectedItem != null)
            {
                selectedItem.IsSelected = true;

                int selectedIndex = cvColLunch.ItemsSource.Cast<lunchtime>()
                                 .ToList().IndexOf(selectedItem);
                vm.colLunchSelectedIDX = selectedIndex.ToString();
                vm.colDateLunchTimeValue = selectedItem.value;

                if (!String.IsNullOrEmpty(selectedItem.value))
                    addColTimeList();
            }
        }
        catch (Exception ex)
        {
            string s = ex.Message;
        }
    }

    void addColTimeList()
    {
        try
        {
            cvTime.ItemsSource = null;
            DateTime nw = DateTime.Now;
            DateTime curr = DateTime.Now.Date;
            string selectedDateStr = vm.colDateSelectedValue;
            DateTime selectedDate = DateTime.ParseExact(selectedDateStr, dateformats, CultureInfo.InvariantCulture, DateTimeStyles.None);
            TimeSpan ts = new TimeSpan(9, 0, 0);
            int sHr = 0;
            int sMin = 0;
            DateTime dt = selectedDate;
            time t;
            List<time> list = new List<time>();
            string val = "";
            string valdispText = "";
            DateTime dispDT = DateTime.Now;
            List<string> lunchhours = getLunchTimeTimesListNew(vm.colDateLunchTimeValue);
            List<string> timesL = times.ToList();
            string[] timesR = null;


            if (lunchhours != null && lunchhours.Count > 0)
            {
                for (int i = timesL.Count - 1; i >= 0; i--)
                {
                    if (lunchhours.Contains(timesL[i]))
                    {
                        timesL.RemoveAt(i);
                    }
                }
            }

            timesR = timesL.ToArray();

            if (selectedDate == curr)
            {
                for (int i = 0; i <= timesR.Length - 1; i++)
                {
                    sHr = Int32.Parse(timesR[i].Substring(0, 2));
                    sMin = Int32.Parse(timesR[i].Substring(3, 2));
                    dt = selectedDate.Date.AddHours(sHr).AddMinutes(sMin);
                    ts = new TimeSpan(sHr, sMin, 0);
                    if (nw < dt)
                    {
                        dispDT = dispDT.Date + ts;
                        valdispText = dispDT.ToString("h:mm tt", CultureInfo.InvariantCulture).ToUpper();
                        val = timesR[i];
                        t = new time() { dispText = valdispText, value = val, IsVisible = true };
                        list.Add(t);
                    }
                }
            }
            else if (selectedDate > curr)
            {
                for (int i = 0; i <= timesR.Length - 1; i++)
                {

                    sHr = Int32.Parse(timesR[i].Substring(0, 2));
                    sMin = Int32.Parse(timesR[i].Substring(3, 2));
                    ts = new TimeSpan(sHr, sMin, 0);
                    dispDT = dispDT.Date + ts;
                    valdispText = dispDT.ToString("h:mm tt", CultureInfo.InvariantCulture).ToUpper();
                    val = timesR[i];
                    t = new time() { dispText = valdispText, value = val, IsVisible = true };
                    list.Add(t);
                }
            }

            cvTime.ItemsSource = list;
            cvTime.SelectedItem = list[0];

        }
        catch (Exception ex)
        {
            string s = ex.Message;
        }
    }

    private void cvTimeOnItemSelected(object sender, SelectionChangedEventArgs e)
    {
        try
        {
            // Clear previous
            if (cvTime.ItemsSource is List<time> all)
                foreach (var d in all) d.IsSelected = false;

            var selectedItem = e.CurrentSelection.FirstOrDefault() as time;
            if (selectedItem != null)
            {
                selectedItem.IsSelected = true;

                vm.ReadyTimeSelectedValue = selectedItem.value;
                setColDateTime();
            }
        }
        catch (Exception ex)
        {
            string s = ex.Message;
        }
    }

    void setColDateTime()
    {
        try
        {
            string selectedDateStr = vm.colDateSelectedValue; //"dd/MM/yyyy"
            DateTime selectedDate = DateTime.ParseExact(selectedDateStr, dateformats, CultureInfo.InvariantCulture, DateTimeStyles.None);
            string selectedReadyTimeStr = vm.ReadyTimeSelectedValue;
            int sHr = Int32.Parse(selectedReadyTimeStr.Substring(0, 2));
            int sMin = Int32.Parse(selectedReadyTimeStr.Substring(3, 2));
            selectedDate = selectedDate.AddHours(sHr).AddMinutes(sMin);
            vm.colDateTimeFinalValue = selectedDate.ToString("dd/MM/yyyy HH:mm");
            vm.colDateTimeFinalDispText = selectedDate.ToString("dd/MM/yyyy h:mm tt", CultureInfo.InvariantCulture).ToUpper();
            
            lblColTime.Text = vm.colDateTimeFinalDispText;
        }
        catch (Exception ex)
        {
            string s = ex.Message;
        }
    }

    async void CloseBSCollectionTime_Clicked(object sender, EventArgs e)
    {
        try
        {
            addTapGestureRecognizer();

            //await BSCollectionTime.CloseBottomSheet();
            //BSCollectionTime.isShowing = false;
            await CloseBottomSheetSafely(BSCollectionTime);
        }
        catch (Exception ex)
        {
            string s = ex.Message;
        }
    }

    async void CloseBSCollectionTimeSave_Clicked(System.Object sender, System.EventArgs e)
    {
        try
        {
            addTapGestureRecognizer();
            CollectionTimeSaveUponClose();

            if (fakeAllowedExpTypes == null || (fakeAllowedExpTypes != null && fakeAllowedExpTypes.Count == 0))
                if (!await addExpTypes()) return;
            

            int cvDFDateCount = cvDFDate.ItemsSource is List<date> collection ? collection.Count : 0;
            if (cvDFDateCount == 0)
                addDelFromDateList();

            //await BSCollectionTime.CloseBottomSheet();
            //BSCollectionTime.isShowing = false;
            await CloseBottomSheetSafely(BSCollectionTime);
            validateButton();
        }
        catch (Exception ex)
        {
            string s = ex.Message;
        }
    }

    void CollectionTimeSaveUponClose()
    {
        try
        {
            bool colDateChange = String.IsNullOrEmpty(vm.colDatePrevSelectedValue) || (!String.IsNullOrEmpty(vm.colDatePrevSelectedValue) && !vm.colDatePrevSelectedValue.Equals(vm.colDateSelectedValue));
            bool colLunchChange = !String.IsNullOrEmpty(vm.colLunchPrevSelectedIDX) && !vm.colLunchPrevSelectedIDX.Equals(vm.colLunchSelectedIDX);
            bool ReadyTimeChange = String.IsNullOrEmpty(vm.ReadyTimePrevSelectedValue) || (!String.IsNullOrEmpty(vm.ReadyTimePrevSelectedValue) && !vm.ReadyTimePrevSelectedValue.Equals(vm.ReadyTimeSelectedValue));


            vm.job1.ReadyDateTime = DateTime.ParseExact(vm.colDateTimeFinalValue, dateformats, CultureInfo.InvariantCulture, DateTimeStyles.None);
            vm.job1.PickByDateTime = vm.job1.ReadyDateTime;

            vm.job1.PULunch_Avoid = !vm.colDateLunchTimeValue.Equals("0");
            vm.job1.PULunch_From = vm.colDateLunchTimeValue.Equals("1") ? vm.job1.ReadyDateTime.Date + new TimeSpan(13, 0, 0) :
                    vm.colDateLunchTimeValue.Equals("2") ? vm.job1.ReadyDateTime.Date + new TimeSpan(12, 0, 0) :
                    vm.colDateLunchTimeValue.Equals("3") ? vm.job1.ReadyDateTime.Date + new TimeSpan(12, 0, 0) :
                    vm.colDateLunchTimeValue.Equals("4") ? vm.job1.ReadyDateTime.Date + new TimeSpan(11, 30, 0) :
                    DateTime.MinValue;

            vm.job1.PULunch_To = vm.colDateLunchTimeValue.Equals("1") ? vm.job1.ReadyDateTime.Date + new TimeSpan(14, 0, 0) :
                    vm.colDateLunchTimeValue.Equals("2") ? vm.job1.ReadyDateTime.Date + new TimeSpan(14, 0, 0) :
                    vm.colDateLunchTimeValue.Equals("3") ? vm.job1.ReadyDateTime.Date + new TimeSpan(13, 0, 0) :
                    vm.colDateLunchTimeValue.Equals("4") ? vm.job1.ReadyDateTime.Date + new TimeSpan(12, 30, 0) :
                    DateTime.MinValue;

            if (colDateChange || colLunchChange || ReadyTimeChange)
            {
                vm.job1.FromDateTime = DateTime.MinValue;
                vm.job1.ToDateTime = DateTime.MinValue;
                vm.job1.ExtFromDateTime = DateTime.MinValue;
                vm.job1.ExtDateTime = DateTime.MinValue;
                vm.job1.DLLunch_From = DateTime.MinValue;
                vm.job1.DLLunch_To = DateTime.MinValue;
                vm.job1.DLLunch_Avoid = false;

                if (vm.job2 != null)
                {
                    vm.job2.ReadyDateTime = DateTime.MinValue;
                    vm.job2.PickByDateTime = DateTime.MinValue;
                    vm.job2.PULunch_Avoid = false;
                    vm.job2.PULunch_From = DateTime.MinValue;
                    vm.job2.PULunch_To = DateTime.MinValue;
                    vm.job2.FromDateTime = DateTime.MinValue;
                    vm.job2.ToDateTime = DateTime.MinValue;
                    vm.job2.ExtFromDateTime = DateTime.MinValue;
                    vm.job2.ExtDateTime = DateTime.MinValue;
                    vm.job2.DLLunch_From = DateTime.MinValue;
                    vm.job2.DLLunch_To = DateTime.MinValue;
                    vm.job2.DLLunch_Avoid = false;
                }

                vm.dfDatePrevSelectedValue = "";
                vm.dfDateSelectedValue = "";
                vm.dfDateLunchTimeValue = "";
                vm.dfLunchPrevSelectedIDX = "";
                vm.dfTimePrevSelectedValue = "";
                vm.dfTimeSelectedValue = "";
                vm.dfDateTimeFinalValue = "";
                vm.dbTimeSelectedValue = "";
                vm.dbTimeSelectedActualValue = "";
                vm.dbDateTimeFinalValue = "";
                vm.dbExtByDateTimeValue = "";
                cvDFDate.ItemsSource = null;
                cvDFLunch.ItemsSource = null;
                cvDFTime.ItemsSource = null;
                cvDBTime.ItemsSource = null;

                vm.dfbDateTimeFinalDispText = "Select";
                lblDelTime.Text = "Select";
                lblExpType.Text = "TBA";

                vm.rtndfDateSelectedValue = "";
                vm.rtndfDateTimeSelectedValue = "";
                vm.rtndfTimeSelectedValue = "";
                vm.rtndfDateLunchTimeValue = "";
                vm.rtndfLunchPrevSelectedIDX = "";
                vm.rtndfDateTimeFinalValue = "";
                vm.rtndbTimeSelectedValue = "";
                vm.rtndbTimeSelectedActualValue = "";
                vm.rtndbDateTimeFinalValue = "";
                vm.rtndbExtByDateTimeValue = "";
                cvRFDate.ItemsSource = null;
                cvRFLunch.ItemsSource = null;
                cvRFTime.ItemsSource = null;
                cvRBTime.ItemsSource = null;

                vm.rtndfbDateTimeFinalDispText = "Select";
                lblRtnTime.Text = "Select";
                lblExpType2.Text = "TBA";
            }

            vm.colDatePrevSelectedValue = vm.colDateSelectedValue;
            vm.colLunchPrevSelectedIDX = vm.colLunchSelectedIDX;
            vm.ReadyTimePrevSelectedValue = vm.ReadyTimeSelectedValue;

            lblColTime.Text = vm.colDateTimeFinalDispText;
        }
        catch (Exception e)
        {
            string s = e.Message;
        }
    }

    void preSelectColDate2()
    {
        try
        {
            string selectedvalue = "";
            int itemCount = 0;
            string coldatetimeStr = vm.colDateTimeFinalValue;
            DateTime coldatetime = DateTime.MinValue;

            if (!String.IsNullOrEmpty(coldatetimeStr))
                coldatetime = DateTime.ParseExact(coldatetimeStr, dateformats, CultureInfo.InvariantCulture, DateTimeStyles.None);

            itemCount = cvDate.ItemsSource is List<date> collection ? collection.Count : 0;
            if (itemCount > 0 && (cvDate.ItemsSource is List<date> items))
            {
                if (coldatetime != DateTime.MinValue)
                    selectedvalue = coldatetime.ToString("dd/MM/yyyy");

                if (!String.IsNullOrEmpty(selectedvalue))
                {
                    date item;
                    bool selected = false;
                    for (int i = 0; i <= items.Count - 1; i++)
                    {
                        item = items[i];
                        if (item.formattedDate.Equals(selectedvalue))
                        {
                            cvDate.SelectedItem = items[i];
                            cvDate.ScrollTo(item, position: ScrollToPosition.MakeVisible, animate: false);
                            selected = true;
                            break;
                        }
                    }
                    if (!selected)
                        cvDate.SelectedItem = items[0];
                }
                else
                {
                    cvDate.SelectedItem = items[0];
                }
            }


            selectedvalue = "";
            itemCount = cvColLunch.ItemsSource is List<lunchtime> collectionlt ? collectionlt.Count : 0;
            if (itemCount > 0 && (cvColLunch.ItemsSource is List<lunchtime> itemslt))
            {
                selectedvalue = vm.colDateLunchTimeValue;
                if (!String.IsNullOrEmpty(selectedvalue))
                {
                    lunchtime itemlt;
                    bool selected = false;
                    for (int i = 0; i <= itemslt.Count - 1; i++)
                    {
                        itemlt = itemslt[i];
                        if (itemlt.value.Equals(selectedvalue))
                        {
                            cvColLunch.SelectedItem = itemslt[i];
                            cvColLunch.ScrollTo(itemlt, position: ScrollToPosition.MakeVisible, animate: false);
                            selected = true;
                            break;
                        }
                    }
                    if (!selected)
                        cvColLunch.SelectedItem = itemslt[0];
                }
                else
                {
                    cvColLunch.SelectedItem = itemslt[0];
                }
            }


            selectedvalue = "";
            itemCount = cvTime.ItemsSource is List<time> collectiont ? collectiont.Count : 0;
            if (itemCount > 0 && (cvTime.ItemsSource is List<time> itemst))
            {
                if (coldatetime != DateTime.MinValue)
                    selectedvalue = coldatetime.ToString("HH:mm");

                if (!String.IsNullOrEmpty(selectedvalue))
                {
                    time itemt;
                    bool selected = false;
                    for (int i = 0; i <= itemst.Count - 1; i++)
                    {
                        itemt = itemst[i];
                        if (itemt.value.Equals(selectedvalue))
                        {
                            cvTime.SelectedItem = itemst[i];
                            cvTime.ScrollTo(itemt, position: ScrollToPosition.MakeVisible, animate: false);
                            selected = true;
                            break;
                        }
                    }
                    if (!selected)
                        cvTime.SelectedItem = itemst[0];
                }
                else
                {
                    cvTime.SelectedItem = itemst[0];
                }
            }

        }
        catch (Exception e)
        {
            string s = e.Message;
        }
    }

    #endregion

    //==================================================================================================================================

    #region DeliveryTime

    async void show_DelTime_BS(object sender, EventArgs e)
    {
        try
        {
            if (vm == null)
                vm = (CreateJobVM)BindingContext;

            string ColDateTimeSelected = vm.colDateTimeFinalValue;

            if (String.IsNullOrEmpty(ColDateTimeSelected))
            {
                showmsg("", "Please select your Collection Time first.");
            }
            else
            {
                if (fakeAllowedExpTypes == null || (fakeAllowedExpTypes != null && fakeAllowedExpTypes.Count == 0))
                {
                    if (!await addExpTypes())
                        return;
                }

                int cvDFTimeCount = cvDFTime.ItemsSource is List<time> collection1 ? collection1.Count : 0;
                int cvDBTimeCount = cvDBTime.ItemsSource is List<delByDateTime> collection2 ? collection2.Count : 0;

                int cvDFDateCount = cvDFDate.ItemsSource is List<date> collection ? collection.Count : 0;
                if (cvDFDateCount == 0)
                {
                    addDelFromDateList();
                }

                preSelectDelDate();
                // Scroll to the top of the ScrollView
                await svDelTime.ScrollToAsync(0, 0, false);

                vm.BSDeliveryTimeopened = true;
                BSDeliveryTime.IsVisible = true;
                removeTapGestureRecognizer();

                await BSDeliveryTime.OpenBottomSheet(false);
                BSDeliveryTime.isShowing = true;
            }

        }
        catch (Exception ex)
        {
            string s = ex.Message;
        }
    }

    void addDelFromDateList()
    {
        try
        {
            cvDFDate.ItemsSource = null;
            string readyDateTimeStr = vm.colDateTimeFinalValue; //"dd/MM/yyyy HH:mm"
            DateTime rdydatetime = DateTime.ParseExact(readyDateTimeStr, dateformats, CultureInfo.InvariantCulture, DateTimeStyles.None);
            date d;
            List<date> list = new List<date>();
            DateTime nw = DateTime.Now;
            int dateV = 0;
            string dateddmm = "";
            String dow = nw.ToString("ddd");
            string formattedDate = nw.ToString("dd/MM/yyyy");
            TimeSpan ts;
            TimeSpan ts9 = new System.TimeSpan(9, 0, 0);

            DateTime sysTime = rdydatetime;
            DateTime customToday = DateTime.Today;
            DateTime customNow = DateTime.Now;
            DateTime et530etNmlCutOffTime = new DateTime();

            XDelServiceRef.eExpressType expType = fakeAllowedExpTypes[0];

            sysTime = sysTime.AddDays(-1);

            do
            {
                sysTime = sysTime.AddDays(1);
            } /*while (v.vIsHoliday(sysTime.Date) || (sysTime.DayOfWeek == DayOfWeek.Saturday || sysTime.DayOfWeek == DayOfWeek.Sunday));*/
            while ((sysTime.DayOfWeek == DayOfWeek.Saturday || sysTime.DayOfWeek == DayOfWeek.Sunday));

            if (sysTime.Date == customToday.Date)
            {
                et530etNmlCutOffTime = customNow;
                if (expType == XDelServiceRef.eExpressType.etNormal || expType == XDelServiceRef.eExpressType.etGuaranteed
                    || expType == XDelServiceRef.eExpressType.etPriority)
                {
                    ts = new System.TimeSpan(11, 31, 0);
                    et530etNmlCutOffTime = et530etNmlCutOffTime.Date + ts;
                }
                else if (expType == XDelServiceRef.eExpressType.etThreeHour)
                {
                    ts = new System.TimeSpan(14, 01, 0);
                    et530etNmlCutOffTime = et530etNmlCutOffTime.Date + ts;
                }
                else if (expType == XDelServiceRef.eExpressType.etTwoHour)
                {
                    ts = new System.TimeSpan(15, 01, 0);
                    et530etNmlCutOffTime = et530etNmlCutOffTime.Date + ts;
                }
                else if (expType == XDelServiceRef.eExpressType.etOneHour)
                {
                    ts = new System.TimeSpan(16, 01, 0);
                    et530etNmlCutOffTime = et530etNmlCutOffTime.Date + ts;
                }
            }
            else if (sysTime.Date > customToday.Date)
            {
                et530etNmlCutOffTime = sysTime;
                //ts = new TimeSpan(11, 31, 0);
                if (expType == XDelServiceRef.eExpressType.etNormal || expType == XDelServiceRef.eExpressType.etGuaranteed
                    || expType == XDelServiceRef.eExpressType.etPriority)
                {
                    ts = new System.TimeSpan(11, 31, 0);
                    et530etNmlCutOffTime = et530etNmlCutOffTime.Date + ts;
                }
                else if (expType == XDelServiceRef.eExpressType.etThreeHour)
                {
                    ts = new System.TimeSpan(14, 01, 0);
                    et530etNmlCutOffTime = et530etNmlCutOffTime.Date + ts;
                }
                else if (expType == XDelServiceRef.eExpressType.etTwoHour)
                {
                    ts = new System.TimeSpan(15, 01, 0);
                    et530etNmlCutOffTime = et530etNmlCutOffTime.Date + ts;
                }
                else if (expType == XDelServiceRef.eExpressType.etOneHour)
                {
                    ts = new System.TimeSpan(16, 01, 0);
                    et530etNmlCutOffTime = et530etNmlCutOffTime.Date + ts;
                }
            }

            if (sysTime >= et530etNmlCutOffTime)
            {
                do
                {
                    sysTime = sysTime.AddDays(1);
                } //while (v.vIsHoliday(sysTime.Date) || (sysTime.DayOfWeek == DayOfWeek.Saturday || sysTime.DayOfWeek == DayOfWeek.Sunday));
                while ((sysTime.DayOfWeek == DayOfWeek.Saturday || sysTime.DayOfWeek == DayOfWeek.Sunday));

                ts = new System.TimeSpan(9, 0, 0);
                sysTime = sysTime.Date + ts;

            }

            for (int i = 1; i <= 14; i++)
            {
                if (i > 1)
                {
                    do
                    {
                        sysTime = sysTime.AddDays(1);
                    } /*while (v.vIsHoliday(sysTime.Date) || (sysTime.DayOfWeek == DayOfWeek.Saturday || sysTime.DayOfWeek == DayOfWeek.Sunday));*/
                    while ((sysTime.DayOfWeek == DayOfWeek.Saturday || sysTime.DayOfWeek == DayOfWeek.Sunday));

                    //sysTime = sysTime.AddDays(1);
                    sysTime = sysTime.Date + ts9;
                }

                dow = sysTime.ToString("ddd");
                dateddmm = sysTime.ToString("dd/MM");
                dateV = sysTime.Day;
                formattedDate = sysTime.ToString("dd/MM/yyyy HH:mm");
                //formattedDate = sysTime.ToString("dd/MM/yyyy");
                d = new date() { dispText = dow + "\n" + dateddmm, formattedDate = formattedDate };
                list.Add(d);
            }

            cvDFDate.ItemsSource = list;

            int itemCount = cvDFDate.ItemsSource is List<date> collection ? collection.Count : 0;

            if (String.IsNullOrEmpty(vm.dfDateSelectedValue) && itemCount > 0)
            {
                if (cvDFDate.ItemsSource is List<date> items)
                {
                    // Select the first item (index 0)
                    cvDFDate.SelectedItem = items[0];
                }
            }
        }
        catch (Exception ex)
        {
            string s = ex.Message;
        }
    }

    private void cvDFDateOnItemSelected(object sender, SelectionChangedEventArgs e)
    {
        try
        {
            // Clear previous
            if (cvDFDate.ItemsSource is List<date> all)
                foreach (var d in all) d.IsSelected = false;

            var selectedItem = e.CurrentSelection.FirstOrDefault() as date;
            if (selectedItem != null)
            {
                selectedItem.IsSelected = true;

                vm.dfDateSelectedValue = selectedItem.formattedDate;
                if (!String.IsNullOrEmpty(vm.dfDatePrevSelectedValue) && !vm.dfDatePrevSelectedValue.Equals(vm.dfDateSelectedValue))
                {
                    vm.dfLunchPrevSelectedIDX = "";
                    vm.dfLunchSelectedIDX = "";
                    vm.dfTimeSelectedValue = "";
                    vm.dbTimeSelectedValue = "";
                }

                addLunchTime(cvDFLunch, 1);
                if (cvDFLunch.ItemsSource != null)
                {
                    int idx = !String.IsNullOrEmpty(vm.dfLunchSelectedIDX) ? int.Parse(vm.dfLunchSelectedIDX) : 0;

                    cvDFLunch.SelectedItem = ((List<lunchtime>)cvDFLunch.ItemsSource)[idx];
                    cvDFLunch.ScrollTo(idx, position: ScrollToPosition.MakeVisible, animate: false);

                }
            }
        }
        catch (Exception ex)
        {
            string s = ex.Message;
        }
    }

    private void cvDFLunchOnItemSelected(object sender, SelectionChangedEventArgs e)
    {
        try
        {
            // Clear previous
            if (cvDFLunch.ItemsSource is List<lunchtime> all)
                foreach (var d in all) d.IsSelected = false;

            string selectedDateStr = vm.dfDateSelectedValue;
            var selectedItem = e.CurrentSelection.FirstOrDefault() as lunchtime;
            if (selectedItem != null)
            {
                selectedItem.IsSelected = true;

                int selectedIndex = cvDFLunch.ItemsSource.Cast<lunchtime>()
                                 .ToList().IndexOf(selectedItem);
                vm.dfLunchSelectedIDX = selectedIndex.ToString();
                vm.dfDateLunchTimeValue = selectedItem.value;

                /*vm.job1.DLLunch_Avoid = !vm.dfDateLunchTimeValue.Equals("0");
                vm.job1.DLLunch_From = vm.dfDateLunchTimeValue.Equals("1") ? vm.job1.FromDateTime.Date + new TimeSpan(13, 0, 0) :
                        vm.dfDateLunchTimeValue.Equals("2") ? vm.job1.FromDateTime.Date + new TimeSpan(12, 0, 0) :
                        vm.dfDateLunchTimeValue.Equals("3") ? vm.job1.FromDateTime.Date + new TimeSpan(12, 0, 0) :
                        vm.dfDateLunchTimeValue.Equals("4") ? vm.job1.FromDateTime.Date + new TimeSpan(11, 30, 0) :
                        DateTime.MinValue;

                vm.job1.DLLunch_To = vm.dfDateLunchTimeValue.Equals("1") ? vm.job1.FromDateTime.Date + new TimeSpan(14, 0, 0) :
                        vm.dfDateLunchTimeValue.Equals("2") ? vm.job1.FromDateTime.Date + new TimeSpan(14, 0, 0) :
                        vm.dfDateLunchTimeValue.Equals("3") ? vm.job1.FromDateTime.Date + new TimeSpan(13, 0, 0) :
                        vm.dfDateLunchTimeValue.Equals("4") ? vm.job1.FromDateTime.Date + new TimeSpan(12, 30, 0) :
                        DateTime.MinValue;*/

                if (!String.IsNullOrEmpty(selectedItem.value))
                    addDelFromTimeList();
            }
        }
        catch (Exception ex)
        {
            string s = ex.Message;
        }
    }

    void addDelFromTimeList()
    {
        try
        {
            cvDFTime.ItemsSource = null;
            string valdispText = "";
            string val = "";
            time t;
            List<time> list = new List<time>();
            string selecteddfDateTimeStr = vm.dfDateSelectedValue;
            bool allExpTypeBlockedForToday = false;
            DateTime originalSysTime = DateTime.ParseExact(selecteddfDateTimeStr, dateformats, CultureInfo.InvariantCulture, DateTimeStyles.None);
            DateTime sysTime = DateTime.Now;
            DateTime compareTime;
            TimeSpan ts;
            TimeSpan ThreeHoursTS = new System.TimeSpan(14, 1, 0);
            string sTime = "";

            //XDelServiceRef.XDelOnlineSettings cxo = null;
            //XDelSys.XDelOnlineSettings xo = null;

            date xo = new date();
            date cxo = new date();
            Int32 GraceMinutes = 0;

            List<string> lunchhours = getLunchTimeTimesListNew(vm.dfDateLunchTimeValue);
            List<string> timesL = times.ToList();
            string[] timesR = null;


            XDelServiceRef.eExpressType expType = fakeAllowedExpTypes[0];

            if (lunchhours != null && lunchhours.Count > 0)
            {
                for (int i = timesL.Count - 1; i >= 0; i--)
                {
                    if (lunchhours.Contains(timesL[i]))
                    {
                        timesL.RemoveAt(i);
                    }
                }
            }

            timesR = timesL.ToArray();

            if (allExpTypeBlockedForToday)
            {

            }
            else
            {
                for (int i = 0; i <= timesR.Length - 1; i++)
                {
                    sysTime = originalSysTime;
                    ts = System.TimeSpan.Parse(timesR[i]);
                    sTime = (DateTime.Today.Add(ts).ToString("h:mm tt"));
                    compareTime = sysTime;
                    sysTime = sysTime.Date + ts;

                    if (sysTime >= compareTime)
                    {
                        if (expType == XDelServiceRef.eExpressType.etThreeHour || expType == XDelServiceRef.eExpressType.etTwoHour ||
                            expType == XDelServiceRef.eExpressType.etOneHour)
                        {
                            if (expType == XDelServiceRef.eExpressType.etThreeHour)
                            {
                                ThreeHoursTS = new System.TimeSpan(14, 1, 0);
                            }
                            else if (expType == XDelServiceRef.eExpressType.etTwoHour)
                            {
                                ThreeHoursTS = new System.TimeSpan(15, 1, 0);
                            }
                            else if (expType == XDelServiceRef.eExpressType.etOneHour)
                            {
                                ThreeHoursTS = new System.TimeSpan(16, 1, 0);
                            }

                            if (ts < ThreeHoursTS)
                            {
                                //if (xo != null && xo.GraceMinutes > 0)
                                if (xo != null && GraceMinutes > 0)
                                {
                                    if (list.Count == 0)
                                    {
                                        valdispText = sysTime.ToString("h:mm tt", CultureInfo.InvariantCulture).ToUpper();
                                        val = sysTime.ToString("HH:mm");
                                        t = new time() { dispText = valdispText, value = val, IsVisible = true };
                                        list.Add(t);
                                        i--;
                                    }
                                    else
                                    {
                                        valdispText = sysTime.ToString("h:mm tt", CultureInfo.InvariantCulture).ToUpper();
                                        val = sysTime.ToString("HH:mm");
                                        t = new time() { dispText = valdispText, value = val, IsVisible = true };
                                        list.Add(t);

                                        //}
                                    }
                                }
                                //else if (xo != null && xo.GraceMinutes == 0)
                                else if (xo != null && GraceMinutes == 0)
                                {
                                    //if (cxo != null && cxo.GraceMinutes > 0)
                                    if (cxo != null && GraceMinutes > 0)
                                    {
                                        if (list.Count == 0)
                                        {
                                            valdispText = sysTime.ToString("h:mm tt", CultureInfo.InvariantCulture).ToUpper();
                                            val = sysTime.ToString("HH:mm");
                                            t = new time() { dispText = valdispText, value = val, IsVisible = true };
                                            list.Add(t);
                                            i--;
                                        }
                                        else
                                        {
                                            valdispText = sysTime.ToString("h:mm tt", CultureInfo.InvariantCulture).ToUpper();
                                            val = sysTime.ToString("HH:mm");
                                            t = new time() { dispText = valdispText, value = val, IsVisible = true };
                                            list.Add(t);
                                            //}
                                        }
                                    }
                                    //else if (cxo != null && cxo.GraceMinutes == 0)
                                    else if (cxo != null && GraceMinutes == 0)
                                    {
                                        valdispText = sysTime.ToString("h:mm tt", CultureInfo.InvariantCulture).ToUpper();
                                        val = sysTime.ToString("HH:mm");
                                        t = new time() { dispText = valdispText, value = val, IsVisible = true };
                                        list.Add(t);
                                    }
                                }
                            }
                        }
                        else if (expType == XDelServiceRef.eExpressType.etNormal || expType == XDelServiceRef.eExpressType.etGuaranteed ||
                                 expType == XDelServiceRef.eExpressType.etPriority)
                        {
                            valdispText = sysTime.ToString("h:mm tt", CultureInfo.InvariantCulture).ToUpper();
                            val = sysTime.ToString("HH:mm");
                            t = new time() { dispText = valdispText, value = val, IsVisible = true };
                            list.Add(t);
                        }
                    }

                }


            }

            cvDFTime.ItemsSource = list;
            cvDFTime.SelectedItem = list[0];
        }
        catch (Exception ex)
        {
            string s = ex.Message;
        }
    }

    private void cvDFTimeOnItemSelected(object sender, SelectionChangedEventArgs e)
    {
        try
        {
            // Clear previous
            if (cvDFTime.ItemsSource is List<time> all)
                foreach (var d in all) d.IsSelected = false;

            var selectedItem = e.CurrentSelection.FirstOrDefault() as time;
            if (selectedItem != null)
            {
                selectedItem.IsSelected = true;

                vm.dfTimeSelectedValue = selectedItem.value;
                setDFDateTime();
                if (!String.IsNullOrEmpty(selectedItem.value))
                    addDelByTimeList();
            }
        }
        catch (Exception ex)
        {
            string s = ex.Message;
        }
    }

    void setDFDateTime()
    {
        try
        {
            string selectedDelFromDateStr = vm.dfDateSelectedValue;
            DateTime selectedDelFromDate = DateTime.ParseExact(selectedDelFromDateStr, dateformats, CultureInfo.InvariantCulture, DateTimeStyles.None);
            string selectedDelFromTimeStr = vm.dfTimeSelectedValue;
            int sHr = Int32.Parse(selectedDelFromTimeStr.Substring(0, 2));
            int sMin = Int32.Parse(selectedDelFromTimeStr.Substring(3, 2));
            TimeSpan ts = new TimeSpan(sHr, sMin, 0);
            selectedDelFromDate = selectedDelFromDate.Date + ts;
            vm.dfDateTimeFinalValue = selectedDelFromDate.ToString("dd/MM/yyyy HH:mm");
        }
        catch (Exception e)
        {
            string s = e.Message;
        }
    }

    void addDelByTimeList()
    {
        try
        {
            cvDBTime.ItemsSource = null;
            string valdispText = "";
            delByDateTime t;
            List<delByDateTime> list = new List<delByDateTime>();
            string selectedDelFromDateTimeStr = vm.dfDateTimeFinalValue;
            bool allExpTypeBlockedForToday = false;
            DateTime originalSysTime = DateTime.ParseExact(selectedDelFromDateTimeStr, dateformats, CultureInfo.InvariantCulture, DateTimeStyles.None);
            DateTime sysTime = DateTime.Now;
            DateTime compareTime;

            string readyDateTimeStr = vm.colDateTimeFinalValue;
            DateTime pickupDateTimeFrom = DateTime.ParseExact(readyDateTimeStr, dateformats, CultureInfo.InvariantCulture, DateTimeStyles.None);

            DateTime deliveryDateTimeFrom = DateTime.ParseExact(selectedDelFromDateTimeStr, dateformats, CultureInfo.InvariantCulture, DateTimeStyles.None);

            string lblCDLTvText = vm.colDateLunchTimeValue;
            DateTime lunchPUAvoidDateTimeTo = new DateTime();
            string lblDFLTvText = vm.dfDateLunchTimeValue;
            DateTime lunchDELAvoidDateTimeTo = new DateTime();
            bool noPULunchAvoidance = false;
            bool noDELLunchAvoidance = false;
            DateTime DelBy = new DateTime();
            DateTime cutOffTime;
            TimeSpan cutOffTS;

            TimeSpan ts;
            TimeSpan ThreeHoursTS = new System.TimeSpan(14, 1, 0);
            string sTime = "";

            //XDelServiceRef.XDelOnlineSettings cxo = null;
            //XDelSys.XDelOnlineSettings xo = null;

            date xo = new date();
            date cxo = new date();
            Int32 GraceMinutes = 0;

            List<string> lunchhours = getLunchTimeTimesListNew(vm.dfDateLunchTimeValue);
            List<string> timesL = times.ToList();
            string[] timesR = null;
            int itemCount = 0;
            string OneWayTwoWaySelectedValue = "0";

            XDelServiceRef.eExpressType expType = fakeAllowedExpTypes[0];

            if (lblCDLTvText.Equals("4"))
            {
                lunchPUAvoidDateTimeTo = pickupDateTimeFrom.Date + new TimeSpan(12, 30, 0);
            }
            else if (lblCDLTvText.Equals("3"))
            {
                lunchPUAvoidDateTimeTo = pickupDateTimeFrom.Date + new TimeSpan(13, 0, 0);
            }
            else if (lblCDLTvText.Equals("2") || lblCDLTvText.Equals("1"))
            {
                lunchPUAvoidDateTimeTo = pickupDateTimeFrom.Date + new TimeSpan(14, 0, 0);
            }
            else
            {
                noPULunchAvoidance = true;
            }

            if (lblDFLTvText.Equals("4"))
            {
                lunchDELAvoidDateTimeTo = deliveryDateTimeFrom.Date + new TimeSpan(12, 30, 0);
            }
            else if (lblDFLTvText.Equals("3"))
            {
                lunchDELAvoidDateTimeTo = deliveryDateTimeFrom.Date + new TimeSpan(13, 0, 0);
            }
            else if (lblDFLTvText.Equals("2") || lblDFLTvText.Equals("1"))
            {
                lunchDELAvoidDateTimeTo = deliveryDateTimeFrom.Date + new TimeSpan(14, 0, 0);
            }
            else
            {
                noDELLunchAvoidance = true;
            }


            itemCount = cvDate.ItemsSource is List<date> collection ? collection.Count : 0;
            date lastPUitem = null;
            if (itemCount > 0 && (cvDate.ItemsSource is List<date> items))
            {
                lastPUitem = items[itemCount - 1];
            }

            DateTime lastDay = DateTime.Now;
            lastDay = lastPUitem != null ? DateTime.ParseExact(lastPUitem.formattedDate, dateformats, CultureInfo.InvariantCulture, DateTimeStyles.None) : DateTime.Now; //abit different from XDelOnline ServiceType.aspx.cs line 1866
            //Boolean hasExtException = false;
            Boolean hasExtException = true;
            DateTime extDelfrom = DateTime.MinValue;
            DateTime extDelby = DateTime.MinValue;
            int extHr = 0;

            bool etOneHourOk = fakeAllowedExpTypes.Contains(XDelServiceRef.eExpressType.etOneHour);
            bool etOneHourOk2 = deliveryDateTimeFrom >= lunchDELAvoidDateTimeTo && pickupDateTimeFrom >= lunchPUAvoidDateTimeTo;
            bool etOneHourOk3 = noDELLunchAvoidance && noPULunchAvoidance;

            if (etOneHourOk &&
                (etOneHourOk2 || etOneHourOk3))
            {
                DelBy = deliveryDateTimeFrom.AddHours(1.5);
                cutOffTS = new System.TimeSpan(16, 00, 0);
                cutOffTime = deliveryDateTimeFrom.Date + cutOffTS;
                //ze = (xs.Get_ZoneExpress((string)Session["webid"], eDeliveryDirection.tdDelivery, deliveryDateTimeFrom, (string)Session["DLPOSTALCODE"], eExpressType.etOneHour)).ZoneExpressList;
                //getZE_ExceptionValues("1.5 Hours", ze, DelBy, ref extDelby, ref hasExtException, ref extHr);

                if (hasExtException)
                {
                    if (deliveryDateTimeFrom <= cutOffTime)
                    {
                        //dr = newDelByDT.NewRow();
                        //dr["Date"] = DelBy;
                        //dr["Time"] = extDelby != DateTime.MinValue ? extDelby.ToString("h:mm TT") : DelBy.ToString("h:mm TT");
                        //dr["extDelby"] = extDelby;

                        //XDelServiceRef.JobTimingStructure job = xs.GetExpressType(uid, eExpressType.etOneHour, deliveryDateTimeFrom, DelBy, null, null, contentType);
                        //dr["ExpressType"] = getExpressSelect(job.ExpressType);
                        //newDelByDT.Rows.Add(dr);
                        //removeHigherExpSvcDueConflict(ref newDelByDT, DelBy);

                        valdispText = extDelby != DateTime.MinValue ? extDelby.ToString("h:mm tt", CultureInfo.InvariantCulture).ToUpper() : 
                            DelBy.ToString("h:mm tt", CultureInfo.InvariantCulture).ToUpper();
                        ////val = sysTime.ToString("HH:mm");
                        //val = extDelby != DateTime.MinValue ? extDelby.ToString("HH:mm") : DelBy.ToString("HH:mm");
                        t = new delByDateTime()
                        {
                            Date = DelBy,
                            formattedDate = DelBy.ToString("dd/MM/yyyy HH:mm"),
                            Time = valdispText,
                            dispText = valdispText,
                            extDelby = extDelby,
                            extDelbyformattedDate = extDelby.ToString("dd/MM/yyyy HH:mm"),
                            ExpressType = common.eExpressType.etOneHour,
                            ExpressTypeStr = common.eExpressType.etOneHour.ToString(),
                            value = DelBy.ToString("HH:mm"),
                            valueselected = valdispText
                        };
                        list.Add(t);
                    }
                }
            }
            //hasExtException = false;
            hasExtException = true;
            extDelby = DateTime.MinValue;
            extHr = 0;

            bool etTwoHour = fakeAllowedExpTypes.Contains(XDelServiceRef.eExpressType.etTwoHour);
            bool etTwoHour2 = deliveryDateTimeFrom >= lunchDELAvoidDateTimeTo && pickupDateTimeFrom >= lunchPUAvoidDateTimeTo;
            bool etTwoHour3 = noDELLunchAvoidance && noPULunchAvoidance;

            if (etTwoHour &&
                (etTwoHour2 || etTwoHour3))
            {
                DelBy = deliveryDateTimeFrom.AddHours(2.5);
                cutOffTS = new System.TimeSpan(15, 00, 0);
                cutOffTime = deliveryDateTimeFrom.Date + cutOffTS;
                //ze = (xs.Get_ZoneExpress((string)Session["webid"], eDeliveryDirection.tdDelivery, deliveryDateTimeFrom, (string)Session["DLPOSTALCODE"], eExpressType.etTwoHour)).ZoneExpressList;
                //getZE_ExceptionValues("2.5 Hours", ze, DelBy, ref extDelby, ref hasExtException, ref extHr);

                if (hasExtException)
                {
                    if (deliveryDateTimeFrom <= cutOffTime)
                    {
                        //dr = newDelByDT.NewRow();
                        //dr["Date"] = DelBy;
                        //dr["Time"] = extDelby != DateTime.MinValue ? extDelby.ToString("h:mm TT") : DelBy.ToString("h:mm TT");
                        //dr["extDelby"] = extDelby;

                        //XDelServiceRef.JobTimingStructure job = xs.GetExpressType(uid, eExpressType.etTwoHour, deliveryDateTimeFrom, DelBy, null, null, contentType);
                        //dr["ExpressType"] = getExpressSelect(job.ExpressType);
                        //newDelByDT.Rows.Add(dr);
                        //removeHigherExpSvcDueConflict(ref newDelByDT, DelBy);

                        valdispText = extDelby != DateTime.MinValue ? extDelby.ToString("h:mm tt", CultureInfo.InvariantCulture).ToUpper() : 
                            DelBy.ToString("h:mm tt", CultureInfo.InvariantCulture).ToUpper();
                        ////val = sysTime.ToString("HH:mm");
                        //val = extDelby != DateTime.MinValue ? extDelby.ToString("HH:mm") : DelBy.ToString("HH:mm");
                        t = new delByDateTime()
                        {
                            Date = DelBy,
                            formattedDate = DelBy.ToString("dd/MM/yyyy HH:mm"),
                            Time = valdispText,
                            dispText = valdispText,
                            extDelby = extDelby,
                            extDelbyformattedDate = extDelby.ToString("dd/MM/yyyy HH:mm"),
                            ExpressType = common.eExpressType.etTwoHour,
                            ExpressTypeStr = common.eExpressType.etTwoHour.ToString(),
                            value = DelBy.ToString("HH:mm"),
                            valueselected = valdispText
                        };
                        list.Add(t);
                    }
                }
            }
            //hasExtException = false;
            hasExtException = true;
            extDelby = DateTime.MinValue;
            extHr = 0;

            bool etThreeHourOk = fakeAllowedExpTypes.Contains(XDelServiceRef.eExpressType.etThreeHour);
            bool etThreeHourOk2 = deliveryDateTimeFrom >= lunchDELAvoidDateTimeTo && pickupDateTimeFrom >= lunchPUAvoidDateTimeTo;
            bool etThreeHourOk3 = noDELLunchAvoidance && noPULunchAvoidance;

            if (etThreeHourOk &&
                (etThreeHourOk2 || etThreeHourOk3))
            {
                DelBy = deliveryDateTimeFrom.AddHours(3.5);
                cutOffTS = new System.TimeSpan(14, 00, 0);
                cutOffTime = deliveryDateTimeFrom.Date + cutOffTS;
                //ze = (xs.Get_ZoneExpress((string)Session["webid"], eDeliveryDirection.tdDelivery, deliveryDateTimeFrom, (string)Session["DLPOSTALCODE"], eExpressType.etThreeHour)).ZoneExpressList;
                //getZE_ExceptionValues("3.5 Hours", ze, DelBy, ref extDelby, ref hasExtException, ref extHr);

                if (hasExtException)
                {
                    if (deliveryDateTimeFrom <= cutOffTime)
                    {
                        //dr = newDelByDT.NewRow();
                        //dr["Date"] = DelBy;
                        //dr["Time"] = extDelby != DateTime.MinValue ? extDelby.ToString("h:mm TT") : DelBy.ToString("h:mm TT");
                        //dr["extDelby"] = extDelby;

                        //XDelServiceRef.JobTimingStructure job = xs.GetExpressType(uid, eExpressType.etThreeHour, deliveryDateTimeFrom, DelBy, null, null, contentType);
                        //dr["ExpressType"] = getExpressSelect(job.ExpressType);
                        //newDelByDT.Rows.Add(dr);
                        //removeHigherExpSvcDueConflict(ref newDelByDT, DelBy);

                        valdispText = extDelby != DateTime.MinValue ? extDelby.ToString("h:mm tt", CultureInfo.InvariantCulture).ToUpper() : 
                            DelBy.ToString("h:mm tt", CultureInfo.InvariantCulture).ToUpper();
                        ////val = sysTime.ToString("HH:mm");
                        //val = extDelby != DateTime.MinValue ? extDelby.ToString("HH:mm") : DelBy.ToString("HH:mm");
                        t = new delByDateTime()
                        {
                            Date = DelBy,
                            formattedDate = DelBy.ToString("dd/MM/yyyy HH:mm"),
                            Time = valdispText,
                            dispText = valdispText,
                            extDelby = extDelby,
                            extDelbyformattedDate = extDelby.ToString("dd/MM/yyyy HH:mm"),
                            ExpressType = common.eExpressType.etThreeHour,
                            ExpressTypeStr = common.eExpressType.etThreeHour.ToString(),
                            value = DelBy.ToString("HH:mm"),
                            valueselected = valdispText
                        };
                        list.Add(t);
                    }
                }
            }
            //hasExtException = false;
            hasExtException = true;
            extDelby = DateTime.MinValue;
            extHr = 0;

            bool etPriority = fakeAllowedExpTypes.Contains(XDelServiceRef.eExpressType.etPriority);
            bool etPriority2 = deliveryDateTimeFrom >= lunchDELAvoidDateTimeTo && pickupDateTimeFrom >= lunchPUAvoidDateTimeTo;
            bool etPriority3 = noDELLunchAvoidance && noPULunchAvoidance;
            bool etPriority4 = deliveryDateTimeFrom.Date < lastDay.Date && OneWayTwoWaySelectedValue == "1";
            bool etPriority5 = deliveryDateTimeFrom.Date <= lastDay.Date && OneWayTwoWaySelectedValue == "0";

            if (etPriority &&
                (etPriority2 || etPriority3)
                && (etPriority4 || etPriority5))
            {
                ts = new System.TimeSpan(16, 30, 0);
                DelBy = deliveryDateTimeFrom.Date + ts;
                cutOffTS = new System.TimeSpan(11, 31, 0);
                cutOffTime = deliveryDateTimeFrom.Date + cutOffTS;
                //ze = (xs.Get_ZoneExpress((string)Session["webid"], eDeliveryDirection.tdDelivery, deliveryDateTimeFrom, (string)Session["DLPOSTALCODE"], eExpressType.etPriority)).ZoneExpressList;
                //getZE_ExceptionValues("Four Thirty", ze, DelBy, ref extDelby, ref hasExtException, ref extHr);

                if (hasExtException)
                {
                    if (deliveryDateTimeFrom <= cutOffTime)
                    {
                        //dr = newDelByDT.NewRow();
                        //dr["Date"] = DelBy;
                        //dr["Time"] = extDelby != DateTime.MinValue ? extDelby.ToString("h:mm TT") : DelBy.ToString("h:mm TT");
                        //dr["extDelby"] = extDelby;

                        //XDelServiceRef.JobTimingStructure job = xs.GetExpressType(uid, eExpressType.etPriority, deliveryDateTimeFrom, DelBy, null, null, contentType);
                        //dr["ExpressType"] = getExpressSelect(job.ExpressType);
                        //newDelByDT.Rows.Add(dr);
                        //removeHigherExpSvcDueConflict(ref newDelByDT, DelBy);

                        valdispText = extDelby != DateTime.MinValue ? extDelby.ToString("h:mm tt", CultureInfo.InvariantCulture).ToUpper() : 
                            DelBy.ToString("h:mm tt", CultureInfo.InvariantCulture).ToUpper();
                        //val = sysTime.ToString("HH:mm");
                        t = new delByDateTime()
                        {
                            Date = DelBy,
                            formattedDate = DelBy.ToString("dd/MM/yyyy HH:mm"),
                            Time = valdispText,
                            dispText = valdispText,
                            extDelby = extDelby,
                            extDelbyformattedDate = extDelby.ToString("dd/MM/yyyy HH:mm"),
                            ExpressType = common.eExpressType.etPriority,
                            ExpressTypeStr = common.eExpressType.etPriority.ToString(),
                            value = DelBy.ToString("HH:mm"),
                            valueselected = valdispText
                        };
                        list.Add(t);
                    }
                }
            }
            //hasExtException = false;
            hasExtException = true;
            extDelby = DateTime.MinValue;
            extHr = 0;

            bool etGuaranteed = fakeAllowedExpTypes.Contains(XDelServiceRef.eExpressType.etGuaranteed);
            bool etGuaranteed2 = deliveryDateTimeFrom.Date < lastDay.Date && OneWayTwoWaySelectedValue == "1";
            bool etGuaranteed3 = deliveryDateTimeFrom.Date <= lastDay.Date && OneWayTwoWaySelectedValue == "0";

            if (etGuaranteed &&
                (etGuaranteed2 || etGuaranteed3))
            {
                ts = new System.TimeSpan(17, 30, 0);
                DelBy = deliveryDateTimeFrom.Date + ts;
                cutOffTS = new System.TimeSpan(11, 31, 0);
                cutOffTime = deliveryDateTimeFrom.Date + cutOffTS;
                //ze = (xs.Get_ZoneExpress((string)Session["webid"], eDeliveryDirection.tdDelivery, deliveryDateTimeFrom, (string)Session["DLPOSTALCODE"], eExpressType.etGuaranteed)).ZoneExpressList;
                //getZE_ExceptionValues("Five Thirty", ze, DelBy, ref extDelby, ref hasExtException, ref extHr);

                if (hasExtException)
                {
                    if (deliveryDateTimeFrom <= cutOffTime)
                    {
                        //dr = newDelByDT.NewRow();
                        //dr["Date"] = DelBy;
                        //dr["Time"] = extDelby != DateTime.MinValue ? extDelby.ToString("h:mm TT") : DelBy.ToString("h:mm TT");
                        //dr["extDelby"] = extDelby;

                        //XDelServiceRef.JobTimingStructure job = xs.GetExpressType(uid, eExpressType.etGuaranteed, deliveryDateTimeFrom, DelBy, null, null, contentType);
                        //dr["ExpressType"] = getExpressSelect(job.ExpressType);
                        //newDelByDT.Rows.Add(dr);
                        //removeHigherExpSvcDueConflict(ref newDelByDT, DelBy);

                        valdispText = extDelby != DateTime.MinValue ? extDelby.ToString("h:mm tt", CultureInfo.InvariantCulture).ToUpper() : 
                            DelBy.ToString("h:mm tt", CultureInfo.InvariantCulture).ToUpper();
                        ////val = sysTime.ToString("HH:mm");
                        //val = extDelby != DateTime.MinValue ? extDelby.ToString("HH:mm") : DelBy.ToString("HH:mm");
                        t = new delByDateTime()
                        {
                            Date = DelBy,
                            formattedDate = DelBy.ToString("dd/MM/yyyy HH:mm"),
                            Time = valdispText,
                            dispText = valdispText,
                            extDelby = extDelby,
                            extDelbyformattedDate = extDelby.ToString("dd/MM/yyyy HH:mm"),
                            ExpressType = common.eExpressType.etGuaranteed,
                            ExpressTypeStr = common.eExpressType.etGuaranteed.ToString(),
                            value = DelBy.ToString("HH:mm"),
                            valueselected = valdispText
                        };
                        list.Add(t);
                    }
                }
            }
            //hasExtException = false;
            hasExtException = true;
            extDelby = DateTime.MinValue;
            extHr = 0;

            bool etNormal = fakeAllowedExpTypes.Contains(XDelServiceRef.eExpressType.etNormal);
            bool etNormal2 = deliveryDateTimeFrom.Date < lastDay.Date && OneWayTwoWaySelectedValue == "1";
            bool etNormal3 = deliveryDateTimeFrom.Date < lastDay.Date && OneWayTwoWaySelectedValue == "0";

            if (etNormal &&
                (etNormal2 || etNormal3))
            {
                //xs.GetExpressType uses 5:30 as "Deliver By" for FIVE THIRTY+ as well. Express Type is determined by desired express type which is the second parameter of xs.GetExpressType.

                ts = new System.TimeSpan(17, 30, 0);
                DelBy = deliveryDateTimeFrom.Date + ts;
                cutOffTS = new System.TimeSpan(11, 31, 0);
                cutOffTime = deliveryDateTimeFrom.Date + cutOffTS;
                //ze = (xs.Get_ZoneExpress((string)Session["webid"], eDeliveryDirection.tdDelivery, deliveryDateTimeFrom, (string)Session["DLPOSTALCODE"], eExpressType.etNormal)).ZoneExpressList;
                //getZE_ExceptionValues("Five Thirty+", ze, DelBy, ref extDelby, ref hasExtException, ref extHr);

                if (hasExtException)
                {
                    if (deliveryDateTimeFrom <= cutOffTime)
                    {
                        //GET Deliver By Extension Date & Time
                        ts = new System.TimeSpan(12, 0, 0);
                        extDelby = deliveryDateTimeFrom.Date + ts;
                        do
                        {
                            extDelby = extDelby.AddDays(1);
                        }
                        //while (v.vIsHoliday(DelBy.Date) || (DelBy.DayOfWeek == DayOfWeek.Saturday || DelBy.DayOfWeek == DayOfWeek.Sunday));
                        while ((extDelby.DayOfWeek == DayOfWeek.Saturday || extDelby.DayOfWeek == DayOfWeek.Sunday));

                        if (extHr > 0)
                        {
                            ts = new TimeSpan(12 + extHr, 0, 0);
                        }
                        extDelby = extDelby.Date + ts;
                        //GET Deliver By Extension Date & Time
                        extDelfrom = extDelby.Date + new TimeSpan(9, 0, 0);

                        valdispText = "Next Working Day" + "\n" + "by 12 PM";
                        ////val = sysTime.ToString("HH:mm");
                        //val = extDelby != DateTime.MinValue ? extDelby.ToString("HH:mm") : DelBy.ToString("HH:mm");
                        //t = new delByDateTime() { Date = DelBy, Time = valdispText, extDelby = extDelby, dispText = valdispText, value = val };
                        t = new delByDateTime()
                        {
                            Date = DelBy,
                            formattedDate = DelBy.ToString("dd/MM/yyyy HH:mm"),
                            Time = valdispText,
                            dispText = valdispText,
                            extDelfrom = extDelfrom,
                            extDelby = extDelby,
                            //extDelbyformattedDate = extDelby.ToString("ddd dd/MM/yyyy h:mm TT"),
                            extDelbyformattedDate = extDelby.ToString("dd/MM/yyyy h:mm tt", CultureInfo.InvariantCulture).ToUpper(),
                            ExpressType = common.eExpressType.etNormal,
                            ExpressTypeStr = common.eExpressType.etNormal.ToString(),
                            value = DelBy.ToString("HH:mm"),
                            valueselected = valdispText
                        };
                        list.Add(t);
                    }
                }
            }

            cvDBTime.ItemsSource = list;
            cvDBTime.SelectedItem = list[0];
        }
        catch (Exception ex)
        {
            string s = ex.Message;
        }
    }

    private void cvDBTimeOnItemSelected(object sender, SelectionChangedEventArgs e)
    {
        try
        {
            // Clear previous
            if (cvDBTime.ItemsSource is List<delByDateTime> all)
                foreach (var d in all) d.IsSelected = false;

            var selectedItem = e.CurrentSelection.FirstOrDefault() as delByDateTime;
            if (selectedItem != null)
            {
                selectedItem.IsSelected = true;

                vm.dbTimeSelectedActualValue = selectedItem.value;
                vm.dbTimeSelectedValue = selectedItem.valueselected;
                vm.exp1 = selectedItem.ExpressType.ToString();
                vm.expStr1 = selectedItem.ExpressTypeStr;
                vm.dbExtByDateTimeValue = selectedItem.extDelbyformattedDate;
                vm.dbExtfromDateTime = selectedItem.extDelfrom;
                vm.dbExtbyDateTime = selectedItem.extDelby;
                setDBDateTime();
            }
        }
        catch (Exception ex)
        {
            string s = ex.Message;
        }
    }

    void setDBDateTime()
    {
        try
        {
            string selectedDelbyDateStr = vm.dfDateTimeFinalValue;
            DateTime selectedDelByDate = DateTime.ParseExact(selectedDelbyDateStr, dateformats, CultureInfo.InvariantCulture, DateTimeStyles.None);
            string selectedDelByTimeStr = vm.dbTimeSelectedActualValue;
            int sHr = Int32.Parse(selectedDelByTimeStr.Substring(0, 2));
            int sMin = Int32.Parse(selectedDelByTimeStr.Substring(3, 2));
            TimeSpan ts = new TimeSpan(sHr, sMin, 0);
            selectedDelByDate = selectedDelByDate.Date + ts;
            vm.dbDateTimeFinalValue = selectedDelByDate.ToString("dd/MM/yyyy HH:mm");

            /*vm.job1.ToDateTime = vm.job1.FromDateTime.Date + ts;

            vm.job1.ExtFromDateTime = (vm.dbExtfromDateTime != DateTime.MinValue) ? vm.dbExtfromDateTime : DateTime.MinValue;
            vm.job1.ExtDateTime = (vm.dbExtbyDateTime != DateTime.MinValue) ? vm.dbExtbyDateTime : DateTime.MinValue;*/
        }
        catch (Exception e)
        {
            string s = e.Message;
        }
    }

    async void CloseBSDeliveryTime_Clicked(object sender, EventArgs e)
    {
        try
        {
            addTapGestureRecognizer();

            //await BSDeliveryTime.CloseBottomSheet();
            //BSDeliveryTime.isShowing = false;
            await CloseBottomSheetSafely(BSDeliveryTime);
        }
        catch (Exception ex)
        {
            string s = ex.Message;
        }
    }

    async void CloseBSDeliveryTimeSave_Clicked(System.Object sender, System.EventArgs e)
    {
        try
        {
            addTapGestureRecognizer();
            DeliveryTimeSaveUponClose();
            //await BSDeliveryTime.CloseBottomSheet();
            //BSDeliveryTime.isShowing = false;
            await CloseBottomSheetSafely(BSDeliveryTime);
            validateButton();
        }
        catch (Exception ex)
        {
            string s = ex.Message;
        }
    }

    void DeliveryTimeSaveUponClose()
    {
        try
        {
            DateTime delfromdatetime = DateTime.ParseExact(vm.dfDateTimeFinalValue, dateformats, CultureInfo.InvariantCulture, DateTimeStyles.None);
            DateTime delbydatetime = DateTime.ParseExact(vm.dbDateTimeFinalValue, dateformats, CultureInfo.InvariantCulture, DateTimeStyles.None);

            vm.job1.FromDateTime = DateTime.ParseExact(vm.dfDateTimeFinalValue, dateformats, CultureInfo.InvariantCulture, DateTimeStyles.None);
            vm.job1.ToDateTime = DateTime.ParseExact(vm.dbDateTimeFinalValue, dateformats, CultureInfo.InvariantCulture, DateTimeStyles.None);

            vm.job1.DLLunch_Avoid = !vm.dfDateLunchTimeValue.Equals("0");
            vm.job1.DLLunch_From = vm.dfDateLunchTimeValue.Equals("1") ? vm.job1.FromDateTime.Date + new TimeSpan(13, 0, 0) :
                    vm.dfDateLunchTimeValue.Equals("2") ? vm.job1.FromDateTime.Date + new TimeSpan(12, 0, 0) :
                    vm.dfDateLunchTimeValue.Equals("3") ? vm.job1.FromDateTime.Date + new TimeSpan(12, 0, 0) :
                    vm.dfDateLunchTimeValue.Equals("4") ? vm.job1.FromDateTime.Date + new TimeSpan(11, 30, 0) :
                    DateTime.MinValue;

            vm.job1.DLLunch_To = vm.dfDateLunchTimeValue.Equals("1") ? vm.job1.FromDateTime.Date + new TimeSpan(14, 0, 0) :
                    vm.dfDateLunchTimeValue.Equals("2") ? vm.job1.FromDateTime.Date + new TimeSpan(14, 0, 0) :
                    vm.dfDateLunchTimeValue.Equals("3") ? vm.job1.FromDateTime.Date + new TimeSpan(13, 0, 0) :
                    vm.dfDateLunchTimeValue.Equals("4") ? vm.job1.FromDateTime.Date + new TimeSpan(12, 30, 0) :
                    DateTime.MinValue;

            vm.job1.ExtFromDateTime = (vm.dbExtfromDateTime != DateTime.MinValue) ? vm.dbExtfromDateTime : DateTime.MinValue;
            vm.job1.ExtDateTime = (vm.dbExtbyDateTime != DateTime.MinValue) ? vm.dbExtbyDateTime : DateTime.MinValue;

            string exptype = vm.expStr1;
            string dfbDateTimeFinalDispText = "Select";
            if (exptype.Equals(common.eExpressType.etNormal.ToString()))
            {
                dfbDateTimeFinalDispText = delfromdatetime.ToString("dd/MM/yyyy h:mm tt", CultureInfo.InvariantCulture).ToUpper() + 
                    "\nto\n" +
                    delbydatetime.ToString("dd/MM/yyyy h:mm tt", CultureInfo.InvariantCulture).ToUpper() + 
                    "\nelse latest by\n" + vm.dbExtByDateTimeValue;
                vm.dfbDateTimeFinalDispText = dfbDateTimeFinalDispText;

                /*DateTime extFromDT = DateTime.ParseExact(vm.dbExtByDateTimeValue, dateformats, CultureInfo.InvariantCulture, DateTimeStyles.None);
                DateTime extToDT = DateTime.ParseExact(vm.dbExtByDateTimeValue , dateformats, CultureInfo.InvariantCulture, DateTimeStyles.None);
                vm.job1.ExtFromDateTime = extFromDT.Date + (new TimeSpan(9,0,0));
                vm.job1.ExtDateTime = extToDT;*/
                vm.job1.ExpressType = common.getEExpType(exptype);
            }
            else
            {
                dfbDateTimeFinalDispText = delfromdatetime.ToString("dd/MM/yyyy h:mm tt", CultureInfo.InvariantCulture).ToUpper() + 
                    "\nto\n" +
                    delbydatetime.ToString("dd/MM/yyyy h:mm tt", CultureInfo.InvariantCulture).ToUpper();
                vm.dfbDateTimeFinalDispText = dfbDateTimeFinalDispText;
                /*vm.job1.ExtFromDateTime = DateTime.MinValue;
                vm.job1.ExtDateTime = DateTime.MinValue;*/
                vm.job1.ExpressType = common.getEExpType(exptype);
            }

            lblDelTime.Text = dfbDateTimeFinalDispText;
            lblExpType.Text = getExpTypeStr();

            bool dfDateChange = String.IsNullOrEmpty(vm.dfDatePrevSelectedValue) || (!String.IsNullOrEmpty(vm.dfDatePrevSelectedValue) && !vm.dfDatePrevSelectedValue.Equals(vm.dfDateSelectedValue));
            bool dfLunchChange = !String.IsNullOrEmpty(vm.dfLunchPrevSelectedIDX) && !vm.dfLunchPrevSelectedIDX.Equals(vm.dfLunchSelectedIDX);
            bool dfTimeChange = String.IsNullOrEmpty(vm.dfTimePrevSelectedValue) || (!String.IsNullOrEmpty(vm.dfTimePrevSelectedValue) && !vm.dfTimePrevSelectedValue.Equals(vm.dfTimeSelectedValue));
            bool dbTimeChange = String.IsNullOrEmpty(vm.dbTimePrevSelectedValue) || (!String.IsNullOrEmpty(vm.dbTimePrevSelectedValue) && !vm.dbTimePrevSelectedValue.Equals(vm.dbTimeSelectedValue));

            if (dfDateChange || dfLunchChange || dfTimeChange || dbTimeChange)
            {
                if (vm.job2 != null)
                {
                    vm.job2.ReadyDateTime = vm.job1.ToDateTime;
                    vm.job2.PickByDateTime = vm.job1.ToDateTime;
                    vm.job2.PULunch_Avoid = false;
                    vm.job2.PULunch_From = DateTime.MinValue;
                    vm.job2.PULunch_To = DateTime.MinValue;
                    vm.job2.FromDateTime = DateTime.MinValue;
                    vm.job2.ToDateTime = DateTime.MinValue;
                    vm.job2.ExtFromDateTime = DateTime.MinValue;
                    vm.job2.ExtDateTime = DateTime.MinValue;
                    vm.job2.DLLunch_From = DateTime.MinValue;
                    vm.job2.DLLunch_To = DateTime.MinValue;
                    vm.job2.DLLunch_Avoid = false;
                }

                vm.rtndfDateSelectedValue = "";
                vm.rtndfDateTimeSelectedValue = "";
                vm.rtndfTimeSelectedValue = "";
                vm.rtndfDateTimeFinalValue = "";
                vm.rtndbTimeSelectedValue = "";
                vm.rtndbTimeSelectedActualValue = "";
                vm.rtndbDateTimeFinalValue = "";
                vm.rtndbExtByDateTimeValue = "";
                cvRFDate.ItemsSource = null;
                cvRFLunch.ItemsSource = null;
                cvRFTime.ItemsSource = null;
                cvRBTime.ItemsSource = null;

                vm.rtndfbDateTimeFinalDispText = "Select";
                lblRtnTime.Text = "Select";
                lblExpType2.Text = "TBA";
            }

            /*vm.dfDatePrevSelectedValue = vm.dfDateSelectedValue;
            vm.dfLunchPrevSelectedIDX = vm.dfLunchSelectedIDX;
            vm.dfTimePrevSelectedValue = vm.dfTimeSelectedValue;*/

            vm.dfDatePrevSelectedValue = vm.dfDateSelectedValue;
            vm.dfLunchPrevSelectedIDX = vm.dfLunchSelectedIDX;
            vm.dfTimePrevSelectedValue = vm.dfTimeSelectedValue;
            vm.dbTimePrevSelectedValue = vm.dbTimeSelectedValue;
        }
        catch (Exception e)
        {
            string s = e.Message;
        }
    }

    private void preSelectDelDate()
    {
        try
        {
            string selectedvalue = "";
            string selectedExtvalue = "";
            int itemCount = 0;

            itemCount = cvDFDate.ItemsSource is List<date> collection ? collection.Count : 0;
            if (itemCount > 0 && (cvDFDate.ItemsSource is List<date> items))
            {
                if (vm != null && vm.job1 != null && vm.job1.FromDateTime != DateTime.MinValue)
                {
                    selectedvalue = vm.job1.FromDateTime.ToString("dd/MM/yyyy");
                    //vm.dfDateSelectedValue = selectedvalue;
                    vm.dfDateSelectedValue = vm.job1.FromDateTime.ToString("dd/MM/yyyy HH:mm");
                }
                else
                    selectedvalue = vm.dfDateSelectedValue;

                if (!String.IsNullOrEmpty(selectedvalue))
                {
                    date item;
                    bool selected = false;
                    DateTime dt;
                    for (int i = 0; i <= items.Count - 1; i++)
                    {
                        item = items[i];
                        dt = !string.IsNullOrEmpty(item.formattedDate) ? 
                            DateTime.ParseExact(item.formattedDate, dateformats, CultureInfo.InvariantCulture, DateTimeStyles.None) : DateTime.MinValue ;

                        if (dt.ToString("dd/MM/yyyy").Equals(selectedvalue))
                        {
                            cvDFDate.SelectedItem = items[i];
                            cvDFDate.ScrollTo(item, position: ScrollToPosition.MakeVisible, animate: false);
                            selected = true;
                            break;
                        }
                    }
                    if (!selected)
                        cvDFDate.SelectedItem = items[0];
                }
                else
                {
                    cvDFDate.SelectedItem = items[0];
                }
            }


            itemCount = cvDFLunch.ItemsSource is List<lunchtime> collectionlt ? collectionlt.Count : 0;
            if (itemCount > 0 && (cvDFLunch.ItemsSource is List<lunchtime> itemslt))
            {
                selectedvalue = vm.dfDateLunchTimeValue;
                if (!String.IsNullOrEmpty(selectedvalue))
                {
                    lunchtime itemlt;
                    bool selected = false;
                    for (int i = 0; i <= itemslt.Count - 1; i++)
                    {
                        itemlt = itemslt[i];
                        if (itemlt.value.Equals(selectedvalue))
                        {
                            cvDFLunch.SelectedItem = itemslt[i];
                            cvDFLunch.ScrollTo(itemlt, position: ScrollToPosition.MakeVisible, animate: false);
                            selected = true;
                            break;
                        }
                    }
                    if (!selected)
                        cvDFLunch.SelectedItem = itemslt[0];
                }
                else
                {
                    cvDFLunch.SelectedItem = itemslt[0];
                }
            }


            itemCount = cvDFTime.ItemsSource is List<time> collectiont ? collectiont.Count : 0;
            if (itemCount > 0 && (cvDFTime.ItemsSource is List<time> itemst))
            {
                if (vm != null && vm.job1 != null && vm.job1.FromDateTime != DateTime.MinValue)
                {
                    selectedvalue = vm.job1.FromDateTime.ToString("HH:mm");
                    vm.dfTimeSelectedValue = selectedvalue;
                }
                else
                    selectedvalue = vm.dfTimeSelectedValue;

                if (!String.IsNullOrEmpty(selectedvalue))
                {
                    time itemt;
                    bool selected = false;
                    for (int i = 0; i <= itemst.Count - 1; i++)
                    {
                        itemt = itemst[i];
                        if (itemt.value.Equals(selectedvalue))
                        {
                            cvDFTime.SelectedItem = itemst[i];
                            cvDFTime.ScrollTo(itemt, position: ScrollToPosition.MakeVisible, animate: false);
                            selected = true;
                            break;
                        }
                    }
                    if (!selected)
                        cvDFTime.SelectedItem = itemst[0];
                }
                else
                {
                    cvDFTime.SelectedItem = itemst[0];
                }
                addDelByTimeList();
            }


            itemCount = cvDBTime.ItemsSource is List<delByDateTime> collectionbt ? collectionbt.Count : 0;
            if (itemCount > 0 && (cvDBTime.ItemsSource is List<delByDateTime> itemsbt))
            {

                if (vm != null && vm.job1 != null && vm.job1.ToDateTime != DateTime.MinValue)
                {
                    selectedvalue = vm.job1.ToDateTime.ToString("HH:mm");
                    vm.dbTimeSelectedValue = selectedvalue;
                }
                else
                    selectedvalue = vm.dbTimeSelectedValue;

                //selectedvalue = vm.dbTimeSelectedValue;
                if (!String.IsNullOrEmpty(selectedvalue))
                {
                    delByDateTime itembt;
                    bool selected = false;
                    for (int i = 0; i <= itemsbt.Count - 1; i++)
                    {
                        itembt = itemsbt[i];
                        if (itembt.value.Equals(selectedvalue) && selectedvalue.Equals("17:30") && vm.job1.ExpressType == eExpressType.etNormal && 
                            itembt.valueselected.Equals("Next Working Day\nby 12 PM"))
                        {
                            cvDBTime.SelectedItem = itemsbt[i];
                            cvDBTime.ScrollTo(itembt, position: ScrollToPosition.MakeVisible, animate: false);
                            selected = true;
                            break;
                        } else if (itembt.value.Equals(selectedvalue) && selectedvalue.Equals("17:30") && vm.job1.ExpressType == eExpressType.etGuaranteed)
                        {
                            cvDBTime.SelectedItem = itemsbt[i];
                            cvDBTime.ScrollTo(itembt, position: ScrollToPosition.MakeVisible, animate: false);
                            selected = true;
                            break;
                        } else if (itembt.value.Equals(selectedvalue) && !selectedvalue.Equals("17:30")) {
                            cvDBTime.SelectedItem = itemsbt[i];
                            cvDBTime.ScrollTo(itembt, position: ScrollToPosition.MakeVisible, animate: false);
                            selected = true;
                            break;
                        }

                        /*if (itembt.valueselected.Equals(selectedvalue))
                        {
                            cvDBTime.SelectedItem = itemsbt[i];
                            cvDBTime.ScrollTo(itembt, position: ScrollToPosition.MakeVisible, animate: false);
                            selected = true;
                            break;
                        }*/
                    }
                    if (!selected)
                        cvDBTime.SelectedItem = itemsbt[0];
                }
                else
                {
                    cvDBTime.SelectedItem = itemsbt[0];
                }
            }

        }
        catch (Exception e)
        {
            string s = e.Message;
        }
    }

    #endregion

    //==================================================================================================================================

    #region RtnTime

    async void show_RtnTime_BS(System.Object sender, System.EventArgs e)
    {
        try
        {
            if (vm == null)
                vm = (CreateJobVM)BindingContext;

            string dbDateTimeSelected = !String.IsNullOrEmpty(vm.dbExtByDateTimeValue) ? vm.dbExtByDateTimeValue : vm.dbDateTimeFinalValue;
            bool dbDateTimeFinalized = !String.IsNullOrEmpty(dbDateTimeSelected) && !String.IsNullOrEmpty(lblDelTime.Text) && !lblDelTime.Text.ToLower().Equals("select");

            //if (String.IsNullOrEmpty(dbDateTimeSelected))
            if (!dbDateTimeFinalized)
            {
                showmsg("", "Please select your Delivery Time first.");
            }
            else
            {
                if (fakeAllowedExpTypes == null || (fakeAllowedExpTypes != null && fakeAllowedExpTypes.Count == 0))
                {
                    if (!await addExpTypes())
                        return;
                }

                int cvRFDateCount = cvRFDate.ItemsSource is List<date> collection ? collection.Count : 0;
                if (cvRFDateCount == 0)
                {
                    addRtnFromDateList();
                }

                preSelectRtnDate();

                vm.BSDeliveryTimeopened = false;
                BSRtnTime.IsVisible = true;
                removeTapGestureRecognizer();

                await BSRtnTime.OpenBottomSheet(false);
                BSRtnTime.isShowing = true;
            }

        }
        catch (Exception ex)
        {
            string s = ex.Message;
        }
    }

    void addRtnFromDateList()
    {
        try
        {
            cvRFDate.ItemsSource = null;
            bool ExtByIsDtMinValue = !String.IsNullOrEmpty(vm.dbExtByDateTimeValue) && (DateTime.TryParse(vm.dbExtByDateTimeValue, out DateTime parsedDate) && parsedDate == DateTime.MinValue);


            string readyDateTimeStr = !ExtByIsDtMinValue ? vm.dbExtByDateTimeValue : vm.dbDateTimeFinalValue; //"dd/MM/yyyy HH:mm"
            //DateTime rdydatetime = DateTime.ParseExact(readyDateTimeStr, !ExtByIsDtMinValue ? "dd/MM/yyyy h:mm TT" : "dd/MM/yyyy HH:mm", System.Globalization.CultureInfo.InvariantCulture);
            DateTime rdydatetime = DateTime.ParseExact(readyDateTimeStr, dateformats, CultureInfo.InvariantCulture, DateTimeStyles.None);
            date d;
            List<date> list = new List<date>();
            DateTime nw = DateTime.Now;
            int dateV = 0;
            string dateddmm = "";
            String dow = nw.ToString("ddd");
            string formattedDate = nw.ToString("dd/MM/yyyy");
            TimeSpan ts;
            TimeSpan ts9 = new System.TimeSpan(9, 0, 0);

            DateTime sysTime = rdydatetime;
            DateTime customToday = DateTime.Today;
            DateTime customNow = DateTime.Now;
            DateTime et530etNmlCutOffTime = new DateTime();

            XDelServiceRef.eExpressType expType = fakeAllowedExpTypes[0];

            sysTime = sysTime.AddDays(-1);

            do
            {
                sysTime = sysTime.AddDays(1);
            } /*while (v.vIsHoliday(sysTime.Date) || (sysTime.DayOfWeek == DayOfWeek.Saturday || sysTime.DayOfWeek == DayOfWeek.Sunday));*/
            while ((sysTime.DayOfWeek == DayOfWeek.Saturday || sysTime.DayOfWeek == DayOfWeek.Sunday));

            if (sysTime.Date == customToday.Date)
            {
                et530etNmlCutOffTime = customNow;
                if (expType == XDelServiceRef.eExpressType.etNormal || expType == XDelServiceRef.eExpressType.etGuaranteed
                    || expType == XDelServiceRef.eExpressType.etPriority)
                {
                    ts = new System.TimeSpan(11, 31, 0);
                    et530etNmlCutOffTime = et530etNmlCutOffTime.Date + ts;
                }
                else if (expType == XDelServiceRef.eExpressType.etThreeHour)
                {
                    ts = new System.TimeSpan(14, 01, 0);
                    et530etNmlCutOffTime = et530etNmlCutOffTime.Date + ts;
                }
                else if (expType == XDelServiceRef.eExpressType.etTwoHour)
                {
                    ts = new System.TimeSpan(15, 01, 0);
                    et530etNmlCutOffTime = et530etNmlCutOffTime.Date + ts;
                }
                else if (expType == XDelServiceRef.eExpressType.etOneHour)
                {
                    ts = new System.TimeSpan(16, 01, 0);
                    et530etNmlCutOffTime = et530etNmlCutOffTime.Date + ts;
                }
            }
            else if (sysTime.Date > customToday.Date)
            {
                et530etNmlCutOffTime = sysTime;
                //ts = new TimeSpan(11, 31, 0);
                if (expType == XDelServiceRef.eExpressType.etNormal || expType == XDelServiceRef.eExpressType.etGuaranteed
                    || expType == XDelServiceRef.eExpressType.etPriority)
                {
                    ts = new System.TimeSpan(11, 31, 0);
                    et530etNmlCutOffTime = et530etNmlCutOffTime.Date + ts;
                }
                else if (expType == XDelServiceRef.eExpressType.etThreeHour)
                {
                    ts = new System.TimeSpan(14, 01, 0);
                    et530etNmlCutOffTime = et530etNmlCutOffTime.Date + ts;
                }
                else if (expType == XDelServiceRef.eExpressType.etTwoHour)
                {
                    ts = new System.TimeSpan(15, 01, 0);
                    et530etNmlCutOffTime = et530etNmlCutOffTime.Date + ts;
                }
                else if (expType == XDelServiceRef.eExpressType.etOneHour)
                {
                    ts = new System.TimeSpan(16, 01, 0);
                    et530etNmlCutOffTime = et530etNmlCutOffTime.Date + ts;
                }
            }

            if (sysTime >= et530etNmlCutOffTime)
            {
                do
                {
                    sysTime = sysTime.AddDays(1);
                } //while (v.vIsHoliday(sysTime.Date) || (sysTime.DayOfWeek == DayOfWeek.Saturday || sysTime.DayOfWeek == DayOfWeek.Sunday));
                while ((sysTime.DayOfWeek == DayOfWeek.Saturday || sysTime.DayOfWeek == DayOfWeek.Sunday));

                ts = new System.TimeSpan(9, 0, 0);
                sysTime = sysTime.Date + ts;

            }

            for (int i = 1; i <= 14; i++)
            {
                if (i > 1)
                {
                    do
                    {
                        sysTime = sysTime.AddDays(1);
                    } /*while (v.vIsHoliday(sysTime.Date) || (sysTime.DayOfWeek == DayOfWeek.Saturday || sysTime.DayOfWeek == DayOfWeek.Sunday));*/
                    while ((sysTime.DayOfWeek == DayOfWeek.Saturday || sysTime.DayOfWeek == DayOfWeek.Sunday));

                    //sysTime = sysTime.AddDays(1);
                    sysTime = sysTime.Date + ts9;
                }

                dow = sysTime.ToString("ddd");
                dateddmm = sysTime.ToString("dd/MM");
                dateV = sysTime.Day;
                formattedDate = sysTime.ToString("dd/MM/yyyy HH:mm");
                //formattedDate = sysTime.ToString("dd/MM/yyyy");
                d = new date() { dispText = dow + "\n" + dateddmm, formattedDate = formattedDate };
                list.Add(d);
            }

            cvRFDate.ItemsSource = list;

            int itemCount = cvRFDate.ItemsSource is List<date> collection ? collection.Count : 0;

            if (String.IsNullOrEmpty(vm.rtndfDateSelectedValue) && itemCount > 0)
            {
                if (cvRFDate.ItemsSource is List<date> items)
                {
                    // Select the first item (index 0)
                    cvRFDate.SelectedItem = items[0];
                }
            }
        }
        catch (Exception e)
        {
            string s = e.Message;
        }
    }

    private void cvRtnDateOnItemSelected(object sender, SelectionChangedEventArgs e)
    {
        try
        {
            // Clear previous
            if (cvRFDate.ItemsSource is List<date> all)
                foreach (var d in all) d.IsSelected = false;

            var selectedItem = e.CurrentSelection.FirstOrDefault() as date;
            if (selectedItem != null)
            {
                selectedItem.IsSelected = true;

                //vm.rtndfDateSelectedValue = selectedItem.formattedDate;//add datetime = selectedItem.formattedDate, rtndfDateSelectedValue will b dd/MM/yyyy
                vm.rtndfDateTimeSelectedValue = selectedItem.formattedDate;
                DateTime dt = DateTime.ParseExact(vm.rtndfDateTimeSelectedValue, "dd/MM/yyyy HH:mm", CultureInfo.InvariantCulture);
                vm.rtndfDateSelectedValue = dt.ToString("dd/MM/yyyy");

                //if (!String.IsNullOrEmpty(vm.rtndfDatePrevSelectedValue) && !vm.rtndfDatePrevSelectedValue.Equals(vm.rtndfDateSelectedValue)) //compare with datetime
                if (!String.IsNullOrEmpty(vm.rtndfDatePrevSelectedValue) && !vm.rtndfDatePrevSelectedValue.Equals(vm.rtndfDateTimeSelectedValue))
                {
                    vm.rtndfLunchPrevSelectedIDX = "";
                    vm.rtndfLunchSelectedIDX = "";
                    vm.rtndfTimeSelectedValue = "";
                    vm.rtndbTimeSelectedValue = "";
                }

                addLunchTime(cvRFLunch, 1);
                if (cvRFLunch.ItemsSource != null)
                {
                    int idx = !String.IsNullOrEmpty(vm.rtndfLunchSelectedIDX) ? int.Parse(vm.rtndfLunchSelectedIDX) : 0;
                    cvRFLunch.SelectedItem = ((List<lunchtime>)cvRFLunch.ItemsSource)[idx];
                    cvRFLunch.ScrollTo(idx, position: ScrollToPosition.MakeVisible, animate: false);
                }
            }
        }
        catch (Exception ex)
        {
            string s = ex.Message;
        }
    }

    private void cvRFLunchOnItemSelected(object sender, SelectionChangedEventArgs e)
    {
        try
        {
            // Clear previous
            if (cvRFLunch.ItemsSource is List<lunchtime> all)
                foreach (var d in all) d.IsSelected = false;

            var selectedItem = e.CurrentSelection.FirstOrDefault() as lunchtime;
            if (selectedItem != null)
            {
                selectedItem.IsSelected = true;

                int selectedIndex = cvRFLunch.ItemsSource.Cast<lunchtime>()
                                 .ToList().IndexOf(selectedItem);
                vm.rtndfLunchSelectedIDX = selectedIndex.ToString();
                vm.rtndfDateLunchTimeValue = selectedItem.value;

                /*vm.job2.DLLunch_Avoid = !vm.rtndfDateLunchTimeValue.Equals("0");
                vm.job2.DLLunch_From = vm.rtndfDateLunchTimeValue.Equals("1") ? vm.job2.FromDateTime.Date + new TimeSpan(13, 0, 0) :
                        vm.rtndfDateLunchTimeValue.Equals("2") ? vm.job2.FromDateTime.Date + new TimeSpan(12, 0, 0) :
                        vm.rtndfDateLunchTimeValue.Equals("3") ? vm.job2.FromDateTime.Date + new TimeSpan(12, 0, 0) :
                        vm.rtndfDateLunchTimeValue.Equals("4") ? vm.job2.FromDateTime.Date + new TimeSpan(11, 30, 0) :
                        DateTime.MinValue;

                vm.job2.DLLunch_To = vm.rtndfDateLunchTimeValue.Equals("1") ? vm.job2.FromDateTime.Date + new TimeSpan(14, 0, 0) :
                        vm.rtndfDateLunchTimeValue.Equals("2") ? vm.job2.FromDateTime.Date + new TimeSpan(14, 0, 0) :
                        vm.rtndfDateLunchTimeValue.Equals("3") ? vm.job2.FromDateTime.Date + new TimeSpan(13, 0, 0) :
                        vm.rtndfDateLunchTimeValue.Equals("4") ? vm.job2.FromDateTime.Date + new TimeSpan(12, 30, 0) :
                        DateTime.MinValue;*/

                if (!String.IsNullOrEmpty(selectedItem.value))
                    addRtnFromTimeList();
            }
        }
        catch (Exception ex)
        {
            string s = ex.Message;
        }
    }

    void addRtnFromTimeList()
    {
        try
        {
            cvRFTime.ItemsSource = null;
            string valdispText = "";
            string val = "";
            time t;
            List<time> list = new List<time>();
            //string selecteddfDateTimeStr = vm.rtndfDateSelectedValue;
            string selecteddfDateTimeStr = vm.rtndfDateTimeSelectedValue;
            bool allExpTypeBlockedForToday = false;
            DateTime originalSysTime = DateTime.ParseExact(selecteddfDateTimeStr, dateformats, CultureInfo.InvariantCulture, DateTimeStyles.None);
            DateTime sysTime = DateTime.Now;
            DateTime compareTime;
            TimeSpan ts;
            TimeSpan ThreeHoursTS = new System.TimeSpan(14, 1, 0);
            string sTime = "";

            //XDelServiceRef.XDelOnlineSettings cxo = null;
            //XDelSys.XDelOnlineSettings xo = null;

            date xo = new date();
            date cxo = new date();
            Int32 GraceMinutes = 0;

            List<string> lunchhours = getLunchTimeTimesListNew(vm.rtndfDateLunchTimeValue);
            List<string> timesL = times.ToList();
            string[] timesR = null;


            XDelServiceRef.eExpressType expType = fakeAllowedExpTypes[0];

            if (lunchhours != null && lunchhours.Count > 0)
            {
                for (int i = timesL.Count - 1; i >= 0; i--)
                {
                    if (lunchhours.Contains(timesL[i]))
                    {
                        timesL.RemoveAt(i);
                    }
                }
            }

            timesR = timesL.ToArray();

            if (allExpTypeBlockedForToday)
            {

            }
            else
            {
                for (int i = 0; i <= timesR.Length - 1; i++)
                {
                    sysTime = originalSysTime;
                    ts = System.TimeSpan.Parse(timesR[i]);
                    sTime = (DateTime.Today.Add(ts).ToString("h:mm tt"));
                    compareTime = sysTime;
                    sysTime = sysTime.Date + ts;

                    if (sysTime >= compareTime)
                    {
                        if (expType == XDelServiceRef.eExpressType.etThreeHour || expType == XDelServiceRef.eExpressType.etTwoHour ||
                            expType == XDelServiceRef.eExpressType.etOneHour)
                        {
                            if (expType == XDelServiceRef.eExpressType.etThreeHour)
                            {
                                ThreeHoursTS = new System.TimeSpan(14, 1, 0);
                            }
                            else if (expType == XDelServiceRef.eExpressType.etTwoHour)
                            {
                                ThreeHoursTS = new System.TimeSpan(15, 1, 0);
                            }
                            else if (expType == XDelServiceRef.eExpressType.etOneHour)
                            {
                                ThreeHoursTS = new System.TimeSpan(16, 1, 0);
                            }

                            if (ts < ThreeHoursTS)
                            {
                                //if (xo != null && xo.GraceMinutes > 0)
                                if (xo != null && GraceMinutes > 0)
                                {
                                    if (list.Count == 0)
                                    {
                                        valdispText = sysTime.ToString("h:mm tt", CultureInfo.InvariantCulture).ToUpper();
                                        val = sysTime.ToString("HH:mm");
                                        t = new time() { dispText = valdispText, value = val, IsVisible = true };
                                        list.Add(t);
                                        i--;
                                    }
                                    else
                                    {
                                        valdispText = sysTime.ToString("h:mm tt", CultureInfo.InvariantCulture).ToUpper();
                                        val = sysTime.ToString("HH:mm");
                                        t = new time() { dispText = valdispText, value = val, IsVisible = true };
                                        list.Add(t);

                                        //}
                                    }
                                }
                                //else if (xo != null && xo.GraceMinutes == 0)
                                else if (xo != null && GraceMinutes == 0)
                                {
                                    //if (cxo != null && cxo.GraceMinutes > 0)
                                    if (cxo != null && GraceMinutes > 0)
                                    {
                                        if (list.Count == 0)
                                        {
                                            valdispText = sysTime.ToString("h:mm tt", CultureInfo.InvariantCulture).ToUpper();
                                            val = sysTime.ToString("HH:mm");
                                            t = new time() { dispText = valdispText, value = val, IsVisible = true };
                                            list.Add(t);
                                            i--;
                                        }
                                        else
                                        {
                                            valdispText = sysTime.ToString("h:mm tt", CultureInfo.InvariantCulture).ToUpper();
                                            val = sysTime.ToString("HH:mm");
                                            t = new time() { dispText = valdispText, value = val, IsVisible = true };
                                            list.Add(t);
                                            //}
                                        }
                                    }
                                    //else if (cxo != null && cxo.GraceMinutes == 0)
                                    else if (cxo != null && GraceMinutes == 0)
                                    {
                                        valdispText = sysTime.ToString("h:mm tt", CultureInfo.InvariantCulture).ToUpper();
                                        val = sysTime.ToString("HH:mm");
                                        t = new time() { dispText = valdispText, value = val, IsVisible = true };
                                        list.Add(t);
                                    }
                                }
                            }
                        }
                        else if (expType == XDelServiceRef.eExpressType.etNormal || expType == XDelServiceRef.eExpressType.etGuaranteed ||
                                 expType == XDelServiceRef.eExpressType.etPriority)
                        {
                            valdispText = sysTime.ToString("h:mm tt", CultureInfo.InvariantCulture).ToUpper();
                            val = sysTime.ToString("HH:mm");
                            t = new time() { dispText = valdispText, value = val, IsVisible = true };
                            list.Add(t);
                        }
                    }

                }


            }

            cvRFTime.ItemsSource = list;
            cvRFTime.SelectedItem = list[0];
        }
        catch (Exception e)
        {
            string s = e.Message;
        }
    }

    private void cvRFTimeOnItemSelected(object sender, SelectionChangedEventArgs e)
    {
        try
        {
            // Clear previous
            if (cvRFTime.ItemsSource is List<time> all)
                foreach (var d in all) d.IsSelected = false;

            var selectedItem = e.CurrentSelection.FirstOrDefault() as time;
            if (selectedItem != null)
            {
                selectedItem.IsSelected = true;

                vm.rtndfTimeSelectedValue = selectedItem.value;
                setRFDateTime();
                if (!String.IsNullOrEmpty(selectedItem.value))
                    addRtnByTimeList();
            }
        }
        catch (Exception ex)
        {
            string s = ex.Message;
        }
    }

    void setRFDateTime()
    {
        try
        {
            //string selectedRtnFromDateStr = vm.rtndfDateSelectedValue;
            string selectedRtnFromDateStr = vm.rtndfDateTimeSelectedValue;
            DateTime selectedRtnFromDate = DateTime.ParseExact(selectedRtnFromDateStr, dateformats, CultureInfo.InvariantCulture, DateTimeStyles.None);
            string selectedRtnFromTimeStr = vm.rtndfTimeSelectedValue;
            int sHr = Int32.Parse(selectedRtnFromTimeStr.Substring(0, 2));
            int sMin = Int32.Parse(selectedRtnFromTimeStr.Substring(3, 2));
            TimeSpan ts = new TimeSpan(sHr, sMin, 0);
            selectedRtnFromDate = selectedRtnFromDate.Date + ts;
            vm.rtndfDateTimeFinalValue = selectedRtnFromDate.ToString("dd/MM/yyyy HH:mm");

            /*vm.rtndfDatePrevSelectedValue = vm.rtndfDateSelectedValue;
            vm.rtndfLunchPrevSelectedIDX = vm.rtndfLunchSelectedIDX;
            vm.rtndfTimePrevSelectedValue = vm.rtndfTimeSelectedValue;*/
        }
        catch (Exception e)
        {
            string s = e.Message;
        }
    }

    void addRtnByTimeList()
    {
        try
        {
            cvRBTime.ItemsSource = null;
            string valdispText = "";
            string val = "";
            delByDateTime t;
            List<delByDateTime> list = new List<delByDateTime>();
            string selectedDelFromDateTimeStr = vm.rtndfDateTimeFinalValue;
            bool allExpTypeBlockedForToday = false;
            DateTime originalSysTime = DateTime.ParseExact(selectedDelFromDateTimeStr, dateformats, CultureInfo.InvariantCulture, DateTimeStyles.None);
            DateTime sysTime = DateTime.Now;
            DateTime compareTime;

            string readyDateTimeStr = vm.rtndfDateTimeFinalValue;
            DateTime pickupDateTimeFrom = DateTime.ParseExact(readyDateTimeStr, dateformats, CultureInfo.InvariantCulture, DateTimeStyles.None);

            DateTime deliveryDateTimeFrom = DateTime.ParseExact(selectedDelFromDateTimeStr, dateformats, CultureInfo.InvariantCulture, DateTimeStyles.None);

            string lblCDLTvText = "0";
            DateTime lunchPUAvoidDateTimeTo = new DateTime();
            string lblDFLTvText = vm.rtndfDateLunchTimeValue;
            DateTime lunchDELAvoidDateTimeTo = new DateTime();
            bool noPULunchAvoidance = false;
            bool noDELLunchAvoidance = false;
            DateTime DelBy = new DateTime();
            DateTime cutOffTime;
            TimeSpan cutOffTS;

            TimeSpan ts;
            TimeSpan ThreeHoursTS = new System.TimeSpan(14, 1, 0);
            string sTime = "";

            //XDelServiceRef.XDelOnlineSettings cxo = null;
            //XDelSys.XDelOnlineSettings xo = null;

            date xo = new date();
            date cxo = new date();
            Int32 GraceMinutes = 0;

            List<string> lunchhours = getLunchTimeTimesListNew(vm.rtndfDateLunchTimeValue);
            List<string> timesL = times.ToList();
            string[] timesR = null;
            int itemCount = 0;
            string OneWayTwoWaySelectedValue = "0";

            XDelServiceRef.eExpressType expType = fakeAllowedExpTypes[0];

            if (lblCDLTvText.Equals("4"))
            {
                lunchPUAvoidDateTimeTo = pickupDateTimeFrom.Date + new TimeSpan(12, 30, 0);
            }
            else if (lblCDLTvText.Equals("3"))
            {
                lunchPUAvoidDateTimeTo = pickupDateTimeFrom.Date + new TimeSpan(13, 0, 0);
            }
            else if (lblCDLTvText.Equals("2") || lblCDLTvText.Equals("1"))
            {
                lunchPUAvoidDateTimeTo = pickupDateTimeFrom.Date + new TimeSpan(14, 0, 0);
            }
            else
            {
                noPULunchAvoidance = true;
            }

            if (lblDFLTvText.Equals("4"))
            {
                lunchDELAvoidDateTimeTo = deliveryDateTimeFrom.Date + new TimeSpan(12, 30, 0);
            }
            else if (lblDFLTvText.Equals("3"))
            {
                lunchDELAvoidDateTimeTo = deliveryDateTimeFrom.Date + new TimeSpan(13, 0, 0);
            }
            else if (lblDFLTvText.Equals("2") || lblDFLTvText.Equals("1"))
            {
                lunchDELAvoidDateTimeTo = deliveryDateTimeFrom.Date + new TimeSpan(14, 0, 0);
            }
            else
            {
                noDELLunchAvoidance = true;
            }


            itemCount = cvDFDate.ItemsSource is List<date> collection ? collection.Count : 0;
            date lastPUitem = null;
            if (itemCount > 0 && (cvDate.ItemsSource is List<date> items))
            {
                lastPUitem = items[itemCount - 1];
            }

            DateTime lastDay = DateTime.Now;
            lastDay = lastPUitem != null ? DateTime.ParseExact(lastPUitem.formattedDate, dateformats, CultureInfo.InvariantCulture, DateTimeStyles.None) : DateTime.Now; //abit different from XDelOnline ServiceType.aspx.cs line 1866
            //Boolean hasExtException = false;
            Boolean hasExtException = true;
            DateTime extDelfrom = DateTime.MinValue;
            DateTime extDelby = DateTime.MinValue;
            int extHr = 0;

            if (fakeAllowedExpTypes.Contains(XDelServiceRef.eExpressType.etOneHour) &&
                ((deliveryDateTimeFrom >= lunchDELAvoidDateTimeTo && pickupDateTimeFrom >= lunchPUAvoidDateTimeTo) || (noDELLunchAvoidance && noPULunchAvoidance)))
            {
                DelBy = deliveryDateTimeFrom.AddHours(1.5);
                cutOffTS = new System.TimeSpan(16, 00, 0);
                cutOffTime = deliveryDateTimeFrom.Date + cutOffTS;
                //ze = (xs.Get_ZoneExpress((string)Session["webid"], eDeliveryDirection.tdDelivery, deliveryDateTimeFrom, (string)Session["DLPOSTALCODE"], eExpressType.etOneHour)).ZoneExpressList;
                //getZE_ExceptionValues("1.5 Hours", ze, DelBy, ref extDelby, ref hasExtException, ref extHr);

                if (hasExtException)
                {
                    if (deliveryDateTimeFrom <= cutOffTime)
                    {
                        //dr = newDelByDT.NewRow();
                        //dr["Date"] = DelBy;
                        //dr["Time"] = extDelby != DateTime.MinValue ? extDelby.ToString("h:mm TT") : DelBy.ToString("h:mm TT");
                        //dr["extDelby"] = extDelby;

                        //XDelServiceRef.JobTimingStructure job = xs.GetExpressType(uid, eExpressType.etOneHour, deliveryDateTimeFrom, DelBy, null, null, contentType);
                        //dr["ExpressType"] = getExpressSelect(job.ExpressType);
                        //newDelByDT.Rows.Add(dr);
                        //removeHigherExpSvcDueConflict(ref newDelByDT, DelBy);

                        valdispText = extDelby != DateTime.MinValue ? extDelby.ToString("h:mm tt", CultureInfo.InvariantCulture).ToUpper() : 
                            DelBy.ToString("h:mm tt", CultureInfo.InvariantCulture).ToUpper();
                        ////val = sysTime.ToString("HH:mm");
                        //val = extDelby != DateTime.MinValue ? extDelby.ToString("HH:mm") : DelBy.ToString("HH:mm");
                        t = new delByDateTime()
                        {
                            Date = DelBy,
                            formattedDate = DelBy.ToString("dd/MM/yyyy HH:mm"),
                            Time = valdispText,
                            dispText = valdispText,
                            extDelby = extDelby,
                            extDelbyformattedDate = extDelby.ToString("dd/MM/yyyy HH:mm"),
                            ExpressType = common.eExpressType.etOneHour,
                            ExpressTypeStr = common.eExpressType.etOneHour.ToString(),
                            value = DelBy.ToString("HH:mm"),
                            valueselected = valdispText
                        };
                        list.Add(t);
                    }
                }
            }
            //hasExtException = false;
            hasExtException = true;
            extDelby = DateTime.MinValue;
            extHr = 0;

            if (fakeAllowedExpTypes.Contains(XDelServiceRef.eExpressType.etTwoHour) &&
                ((deliveryDateTimeFrom >= lunchDELAvoidDateTimeTo && pickupDateTimeFrom >= lunchPUAvoidDateTimeTo) || (noDELLunchAvoidance && noPULunchAvoidance)))
            {
                DelBy = deliveryDateTimeFrom.AddHours(2.5);
                cutOffTS = new System.TimeSpan(15, 00, 0);
                cutOffTime = deliveryDateTimeFrom.Date + cutOffTS;
                //ze = (xs.Get_ZoneExpress((string)Session["webid"], eDeliveryDirection.tdDelivery, deliveryDateTimeFrom, (string)Session["DLPOSTALCODE"], eExpressType.etTwoHour)).ZoneExpressList;
                //getZE_ExceptionValues("2.5 Hours", ze, DelBy, ref extDelby, ref hasExtException, ref extHr);

                if (hasExtException)
                {
                    if (deliveryDateTimeFrom <= cutOffTime)
                    {
                        //dr = newDelByDT.NewRow();
                        //dr["Date"] = DelBy;
                        //dr["Time"] = extDelby != DateTime.MinValue ? extDelby.ToString("h:mm TT") : DelBy.ToString("h:mm TT");
                        //dr["extDelby"] = extDelby;

                        //XDelServiceRef.JobTimingStructure job = xs.GetExpressType(uid, eExpressType.etTwoHour, deliveryDateTimeFrom, DelBy, null, null, contentType);
                        //dr["ExpressType"] = getExpressSelect(job.ExpressType);
                        //newDelByDT.Rows.Add(dr);
                        //removeHigherExpSvcDueConflict(ref newDelByDT, DelBy);

                        valdispText = extDelby != DateTime.MinValue ? extDelby.ToString("h:mm tt", CultureInfo.InvariantCulture).ToUpper() : 
                            DelBy.ToString("h:mm tt", CultureInfo.InvariantCulture).ToUpper();
                        ////val = sysTime.ToString("HH:mm");
                        //val = extDelby != DateTime.MinValue ? extDelby.ToString("HH:mm") : DelBy.ToString("HH:mm");
                        t = new delByDateTime()
                        {
                            Date = DelBy,
                            formattedDate = DelBy.ToString("dd/MM/yyyy HH:mm"),
                            Time = valdispText,
                            dispText = valdispText,
                            extDelby = extDelby,
                            extDelbyformattedDate = extDelby.ToString("dd/MM/yyyy HH:mm"),
                            ExpressType = common.eExpressType.etTwoHour,
                            ExpressTypeStr = common.eExpressType.etTwoHour.ToString(),
                            value = DelBy.ToString("HH:mm"),
                            valueselected = valdispText
                        };
                        list.Add(t);
                    }
                }
            }
            //hasExtException = false;
            hasExtException = true;
            extDelby = DateTime.MinValue;
            extHr = 0;

            if (fakeAllowedExpTypes.Contains(XDelServiceRef.eExpressType.etThreeHour) &&
                ((deliveryDateTimeFrom >= lunchDELAvoidDateTimeTo && pickupDateTimeFrom >= lunchPUAvoidDateTimeTo) || (noDELLunchAvoidance && noPULunchAvoidance)))
            {
                DelBy = deliveryDateTimeFrom.AddHours(3.5);
                cutOffTS = new System.TimeSpan(14, 00, 0);
                cutOffTime = deliveryDateTimeFrom.Date + cutOffTS;
                //ze = (xs.Get_ZoneExpress((string)Session["webid"], eDeliveryDirection.tdDelivery, deliveryDateTimeFrom, (string)Session["DLPOSTALCODE"], eExpressType.etThreeHour)).ZoneExpressList;
                //getZE_ExceptionValues("3.5 Hours", ze, DelBy, ref extDelby, ref hasExtException, ref extHr);

                if (hasExtException)
                {
                    if (deliveryDateTimeFrom <= cutOffTime)
                    {
                        //dr = newDelByDT.NewRow();
                        //dr["Date"] = DelBy;
                        //dr["Time"] = extDelby != DateTime.MinValue ? extDelby.ToString("h:mm TT") : DelBy.ToString("h:mm TT");
                        //dr["extDelby"] = extDelby;

                        //XDelServiceRef.JobTimingStructure job = xs.GetExpressType(uid, eExpressType.etThreeHour, deliveryDateTimeFrom, DelBy, null, null, contentType);
                        //dr["ExpressType"] = getExpressSelect(job.ExpressType);
                        //newDelByDT.Rows.Add(dr);
                        //removeHigherExpSvcDueConflict(ref newDelByDT, DelBy);

                        valdispText = extDelby != DateTime.MinValue ? extDelby.ToString("h:mm tt", CultureInfo.InvariantCulture).ToUpper() : 
                            DelBy.ToString("h:mm tt", CultureInfo.InvariantCulture).ToUpper();
                        ////val = sysTime.ToString("HH:mm");
                        //val = extDelby != DateTime.MinValue ? extDelby.ToString("HH:mm") : DelBy.ToString("HH:mm");
                        t = new delByDateTime()
                        {
                            Date = DelBy,
                            formattedDate = DelBy.ToString("dd/MM/yyyy HH:mm"),
                            Time = valdispText,
                            dispText = valdispText,
                            extDelby = extDelby,
                            extDelbyformattedDate = extDelby.ToString("dd/MM/yyyy HH:mm"),
                            ExpressType = common.eExpressType.etThreeHour,
                            ExpressTypeStr = common.eExpressType.etThreeHour.ToString(),
                            value = DelBy.ToString("HH:mm"),
                            valueselected = valdispText
                        };
                        list.Add(t);
                    }
                }
            }
            //hasExtException = false;
            hasExtException = true;
            extDelby = DateTime.MinValue;
            extHr = 0;

            if (fakeAllowedExpTypes.Contains(XDelServiceRef.eExpressType.etPriority) &&
                ((deliveryDateTimeFrom >= lunchDELAvoidDateTimeTo && pickupDateTimeFrom >= lunchPUAvoidDateTimeTo) || (noDELLunchAvoidance && noPULunchAvoidance))
                && (((deliveryDateTimeFrom.Date < lastDay.Date && OneWayTwoWaySelectedValue == "1")) || (deliveryDateTimeFrom.Date <= lastDay.Date && OneWayTwoWaySelectedValue == "0")))
            {
                ts = new System.TimeSpan(16, 30, 0);
                DelBy = deliveryDateTimeFrom.Date + ts;
                cutOffTS = new System.TimeSpan(11, 31, 0);
                cutOffTime = deliveryDateTimeFrom.Date + cutOffTS;
                //ze = (xs.Get_ZoneExpress((string)Session["webid"], eDeliveryDirection.tdDelivery, deliveryDateTimeFrom, (string)Session["DLPOSTALCODE"], eExpressType.etPriority)).ZoneExpressList;
                //getZE_ExceptionValues("Four Thirty", ze, DelBy, ref extDelby, ref hasExtException, ref extHr);

                if (hasExtException)
                {
                    if (deliveryDateTimeFrom <= cutOffTime)
                    {
                        //dr = newDelByDT.NewRow();
                        //dr["Date"] = DelBy;
                        //dr["Time"] = extDelby != DateTime.MinValue ? extDelby.ToString("h:mm TT") : DelBy.ToString("h:mm TT");
                        //dr["extDelby"] = extDelby;

                        //XDelServiceRef.JobTimingStructure job = xs.GetExpressType(uid, eExpressType.etPriority, deliveryDateTimeFrom, DelBy, null, null, contentType);
                        //dr["ExpressType"] = getExpressSelect(job.ExpressType);
                        //newDelByDT.Rows.Add(dr);
                        //removeHigherExpSvcDueConflict(ref newDelByDT, DelBy);

                        valdispText = extDelby != DateTime.MinValue ? extDelby.ToString("h:mm tt", CultureInfo.InvariantCulture).ToUpper() : 
                            DelBy.ToString("h:mm tt", CultureInfo.InvariantCulture).ToUpper();
                        //val = sysTime.ToString("HH:mm");
                        t = new delByDateTime()
                        {
                            Date = DelBy,
                            formattedDate = DelBy.ToString("dd/MM/yyyy HH:mm"),
                            Time = valdispText,
                            dispText = valdispText,
                            extDelby = extDelby,
                            extDelbyformattedDate = extDelby.ToString("dd/MM/yyyy HH:mm"),
                            ExpressType = common.eExpressType.etPriority,
                            ExpressTypeStr = common.eExpressType.etPriority.ToString(),
                            value = DelBy.ToString("HH:mm"),
                            valueselected = valdispText
                        };
                        list.Add(t);
                    }
                }
            }
            //hasExtException = false;
            hasExtException = true;
            extDelby = DateTime.MinValue;
            extHr = 0;

            if (fakeAllowedExpTypes.Contains(XDelServiceRef.eExpressType.etGuaranteed) &&
                (((deliveryDateTimeFrom.Date < lastDay.Date && OneWayTwoWaySelectedValue == "1")) || (deliveryDateTimeFrom.Date <= lastDay.Date && OneWayTwoWaySelectedValue == "0")))
            {
                ts = new System.TimeSpan(17, 30, 0);
                DelBy = deliveryDateTimeFrom.Date + ts;
                cutOffTS = new System.TimeSpan(11, 31, 0);
                cutOffTime = deliveryDateTimeFrom.Date + cutOffTS;
                //ze = (xs.Get_ZoneExpress((string)Session["webid"], eDeliveryDirection.tdDelivery, deliveryDateTimeFrom, (string)Session["DLPOSTALCODE"], eExpressType.etGuaranteed)).ZoneExpressList;
                //getZE_ExceptionValues("Five Thirty", ze, DelBy, ref extDelby, ref hasExtException, ref extHr);

                if (hasExtException)
                {
                    if (deliveryDateTimeFrom <= cutOffTime)
                    {
                        //dr = newDelByDT.NewRow();
                        //dr["Date"] = DelBy;
                        //dr["Time"] = extDelby != DateTime.MinValue ? extDelby.ToString("h:mm TT") : DelBy.ToString("h:mm TT");
                        //dr["extDelby"] = extDelby;

                        //XDelServiceRef.JobTimingStructure job = xs.GetExpressType(uid, eExpressType.etGuaranteed, deliveryDateTimeFrom, DelBy, null, null, contentType);
                        //dr["ExpressType"] = getExpressSelect(job.ExpressType);
                        //newDelByDT.Rows.Add(dr);
                        //removeHigherExpSvcDueConflict(ref newDelByDT, DelBy);

                        valdispText = extDelby != DateTime.MinValue ? extDelby.ToString("h:mm tt", CultureInfo.InvariantCulture).ToUpper() :
                            DelBy.ToString("h:mm tt", CultureInfo.InvariantCulture).ToUpper();
                        ////val = sysTime.ToString("HH:mm");
                        //val = extDelby != DateTime.MinValue ? extDelby.ToString("HH:mm") : DelBy.ToString("HH:mm");
                        t = new delByDateTime()
                        {
                            Date = DelBy,
                            formattedDate = DelBy.ToString("dd/MM/yyyy HH:mm"),
                            Time = valdispText,
                            dispText = valdispText,
                            extDelby = extDelby,
                            extDelbyformattedDate = extDelby.ToString("dd/MM/yyyy HH:mm"),
                            ExpressType = common.eExpressType.etGuaranteed,
                            ExpressTypeStr = common.eExpressType.etGuaranteed.ToString(),
                            value = DelBy.ToString("HH:mm"),
                            valueselected = valdispText
                        };
                        list.Add(t);
                    }
                }
            }
            //hasExtException = false;
            hasExtException = true;
            extDelby = DateTime.MinValue;
            extHr = 0;

            if (fakeAllowedExpTypes.Contains(XDelServiceRef.eExpressType.etNormal) &&
                (((deliveryDateTimeFrom.Date < lastDay.Date && OneWayTwoWaySelectedValue == "1")) || (deliveryDateTimeFrom.Date < lastDay.Date && OneWayTwoWaySelectedValue == "0")))
            {
                //xs.GetExpressType uses 5:30 as "Deliver By" for FIVE THIRTY+ as well. Express Type is determined by desired express type which is the second parameter of xs.GetExpressType.

                ts = new System.TimeSpan(17, 30, 0);
                DelBy = deliveryDateTimeFrom.Date + ts;
                cutOffTS = new System.TimeSpan(11, 31, 0);
                cutOffTime = deliveryDateTimeFrom.Date + cutOffTS;
                //ze = (xs.Get_ZoneExpress((string)Session["webid"], eDeliveryDirection.tdDelivery, deliveryDateTimeFrom, (string)Session["DLPOSTALCODE"], eExpressType.etNormal)).ZoneExpressList;
                //getZE_ExceptionValues("Five Thirty+", ze, DelBy, ref extDelby, ref hasExtException, ref extHr);

                if (hasExtException)
                {
                    if (deliveryDateTimeFrom <= cutOffTime)
                    {
                        //GET Deliver By Extension Date & Time
                        ts = new System.TimeSpan(12, 0, 0);
                        extDelby = deliveryDateTimeFrom.Date + ts;
                        do
                        {
                            extDelby = extDelby.AddDays(1);
                        }
                        //while (v.vIsHoliday(DelBy.Date) || (DelBy.DayOfWeek == DayOfWeek.Saturday || DelBy.DayOfWeek == DayOfWeek.Sunday));
                        while ((extDelby.DayOfWeek == DayOfWeek.Saturday || extDelby.DayOfWeek == DayOfWeek.Sunday));

                        if (extHr > 0)
                        {
                            ts = new TimeSpan(12 + extHr, 0, 0);
                        }
                        extDelby = extDelby.Date + ts;
                        //GET Deliver By Extension Date & Time
                        extDelfrom = extDelby.Date + new TimeSpan(9, 0, 0);

                        valdispText = "Next Working Day" + "\n" + "by 12 PM";
                        ////val = sysTime.ToString("HH:mm");
                        //val = extDelby != DateTime.MinValue ? extDelby.ToString("HH:mm") : DelBy.ToString("HH:mm");
                        //t = new delByDateTime() { Date = DelBy, Time = valdispText, extDelby = extDelby, dispText = valdispText, value = val };
                        t = new delByDateTime()
                        {
                            Date = DelBy,
                            formattedDate = DelBy.ToString("dd/MM/yyyy HH:mm"),
                            Time = valdispText,
                            dispText = valdispText,
                            extDelfrom = extDelfrom,
                            extDelby = extDelby,
                            //extDelbyformattedDate = extDelby.ToString("ddd dd/MM/yyyy h:mm TT"),
                            extDelbyformattedDate = extDelby.ToString("dd/MM/yyyy h:mm tt", CultureInfo.InvariantCulture).ToUpper(),
                            ExpressType = common.eExpressType.etNormal,
                            ExpressTypeStr = common.eExpressType.etNormal.ToString(),
                            value = DelBy.ToString("HH:mm"),
                            valueselected = valdispText
                        };
                        list.Add(t);
                    }
                }
            }

            cvRBTime.ItemsSource = list;
            cvRBTime.SelectedItem = list[0];
        }
        catch (Exception e)
        {
            string s = e.Message;
        }
    }

    private void cvRBTimeOnItemSelected(object sender, SelectionChangedEventArgs e)
    {
        try
        {
            // Clear previous
            if (cvRBTime.ItemsSource is List<delByDateTime> all)
                foreach (var d in all) d.IsSelected = false;

            var selectedItem = e.CurrentSelection.FirstOrDefault() as delByDateTime;
            if (selectedItem != null)
            {
                selectedItem.IsSelected = true;

                vm.rtndbTimeSelectedActualValue = selectedItem.value;
                vm.rtndbTimeSelectedValue = selectedItem.valueselected;
                vm.exp2 = selectedItem.ExpressType.ToString();
                vm.expStr2 = selectedItem.ExpressTypeStr;

                vm.rtndbExtByDateTimeValue = selectedItem.extDelbyformattedDate;
                vm.rtndbExtfromDateTime = selectedItem.extDelfrom;
                vm.rtndbExtbyDateTime = selectedItem.extDelby;
                setRBDateTime();
            }
        }
        catch (Exception ex)
        {
            string s = ex.Message;
        }
    }

    void setRBDateTime()
    {
        try
        {
            string selectedDelbyDateStr = vm.rtndfDateTimeFinalValue;
            DateTime selectedDelByDate = DateTime.ParseExact(selectedDelbyDateStr, dateformats, CultureInfo.InvariantCulture, DateTimeStyles.None);
            string selectedDelByTimeStr = vm.rtndbTimeSelectedActualValue;
            int sHr = Int32.Parse(selectedDelByTimeStr.Substring(0, 2));
            int sMin = Int32.Parse(selectedDelByTimeStr.Substring(3, 2));
            TimeSpan ts = new TimeSpan(sHr, sMin, 0);
            selectedDelByDate = selectedDelByDate.Date + ts;
            vm.rtndbDateTimeFinalValue = selectedDelByDate.ToString("dd/MM/yyyy HH:mm");

            //vm.job2.ToDateTime = vm.job2.FromDateTime.Date + ts;

        }
        catch (Exception e)
        {
            string s = e.Message;
        }
    }

    async void CloseBSRtnTime_Clicked(object sender, EventArgs e)
    {
        try
        {
            addTapGestureRecognizer();

            //await BSRtnTime.CloseBottomSheet();
            //BSRtnTime.isShowing = false;
            await CloseBottomSheetSafely(BSRtnTime);
        }
        catch (Exception ex)
        {
            string s = ex.Message;
        }
    }

    async void CloseBSRtnTimeSave_Clicked(System.Object sender, System.EventArgs e)
    {
        try
        {
            addTapGestureRecognizer();
            ReturnTimeSaveUponClose();
            //await BSRtnTime.CloseBottomSheet();
            //BSRtnTime.isShowing = false;
            await CloseBottomSheetSafely(BSRtnTime);
            validateButton();
        }
        catch (Exception ex)
        {
            string s = ex.Message;
        }
    }

    void ReturnTimeSaveUponClose()
    {
        try
        {
            DateTime delfromdatetime = DateTime.ParseExact(vm.rtndfDateTimeFinalValue, dateformats, CultureInfo.InvariantCulture, DateTimeStyles.None);
            DateTime delbydatetime = DateTime.ParseExact(vm.rtndbDateTimeFinalValue, dateformats, CultureInfo.InvariantCulture, DateTimeStyles.None);

            vm.job2.ReadyDateTime = delfromdatetime;
            vm.job2.FromDateTime = delfromdatetime;
            vm.job2.ToDateTime = delbydatetime;

            vm.job2.DLLunch_Avoid = !vm.rtndfDateLunchTimeValue.Equals("0");
            vm.job2.DLLunch_From = vm.rtndfDateLunchTimeValue.Equals("1") ? vm.job2.FromDateTime.Date + new TimeSpan(13, 0, 0) :
                    vm.rtndfDateLunchTimeValue.Equals("2") ? vm.job2.FromDateTime.Date + new TimeSpan(12, 0, 0) :
                    vm.rtndfDateLunchTimeValue.Equals("3") ? vm.job2.FromDateTime.Date + new TimeSpan(12, 0, 0) :
                    vm.rtndfDateLunchTimeValue.Equals("4") ? vm.job2.FromDateTime.Date + new TimeSpan(11, 30, 0) :
                    DateTime.MinValue;

            vm.job2.DLLunch_To = vm.rtndfDateLunchTimeValue.Equals("1") ? vm.job2.FromDateTime.Date + new TimeSpan(14, 0, 0) :
                    vm.rtndfDateLunchTimeValue.Equals("2") ? vm.job2.FromDateTime.Date + new TimeSpan(14, 0, 0) :
                    vm.rtndfDateLunchTimeValue.Equals("3") ? vm.job2.FromDateTime.Date + new TimeSpan(13, 0, 0) :
                    vm.rtndfDateLunchTimeValue.Equals("4") ? vm.job2.FromDateTime.Date + new TimeSpan(12, 30, 0) :
                    DateTime.MinValue;

            vm.job2.ExtFromDateTime = (vm.rtndbExtfromDateTime != DateTime.MinValue) ? vm.rtndbExtfromDateTime : DateTime.MinValue;
            vm.job2.ExtDateTime = (vm.rtndbExtbyDateTime != DateTime.MinValue) ? vm.rtndbExtbyDateTime : DateTime.MinValue;

            string exptype = vm.expStr2;
            string rtndfbDateTimeFinalDispText = "Select";
            if (exptype.Equals(common.eExpressType.etNormal.ToString()))
            {
                rtndfbDateTimeFinalDispText = delfromdatetime.ToString("dd/MM/yyyy h:mm tt", CultureInfo.InvariantCulture).ToUpper() + 
                    "\nto\n" +
                    delbydatetime.ToString("dd/MM/yyyy h:mm tt", CultureInfo.InvariantCulture).ToUpper() + 
                    "\nelse latest by\n" + vm.rtndbExtByDateTimeValue;
                vm.rtndfbDateTimeFinalDispText = rtndfbDateTimeFinalDispText;
                
                /*DateTime extFromDT = DateTime.ParseExact(vm.dbExtByDateTimeValue, dateformats, CultureInfo.InvariantCulture, DateTimeStyles.None);
                DateTime extToDT = DateTime.ParseExact(vm.dbExtByDateTimeValue, dateformats, CultureInfo.InvariantCulture, DateTimeStyles.None);
                vm.job2.ExtFromDateTime = extFromDT + (new TimeSpan(9,0,0));
                vm.job2.ExtDateTime = extToDT;*/
                vm.job2.ExpressType = common.getEExpType(exptype);
            }
            else
            {
                rtndfbDateTimeFinalDispText = delfromdatetime.ToString("dd/MM/yyyy h:mm tt", CultureInfo.InvariantCulture).ToUpper() + 
                    "\nto\n" +
                    delbydatetime.ToString("dd/MM/yyyy h:mm tt", CultureInfo.InvariantCulture).ToUpper();
                vm.rtndfbDateTimeFinalDispText = rtndfbDateTimeFinalDispText;

                /*vm.job2.ExtFromDateTime = DateTime.MinValue;
                vm.job2.ExtDateTime = DateTime.MinValue;*/
                vm.job2.ExpressType = common.getEExpType(exptype);
            }
            lblRtnTime.Text = rtndfbDateTimeFinalDispText;
            lblExpType2.Text = getExpTypeStr();
        }
        catch (Exception e)
        {
            string s = e.Message;
        }
    }

    private void preSelectRtnDate()
    {
        try
        {
            string selectedvalue = "";
            string selectedExtvalue = "";
            int itemCount = 0;

            itemCount = cvRFDate.ItemsSource is List<date> collection ? collection.Count : 0;
            if (itemCount > 0 && (cvRFDate.ItemsSource is List<date> items))
            {
                if (vm != null && vm.job2 != null && vm.job2.FromDateTime != DateTime.MinValue)
                {
                    selectedvalue = vm.job2.FromDateTime.ToString("dd/MM/yyyy");
                    vm.rtndfDateSelectedValue = selectedvalue;
                    vm.rtndfDateTimeSelectedValue = vm.job2.FromDateTime.ToString("dd/MM/yyyy HH:mm");
                }
                else
                {
                    selectedvalue = vm.rtndfDateTimeSelectedValue;
                }

                //selectedvalue = vm.rtndfDateSelectedValue;
                if (!String.IsNullOrEmpty(selectedvalue))
                {
                    date item;
                    bool selected = false;
                    DateTime dt;
                    for (int i = 0; i <= items.Count - 1; i++)
                    {
                        item = items[i];
                        dt = !string.IsNullOrEmpty(item.formattedDate) ?
                            DateTime.ParseExact(item.formattedDate, dateformats, CultureInfo.InvariantCulture, DateTimeStyles.None) : DateTime.MinValue;

                        if (dt.ToString("dd/MM/yyyy").Equals(selectedvalue))
                        {
                            cvRFDate.SelectedItem = items[i];
                            cvRFDate.ScrollTo(item, position: ScrollToPosition.MakeVisible, animate: false);
                            selected = true;
                            break;
                        }

                        /*if (item.formattedDate.Equals(selectedvalue))
                        {
                            cvRFDate.SelectedItem = items[i];
                            cvRFDate.ScrollTo(item, position: ScrollToPosition.MakeVisible, animate: false);
                            selected = true;
                            break;
                        }*/
                    }
                    if (!selected)
                        cvRFDate.SelectedItem = items[0];
                }
                else
                {
                    cvRFDate.SelectedItem = items[0];
                }
            }


            itemCount = cvRFLunch.ItemsSource is List<lunchtime> collectionlt ? collectionlt.Count : 0;
            if (itemCount > 0 && (cvRFLunch.ItemsSource is List<lunchtime> itemslt))
            {
                selectedvalue = vm.rtndfDateLunchTimeValue;
                if (!String.IsNullOrEmpty(selectedvalue))
                {
                    lunchtime itemlt;
                    bool selected = false;
                    for (int i = 0; i <= itemslt.Count - 1; i++)
                    {
                        itemlt = itemslt[i];
                        if (itemlt.value.Equals(selectedvalue))
                        {
                            cvRFLunch.SelectedItem = itemslt[i];
                            cvRFLunch.ScrollTo(itemlt, position: ScrollToPosition.MakeVisible, animate: false);
                            selected = true;
                            break;
                        }
                    }
                    if (!selected)
                        cvRFLunch.SelectedItem = itemslt[0];
                }
                else
                {
                    cvRFLunch.SelectedItem = itemslt[0];
                }
            }


            itemCount = cvRFTime.ItemsSource is List<time> collectiont ? collectiont.Count : 0;
            if (itemCount > 0 && (cvRFTime.ItemsSource is List<time> itemst))
            {
                if (vm != null && vm.job2 != null && vm.job2.FromDateTime != DateTime.MinValue)
                {
                    selectedvalue = vm.job2.FromDateTime.ToString("HH:mm");
                    vm.rtndfTimeSelectedValue = selectedvalue;
                }
                else
                    selectedvalue = vm.rtndfTimeSelectedValue;

                //selectedvalue = vm.rtndfTimeSelectedValue;
                if (!String.IsNullOrEmpty(selectedvalue))
                {
                    time itemt;
                    bool selected = false;
                    for (int i = 0; i <= itemst.Count - 1; i++)
                    {
                        itemt = itemst[i];
                        if (itemt.value.Equals(selectedvalue))
                        {
                            cvRFTime.SelectedItem = itemst[i];
                            cvRFTime.ScrollTo(itemt, position: ScrollToPosition.MakeVisible, animate: false);
                            selected = true;
                            break;
                        }
                    }
                    if (!selected)
                        cvRFTime.SelectedItem = itemst[0];
                }
                else
                {
                    cvRFTime.SelectedItem = itemst[0];
                }
            }


            itemCount = cvRBTime.ItemsSource is List<delByDateTime> collectionbt ? collectionbt.Count : 0;
            if (itemCount > 0 && (cvRBTime.ItemsSource is List<delByDateTime> itemsbt))
            {
                if (vm != null && vm.job2 != null && vm.job2.ToDateTime != DateTime.MinValue)
                {
                    selectedvalue = vm.job2.ToDateTime.ToString("HH:mm");
                    vm.rtndbTimeSelectedValue = selectedvalue;
                }
                else
                    selectedvalue = vm.rtndbTimeSelectedValue;

                //selectedvalue = vm.rtndbTimeSelectedValue;
                if (!String.IsNullOrEmpty(selectedvalue))
                {
                    delByDateTime itembt;
                    bool selected = false;
                    for (int i = 0; i <= itemsbt.Count - 1; i++)
                    {
                        itembt = itemsbt[i];
                        if (itembt.value.Equals(selectedvalue) && selectedvalue.Equals("17:30") && vm.job2.ExpressType == eExpressType.etNormal &&
                            itembt.valueselected.Equals("Next Working Day\nby 12 PM"))
                        {
                            cvRBTime.SelectedItem = itemsbt[i];
                            cvRBTime.ScrollTo(itembt, position: ScrollToPosition.MakeVisible, animate: false);
                            selected = true;
                            break;
                        }
                        else if (itembt.value.Equals(selectedvalue) && selectedvalue.Equals("17:30") && vm.job2.ExpressType == eExpressType.etGuaranteed)
                        {
                            cvRBTime.SelectedItem = itemsbt[i];
                            cvRBTime.ScrollTo(itembt, position: ScrollToPosition.MakeVisible, animate: false);
                            selected = true;
                            break;
                        }
                        else if (itembt.value.Equals(selectedvalue) && !selectedvalue.Equals("17:30"))
                        {
                            cvRBTime.SelectedItem = itemsbt[i];
                            cvRBTime.ScrollTo(itembt, position: ScrollToPosition.MakeVisible, animate: false);
                            selected = true;
                            break;
                        }

                        /*if (itembt.valueselected.Equals(selectedvalue))
                        {
                            cvRBTime.SelectedItem = itemsbt[i];
                            cvRBTime.ScrollTo(itembt, position: ScrollToPosition.MakeVisible, animate: false);
                            selected = true;
                            break;
                        }*/
                    }
                    if (!selected)
                        cvRBTime.SelectedItem = itemsbt[0];
                }
                else
                {
                    cvRBTime.SelectedItem = itemsbt[0];
                }
            }

        }
        catch (Exception e)
        {
            string s = e.Message;
        }
    }

    #endregion

    //==================================================================================================================================

    #region InvoiceBS

    async void show_BSInvP1_BS(System.Object sender, System.EventArgs e)
    {
        try
        {
            if (vm == null)
                vm = (CreateJobVM)BindingContext;

            cvInvP1.ItemsSource = vm.invoicesP1;
            vm.BSinvP2opened = false;

            await svBSInv.ScrollToAsync(0, 0, false);

            BSInv.IsVisible = true;
            removeTapGestureRecognizer();
            addInvBSHeaderLabelGestureRecog();

            await BSInv.OpenBottomSheet(false);
            BSInv.isShowing = true;
        }
        catch (Exception ex)
        {
            string s = ex.Message;
        }
    }

    async void show_BSInvP2_BS(System.Object sender, System.EventArgs e)
    {
        try
        {
            if (vm == null)
                vm = (CreateJobVM)BindingContext;

            cvInvP1.ItemsSource = vm.invoicesP2;
            vm.BSinvP2opened = true;

            await svBSInv.ScrollToAsync(0, 0, false);

            BSInv.IsVisible = true;
            removeTapGestureRecognizer();
            addInvBSHeaderLabelGestureRecog();

            await BSInv.OpenBottomSheet(false);
            BSInv.isShowing = true;
        }
        catch (Exception ex)
        {
            string s = ex.Message;
        }
    }

    void dbtnAddInvP1_Clicked(object sender, EventArgs e)
    {
        string dispText = "";
        string amtText = "";
        decimal amt = 0;
        try
        {
            dispText = txtDOP1.Text;
            amtText = txtAmtP1.Text;
            if (String.IsNullOrEmpty(dispText))
            {
                showmsg("", "Please enter a D/O Invoice number.");
            }
            else if (!hasDupInv(dispText))
            {
                try
                {
                    amt = String.IsNullOrEmpty(amtText) ? 0 : Decimal.Parse(amtText);
                }
                catch (Exception exx)
                {
                    string ss = exx.Message;
                }

                addInvoice(dispText, amt);
            }
            else
            {
                showmsg("Duplicate!", "Duplicate D/O or Invoice number.");
            }
        }
        catch (Exception ex)
        {
            string s = ex.Message;
        }
    }

    bool hasDupInv(string invno)
    {
        bool asd = false;
        try
        {
            if (vm == null)
                vm = (CreateJobVM)BindingContext;

            ObservableCollection<invoice> invs = !vm.BSinvP2opened ? vm.getInvoicesP1() : vm.getInvoicesP2();
            if (invs != null && invs.Count > 0)
            {
                foreach (invoice inv in invs)
                {
                    if (inv.DispText.ToLower().Equals(invno.ToLower()))
                    {
                        asd = true;
                        break;
                    }
                }
            }
        }
        catch (Exception ex)
        {
            string s = ex.Message;
        }
        return asd;
    }

    void addInvoice(string desp, decimal amt)
    {
        try
        {
            invoice inv = new invoice(0, desp, amt, amt.ToString("F2"));
            if (vm == null)
                vm = (CreateJobVM)BindingContext;

            ObservableCollection<invoice> invs = !vm.BSinvP2opened ? vm.getInvoicesP1() : vm.getInvoicesP2();

            if (invs == null && !vm.BSinvP2opened)
                vm.newInvoicesP1();
            if (invs == null && vm.BSinvP2opened)
                vm.newInvoicesP2();

            if (!vm.BSinvP2opened)
                vm.addInvP1(inv);
            if (vm.BSinvP2opened)
                vm.addInvP2(inv);

            txtAmtP1.Text = "0.00";
            txtDOP1.Text = "";
            ////txtp1DO.Focus(); //should not focus, will auto bring up softkeyboard
        }
        catch (Exception ex)
        {
            string s = ex.Message;
        }
    }

    private async void cvInvP1OnItemSelected(object sender, SelectionChangedEventArgs e)
    {
        try
        {
            var selectedItem = e.CurrentSelection.FirstOrDefault() as invoice;
            if (selectedItem != null)
            {
                if (await DisplayAlertAsync("Are you sure?", "Confirm to remove the selecte D/O Invoice?", "Yes", "No"))
                {
                    if (vm == null)
                        vm = (CreateJobVM)BindingContext;
                    vm.removeInvP1(selectedItem);
                }
            }
        }
        catch (Exception ex)
        {
            string s = ex.Message;
        }
    }

    ///======= use cvInvP1OnItemTap instead of cvInvP1OnItemSelected for multiple tap
    /// ======= SelectionChanged by default only respond to a single tap, marking an item as selected
    private async void cvInvP1OnItemTap(object sender, EventArgs e)
    {
        try
        {
            var tappedGrid = sender as Grid;
            if (tappedGrid != null)
            {
                var selectedItem = tappedGrid.BindingContext as invoice;

                if (selectedItem != null && selectedItem is invoice)
                {
                    if (await DisplayAlertAsync("Are you sure?", "Confirm to remove the selecte D/O Invoice?" + "\n\n" + ((invoice)selectedItem).DispText, "Yes", "No"))
                    {
                        if (vm == null)
                            vm = (CreateJobVM)BindingContext;
                        if (!vm.BSinvP2opened)
                            vm.removeInvP1((invoice)selectedItem);
                        if (vm.BSinvP2opened)
                            vm.removeInvP2((invoice)selectedItem);
                    }
                }

            }
        }
        catch (Exception ex)
        {
            string s = ex.Message;
        }
    }

    async void CloseBSInv_Clicked(object sender, EventArgs e)
    {
        try
        {
            addTapGestureRecognizer();
            removeInvBSHeaderLabelGestureRecog();
            
            txtAmtP1.Text = "0.00";
            txtDOP1.Text = "";
            //await BSInv.CloseBottomSheet();
            //BSInv.isShowing = false;
            await CloseBottomSheetSafely(BSInv);
        }
        catch (Exception ex)
        {
            string s = ex.Message;
        }
    }

    async void CloseBSInvSave_Clicked(System.Object sender, System.EventArgs e)
    {
        try
        {
            addTapGestureRecognizer();
            removeInvBSHeaderLabelGestureRecog();
            if (vm == null) vm = (CreateJobVM)BindingContext;

            InvoiceSaveUponClose();
            ObservableCollection<invoice> invs = !vm.BSinvP2opened ? vm.getInvoicesP1() : vm.getInvoicesP2();

            if (!vm.BSinvP2opened)
                lblColInv.Text = "Add/Remove" + ((invs != null && invs.Count > 0) ? " (" + invs.Count.ToString() + ")" : "");

            if (vm.BSinvP2opened)
                lblRtnInv.Text = "Add/Remove" + ((invs != null && invs.Count > 0) ? " (" + invs.Count.ToString() + ")" : "");

            txtAmtP1.Text = "0.00";
            txtDOP1.Text = "";
            //await BSInv.CloseBottomSheet();
            //BSInv.isShowing = false;
            await CloseBottomSheetSafely(BSInv);
            validateButton();
        }
        catch (Exception ex)
        {
            string s = ex.Message;
        }
    }
    
    void InvoiceSaveUponClose()
    {
        DOObject _do = null;
        DOObject[] doList = null;

        try
        {
            ObservableCollection<invoice> invs = !vm.BSinvP2opened ? vm.getInvoicesP1() : vm.getInvoicesP2();

            if (invs != null && invs.Count > 0)
            {
                doList = new DOObject[invs.Count];
                for (int i = 0; i <= invs.Count -1; i++)
                {
                    _do = new DOObject();
                    _do.Amount = invs[i].Amount;
                    _do.Invoice = invs[i].DispText;
                    _do.DOIDX = invs[i].IDX;
                    doList[i] = _do;
                }
            }

            if (!vm.BSinvP2opened)
                vm.job1.DOList = doList;
            if (vm.BSinvP2opened)
                vm.job2.DOList = doList;
        } catch (Exception e)
        {
            string s = e.Message;
        }
    }

    private void InvBSOnBackgroundClicked(object sender, TappedEventArgs e)
    {
        // Manually unfocus the Entry
        txtAmtP1.Unfocus();
    }

    // Named handler for the inv BS header label
    void OnInvBSHeaderTapped(object s, TappedEventArgs e) => InvBSOnBackgroundClicked(null, null);

    void addInvBSHeaderLabelGestureRecog()
    {
        //TapGestureRecognizer headerLabelTGR = new TapGestureRecognizer();
        //headerLabelTGR.Tapped += (s, e) =>
        //{
        //    //ontap2();
        //    InvBSOnBackgroundClicked(null, null);
        //};
        //BSInv.headerLabel.GestureRecognizers.Add(headerLabelTGR);

        // Clear first to prevent stacking
        BSInv.headerLabel.GestureRecognizers.Clear();
        var tgr = new TapGestureRecognizer();
        tgr.Tapped += OnInvBSHeaderTapped;
        BSInv.headerLabel.GestureRecognizers.Add(tgr);
    }
    
    void removeInvBSHeaderLabelGestureRecog()
    {
        BSInv.headerLabel.GestureRecognizers.Clear();
    }

    #endregion

    //==================================================================================================================================

    #region Gestures

    // Named handlers - no lambda capture, safe to add/remove
    void OnColAddressTapped(object s, TappedEventArgs e) => show_Address_BS(1);
    void OnDelAddressTapped(object s, TappedEventArgs e) => show_Address_BS(2);
    void OnRtnAddressTapped(object s, TappedEventArgs e) => show_Address_BS(3);
    void OnContentTapped(object s, TappedEventArgs e) => show_ContentType_BS(null, null);
    void OnContent2Tapped(object s, TappedEventArgs e) => show_ContentType_BS2(null, null);
    void OnInvP1Tapped(object s, TappedEventArgs e) => show_BSInvP1_BS(null, null);
    void OnInvP2Tapped(object s, TappedEventArgs e) => show_BSInvP2_BS(null, null);
    void OnColTimeTapped(object s, TappedEventArgs e) => show_CollectionTime_BS(null, null);
    void OnDelTimeTapped(object s, TappedEventArgs e) => show_DelTime_BS(null, null);
    void OnRtnTimeTapped(object s, TappedEventArgs e) => show_RtnTime_BS(null, null);

    void addTapGestureRecognizer()
    {
        // Always clear first (defensive, even if removeTap was called)
        removeTapGestureRecognizer();

        try
        {

            var gridColAddressTGR = new TapGestureRecognizer();
            gridColAddressTGR.Tapped += OnColAddressTapped;
            gridColAddress.GestureRecognizers.Add(gridColAddressTGR);

            var gridDelAddressTGR = new TapGestureRecognizer();
            gridDelAddressTGR.Tapped += OnDelAddressTapped;
            gridDelAddress.GestureRecognizers.Add(gridDelAddressTGR);

            var gridRtnAddressTGR = new TapGestureRecognizer();
            gridRtnAddressTGR.Tapped += OnRtnAddressTapped;
            gridRtnAddress.GestureRecognizers.Add(gridRtnAddressTGR);

            var gridContentTGR = new TapGestureRecognizer();
            gridContentTGR.Tapped += OnContentTapped;
            gridContent.GestureRecognizers.Add(gridContentTGR);

            var gridContent2TGR = new TapGestureRecognizer();
            gridContent2TGR.Tapped += OnContent2Tapped;
            gridRtnContent.GestureRecognizers.Add(gridContent2TGR);

            var gridInvP1TGR = new TapGestureRecognizer();
            gridInvP1TGR.Tapped += OnInvP1Tapped;
            gridColInv.GestureRecognizers.Add(gridInvP1TGR);

            var gridInvP2TGR = new TapGestureRecognizer();
            gridInvP2TGR.Tapped += OnInvP2Tapped;
            gridRtnInv.GestureRecognizers.Add(gridInvP2TGR);

            var gridColTimeTGR = new TapGestureRecognizer();
            gridColTimeTGR.Tapped += OnColTimeTapped;
            gridColTime.GestureRecognizers.Add(gridColTimeTGR);

            var gridDelTimeTGR = new TapGestureRecognizer();
            gridDelTimeTGR.Tapped += OnDelTimeTapped;
            gridDelTime.GestureRecognizers.Add(gridDelTimeTGR);

            var gridRtnTimeTGR = new TapGestureRecognizer();
            gridRtnTimeTGR.Tapped += OnRtnTimeTapped;
            gridRtnTime.GestureRecognizers.Add(gridRtnTimeTGR);

            btnTitle.IsEnabled = true;
            svMain.IsEnabled = true;
            swOneTwoWay.IsEnabled = true;
        } catch (Exception e)
        {
            string s = e.Message;
        }

        //TapGestureRecognizer gridColAddressTGR = new TapGestureRecognizer();
        //TapGestureRecognizer gridDelAddressTGR = new TapGestureRecognizer();
        //TapGestureRecognizer gridRtnAddressTGR = new TapGestureRecognizer();

        //TapGestureRecognizer gridContentTGR = new TapGestureRecognizer();
        //TapGestureRecognizer gridContent2TGR = new TapGestureRecognizer();

        //TapGestureRecognizer gridInvP1TGR = new TapGestureRecognizer();
        //TapGestureRecognizer gridInvP2TGR = new TapGestureRecognizer();

        //TapGestureRecognizer gridColTimeTGR = new TapGestureRecognizer();
        //TapGestureRecognizer gridDelTimeTGR = new TapGestureRecognizer();
        //TapGestureRecognizer gridRtnTimeTGR = new TapGestureRecognizer();

        //try
        //{
        //    btnTitle.IsEnabled = true;
        //    svMain.IsEnabled = true;
        //    swOneTwoWay.IsEnabled = true;

        //    gridColAddressTGR.Tapped += (s, e) =>
        //    {
        //        show_Address_BS(1);
        //    };
        //    gridColAddress.GestureRecognizers.Add(gridColAddressTGR);

        //    gridDelAddressTGR.Tapped += (s, e) =>
        //    {
        //        show_Address_BS(2);
        //    };
        //    gridDelAddress.GestureRecognizers.Add(gridDelAddressTGR);

        //    gridRtnAddressTGR.Tapped += (s, e) =>
        //    {
        //        show_Address_BS(3);
        //    };
        //    gridRtnAddress.GestureRecognizers.Add(gridRtnAddressTGR);

        //    gridContentTGR.Tapped += (s, e) =>
        //    {
        //        show_ContentType_BS(null, null);
        //    };
        //    gridContent.GestureRecognizers.Add(gridContentTGR);

        //    gridContent2TGR.Tapped += (s, e) =>
        //    {
        //        show_ContentType_BS2(null, null);
        //    };
        //    gridRtnContent.GestureRecognizers.Add(gridContent2TGR);

        //    gridInvP1TGR.Tapped += (s, e) =>
        //    {
        //        show_BSInvP1_BS(null, null);
        //    };
        //    gridColInv.GestureRecognizers.Add(gridInvP1TGR);

        //    gridInvP2TGR.Tapped += (s, e) =>
        //    {
        //        show_BSInvP2_BS(null, null);
        //    };
        //    gridRtnInv.GestureRecognizers.Add(gridInvP2TGR);

        //    gridColTimeTGR.Tapped += (s, e) =>
        //    {
        //        show_CollectionTime_BS(null, null);
        //    };
        //    gridColTime.GestureRecognizers.Add(gridColTimeTGR);

        //    gridDelTimeTGR.Tapped += (s, e) =>
        //    {
        //        show_DelTime_BS(null, null);
        //    };
        //    gridDelTime.GestureRecognizers.Add(gridDelTimeTGR);

        //    gridRtnTimeTGR.Tapped += (s, e) =>
        //    {
        //        show_RtnTime_BS(null, null);
        //    };
        //    gridRtnTime.GestureRecognizers.Add(gridRtnTimeTGR);

        //}
        //catch (Exception ex)
        //{
        //    string s = ex.Message;
        //}
    }

    void removeTapGestureRecognizer()
    {
        try
        {
            btnTitle.IsEnabled = false;
            svMain.IsEnabled = false;
            swOneTwoWay.IsEnabled = false;
            gridColAddress.GestureRecognizers.Clear();
            gridDelAddress.GestureRecognizers.Clear();
            gridRtnAddress.GestureRecognizers.Clear();

            gridContent.GestureRecognizers.Clear();
            gridRtnContent.GestureRecognizers.Clear();

            gridColInv.GestureRecognizers.Clear();
            gridRtnInv.GestureRecognizers.Clear();

            gridColTime.GestureRecognizers.Clear();
            gridDelTime.GestureRecognizers.Clear();
            gridRtnTime.GestureRecognizers.Clear();
        }
        catch (Exception ex)
        {
            string s = ex.Message;
        }
    }

    #endregion

    //==================================================================================================================================




    //==================================================================================================================================


    //another method of opening BS
    public void show_Content_BS(object sender, EventArgs e)
    {
        try
        {
            //BScontent = new CJob_Content_BS();
            //BScontent.HasBackdrop = true;
            //BScontent.CornerRadius = 20;
            //BScontent.IsCancelable = false;
            //BScontent.Dismissed += (s, e) =>
            //{
            //    ////DisplayAlert("Sheet was dismissed", e == DismissOrigin.Gesture ? "Sheet was dismissed by a user gesture" : "Sheet was dismissed programmatically", "close");
            //    BScontent = null;
            //};
            //BScontent.ShowAsync();
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