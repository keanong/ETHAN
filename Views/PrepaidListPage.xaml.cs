using ETHAN.classes;
using ETHAN.Network;
using ETHAN.ProgressDialog;
using ETHAN.ViewModel;
using ETHAN.XDelSys;
using System.Collections.ObjectModel;
using System.Data;
using XDelServiceRef;

namespace ETHAN.Views;

[QueryProperty(nameof(Vmm), "vmm")] // Add a QueryProperty to handle the navigation parameter
//[QueryProperty(nameof(LOGININFO), "LOGININFO")] // Add a QueryProperty to handle the navigation parameter



public partial class PrepaidListPage : ContentPage
{
    //private XWSSoapClient xs = new XWSSoapClient(XWSSoapClient.EndpointConfiguration.XWSSoap);
    private XOEWSSoapClient xs = new XOEWSSoapClient(XOEWSSoapClient.EndpointConfiguration.XOEWSSoap);
    //private ProgressDialogService _progressService;
    private readonly IProgressDialogService _progressService;
    private bool _isInitialized = false;

    private PrepaidVM? vm;

    public PrepaidVM Vmm
    {
        set
        {
            if (value != null)
            {
                vm = value;
                BindingContext = vm;
            }
            else if (vm == null)
            {
                vm = new PrepaidVM();
                BindingContext = vm;
            }
        }
    }


    private LoginInfo? logininfo;
    /*public LoginInfo? LOGININFO
    {
        set
        {
            logininfo = value;
            if (logininfo != null && logininfo.PrePaidBalance != null)
                lblAvailBal.Text = logininfo.PrePaidBalance == null ? "$0.00" : "$" + logininfo.PrePaidBalance.Value.ToString("F2");
        }
    }*/

    public PrepaidListPage(IProgressDialogService progressService)
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
            if (_isInitialized) return;

            logininfo = AppSession.logininfo;
            if (logininfo == null)
                return; // or await DisplayAlert("Error", "Login info missing.", "OK");

            if (logininfo?.clientInfo == null ||
            string.IsNullOrEmpty(logininfo.clientInfo.Web_UID) ||
            logininfo.clientInfo.CAIDX <= 0 || logininfo?.ContactLvlSettingsInfo == null || vm == null)
            {
                await DisplayAlertAsync("Session expired", "Please Login again.", "OK");
                await common.BackToLogin();
                return;
            }

            if (logininfo != null && logininfo.PrePaidBalance != null)
                lblAvailBal.Text = logininfo.PrePaidBalance == null ? "$0.00" : "$" + logininfo.PrePaidBalance.Value.ToString("F2");

            _isInitialized = true;
            await Task.Yield();
            //ClientInfo ci = logininfo.clientInfo;
            //SettingsInfo customersets = logininfo.ContactLvlSettingsInfo;
            //await GetPrepaidList(ci, customersets, DateTime.MinValue, DateTime.MinValue, "");
        }
        catch (Exception e)
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
    
    async void BackToHomePage()
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
                        { "BARCODE", null }
                    });*/
                await Shell.Current.GoToAsync("///CardShellPage", new Dictionary<string, object>
                    {
                        { "LOGININFO", logininfo }
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
            BackToHomePage();
        } catch (Exception e)
        {
            string s = e.Message;
        }
        return true;
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

    async void btnSOA_Clicked(System.Object sender, System.EventArgs e)
    {
        try
        {
            if (NetworkHelper.IsDisconnected())
            {
                await DisplayAlertAsync("No Internet Connection", "Please check your internet connection and try again.", "OK");
                return;
            }

            if (logininfo?.clientInfo == null || (logininfo != null && logininfo.clientInfo == null) ||
                (logininfo != null && logininfo.clientInfo != null && string.IsNullOrEmpty(logininfo.clientInfo.Web_UID)))
            {
                await DisplayAlertAsync("Session expired", "Please Login again.", "OK");
                await common.BackToLogin();
                return;
            }

            await showProgress_Dialog("Processing...");

            ClientInfo ci = logininfo.clientInfo;

            XDelServiceRef.XWSBase xb = await Task.Run(async () =>
            {
                return await xs.Print_PrePaidSOAAsync(ci.Web_UID);
            });

            await Task.Delay(500);

            await closeProgress_dialog();

            if (xb != null )
                await AlertService.ShowError("", xb.Message);

        }
        catch (Exception ex)
        {
            string s = ex.Message;
        }
    }

    async void btnTopUp_Clicked(System.Object sender, System.EventArgs e)
    {
        try
        {
            if (NetworkHelper.IsDisconnected())
            {
                await DisplayAlertAsync("No Internet Connection", "Please check your internet connection and try again.", "OK");
                return;
            }

            if (logininfo?.clientInfo == null || (logininfo != null && logininfo.clientInfo == null) ||
                (logininfo != null && logininfo.clientInfo != null && string.IsNullOrEmpty(logininfo.clientInfo.Web_UID)) || 
                (logininfo != null && logininfo.clientInfo != null && string.IsNullOrEmpty(logininfo.clientInfo.Web_UID) && logininfo.ContactLvlSettingsInfo != null))
            {
                await DisplayAlertAsync("Session expired", "Please Login again.", "OK");
                await common.BackToLogin();
                return;
            }

            string amt = txtAmt.Text;

            if (string.IsNullOrEmpty(amt))
            {
                await DisplayAlertAsync("", "Please enter a value for Top Up Amount.", "OK");
                return;
            }

            decimal value = Decimal.Parse(amt);
            if (value == 0)
            {
                await DisplayAlertAsync("", "The specified amount is below the minimum required.", "OK");
                return;
            }

            txtAmt.Text = "";

            await showProgress_Dialog("Processing...");

            ClientInfo ci = logininfo.clientInfo;

            XDelServiceRef.XWSBase xb = await Task.Run(async () =>
            {
                return await xs.Create_PrePaidItemAsync(ci.Web_UID, value);
            });

            if (xb != null && xb.Status == 0)
            {
                SettingsInfo customersets = logininfo.ContactLvlSettingsInfo;
                await GetPrepaidList(ci, customersets, DateTime.MinValue, DateTime.MinValue, "", false);
            }

            await Task.Delay(500);

            await closeProgress_dialog();

            if (xb != null && xb.Status == 0)
                await AlertService.ShowError("", "Top Up Request successful.");
            else if (xb != null && xb.Status != 0)
                await AlertService.ShowError("", xb.Message);
            else
                await AlertService.ShowError("", "Error processing this request");

        }
        catch (Exception ex)
        {
            string s = ex.Message;
        }
    }

    async void btnDTSearch_Clicked(System.Object sender, System.EventArgs e)
    {
        try
        {
            if (NetworkHelper.IsDisconnected())
            {
                await DisplayAlertAsync("No Internet Connection", "Please check your internet connection and try again.", "OK");
                return;
            }

            DateTime fromdate = dpFrom.Date ?? DateTime.Now;
            DateTime toDate = dpTo.Date ?? DateTime.Now;
            if (logininfo?.clientInfo == null ||
            string.IsNullOrEmpty(logininfo.clientInfo.Web_UID) ||
            logininfo.clientInfo.CAIDX <= 0 || logininfo?.ContactLvlSettingsInfo == null || vm == null)
            {
                await DisplayAlertAsync("Session expired", "Please Login again.", "OK");
                await common.BackToLogin();
                return;
            }

            ClientInfo ci = logininfo.clientInfo;
            SettingsInfo customersets = logininfo.ContactLvlSettingsInfo;
            await GetPrepaidList(ci, customersets, fromdate, toDate, "");
        }
        catch (Exception ex)
        {
            string s = ex.Message;
            await closeProgress_dialog();
            await DisplayAlertAsync("Exception", ex.Message, "OK");
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

            DateTime fromdate = DateTime.MinValue;
            DateTime toDate = DateTime.MinValue;
            string search = txtRefNo.Text;
            if (logininfo?.clientInfo == null ||
            string.IsNullOrEmpty(logininfo.clientInfo.Web_UID) ||
            logininfo.clientInfo.CAIDX <= 0 || logininfo?.ContactLvlSettingsInfo == null || vm == null)
            {
                await DisplayAlertAsync("Session expired", "Please Login again.", "OK");
                await common.BackToLogin();
                return;
            }

            ClientInfo ci = logininfo.clientInfo;
            SettingsInfo customersets = logininfo.ContactLvlSettingsInfo;
            await GetPrepaidList(ci, customersets, fromdate, toDate, search);
        }
        catch (Exception ex)
        {
            string s = ex.Message;
            await closeProgress_dialog();
            await DisplayAlertAsync("Exception", ex.Message, "OK");
        }
    }

    private async Task GetPrepaidList(ClientInfo ci, SettingsInfo customersets, DateTime from, DateTime to, string inv, bool showProgress = true)
    {
        DataTable? rdt;
        DataSet ds;
        DataRow r;
        string formatdate = "d/MM/yyyy";
        String AFromDate = "";
        String AToDate = "";       
        String invv = "";
        long ACLIDX = 0;
        long ACAIDX = 0;
        long ACNIDX = 0;
        XDelServiceRef.PrePaidInfo ppi = null;
        try
        {
            if (NetworkHelper.IsDisconnected())
            {
                await DisplayAlertAsync("No Internet Connection", "Please check your internet connection and try again.", "OK");
                return;
            }

            if (showProgress)
                await showProgress_Dialog("Processing...");

            ACLIDX = ci != null ? ci.CLIDX : customersets != null ? customersets.XDelOnlineSettings.ClientIDX : 0;
            ACAIDX = ci != null && customersets != null && customersets.XDelOnlineSettings.JobViewLevel == XDelServiceRef.TViewLevel.vlAddress ? ci.CAIDX : 0;
            ACNIDX = ci != null && customersets != null && customersets.XDelOnlineSettings.JobViewLevel == XDelServiceRef.TViewLevel.vlContact ? ci.CNIDX : 0;

            if (from == DateTime.MinValue && to == DateTime.MinValue)
            {
                from = new DateTime(DateTime.Now.Year, 1, 1);
                to = new DateTime(DateTime.Now.Year, 12, 31);
            }

            if (!String.IsNullOrEmpty(inv))
            {
                string[] invarr = inv.ToUpper().Replace("PP", "").Replace(" ", "").Trim().Split(',');
                if (invarr.Length > 0)
                {
                    for (int i = 0; i <= invarr.Length - 1; i++)
                    {
                        invv += (i < invarr.Length - 1) ? invarr[i] + "," : invarr[i];
                    }
                }
            }

            if (!string.IsNullOrEmpty(invv))
            {
                ppi = await Task.Run(async () =>
                {
                    return await xs.Get_PrePaidInfo_SearchAsync(ci.Web_UID, invv);
                });
            } else
            {
                ppi = await Task.Run(async () =>
                {
                    return await xs.Get_PrePaidInfo_DateRangeAsync(ci.Web_UID, from, to);
                });
            }

            if (ppi != null && ppi.Status == 0)
            {
                if (ppi.PrePaidItems != null && ppi.PrePaidItems.Length > 0)
                {
                    vm.newItems();

                    foreach (var pi in ppi.PrePaidItems)
                    {
                        vm.addItem(new PrepaidItem
                        {
                            IDX = pi.IDX,
                            CL_CONTACTIDX = pi.ContactIDX,
                            INV = pi.InvoiceNumber,
                            DATEISSUED = pi.DateIssued != DateTime.MinValue ? pi.DateIssued.ToString(formatdate) : "",
                            AMT = (string)"$" + (Math.Round((decimal)pi.Amount, 2)).ToString(),
                            CURR_STATUS = pi.Status == PPStatus.ppCanUse ? "Processed" : "Unprocessed",
                            HAS_INVOICEPDF = pi.Has_InvoicePDF ? 1 : 0,
                            HAS_RECEIPTPDF = pi.Has_ReceiptPDF ? 1 : 0,
                            PrePaidInvoicePDFURL = pi.Has_InvoicePDF && !string.IsNullOrEmpty(pi.InvoicePDFURL) ? pi.InvoicePDFURL : "",
                            PrePaidReceiptPDFURL = pi.Has_ReceiptPDF && !string.IsNullOrEmpty(pi.ReceiptPDFURL) ? pi.ReceiptPDFURL : ""
                        });
                    }

                    _isDateAscending = true;
                    OnSortByDateClicked(null, null);

                    if (showProgress)
                        await closeProgress_dialog();
                } else
                {
                    if (vm != null)
                        vm.Items.Clear();
                    if (showProgress)
                        await closeProgress_dialog();

                    await DisplayAlertAsync("", "No record found.", "OK");
                }
            } else if ((ppi != null && ppi.Status != 0) || (ppi == null))
            {
                if (vm != null)
                    vm.Items.Clear();
                if (showProgress)
                    await closeProgress_dialog();

                if (ppi != null)
                    await DisplayAlertAsync("", !string.IsNullOrEmpty(ppi.Message) ? ppi.Message : "Error retrieving data.", "OK");
                else
                    await DisplayAlertAsync("", "Error retrieving data.", "OK");
            }
        }
        catch (Exception e)
        {
            string s = e.Message;
            if (showProgress)
                await closeProgress_dialog();

            await DisplayAlertAsync("", "Error retrieving data.", "OK");
        }
    }

    private bool _isInvoiceAscending = true;
    private bool _isDateAscending = true;
    private bool _isAmtAscending = true;
    private bool _isStatusAscending = true;

    private void OnSortByInvoiceClicked(object sender, EventArgs e)
    {
        try
        {
            if (vm?.Items == null || vm.Items.Count == 0) return;

            var sorted = _isInvoiceAscending
                ? vm.Items.OrderBy(x => x.INV)
                : vm.Items.OrderByDescending(x => x.INV);

            vm.newItems();
            foreach (var item in sorted)
            {
                vm.addItem(item);
            }

            cvList.ItemsSource = vm.Items;
            _isInvoiceAscending = !_isInvoiceAscending;
        }
        catch (Exception ex)
        {
            string s = ex.Message;
        }
    }

    private void OnSortByDateClicked(object sender, EventArgs e)
    {
        try
        {
            if (vm?.Items == null || vm.Items.Count == 0) return;

            var sorted = _isDateAscending
                ? vm.Items.OrderBy(x => x.DATEISSUED)
                : vm.Items.OrderByDescending(x => x.DATEISSUED);

            vm.newItems();
            foreach (var item in sorted)
            {
                vm.addItem(item);
            }

            cvList.ItemsSource = vm.Items;
            _isDateAscending = !_isDateAscending;
        }
        catch (Exception ex)
        {
            string s = ex.Message;
        }
    }

    private void OnSortByAmtClicked(object sender, EventArgs e)
    {
        try
        {
            if (vm?.Items == null || vm.Items.Count == 0) return;

            var sorted = _isAmtAscending
                ? vm.Items.OrderBy(x => x.AMT)
                : vm.Items.OrderByDescending(x => x.AMT);

            vm.newItems();
            foreach (var item in sorted)
            {
                vm.addItem(item);
            }

            cvList.ItemsSource = vm.Items;
            _isAmtAscending = !_isAmtAscending;
        }
        catch (Exception ex)
        {
            string s = ex.Message;
        }
    }

    private void OnSortByStatusClicked(object sender, EventArgs e)
    {
        try
        {
            if (vm?.Items == null || vm.Items.Count == 0) return;

            var sorted = _isStatusAscending
                ? vm.Items.OrderBy(x => x.CURR_STATUS)
                : vm.Items.OrderByDescending(x => x.CURR_STATUS);

            vm.newItems();
            foreach (var item in sorted)
            {
                vm.addItem(item);
            }

            cvList.ItemsSource = vm.Items;
            _isStatusAscending = !_isStatusAscending;
        }
        catch (Exception ex)
        {
            string s = ex.Message;
        }
    }

    private async void tabOnItemTap(object sender, EventArgs e)
    {
        try
        {
            var tappedLayout = sender as Grid;
            if (tappedLayout != null)
            {
                var selectedItem = tappedLayout.BindingContext as PrepaidItem;

                if (selectedItem != null && selectedItem is PrepaidItem && BindingContext != null && BindingContext is PrepaidVM vm)
                {
                    vm.newOptions();

                    if (selectedItem.HAS_INVOICEPDF == 1)
                        vm.addOptions(new ManageJobOptionSelector
                        {
                            value = "INVOICEPDF",
                            DispText = "Invoice PDF",
                            PrePaidInvoicePDFURL = selectedItem.PrePaidInvoicePDFURL
                        });

                    if (selectedItem.HAS_RECEIPTPDF == 1)
                        vm.addOptions(new ManageJobOptionSelector
                        {
                            value = "RECEIPTPDF",
                            DispText = "Receipt PDF",
                            PrePaidReceiptPDFURL = selectedItem.PrePaidReceiptPDFURL
                        });

                    if (vm.Options != null && vm.Options.Count > 0)
                    {
                        removeTapGestureRecognizer();

                        cvOptions.ItemsSource = vm.Options;
                        await BSOptions.OpenBottomSheet(false);
                        BSOptions.IsVisible = true;
                        BSOptions.isShowing = true;
                    }
                }
            }

        } catch (Exception ex)
        {
            string gs = ex.Message;
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

    private void NumericEntry_TextChanged(object sender, TextChangedEventArgs e)
    {
        var entry = (Entry)sender;
        // Allow only digits and commas
        string validText = new string(e.NewTextValue?.Where(c => char.IsDigit(c) || c == ',').ToArray());

        if (entry.Text != validText)
            entry.Text = validText;
    }


    #region OPTIONS

    private async void cvOptionsOnItemTap(object sender, EventArgs e)
    {
        long JobsIDX = 0;
        string value, URL = "";
        string[] allowedOpenURLValues = { "INVOICEPDF", "RECEIPTPDF" };
        try
        {
            var tappedLayout = sender as Grid;
            if (tappedLayout != null)
            {
                var selectedItem = tappedLayout.BindingContext as ManageJobOptionSelector;

                //// Ensure the ViewModel exists -- if (BindingContext is ManageJobPageVM vm)
                if (selectedItem != null && selectedItem is ManageJobOptionSelector && BindingContext is PrepaidVM vm)
                {
                    value = selectedItem.value;
                    bool openurl = allowedOpenURLValues.Any(v => v.Equals(value, StringComparison.OrdinalIgnoreCase));
                    if (openurl)
                        URL =
                            string.Equals(value, "INVOICEPDF", StringComparison.OrdinalIgnoreCase) && !string.IsNullOrEmpty(selectedItem.PrePaidInvoicePDFURL) ? selectedItem.PrePaidInvoicePDFURL :
                            string.Equals(value, "RECEIPTPDF", StringComparison.OrdinalIgnoreCase) && !string.IsNullOrEmpty(selectedItem.PrePaidReceiptPDFURL) ? selectedItem.PrePaidReceiptPDFURL :
                            "";

                    if (openurl && !string.IsNullOrEmpty(URL))
                    {
                        if (NetworkHelper.IsDisconnected())
                        {
                            await DisplayAlertAsync("No Internet Connection", "Please check your internet connection and try again.", "OK");
                            return;
                        }

                        btnCXBSOption_Clicked(null, null);
                        await Task.Yield();
                        await OpenInBrowser(URL); //tested ok
                        ////await OpenWithBrowser(URL); //tested ok, but difficult to download as pdf, need to open from chrome

                        ////Navigate to this page from anywhere //tested ok
                        //await Navigation.PushAsync(new WebViewPage(URL));
                        return;
                    }
                }

            }
        }
        catch (Exception ex)
        {
            string s = ex.Message;
        }
    }

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

    #endregion

    #region Gestures

    void addTapGestureRecognizer()
    {
        try
        {
            btnTopNavBack.InputTransparent = false;

            btnSOA.InputTransparent = false;
            txtAmt.InputTransparent = false;
            btnTopUp.InputTransparent = false;

            dpFrom.InputTransparent = false;
            dpTo.InputTransparent = false;
            btnDTSearch.InputTransparent = false;

            txtRefNo.InputTransparent = false;
            btnParSearch.InputTransparent = false;

            cvList.InputTransparent = false;
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

            btnSOA.InputTransparent = true;
            txtAmt.InputTransparent = true;
            btnTopUp.InputTransparent = true;

            dpFrom.InputTransparent = true;
            dpTo.InputTransparent = true;
            btnDTSearch.InputTransparent = true;

            txtRefNo.InputTransparent = true;
            btnParSearch.InputTransparent = true;

            cvList.InputTransparent = true;
        }
        catch (Exception ex)
        {
            string s = ex.Message;
        }
    }

    #endregion

}