using ETHAN.classes;
using ETHAN.Network;
using ETHAN.ProgressDialog;
using ETHAN.ViewModel;
using ETHAN.XDelSys;
using System.Globalization;
using XDelServiceRef;

namespace ETHAN.Views;

[QueryProperty(nameof(Vmm), "vmm")] // Add a QueryProperty to handle the navigation parameter
//[QueryProperty(nameof(LOGININFO), "LOGININFO")] // Add a QueryProperty to handle the navigation parameter
[QueryProperty(nameof(LTR), "LTR")] // Add a QueryProperty to handle the navigation parameter
[QueryProperty(nameof(Source), "Source")]



public partial class JobSummary : ContentPage
{
    //private XWSSoapClient xs = new XWSSoapClient(XWSSoapClient.EndpointConfiguration.XWSSoap);
    private XOEWSSoapClient xs = new XOEWSSoapClient(XOEWSSoapClient.EndpointConfiguration.XOEWSSoap);
    //private ProgressDialogService _progressService;
    private readonly IProgressDialogService _progressService;

    private CreateJobVM vm;
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
                vm = new CreateJobVM();
                BindingContext = vm;
            }
            // value=null AND vm exists = returning via ".." from CreateJob, keep existing vm
        }
    }

    private LoginInfo? logininfo;
    public LoginInfo? LOGININFO
    {
        set
        {
            //logininfo = value;
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

    private string source;
    public string Source
    {
        set
        {
            /*source = value;*/
            if (value != null) source = value;
        }
    }

    private bool _loadedOnce = false;

    public JobSummary(IProgressDialogService progressService)
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

    //protected async override void OnAppearing()
    protected override void OnAppearing()
    {
        base.OnAppearing();
        try
        {
            if (vm != null)
                BindingContext = vm; // refresh UI when returning via ".." from CreateJob

            _ = LoadAsync();
        }
        catch (Exception e)
        {
            string s = e.Message;
        }

        /*try
        {
            if (!_loadedOnce)
            {
                _loadedOnce = true;

                logininfo = AppSession.logininfo;

                if (logininfo == null)
                    return;

                if (logininfo?.clientInfo == null ||
                string.IsNullOrEmpty(logininfo.clientInfo.Web_UID) ||
                logininfo.clientInfo.CAIDX <= 0)
                {
                    await DisplayAlert("Session expired", "Please Login again.", "OK");
                    await common.BackToLogin();
                    return;
                }

                if (vm == null)
                    return;

                if (vm != null && vm.JobsIDX == 0)
                    return;

                hideshowPrepaid();
                await GetJob();

            }
        }
        catch (Exception e)
        {
            string s = e.Message;
        }*/
    }

    bool _isLoading;
    async Task LoadAsync()
    {
        if (_isLoading) return;
        _isLoading = true;

        try
        {
            if (NetworkHelper.IsDisconnected())
            {
                await DisplayAlertAsync("No Internet Connection", "Please check your internet connection and try again.", "OK");
                BackToMangeJobPage();
                return;
            }

            logininfo = AppSession.logininfo;

            if (logininfo == null)
                return;

            if (logininfo?.clientInfo == null ||
            string.IsNullOrEmpty(logininfo.clientInfo.Web_UID) ||
            logininfo.clientInfo.CAIDX <= 0)
            {
                await DisplayAlertAsync("Session expired", "Please Login again.", "OK");
                await common.BackToLogin();
                return;
            }

            if (vm?.JobsIDX <= 0)
                return;

            hideshowPrepaid();
            await GetJob();
        }
        catch (Exception e)
        {
            string s = e.Message;
        }
        finally
        {
            _isLoading = false;
        }
    }

    private void hideshowPrepaid()
    {
        try
        {
            if (logininfo != null && logininfo.clientInfo != null && logininfo.clientInfo.AccountType == TAccountType.atPrePaid)
            {
                lblTotaltxt.IsVisible = true;
                lblTotalValueCurr.IsVisible = true;
                lblTotalValue.IsVisible = true;
                lblwgst.IsVisible = true;
                lblBalancetxt.IsVisible = true;
                lblCurrencytxt.IsVisible = true;
                lblBalance.IsVisible = true;
                lblBalance.Text = logininfo.PrePaidBalance == null ? "$0.00" : "$" + logininfo.PrePaidBalance.Value.ToString("F2");
            }
            else
            {
                lblTotaltxt.IsVisible = false;
                lblTotalValueCurr.IsVisible = false;
                lblTotalValue.IsVisible = false;
                lblwgst.IsVisible = false;
                lblBalancetxt.IsVisible = false;
                lblCurrencytxt.IsVisible = false;
                lblBalance.IsVisible = false;
            }
        } catch (Exception e)
        {
            string s = e.Message;
        }
    }

    async void BackToCreateJob()
    {
        try
        {
            /*await Shell.Current.GoToAsync("CreateJob", new Dictionary<string, object>
                    {
                        { "vmm", vm },
                        { "LTR", ltr },
                        {"LOGININFO",  logininfo}
                    });*/
            await Shell.Current.GoToAsync("CreateJob", new Dictionary<string, object>
                    {
                        { "vmm", vm },
                        { "LTR", ltr }
                    });
        }
        catch (Exception e)
        {
            string s = e.Message;
        }
    }

    async void BackToMangeJobPage()
    {
        try
        {
            /*await Shell.Current.GoToAsync("//ManageJobPage", new Dictionary<string, object>
                        {
                            { "BARCODE", null },
                            { "LTR", ltr },
                            { "vmm", null }
                        });*/

            if (source == "ManageJobPage")
            {
                // Stack: CardShellPage > ManageJobPage > JobSummaryPage
                // Simple pop back
                await Shell.Current.GoToAsync("..", new Dictionary<string, object>
        {
            { "BARCODE", null },
            { "LTR", ltr },
            { "vmm", null }
        });
            }
            else if (source == "CreateJobPage")
            {
                // Stack: CardShellPage > ManageJobPage > JobSummaryPage(1st) > CreateJobPage > JobSummaryPage(current)
                // Manually remove JobSummaryPage(1st) and CreateJobPage from stack
                var stack = Shell.Current.Navigation.NavigationStack.ToList();
                var currentPage = stack.Last();

                foreach (var page in stack)
                {
                    // Skip current page — only remove pages BELOW current
                    if (page == currentPage) continue;

                    if (page is CreateJob || page is JobSummary)
                        Shell.Current.Navigation.RemovePage(page);
                }

                // Then pop current JobSummaryPage back to ManageJobPage
                await Shell.Current.GoToAsync("..", new Dictionary<string, object>
        {
            { "BARCODE", null },
            { "LTR", ltr },
            { "vmm", null }
        });
            }
        }
        catch (Exception e)
        {
            string s = e.Message;
        }
    }

    async void BackToHomePage()
    {
        try
        {
            MainThread.BeginInvokeOnMainThread(async () =>
            {
                BindingContext = null;
                /*await Shell.Current.GoToAsync("///CardShellPage", new Dictionary<string, object>
                        {
                            { "LOGININFO", logininfo },
                            { "BARCODE", null }
                        });*/
                await Shell.Current.GoToAsync("///CardShellPage", new Dictionary<string, object>
                        {
                            { "BARCODE", null }
                        });
            }
                                    );
        }
        catch (Exception e)
        {
            string s = e.Message;
        }
    }

    async void Back(System.Object sender, System.EventArgs e)
    {
        try
        {
            BackToCreateJob();
        }
        catch (Exception ex)
        {
            string s = ex.Message;
        }
    }

    async void Return(System.Object sender, System.EventArgs e)
    {
        try
        {
            BackToMangeJobPage();
        }
        catch (Exception ex)
        {
            string s = ex.Message;
        }
    }

    protected override bool OnBackButtonPressed()
    {
        if (_progressService != null && _progressService.IsShowing)
            return true;

        if (vm != null && vm.FromSummary)
        {
            BackToMangeJobPage();
        } else 
            BackToCreateJob();

        return true;
    }

    async void btnProceed_Clicked(object sender, EventArgs e)
    {
        try
        {
            //vm.JobsIDX = 9360134;
            string message = "";
            logininfo = AppSession.logininfo;
            if (logininfo?.clientInfo == null || string.IsNullOrEmpty(logininfo.clientInfo.Web_UID))
            {
                await DisplayAlertAsync("Session expired", "Please Login again.", "OK");
                await common.BackToLogin();
                return;
            }

            if (vm == null)
            {
                await DisplayAlertAsync("", "Unable to proceed.\r\nPlease try again.", "OK");
                BackToHomePage();
                return;
            }

            if (vm.job1 == null)
            {
                await DisplayAlertAsync("", "Unable to proceed.\r\nPlease try again.", "OK");
                BackToHomePage();
                return;
            }

            if (NetworkHelper.IsDisconnected())
            {
                await DisplayAlertAsync("No Internet Connection", "Please check your internet connection and try again.", "OK");
                return;
            }

            await showProgress_Dialog("Processing...");

            vm.job1.JobNo = "";
            if (vm.job1.ReadyDateTime > DateTime.Now)
                vm.job1.StatusCodeIDX = 165;
            else if (vm.job1.ReadyDateTime == DateTime.Now)
                vm.job1.StatusCodeIDX = 0;

            XDelServiceRef.JobStructure JobStruct = await xs.UpdateJobAsync(logininfo.clientInfo.Web_UID, vm.job1);
            if (JobStruct.Status == 0)
            {
                /*Int64 JobOwnerUserSelectedValue = vm.job1.ContactIDX;
                string msg = "Job submitted.";
                msg += JobOwnerUserSelectedValue > 0 ? "\nOwner ContactIDX: " + JobOwnerUserSelectedValue.ToString() : "";
                msg += "\n\n" + common.JSONSerialize(JobStruct);
                XDelSys.XWS.JobInfo.WriteJobLog(
                    (long)jobstruct.JobList[0].JobsIDX, XDelSys.HOMES.Common.C_LogEntry, -1, 
                    XDelSys.Constants.RobotIDX, Session["ContactIDX"] != null ? Int64.Parse((String)Session["ContactIDX"]) : -1, 
                    msg, Common.C_XDelOnline);*/

                XDelServiceRef.XDelOnlineSettings? cxo = logininfo.ClientXDelOnlineSettings;
                XDelServiceRef.XDelOnlineSettings? xo = logininfo.xdelOnlineSettings;

                if (vm.job2 != null)
                {
                    vm.job2.JobNo = "";
                    if (vm.job2.ReadyDateTime > DateTime.Now)
                        vm.job2.StatusCodeIDX = 165;
                    else if (vm.job2.ReadyDateTime == DateTime.Now)
                        vm.job2.StatusCodeIDX = 0;

                    XDelServiceRef.JobStructure JobStruct2 = await xs.UpdateJobAsync(logininfo.clientInfo.Web_UID, vm.job2);
                    if (JobStruct2.Status == 0)
                    {
                        Int64[] jobsIDXarr = new Int64[] { (long)JobStruct.JobList[0].JobsIDX, (long)JobStruct2.JobList[0].JobsIDX };
                        await xs.UpdateCompletedAsync(logininfo.clientInfo.Web_UID, jobsIDXarr);

                        if ((xo != null && xo.GraceMinutes > 0) || (cxo != null && cxo.GraceMinutes > 0))
                        {
                            message = "Thank you for using XDel.\nAll linked jobs have been submitted successfully.\n\nPlease drop off your shipment to your mailroom before " 
                                + vm.job1.ReadyDateTime.ToString("dd/MM/yyyy hh:mm tt") + " Hr.";
                        }
                        else
                        {
                            message = "Thank you for using XDel.\nAll linked jobs have been submitted successfully.";
                        }

                        await closeProgress_dialog();
                        //await UiPump.Yield();
                        await ShowAlertSafe("", message);
                        await UiPump.Yield();
                        BackToHomePage();
                    }
                } else
                {
                    if ((xo != null && xo.GraceMinutes > 0) || (cxo != null && cxo.GraceMinutes > 0))
                    {
                        message = "Thank You for using XDel.\nYour Job, " + 
                            JobStruct.JobList[0].JobNo.ToString() + " has been submitted successfully.\n\nPlease drop off your shipment to your mailroom before " 
                            + JobStruct.JobList[0].ReadyDateTime.ToString("dd/MM/yyyy hh:mm tt") + " Hr.";
                    }
                    else
                    {
                        message = "Thank You for using XDel.\nYour Job, " + 
                            JobStruct.JobList[0].JobNo.ToString() + " has been submitted successfully.";
                    }

                    await closeProgress_dialog();
                    //await UiPump.Yield();
                    await ShowAlertSafe("", message);
                    await UiPump.Yield();
                    BackToHomePage();
                }

            }
        }
        catch (Exception ex)
        {
            string s = ex.Message;
        }
    }


    private async Task GetJob()
    {
        XDelServiceRef.JobStructure JobStruct = new XDelServiceRef.JobStructure();
        XDelServiceRef.JobInfo job = new XDelServiceRef.JobInfo();
        XDelServiceRef.JobInfo? RLjob = null;
        XDelServiceRef.AddressStructure? PL;
        XDelServiceRef.AddressStructure? DL;
        XDelServiceRef.AddressStructure? RL;
        XDelServiceRef.AddressStructure? PL2;
        XDelServiceRef.AddressStructure? DL2;
        double TotalPayableAmt = 0.0;

        string FromAdd = "", PUBLOCK = "", PUSTREET = "", PUUNIT = "", PUBUILDING = "", PUPOSTALCODE = "", PURdy = "", 
            PUCompany = "", Sender = "", PUTel = "", PUMobile = "", PUInstruction = "", PUAvoid = "",
            ToAdd = "", DLBLOCK = "", DLSTREET = "", DLUNIT = "", DLBUILDING = "", DLPOSTALCODE = "", 
            DLCompany = "", Receiver = "", DLTel = "", DLMobile = "", DLInstruction = "", DLAvoid = "",
            RLPURdy = "", RLPUInstruction = "", RLPUAvoid = "",
            RLAdd = "", RLBLOCK = "", RLSTREET = "", RLUNIT = "", RLBUILDING = "", RLPOSTALCODE = "",
            RLCompany = "", RLReceiver = "", RLTel = "", RLMobile = "", RLInstruction = "", RLAvoid = "",
            DateFrom = "", DateTo = "", DateTo2 = "", RLDateFrom = "", RLDateTo = "", RLDateTo2 = "";

        bool hasAltDelWin;
        bool hasAltDelWin2;
        decimal? COD = 0;
        decimal? RLCOD = 0;
        int DLLocationType = 0;
        int RLLocationType = 0;

        try
        {
            await showProgress_Dialog("Processing...");
            //await Task.Delay(200);

            JobStruct = await xs.GetJobAsync(logininfo!.clientInfo.Web_UID, vm.JobsIDX);
            
            if (JobStruct != null && JobStruct.Status == 0)
            {
                job = JobStruct.JobList[0];
                PL = job.PURedirectedLocation != null ? job.PURedirectedLocation : job.PULocation;
                DL = job.DLRedirectedLocation != null ? job.DLRedirectedLocation : job.DLLocation;

                RLjob = job.LinkedJobs != null && job.LinkedJobs.Length > 0 && job.LinkedJobs[0] != null ? job.LinkedJobs[0] : null;

                RL = RLjob == null ? null : RLjob.DLRedirectedLocation != null ? RLjob.DLRedirectedLocation : job.LinkedJobs![0].DLLocation;

                if (job != null)
                {
                    COD = job != null && job.COD == null ? 0 : job.COD;
                    hasAltDelWin = (job.ExtFromDateTime != DateTime.MinValue) &&
                           (job.ExtDateTime != DateTime.MinValue) && job.ExpressType != eExpressType.etNormal;

                    DateFrom = job.FromDateTime.ToString("dd/MM/yyyy h:mm tt", CultureInfo.InvariantCulture).ToUpper();
                    /*DateTo = job.ExpressType == eExpressType.etNormal && !hasAltDelWin ?
                        job.ToDateTime.ToString("dd/MM/yyyy h:mm tt").ToUpper() + " else, latest by " + job.ExtDateTime.ToString("dd/MM/yyyy h:mm tt").ToUpper() :

                        job.ExpressType != eExpressType.etNormal && hasAltDelWin ?
                        job.ToDateTime.ToString("dd/MM/yyyy h:mm tt").ToUpper().ToUpper() + "\r\nALTERNATIVELY\r\n" + 
                        job.ExtFromDateTime.ToString("dd/MM/yyyy h:mm tt").ToUpper() + " to " + job.ExtDateTime.ToString("dd/MM/yyyy h:mm tt").ToUpper() :
                        job.ToDateTime.ToString("dd/MM/yyyy h:mm tt").ToUpper();*/

                    if (job.ExpressType == eExpressType.etNormal && !hasAltDelWin)
                    {
                        DateTo = job.ToDateTime.ToString("dd/MM/yyyy h:mm tt", CultureInfo.InvariantCulture).ToUpper();
                        DateTo2 = "else, latest by " + job.ExtDateTime.ToString("dd/MM/yyyy h:mm tt", CultureInfo.InvariantCulture).ToUpper();
                    }
                    else if (job.ExpressType != eExpressType.etNormal && hasAltDelWin)
                    {
                        DateTo = job.ToDateTime.ToString("dd/MM/yyyy h:mm tt", CultureInfo.InvariantCulture).ToUpper();
                        DateTo2 = "ALTERNATIVELY\r\n" + job.ExtFromDateTime.ToString("dd/MM/yyyy h:mm tt", CultureInfo.InvariantCulture).ToUpper() + 
                            " to " + job.ExtDateTime.ToString("dd/MM/yyyy h:mm tt", CultureInfo.InvariantCulture).ToUpper();
                    }
                    else
                    {
                        DateTo = job.ToDateTime.ToString("dd/MM/yyyy h:mm tt", CultureInfo.InvariantCulture).ToUpper();
                        DateTo2 = "";
                    }

                    PURdy = job.ReadyDateTime.ToString("dd/MM/yyyy h:mm tt", CultureInfo.InvariantCulture).ToUpper();
                }

                if (PL != null && PL.Contacts != null && PL.Contacts.Length > 0)
                {
                    if (!String.IsNullOrEmpty(PL.Contacts[0].NAME))
                        Sender = PL.Contacts[0].NAME;
                    else Sender = "No Information";

                    PUTel = PL.Contacts[0].TEL;
                    PUMobile = PL.Contacts[0].MOBILE;
                }

                PUInstruction = !String.IsNullOrEmpty(job.PUSI) ? job.PUSI : "";
                PUAvoid = job.PULunch_Avoid ? (job.PULunch_From.ToString("dd/MM/yyyy h:mm tt", CultureInfo.InvariantCulture).ToUpper() + 
                    " to " + job.PULunch_To.ToString("dd/MM/yyyy h:mm tt", CultureInfo.InvariantCulture).ToUpper()) : "";

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

                if (DL != null && DL.Contacts != null && DL.Contacts.Length > 0)
                {
                    if (!String.IsNullOrEmpty(DL.Contacts[0].NAME))
                        Receiver = DL.Contacts[0].NAME;
                    else Receiver = "No Information";

                    DLTel = DL.Contacts[0].TEL;
                    DLMobile = DL.Contacts[0].MOBILE;
                }

                DLInstruction = !String.IsNullOrEmpty(job.DLSI) ? job.DLSI : "";
                DLAvoid = job.DLLunch_Avoid ? (job.DLLunch_From.ToString("dd/MM/yyyy h:mm tt", CultureInfo.InvariantCulture).ToUpper() + 
                    " to " + job.DLLunch_To.ToString("dd/MM/yyyy h:mm tt", CultureInfo.InvariantCulture).ToUpper()) : "";

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


                if (RLjob != null)
                {
                    RLCOD = RLjob.COD == null ? 0 : RLjob.COD;
                    hasAltDelWin2 = (RLjob.ExtFromDateTime != DateTime.MinValue) &&
                           (RLjob.ExtDateTime != DateTime.MinValue) && RLjob.ExpressType != eExpressType.etNormal;

                    RLDateFrom = RLjob.FromDateTime.ToString("dd/MM/yyyy h:mm tt", CultureInfo.InvariantCulture).ToUpper();
                    /*RLDateTo = Rjob.ExpressType == eExpressType.etNormal && !hasAltDelWin2 ?
                        Rjob.ToDateTime.ToString("dd/MM/yyyy h:mm tt").ToUpper() + " else, latest by " + Rjob.ExtDateTime.ToString("dd/MM/yyyy h:mm tt").ToUpper() :

                        Rjob.ExpressType != eExpressType.etNormal && hasAltDelWin2 ?
                        Rjob.ToDateTime.ToString("dd/MM/yyyy h:mm tt").ToUpper() + "\r\nALTERNATIVELY\r\n" + 
                        Rjob.ExtFromDateTime.ToString("dd/MM/yyyy h:mm tt").ToUpper() + " to " + Rjob.ExtDateTime.ToString("dd/MM/yyyy h:mm tt").ToUpper() :
                        Rjob.ToDateTime.ToString("dd/MM/yyyy h:mm tt").ToUpper();*/

                    if (RLjob.ExpressType == eExpressType.etNormal && !hasAltDelWin2)
                    {
                        RLDateTo = RLjob.ToDateTime.ToString("dd/MM/yyyy h:mm tt", CultureInfo.InvariantCulture).ToUpper();
                        RLDateTo2 = "else, latest by " + RLjob.ExtDateTime.ToString("dd/MM/yyyy h:mmtt", CultureInfo.InvariantCulture).ToUpper();
                    } else if (RLjob.ExpressType != eExpressType.etNormal && hasAltDelWin2)
                    {
                        RLDateTo = RLjob.ToDateTime.ToString("dd/MM/yyyy h:mm tt", CultureInfo.InvariantCulture).ToUpper();
                        RLDateTo2 = "ALTERNATIVELY\r\n" + RLjob.ExtFromDateTime.ToString("dd/MM/yyyy h:mm tt", CultureInfo.InvariantCulture).ToUpper() + 
                            " to " + RLjob.ExtDateTime.ToString("dd/MM/yyyy h:mm tt", CultureInfo.InvariantCulture).ToUpper();
                    } else
                    {
                        RLDateTo = RLjob.ToDateTime.ToString("dd/MM/yyyy h:mm tt", CultureInfo.InvariantCulture).ToUpper();
                        RLDateTo2 = "";
                    }

                    RLPURdy = RLjob.ReadyDateTime.ToString("dd/MM/yyyy h:mm tt", CultureInfo.InvariantCulture).ToUpper();

                    if (RL != null && RL.Contacts != null && RL.Contacts.Length > 0)
                    {
                        if (!String.IsNullOrEmpty(RL.Contacts[0].NAME))
                            RLReceiver = RL.Contacts[0].NAME;
                        else RLReceiver = "No Information";

                        RLTel = RL.Contacts[0].TEL;
                        RLMobile = RL.Contacts[0].MOBILE;
                    }

                    RLInstruction = !String.IsNullOrEmpty(RLjob.DLSI) ? RLjob.DLSI : "";
                    RLAvoid = RLjob.DLLunch_Avoid ? (RLjob.DLLunch_From.ToString("dd/MM/yyyy h:mm tt", CultureInfo.InvariantCulture).ToUpper() + 
                        " to " + RLjob.DLLunch_To.ToString("dd/MM/yyyy h:mm tt", CultureInfo.InvariantCulture).ToUpper()) : "";

                    RLCompany = RL != null && !String.IsNullOrEmpty(RL.COMPANY) ? RL.COMPANY : "";

                    if (RL != null && !String.IsNullOrEmpty(RL.BLOCK))
                        RLAdd = RL.BLOCK;
                    if (RL != null && !String.IsNullOrEmpty(RL.STREET))
                        RLAdd += (!String.IsNullOrEmpty(ToAdd) ? ", " : "") + RL.STREET;
                    if (RL != null && !String.IsNullOrEmpty(RL.UNIT))
                        RLAdd += (!String.IsNullOrEmpty(ToAdd) ? ", " : "") + RL.UNIT;
                    if (RL != null && !String.IsNullOrEmpty(RL.BUILDING))
                        RLAdd += (!String.IsNullOrEmpty(ToAdd) ? ", " : "") + RL.BUILDING;

                    if (RL != null && !String.IsNullOrEmpty(RL.BLOCK))
                        RLBLOCK = RL.BLOCK;
                    if (RL != null && !String.IsNullOrEmpty(RL.STREET))
                        RLSTREET = RL.STREET;
                    if (RL != null && !String.IsNullOrEmpty(RL.UNIT))
                        RLUNIT = RL.UNIT;
                    if (RL != null && !String.IsNullOrEmpty(RL.BUILDING))
                        RLBUILDING = RL.BUILDING;
                    if (RL != null && !String.IsNullOrEmpty(RL.POSTALCODE))
                        RLPOSTALCODE = RL.POSTALCODE;

                    if (RL != null)
                        RLLocationType = RL.LocationType == XDelServiceRef.Location_Type.Residential ? 1 :
                            RL.LocationType == XDelServiceRef.Location_Type.Office ? 2 : 2;

                }

                lblBSSummaryJobIDX.Text = job.JobsIDX.ToString();
                lblBSSummaryJobIDXText.Text = "Job ID: " + job.JobsIDX.ToString();
                lblBSSummaryJobNoHidden.Text = job.JobNo;
                lblBSSummaryJobNo.Text = "Job No: " + job.JobNo;
                lblBSSummaryRefNo.Text = "Ref No: " + job.ConsignmentNote;

                lblBSSummaryJobIDXText.IsVisible = string.IsNullOrEmpty(job.JobNo);
                lblBSSummaryJobNo.IsVisible = !string.IsNullOrEmpty(job.JobNo);
                lblBSSummaryRefNo.IsVisible = !string.IsNullOrEmpty(job.ConsignmentNote);

                lblBSSummarySvcType.Text = common.getServiceSelected(job.TOSType);
                lblBSSummaryExpType.Text = common.convertExpressType(job.ExpressType);
                lblBSSummaryContentType.Text = job.DeliveryContents ?? "";
                lblBSSummaryPcs.Text = job.Pieces.ToString();
                lblBSSummaryWeight.Text = job.Weight.ToString() + " KG";

                lblBSSummaryPURdyTime.Text = PURdy;
                lblBSSummaryPULunch.Text = PUAvoid;
                lblBSSummaryDLFrom.Text = DateFrom;
                lblBSSummaryDLBy.Text = DateTo;
                lblBSSummaryDLBy2.Text = DateTo2;
                lblBSSummaryDLLunch.Text = DLAvoid;

                lblBSSummaryPUCompany.Text = PUCompany;
                lblBSSummaryPUAddress.Text = FromAdd;
                lblBSSummaryPUPostal.Text = PUPOSTALCODE;
                lblBSSummaryPUContact.Text = Sender;
                lblBSSummaryPUTel.Text = PUTel;
                lblBSSummaryPUMobile.Text = PUMobile;
                lblBSSummaryPUInstruction.Text = PUInstruction;

                lblBSSummaryDLCompany.Text = DLCompany;
                lblBSSummaryDLAddress.Text = ToAdd;
                lblBSSummaryDLBLOCK.Text = DLBLOCK;
                lblBSSummaryDLSTREET.Text = DLSTREET;
                lblBSSummaryDLUNIT.Text = DLUNIT;
                lblBSSummaryDLBUILDING.Text = DLBUILDING;
                lblBSSummaryDLLocationType.Text = DLLocationType.ToString();

                lblBSSummaryDLPostal.Text = DLPOSTALCODE;
                lblBSSummaryDLContact.Text = Receiver;
                lblBSSummaryDLTel.Text = DLTel;
                lblBSSummaryDLMobile.Text = DLMobile;
                lblBSSummaryDLInstruction.Text = DLInstruction;

                b_payable1.IsVisible = (logininfo.clientInfo.AccountType == TAccountType.atPrePaid);
                if (logininfo.clientInfo.AccountType != TAccountType.atCredit)
                {
                    XDelServiceRef.XDelCODStructure cashbreak = await xs.GetCODBreakdownAsync(logininfo!.clientInfo.Web_UID, job.JobsIDX ?? 0);
                    if (cashbreak.Status == 0)
                    {
                        double codvalue = Convert.ToDouble(cashbreak.CODValue);
                        double codgst = Convert.ToDouble(cashbreak.GSTValue);
                        double payable = codvalue + codgst;
                        payable = Math.Ceiling(payable * 20) / 20;
                        TotalPayableAmt += payable;

                        lblBSSummaryNetprice.Text = cashbreak.CODValue.ToString("C", new CultureInfo("en-US"));
                        lblBSSummaryGST.Text = cashbreak.GSTValue.ToString("C", new CultureInfo("en-US"));
                        lblBSSummaryTotalAmt.Text = payable.ToString("C", new CultureInfo("en-US"));
                    }
                }

                    if (RLjob != null)
                {
                    lblBSSummaryJobIDX2.Text = RLjob.JobsIDX.ToString();
                    lblBSSummaryJobIDXText2.Text = "Job ID: " + RLjob.JobsIDX.ToString();
                    lblBSSummaryJobNoHidden2.Text = RLjob.JobNo;
                    lblBSSummaryJobNo2.Text = "Job No: " + RLjob.JobNo;
                    lblBSSummaryRefNo2.Text = "Ref No: " + RLjob.ConsignmentNote;

                    lblBSSummaryJobIDXText2.IsVisible = string.IsNullOrEmpty(RLjob.JobNo);
                    lblBSSummaryJobNo2.IsVisible = !string.IsNullOrEmpty(RLjob.JobNo);
                    lblBSSummaryRefNo2.IsVisible = !string.IsNullOrEmpty(RLjob.ConsignmentNote);

                    lblBSSummarySvcType2.Text = common.getServiceSelected(RLjob.TOSType);
                    lblBSSummaryExpType2.Text = common.convertExpressType(RLjob.ExpressType);
                    lblBSSummaryContentType2.Text = RLjob.DeliveryContents ?? "";
                    lblBSSummaryPcs2.Text = RLjob.Pieces.ToString();
                    lblBSSummaryWeight2.Text = RLjob.Weight.ToString() + " KG";

                    lblBSSummaryPURdyTime2.Text = RLPURdy;
                    lblBSSummaryPULunch2.Text = RLAvoid;
                    lblBSSummaryDLFrom2.Text = RLDateFrom;
                    lblBSSummaryDLBy3.Text = RLDateTo;
                    lblBSSummaryDLBy4.Text = RLDateTo2;
                    lblBSSummaryDLLunch2.Text = RLAvoid;

                    lblBSSummaryDLCompany2.Text = RLCompany;
                    lblBSSummaryDLAddress2.Text = RLAdd;
                    lblBSSummaryDLBLOCK2.Text = RLBLOCK;
                    lblBSSummaryDLSTREET2.Text = RLSTREET;
                    lblBSSummaryDLUNIT2.Text = RLUNIT;
                    lblBSSummaryDLBUILDING2.Text = RLBUILDING;
                    lblBSSummaryDLLocationType2.Text = RLLocationType.ToString();

                    lblBSSummaryDLPostal2.Text = RLPOSTALCODE;
                    lblBSSummaryDLContact2.Text = RLReceiver;
                    lblBSSummaryDLTel2.Text = RLTel;
                    lblBSSummaryDLMobile2.Text = RLMobile;
                    lblBSSummaryDLInstruction2.Text = RLInstruction;

                    b_payable2.IsVisible = (logininfo.clientInfo.AccountType == TAccountType.atPrePaid);
                    if (logininfo.clientInfo.AccountType != TAccountType.atCredit)
                    {
                        XDelServiceRef.XDelCODStructure cashbreak = await xs.GetCODBreakdownAsync(logininfo!.clientInfo.Web_UID, RLjob.JobsIDX ?? 0);
                        if (cashbreak.Status == 0)
                        {
                            double codvalue = Convert.ToDouble(cashbreak.CODValue);
                            double codgst = Convert.ToDouble(cashbreak.GSTValue);
                            double payable = codvalue + codgst;
                            payable = Math.Ceiling(payable * 20) / 20;
                            TotalPayableAmt += payable;

                            lblBSSummaryNetprice2.Text = cashbreak.CODValue.ToString("C", new CultureInfo("en-US"));
                            lblBSSummaryGST2.Text = cashbreak.GSTValue.ToString("C", new CultureInfo("en-US"));
                            lblBSSummaryTotalAmt2.Text = payable.ToString("C", new CultureInfo("en-US"));
                        }
                    }
                }

                gp1.IsVisible = RLjob != null;
                gp2.IsVisible = RLjob != null;
                b2_1.IsVisible = RLjob != null;
                b2_2.IsVisible = RLjob != null;
                b2_3.IsVisible = RLjob != null;
                lblTotalValue.Text = TotalPayableAmt.ToString("C", new CultureInfo("en-US"));

                CheckIfPURdyDateTimeExpired(job, RLjob);
                CheckIfDelStartDateTimeExpired(job, RLjob);
                convertJobsToCreateJobVM(JobStruct);
                await closeProgress_dialog();
                validateButton();
            } else
            {
                await closeProgress_dialog();
                await DisplayAlertAsync("", "Unable to load summary.\r\nPlease try again", "OK");
                BackToCreateJob();
                return;
            }
        } catch (Exception e)
        {
            string s = e.Message;
            await closeProgress_dialog();
            await DisplayAlertAsync("", "Unable to load summary.\r\nPlease try again", "OK");
            BackToCreateJob();
        }
    }

    private void convertJobsToCreateJobVM(JobStructure JobStruct)
    {
        XDelServiceRef.JobInfo job = new XDelServiceRef.JobInfo();
        XDelServiceRef.JobInfo? Rjob = null;
        XDelServiceRef.AddressStructure? PL;
        XDelServiceRef.AddressStructure? DL;
        XDelServiceRef.AddressStructure? RL;
        try
        {
            if (JobStruct != null)
            {
                if (vm == null)
                    vm = new CreateJobVM();

                vm.FromSummary = true;
                btnTopNavEditJob.IsVisible = ltr != null;

                job = JobStruct.JobList[0];
                if (job == null)
                    return;

                Rjob = job.LinkedJobs != null && job.LinkedJobs.Length > 0 && job.LinkedJobs[0] != null ? job.LinkedJobs[0] : null;

                PL = job.PURedirectedLocation != null ? job.PURedirectedLocation : job.PULocation;
                DL = job.DLRedirectedLocation != null ? job.DLRedirectedLocation : job.DLLocation;
                RL = Rjob == null ? null : Rjob.DLRedirectedLocation != null ? Rjob.DLRedirectedLocation : job.LinkedJobs![0].DLLocation;
                vm.job1 = job != null ? job : null;
                vm.job2 = Rjob != null ? Rjob : null;
                vm.JobsIDX = (long)(job != null ? job.JobsIDX : 0);
                vm.JobsIDX2 = (long)(Rjob != null ? Rjob.JobsIDX : 0);
                vm.JobsIDX2_ = (long)(Rjob != null ? Rjob.JobsIDX : 0);

                vm.TwoWay = Rjob != null;

                //Address
                vm.ColAddressFinal = PL;
                vm.DelAddressFinal = DL;
                vm.RtnAddressFinal = RL;

                vm.PUSI = job.PUSI;
                vm.DLSI = job.DLSI;
                vm.DLSI2 = Rjob != null ? Rjob.DLSI : "";


                //ContentType
                vm.reqCold1 = (job.FLAG2 & 64) == 64;
                vm.pcs1 = job.Pieces != null ? (int) job.Pieces : 1;
                vm.kg1 = job.Weight != null ? (int) job.Weight : 1;
                vm.ContentType1 = job.DeliveryContents;

                vm.reqCold2 = Rjob != null ? (Rjob.FLAG2 & 64) == 64 : false;
                vm.pcs2 = Rjob != null ? (int) Rjob.Pieces : 1;
                vm.kg2 = Rjob != null ? (int) Rjob.Weight : 1;
                vm.ContentType2 = Rjob != null ? Rjob.DeliveryContents : "";

                //InvoiceBS
                if (job != null && job.DOList != null && job.DOList.Length > 0)
                {
                    vm.newInvoicesP1();
                    foreach (DOObject _d in job.DOList)
                    {
                        vm.addInvP1(new invoice(0, _d.Invoice, _d.Amount, _d.Amount.ToString("F2")));
                    }
                }
                if (Rjob != null && Rjob.DOList != null && Rjob.DOList.Length > 0)
                {
                    vm.newInvoicesP2();
                    foreach (DOObject _d in Rjob.DOList)
                    {
                        vm.addInvP2(new invoice(0, _d.Invoice, _d.Amount, _d.Amount.ToString("F2")));
                    }
                }

                if (job != null)
                {
                    //CollectionTime
                    vm.colDateSelectedValue = job.ReadyDateTime.ToString("dd/MM/yyyy");
                    vm.ReadyTimeSelectedValue = job.ReadyDateTime.ToString("HH:mm");

                    vm.colLunchSelectedIDX = "0";
                    vm.colDateLunchTimeValue = "0";

                    if (job.PULunch_Avoid)
                    {
                        if (job.PULunch_From != DateTime.MinValue && job.PULunch_To != DateTime.MinValue)
                        {
                            if (job.PULunch_From.ToString("HH:mm").Equals("11:30") && job.PULunch_To.ToString("HH:mm").Equals("12:30"))
                            {
                                vm.colDateLunchTimeValue = "4";
                                vm.colLunchSelectedIDX = "1";
                            } else if (job.PULunch_From.ToString("HH:mm").Equals("12:00") && job.PULunch_To.ToString("HH:mm").Equals("13:00"))
                            {
                                vm.colDateLunchTimeValue = "3";
                                vm.colLunchSelectedIDX = "2";
                            }
                            else if (job.PULunch_From.ToString("HH:mm").Equals("12:00") && job.PULunch_To.ToString("HH:mm").Equals("14:00"))
                            {
                                vm.colDateLunchTimeValue = "2";
                                vm.colLunchSelectedIDX = "3";
                            }
                            else if (job.PULunch_From.ToString("HH:mm").Equals("13:00") && job.PULunch_To.ToString("HH:mm").Equals("14:00"))
                            {
                                vm.colDateLunchTimeValue = "1";
                                vm.colLunchSelectedIDX = "4";
                            }
                        }
                    }

                    vm.colDateTimeFinalValue = job.ReadyDateTime.ToString("dd/MM/yyyy HH:mm");
                    vm.colDateTimeFinalDispText = job.ReadyDateTime.ToString("dd/MM/yyyy h:mm tt", CultureInfo.InvariantCulture).ToUpper();

                    //DeliveryTime
                    //vm.dfDateSelectedValue = job.FromDateTime.ToString("dd/MM/yyyy");
                    vm.dfDateSelectedValue = job.FromDateTime.ToString("dd/MM/yyyy HH:mm");
                    vm.dfTimeSelectedValue = job.FromDateTime.ToString("HH:mm");

                    vm.dfLunchSelectedIDX = "0";
                    vm.dfDateLunchTimeValue = "0";

                    if (job.DLLunch_Avoid)
                    {
                        if (job.DLLunch_From != DateTime.MinValue && job.DLLunch_To != DateTime.MinValue)
                        {
                            if (job.DLLunch_From.ToString("HH:mm").Equals("11:30") && job.DLLunch_To.ToString("HH:mm").Equals("12:30"))
                            {
                                vm.dfDateLunchTimeValue = "4";
                                vm.dfLunchSelectedIDX = "1";
                            }
                            else if (job.DLLunch_From.ToString("HH:mm").Equals("12:00") && job.DLLunch_To.ToString("HH:mm").Equals("13:00"))
                            {
                                vm.dfDateLunchTimeValue = "3";
                                vm.dfLunchSelectedIDX = "2";
                            }
                            else if (job.DLLunch_From.ToString("HH:mm").Equals("12:00") && job.DLLunch_To.ToString("HH:mm").Equals("14:00"))
                            {
                                vm.dfDateLunchTimeValue = "2";
                                vm.dfLunchSelectedIDX = "3";
                            }
                            else if (job.DLLunch_From.ToString("HH:mm").Equals("13:00") && job.DLLunch_To.ToString("HH:mm").Equals("14:00"))
                            {
                                vm.dfDateLunchTimeValue = "1";
                                vm.dfLunchSelectedIDX = "4";
                            }
                        }
                    }

                    DateTime extDelby = DateTime.MinValue;
                    vm.dfDateTimeFinalValue = job.FromDateTime.ToString("dd/MM/yyyy HH:mm");

                    vm.dbTimeSelectedActualValue = job.ToDateTime.ToString("HH:mm");
                    vm.dbTimeSelectedValue = job.ExpressType == eExpressType.etNormal ? "Next Working Day\nby 12 PM" :
                        job.ToDateTime.ToString("h:mm tt", CultureInfo.InvariantCulture).ToUpper();


                    vm.dbExtByDateTimeValue = extDelby.ToString("dd/MM/yyyy HH:mm");

                    vm.dbDateTimeFinalValue = job.ToDateTime.ToString("dd/MM/yyyy HH:mm");
                    
                    
                    vm.exp1 = job.ExpressType.ToString();
                    vm.expStr1 = job.ExpressType.ToString();

                    vm.dbExtByDateTimeValue = job.ExpressType == eExpressType.etNormal ? 
                        job.ExtDateTime.ToString("dd/MM/yyyy HH:mm tt", CultureInfo.InvariantCulture).ToUpper() : 
                        DateTime.MinValue.ToString("dd/MM/yyyy HH:mm");

                    string dfbDateTimeFinalDispText = "Select";
                    if (vm.expStr1.Equals(common.eExpressType.etNormal.ToString()))
                    {
                        dfbDateTimeFinalDispText = job.FromDateTime.ToString("dd/MM/yyyy h:mm tt", CultureInfo.InvariantCulture).ToUpper() +
                            "\nto\n" +
                            job.ToDateTime.ToString("dd/MM/yyyy h:mm tt", CultureInfo.InvariantCulture).ToUpper() +
                            "\nelse latest by\n" + vm.dbExtByDateTimeValue;
                        vm.dfbDateTimeFinalDispText = dfbDateTimeFinalDispText;
                    }
                    else
                    {
                        dfbDateTimeFinalDispText = job.FromDateTime.ToString("dd/MM/yyyy h:mm tt", CultureInfo.InvariantCulture).ToUpper() +
                            "\nto\n" +
                            job.ToDateTime.ToString("dd/MM/yyyy h:mm tt", CultureInfo.InvariantCulture).ToUpper();
                        vm.dfbDateTimeFinalDispText = dfbDateTimeFinalDispText;
                    }
                }

                if (Rjob != null)
                {
                    //RtnTime
                    vm.rtndfDateSelectedValue = Rjob.FromDateTime.ToString("dd/MM/yyyy HH:mm");
                    vm.rtndfTimeSelectedValue = Rjob.FromDateTime.ToString("HH:mm");

                    vm.rtndfLunchSelectedIDX = "0";
                    vm.rtndfDateLunchTimeValue = "0";

                    if (Rjob.DLLunch_Avoid)
                    {
                        if (Rjob.DLLunch_From != DateTime.MinValue && Rjob.DLLunch_To != DateTime.MinValue)
                        {
                            if (Rjob.DLLunch_From.ToString("HH:mm").Equals("11:30") && Rjob.DLLunch_To.ToString("HH:mm").Equals("12:30"))
                            {
                                vm.rtndfDateLunchTimeValue = "4";
                                vm.rtndfLunchSelectedIDX = "1";
                            }
                            else if (Rjob.DLLunch_From.ToString("HH:mm").Equals("12:00") && Rjob.DLLunch_To.ToString("HH:mm").Equals("13:00"))
                            {
                                vm.rtndfDateLunchTimeValue = "3";
                                vm.rtndfLunchSelectedIDX = "2";
                            }
                            else if (Rjob.DLLunch_From.ToString("HH:mm").Equals("12:00") && Rjob.DLLunch_To.ToString("HH:mm").Equals("14:00"))
                            {
                                vm.rtndfDateLunchTimeValue = "2";
                                vm.rtndfLunchSelectedIDX = "3";
                            }
                            else if (Rjob.DLLunch_From.ToString("HH:mm").Equals("13:00") && Rjob.DLLunch_To.ToString("HH:mm").Equals("14:00"))
                            {
                                vm.rtndfDateLunchTimeValue = "1";
                                vm.rtndfLunchSelectedIDX = "4";
                            }
                        }
                    }

                    DateTime extDelby = DateTime.MinValue;
                    vm.rtndfDateTimeFinalValue = Rjob.FromDateTime.ToString("dd/MM/yyyy HH:mm");

                    vm.rtndbTimeSelectedActualValue = Rjob.ToDateTime.ToString("HH:mm");
                    vm.rtndbTimeSelectedValue = Rjob.ExpressType == eExpressType.etNormal ? "Next Working Day\nby 12 PM" :
                        Rjob.ToDateTime.ToString("h:mm tt", CultureInfo.InvariantCulture).ToUpper();

                    vm.rtndbExtByDateTimeValue = extDelby.ToString("dd/MM/yyyy HH:mm");

                    vm.rtndbDateTimeFinalValue = Rjob.ToDateTime.ToString("dd/MM/yyyy HH:mm");

                    vm.exp2 = Rjob.ExpressType.ToString();
                    vm.expStr2 = Rjob.ExpressType.ToString();

                    vm.rtndbExtByDateTimeValue = Rjob.ExpressType == eExpressType.etNormal ?
                        Rjob.ExtDateTime.ToString("dd/MM/yyyy HH:mm tt", CultureInfo.InvariantCulture).ToUpper() :
                        DateTime.MinValue.ToString("dd/MM/yyyy HH:mm");

                    string rtndfbDateTimeFinalDispText = "Select";
                    if (vm.expStr2.Equals(common.eExpressType.etNormal.ToString()))
                    {
                        rtndfbDateTimeFinalDispText = Rjob.FromDateTime.ToString("dd/MM/yyyy h:mm tt", CultureInfo.InvariantCulture).ToUpper() +
                            "\nto\n" +
                            Rjob.ToDateTime.ToString("dd/MM/yyyy h:mm tt", CultureInfo.InvariantCulture).ToUpper() +
                            "\nelse latest by\n" + vm.rtndbExtByDateTimeValue;
                        vm.rtndfbDateTimeFinalDispText = rtndfbDateTimeFinalDispText;
                    }
                    else
                    {
                        rtndfbDateTimeFinalDispText = Rjob.FromDateTime.ToString("dd/MM/yyyy h:mm tt", CultureInfo.InvariantCulture).ToUpper() +
                            "\nto\n" +
                            Rjob.ToDateTime.ToString("dd/MM/yyyy h:mm tt", CultureInfo.InvariantCulture).ToUpper();
                        vm.rtndfbDateTimeFinalDispText = rtndfbDateTimeFinalDispText;
                    }
                }
            }
        } catch (Exception e)
        {
            string s = e.Message;
        }
    }

    bool p1purdyOk = true;
    bool p1dfOk = true;
    bool p1dtOk = true;
    bool p2purdyOk = true;
    bool p2dfOk = true;
    bool p2dtOk = true;

    void CheckIfPURdyDateTimeExpired(XDelServiceRef.JobInfo? job1, XDelServiceRef.JobInfo? job2)
    {
        XDelServiceRef.JobInfo? job = null;
        int p1p2 = 0;
        TimeSpan NMLnGUARANTEEcutoffTimeTS = new System.TimeSpan(11, 31, 0);
        DateTime NMLnGUARANTEEcutoffTime;
        System.TimeSpan ts;
        System.TimeSpan ts2 = new System.TimeSpan(17, 30, 0);
        System.TimeSpan ts3;
        DateTime todayPUcutoffTime = DateTime.Today.Date + ts2;

        DateTime PURdyDateTime = DateTime.Now;
        DateTime DelFromDateTime = DateTime.Now;
        DateTime DelToDateTime = DateTime.Now;
        DateTime NewPuRdyDate = DateTime.Now;
        DateTime newDelFromDateTime = DateTime.Now;
        DateTime newDelToDateTime = DateTime.Now;
        DateTime todayDateTime = DateTime.Now;
        bool currentTimeIsToday = true;
        Holiday v = new Holiday();
        DateTime minVal = DateTime.MinValue;
        Label? lblPURdy;

        try
        {
            job = job1 != null ? job1 : job2 != null ? job2 : null;
            p1p2 = job1 != null ? 1 : job2 != null ? 2 : 0;
            lblPURdy = job1 != null ? lblBSSummaryPURdyTime : job2 != null ? lblBSSummaryPURdyTime2 : null;

            if (job != null)
            {
                lblPURdy = lblBSSummaryPURdyTime;
                if (job.ExpressType == eExpressType.etNormal || job.ExpressType == eExpressType.etGuaranteed)
                {
                    PURdyDateTime = DateTime.Parse(lblPURdy.Text);
                    NMLnGUARANTEEcutoffTime = todayDateTime.Date;
                    NMLnGUARANTEEcutoffTime = NMLnGUARANTEEcutoffTime.Date + NMLnGUARANTEEcutoffTimeTS;

                    if ((PURdyDateTime.Date == todayDateTime.Date) && 
                        (todayDateTime < NMLnGUARANTEEcutoffTime) && 
                        (PURdyDateTime < NMLnGUARANTEEcutoffTime))
                    {
                        if (p1p2 == 1)
                        {
                            p1purdyOk = true;
                            p1dfOk = true;
                            p1dtOk = true;
                            p2purdyOk = true;
                            p2dfOk = true;
                            p2dtOk = true;
                        } else if (p1p2 == 2)
                        {
                            p2purdyOk = true;
                            p2dfOk = true;
                            p2dtOk = true;
                        }
                    }
                    else if ((PURdyDateTime.Date == todayDateTime.Date) &&
                        ((todayDateTime > NMLnGUARANTEEcutoffTime) && (todayDateTime < todayPUcutoffTime)) &&
                        ((PURdyDateTime < todayDateTime) && (PURdyDateTime > NMLnGUARANTEEcutoffTime)))
                    {
                        if (p1p2 == 1)
                        {
                            p1purdyOk = true;
                            p1dfOk = true;
                            p1dtOk = true;
                            p2purdyOk = true;
                            p2dfOk = true;
                            p2dtOk = true;
                        }
                        else if (p1p2 == 2)
                        {
                            p2purdyOk = true;
                            p2dfOk = true;
                            p2dtOk = true;
                        }
                    }
                    else if (PURdyDateTime.Date < todayDateTime.Date)
                    {
                        if(p1p2 == 1)
                        {
                            p1purdyOk = false;
                            p1dfOk = false;
                            p1dtOk = false;
                            p2purdyOk = false;
                            p2dfOk = false;
                            p2dtOk = false;
                        } else
                        {
                            p2purdyOk = false;
                            p2dfOk = false;
                            p2dtOk = false;
                        }
                    }
                    else if ((PURdyDateTime.Date == todayDateTime.Date) && 
                        (todayDateTime > NMLnGUARANTEEcutoffTime) && 
                        (PURdyDateTime < todayDateTime))
                    {
                        if (p1p2 == 1)
                        {
                            p1purdyOk = false;
                            p1dfOk = false;
                            p1dtOk = false;
                            p2purdyOk = false;
                            p2dfOk = false;
                            p2dtOk = false;
                        }
                        else
                        {
                            p2purdyOk = false;
                            p2dfOk = false;
                            p2dtOk = false;
                        }
                    }
                    else if ((PURdyDateTime.Date == todayDateTime.Date) && 
                        (todayDateTime < NMLnGUARANTEEcutoffTime) && 
                        (PURdyDateTime < todayDateTime))
                    {
                        if (p1p2 == 1)
                        {
                            p1purdyOk = false;
                            p1dfOk = false;
                            p1dtOk = false;
                            p2purdyOk = false;
                            p2dfOk = false;
                            p2dtOk = false;
                        }
                        else
                        {
                            p2purdyOk = false;
                            p2dfOk = false;
                            p2dtOk = false;
                        }
                    }
                }
                else if (job.ExpressType == eExpressType.etPriority)
                {
                    NMLnGUARANTEEcutoffTimeTS = new System.TimeSpan(11, 31, 0);
                    PURdyDateTime = DateTime.Parse(lblPURdy.Text);
                    NMLnGUARANTEEcutoffTime = todayDateTime.Date;
                    NMLnGUARANTEEcutoffTime = NMLnGUARANTEEcutoffTime.Date + NMLnGUARANTEEcutoffTimeTS;

                    if ((PURdyDateTime.Date == todayDateTime.Date) && 
                        (todayDateTime < NMLnGUARANTEEcutoffTime) && 
                        (PURdyDateTime < NMLnGUARANTEEcutoffTime))
                    {
                        if (p1p2 == 1)
                        {
                            p1purdyOk = true;
                            p1dfOk = true;
                            p1dtOk = true;
                            p2purdyOk = true;
                            p2dfOk = true;
                            p2dtOk = true;
                        }
                        else if (p1p2 == 2)
                        {
                            p2purdyOk = true;
                            p2dfOk = true;
                            p2dtOk = true;
                        }
                    } else if ((PURdyDateTime.Date == todayDateTime.Date) &&
                        ((todayDateTime > NMLnGUARANTEEcutoffTime) && (todayDateTime < todayPUcutoffTime)) &&
                        ((PURdyDateTime < todayDateTime) && (PURdyDateTime > NMLnGUARANTEEcutoffTime)))
                    {
                        if (p1p2 == 1)
                        {
                            p1purdyOk = true;
                            p1dfOk = true;
                            p1dtOk = true;
                            p2purdyOk = true;
                            p2dfOk = true;
                            p2dtOk = true;
                        }
                        else if (p1p2 == 2)
                        {
                            p2purdyOk = true;
                            p2dfOk = true;
                            p2dtOk = true;
                        }
                    } else if (PURdyDateTime.Date < todayDateTime.Date)
                    {
                        if (p1p2 == 1)
                        {
                            p1purdyOk = false;
                            p1dfOk = false;
                            p1dtOk = false;
                            p2purdyOk = false;
                            p2dfOk = false;
                            p2dtOk = false;
                        }
                        else
                        {
                            p2purdyOk = false;
                            p2dfOk = false;
                            p2dtOk = false;
                        }
                    } else if ((PURdyDateTime.Date == todayDateTime.Date) &&
                        (todayDateTime > NMLnGUARANTEEcutoffTime) &&
                        (PURdyDateTime < todayDateTime))
                    {
                        if (p1p2 == 1)
                        {
                            p1purdyOk = false;
                            p1dfOk = false;
                            p1dtOk = false;
                            p2purdyOk = false;
                            p2dfOk = false;
                            p2dtOk = false;
                        }
                        else
                        {
                            p2purdyOk = false;
                            p2dfOk = false;
                            p2dtOk = false;
                        }
                    } else if ((todayDateTime < NMLnGUARANTEEcutoffTime) &&
                        (PURdyDateTime < todayDateTime))
                    {
                        if (p1p2 == 1)
                        {
                            p1purdyOk = false;
                            p1dfOk = false;
                            p1dtOk = false;
                            p2purdyOk = false;
                            p2dfOk = false;
                            p2dtOk = false;
                        }
                        else
                        {
                            p2purdyOk = false;
                            p2dfOk = false;
                            p2dtOk = false;
                        }
                    }
                }
                else if (job.ExpressType == eExpressType.etThreeHour)
                {
                    NMLnGUARANTEEcutoffTimeTS = new System.TimeSpan(14, 01, 0);
                    PURdyDateTime = DateTime.Parse(lblPURdy.Text);
                    NMLnGUARANTEEcutoffTime = todayDateTime.Date;
                    NMLnGUARANTEEcutoffTime = NMLnGUARANTEEcutoffTime.Date + NMLnGUARANTEEcutoffTimeTS;

                    if ((PURdyDateTime.Date == todayDateTime.Date) && 
                        (todayDateTime < NMLnGUARANTEEcutoffTime) && 
                        (PURdyDateTime < NMLnGUARANTEEcutoffTime))
                    {
                        if (p1p2 == 1)
                        {
                            p1purdyOk = true;
                            p1dfOk = true;
                            p1dtOk = true;
                            p2purdyOk = true;
                            p2dfOk = true;
                            p2dtOk = true;
                        }
                        else if (p1p2 == 2)
                        {
                            p2purdyOk = true;
                            p2dfOk = true;
                            p2dtOk = true;
                        }
                    } else if ((PURdyDateTime.Date == todayDateTime.Date) &&
                        ((todayDateTime > NMLnGUARANTEEcutoffTime) && (todayDateTime < todayPUcutoffTime)) &&
                        ((PURdyDateTime < todayDateTime) && (PURdyDateTime > NMLnGUARANTEEcutoffTime)))
                    {
                        if (p1p2 == 1)
                        {
                            p1purdyOk = true;
                            p1dfOk = true;
                            p1dtOk = true;
                            p2purdyOk = true;
                            p2dfOk = true;
                            p2dtOk = true;
                        }
                        else if (p1p2 == 2)
                        {
                            p2purdyOk = true;
                            p2dfOk = true;
                            p2dtOk = true;
                        }
                    } else if (PURdyDateTime.Date < todayDateTime.Date)
                    {
                        if (p1p2 == 1)
                        {
                            p1purdyOk = false;
                            p1dfOk = false;
                            p1dtOk = false;
                            p2purdyOk = false;
                            p2dfOk = false;
                            p2dtOk = false;
                        }
                        else
                        {
                            p2purdyOk = false;
                            p2dfOk = false;
                            p2dtOk = false;
                        }
                    } else if ((PURdyDateTime.Date == todayDateTime.Date) &&
                        (todayDateTime > NMLnGUARANTEEcutoffTime) &&
                        (PURdyDateTime < todayDateTime))
                    {
                        if (p1p2 == 1)
                        {
                            p1purdyOk = false;
                            p1dfOk = false;
                            p1dtOk = false;
                            p2purdyOk = false;
                            p2dfOk = false;
                            p2dtOk = false;
                        }
                        else
                        {
                            p2purdyOk = false;
                            p2dfOk = false;
                            p2dtOk = false;
                        }
                    } else if ((PURdyDateTime.Date == todayDateTime.Date) &&
                        (todayDateTime < NMLnGUARANTEEcutoffTime) &&
                        (PURdyDateTime < todayDateTime))
                    {
                        if (p1p2 == 1)
                        {
                            p1purdyOk = false;
                            p1dfOk = false;
                            p1dtOk = false;
                            p2purdyOk = false;
                            p2dfOk = false;
                            p2dtOk = false;
                        }
                        else
                        {
                            p2purdyOk = false;
                            p2dfOk = false;
                            p2dtOk = false;
                        }
                    }

                }
                else if (job.ExpressType == eExpressType.etTwoHour)
                {
                    NMLnGUARANTEEcutoffTimeTS = new System.TimeSpan(15, 01, 0);
                    PURdyDateTime = DateTime.Parse(lblPURdy.Text);
                    NMLnGUARANTEEcutoffTime = todayDateTime.Date;
                    NMLnGUARANTEEcutoffTime = NMLnGUARANTEEcutoffTime.Date + NMLnGUARANTEEcutoffTimeTS;

                    if ((PURdyDateTime.Date == todayDateTime.Date) &&
                        (todayDateTime < NMLnGUARANTEEcutoffTime) &&
                        (PURdyDateTime < NMLnGUARANTEEcutoffTime))
                    {
                        if (p1p2 == 1)
                        {
                            p1purdyOk = true;
                            p1dfOk = true;
                            p1dtOk = true;
                            p2purdyOk = true;
                            p2dfOk = true;
                            p2dtOk = true;
                        }
                        else if (p1p2 == 2)
                        {
                            p2purdyOk = true;
                            p2dfOk = true;
                            p2dtOk = true;
                        }
                    } else if ((PURdyDateTime.Date == todayDateTime.Date) &&
                        ((todayDateTime > NMLnGUARANTEEcutoffTime) && (todayDateTime < todayPUcutoffTime)) &&
                        ((PURdyDateTime < todayDateTime) && (PURdyDateTime > NMLnGUARANTEEcutoffTime)))
                    {
                        if (p1p2 == 1)
                        {
                            p1purdyOk = true;
                            p1dfOk = true;
                            p1dtOk = true;
                            p2purdyOk = true;
                            p2dfOk = true;
                            p2dtOk = true;
                        }
                        else if (p1p2 == 2)
                        {
                            p2purdyOk = true;
                            p2dfOk = true;
                            p2dtOk = true;
                        }
                    } else if (PURdyDateTime.Date < todayDateTime.Date)
                    {
                        if (p1p2 == 1)
                        {
                            p1purdyOk = false;
                            p1dfOk = false;
                            p1dtOk = false;
                            p2purdyOk = false;
                            p2dfOk = false;
                            p2dtOk = false;
                        }
                        else
                        {
                            p2purdyOk = false;
                            p2dfOk = false;
                            p2dtOk = false;
                        }
                    } else if ((PURdyDateTime.Date == todayDateTime.Date) &&
                        (todayDateTime > NMLnGUARANTEEcutoffTime) &&
                        (PURdyDateTime < todayDateTime))
                    {
                        if (p1p2 == 1)
                        {
                            p1purdyOk = false;
                            p1dfOk = false;
                            p1dtOk = false;
                            p2purdyOk = false;
                            p2dfOk = false;
                            p2dtOk = false;
                        }
                        else
                        {
                            p2purdyOk = false;
                            p2dfOk = false;
                            p2dtOk = false;
                        }
                    } else if ((PURdyDateTime.Date == todayDateTime.Date) &&
                        (todayDateTime < NMLnGUARANTEEcutoffTime) &&
                        (PURdyDateTime < todayDateTime))
                    {
                        if (p1p2 == 1)
                        {
                            p1purdyOk = false;
                            p1dfOk = false;
                            p1dtOk = false;
                            p2purdyOk = false;
                            p2dfOk = false;
                            p2dtOk = false;
                        }
                        else
                        {
                            p2purdyOk = false;
                            p2dfOk = false;
                            p2dtOk = false;
                        }
                    }
                }
                else if (job.ExpressType == eExpressType.etOneHour)
                {
                    NMLnGUARANTEEcutoffTimeTS = new System.TimeSpan(16, 01, 0);
                    PURdyDateTime = DateTime.Parse(lblPURdy.Text);
                    NMLnGUARANTEEcutoffTime = todayDateTime.Date;
                    NMLnGUARANTEEcutoffTime = NMLnGUARANTEEcutoffTime.Date + NMLnGUARANTEEcutoffTimeTS;

                    if ((PURdyDateTime.Date == todayDateTime.Date) &&
                        (todayDateTime < NMLnGUARANTEEcutoffTime) &&
                        (PURdyDateTime < NMLnGUARANTEEcutoffTime))
                    {
                        if (p1p2 == 1)
                        {
                            p1purdyOk = true;
                            p1dfOk = true;
                            p1dtOk = true;
                            p2purdyOk = true;
                            p2dfOk = true;
                            p2dtOk = true;
                        }
                        else if (p1p2 == 2)
                        {
                            p2purdyOk = true;
                            p2dfOk = true;
                            p2dtOk = true;
                        }
                    } else if ((PURdyDateTime.Date == todayDateTime.Date) &&
                        ((todayDateTime > NMLnGUARANTEEcutoffTime) && (todayDateTime < todayPUcutoffTime)) &&
                        ((PURdyDateTime < todayDateTime) && (PURdyDateTime > NMLnGUARANTEEcutoffTime)))
                    {
                        if (p1p2 == 1)
                        {
                            p1purdyOk = true;
                            p1dfOk = true;
                            p1dtOk = true;
                            p2purdyOk = true;
                            p2dfOk = true;
                            p2dtOk = true;
                        }
                        else if (p1p2 == 2)
                        {
                            p2purdyOk = true;
                            p2dfOk = true;
                            p2dtOk = true;
                        }
                    } else if (PURdyDateTime.Date < todayDateTime.Date)
                    {
                        if (p1p2 == 1)
                        {
                            p1purdyOk = false;
                            p1dfOk = false;
                            p1dtOk = false;
                            p2purdyOk = false;
                            p2dfOk = false;
                            p2dtOk = false;
                        }
                        else
                        {
                            p2purdyOk = false;
                            p2dfOk = false;
                            p2dtOk = false;
                        }
                    } else if ((PURdyDateTime.Date == todayDateTime.Date) &&
                        (todayDateTime > NMLnGUARANTEEcutoffTime) &&
                        (PURdyDateTime < todayDateTime))
                    {
                        if (p1p2 == 1)
                        {
                            p1purdyOk = false;
                            p1dfOk = false;
                            p1dtOk = false;
                            p2purdyOk = false;
                            p2dfOk = false;
                            p2dtOk = false;
                        }
                        else
                        {
                            p2purdyOk = false;
                            p2dfOk = false;
                            p2dtOk = false;
                        }
                    } else if ((PURdyDateTime.Date == todayDateTime.Date) &&
                        (todayDateTime < NMLnGUARANTEEcutoffTime) &&
                        (PURdyDateTime < todayDateTime))
                    {
                        if (p1p2 == 1)
                        {
                            p1purdyOk = false;
                            p1dfOk = false;
                            p1dtOk = false;
                            p2purdyOk = false;
                            p2dfOk = false;
                            p2dtOk = false;
                        }
                        else
                        {
                            p2purdyOk = false;
                            p2dfOk = false;
                            p2dtOk = false;
                        }
                    }
                }
            }

            if (p1p2 == 1 && !p1purdyOk)
                return;
            if (p1p2 == 1 && job2 == null)
                return;

            if (p1p2 == 1 && job2 != null)
            {
                CheckIfPURdyDateTimeExpired(null, job2);
            }
        } catch (Exception e)
        {
            string s = e.Message;
        }
    }

    void CheckIfDelStartDateTimeExpired(XDelServiceRef.JobInfo? job1, XDelServiceRef.JobInfo? job2)
    {
        System.TimeSpan ts;
        System.TimeSpan ts2 = new System.TimeSpan(17, 30, 0);
        DateTime DelFromDateTime = DateTime.Now;
        DateTime todayDateTime = DateTime.Now;
        Holiday v = new Holiday();
        Label? lblDelFrom = null;
        XDelServiceRef.JobInfo? job = null;
        int p1p2 = 0;
        try
        {
            job = job1 != null ? job1 : job2 != null ? job2 : null;
            lblDelFrom = job1 != null ? lblBSSummaryDLFrom : job2 != null ? lblBSSummaryDLFrom2 : null;
            p1p2 = job1 != null ? 1 : job2 != null ? 2 : 0;
            DelFromDateTime = DateTime.Parse(lblDelFrom.Text);

            if (p1p2 == 1)
            {
                if ((DelFromDateTime.Date == todayDateTime.Date) &&
                    (todayDateTime > DelFromDateTime))
                {
                    p1dfOk = false;
                    p1dtOk = false;
                    p2purdyOk = false;
                    p2dfOk = false;
                    p2dtOk = false;
                }

            }
        } catch (Exception e)
        {
            string s = e.Message;
        }
    }


    async void validateButton()
    {
        bool ok = false;
        try
        {
            ok = p1purdyOk && p1dfOk && p1dtOk && p2purdyOk && p2dfOk && p2dtOk;
            btnProceed.IsEnabled = ok;
            btnProceed.Style = ok ? (Style)Application.Current.Resources["bstyleOrangeSmall"] : (Style)Application.Current.Resources["bstyleDisabledSmall"];

            lblBSSummaryPURdyTime.Style = p1purdyOk ? (Style)Application.Current.Resources["textStyle15BlackStart"] : (Style)Application.Current.Resources["textStyle15RedStart"];
            lblBSSummaryDLFrom.Style = p1dfOk ? (Style)Application.Current.Resources["textStyle15BlackStart"] : (Style)Application.Current.Resources["textStyle15RedStart"];
            lblBSSummaryDLBy.Style = p1dtOk ? (Style)Application.Current.Resources["textStyle15BlackStart"] : (Style)Application.Current.Resources["textStyle15RedStart"];
            lblBSSummaryDLBy2.Style = p1dtOk ? (Style)Application.Current.Resources["textStyle15BlackStart"] : (Style)Application.Current.Resources["textStyle15RedStart"];
            lblBSSummaryPURdyTime2.Style = p2purdyOk ? (Style)Application.Current.Resources["textStyle15BlackStart"] : (Style)Application.Current.Resources["textStyle15RedStart"];
            lblBSSummaryDLFrom2.Style = p2dfOk ? (Style)Application.Current.Resources["textStyle15BlackStart"] : (Style)Application.Current.Resources["textStyle15RedStart"];
            lblBSSummaryDLBy3.Style = p2dtOk ? (Style)Application.Current.Resources["textStyle15BlackStart"] : (Style)Application.Current.Resources["textStyle15RedStart"];
            lblBSSummaryDLBy4.Style = p2dtOk ? (Style)Application.Current.Resources["textStyle15BlackStart"] : (Style)Application.Current.Resources["textStyle15RedStart"];

            if (!ok)
                await DisplayAlertAsync("", "Selected Date/Time expired. Please amend.", "OK");
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

    private Task ShowAlertSafe(string title, string message, string button = "OK")
    {
        if (MainThread.IsMainThread)
            return DisplayAlertAsync(title, message, button);

        return MainThread.InvokeOnMainThreadAsync(() =>
            DisplayAlertAsync(title, message, button));
    }

}