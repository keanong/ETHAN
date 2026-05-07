using CommunityToolkit.Maui.Extensions;
using CommunityToolkit.Maui.Views;
using ETHAN.classes;
using ETHAN.Network;
using ETHAN.ProgressDialog;
using ETHAN.ViewModel;
using ETHAN.XDelSys;
using Microsoft.Maui.Controls;
using System.Collections.ObjectModel;
using System.Data;
using XDelServiceRef;

namespace ETHAN.Views;

[QueryProperty(nameof(Vmm), "vmm")] // Add a QueryProperty to handle the navigation parameter
//[QueryProperty(nameof(LOGININFO), "LOGININFO")] // Add a QueryProperty to handle the navigation parameter
[QueryProperty(nameof(LTR), "LTR")] // Add a QueryProperty to handle the navigation parameter



public partial class ManageJobPage : ContentPage
{
    private XOEWSSoapClient xs = new XOEWSSoapClient(XOEWSSoapClient.EndpointConfiguration.XOEWSSoap);
    private readonly IProgressDialogService _progressService;
    private bool _firstTabSelected = false;

    private ManageJobPageVM? vm;
    public ManageJobPageVM Vmm
    {
        set
        {
            /*if (vm != null)
            {
                vm.PropertyChanged -= Vm_PropertyChanged;
                vm.Tabs.CollectionChanged -= Tabs_CollectionChanged;
                TabHeaderLayout.SizeChanged -= TabHeaderLayout_SizeChanged;
            }

            if (!txtPostal.IsReadOnly && swNewAddress.IsToggled)
                txtPostalTextChangedSubscribed();
            else
                txtPostalTextChangedUnSubscribed();

            if (vm == null || (vm != null && (vm.aDraft == null && vm.aInProgress == null && vm.aAttempted == null && vm.aCompleted == null)))
                vm = value ?? new ManageJobPageVM();
            BindingContext = vm;

            vm.PropertyChanged += Vm_PropertyChanged;
            vm.Tabs.CollectionChanged += Tabs_CollectionChanged;
            TabHeaderLayout.SizeChanged += TabHeaderLayout_SizeChanged;*/

            if (value != null)
            {
                // Unsubscribe old vm before replacing
                if (vm != null)
                {
                    vm.PropertyChanged -= Vm_PropertyChanged;
                    vm.Tabs.CollectionChanged -= Tabs_CollectionChanged;
                }
                TabHeaderLayout.SizeChanged -= TabHeaderLayout_SizeChanged;

                vm = value;
                BindingContext = vm;

                vm.PropertyChanged += Vm_PropertyChanged;
                vm.Tabs.CollectionChanged += Tabs_CollectionChanged;
                TabHeaderLayout.SizeChanged += TabHeaderLayout_SizeChanged;
            }
            else if (vm == null)
            {
                vm = new ManageJobPageVM();
                BindingContext = vm;

                vm.PropertyChanged += Vm_PropertyChanged;
                vm.Tabs.CollectionChanged += Tabs_CollectionChanged;
                TabHeaderLayout.SizeChanged += TabHeaderLayout_SizeChanged;
            }
            // value=null AND vm exists = returning via "..", keep existing vm

        }
    }

    private ManageJobPageVM.LoadTabsRequest? ltr;

    public ManageJobPageVM.LoadTabsRequest? LTR
    {
        set
        {
            /*ltr = value;*/
            if (value != null) ltr = value; 
        }
    }

    private LoginInfo? logininfo;
    /*public LoginInfo? LOGININFO
    {
        set
        {
            logininfo = value;
        }
    }*/

    private static string Completed = ",63,96,115,135,373,";
    private static string Reversed = ",62,64,65,112,262,329,334,";
    private static string InProgress = ",0,49,101,103,104,154,155,165,232,233,235,261,321,322,325,340,342,343,344,359,";
    private static string Attempted = ",67,68,77,78,80,108,109,116,117,119,126,127,128,134,136,255,257,372,391,392";

    /*public ManageJobPage()
    {
        InitializeComponent();
        BindingContext = vm;
        Shell.SetTabBarIsVisible(this, false);

    }*/
    public ManageJobPage(IProgressDialogService progressService)
    {
        InitializeComponent();
        _progressService = progressService;
        BindingContext = vm;
        Shell.SetTabBarIsVisible(this, false);

    }

    private bool _bottomSheetsInitialized = false;

    protected override void OnSizeAllocated(double width, double height)
    {
        base.OnSizeAllocated(width, height);

        if (DeviceInfo.Idiom == DeviceIdiom.Tablet)
        {
            //AppContainer.WidthRequest = Math.Min(width * 0.32, 400); // 40% of screen width, max 800
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

    protected override void OnAppearing()
    {
        base.OnAppearing();
        try
        {
            /*BSOptions.IsVisible = false;
            BSSS.IsVisible = false;
            BSINST.IsVisible = false;
            BSSummary.IsVisible = false;
            BSResch.IsVisible = false;
            BSRedir.IsVisible = false;

            if (ltr != null)
                reloadData();

            if (!txtPostal.IsReadOnly && swNewAddress.IsToggled)
                txtPostalTextChangedSubscribed();
            else
                txtPostalTextChangedUnSubscribed();*/

            // Refresh BindingContext when returning via ".."

            if (vm != null)
                BindingContext = vm;

            // Re-subscribe events that were unsubscribed in OnDisappearing
            // Safe to call multiple times since OnDisappearing always unsubscribes first
            if (vm != null)
            {
                vm.PropertyChanged -= Vm_PropertyChanged;           // defensive unsub first
                vm.Tabs.CollectionChanged -= Tabs_CollectionChanged; // defensive unsub first
                vm.PropertyChanged += Vm_PropertyChanged;
                vm.Tabs.CollectionChanged += Tabs_CollectionChanged;
            }
            TabHeaderLayout.SizeChanged -= TabHeaderLayout_SizeChanged; // defensive unsub first
            TabHeaderLayout.SizeChanged += TabHeaderLayout_SizeChanged;

            BSOptions.IsVisible = false;
            BSSS.IsVisible = false;
            BSINST.IsVisible = false;
            BSSummary.IsVisible = false;
            BSResch.IsVisible = false;
            BSRedir.IsVisible = false;

            if (ltr != null)
                reloadData();

            // Move postal subscription here - safe to access UI in OnAppearing
            txtPostalTextChangedUnSubscribed();
            if (!txtPostal.IsReadOnly && swNewAddress.IsToggled)
                txtPostalTextChangedSubscribed();

        }
        catch (Exception ex)
        {
            string s = ex.Message;
        }
    }

    protected override void OnDisappearing()
    {
        base.OnDisappearing();

        /*if (vm != null)
        {
            vm.PropertyChanged -= Vm_PropertyChanged;
            vm.Tabs.CollectionChanged -= Tabs_CollectionChanged;
        }
        TabHeaderLayout.SizeChanged -= TabHeaderLayout_SizeChanged;*/

        if (vm != null)
        {
            vm.PropertyChanged -= Vm_PropertyChanged;
            vm.Tabs.CollectionChanged -= Tabs_CollectionChanged;
        }
        TabHeaderLayout.SizeChanged -= TabHeaderLayout_SizeChanged;

        // Unsubscribe postal text changed
        txtPostalTextChangedUnSubscribed();

        // Unsubscribe switch toggles
        swNewAddress.Toggled -= swNewAddressonToggle;
        swNewContact.Toggled -= swNewContactonToggle;        
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

    async void BackToHomePage()
    {
        try
        {
            bool BSOptionsnotopen = ((BSOptions == null) || (BSOptions != null && !BSOptions.isShowing));
            bool BSSSnotopen = ((BSSS == null) || (BSSS != null && !BSSS.isShowing));
            bool BSINSTnotopen = ((BSINST == null) || (BSINST != null && !BSINST.isShowing));
            bool BSSummarynotopen = ((BSSummary == null) || (BSSummary != null && !BSSummary.isShowing));
            bool BSReschnotopen = ((BSResch == null) || (BSResch != null && !BSResch.isShowing));
            bool BSRedirnotopen = ((BSRedir == null) || (BSRedir != null && !BSRedir.isShowing));

            if (BSOptionsnotopen && BSSSnotopen && BSINSTnotopen && BSSummarynotopen && BSReschnotopen && BSRedirnotopen)
            {
                /*MainThread.BeginInvokeOnMainThread(async () =>
                {
                    BindingContext = null;
                    string v = string.Empty;
                    await Shell.Current.GoToAsync("///CardShellPage", new Dictionary<string, object>
                    {
                        { "LOGININFO", null },
                        { "BARCODE", null }
                    });
                }
                );*/

                BindingContext = null;
                await Shell.Current.GoToAsync("///CardShellPage", new Dictionary<string, object>
            {
                { "LOGININFO", null },
                { "BARCODE", null }
            });
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
            if (_progressService != null && _progressService.IsShowing)
                return true;

            BackToHomePage();
        } catch (Exception e)
        {
            string s = e.Message;
        }
        return true;
    }

    private void SelectFirstTab()
    {
        if (!_firstTabSelected && vm?.Tabs.Count > 0)
        {
            _firstTabSelected = true;
            Dispatcher.Dispatch(async () =>
            {
                await Task.Delay(50); // let layout settle
                vm.SelectedTabIndex = 0;
                TabCarousel.Position = 0;
                ChangeCarouselBGColor(0);
                await MoveIndicator(0);
                ScrollTabIntoView(0);
            });
        }
    }

    private void ChangeCarouselBGColor(int index)
    {
        string mode = AppSession.LoginMode;
        if (mode.Equals("r"))
        {
            TabCarousel.BackgroundColor = GetAppColor(
                index == 0 ? "Yellow100Accent" :
                index == 1 ? "Red" : "Green");
        } else
        {
            TabCarousel.BackgroundColor = GetAppColor(
                index == 0 ? "Blue300Accent" :
                index == 1 ? "Yellow100Accent" :
                index == 2 ? "Red" : "Green");
        }
    }

    public static Color GetAppColor(string key) =>
    (Color)Application.Current!.Resources[key];

    private void TabHeaderLayout_SizeChanged(object? sender, EventArgs e)
        => SelectFirstTab();

    private void Tabs_CollectionChanged(object? sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        => SelectFirstTab();

    private void Vm_PropertyChanged(object? sender, System.ComponentModel.PropertyChangedEventArgs e)
    {
        if (e.PropertyName == nameof(vm.SelectedTabIndex) && vm != null)
        {
            Dispatcher.Dispatch(async () =>
            {
                ChangeCarouselBGColor(vm.SelectedTabIndex);
                await MoveIndicator(vm.SelectedTabIndex);
                ScrollTabIntoView(vm.SelectedTabIndex);
            });
        }
    }

    // Handle tab clicks (Label tap)
    private void OnTabClicked(object sender, TappedEventArgs e)
    {
        if (sender is Label lbl && vm?.Tabs != null)
        {
            var tab = lbl.BindingContext as TabItem;
            if (tab != null)
            {
                vm.SelectedTabIndex = vm.Tabs.IndexOf(tab);
                TabCarousel.Position = vm.SelectedTabIndex;
                ChangeCarouselBGColor(vm.SelectedTabIndex);
            }
        }
    }

    // Sync swipe to tab
    private void TabCarousel_PositionChanged(object sender, PositionChangedEventArgs e)
    {
        if (vm == null) return;
        vm.SelectedTabIndex = e.CurrentPosition;
        ChangeCarouselBGColor(vm.SelectedTabIndex);
    }

    // Animate underline to text width (non-obsolete Measure)
    private async Task MoveIndicator(int tabIndex)
    {
        if (TabHeaderLayout.Children.Count == 0 || tabIndex < 0 || tabIndex >= TabHeaderLayout.Children.Count)
            return;

        if (TabHeaderLayout.Children[tabIndex] is Label lbl)
        {
            int tries = 0;
            while (lbl.Width <= 0 && tries < 10)
            {
                await Task.Delay(25);
                tries++;
            }

            var measured = lbl.Measure(double.PositiveInfinity, double.PositiveInfinity);
            double textWidth = measured.Width;
            double targetX = lbl.X + (lbl.Width - textWidth) / 2.0;

            TabIndicator.WidthRequest = textWidth;

            string mode = AppSession.LoginMode;
            if (mode.Equals("r"))
            {
                TabIndicator.Color = GetAppColor(
                tabIndex == 0 ? "Yellow100Accent" :
                tabIndex == 1 ? "Red" : "Green");
            } else
            {
                TabIndicator.Color = GetAppColor(
                tabIndex == 0 ? "Blue300Accent" :
                tabIndex == 1 ? "Yellow100Accent" :
                tabIndex == 2 ? "Red" : "Green");
            }

            await TabIndicator.TranslateTo(targetX, 0, 250, Easing.CubicOut);
        }
    }

    // Scroll the selected tab into view
    private void ScrollTabIntoView(int tabIndex)
    {
        if (TabHeaderLayout.Children.Count == 0 || tabIndex < 0 || tabIndex >= TabHeaderLayout.Children.Count)
            return;

        if (TabHeaderLayout.Children[tabIndex] is Label lbl)
        {
            double scrollX = lbl.X + lbl.Width / 2 - TabScroll.Width / 2;
            if (scrollX < 0) scrollX = 0;
            TabScroll.ScrollToAsync(scrollX, 0, true);
        }
    }

    private void DpFrom_DateSelected(object sender, DateChangedEventArgs e)
    {
        // e.OldDate is the previous date
        // e.NewDate is the newly selected date
        DateTime selectedDate = e.NewDate ?? DateTime.Now;
        lbldpFromSelected.Text = selectedDate.ToString();
    }

    private void DpTo_DateSelected(object sender, DateChangedEventArgs e)
    {
        // e.OldDate is the previous date
        // e.NewDate is the newly selected date
        DateTime selectedDate = e.NewDate ?? DateTime.Now;
        lbldpToSelected.Text = selectedDate.ToString();
    }

    // filter

    async void btnDTSearch_Clicked(System.Object sender, System.EventArgs e)
    {
        try
        {
            if (NetworkHelper.IsDisconnected())
            {
                await DisplayAlertAsync("No Internet Connection", "Please check your internet connection and try again.", "OK");
                return;
            }

            await searchDT();
        }
        catch (Exception ex)
        {
            string s = ex.Message;
        }
    }

    async void btnSearch_Clicked(System.Object sender, System.EventArgs e)
    {
        try
        {
            if (NetworkHelper.IsDisconnected())
            {
                await DisplayAlertAsync("No Internet Connection", "Please check your internet connection and try again.", "OK");
                return;
            }

            await search();
        } catch (Exception ex)
        {
            string s = ex.Message;
        }
    }

    private async Task reloadData()
    {
        ManageJobPageVM.LoadTabsRequest request = null;
        try
        {
            // Update UI bindings on the main thread
            if (BindingContext is ManageJobPageVM Vm)
            {
                _firstTabSelected = false;
                await MainThread.InvokeOnMainThreadAsync(async () =>
                {
                await Vm.LoadTabs2Command.ExecuteAsync(ltr);
                });
            }

        } catch (Exception e)
        {
            string s = e.Message;
        }
    }

    private async Task searchDT()
    {
        XDelServiceRef.JobStructure? jobarray = null;
        XDelServiceRef.JobStructure? unpostedJob = null;
        XDelServiceRef.JobInfo[] jobs = null;
        XDelServiceRef.JobInfo[] upjobs = null;
        XDelServiceRef.JobInfo[] mergedjobs = null;

        try
        {
            DateTime fromdate = dpFrom.Date ?? DateTime.Now;
            DateTime toDate = dpTo.Date ?? DateTime.Now;

            string mode = AppSession.LoginMode;
            logininfo = AppSession.logininfo;

            if (logininfo?.clientInfo == null ||
            string.IsNullOrEmpty(logininfo.clientInfo.Web_UID) ||
            mode.Equals("s") && logininfo.clientInfo.CAIDX <= 0)
            {
                await DisplayAlertAsync("Session expired", "Please Login again.", "OK");
                await common.BackToLogin();
                return;
            }

            await showProgress_Dialog("Processing...");
            //await UiPump.Yield();

            ClientInfo ci = logininfo.clientInfo;

            // Run SOAP query on background thread
            jobarray = await Task.Run(async () =>
            {
                return mode.Equals("s") ? await xs.QueryDateAsync(ci.Web_UID, fromdate, toDate, 1) : await xs.XOE_QueryDateAsync(ci.Web_UID, fromdate, toDate);
            });

            if (mode.Equals("s"))
                unpostedJob = await Task.Run(async () =>
                {
                    return await xs.GetUnpostedJobsAsync(ci.Web_UID) ;
                });

            /*if (jobarray != null && jobarray.JobList != null)
                jobs = alterJobs(jobarray.JobList);

            if (unpostedJob != null && unpostedJob.JobList != null)
                upjobs = alterJobs(unpostedJob.JobList);*/

            if (jobarray != null && jobarray.JobList != null)
                jobs = jobarray.JobList;

            if (unpostedJob != null && unpostedJob.JobList != null)
                upjobs = unpostedJob.JobList;

            mergedjobs = (jobs ?? Array.Empty<XDelServiceRef.JobInfo>())
               .Concat(upjobs ?? Array.Empty<XDelServiceRef.JobInfo>())
               .GroupBy(j => j.JobsIDX)
               .Select(g => g.Last())   // use Last() if upjobs should override jobs
               .ToArray();

            if (mergedjobs == null || mergedjobs.Length == 0)
            {
                await closeProgress_dialog();
                if (BindingContext is ManageJobPageVM vm)
                {
                    await DisplayAlertAsync("", "No job found.", "OK");
                    await vm.ClearTabsCommand.ExecuteAsync(null);
                }
                return;
            }

            // Heavy data processing off the main thread
            ManageJobPageVM.LoadTabsRequest request =
                await Task.Run(() => getData2(ci.Web_UID, mergedjobs));

            await closeProgress_dialog();

            // Update UI bindings on the main thread
            if (BindingContext is ManageJobPageVM Vm)
            {
                _firstTabSelected = false;
                await MainThread.InvokeOnMainThreadAsync(async () =>
                {
                    await Vm.LoadTabs2Command.ExecuteAsync(request);
                });
            }
        }
        catch (Exception e)
        {
            string s = e.Message;
            await closeProgress_dialog();
            await DisplayAlertAsync("Exception", "Error processing.", "OK");
        }
    }

    XDelServiceRef.JobInfo[] alterJobs(XDelServiceRef.JobInfo[] jobs)
    {
        XDelServiceRef.JobInfo[] jobss = null;
        List<XDelServiceRef.JobInfo> list = new();
        List<long> IDXs = new();
        try
        {
            foreach (XDelServiceRef.JobInfo j in jobs)
            {
                if ((j.ParentJobsIDX ?? 0) > 0)
                    continue;
                list.Add(j);
            }
            jobss = list.ToArray();
        } catch (Exception e)
        {
            string s = e.Message;
            jobss = jobs;
        }
        return jobss;
    }

    private async Task search()
    {
        XDelServiceRef.JobStructure? jobarray = null;
        XDelServiceRef.JobStructure? unpostedJob = null;
        XDelServiceRef.JobInfo[] jobs = null;
        XDelServiceRef.JobInfo[] upjobs = null;
        XDelServiceRef.JobInfo[] mergedjobs = null;

        try
        {
            string par = txtRefNo.Text?.Trim();

            if (string.IsNullOrEmpty(par))
            {
                await DisplayAlertAsync("", "Please enter reference to search.", "OK");
                return;
            }

            string mode = AppSession.LoginMode;
            logininfo = AppSession.logininfo;

            if (logininfo?.clientInfo == null ||
            string.IsNullOrEmpty(logininfo.clientInfo.Web_UID) ||
            mode.Equals("s") && logininfo.clientInfo.CAIDX <= 0)
            {
                await DisplayAlertAsync("Session expired", "Please Login again.", "OK");
                await common.BackToLogin();
                return;
            }

            await showProgress_Dialog("Processing...");

            ClientInfo ci = logininfo.clientInfo;

            // Run SOAP query on background thread
            jobarray = await Task.Run(async () =>
            {
                return mode.Equals("s") ? await xs.FindJobAsync(ci.Web_UID, par, 1) : await xs.XOE_FindJobAsync(ci.Web_UID, par);
            });

            if (mode.Equals("s"))
                unpostedJob = await Task.Run(async () =>
                {
                    return await xs.GetUnpostedJobsAsync(ci.Web_UID);
                });

            if (jobarray != null)
                jobs = alterJobs(jobarray.JobList);

            if (unpostedJob != null)
                upjobs = alterJobs(unpostedJob.JobList);

            mergedjobs = (jobs ?? Array.Empty<XDelServiceRef.JobInfo>())
                .Concat(upjobs ?? Array.Empty<XDelServiceRef.JobInfo>())
                .GroupBy(j => j.JobsIDX)
                .Select(g => g.Last())   // use Last() if upjobs should override jobs
                .ToArray();

            if (mergedjobs == null || mergedjobs.Length == 0)
            {
                await closeProgress_dialog();
                if (BindingContext is ManageJobPageVM vm)
                {
                    await DisplayAlertAsync("", "No job found.", "OK");
                    await vm.ClearTabsCommand.ExecuteAsync(null);
                }
                return;
            }

            // Heavy data processing off the main thread
            ManageJobPageVM.LoadTabsRequest request =
                await Task.Run(() => getData2(ci.Web_UID, mergedjobs));

            await closeProgress_dialog();

            // Update UI bindings on the main thread
            if (BindingContext is ManageJobPageVM Vm)
            {
                _firstTabSelected = false;
                await MainThread.InvokeOnMainThreadAsync(async () =>
                {
                    await Vm.LoadTabs2Command.ExecuteAsync(request);
                });
            }
        } catch (Exception e)
        {
            string s = e.Message;
            await closeProgress_dialog();
            await DisplayAlertAsync("Exception", "Error processing.", "OK");
        }
    }

    private ManageJobPageVM.LoadTabsRequest getEmptyData()
    {
        return new ManageJobPageVM.LoadTabsRequest(null, null, null, null);
    }

    private ManageJobPageVM.LoadTabsRequest getData2(string Web_UID, XDelServiceRef.JobInfo[] jobs)
    {
        ManageJobPageVM.LoadTabsRequest request = new ManageJobPageVM.LoadTabsRequest(null, null, null, null);

        XDelServiceRef.JobStructure? unpostedJob;

        ManageJobSelector mjs = null;

        string InvPDFURL = "";
        string JobNo, RefNo, Status, SvcType, ExpType, ContentType,
            FromAdd, PUBLOCK, PUSTREET, PUUNIT, PUBUILDING, PUPOSTALCODE, PURdy, PUCompany, Sender, PUTel, PUMobile, PUInstruction, PUAvoid,
            ToAdd, DLBLOCK, DLSTREET, DLUNIT, DLBUILDING, DLPOSTALCODE, DLCompany, Receiver, DLTel, DLMobile, DLInstruction, DLAvoid, DateFrom, DateTo,
            ConsignmentURL, ScanURL, TrackURL, SignaturesURL;
        int DLLocationType = 0;
        int? Pcs = 0;
        int? Weight = 0;
        int FLAG = 0;
        int FLAG2 = 0;
        Int64? StatusCodeIDX = -1;
        decimal? COD = 0;
        bool hasAltDelWin;
        string PULocation, DLLocation;
        XDelServiceRef.AddressStructure? PL;
        XDelServiceRef.AddressStructure? DL;
        List<long> addedIDXs = new List<long>();
        bool isp1 = false;
        bool isp2 = false;

        try
        {
            string mode = AppSession.LoginMode;
            var statusDict = new Dictionary<long, string>();
            XDelServiceRef.StatusCodeStructure[] statuscodeStruct = common.getAllStatusCodes(Web_UID, null, mode);

            if (statuscodeStruct != null)
            {
                foreach (var s in statuscodeStruct)
                    if (!statusDict.ContainsKey(s.IDX))
                        statusDict[s.IDX] = s.Description;
            }

            var Dra = new List<ManageJobSelector>();
            var InP = new List<ManageJobSelector>();
            var Att = new List<ManageJobSelector>();
            var Comp = new List<ManageJobSelector>();

            const string trackurl = "https://www.xdel.com/xdeltrack/trackresult.aspx?&JOBSIDX={0}&Hash={1}";

            foreach (var job in jobs)
            {
                //if (string.IsNullOrEmpty(job.JobNo)) continue;
                isp1 = (job.LinkedJobs != null && job.LinkedJobs.Count() > 0);
                isp2 = (job.ParentJobsIDX ?? 0) > 0;
                JobNo = "";
                RefNo = "";
                Status = "";
                SvcType = "";
                ExpType = "";
                ContentType = "";
                FromAdd = "";
                PUBLOCK = "";
                PUSTREET = "";
                PUUNIT = "";
                PUBUILDING = "";
                PUPOSTALCODE = "";
                PURdy = "";
                PUCompany = "";
                Sender = "";
                PUTel = "";
                PUMobile = "";
                PUInstruction = "";
                PUAvoid = "";
                ToAdd = "";
                DLBLOCK = "";
                DLSTREET = "";
                DLUNIT = "";
                DLBUILDING = "";
                DLPOSTALCODE = "";
                DLLocationType = 0;
                DLCompany = "";
                Receiver = "";
                DLTel = "";
                DLMobile = "";
                DLInstruction = "";
                DLAvoid = "";
                DateFrom = "";
                DateTo = "";
                PULocation = "";
                DLLocation = "";
                ConsignmentURL = "";
                ScanURL = "";
                TrackURL = "";
                SignaturesURL = "";
                InvPDFURL = "";
                StatusCodeIDX = -1;
                COD = 0;
                Pcs = 0;
                Weight = 0;

                Pcs = job.Pieces;
                Weight = job.Weight;

                FLAG = job.FLAG;
                FLAG2 = job.FLAG2;

                JobNo = String.IsNullOrEmpty(job.JobNo) ? "" : job.JobNo;
                RefNo = String.IsNullOrEmpty(job.ConsignmentNote) ? "" : job.ConsignmentNote;

                ConsignmentURL = String.IsNullOrEmpty(job.CNURL) ? "" : job.CNURL;
                ScanURL = String.IsNullOrEmpty(job.ScansURL) ? "" : job.ScansURL;
                TrackURL = String.Format(trackurl, job.JobsIDX.ToString(), Hash.GetEncodedHash(job.JobsIDX.ToString()));
                SignaturesURL = String.IsNullOrEmpty(job.SignaturesURL) ? "" : job.SignaturesURL;
                COD = job.COD == null ? 0 : job.COD;
                InvPDFURL = COD > 0 && job.JobsIDX > 0 ? XDelSys.JobInfo.FormatURL(XWS.JobInfo.ReceiptPDFDirectURL, (long)(job.JobsIDX ?? 0)) : "";
                StatusCodeIDX = job.StatusCodeIDX;

                var statusCode = string.IsNullOrEmpty(JobNo) ? -1 : job.StatusCodeIDX ?? 0;
                Status = statusDict.TryGetValue(statusCode, out var desc) ? desc : "Please Call";
                if (statusCode == 0) Status = "Awaiting Pickup";
                else if (statusCode == 165) Status = "New";
                else if (statusCode is 391 or 392) Status = "Failed Attempt Pending Approval";
                else if (statusCode == -1) Status = "";
                if (statusCode == -1)
                    JobNo = job.JobsIDX.ToString();

                SvcType = common.getServiceSelected(job.TOSType);
                ExpType = common.convertExpressType(job.ExpressType);
                ContentType = job.DeliveryContents ?? "";

                hasAltDelWin = (job.ExtFromDateTime != DateTime.MinValue) &&
                           (job.ExtDateTime != DateTime.MinValue) && job.ExpressType != eExpressType.etNormal;

                DateFrom = job.FromDateTime.ToString("dd/MM/yyyy h:mm tt");
                DateTo = job.ExpressType == eExpressType.etNormal && !hasAltDelWin ?
                    job.ToDateTime.ToString("dd/MM/yyyy h:mm tt") + " else, latest by " + job.ExtDateTime.ToString("dd/MM/yyyy h:mm tt") :

                    job.ExpressType != eExpressType.etNormal && hasAltDelWin ?
                    job.ToDateTime.ToString("dd/MM/yyyy h:mm tt") + "\r\nALTERNATIVELY\r\n" + job.ExtFromDateTime.ToString("dd/MM/yyyy h:mm tt") + " to " + job.ExtDateTime.ToString("dd/MM/yyyy h:mm tt") :
                    job.ToDateTime.ToString("dd/MM/yyyy h:mm tt");

                PURdy = job.ReadyDateTime.ToString("dd/MM/yyyy h:mm tt");

                PL = job.PURedirectedLocation != null ? job.PURedirectedLocation : job.PULocation;
                if (PL != null && PL.Contacts != null && PL.Contacts.Length > 0)
                {
                    if (!String.IsNullOrEmpty(PL.Contacts[0].NAME))
                        Sender = PL.Contacts[0].NAME;
                    else Sender = "No Information";

                    PUTel = PL.Contacts[0].TEL;
                    PUMobile = PL.Contacts[0].MOBILE;
                }

                PUInstruction = !String.IsNullOrEmpty(job.PUSI) ? job.PUSI : "";
                PUAvoid = job.PULunch_Avoid ? ((job.PULunch_From.ToString("dd/MM/yyyy h:mm tt")) + " to " + (job.PULunch_To.ToString("dd/MM/yyyy h:mm tt"))) : "";

                PUCompany = PL != null && !String.IsNullOrEmpty(PL.COMPANY) ? PL.COMPANY : "";

                if (PL != null && !String.IsNullOrEmpty(PL.BLOCK))
                    FromAdd = PL.BLOCK;
                if (PL != null && !String.IsNullOrEmpty(PL.STREET))
                    FromAdd += (!String.IsNullOrEmpty(FromAdd) ? ", " : "") + PL.STREET;
                if (PL != null && !String.IsNullOrEmpty(PL.UNIT))
                    FromAdd += (!String.IsNullOrEmpty(FromAdd) ? ", " : "") + PL.UNIT;
                if (PL != null && !String.IsNullOrEmpty(PL.BUILDING))
                    FromAdd += (!String.IsNullOrEmpty(FromAdd) ? ", " : "") + PL.BUILDING;

                if (PL != null && !String.IsNullOrEmpty(PL.BLOCK))
                    PUBLOCK = PL.BLOCK;
                if (PL != null && !String.IsNullOrEmpty(PL.STREET))
                    PUSTREET = PL.STREET;
                if (PL != null && !String.IsNullOrEmpty(PL.UNIT))
                    PUUNIT = PL.UNIT;
                if (PL != null && !String.IsNullOrEmpty(PL.BUILDING))
                    PUBUILDING = PL.BUILDING;
                if (PL != null && !String.IsNullOrEmpty(PL.POSTALCODE))
                    PUPOSTALCODE = PL.POSTALCODE;

                DL = job.DLRedirectedLocation != null ? job.DLRedirectedLocation : job.DLLocation;

                if (DL != null && DL.Contacts != null && DL.Contacts.Length > 0)
                {
                    if (!String.IsNullOrEmpty(DL.Contacts[0].NAME))
                        Receiver = DL.Contacts[0].NAME;
                    else Receiver = "No Information";

                    DLTel = DL.Contacts[0].TEL;
                    DLMobile = DL.Contacts[0].MOBILE;
                }

                DLInstruction = !String.IsNullOrEmpty(job.DLSI) ? job.DLSI : "";
                DLAvoid = job.DLLunch_Avoid ? ((job.DLLunch_From.ToString("dd/MM/yyyy h:mm tt")) + " to " + (job.DLLunch_To.ToString("dd/MM/yyyy h:mm tt"))) : "";

                DLCompany = DL != null && !String.IsNullOrEmpty(DL.COMPANY) ? DL.COMPANY : "";

                if (DL != null && !String.IsNullOrEmpty(DL.BLOCK))
                    ToAdd = DL.BLOCK;
                if (DL != null && !String.IsNullOrEmpty(DL.STREET))
                    ToAdd += (!String.IsNullOrEmpty(ToAdd) ? ", " : "") + DL.STREET;
                if (DL != null && !String.IsNullOrEmpty(DL.UNIT))
                    ToAdd += (!String.IsNullOrEmpty(ToAdd) ? ", " : "") + DL.UNIT;
                if (DL != null && !String.IsNullOrEmpty(DL.BUILDING))
                    ToAdd += (!String.IsNullOrEmpty(ToAdd) ? ", " : "") + DL.BUILDING;

                if (DL != null && !String.IsNullOrEmpty(DL.BLOCK))
                    DLBLOCK = DL.BLOCK;
                if (DL != null && !String.IsNullOrEmpty(DL.STREET))
                    DLSTREET = DL.STREET;
                if (DL != null && !String.IsNullOrEmpty(DL.UNIT))
                    DLUNIT = DL.UNIT;
                if (DL != null && !String.IsNullOrEmpty(DL.BUILDING))
                    DLBUILDING = DL.BUILDING;
                if (DL != null && !String.IsNullOrEmpty(DL.POSTALCODE))
                    DLPOSTALCODE = DL.POSTALCODE;

                if (DL != null)
                    DLLocationType = DL.LocationType == XDelServiceRef.Location_Type.Residential ? 1 :
                        DL.LocationType == XDelServiceRef.Location_Type.Office ? 2 : 2;

                var item = new ManageJobSelector
                {
                    JobsIDX = job.JobsIDX != null ? (long)job.JobsIDX : 0,
                    JobNo = JobNo,
                    RefNo = RefNo,
                    Status = Status,
                    SvcType = SvcType,
                    ExpType = ExpType,
                    ContentType = ContentType,
                    FromAdd = FromAdd,
                    PUBLOCK = PUBLOCK,
                    PUSTREET = PUSTREET,
                    PUUNIT = PUUNIT,
                    PUBUILDING = PUBUILDING,
                    PUPOSTALCODE = PUPOSTALCODE,
                    PURdy = PURdy,
                    PUCompany = PUCompany,
                    Sender = Sender,
                    PUTel = PUTel,
                    PUMobile = PUMobile,
                    PUInstruction = PUInstruction,
                    PUAvoid = PUAvoid,
                    ToAdd = ToAdd,
                    DLBLOCK = DLBLOCK,
                    DLSTREET = DLSTREET,
                    DLUNIT = DLUNIT,
                    DLBUILDING = DLBUILDING,
                    DLPOSTALCODE = DLPOSTALCODE,
                    DLLocationType = DLLocationType,
                    DLCompany = DLCompany,
                    Receiver = Receiver,
                    DLTel = DLTel,
                    DLMobile = DLMobile,
                    DLInstruction = DLInstruction,
                    DLAvoid = DLAvoid,
                    DateFrom = DateFrom,
                    DateTo = DateTo,
                    ConsignmentURL = ConsignmentURL,
                    ScanURL = ScanURL,
                    TrackURL = TrackURL,
                    SignaturesURL = SignaturesURL,
                    COD = COD,
                    InvPDFURL = InvPDFURL,
                    StatusCodeIDX = StatusCodeIDX,
                    Pcs = Pcs,
                    Weight = Weight,
                    ShowP1 = isp1,
                    ShowP2 = isp2,
                    ShowDelFrom = !mode.Equals("r"),
                    FLAG = FLAG,
                    FLAG2 = FLAG2
                };

                // Categorize job
                if (InProgress.Contains("," + statusCode + ",")) item.TabIndex = 1;
                else if (Attempted.Contains("," + statusCode + ",")) item.TabIndex = 2;
                else if (Completed.Contains("," + statusCode + ",")) item.TabIndex = 3;
                else item.TabIndex = 0;

                switch (item.TabIndex)
                {
                    case 1: InP.Add(item); break;
                    case 2: Att.Add(item); break;
                    case 3: Comp.Add(item); break;
                    default: Dra.Add(item); break;
                }


            }

            // Sort once at the end
            Dra = Dra.OrderByDescending(j => j.JobsIDX).ToList();
            InP = InP.OrderByDescending(j => j.JobNo).ToList();
            Att = Att.OrderByDescending(j => j.JobNo).ToList();
            Comp = Comp.OrderByDescending(j => j.JobNo).ToList();

            return new ManageJobPageVM.LoadTabsRequest(
                new ObservableCollection<ManageJobSelector>(Dra),
                new ObservableCollection<ManageJobSelector>(InP),
                new ObservableCollection<ManageJobSelector>(Att),
                new ObservableCollection<ManageJobSelector>(Comp)
            );
        }
        catch (Exception e)
        {
            System.Diagnostics.Debug.WriteLine($"getData error: {e.Message}");
            return new ManageJobPageVM.LoadTabsRequest(null, null, null, null);
        }
    }

    private ObservableCollection<ManageJobSelector> SortByJobNoDesc(ObservableCollection<ManageJobSelector> mjss)
    {
        try
        {
            if (mjss == null || mjss.Count == 0)
                return mjss;

            var sorted = mjss.OrderByDescending(j => j.JobNo).ToList();
            // Clear and re-add in sorted order
            mjss.Clear();
            foreach (var job in sorted)
            {
                mjss.Add(job);
            }
        } catch (Exception e)
        {
            string s = e.Message;
        }
        return mjss;
    }



    void btnHelp_Clicked(System.Object sender, System.EventArgs e)
    {
        try
        {
            show_BSHelp();
        }
        catch (Exception ex)
        {
            string s = ex.Message;
        }
    }

    #region BOTTOMSHEET

    //HELP
    async void show_BSHelp()
    {
        try
        {
            //await svBSHelp.ScrollToAsync(0, 0, false);
            //await BSHelp.OpenBottomSheet(false);
            //BSHelp.IsVisible = true;
            //BSHelp.isShowing = true;
        }
        catch (Exception e)
        {
            string s = e.Message;
        }
    }

    async void CloseBSHelp_Clicked(object sender, EventArgs e)
    {
        try
        {
            //await BSHelp.CloseBottomSheet();
            //BSHelp.IsVisible = false;
            //BSHelp.isShowing = false;
        }
        catch (Exception ex)
        {
            string s = ex.Message;
        }
    }


    #region OPTIONS

    private async void tabOnItemTap(object sender, EventArgs e)
    {
        try
        {
            var tappedLayout = sender as VerticalStackLayout;
            if (tappedLayout != null)
            {
                var selectedItem = tappedLayout.BindingContext as ManageJobSelector;
                string mode = AppSession.LoginMode;
                bool isSender = mode.Equals("s");

                if (isSender && selectedItem != null && selectedItem is ManageJobSelector && BindingContext is ManageJobPageVM vm0 && selectedItem.TabIndex == 0)
                {
                    if (NetworkHelper.IsDisconnected())
                    {
                        await DisplayAlertAsync("No Internet Connection", "Please check your internet connection and try again.", "OK");
                        return;
                    }

                    CreateJobVM vmm = new CreateJobVM() { JobsIDX = selectedItem.JobsIDX };
                    ltr = vm.request_stored;
                    await Shell.Current.GoToAsync("/JobSummary", new Dictionary<string, object>
                        {
                            { "vmm", vmm },
                            { "LTR", ltr },
                            { "Source", "ManageJobPage" }
                        });
                }
                else if (selectedItem != null && selectedItem is ManageJobSelector && BindingContext is ManageJobPageVM vm)
                {
                    vm.newOptions();
                    vm.addOptions(new ManageJobOptionSelector
                    {
                        value = "details",
                        DispText = "View Details",
                        JobsIDX = selectedItem.JobsIDX,
                        JobNo = selectedItem.JobNo,
                        RefNo = selectedItem.RefNo,
                        ConsignmentURL = selectedItem.ConsignmentURL,
                        ScanURL = selectedItem.ScanURL,
                        TrackURL = selectedItem.TrackURL,
                        SignaturesURL = selectedItem.SignaturesURL,
                        InvPDFURL = selectedItem.InvPDFURL,
                        Status = selectedItem.Status,
                        StatusCodeIDX = selectedItem.StatusCodeIDX,
                        SvcType = selectedItem.SvcType,
                        ExpType = selectedItem.ExpType,
                        ContentType = selectedItem.ContentType,
                        Pieces = selectedItem.Pcs,
                        Weight = selectedItem.Weight,
                        PUAddress = selectedItem.FromAdd,
                        PUBLOCK = selectedItem.PUBLOCK,
                        PUSTREET = selectedItem.PUSTREET,
                        PUUNIT = selectedItem.PUUNIT,
                        PUBUILDING = selectedItem.PUBUILDING,
                        PUPOSTALCODE = selectedItem.PUPOSTALCODE,
                        PURdy = selectedItem.PURdy,
                        PUCompany = selectedItem.PUCompany,
                        Sender = selectedItem.Sender,
                        PUTel = selectedItem.PUTel,
                        PUMobile = selectedItem.PUMobile,
                        PUInstruction = selectedItem.PUInstruction,
                        PUAvoid = selectedItem.PUAvoid,
                        DLAddress = selectedItem.ToAdd,
                        DLBLOCK = selectedItem.DLBLOCK,
                        DLSTREET = selectedItem.DLSTREET,
                        DLUNIT = selectedItem.DLUNIT,
                        DLBUILDING = selectedItem.DLBUILDING,
                        DLPOSTALCODE = selectedItem.DLPOSTALCODE,
                        DLLocationType = selectedItem.DLLocationType,
                        DLCompany = selectedItem.DLCompany,
                        Receiver = selectedItem.Receiver,
                        DLTel = selectedItem.DLTel,
                        DLMobile = selectedItem.DLMobile,
                        DLInstruction = selectedItem.DLInstruction,
                        DLAvoid = selectedItem.DLAvoid,
                        DateFrom = selectedItem.DateFrom,
                        DateTo = selectedItem.DateTo
                    });

                    if (selectedItem.TabIndex > 0)
                    {
                        if (isSender && !string.IsNullOrEmpty(selectedItem.ConsignmentURL))
                            vm.addOptions(new ManageJobOptionSelector
                            {
                                value = "cn",
                                DispText = "Consignment Note",
                                JobsIDX = selectedItem.JobsIDX,
                                ConsignmentURL = selectedItem.ConsignmentURL,
                                ScanURL = selectedItem.ScanURL,
                                TrackURL = selectedItem.TrackURL,
                                SignaturesURL = selectedItem.SignaturesURL,
                                InvPDFURL = selectedItem.InvPDFURL
                            });

                        if (isSender && !string.IsNullOrEmpty(selectedItem.ScanURL))
                            vm.addOptions(new ManageJobOptionSelector
                            {
                                value = "pod",
                                DispText = "View POD",
                                JobsIDX = selectedItem.JobsIDX,
                                ConsignmentURL = selectedItem.ConsignmentURL,
                                ScanURL = selectedItem.ScanURL,
                                TrackURL = selectedItem.TrackURL,
                                SignaturesURL = selectedItem.SignaturesURL,
                                InvPDFURL = selectedItem.InvPDFURL
                            });

                        if (!string.IsNullOrEmpty(selectedItem.TrackURL))
                            vm.addOptions(new ManageJobOptionSelector
                            {
                                value = "track",
                                DispText = "Track Job",
                                JobsIDX = selectedItem.JobsIDX,
                                ConsignmentURL = selectedItem.ConsignmentURL,
                                ScanURL = selectedItem.ScanURL,
                                TrackURL = selectedItem.TrackURL,
                                SignaturesURL = selectedItem.SignaturesURL,
                                InvPDFURL = selectedItem.InvPDFURL
                            });

                        if ((selectedItem.StatusCodeIDX == Common.C_DIP || selectedItem.StatusCodeIDX == Common.C_Parked) && (selectedItem.FLAG & 2097152) == 2097152)
                        {
                            vm.addOptions(new ManageJobOptionSelector
                            {
                                value = "livetrack",
                                DispText = "Live Tracking",
                                JobsIDX = selectedItem.JobsIDX,
                                ConsignmentURL = selectedItem.ConsignmentURL,
                                ScanURL = selectedItem.ScanURL,
                                TrackURL = selectedItem.TrackURL,
                                SignaturesURL = selectedItem.SignaturesURL,
                                InvPDFURL = selectedItem.InvPDFURL
                            });
                        }

                        if (isSender && (!string.IsNullOrEmpty(selectedItem.SignaturesURL)))
                            vm.addOptions(new ManageJobOptionSelector
                            {
                                value = "sign",
                                DispText = "View Signature",
                                JobsIDX = selectedItem.JobsIDX,
                                ConsignmentURL = selectedItem.ConsignmentURL,
                                ScanURL = selectedItem.ScanURL,
                                TrackURL = selectedItem.TrackURL,
                                SignaturesURL = selectedItem.SignaturesURL,
                                InvPDFURL = selectedItem.InvPDFURL
                            });

                        if (isSender && (selectedItem.TabIndex == 1 || selectedItem.TabIndex == 2))
                            vm.addOptions(new ManageJobOptionSelector
                            {
                                value = "cancel",
                                DispText = "Stop Shipment",
                                JobsIDX = selectedItem.JobsIDX,
                                ConsignmentURL = selectedItem.ConsignmentURL,
                                ScanURL = selectedItem.ScanURL,
                                TrackURL = selectedItem.TrackURL,
                                SignaturesURL = selectedItem.SignaturesURL,
                                InvPDFURL = selectedItem.InvPDFURL
                            });

                        if (isSender && (selectedItem.TabIndex == 2))
                            vm.addOptions(new ManageJobOptionSelector
                            {
                                value = "instruction",
                                DispText = "Add Instruction(s)",
                                JobsIDX = selectedItem.JobsIDX,
                                ConsignmentURL = selectedItem.ConsignmentURL,
                                ScanURL = selectedItem.ScanURL,
                                TrackURL = selectedItem.TrackURL,
                                SignaturesURL = selectedItem.SignaturesURL,
                                InvPDFURL = selectedItem.InvPDFURL
                            });

                        // (isSender)
                            vm.addOptions(new ManageJobOptionSelector
                            {
                                value = "vcs",
                                DispText = "Enquire via VCS",
                                JobsIDX = selectedItem.JobsIDX,
                                ConsignmentURL = selectedItem.ConsignmentURL,
                                ScanURL = selectedItem.ScanURL,
                                TrackURL = selectedItem.TrackURL,
                                SignaturesURL = selectedItem.SignaturesURL,
                                InvPDFURL = selectedItem.InvPDFURL
                            });

                        if (isSender && (selectedItem.COD != null && selectedItem.COD > 0 && !string.IsNullOrEmpty(selectedItem.InvPDFURL)))
                            vm.addOptions(new ManageJobOptionSelector
                            {
                                value = "invpdf",
                                DispText = "Receipt PDF",
                                JobsIDX = selectedItem.JobsIDX,
                                ConsignmentURL = selectedItem.ConsignmentURL,
                                ScanURL = selectedItem.ScanURL,
                                TrackURL = selectedItem.TrackURL,
                                SignaturesURL = selectedItem.SignaturesURL,
                                InvPDFURL = selectedItem.InvPDFURL
                            });
                    }

                    removeTapGestureRecognizer();

                    cvOptions.ItemsSource = vm.Options;
                    await BSOptions.OpenBottomSheet(false);
                    BSOptions.IsVisible = true;
                    BSOptions.isShowing = true;
                }
            }
        }
        catch (Exception ex)
        {
            string s = ex.Message;
        }
    }

    private async void cvOptionsOnItemTap(object sender, EventArgs e)
    {
        long JobsIDX = 0;
        string value, URL = "";
        string[] allowedOpenURLValues = { "cn", "pod", "track", "sign", "invpdf", "livetrack",  };
        try
        {
            var tappedLayout = sender as Grid;
            if (tappedLayout != null)
            {
                var selectedItem = tappedLayout.BindingContext as ManageJobOptionSelector;

                //// Ensure the ViewModel exists -- if (BindingContext is ManageJobPageVM vm)
                if (selectedItem != null && selectedItem is ManageJobOptionSelector && BindingContext is ManageJobPageVM vm)
                {
                    logininfo = AppSession.logininfo;
                    string UID = logininfo != null && logininfo.clientInfo != null && !string.IsNullOrEmpty(logininfo.clientInfo.Web_UID) ? logininfo.clientInfo.Web_UID : "";
                    value = selectedItem.value;
                    bool openurl = allowedOpenURLValues.Any(v => v.Equals(value, StringComparison.OrdinalIgnoreCase));
                    if (openurl)
                        URL =
                            string.Equals(value, "cn", StringComparison.OrdinalIgnoreCase) && !string.IsNullOrEmpty(selectedItem.ConsignmentURL) ? selectedItem.ConsignmentURL :
                            string.Equals(value, "pod", StringComparison.OrdinalIgnoreCase) && !string.IsNullOrEmpty(selectedItem.ScanURL) ? selectedItem.ScanURL :
                            string.Equals(value, "track", StringComparison.OrdinalIgnoreCase) && !string.IsNullOrEmpty(selectedItem.TrackURL) ? selectedItem.TrackURL :
                            string.Equals(value, "sign", StringComparison.OrdinalIgnoreCase) && !string.IsNullOrEmpty(selectedItem.SignaturesURL) ? selectedItem.SignaturesURL :
                            string.Equals(value, "invpdf", StringComparison.OrdinalIgnoreCase) && !string.IsNullOrEmpty(selectedItem.InvPDFURL) ? selectedItem.InvPDFURL :
                            string.Equals(value, "livetrack", StringComparison.OrdinalIgnoreCase) && selectedItem.JobsIDX > 0 ? await xs.XOE_GetLiveTrackingLinkAsync(UID, selectedItem.JobsIDX) :
                            "";

                    if (openurl && !string.IsNullOrEmpty(URL))
                    {
                        if (NetworkHelper.IsDisconnected())
                        {
                            await DisplayAlertAsync("No Internet Connection", "Please check your internet connection and try again.", "OK");
                            return;
                        }

                        //btnCXBSOption_Clicked(null, null);
                        //await Task.Yield();

                        // Close BSOptions first
                        await BSOptions.CloseBottomSheet();
                        BSOptions.IsVisible = false;
                        BSOptions.isShowing = false;
                        addTapGestureRecognizer();

                        // Give iOS time to fully complete the dismiss animation
                        await Task.Delay(400); // iOS needs this breathing room


                        await OpenInBrowser(URL); //tested ok
                        ////await OpenWithBrowser(URL); //tested ok, but difficult to download as pdf, need to open from chrome

                        ////Navigate to this page from anywhere //tested ok
                        //await Navigation.PushAsync(new WebViewPage(URL));
                        return;
                    }

                    if (string.Equals(value, "details", StringComparison.OrdinalIgnoreCase))
                    {
                        //btnCXBSOption_Clicked(null, null);
                        //removeTapGestureRecognizer();
                        //await Task.Yield();
                        //setDetail(selectedItem);
                        //await svBSSummary.ScrollToAsync(0, 0, true);
                        //await BSSummary.OpenBottomSheet(false);
                        //BSSummary.IsVisible = true;
                        //BSSummary.isShowing = true;

                        // Close BSOptions first
                        await BSOptions.CloseBottomSheet();
                        BSOptions.IsVisible = false;
                        BSOptions.isShowing = false;
                        addTapGestureRecognizer();

                        // Give iOS time to fully complete the dismiss animation
                        await Task.Delay(400); // iOS needs this breathing room

                        removeTapGestureRecognizer();
                        setDetail(selectedItem);
                        await svBSSummary.ScrollToAsync(0, 0, false); // use false (no animation) to avoid layering two animations

                        await Task.Delay(100); // let scroll settle

                        await BSSummary.OpenBottomSheet(false);
                        BSSummary.IsVisible = true;
                        BSSummary.isShowing = true;

                    }
                    else if (string.Equals(value, "cancel", StringComparison.OrdinalIgnoreCase))
                    {
                        vm._SelectedJobsIDX = selectedItem.JobsIDX;
                        //btnCXBSOption_Clicked(null, null);

                        // Close BSOptions first
                        await BSOptions.CloseBottomSheet();
                        BSOptions.IsVisible = false;
                        BSOptions.isShowing = false;
                        addTapGestureRecognizer();

                        // Give iOS time to fully complete the dismiss animation
                        await Task.Delay(400); // iOS needs this breathing room

                        removeTapGestureRecognizer();
                        await Task.Yield();
                        await BSSS.OpenBottomSheet(false);
                        BSSS.IsVisible = true;
                        BSSS.isShowing = true;
                    }
                    else if (string.Equals(value, "instruction", StringComparison.OrdinalIgnoreCase))
                    {
                        vm._SelectedJobsIDX = selectedItem.JobsIDX;
                        //await BSOptions.CloseBottomSheet();
                        //BSOptions.IsVisible = false;
                        //BSOptions.isShowing = false;

                        // Close BSOptions first
                        await BSOptions.CloseBottomSheet();
                        BSOptions.IsVisible = false;
                        BSOptions.isShowing = false;
                        addTapGestureRecognizer();

                        // Give iOS time to fully complete the dismiss animation
                        await Task.Delay(400); // iOS needs this breathing room

                        removeTapGestureRecognizer();
                        await Task.Yield();
                        await BSINST.OpenBottomSheet(false);
                        BSINST.IsVisible = true;
                        BSINST.isShowing = true;
                    }
                    else if (string.Equals(value, "vcs", StringComparison.OrdinalIgnoreCase))
                    {
                        if (NetworkHelper.IsDisconnected())
                        {
                            await DisplayAlertAsync("No Internet Connection", "Please check your internet connection and try again.", "OK");
                            return;
                        }

                        //await AlertService.ShowError("", "vcs");
                        JobsIDX = selectedItem.JobsIDX;
                        //await BSOptions.CloseBottomSheet();
                        //BSOptions.IsVisible = false;
                        //BSOptions.isShowing = false;

                        // Close BSOptions first
                        await BSOptions.CloseBottomSheet();
                        BSOptions.IsVisible = false;
                        BSOptions.isShowing = false;
                        addTapGestureRecognizer();

                        // Give iOS time to fully complete the dismiss animation
                        await Task.Delay(400); // iOS needs this breathing room

                        NewChat_Click(JobsIDX);
                    }
                }

            }
        }
        catch (Exception ex)
        {
            string s = ex.Message;
        }
    }

    async void btnCXBSOption_Clicked(object sender, EventArgs e)
    {
        try
        {
            addTapGestureRecognizer();
            await BSOptions.CloseBottomSheet();
            BSOptions.IsVisible = false;
            BSOptions.isShowing = false;
        }
        catch (Exception ex)
        {
            string s = ex.Message;
        }
    }

    private bool canProceedToChat()
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

    async void NewChat_Click(long JobsIDX)
    {
        XDelServiceRef.ChatSession[]? list = null;
        XDelServiceRef.ChatSession[]? list2 = null;
        XDelServiceRef.ChatSession[] finallist = null;
        List<XDelServiceRef.ChatSession> al = new();
        ChatSessions css = null;
        ChatSessions css2 = null;
        ChatSession cs = null;
        ChatSession cs2 = null;
        ChatChild? cc = null;
        ObservableCollection<long>? sIDXs_final = new();
        string errmsg_relogin = "Session expired. Please Login again.";
        string ccref = "";
        string ccTitle = "";
        bool hasVCSSession = false;
        try
        {
            if (canProceedToChat() && JobsIDX > 0)
            {
                string mode = AppSession.LoginMode;
                logininfo = AppSession.logininfo;
                ClientInfo ci = logininfo.clientInfo;
                await showProgress_Dialog("Processing...");

                
                if (mode.Equals("r"))
                    css = await xs.XOE_VCS_Get_ChatsAsync(ci.Web_UID, 0);
                else
                    css = await xs.VCS_Get_ChatsAsync(ci.Web_UID, 0);
                if (css != null && css.Status == 0)
                    list = css.Sessions;

                if (list != null && list.Length > 0)
                {
                    for (int i = 0; i <= list.Length -1; i++)
                    {
                        cs = list[i];
                        if (cs != null && cs.JobsIDX == JobsIDX)
                            al.Add(cs);
                    }

                    if (al != null && al.Count > 0)
                    {
                        if (al[0].Chat_Status == Status.csClosed)
                        {
                            
                            if (mode.Equals("r"))
                                css2 = await xs.XOE_VCS_CreateAsync(ci.Web_UID, JobsIDX);
                            else
                                css2 = await xs.VCS_CreateAsync(ci.Web_UID, JobsIDX);
                            if (css2 != null && css2.Status == 0)
                                al.Insert(0, css2.Sessions[0]);
                        }

                        finallist = al.ToArray();
                        //Array.Reverse(finallist);
                    } else
                    {
                        
                        if (mode.Equals("r"))
                            css2 = await xs.XOE_VCS_CreateAsync(ci.Web_UID, JobsIDX);
                        else
                            css2 = await xs.VCS_CreateAsync(ci.Web_UID, JobsIDX);
                        if (css2 != null && css2.Status == 0)
                            finallist = css2.Sessions;
                    }
                } else
                {
                    
                    if (mode.Equals("r"))
                        css2 = await xs.XOE_VCS_CreateAsync(ci.Web_UID, JobsIDX);
                    else
                        css2 = await xs.VCS_CreateAsync(ci.Web_UID, JobsIDX);
                    if (css2 != null && css2.Status == 0)
                        finallist = css2.Sessions;
                }

                await closeProgress_dialog();

                if (finallist != null && finallist.Length > 0)
                {
                    foreach (var c in finallist)
                    {
                        sIDXs_final.Add(c.SessionIDX);
                        if (!hasVCSSession && (c.Chat_Status == Status.csOpen || c.Chat_Status == Status.csPrivate))
                            hasVCSSession = true;
                    }
                    cs2 = finallist[0];

                    ccref = !string.IsNullOrEmpty(cs2.Reference) ? cs2.Reference : "";
                    if (!string.IsNullOrEmpty(ccref) && ccref.Substring(ccref.Length - 1).Equals("/"))
                        ccref = ccref.Substring(0, ccref.Length - 1);

                    ccTitle = !string.IsNullOrEmpty(ccref) && cs2.TimeStamp != DateTime.MinValue ? cs2.TimeStamp.ToString("hh:mm:tt") + "\r\n" + ccref :
                        cs2.TimeStamp != DateTime.MinValue ? cs2.TimeStamp.ToString("hh:mm:tt") : "";

                    cc = new ChatChild { Fulltext = ccTitle, Reference = ccref, Chatsession = cs2, SessionIDXs = sIDXs_final, isNewChat = hasVCSSession, Status = hasVCSSession ? 1 : 0 };

                    ltr = vm.request_stored;

                    /*await Shell.Current.GoToAsync("ChatPage", new Dictionary<string, object>
                    {
                        { "CHATCHILD", cc },
                        { "CHATTITLE", ccref },
                        { "CHATSTATUS", 1 },
                        { "LOGININFO",  logininfo }
                    });*/
                    await Shell.Current.GoToAsync("ChatPage", new Dictionary<string, object>
                    {
                        { "CHATCHILD", cc },
                        { "CHATTITLE", ccref },
                        { "CHATSTATUS", 1 },
                        { "LTR", ltr }
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

    //OptionsActions

    ///Open URL in the device's default browser
    // Async method to open URL
    async Task OpenInBrowser(string url)
    {
        try
        {
            //if (await Launcher.CanOpenAsync(url))
            //{
            //    await Launcher.OpenAsync(url); // opens in default browser
            //}
            //else
            //{
            //    await AlertService.ShowError("Error", "Cannot open URL");
            //}
            await Launcher.OpenAsync(new Uri(url)); // just try to open
        }
        catch (Exception e)
        {
            string s = e.Message;
            await AlertService.ShowError("Error", "Cannot open URL");
        }
    }

    ///Optional: Use a more advanced in-app browser (like SafariViewController / Chrome Custom Tabs)
    async Task OpenWithBrowser(string url)
    {
        try
        {
            await Browser.Default.OpenAsync(url, BrowserLaunchMode.SystemPreferred);
        }
        catch (Exception e)
        {
            string s = e.Message;
            await AlertService.ShowError("Error", "Cannot open URL");
        }
    }

    #endregion

    #region STOP SHIPMENT

    async void CloseBSSS_Clicked(object sender, EventArgs e)
    {
        try
        {
            if (BSOptions.isShowing)
                await BSOptions.CloseBottomSheet();
            if (BSOptions.IsVisible)
                BSOptions.IsVisible = false;
            if (BSOptions.isShowing)
                BSOptions.isShowing = false;

            await BSSS.CloseBottomSheet();
            BSSS.IsVisible = false;
            BSSS.isShowing = false;

            addTapGestureRecognizer();

        }
        catch (Exception ex)
        {
            string s = ex.Message;
        }
    }

    async void CloseBSSSSave_Clicked(object sender, EventArgs e)
    {
        try
        {
            if (string.IsNullOrEmpty(txtReason.Text))
            {
                await AlertService.ShowError("", "Please enter your reason to STOP this shipment.");
            }
            else
            {
                await cancelJob();
            }
        }
        catch (Exception ex)
        {
            string s = ex.Message;
        }
    }

    private async Task cancelJob()
    {
        try
        {
            //// Ensure the ViewModel exists -- if (BindingContext is ManageJobPageVM vm)

            if (logininfo != null && logininfo.clientInfo != null && !String.IsNullOrEmpty(logininfo.clientInfo.Web_UID) && logininfo.clientInfo.CAIDX > 0 && BindingContext is ManageJobPageVM vm && vm._SelectedJobsIDX > 0)
            {
                string rea = txtReason.Text;

                await showProgress_Dialog("Processing...");

                ClientInfo ci = logininfo.clientInfo;

                XDelServiceRef.XWSBase xb = await Task.Run(async () =>
                {
                    return await xs.CancelJobAsync(ci.Web_UID, vm._SelectedJobsIDX, rea);
                });

                if (xb != null && xb.Status == 0)
                    txtReason.Text = "";
                await Task.Delay(500);

                await closeProgress_dialog();

                await AlertService.ShowError((xb == null) ? "Error" : "",
                    (xb == null) ? "Unable to submit this request." :
                    (xb != null && xb.Status == 0) ? "Your request to stop this shipment has been received successfully.\nIf there are any issue, our Customer Service will contact you shortly." :
                    "Unable to submit this request."
                    );

                CloseBSSS_Clicked(null, null);
            }
            else
            {
                await AlertService.ShowError("Error", "Unable to submit this request.");
                CloseBSSS_Clicked(null, null);
            }
        }
        catch (Exception e)
        {
            string s = e.Message;// Ensure dismiss is always attempted

            await closeProgress_dialog();
            await AlertService.ShowError("Error", "Unable to submit this request.");
            CloseBSSS_Clicked(null, null);
        }
    }


    #endregion


    #region ADD INSTRUCTIONS

    async void CloseBSINST_Clicked(object sender, EventArgs e)
    {
        try
        {
            if (BSOptions.isShowing)
                await BSOptions.CloseBottomSheet();
            if (BSOptions.IsVisible)
                BSOptions.IsVisible = false;
            if (BSOptions.isShowing)
                BSOptions.isShowing = false;

            await BSINST.CloseBottomSheet();
            BSINST.IsVisible = false;
            BSINST.isShowing = false;

            addTapGestureRecognizer();

        }
        catch (Exception ex)
        {
            string s = ex.Message;
        }
    }

    async void CloseBSINSTSave_Clicked(object sender, EventArgs e)
    {
        try
        {
            if (string.IsNullOrEmpty(txtIns.Text))
            {
                await AlertService.ShowError("", "Please enter your instruction to proceed.");
            }
            else
            {
                if (NetworkHelper.IsDisconnected())
                {
                    await DisplayAlertAsync("No Internet Connection", "Please check your internet connection and try again.", "OK");
                    return;
                }

                await saveInstruction();
            }
        }
        catch (Exception ex)
        {
            string s = ex.Message;
        }
    }

    private async Task saveInstruction()
    {
        try
        {
            //// Ensure the ViewModel exists -- if (BindingContext is ManageJobPageVM vm)

            if (logininfo != null && logininfo.clientInfo != null && !String.IsNullOrEmpty(logininfo.clientInfo.Web_UID) && logininfo.clientInfo.CAIDX > 0 && BindingContext is ManageJobPageVM vm && vm._SelectedJobsIDX > 0)
            {
                string ins = txtIns.Text;

                await showProgress_Dialog("Processing...");

                ClientInfo ci = logininfo.clientInfo;

                XDelServiceRef.XWSBase xb = await Task.Run(async () =>
                {
                    XDelServiceRef.eInstructions eins = XDelServiceRef.eInstructions.jiGeneralInstructions;
                    return await xs.AddJobInstructionsAsync(ci.Web_UID, vm._SelectedJobsIDX, eins, ins);
                });

                if (xb != null && xb.Status == 0)
                    txtIns.Text = "";
                await Task.Delay(500);

                await closeProgress_dialog();

                await AlertService.ShowError((xb == null) ? "Error" : "",
                    (xb == null) ? "Unable to submit new instruction." :
                    (xb != null && xb.Status == 0) ? "Your instructions has been received successfully." :
                    "Unable to submit new instruction."
                    );

                CloseBSINST_Clicked(null, null);
            }
            else
            {
                await AlertService.ShowError("Error", "Unable to submit new instruction.");
                CloseBSINST_Clicked(null, null);
            }
        }
        catch (Exception e)
        {
            string s = e.Message;// Ensure dismiss is always attempted

            await closeProgress_dialog();
            await AlertService.ShowError("Error", "Unable to submit new instruction.");
            CloseBSINST_Clicked(null, null);
        }
    }

    #endregion

    #region SUMMARY

    async void CloseBSSummary_Clicked(object sender, EventArgs e)
    {
        try
        {
            if (BindingContext != null && BindingContext is ManageJobPageVM vm)
            {
                vm.changeSetting = null;
                vm.txtPostalTextChangedSubscribed = false;
            }

            if (BSOptions.isShowing)
                await BSOptions.CloseBottomSheet();
            if (BSOptions.IsVisible)
                BSOptions.IsVisible = false;
            if (BSOptions.isShowing)
                BSOptions.isShowing = false;

            await BSSummary.CloseBottomSheet();
            BSSummary.IsVisible = false;
            BSSummary.isShowing = false;

            addTapGestureRecognizer();

        }
        catch (Exception ex)
        {
            string s = ex.Message;
        }
    }

    private void setDetail(ManageJobOptionSelector selectedItem)
    {
        try
        {
            if (selectedItem != null)
            {
                string mode = AppSession.LoginMode;

                lblBSSummaryJobIDX.Text = selectedItem.JobsIDX.ToString();
                lblBSSummaryJobNoHidden.Text = selectedItem.JobNo;
                lblBSSummaryJobNo.Text = "Job No: " + selectedItem.JobNo;

                lblBSSummaryStatus.Text = selectedItem.Status;
                lblBSSummarySvcType.Text = selectedItem.SvcType;
                lblBSSummaryExpType.Text = selectedItem.ExpType;
                lblBSSummaryContentType.Text = selectedItem.ContentType;
                lblBSSummaryPcs.Text = selectedItem.Pieces.ToString();
                lblBSSummaryWeight.Text = selectedItem.Weight.ToString() + " KG";

                lblBSSummaryExpType.Text = selectedItem.ExpType;
                lblBSSummaryContentType.Text = selectedItem.ContentType;

                lblBSSummaryPUCompany.Text = selectedItem.PUCompany;
                lblBSSummaryPUAddress.Text = selectedItem.PUAddress;
                lblBSSummaryPUPostal.Text = selectedItem.PUPOSTALCODE;
                lblBSSummaryPUContact.Text = selectedItem.Sender;
                lblBSSummaryPUTel.Text = selectedItem.PUTel;
                lblBSSummaryPUMobile.Text = selectedItem.PUMobile;
                lblBSSummaryPUInstruction.Text = selectedItem.PUInstruction;
                lblBSSummaryPURdyTime.Text = selectedItem.PURdy;
                lblBSSummaryPULunch.Text = selectedItem.PUAvoid;

                lblBSSummaryDLCompany.Text = selectedItem.DLCompany;
                lblBSSummaryDLAddress.Text = selectedItem.DLAddress;
                lblBSSummaryDLBLOCK.Text = selectedItem.DLBLOCK;
                lblBSSummaryDLSTREET.Text = selectedItem.DLSTREET;
                lblBSSummaryDLUNIT.Text = selectedItem.DLUNIT;
                lblBSSummaryDLBUILDING.Text = selectedItem.DLBUILDING;
                lblBSSummaryDLLocationType.Text = selectedItem.DLLocationType.ToString();

                lblBSSummaryDLPostal.Text = selectedItem.DLPOSTALCODE;
                lblBSSummaryDLContact.Text = selectedItem.Receiver;
                lblBSSummaryDLTel.Text = selectedItem.DLTel;
                lblBSSummaryDLMobile.Text = selectedItem.DLMobile;
                lblBSSummaryDLInstruction.Text = selectedItem.DLInstruction;
                lblBSSummaryDLFrom.Text = selectedItem.DateFrom;
                lblBSSummaryDLBy.Text = selectedItem.DateTo;
                lblBSSummaryDLLunch.Text = selectedItem.DLAvoid;

                if (mode.Equals("r"))
                {
                    btnBSSummaryResch.IsVisible = false;
                    btnBSSummaryRedir.IsVisible = false;
                    bPUL.IsVisible = false;
                    bSvc.IsVisible = false;
                } else if (mode.Equals("s"))
                {
                    logininfo = AppSession.logininfo;
                    bool InProgressStatusCode = (InProgress.Contains("," + selectedItem.StatusCodeIDX.ToString() + ","));
                    btnBSSummaryResch.IsVisible = InProgressStatusCode && (logininfo != null && logininfo.xdelOnlineSettings != null &&
                        ((logininfo.xdelOnlineSettings.Options & XDelServiceRef.OptionsFlag.EnabledChangeSlot) == XDelServiceRef.OptionsFlag.EnabledChangeSlot));
                    btnBSSummaryRedir.IsVisible = InProgressStatusCode && (logininfo != null && logininfo.xdelOnlineSettings != null &&
                        ((logininfo.xdelOnlineSettings.Options & XDelServiceRef.OptionsFlag.EnableChangeAddress) == XDelServiceRef.OptionsFlag.EnableChangeAddress));
                }
            }
        }
        catch (Exception ex)
        {
            string s = ex.Message;
        }
    }

    async void btnBSSummaryResch_Clicked(object sender, EventArgs e)
    {
        try
        {
            if (BindingContext != null && BindingContext is ManageJobPageVM vm)
            {
                vm.changeSetting = null;
                vm.txtPostalTextChangedSubscribed = false;
            }

            await setReschDetail();
        }
        catch (Exception ex)
        {
            string s = ex.Message;
        }
    }

    async void btnBSSummaryRedir_Clicked(object sender, EventArgs e)
    {
        try
        {
            if (BindingContext != null && BindingContext is ManageJobPageVM vm)
            {
                vm.changeSetting = null;
                vm.txtPostalTextChangedSubscribed = false;
            }

            await setRedirDetail();
        }
        catch (Exception ex)
        {
            string s = ex.Message;
        }
    }

    #endregion

    #region RESCHEDULE

    XDelServiceRef.XDelOnlineSettings xos;
    XDelServiceRef.DeliveryTimeWindow[]? dtw;

    private string getAddressString(XDelServiceRef.AddressStructure DL)
    {
        string ToAdd, DLBLOCK, DLSTREET, DLUNIT, DLBUILDING, DLPOSTALCODE, DLCompany;
        try
        {
            ToAdd = "";
            DLBLOCK = "";
            ToAdd = "";
            ToAdd = "";
            ToAdd = "";
            ToAdd = "";
            ToAdd = "";

            if (DL != null && !String.IsNullOrEmpty(DL.BLOCK))
                ToAdd = DL.BLOCK;
            if (DL != null && !String.IsNullOrEmpty(DL.STREET))
                ToAdd += (!String.IsNullOrEmpty(ToAdd) ? ", " : "") + DL.STREET;
            if (DL != null && !String.IsNullOrEmpty(DL.UNIT))
                ToAdd += (!String.IsNullOrEmpty(ToAdd) ? ", " : "") + DL.UNIT;
            if (DL != null && !String.IsNullOrEmpty(DL.BUILDING))
                ToAdd += (!String.IsNullOrEmpty(ToAdd) ? ", " : "") + DL.BUILDING;
            return ToAdd;
        } catch (Exception e)
        {
            string s = e.Message;
            return "";
        }
    }

    async Task setReschDetail()
    {
        string JobsIDX, JobNo, Receiver, DLCompany, DLAddress, DLPostal, DLTel, DLMobile, DLFrom, DLBy;
        Int64 aJobsIDX = 0;
        DateTime aFromDateTime;
        ChangeSetting c = null;
        XDelServiceRef.AddressStructure ads = null;
        XDelServiceRef.ContactStructure cts = null;
        string addr = "";
        try
        {
            if (vm != null && vm.changeSetting != null)
                c = vm.changeSetting;
            if (c != null)
                ads = c.Address;
            if (ads != null && ads.Contacts != null && ads.Contacts[0] != null)
                cts = ads.Contacts[0];
            if (ads != null)
                addr = getAddressString(ads);

            JobsIDX = lblBSSummaryJobIDX.Text;
            aJobsIDX = !string.IsNullOrEmpty(JobsIDX) ? Int64.Parse(JobsIDX) : 0;
            JobNo = lblBSSummaryJobNoHidden.Text;

            Receiver = cts != null && !string.IsNullOrEmpty(cts.NAME) ? cts.NAME : lblBSSummaryDLContact.Text;
            DLCompany = ads != null && !string.IsNullOrEmpty(ads.COMPANY) ? ads.COMPANY : lblBSSummaryDLCompany.Text;
            DLAddress = !string.IsNullOrEmpty(addr) ? addr : lblBSSummaryDLAddress.Text;
            DLPostal = ads != null && !string.IsNullOrEmpty(ads.POSTALCODE) ? ads.POSTALCODE : lblBSSummaryDLPostal.Text;
            DLTel = cts != null && !string.IsNullOrEmpty(cts.TEL) ? cts.TEL : lblBSSummaryDLTel.Text;
            DLMobile = cts != null && !string.IsNullOrEmpty(cts.MOBILE) ? cts.MOBILE : lblBSSummaryDLMobile.Text;
            DLFrom = lblBSSummaryDLFrom.Text;
            DLBy = lblBSSummaryDLBy.Text;

            if (logininfo != null && logininfo.clientInfo != null && !string.IsNullOrEmpty(logininfo.clientInfo.Web_UID) &&
                logininfo.xdelOnlineSettings != null && logininfo.xdelOnlineSettings.AvailableTimeWindows != null)
            {
                aFromDateTime = !string.IsNullOrEmpty(DLFrom) ? DateTime.ParseExact(DLFrom, "dd/MM/yyyy h:mm tt",
                                  System.Globalization.CultureInfo.InvariantCulture) : DateTime.MinValue;

                dtw = logininfo != null && logininfo.xdelOnlineSettings != null && logininfo.xdelOnlineSettings.AvailableTimeWindows != null ? logininfo.xdelOnlineSettings.AvailableTimeWindows : null;
                if (dtw != null && dtw.Length > 0 && aFromDateTime != DateTime.MinValue && BindingContext is ManageJobPageVM vm)
                {
                    vm.newReschSelectors();
                    XDelServiceRef.DeliveryTimeWindow tw;
                    DateTime aaFromDateTime;
                    DateTime aaToDateTime;
                    DateTime EditedaFromDateTime;
                    DateTime EditedaToDateTime;
                    DateTime EarliestDeliveryDateTime;
                    DateTime nw = DateTime.Now;
                    bool jFromDateTimeIsFuture = !(nw.CompareTo(aFromDateTime) > 0);
                    DateTime new_jFromDateTime = nw.CompareTo(aFromDateTime) > 0 ? nw : aFromDateTime;
                    Holiday holiday = new Holiday();
                    ClientInfo clientinfo = null;

                    clientinfo = logininfo.clientInfo;
                    XDelServiceRef.DateArray HolRes = await xs.Get_HolidaysAsync(clientinfo.Web_UID);
                    if (HolRes != null && HolRes.Status == 0 && HolRes.Holidays != null && HolRes.Holidays.Length > 0)
                        holiday.Hols = HolRes.Holidays;

                    if (jFromDateTimeIsFuture && isMoreThanOneDay(aFromDateTime))
                        EarliestDeliveryDateTime = aFromDateTime;
                    else
                    {
                        EarliestDeliveryDateTime = new_jFromDateTime.Date.Equals(DateTime.Now.Date)
                                                       ? (new_jFromDateTime.TimeOfDay.CompareTo(XDelSys.JobInfo.Def1100Time) <=
                                                          0
                                                              ? new_jFromDateTime.Date.Add(XDelSys.JobInfo.Def1800Time)
                                                              : new_jFromDateTime.TimeOfDay.CompareTo(
                                                                  XDelSys.JobInfo.Def1600Time) <=
                                                                0
                                                                    ? new_jFromDateTime.Date.AddDays(1)
                                                                                       .Add(XDelSys.JobInfo.DefStartTime)
                                                                    : new_jFromDateTime.Date.AddDays(1)
                                                                                       .Add(XDelSys.JobInfo.Def1300Time))
                                                       : (new_jFromDateTime.TimeOfDay.CompareTo(XDelSys.JobInfo.Def1100Time) <=
                                                          0
                                                              ? new_jFromDateTime.Date.Add(XDelSys.JobInfo.Def1300Time)
                                                              : new_jFromDateTime.TimeOfDay.CompareTo(
                                                                  XDelSys.JobInfo.Def1600Time) <=
                                                                0
                                                                    ? new_jFromDateTime.Date.Add(XDelSys.JobInfo.DefAOHStart)
                                                                    : new_jFromDateTime.Date.AddDays(1)
                                                                                       .Add(XDelSys.JobInfo.Def1300Time));

                    }

                    bool is6daywk = clientinfo != null && XDelSys.ClientFlags.HasFlag(clientinfo.Flag, ClientFlags.Flag.SixDayWeek);
                    bool is7daywk = clientinfo != null && XDelSys.ClientFlags.HasFlag(clientinfo.Flag, ClientFlags.Flag.SevenDayWeek);
                    bool is5daywk = !is6daywk && !is7daywk;
                    bool isholi = false;
                    System.TimeSpan ets = System.TimeSpan.Parse(EarliestDeliveryDateTime.ToString("HH:mm"));
                    String ddmmyyyy = "";
                    System.TimeSpan fts;
                    System.TimeSpan tts;
                    DateTime fDatet;
                    DateTime tDatet;
                    String dttw = "";

                    if (dtw != null && dtw.Length > 0)
                    {
                        for (int v = 0; v <= 6; v++)
                        {
                            ets = System.TimeSpan.Parse(EarliestDeliveryDateTime.ToString("HH:mm"));

                            isholi = (is6daywk &&
                                      (EarliestDeliveryDateTime.DayOfWeek == DayOfWeek.Sunday ||
                                       holiday.vIsHoliday(EarliestDeliveryDateTime))) ||
                                     (is5daywk &&
                                      (EarliestDeliveryDateTime.DayOfWeek == DayOfWeek.Sunday ||
                                       EarliestDeliveryDateTime.DayOfWeek == DayOfWeek.Saturday ||
                                       holiday.vIsHoliday(EarliestDeliveryDateTime)));

                            if (!isholi)
                            {

                                for (int i = 0; i <= dtw.Length - 1; i++)
                                {
                                    tw = dtw[i];
                                    aaFromDateTime = tw.FromTime;
                                    aaToDateTime = tw.ToTime;
                                    EditedaFromDateTime = EarliestDeliveryDateTime;
                                    EditedaToDateTime = EarliestDeliveryDateTime;
                                    EditedaFromDateTime = EditedaFromDateTime.Add(-ets);
                                    EditedaToDateTime = EditedaToDateTime.Add(-ets);
                                    fts = System.TimeSpan.Parse(aaFromDateTime.ToString("HH:mm"));
                                    tts = System.TimeSpan.Parse(aaToDateTime.ToString("HH:mm"));
                                    EditedaFromDateTime = EditedaFromDateTime.Add(fts);
                                    EditedaToDateTime = EditedaToDateTime.Add(tts);

                                    if (EditedaFromDateTime >= EarliestDeliveryDateTime)
                                    {
                                        ddmmyyyy = EditedaFromDateTime.ToString("dd/MM/yyyy");
                                        fDatet = EditedaFromDateTime.Date.Add(fts);
                                        tDatet = EditedaFromDateTime.Date.Add(tts);
                                        dttw = ddmmyyyy + " " + fDatet.ToString("h:mm tt") + " To " + tDatet.ToString("h:mm tt");
                                        vm.addReschSelectors(new ManageJobReschSelector
                                        {
                                            dttw = dttw,
                                            FromDateTime = EditedaFromDateTime,
                                            ToDateTime = EditedaToDateTime,
                                            JobsIDX = aJobsIDX
                                        });
                                    }
                                }
                            }
                            else
                                v--;

                            EarliestDeliveryDateTime = EarliestDeliveryDateTime.AddDays(1);
                            ets = System.TimeSpan.Parse(EarliestDeliveryDateTime.ToString("HH:mm"));
                            EarliestDeliveryDateTime = EarliestDeliveryDateTime.Add(-ets);
                            EarliestDeliveryDateTime = EarliestDeliveryDateTime.Add(new TimeSpan(9, 0, 0));
                        }
                        vm.SortReschSelectors();
                        cvReschSelectors.ItemsSource = vm.ReschSelectors;

                        if (vm.ReschSelectors.Count > 0)
                        {
                            lblBSReschJobIDX.Text = JobsIDX;
                            lblBSReschJobNoHidden.Text = JobNo;
                            lblBSReschJobNo.Text = JobNo;

                            lblBSReschDLCompany.Text = DLCompany;
                            lblBSReschDLAdd.Text = DLAddress;
                            lblBSReschDLPostal.Text = DLPostal;
                            lblBSReschRecevier.Text = Receiver;
                            lblBSReschFrom.Text = DLFrom;
                            lblBSReschBy.Text = DLBy;

                            await svBSResch.ScrollToAsync(0, 0, false);
                            await Task.Delay(100);
                            await BSResch.OpenBottomSheet(false);
                            BSResch.IsVisible = true;
                            BSResch.isShowing = true;
                        }
                        else
                        {
                            await DisplayAlertAsync("", "There is no available Time Window to reschedule.", "OK");
                        }
                    }
                }
            }
        } catch (Exception e)
        {
            string s = e.Message;
        }
    }

    private bool isMoreThanOneDay(DateTime jFromDateTime)
    {
        bool asd = false;
        try
        {
            asd = (jFromDateTime - DateTime.Now).TotalDays > 1;
        }
        catch (Exception ex)
        {
            String s = ex.Message;
        }
        return asd;
    }


    async void CloseBSResch_Clicked(object sender, EventArgs e)
    {
        try
        {
            if (BindingContext != null && BindingContext is ManageJobPageVM vm)
                vm.changeSetting = null;
            await BSResch.CloseBottomSheet();
            BSResch.IsVisible = false;
            BSResch.isShowing = false;
        }
        catch (Exception ex)
        {
            string s = ex.Message;
        }
    }

    private async void cvcvReschSelectorsOnItemTap(object sender, EventArgs e)
    {
        try
        {
            if (NetworkHelper.IsDisconnected())
            {
                await DisplayAlertAsync("No Internet Connection", "Please check your internet connection and try again.", "OK");
                return;
            }

            var tappedGrid = sender as VerticalStackLayout;
            if (tappedGrid != null)
            {
                var selectedItem = tappedGrid.BindingContext as ManageJobReschSelector;
                if(selectedItem != null)
                    await confirmResch(selectedItem);
            }
        }
        catch (Exception ex)
        {
            string s = ex.Message;
        }
    }

    private async Task confirmResch(ManageJobReschSelector selectedItem)
    {
        try
        {
            var currentPage = App.Current?.Windows.FirstOrDefault()?.Page;

            if (currentPage is null)
                return; // or handle gracefully if app not ready

            if (selectedItem != null)
            {
                bool answer = await currentPage.DisplayAlertAsync(
            "Confirmation",
            "Confirm to reschedule to " + selectedItem.dttw + "?",
            "OK",
            "Cancel");

                if (answer)
                {
                    // User clicked OK
                    await resch(selectedItem);
                }
                else
                {
                    // User clicked Cancel — do nothing or close dialog
                }
            }

        } catch (Exception e)
        {
            string s = e.Message;
        }
    }

    private async Task resch(ManageJobReschSelector selectedItem)
    {
        try
        {
            if (logininfo != null && logininfo.clientInfo != null && !String.IsNullOrEmpty(logininfo.clientInfo.Web_UID) && selectedItem != null)
            {
                await showProgress_Dialog("Processing...");

                XDelServiceRef.ChangeSetting? c = null;

                if (BindingContext != null && BindingContext is ManageJobPageVM vm)
                    c = vm.changeSetting != null ? vm.changeSetting : new XDelServiceRef.ChangeSetting();
                else
                    c = new XDelServiceRef.ChangeSetting();

                DateTime fdt = selectedItem.FromDateTime;
                DateTime tdt = selectedItem.ToDateTime;
                c.FromDateTime = fdt;
                c.ToDateTime = tdt;
                c.ChangeTimeSlot = true;

                XWSBase xb = await Task.Run(async () =>
                {
                    return await xs.ChangeJobScheduleAsync(logininfo.clientInfo.Web_UID, selectedItem.JobsIDX, c);
                });

                await Task.Delay(300);

                await closeProgress_dialog();

                await AlertService.ShowError((xb == null) ? "Error" : "",
                    (xb == null) && !c.ChangeAddress ? "Unable to update Delivery Timing." :
                    (xb == null) && c.ChangeAddress ? "Unable to update Delivery Timing and Address Information." :
                    (xb != null && xb.Status == 0) && !c.ChangeAddress ? "Delivery Timing updated successfully." :
                    (xb != null && xb.Status == 0) && c.ChangeAddress ? "Delivery Timing and Address Information updated successfully." :
                    "Unable to update Delivery Timing."
                    );
                CloseBSResch_Clicked(null, null);
                CloseBSSummary_Clicked(null, null);
            } else
            {
                await AlertService.ShowError("Error", "Unable to update Delivery Timing.");
                CloseBSResch_Clicked(null, null);
                CloseBSSummary_Clicked(null, null);
            }
        } catch (Exception e)
        {
            await closeProgress_dialog();
            string s = e.Message;
        }
    }


    #endregion

    #region REDIRECT

    async Task setRedirDetail()
    {
        string JobsIDX, JobNo, Receiver, DLAddress, DLBLOCK, DLSTREET, DLUNIT, DLBUILDING, DLLocationTypeStr, DLPOSTALCODE, DLCompany, DLInstruction, DLTel, DLMobile;
        int SelectedLocationType = 0;
        Int64 aJobsIDX = 0;
        try
        {
            await loadLocationType();

            JobsIDX = lblBSSummaryJobIDX.Text;
            aJobsIDX = !string.IsNullOrEmpty(JobsIDX) ? Int64.Parse(JobsIDX) : 0;
            JobNo = lblBSSummaryJobNoHidden.Text;

            DLBLOCK = lblBSSummaryDLBLOCK.Text;
            DLSTREET = lblBSSummaryDLSTREET.Text;
            DLUNIT = lblBSSummaryDLUNIT.Text;
            DLBUILDING = lblBSSummaryDLBUILDING.Text;
            DLLocationTypeStr = lblBSSummaryDLLocationType.Text;
            DLPOSTALCODE = lblBSSummaryDLPostal.Text;
            DLCompany = lblBSSummaryDLCompany.Text;
            DLInstruction = lblBSSummaryDLInstruction.Text;

            SelectedLocationType = !string.IsNullOrEmpty(DLLocationTypeStr) ? int.Parse(DLLocationTypeStr) : 0;

            Receiver = lblBSSummaryDLContact.Text;
            DLTel = lblBSSummaryDLTel.Text;
            DLMobile = lblBSSummaryDLMobile.Text;



            lblBSRedirJobIDX.Text = JobsIDX;
            lblBSRedirJobNoHidden.Text = JobNo;
            lblBSRedirJobNo.Text = JobNo;

            txtPostal.Text = DLPOSTALCODE;
            txtBlock.Text = DLBLOCK;
            txtUnit.Text = DLUNIT;
            txtStreet.Text = DLSTREET;
            txtBldg.Text = DLBUILDING;
            txtCompany.Text = DLCompany;
            txtInstruction.Text = DLInstruction;

            lblSelectedLocationType.Text = DLLocationTypeStr;
            if ((cvLocationType.ItemsSource is List<LocationTypeDisp> items))
            {
                LocationTypeDisp item;
                for (int i = 0; i <= items.Count - 1; i++)
                {
                    item = items[i];
                    if (item.value.Equals(SelectedLocationType))
                    {
                        cvLocationType.SelectedItem = items[i];
                        break;
                    }
                }
            }

            txtContactName.Text = Receiver;
            txtTel.Text = DLTel;
            txtHp.Text = DLMobile;

            txtPostal.IsReadOnly = true;
            txtBlock.IsReadOnly = true;
            txtStreet.IsReadOnly = true;

            lblswNewContact.IsVisible = true;
            lblswNewContact.Text = "Activate to use New Contact";
            swNewContact.IsVisible = true;
            swNewContact.Toggled -= swNewContactonToggle;
            swNewContact.IsToggled = false;
            swNewContact.Toggled += swNewContactonToggle;

            await svBSRedir.ScrollToAsync(0, 0, false);
            await Task.Delay(100);
            await BSRedir.OpenBottomSheet(false);
            BSRedir.IsVisible = true;
            BSRedir.isShowing = true;
        } catch (Exception e)
        {
            string s = e.Message;
        }
    }

    private async Task closeBSRedir(bool nullchangesetting = false)
    {
        try
        {
            if (BindingContext != null && BindingContext is ManageJobPageVM vm)
            {
                if (nullchangesetting)
                    vm.changeSetting = null;
                vm.txtPostalTextChangedSubscribed = false;
            }

            txtPostalTextChangedUnSubscribed();
            swNewAddress.Toggled -= swNewAddressonToggle;
            swNewAddress.IsToggled = false;
            swNewAddress.Toggled += swNewAddressonToggle;
            swNewContact.Toggled -= swNewContactonToggle;
            swNewContact.IsToggled = false;
            swNewContact.Toggled += swNewContactonToggle;

            await BSRedir.CloseBottomSheet();
            BSRedir.IsVisible = false;
            BSRedir.isShowing = false;
        } catch (Exception e)
        {
            string s = e.Message;
        }
    }

    async void CloseBSRedir_Clicked(object sender, EventArgs e)
    {
        try
        {
            await closeBSRedir(true);
        }
        catch (Exception ex)
        {
            string s = ex.Message;
        }
    }

    async void CloseBSRedirSave_Clicked(object sender, EventArgs e)
    {
        try
        {
            if (NetworkHelper.IsDisconnected())
            {
                await DisplayAlertAsync("No Internet Connection", "Please check your internet connection and try again.", "OK");
                return;
            }

            await confirmRedir();
        }
        catch (Exception ex)
        {
            string s = ex.Message;
        }
    }

    private async Task loadLocationType()
    {
        try
        {
            await Task.Yield(); // minimal async yield to avoid warning

            List<LocationTypeDisp> list = new List<LocationTypeDisp>
            {
                new LocationTypeDisp(1, "RESIDENTIAL"),
                new LocationTypeDisp(2, "OFFICE")
            };

            cvLocationType.ItemsSource = null;
            cvLocationType.ItemsSource = list;
        }
        catch (Exception ex)
        {
            string s = ex.Message;
        }
    }

    private void cvLocationTypeOnItemSelected(object sender, SelectionChangedEventArgs e)
    {
        try
        {
            var selectedItem = e.CurrentSelection.FirstOrDefault() as LocationTypeDisp;
            if (selectedItem != null)
            {
                lblSelectedLocationType.Text = selectedItem.value.ToString();

            }
        }
        catch (Exception ex)
        {
            string s = ex.Message;
        }
    }

    private async void swNewAddressonToggle(object? sender, ToggledEventArgs e)
    {
        try
        {
            if (e.Value)
            {
                txtPostalTextChangedUnSubscribed();
                await loadLocationType();
                lblSelectedLocationType.Text = "";

                txtPostal.Text = "";
                txtBlock.Text = "";
                txtUnit.Text = "";
                txtStreet.Text = "";
                txtBldg.Text = "";
                txtCompany.Text = "";

                txtContactName.Text = "";
                txtTel.Text = "";
                txtHp.Text = "";

                txtPostal.IsReadOnly = false;
                txtBlock.IsReadOnly = false;
                txtStreet.IsReadOnly = false;
                lblswNewContact.IsVisible = false;
                lblswNewContact.Text = "Activate to use New Contact";
                swNewContact.IsVisible = false;
                swNewContact.Toggled -= swNewContactonToggle;
                swNewContact.IsToggled = false;
                swNewContact.Toggled += swNewContactonToggle;
                txtPostalTextChangedSubscribed();
            }
            else
            {
                txtPostalTextChangedUnSubscribed();
                await setRedirDetail();
            }
        }
        catch (Exception ex)
        {
            string s = ex.Message;
        }
    }

    private async void swNewContactonToggle(object? sender, ToggledEventArgs e)
    {
        try
        {
            if (e.Value)
            {
                txtContactName.Text = "";
                txtTel.Text = "";
                txtHp.Text = "";
                lblswNewContact.Text = "Deactivate to use Original Contact";
            }
            else
            {
                await setRedirDetail();
            }
        } catch (Exception ex)
        {
            string s = ex.Message;
        }
    }

    private async Task confirmRedir()
    {
        try
        {
            string JobsIDX = lblBSRedirJobIDX.Text;
            long aJobsIDX = !string.IsNullOrEmpty(JobsIDX) ? long.Parse(JobsIDX) : 0;
            if (aJobsIDX == 0)
                await AlertService.ShowError("Error", "Error updating Delivery Address Info.");
            else if (logininfo != null && logininfo.clientInfo != null && !String.IsNullOrEmpty(logininfo.clientInfo.Web_UID)
                            && BindingContext != null && BindingContext is ManageJobPageVM vm)
            {
                var currentPage = App.Current?.Windows.FirstOrDefault()?.Page;

                if (currentPage is null)
                    return; // or handle gracefully if app not ready

                int hc_value = hasChanges();
                string msg = hc_value == 0 ? "No changes found.\nDelivery Address Info not updated." :
                    hc_value == 1 ? "Confirm to update Delivery Address Info" :
                    "There is a change in POSTAL CODE.\nYou will need to reschedule the Delivery Time Window for this job.\n\nNew delivery address will be saved together with the new selected Delivery Time Window.";

                if (hc_value == 0)
                    await AlertService.ShowError("Confirmation", msg);
                else
                {
                    bool answer = await currentPage.DisplayAlertAsync("Confirmation", msg, "OK", "Cancel");

                    if (answer)
                    {
                        if (logininfo != null && logininfo.clientInfo != null && !String.IsNullOrEmpty(logininfo.clientInfo.Web_UID)
                            && BindingContext != null && BindingContext is ManageJobPageVM vvm)
                        {
                            vvm.changeSetting = null;
                            ChangeSetting c = prepChangeSetting();
                            if (c == null)
                                await AlertService.ShowError("Error", "Error updating Delivery Address Info.");
                            else
                            {
                                vvm.changeSetting = c;
                                if (hc_value == 1)
                                {
                                    await redir(logininfo.clientInfo.Web_UID, aJobsIDX, vvm);
                                }
                                else
                                {
                                    await Task.Delay(200);
                                    await closeBSRedir(false);
                                    await Task.Delay(200);
                                    await setReschDetail();
                                }
                            }
                        }
                        else
                        {
                            await AlertService.ShowError("Error", "Error updating Delivery Address Info.");
                        }
                    }
                    else
                    {
                        // User clicked Cancel — do nothing or close dialog
                    }
                }

            }
            else
            {
                await AlertService.ShowError("Session expired", "Session expired.\nPlease Login again.");
            }
        }
        catch (Exception e)
        {
            string s = e.Message;
        }
    }

    private async Task redir(string Web_UID, long JobsIDX, ManageJobPageVM vm)
    {
        try
        {
            await showProgress_Dialog("Processing...");

            XDelServiceRef.XWSBase xb = await Task.Run(async () =>
            {
                return await xs.ChangeJobScheduleAsync(Web_UID, JobsIDX, vm.changeSetting);
            });

            await Task.Delay(500);

            await closeProgress_dialog();

            await AlertService.ShowError((xb == null) ? "Error" : "",
                (xb == null) ? "Error updating Delivery Address Info." :
                (xb != null && xb.Status == 0) ? "Delivery Address information updated successfully." :
                "Error updating Delivery Address Info."
                );
            //CloseBSRedir_Clicked(null, null);
            await closeBSRedir(true);
            CloseBSSummary_Clicked(null, null);
        } catch (Exception e)
        {
            string s = e.Message;
        }
    }

    private int hasChanges()
    {
        string Receiver, DLAddress, DLBLOCK, DLSTREET, DLUNIT, DLBUILDING, DLLocationTypeStr, DLPOSTALCODE, DLCompany, DLInstruction, DLTel, DLMobile;
        string nReceiver, nDLAddress, nDLBLOCK, nDLSTREET, nDLUNIT, nDLBUILDING, nDLLocationTypeStr, nDLPOSTALCODE, nDLCompany, nDLInstruction, nDLTel, nDLMobile;
        try
        {
            DLBLOCK = lblBSSummaryDLBLOCK.Text;
            DLSTREET = lblBSSummaryDLSTREET.Text;
            DLUNIT = lblBSSummaryDLUNIT.Text;
            DLBUILDING = lblBSSummaryDLBUILDING.Text;
            DLLocationTypeStr = lblBSSummaryDLLocationType.Text;
            DLPOSTALCODE = lblBSSummaryDLPostal.Text;
            DLCompany = lblBSSummaryDLCompany.Text;
            DLInstruction = lblBSSummaryDLInstruction.Text;
            Receiver = lblBSSummaryDLContact.Text;
            DLTel = lblBSSummaryDLTel.Text;
            DLMobile = lblBSSummaryDLMobile.Text;

            nDLBLOCK = txtBlock.Text;
            nDLSTREET = txtStreet.Text;
            nDLUNIT = txtUnit.Text;
            nDLBUILDING = txtBldg.Text;
            nDLLocationTypeStr = lblSelectedLocationType.Text;
            nDLPOSTALCODE = txtPostal.Text;
            nDLCompany = txtCompany.Text;
            nDLInstruction = txtInstruction.Text;
            nReceiver = txtContactName.Text;
            nDLTel = txtTel.Text;
            nDLMobile = txtHp.Text;

            if (!string.Equals(nDLPOSTALCODE, DLPOSTALCODE, StringComparison.OrdinalIgnoreCase))
                return 2;
            if (!string.Equals(nDLBLOCK, DLBLOCK, StringComparison.OrdinalIgnoreCase))
                return 1;
            if (!string.Equals(nDLSTREET, DLSTREET, StringComparison.OrdinalIgnoreCase))
                return 1;
            if (!string.Equals(nDLUNIT, DLUNIT, StringComparison.OrdinalIgnoreCase))
                return 1;
            if (!string.Equals(nDLBUILDING, DLBUILDING, StringComparison.OrdinalIgnoreCase))
                return 1;
            if (!string.Equals(nDLLocationTypeStr, DLLocationTypeStr, StringComparison.OrdinalIgnoreCase))
                return 1;
            if (!string.Equals(nDLCompany, DLCompany, StringComparison.OrdinalIgnoreCase))
                return 1;
            if (!string.Equals(nDLInstruction, DLInstruction, StringComparison.OrdinalIgnoreCase))
                return 1;
            if (!string.Equals(nReceiver, Receiver, StringComparison.OrdinalIgnoreCase))
                return 1;
            if (!string.Equals(nDLTel, DLTel, StringComparison.OrdinalIgnoreCase))
                return 1;
            if (!string.Equals(nDLMobile, DLMobile, StringComparison.OrdinalIgnoreCase))
                return 1;

            return 0;
        }
        catch (Exception e)
        {
            string s = e.Message;
            return 0;
        }
    }

    private ChangeSetting prepChangeSetting()
    {
        ChangeSetting c = new ChangeSetting();
        XDelServiceRef.AddressStructure newCompany = null;
        string DLInstruction;
        string nReceiver, nDLAddress, nDLBLOCK, nDLSTREET, nDLUNIT, nDLBUILDING, nDLLocationTypeStr, nDLPOSTALCODE, nDLCompany, nDLInstruction, nDLTel, nDLMobile;
        try
        {
            nDLBLOCK = txtBlock.Text;
            nDLSTREET = txtStreet.Text;
            nDLUNIT = txtUnit.Text;
            nDLBUILDING = txtBldg.Text;
            nDLLocationTypeStr = lblSelectedLocationType.Text;
            nDLPOSTALCODE = txtPostal.Text;
            nDLCompany = txtCompany.Text;
            nReceiver = txtContactName.Text;
            nDLTel = txtTel.Text;
            nDLMobile = txtHp.Text;
            nDLInstruction = txtInstruction.Text;
            DLInstruction = lblBSSummaryDLInstruction.Text;

            c.ChangeAddress = true;
            c.ChangeTimeSlot = false;
            c.ChangeDLSI = !string.Equals(nDLInstruction, DLInstruction, StringComparison.OrdinalIgnoreCase);
            c.DLSI = !String.IsNullOrEmpty(nDLInstruction) ? nDLInstruction.ToUpper() : "";

            XDelServiceRef.ContactStructure defaultContact = new XDelServiceRef.ContactStructure();
            XDelServiceRef.ContactStructure[] contactarray = new XDelServiceRef.ContactStructure[1];
            newCompany = new XDelServiceRef.AddressStructure();
            newCompany = new XDelServiceRef.AddressStructure();
            newCompany.IDX = 0;
            newCompany.CLIENTIDX = 0;
            newCompany.ADDRESSTYPE = 2;
            newCompany.ACTIVE = true;

            newCompany.ACCOUNT = "";
            newCompany.COMPANY = nDLCompany;
            newCompany.BLOCK = nDLBLOCK;
            newCompany.STREET = nDLSTREET;
            newCompany.UNIT = nDLUNIT;
            newCompany.POSTALCODE = nDLPOSTALCODE;
            newCompany.BUILDING = nDLBUILDING;
            if (!string.IsNullOrEmpty(nDLLocationTypeStr))
                newCompany.LocationType = nDLLocationTypeStr.Equals("1") ? XDelServiceRef.Location_Type.Residential : XDelServiceRef.Location_Type.Office;
            else
                newCompany.LocationType = XDelServiceRef.Location_Type.Office;

            defaultContact.NAME = nReceiver;
            defaultContact.TEL = nDLTel;
            defaultContact.MOBILE = nDLMobile;

            defaultContact.ACTIVE = true;
            contactarray[0] = defaultContact;
            newCompany.Contacts = contactarray;
            newCompany.CUSTOMER_NOTES = "";
            c.Address = newCompany;

            return c;
        } catch (Exception e)
        {
            string s = e.Message;
            return null;
        }
    }

    private async Task SearchPostal()
    {
        XDelServiceRef.AddressBook ab = null;
        XDelServiceRef.AddressStructure? ads = null;
        string postal = txtPostal.Text;
        try
        {
            if (NetworkHelper.IsDisconnected())
            {
                await DisplayAlertAsync("No Internet Connection", "Please check your internet connection and try again.", "OK");
                return;
            }

            if (logininfo != null && logininfo.clientInfo != null && !String.IsNullOrEmpty(logininfo.clientInfo.Web_UID))
            {
                await showProgress_Dialog("Processing...");

                ClientInfo ci = logininfo.clientInfo;
                ab = await Task.Run(async () =>
                {
                    return await xs.AddressLookupAsync(ci.Web_UID, postal);
                });


                if (ab != null && ab.Status == 0)
                {
                    ads = ab.AddressList[0];
                }
                if (ads != null)
                {
                    loadDefAddress(ads);
                    await closeProgress_dialog();
                }
            }
        }
        catch (Exception ex)
        {
            await closeProgress_dialog();

            string s = ex.Message;
        }
        finally
        {
            await closeProgress_dialog();
        }
    }

    private void loadDefAddress(XDelServiceRef.AddressStructure defAddress)
    {
        try
        {
            txtPostalTextChangedUnSubscribed();
            XDelServiceRef.ContactStructure? cts;
            cts = defAddress.Contacts != null && defAddress.Contacts.Length > 0 ? defAddress.Contacts[0] : null;

            txtPostal.Text = defAddress.POSTALCODE;
            txtBlock.Text = defAddress.BLOCK;
            txtUnit.Text = defAddress.UNIT;
            txtStreet.Text = defAddress.STREET;
            txtBldg.Text = defAddress.BUILDING;
            txtCompany.Text = defAddress.COMPANY;

            int SelectedLocationType = defAddress.LocationType == XDelServiceRef.Location_Type.Office ? 2 : defAddress.LocationType == XDelServiceRef.Location_Type.Residential ? 1 : 0;
            lblSelectedLocationType.Text = SelectedLocationType.ToString();

            int cvLocationTypeCount = cvLocationType.ItemsSource is List<LocationTypeDisp> collection ? collection.Count : 0;

            cvLocationType.SelectedItem = null;

            if ((cvLocationType.ItemsSource is List<LocationTypeDisp> items))
            {
                LocationTypeDisp item;
                for (int i = 0; i <= items.Count - 1; i++)
                {
                    item = items[i];
                    if (item.value.Equals(SelectedLocationType))
                    {
                        cvLocationType.SelectedItem = items[i];
                        break;
                    }
                }
            }

            txtPostal.IsReadOnly = false;
            txtBlock.IsReadOnly = false;
            txtUnit.IsReadOnly = false;
            txtStreet.IsReadOnly = true;
            txtBldg.IsReadOnly = false;
            txtCompany.IsReadOnly = false;
            txtPostalTextChangedSubscribed();
        }
        catch (Exception e)
        {
            string s = e.Message;
        }
    }

    private void txtPostalTextChangedSubscribed()
    {
        try
        {
            if (vm != null && !vm.txtPostalTextChangedSubscribed)
            {
                vm.txtPostalTextChangedSubscribed = true;
                txtPostal.TextChanged += OnEntryTextChanged;
            }
        }
        catch (Exception e)
        {
            string s = e.Message;
        }
    }

    private void txtPostalTextChangedUnSubscribed()
    {
        try
        {
            if (vm != null)
                vm.txtPostalTextChangedSubscribed = false;
            txtPostal.TextChanged -= OnEntryTextChanged;
        }
        catch (Exception e)
        {
            string s = e.Message;
        }
    }

    private async void OnEntryTextChanged(object sender, TextChangedEventArgs e)
    {
        try
        {
            string oldText = e.OldTextValue;
            string newText = e.NewTextValue;

            if (!string.IsNullOrEmpty(newText) && newText.Length == 6 && !txtPostal.IsReadOnly && swNewAddress.IsToggled)
                await SearchPostal();
            else if (!string.IsNullOrEmpty(oldText) && oldText.Length == 6 && oldText.Length > newText.Length && !txtPostal.IsReadOnly && swNewAddress.IsToggled)
                resetFieldsForPostalChange();
        }
        catch (Exception ex)
        {
            string s = ex.Message;
        }
    }

    private void resetFieldsForPostalChange()
    {
        try
        {
            txtPostalTextChangedUnSubscribed();

            cvLocationType.SelectedItem = null;
            lblSelectedLocationType.Text = "0";

            txtBlock.Text = "";
            txtUnit.Text = "";
            txtStreet.Text = "";
            txtBldg.Text = "";
            txtCompany.Text = "";
            txtContactName.Text = "";
            txtTel.Text = "";
            txtHp.Text = "";

            txtPostal.IsReadOnly = false;
            txtBlock.IsReadOnly = false;
            txtUnit.IsReadOnly = false;
            txtStreet.IsReadOnly = true;
            txtBldg.IsReadOnly = false;
            txtCompany.IsReadOnly = false;

            txtPostalTextChangedSubscribed();
        }
        catch (Exception ex)
        {
            string s = ex.Message;
        }
    }


    #endregion

    #endregion


    #region Gestures

    void addTapGestureRecognizer()
    {
        try
        {
            btnTopNavBack.InputTransparent = false;

            dpFrom.InputTransparent = false;
            dpTo.InputTransparent = false;
            txtRefNo.InputTransparent = false;
            //btnHelp.InputTransparent = false;
            btnSearch.InputTransparent = false;
            btnParSearch.InputTransparent = false;
            btnDTSearch.InputTransparent = false;
            TabScroll.InputTransparent = false;
            TabCarousel.InputTransparent = false;
            TabCarousel.IsSwipeEnabled = true;

            if (vm != null)
            {
                Dispatcher.Dispatch(async () =>
                {
                    await MoveIndicator(vm.SelectedTabIndex);
                    ScrollTabIntoView(vm.SelectedTabIndex);
                });
            }

            // enable==true => allow scroll; enable==false => block scroll
            foreach (var view in TabCarousel.VisibleViews)
            {
                var cv = view.FindByName<CollectionView>("cvJobs");
                if (cv == null) continue;

                // Option A: disable all interactions
                //cv.IsEnabled = true;
                cv.InputTransparent = false;
            }
        }
        catch (Exception ex)
        {
            string s = ex.Message;
        }
    }

    void removeTapGestureRecognizer()
    {
        try
        {
            btnTopNavBack.InputTransparent = true;

            dpFrom.InputTransparent = true;
            dpTo.InputTransparent = true;
            txtRefNo.InputTransparent = true;
            //btnHelp.InputTransparent = true;
            btnSearch.InputTransparent = true;
            btnParSearch.InputTransparent = true;
            btnDTSearch.InputTransparent = true;
            TabScroll.InputTransparent = true;
            TabCarousel.InputTransparent = true;
            TabCarousel.IsSwipeEnabled = false;

            // enable==true => allow scroll; enable==false => block scroll
            foreach (var view in TabCarousel.VisibleViews)
            {
                var cv = view.FindByName<CollectionView>("cvJobs");
                if (cv == null) continue;

                // Option A: disable all interactions
                //cv.IsEnabled = false;
                cv.InputTransparent = true;
            }
        }
        catch (Exception ex)
        {
            string s = ex.Message;
        }
    }

    #endregion

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