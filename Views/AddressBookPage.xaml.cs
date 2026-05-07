using ETHAN.classes;
using ETHAN.Network;
using ETHAN.ProgressDialog;
using ETHAN.ViewModel;
using System.Diagnostics;
using System.Runtime.Serialization;
using XDelServiceRef;

namespace ETHAN.Views;

[QueryProperty(nameof(TitleName), "titleName")]
[QueryProperty(nameof(Vmm), "vmm")] // Add a QueryProperty to handle the navigation parameter
[QueryProperty(nameof(LOGININFO), "LOGININFO")] // Add a QueryProperty to handle the navigation parameter
[QueryProperty(nameof(CRITERIA1), "CRITERIA1")] // Add a QueryProperty to handle the navigation parameter
[QueryProperty(nameof(CRITERIA2), "CRITERIA2")] // Add a QueryProperty to handle the navigation parameter
[QueryProperty(nameof(SEARCHEDADDRESS), "SEARCHEDADDRESS")] // Add a QueryProperty to handle the navigation parameter
[QueryProperty(nameof(LTR), "LTR")] // Add a QueryProperty to handle the navigation parameter


public partial class AddressBookPage : ContentPage
{
    //private XWSSoapClient xs = new XWSSoapClient(XWSSoapClient.EndpointConfiguration.XWSSoap);
    private XOEWSSoapClient xs = new XOEWSSoapClient(XOEWSSoapClient.EndpointConfiguration.XOEWSSoap);
    //private ProgressDialogService _progressService;
    private readonly IProgressDialogService _progressService;

    private string title;
    public string TitleName
    {
        set
        {
            title = value;
        }
    }

    private CreateJobVM vm;
    public CreateJobVM Vmm
    {
        set
        {
            //vm = value;
            //BindingContext = vm ?? new CreateJobVM(); // If null, create a new instance

            //if (!txtPostal.IsReadOnly && swAddress.IsToggled)
            //    txtPostalTextChangedSubscribed();
            //else
            //    txtPostalTextChangedUnSubscribed();

            vm = value ?? new CreateJobVM();
            BindingContext = vm; // If null, create a new instance
        }
    }

    private LoginInfo? logininfo;
    public LoginInfo? LOGININFO
    {
        set
        {
            logininfo = value;
            try
            {
                btnAddAddress.IsVisible = logininfo != null && logininfo.ContactLvlSettingsInfo != null && logininfo.ContactLvlSettingsInfo.XDelOnlineSettings != null
                    ? logininfo.ContactLvlSettingsInfo.XDelOnlineSettings.CanManageAddressBook : false;

            }
            catch (Exception e)
            {
                string s = e.Message;
            }
        }
    }

    private string? criteria1;
    public string? CRITERIA1
    {
        set
        {
            criteria1 = value;
        }
    }

    private string? criteria2;
    public string? CRITERIA2
    {
        set
        {
            criteria2 = value;
        }
    }

    private ManageJobPageVM.LoadTabsRequest? ltr;

    public ManageJobPageVM.LoadTabsRequest? LTR
    {
        set
        {
            ltr = value;
        }
    }

    private XDelServiceRef.AddressStructure[]? searchedAdd;
    public XDelServiceRef.AddressStructure[]? SEARCHEDADDRESS
    {
        set
        {
            searchedAdd = value;
        }
    }


    private SelectableListAddressBookVM _viewModel;
    private SelectableExpListAddressBookVM _ExpviewModel;
    private AddressBookPickerOptionsVM _options = new AddressBookPickerOptionsVM();

    public AddressBookPage(IProgressDialogService progressService)
	{
		InitializeComponent();
        _progressService = progressService;
        try
        {
            if (criteria1 != null)
            {
                AddressBookPickerOptions op = _options.Options.FirstOrDefault(o => o.Name == "C");
                if (op != null)
                    _options.SelectedOption = op;
            }

            ddlOptions.SelectedIndexChanged -= OnPickerSelectedIndexChanged;
            ddlOptions.BindingContext = _options;
            ddlOptions.SelectedIndexChanged += OnPickerSelectedIndexChanged;

            Console.WriteLine($"Options count: {_options.Options?.Count}");
            Console.WriteLine($"Picker BindingContext: {ddlOptions.BindingContext?.GetType().Name}");

            if (criteria2 != null)
                txtKeyword.Text = criteria2;

            _ExpviewModel = new SelectableExpListAddressBookVM();
            cvAddress.BindingContext = _ExpviewModel;

            Shell.SetTabBarIsVisible(this, false);
        } catch (Exception e)
        {
            string s = e.Message;
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
		try {

            /*await Task.Yield();
            BindingContext = new SelectableExpListAddressBookVM();
            if (BindingContext != null && BindingContext is SelectableExpListAddressBookVM sm)
            {
                int csm = sm.Items.Count;
                ObservableCollection<ETHAN.classes.AddressHeader> ah = cvAddress.ItemsSource != null
                    && cvAddress.ItemsSource is ObservableCollection<ETHAN.classes.AddressHeader> ? 
                    (ObservableCollection<ETHAN.classes.AddressHeader>)cvAddress.ItemsSource : null;
                if (ah != null)
                {
                    int asm = ah.Count;
                }
                    
            }*/

            txtPostalTextChangedUnSubscribed();

            if (!txtPostal.IsReadOnly && swAddress.IsToggled)
                txtPostalTextChangedSubscribed();
                
        } catch (Exception e)
		{
			string s = e.Message;
		}
	}

    protected override void OnDisappearing()
    {
        base.OnDisappearing();

        // Unsubscribe TextChanged
        txtPostalTextChangedUnSubscribed();

        // Unsubscribe ViewModel events
        UnsubscribeExpViewModel();

        // Unsubscribe Picker
        ddlOptions.SelectedIndexChanged -= OnPickerSelectedIndexChanged;

        // Unsubscribe switch toggles
        swAddress.Toggled -= swAddressonToggle;
        swContact.Toggled -= swContactonToggle;

        // Close any open bottom sheets
        _ = CloseBottomSheetSafely(BSContact);
        _ = CloseBottomSheetSafely(BSADDRESS);

        // Clear CollectionView sources
        cvAddress.BindingContext = null;
        cvLocationType.ItemsSource = null;
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

    async void BackToAddress()
    {
        bool BSContactnotopen = ((BSContact == null) || (BSContact != null && !BSContact.isShowing));
        bool BSAddressnotopen = ((BSADDRESS == null) || (BSADDRESS != null && !BSADDRESS.isShowing));

        //if (BSContactnotopen && BSAddressnotopen)
        //{
        //    MainThread.BeginInvokeOnMainThread(async () =>
        //    {
        //        await Shell.Current.GoToAsync("..", new Dictionary<string, object>
        //            {
        //                { "vmm", vm },
        //                { "titleName", title },
        //                {"LOGININFO",  logininfo},
        //                { "LTR", ltr }
        //            });
        //    }
        //        );
        //}

        if (BSContactnotopen && BSAddressnotopen)
            await Shell.Current.GoToAsync("..");


    }

    async Task SaveToAddress()
    {
        //await Shell.Current.GoToAsync("AddressPage", new Dictionary<string, object>
        //            {
        //                { "vmm", vm },
        //                { "titleName", title },
        //                {"LOGININFO",  logininfo},
        //                { "LTR", ltr }
        //            });

        await Shell.Current.GoToAsync("..");
    }

    async void Back(System.Object sender, System.EventArgs e)
    {
        try
        {
            BackToAddress();
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

        BackToAddress();
        return true;
    }

    async void Add_Address(System.Object sender, System.EventArgs e)
    {
        try
        {
            if (logininfo != null && logininfo.clientInfo != null && !String.IsNullOrEmpty(logininfo.clientInfo.Web_UID) && logininfo.ContactLvlSettingsInfo != null)
            {
                removeTapGestureRecognizer();
                AddEditAddressFieldsTapGestureRecognizer();
                txtPostalTextChangedUnSubscribed();

                lblBSADDRESSCAIDX.Text = "";
                txtPostal.Text = "";
                txtBlock.Text = "";
                txtUnit.Text = "";
                txtStreet.Text = "";
                txtBldg.Text = "";
                txtCompany.Text = "";
                swAddress.Toggled -= swAddressonToggle;
                swAddress.IsToggled = true;
                swAddress.Toggled += swAddressonToggle;
                swAddress.IsVisible = false;
                lblBSAddressActive.IsVisible = false;

                lblSelectedLocationType.Text = "";

                await loadLocationType();

                txtPostal.IsReadOnly = false;
                txtBlock.IsReadOnly = false;
                txtUnit.IsReadOnly = false;
                txtStreet.IsReadOnly = true;
                txtBldg.IsReadOnly = false;
                txtPostalTextChangedSubscribed();

                btnSaveBSADDRESS.IsVisible = true;
                btnUpdateBSADDRESS.IsVisible = false;

                await Task.Delay(100);
                await BSADDRESS.OpenBottomSheet(false);
                BSADDRESS.HeaderText = "ADD NEW ADDRESS";
                BSADDRESS.IsVisible = true;
                BSADDRESS.isShowing = true;
            }
        } catch (Exception ex)
        {
            string s = ex.Message;
            addTapGestureRecognizer();
        }
    }

    void Add_Contact(System.Object sender, System.EventArgs e)
    {
        try
        {
            addEditContactFieldsTapGestureRecognizer();
            swContact.Toggled -= swContactonToggle;
            swContact.IsToggled = true;
            swContact.Toggled += swContactonToggle;
            swContact.IsVisible = false;
            lblBSContactActive.IsVisible = false;
            //btnAddContact.IsVisible = false;
            lblBSContactCNIDX.Text = "";

            btnSaveBSContact.IsVisible = true;
            btnUpdateBSContact.IsVisible = false;

            txtContactName.Text = "";
            txtDept.Text = "";
            txtTel.Text = "";
            txtHp.Text = "";
            BSContact.HeaderText = "ADD NEW CONTACT";

        } catch (Exception ex)
        {
            string s = ex.Message;
        }
    }

    async void btnCancel_Clicked(System.Object sender, System.EventArgs e)
    {
        try
        {
            BackToAddress();
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

            if (ddlOptions.SelectedIndex >= 0)
                await getSearchAddressExp();
            
        }
        catch (Exception ex)
        {
            string s = ex.Message;
        }
    }

    private void OnRadioButtonCheckedChanged(object sender, CheckedChangedEventArgs e)
    {
        if (e.Value) // Only process when checked (not unchecked)
        {
            //RadioButton radioButton = (RadioButton)sender;
            //SelectableItemAddressBook item = (SelectableItemAddressBook)radioButton.BindingContext;

            //var viewModel = (SelectableListAddressBookVM)cvAddress.BindingContext;
            //viewModel.SelectedItem = item;

            //// Update the model to reflect the new selection
            //foreach (var radioItem in viewModel.Items)
            //{
            //    if (radioItem != item && radioItem.IsSelected)
            //    {
            //        radioItem.IsSelected = false;
            //    }
            //}

            //common.showAlert(item.Text);

            var radioButton = (RadioButton)sender;
            var item = (SelectableItemAddressBook)radioButton.BindingContext;

            common.showAlert("Editing\r\n\r\n " + item.Text);
        }
    }

    private async void OnSelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (e.CurrentSelection.Count > 0)
        {
            // The ViewModel will handle updating IsSelected via the two-way binding
            // on SelectedItem, which will trigger the DataTriggers to change the style
            try
            {
                var selectedItem = e.CurrentSelection[0] as SelectableItemAddressBook;
                if (selectedItem == null)
                    return;

                // Update the ViewModel's SelectedItem, which will update IsSelected
                _viewModel.SelectedItem = selectedItem;

                // Ensure the radio button is checked
                selectedItem.IsSelected = true;

                XDelServiceRef.AddressStructure ads = (XDelServiceRef.AddressStructure)selectedItem.addressStructure;
                if (vm != null && ads != null)
                {
                    if (vm.AddressMode == 1)
                        vm.ColAddress = ads;
                    if (vm.AddressMode == 2)
                        vm.DelAddress = ads;
                    if (vm.AddressMode == 3)
                        vm.RtnAddress = ads;

                    await SaveToAddress();
                }
            } catch (Exception ex)
            {
                string s = ex.Message;
            }
        }
    }

    async void OnPickerSelectedIndexChanged(object sender, EventArgs e)
    {
        try
        {
            var picker = (Picker)sender;
            int selectedIndex = picker.SelectedIndex;

            if (_ExpviewModel != null)
            {
                _ExpviewModel.Items.Clear();
                _ExpviewModel.setsearchedAddress(null);
            }

            if (selectedIndex >= 0)
            {
                if (NetworkHelper.IsDisconnected())
                {
                    await DisplayAlertAsync("No Internet Connection", "Please check your internet connection and try again.", "OK");
                    return;
                }

                txtKeyword.IsEnabled = true;
                txtKeyword.Text = "";
                await getSearchAddressExp();
            } else
            {
                txtKeyword.IsEnabled = false;
                txtKeyword.Text = "";
                cvAddress.ItemsSource = null;
            }
        } catch (Exception ex)
        {
            string s = ex.Message;
        }
    }

    public static T DeepClone<T>(T obj)
    {
        using (var ms = new MemoryStream())
        {
            var serializer = new DataContractSerializer(typeof(T));
            serializer.WriteObject(ms, obj);
            ms.Position = 0;
            return (T)serializer.ReadObject(ms);
        }
    }

    // Named event handlers - these are stable method references
    // unlike lambdas, -= will correctly match += 
    private async void OnChildTapped(object s, AddressChild child)
    {
        await SetAddressToAddressPage(child);
    }

    private async void OnChildEdit(object s, AddressChild child)
    {
        await EditContact(child);
    }

    private async void OnHeaderEdit(object s, AddressHeader header)
    {
        await EditAddress(header);
    }

    private async void OnHeaderContactAdd(object s, AddressHeader header)
    {
        await AddContact(header);
    }

    private void UnsubscribeExpViewModel()
    {
        if (_ExpviewModel == null) return;
        _ExpviewModel.ChildTappedEvent -= OnChildTapped;
        _ExpviewModel.ChildEditEvent -= OnChildEdit;
        _ExpviewModel.HeaderEditEvent -= OnHeaderEdit;
        _ExpviewModel.HeaderContactAddEvent -= OnHeaderContactAdd;
    }

    private void SubscribeExpViewModel()
    {
        if (_ExpviewModel == null) return;
        _ExpviewModel.ChildTappedEvent += OnChildTapped;
        _ExpviewModel.ChildEditEvent += OnChildEdit;
        _ExpviewModel.HeaderEditEvent += OnHeaderEdit;
        _ExpviewModel.HeaderContactAddEvent += OnHeaderContactAdd;
    }

    private async Task getSearchAddressExp()
    {
        AddressBookPickerOptions? abpo = null;
        abpo = (AddressBookPickerOptions)ddlOptions.SelectedItem;
        string cri1 = abpo != null ? abpo.Value : "- SELECT -"; //on purpose here to get 0 results if null
        string cri2 = txtKeyword.Text.ToUpper();
        XDelServiceRef.AddressStructure[]? list = null;
        XDelServiceRef.AddressStructure[]? listtemp = null;
        XDelServiceRef.AddressStructure temp = null;
        XDelServiceRef.AddressStructure[]? listfinal = null;
        List<XDelServiceRef.AddressStructure> list2 = null;
        XDelServiceRef.AddressStructure ads = null;
        XDelServiceRef.AddressStructure nads = null;
        XDelServiceRef.AddressStructure cads = null;
        XDelServiceRef.ContactStructure cts = null;
        XDelServiceRef.AddressBook ab = null;
        string errmsg = "Session expired. Please Login again.";
        string errmsg2 = "No result found.";
        try
        {
            if (logininfo != null && logininfo.clientInfo != null && !String.IsNullOrEmpty(logininfo.clientInfo.Web_UID))
            {
                ClientInfo ci = logininfo.clientInfo;
                removeTapGestureRecognizer();
                await showProgress_Dialog("Processing...");

                /*if (_ExpviewModel != null)
                    list = _ExpviewModel.getsearchedAddress();*/
                if (_ExpviewModel != null)
                {
                    UnsubscribeExpViewModel(); // clean up before clearing
                    _ExpviewModel.Items.Clear();
                    _ExpviewModel.setsearchedAddress(null);
                    _ExpviewModel.cri1 = null;
                    _ExpviewModel.cri2 = null;
                    cvAddress.BindingContext = null;
                }

                if (list == null)
                {
                    XDelServiceRef.AddressBook searchedAddress = await Task.Run(async () =>
                    {
                        return await xs.GetAddressesAsync(ci.Web_UID, 0, cri1);
                    });
                    if (searchedAddress != null && searchedAddress.Status != 0)
                    {
                        list = null;
                        errmsg = searchedAddress.Message;
                    }
                    else if (searchedAddress != null && searchedAddress.Status == 0 && searchedAddress.AddressList != null)
                    {
                        list = searchedAddress.AddressList;
                        list = list.OrderBy(o => o.COMPANY).ToList().ToArray();

                        ////
                    }
                }

                if (list != null && list.Length > 0)
                {
                    for (int i = 0; i <= list.Length - 1; i++)
                    {
                        ads = list[i];
                        if (ads != null)
                        {
                            ab = await Task.Run(async () =>
                            {
                                return await xs.GetAddressesAsync(ci.Web_UID, ads.IDX, "");
                            });
                            if (ab != null && ab.AddressList != null)
                            {
                                listtemp = ab.AddressList;
                                for (int a = 0; a <= listtemp.Length - 1; a++)
                                {
                                    temp = listtemp[a];
                                    if (temp.IDX == ads.IDX && temp.Contacts != null && temp.Contacts.Length > 0)
                                    {
                                        if (list2 == null)
                                            list2 = new List<XDelServiceRef.AddressStructure>();
                                        nads = DeepClone(ads);
                                        nads.Contacts = temp.Contacts;
                                        list2.Add(nads);
                                    }
                                }
                            }
                        }
                    }

                    if (list2 != null && list2.Count > 0 && !String.IsNullOrEmpty(cri2))
                    {
                        bool asd = false;
                        for (int i = list2.Count - 1; i >= 0; i--)
                        {
                            cads = list2[i];
                            asd = cads.COMPANY.IndexOf(cri2, StringComparison.OrdinalIgnoreCase) >= 0;
                            if (!asd)
                                asd = cads.STREET.IndexOf(cri2, StringComparison.OrdinalIgnoreCase) >= 0;
                            if (!asd)
                                asd = cads.BUILDING.IndexOf(cri2, StringComparison.OrdinalIgnoreCase) >= 0;
                            if (!asd)
                                asd = cads.POSTALCODE.IndexOf(cri2, StringComparison.OrdinalIgnoreCase) >= 0;
                            if (!asd && cads.Contacts != null && cads.Contacts.Length > 0 && !String.IsNullOrEmpty(cads.Contacts[0].NAME))
                                asd = cads.Contacts[0].NAME.IndexOf(cri2, StringComparison.OrdinalIgnoreCase) >= 0;

                            if (!asd)
                                list2.RemoveAt(i);
                        }
                    }

                    if (list2 != null && list2.Count > 0)
                        listfinal = list2.ToArray();

                }

                await closeProgress_dialog();

                if (listfinal == null)
                {
                    if (_ExpviewModel != null)
                    {
                        _ExpviewModel.Items.Clear();
                        _ExpviewModel.setsearchedAddress(null);
                        _ExpviewModel.cri1 = null;
                        _ExpviewModel.cri2 = null;
                        cvAddress.BindingContext = null;
                    }
                    await DisplayAlertAsync("", errmsg2, "OK");
                    addTapGestureRecognizer();
                }
                else if (listfinal != null && listfinal.Length > 0)
                {
                    /*_ExpviewModel = new SelectableExpListAddressBookVM(listfinal,
                        logininfo != null && logininfo.ContactLvlSettingsInfo != null && logininfo.ContactLvlSettingsInfo.XDelOnlineSettings != null ? logininfo.ContactLvlSettingsInfo.XDelOnlineSettings.CanManageAddressBook : false,
                        logininfo != null && logininfo.ContactLvlSettingsInfo != null && logininfo.ContactLvlSettingsInfo.XDelOnlineSettings != null ? logininfo.ContactLvlSettingsInfo.XDelOnlineSettings.CanAddAuthShippers : false);
                    _ExpviewModel.setsearchedAddress(listfinal);
                    
                    //// Subscribe to ChildTappedEvent
                    _ExpviewModel.ChildTappedEvent += async (s, child) => await SetAddressToAddressPage(child);
                    //// Subscribe to ChildEditEvent
                    _ExpviewModel.ChildEditEvent += async (s, child) => await EditContact(child);
                    //// Subscribe to HeaderEditEvent
                    _ExpviewModel.HeaderEditEvent += async (s, header) => await EditAddress(header);
                    //// Subscribe to HeaderContactAddEvent
                    _ExpviewModel.HeaderContactAddEvent += async (s, header) => await AddContact(header);*/

                    UnsubscribeExpViewModel(); // unsubscribe OLD model before replacing

                    _ExpviewModel = new SelectableExpListAddressBookVM(listfinal,
                        logininfo?.ContactLvlSettingsInfo?.XDelOnlineSettings?.CanManageAddressBook ?? false,
                        logininfo?.ContactLvlSettingsInfo?.XDelOnlineSettings?.CanAddAuthShippers ?? false);
                    _ExpviewModel.setsearchedAddress(listfinal);

                    SubscribeExpViewModel(); // subscribe named handlers to NEW model

                    cvAddress.BindingContext = _ExpviewModel;
                    addTapGestureRecognizer();
                }
                else if (_ExpviewModel != null)
                {
                    /*_ExpviewModel.Items.Clear();
                    _ExpviewModel.setsearchedAddress(null);
                    _ExpviewModel.cri1 = null;
                    _ExpviewModel.cri2 = null;
                    addTapGestureRecognizer();*/

                    UnsubscribeExpViewModel(); // clean up before clearing
                    _ExpviewModel.Items.Clear();
                    _ExpviewModel.setsearchedAddress(null);
                    _ExpviewModel.cri1 = null;
                    _ExpviewModel.cri2 = null;
                }
            }
            else
            {
                await closeProgress_dialog();

                await DisplayAlertAsync("", errmsg, "OK");
                addTapGestureRecognizer();
                await common.BackToLogin();
            }
        }
        catch (Exception e)
        {
            await closeProgress_dialog();

            string s = e.Message;
        }
        finally
        {
            await closeProgress_dialog();

            addTapGestureRecognizer();
        }
    }

    async Task SetAddressToAddressPage(AddressChild ac)
    {
        try
        {
            XDelServiceRef.AddressStructure? ads = ac!= null ? ac.addressStructure : null;
            if (vm != null && vm is CreateJobVM && ads != null)
            {
                if (vm.AddressMode == 1)
                    vm.ColAddress = ads;
                if (vm.AddressMode == 2)
                    vm.DelAddress = ads;
                if (vm.AddressMode == 3)
                    vm.RtnAddress = ads;

                await SaveToAddress();
            }
        } catch (Exception e)
        {
            string s = e.Message;
        }
    }

    async Task EditAddress(ETHAN.classes.AddressHeader ah)
    {
        string errmsg = "";
        XDelServiceRef.XWSBase xbase;
        try
        {
            if (NetworkHelper.IsDisconnected())
            {
                await DisplayAlertAsync("No Internet Connection", "Please check your internet connection and try again.", "OK");
                return;
            }

            if (ah == null)
                return;

            BSADDRESS.HeaderText = "EDIT ADDRESS INFO";

            if (logininfo != null && logininfo.clientInfo != null && !String.IsNullOrEmpty(logininfo.clientInfo.Web_UID) && logininfo.ContactLvlSettingsInfo != null)
            {
                await showProgress_Dialog("Processing...");
                removeTapGestureRecognizer();
                removeEditAddressFieldsTapGestureRecognizer();
                txtPostalTextChangedUnSubscribed();
                txtPostal.Text = "";
                txtBlock.Text = "";
                txtUnit.Text = "";
                txtStreet.Text = "";
                txtBldg.Text = "";
                txtCompany.Text = "";

                lblSelectedLocationType.Text = "";

                await loadLocationType();

                if (CanAddAuthShippers(ah.AddressType, ref errmsg))
                {
                    GetAddressObj ao = await setAddressFields(logininfo.clientInfo.Web_UID, ah.Caidx);
                    if (!ao.found)
                    {
                        addTapGestureRecognizer();
                        await closeProgress_dialog();

                        await DisplayAlertAsync("", "Error loading information. Please try again.", "OK");
                    }
                    else
                    {
                        xbase = await xs.AddressCanEditAsync(logininfo.clientInfo.Web_UID, ah.Caidx, 0);
                        if (xbase.Status < 0)
                        {
                            //got error, need send debug report using XDelMail.SendDebugMail
                            addTapGestureRecognizer();
                            await closeProgress_dialog();
                        }
                        else if (xbase.Status == 0)
                        {
                            //cannot edit
                            removeEditAddressFieldsTapGestureRecognizer();
                            addTapGestureRecognizer();
                            await closeProgress_dialog();

                            await DisplayAlertAsync("", "Unable to Edit Selected Address.\r\nSelected Address is already in use.\r\nYou can create a new Address under the this Company/Name.", "OK");
                            /*btnSaveBSADDRESS.IsVisible = false;
                            btnUpdateBSADDRESS.IsVisible = false;
                            await Task.Delay(100);
                            await BSADDRESS.OpenBottomSheet(false);
                            BSADDRESS.IsVisible = true;
                            BSADDRESS.isShowing = true;*/
                        }
                        else if (xbase.Status == 1)
                        {
                            //can edit
                            await closeProgress_dialog();

                            AddEditAddressFieldsTapGestureRecognizer();
                            txtPostal.IsReadOnly = false;
                            txtBlock.IsReadOnly = false;
                            txtUnit.IsReadOnly = false;
                            txtStreet.IsReadOnly = false;
                            txtBldg.IsReadOnly = false;
                            btnSaveBSADDRESS.IsVisible = false;
                            btnUpdateBSADDRESS.IsVisible = true;
                            await Task.Delay(100);
                            await BSADDRESS.OpenBottomSheet(false);
                            BSADDRESS.IsVisible = true;
                            BSADDRESS.isShowing = true;
                            txtPostalTextChangedSubscribed();
                        }
                    }
                }
                else
                {
                    addTapGestureRecognizer();
                    await closeProgress_dialog();

                    await DisplayAlertAsync("", errmsg, "OK");
                }
            }
            else
            {
                addTapGestureRecognizer();
                return;
            }
        } catch (Exception e)
        {
            await closeProgress_dialog();

            string s = e.Message;
            addTapGestureRecognizer();
        }
    }

    private bool CanAddAuthShippers(Int32 addType, ref string errMsg)
    {
        try
        {
            if (logininfo != null && logininfo.ContactLvlSettingsInfo != null)
            {
                if (addType == 0 || addType == 1)
                {
                    XDelServiceRef.SettingsInfo custSets = logininfo.ContactLvlSettingsInfo;

                    if (custSets.Status == 0)
                    {
                        if (custSets.XDelOnlineSettings.CanAddAuthShippers != true)
                        {
                            errMsg = "Operation not allow. No access to edit Authorized Shipper.";
                            return false;
                        }
                        else
                            return true;
                    }
                    else
                    {
                        errMsg = custSets.Message;
                        return false;
                    }
                }
                else
                    return true;
            }
            else
                return false;
        } catch (Exception e)
        {
            string s = e.Message;
            return false;
        }
    }

    private async Task<GetAddressObj> setAddressFields(string uid, long AddressIDX)
    {
        string DLBLOCK, DLSTREET, DLUNIT, DLBUILDING, DLPOSTALCODE, DLCompany;
        int locationType = 0;
        GetAddressObj ao = new GetAddressObj();
        XDelServiceRef.AddressBook addbook = null;
        try
        {
            addbook = await xs.GetAddressesAsync(uid, AddressIDX, "");
            if (addbook != null && addbook.Status != 0)
            {
                ao.msg = "Error getting Address." + addbook.Status + " Message: " + addbook.Message;
            } else if (addbook != null && addbook.Status == 0)
            {
                XDelServiceRef.AddressStructure[] addstruct = addbook.AddressList;
                if (addstruct != null && addstruct.Length > 0)
                {
                    foreach (XDelServiceRef.AddressStructure ads in addstruct)
                    {
                        if (ads.IDX == AddressIDX)
                        {
                            DLPOSTALCODE = !string.IsNullOrEmpty(ads.POSTALCODE) ? ads.POSTALCODE : "";
                            DLBLOCK = !string.IsNullOrEmpty(ads.BLOCK) ? ads.BLOCK + " " : "";
                            DLUNIT = !string.IsNullOrEmpty(ads.UNIT) ? ads.UNIT + " " : "";
                            DLSTREET = !string.IsNullOrEmpty(ads.STREET) ? ads.STREET + " " : "";
                            DLBUILDING = !string.IsNullOrEmpty(ads.BUILDING) ? ads.BUILDING : "";
                            DLCompany = !string.IsNullOrEmpty(ads.COMPANY) ? ads.COMPANY : "";
                            locationType = ads.LocationType == XDelServiceRef.Location_Type.Residential ? 1 :
                                ads.LocationType == XDelServiceRef.Location_Type.Office ? 2 : 2;

                            lblBSADDRESSCAIDX.Text = ads.IDX.ToString();
                            txtPostal.Text = DLPOSTALCODE;
                            txtBlock.Text = DLBLOCK;
                            txtUnit.Text = DLUNIT;
                            txtStreet.Text = DLSTREET;
                            txtBldg.Text = DLBUILDING;
                            txtCompany.Text = DLCompany;

                            lblSelectedLocationType.Text = locationType.ToString();
                            if ((cvLocationType.ItemsSource is List<LocationTypeDisp> items))
                            {
                                LocationTypeDisp item;
                                for (int i = 0; i <= items.Count - 1; i++)
                                {
                                    item = items[i];
                                    if (item.value.Equals(locationType))
                                    {
                                        cvLocationType.SelectedItem = items[i];
                                        break;
                                    }
                                }
                            }

                            swAddress.Toggled -= swAddressonToggle;
                            swAddress.IsToggled = ads.ACTIVE;
                            swAddress.Toggled += swAddressonToggle;
                            swAddress.IsVisible = true;
                            lblBSAddressActive.IsVisible = true;
                            lblBSAddressActive.Text = "Click to Deactivate";

                            ao.found = true;
                            break;
                        }
                    }
                } else
                {
                    ao.msg = "Error getting Address.";
                }
            }
            else
            {
                ao.msg = "Error getting Address.";
            }
        } catch (Exception e)
        {
            string s = e.Message;
        }
        return ao;
    }

    private class GetAddressObj()
    {
        public bool found { get; set; } = false;
        public string msg { get; set; } = string.Empty;
    }

    async Task AddContact(AddressHeader ah)
    {
        string errmsg = "";
        try
        {
            if (ah == null)
                return;

            if (logininfo != null && logininfo.clientInfo != null && !String.IsNullOrEmpty(logininfo.clientInfo.Web_UID) && logininfo.ContactLvlSettingsInfo != null)
            {
                removeTapGestureRecognizer();
                addEditContactFieldsTapGestureRecognizer();
                lblBSContactCAIDX.Text = ah.Caidx.ToString();
                swContact.Toggled -= swContactonToggle;
                swContact.IsToggled = true;
                swContact.Toggled += swContactonToggle;
                swContact.IsVisible = false;
                lblBSContactActive.IsVisible = false;
                //btnAddContact.IsVisible = false;
                lblBSContactCNIDX.Text = "";

                btnSaveBSContact.IsVisible = true;
                btnUpdateBSContact.IsVisible = false;

                txtContactName.Text = "";
                txtDept.Text = "";
                txtTel.Text = "";
                txtHp.Text = "";
                BSContact.HeaderText = "ADD NEW CONTACT"; 
                if (_ExpviewModel != null)
                    _ExpviewModel.setSelectedAddressStructure(ah.AddressStructure_);


                if (!CanAddAuthShippers(ah.AddressType, ref errmsg))
                {
                    addTapGestureRecognizer();
                    await closeProgress_dialog();

                    await DisplayAlertAsync("", errmsg, "OK");
                    return;
                }
            }
            else
            {
                addEditContactFieldsTapGestureRecognizer();
                return;
            }

            removeTapGestureRecognizer();

            await Task.Delay(100);
            await BSContact.OpenBottomSheet(false);
            BSContact.IsVisible = true;
            BSContact.isShowing = true;


        }
        catch (Exception e)
        {
            string s = e.Message;
            addTapGestureRecognizer();
        }
    }

    async Task EditContact(AddressChild ac)
    {
        ETHAN.classes.AddressHeader? ah = null;
        string errmsg = "";
        XDelServiceRef.XWSBase xbase;
        try
        {
            if (NetworkHelper.IsDisconnected())
            {
                await DisplayAlertAsync("No Internet Connection", "Please check your internet connection and try again.", "OK");
                return;
            }

            if (ac == null)
                return;

            ah = ac.AddressHeader;

            if (ah == null)
                return;


            BSContact.HeaderText = "EDIT CONTACT INFO";

            if (logininfo != null && logininfo.clientInfo != null && !String.IsNullOrEmpty(logininfo.clientInfo.Web_UID) && logininfo.ContactLvlSettingsInfo != null)
            {
                await showProgress_Dialog("Processing...");
                removeTapGestureRecognizer();
                removeEditContactFieldsTapGestureRecognizer();

                lblBSContactCAIDX.Text = "";
                lblBSContactCNIDX.Text = "";

                txtContactName.Text = "";
                txtDept.Text = "";
                txtTel.Text = "";
                txtHp.Text = "";

                if (CanAddAuthShippers(ah.AddressType, ref errmsg))
                {
                    GetAddressObj ao = await setContactFields(logininfo.clientInfo.Web_UID, ah.Caidx, ac.Cnidx);
                    if (!ao.found)
                    {
                        addTapGestureRecognizer();
                        await closeProgress_dialog();

                        await DisplayAlertAsync("", "Error loading information. Please try again.", "OK");
                    }
                    else
                    {
                        xbase = await xs.AddressCanEditAsync(logininfo.clientInfo.Web_UID, ah.Caidx, ac.Cnidx);
                        if (xbase.Status < 0)
                        {
                            //got error, need send debug report using XDelMail.SendDebugMail
                            addTapGestureRecognizer();
                            await closeProgress_dialog();

                        }
                        else if (xbase.Status == 0)
                        {
                            //cannot edit
                            removeEditContactFieldsTapGestureRecognizer();
                            await closeProgress_dialog();

                            await DisplayAlertAsync("", "Unable to Edit Selected Contact.\r\nSelected Contact is already in use.\r\nYou can only deactivate this Contact or press CLEAR to add a new contact.", "OK");
                            if (_ExpviewModel != null)
                                _ExpviewModel.setSelectedAddressStructure(ac.addressStructure);
                            btnSaveBSContact.IsVisible = false;
                            btnUpdateBSContact.IsVisible = true;
                            //btnAddContact.IsVisible = false;
                            await Task.Delay(100);
                            await BSContact.OpenBottomSheet(false);
                            BSContact.IsVisible = true;
                            BSContact.isShowing = true;
                        }
                        else if (xbase.Status == 1)
                        {
                            //can edit
                            await closeProgress_dialog();

                            addEditContactFieldsTapGestureRecognizer();
                            txtContactName.IsReadOnly = false;
                            txtDept.IsReadOnly = false;
                            txtTel.IsReadOnly = false;
                            txtHp.IsReadOnly = false;
                            if (_ExpviewModel != null)
                                _ExpviewModel.setSelectedAddressStructure(ac.addressStructure);
                            btnSaveBSContact.IsVisible = false;
                            btnUpdateBSContact.IsVisible = true;
                            //btnAddContact.IsVisible = true;
                            await Task.Delay(100);
                            await BSContact.OpenBottomSheet(false);
                            BSContact.IsVisible = true;
                            BSContact.isShowing = true;
                        }
                    }
                }
                else
                {
                    addTapGestureRecognizer();
                    await closeProgress_dialog();

                    await DisplayAlertAsync("", errmsg, "OK");
                    return;
                }
            } else
            {
                addEditContactFieldsTapGestureRecognizer();
                return;
            }

            removeTapGestureRecognizer();

            await Task.Delay(100);
            await BSContact.OpenBottomSheet(false);
            BSContact.IsVisible = true;
            BSContact.isShowing = true;
        }
        catch (Exception e)
        {
            string s = e.Message;
            addTapGestureRecognizer();
        }
    }

    private async Task<GetAddressObj> setContactFields(string uid, long AddressIDX, long CNIDX)
    {
        string name, dept, tel, mobile;
        GetAddressObj ao = new GetAddressObj();
        XDelServiceRef.AddressBook addbook = null;
        XDelServiceRef.ContactStructure cs = null;
        try
        {
            addbook = await xs.GetAddressesAsync(uid, AddressIDX, "");
            if (addbook != null && addbook.Status != 0)
            {
                ao.msg = "Error getting Address." + addbook.Status + " Message: " + addbook.Message;
            }
            else if (addbook != null && addbook.Status == 0)
            {
                XDelServiceRef.AddressStructure[] addstruct = addbook.AddressList;
                if (addstruct != null && addstruct.Length > 0)
                {
                    foreach (XDelServiceRef.AddressStructure ads in addstruct)
                    {
                        if (ads.IDX == AddressIDX && ads.Contacts != null && ads.Contacts.Length > 0)
                        {
                            foreach (XDelServiceRef.ContactStructure c in ads.Contacts)
                            {
                                if (c.IDX == CNIDX)
                                {
                                    name = !string.IsNullOrEmpty(c.NAME) ? c.NAME : "";
                                    dept = !string.IsNullOrEmpty(c.DEPARTMENT) ? c.DEPARTMENT : "";
                                    tel = !string.IsNullOrEmpty(c.TEL) ? c.TEL : "";
                                    mobile = !string.IsNullOrEmpty(c.MOBILE) ? c.MOBILE : "";

                                    lblBSContactCAIDX.Text = c.CLIENTADDRESSIDX.ToString();
                                    lblBSContactCNIDX.Text = c.IDX.ToString();

                                    txtContactName.Text = name;
                                    txtDept.Text = dept;
                                    txtTel.Text = tel;
                                    txtHp.Text = mobile;

                                    swContact.Toggled -= swContactonToggle;
                                    swContact.IsToggled = c.ACTIVE;
                                    swContact.Toggled += swContactonToggle;
                                    swContact.IsVisible = true;
                                    lblBSContactActive.IsVisible = true;
                                    lblBSContactActive.Text = "Click to Deactivate";
                                    ao.found = true;
                                    break;
                                }
                            }

                            break;
                        }
                    }
                }
                else
                {
                    ao.msg = "Error getting Contact.";
                }
            }
            else
            {
                ao.msg = "Error getting Contact.";
            }
        } catch (Exception e)
        {
            string s = e.Message;
        }
        return ao;
    }

    async void btnCXBSContact_Clicked(object sender, EventArgs e)
    {
        try
        {
            //btnAddContact.IsVisible = true;
            addEditContactFieldsTapGestureRecognizer();
            addTapGestureRecognizer();
            /*await BSContact.CloseBottomSheet();
            BSContact.IsVisible = false;
            BSContact.isShowing = false;*/
            await CloseBottomSheetSafely(BSContact);
        } catch (Exception ex)
        {
            string s = ex.Message;
        }
    }

    async void btnSaveBSContact_Clicked(object sender, EventArgs e)
    {
        try
        {
            if (NetworkHelper.IsDisconnected())
            {
                await DisplayAlertAsync("No Internet Connection", "Please check your internet connection and try again.", "OK");
                return;
            }

            if (logininfo != null && logininfo.clientInfo != null && !string.IsNullOrEmpty(logininfo.clientInfo.Web_UID))
            {
                long CAIDX = string.IsNullOrEmpty(lblBSContactCAIDX.Text) ? 0 : long.Parse(lblBSContactCAIDX.Text);
                long CNIDX = string.IsNullOrEmpty(lblBSContactCNIDX.Text) ? 0 : long.Parse(lblBSContactCNIDX.Text);

                if (!cannotSaveUpdateContact())
                {
                    XDelServiceRef.ContactStructure cts = await Task.Run(async () =>
                    {
                        return await InsertUpdateContact(false, logininfo.clientInfo.Web_UID, CAIDX, CNIDX);
                    });
                    if (cts == null)
                        await DisplayAlertAsync("", "Error processing this request.", "OK");
                    else
                    {
                        btnCXBSContact_Clicked(null, null);
                        await DisplayAlertAsync("", "New Contact saved.", "OK");
                    }
                }
                else
                {
                    await DisplayAlertAsync("", "Please enter Contact Name and at least a Telephone or Mobile Number.", "OK");
                }
            } else
            {
                btnCXBSContact_Clicked(null, null);
                await DisplayAlertAsync("", "Timeout. Please login again.", "OK");
            }
        }
        catch (Exception ex)
        {
            string s = ex.Message;
            await closeProgress_dialog();

            await DisplayAlertAsync("", "Error processing this request.", "OK");
        }
    }

    async void btnUpdateBSContact_Clicked(object sender, EventArgs e)
    {
        try
        {
            if (NetworkHelper.IsDisconnected())
            {
                await DisplayAlertAsync("No Internet Connection", "Please check your internet connection and try again.", "OK");
                return;
            }

            if (logininfo != null && logininfo.clientInfo != null && !string.IsNullOrEmpty(logininfo.clientInfo.Web_UID))
            {
                long CAIDX = string.IsNullOrEmpty(lblBSContactCAIDX.Text) ? 0 : long.Parse(lblBSContactCAIDX.Text);
                long CNIDX = string.IsNullOrEmpty(lblBSContactCNIDX.Text) ? 0 : long.Parse(lblBSContactCNIDX.Text);

                if (!cannotSaveUpdateContact())
                {
                    XDelServiceRef.ContactStructure cts = await Task.Run(async () =>
                    {
                        return await InsertUpdateContact(false, logininfo.clientInfo.Web_UID, CAIDX, CNIDX);
                    });
                    if (cts == null)
                        await DisplayAlertAsync("", "Error processing this request.", "OK");
                    else
                    {
                        btnCXBSContact_Clicked(null, null);
                        await DisplayAlertAsync("", "Contact updated.", "OK");
                    }
                }
                else
                {
                    await DisplayAlertAsync("", "Please enter Contact Name and at least a Telephone or Mobile Number.", "OK");
                }
            }
            else
            {
                btnCXBSContact_Clicked(null, null);
                await DisplayAlertAsync("", "Timeout. Please login again.", "OK");
            }
        }
        catch (Exception ex)
        {
            string s = ex.Message;
            await closeProgress_dialog();

            await DisplayAlertAsync("", "Error processing this request.", "OK");
        }
    }

    async void btnCXBSADDRESS_Clicked(object sender, EventArgs e)
    {
        try
        {
            AddEditAddressFieldsTapGestureRecognizer();
            addTapGestureRecognizer();
            /*await BSADDRESS.CloseBottomSheet();
            BSADDRESS.IsVisible = false;
            BSADDRESS.isShowing = false;*/
            await CloseBottomSheetSafely(BSADDRESS);
        }
        catch (Exception ex)
        {
            string s = ex.Message;
        }
    }

    async void btnSaveBSADDRESS_Clicked(object sender, EventArgs e)
    {
        try
        {
            if (NetworkHelper.IsDisconnected())
            {
                await DisplayAlertAsync("No Internet Connection", "Please check your internet connection and try again.", "OK");
                return;
            }

            if (logininfo != null && logininfo.clientInfo != null && !string.IsNullOrEmpty(logininfo.clientInfo.Web_UID))
            {
                if (!cannotSaveUpdateAddress())
                {
                    await InsertUpdateAddress(true);
                }
                else
                {
                    await DisplayAlertAsync("", "Please enter valid Postal Code and select if address is Residential or Office.", "OK");
                }
            } else
            {
                await DisplayAlertAsync("", "Timeout. Please login again.", "OK");
            }
        }
        catch (Exception ex)
        {
            string s = ex.Message;
            await closeProgress_dialog();

            await DisplayAlertAsync("", "Error processing this request.", "OK");
        }
    }

    async void btnUpdateBSADDRESS_Clicked(object sender, EventArgs e)
    {
        try
        {
            if (NetworkHelper.IsDisconnected())
            {
                await DisplayAlertAsync("No Internet Connection", "Please check your internet connection and try again.", "OK");
                return;
            }

            if (logininfo != null && logininfo.clientInfo != null && !string.IsNullOrEmpty(logininfo.clientInfo.Web_UID))
            {
                if (!cannotSaveUpdateAddress())
                {
                    await InsertUpdateAddress(false);
                }
                else
                {
                    await DisplayAlertAsync("", "Please enter valid Postal Code and select if address is Residential or Office.", "OK");
                }
            }
            else
            {
                await DisplayAlertAsync("", "Timeout. Please login again.", "OK");
            }
        }
        catch (Exception ex)
        {
            string s = ex.Message;
            await closeProgress_dialog();

            await DisplayAlertAsync("", "Error processing this request.", "OK");
        }
    }

    private async void swContactonToggle(object? sender, ToggledEventArgs e)
    {
        try
        {
            lblBSContactActive.Text = e.Value ? "Click to Deactivate" : "Click to Activate";
        } catch (Exception ex)
        {
            string s = ex.Message;
        }
    }

    private async void swAddressonToggle(object? sender, ToggledEventArgs e)
    {
        try
        {
            lblBSAddressActive.Text = e.Value ? "Click to Deactivate" : "Click to Activate";
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

    private async void OnEntryTextChanged(object sender, TextChangedEventArgs e)
    {
        try
        {
            string oldText = e.OldTextValue;
            string newText = e.NewTextValue;

            if (!string.IsNullOrEmpty(newText) && newText.Length == 6 && !txtPostal.IsReadOnly && swAddress.IsToggled)
                await SearchPostal();
            else if (!string.IsNullOrEmpty(oldText) && oldText.Length == 6 && oldText.Length > newText.Length && !txtPostal.IsReadOnly && swAddress.IsToggled)
                resetFieldsForPostalChange();


            /*Debug.WriteLine($"Old: {oldText}, New: {newText}");
            Debug.WriteLine($"newText: {newText}, newText.Length: {newText.Length.ToString()}, txtPostal.IsReadOnly: " +
                $"{txtPostal.IsReadOnly.ToString()}, swAddress.IsToggled: {swAddress.IsToggled.ToString()}");
            if (!string.IsNullOrEmpty(newText) && newText.Length == 6 && !txtPostal.IsReadOnly && swAddress.IsToggled)
            {
                await SearchPostal();
            }
            else if (!string.IsNullOrEmpty(oldText) && oldText.Length == 6 && oldText.Length > newText.Length && !txtPostal.IsReadOnly && swAddress.IsToggled)
            {
                resetFieldsForPostalChange();
            }*/

        }
        catch (Exception ex)
        {
            string s = ex.Message;
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

    private async Task SearchPostal()
    {
        XDelServiceRef.AddressBook ab = null;
        XDelServiceRef.AddressStructure? ads = null;
        string postal = txtPostal.Text;
        try
        {
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

    private bool cannotSaveUpdateAddress()
    {
        bool asd = false;
        try
        {
            asd = string.IsNullOrEmpty(txtPostal.Text) ||
                string.IsNullOrEmpty(txtStreet.Text) ||
                string.IsNullOrEmpty(lblSelectedLocationType.Text);
        } catch (Exception e)
        {
            string s = e.Message;
        }
        return asd;
    }

    private bool cannotSaveUpdateContact()
    {
        bool asd = false;
        try
        {
            if (!swContact.IsToggled)
                return false;
            asd = string.IsNullOrEmpty(txtContactName.Text) ||
                (string.IsNullOrEmpty(txtTel.Text) && string.IsNullOrEmpty(txtHp.Text));
        }
        catch (Exception e)
        {
            string s = e.Message;
        }
        return asd;
    }

    async Task InsertUpdateAddress(bool insert)
    {
        XDelServiceRef.AddressStructure? newCompany = null;
        XDelServiceRef.AddressBookUpdateStatus? stats = null;
        bool remove = false;
        try
        {
            if (logininfo != null && logininfo.clientInfo != null && !string.IsNullOrEmpty(logininfo.clientInfo.Web_UID))
            {
                await showProgress_Dialog("Processing...");

                ClientInfo ci = logininfo.clientInfo;
                if (insert)
                {
                    newCompany = new XDelServiceRef.AddressStructure();
                    newCompany.IDX = 0;
                    newCompany.CLIENTIDX = 0;
                    newCompany.ADDRESSTYPE = 2;
                    newCompany.ACTIVE = true;
                    newCompany.COMPANY = !string.IsNullOrEmpty(txtCompany.Text) ? txtCompany.Text.ToUpper() : "";
                    newCompany.BLOCK = !string.IsNullOrEmpty(txtBlock.Text) ? txtBlock.Text.ToUpper() : "";
                    newCompany.STREET = !string.IsNullOrEmpty(txtStreet.Text) ? txtStreet.Text.ToUpper() : "";
                    newCompany.UNIT = !string.IsNullOrEmpty(txtUnit.Text) ? txtUnit.Text.ToUpper() : "";
                    newCompany.POSTALCODE = !string.IsNullOrEmpty(txtPostal.Text) ? txtPostal.Text.ToUpper() : "";
                    newCompany.BUILDING = !string.IsNullOrEmpty(txtBldg.Text) ? txtBldg.Text.ToUpper() : "";
                    newCompany.CUSTOMER_NOTES = "";
                    int SelectedLocationType = !string.IsNullOrEmpty(lblSelectedLocationType.Text) ? int.Parse(lblSelectedLocationType.Text) : 2;
                    newCompany.LocationType = SelectedLocationType == 1 ? XDelServiceRef.Location_Type.Residential : XDelServiceRef.Location_Type.Office;
                }
                else
                {
                    long CAIDX = !string.IsNullOrEmpty(lblBSADDRESSCAIDX.Text) ? long.Parse(lblBSADDRESSCAIDX.Text) : 0;
                    XDelServiceRef.AddressBook ab = await Task.Run(async () =>
                    {
                        return await xs.GetAddressesAsync(ci.Web_UID, CAIDX, "");
                    });
                    if (ab != null && ab.Status == 0 && ab.AddressList != null && ab.AddressList.Length > 0)
                    {
                        foreach (XDelServiceRef.AddressStructure ads in ab.AddressList)
                        {
                            if (ads.IDX == CAIDX)
                            {
                                newCompany = ads;
                                newCompany.ACTIVE = swAddress.IsToggled;
                                remove = !swAddress.IsToggled;
                                newCompany.COMPANY = txtCompany.Text.ToUpper();
                                newCompany.BLOCK = txtBlock.Text.ToUpper();
                                newCompany.STREET = txtStreet.Text.ToUpper();
                                newCompany.UNIT = txtUnit.Text.ToUpper();
                                newCompany.POSTALCODE = txtPostal.Text.ToUpper();
                                newCompany.BUILDING = txtBldg.Text.ToUpper();
                                newCompany.CUSTOMER_NOTES = "";
                                int SelectedLocationType = !string.IsNullOrEmpty(lblSelectedLocationType.Text) ? int.Parse(lblSelectedLocationType.Text) : 2;
                                newCompany.LocationType = SelectedLocationType == 1 ? XDelServiceRef.Location_Type.Residential : XDelServiceRef.Location_Type.Office;
                                break;
                            }
                        }
                    }
                }

                if (newCompany != null)
                {
                    stats = await Task.Run(async () =>
                    {
                        return await xs.UpdateAddressAsync(ci.Web_UID, newCompany);
                    });
                }
                
                if (stats != null && stats.Status == 0)
                {
                    if (insert)
                    {
                        long newCAIDX = stats.AddressStructure.IDX;
                        XDelServiceRef.ContactStructure? contacts = await Task.Run(async () =>
                        {
                            return await InsertUpdateContact(true, ci.Web_UID, newCAIDX, 0);
                        });

                        newCompany = stats.AddressStructure;
                        newCompany.Contacts = new XDelServiceRef.ContactStructure[] { contacts };
                        XDelServiceRef.AddressStructure[]? listfinal = new XDelServiceRef.AddressStructure[] { newCompany };

                        _ExpviewModel = new SelectableExpListAddressBookVM(listfinal,
                        logininfo != null && logininfo.ContactLvlSettingsInfo != null && logininfo.ContactLvlSettingsInfo.XDelOnlineSettings != null ? logininfo.ContactLvlSettingsInfo.XDelOnlineSettings.CanManageAddressBook : false,
                        logininfo != null && logininfo.ContactLvlSettingsInfo != null && logininfo.ContactLvlSettingsInfo.XDelOnlineSettings != null ? logininfo.ContactLvlSettingsInfo.XDelOnlineSettings.CanAddAuthShippers : false);
                        _ExpviewModel.setsearchedAddress(listfinal);

                        /*//// UnSubscribe to ChildTappedEvent
                        _ExpviewModel.ChildTappedEvent -= async (s, child) => await SetAddressToAddressPage(child);
                        //// UnSubscribe to ChildEditEvent
                        _ExpviewModel.ChildEditEvent -= async (s, child) => await EditContact(child);
                        //// UnSubscribe to HeaderEditEvent
                        _ExpviewModel.HeaderEditEvent -= async (s, header) => await EditAddress(header);
                        //// UnSubscribe to HeaderContactAddEvent
                        _ExpviewModel.HeaderContactAddEvent -= async (s, header) => await AddContact(header);

                        //// Subscribe to ChildTappedEvent
                        _ExpviewModel.ChildTappedEvent += async (s, child) => await SetAddressToAddressPage(child);
                        //// Subscribe to ChildEditEvent
                        _ExpviewModel.ChildEditEvent += async (s, child) => await EditContact(child);
                        //// Subscribe to HeaderEditEvent
                        _ExpviewModel.HeaderEditEvent += async (s, header) => await EditAddress(header);
                        //// Subscribe to HeaderContactAddEvent
                        _ExpviewModel.HeaderContactAddEvent += async (s, header) => await AddContact(header);

                        cvAddress.BindingContext = _ExpviewModel;*/

                        UnsubscribeExpViewModel(); // unsubscribe old model

                        _ExpviewModel = new SelectableExpListAddressBookVM(listfinal,
                            logininfo?.ContactLvlSettingsInfo?.XDelOnlineSettings?.CanManageAddressBook ?? false,
                            logininfo?.ContactLvlSettingsInfo?.XDelOnlineSettings?.CanAddAuthShippers ?? false);
                        _ExpviewModel.setsearchedAddress(listfinal);

                        SubscribeExpViewModel(); // subscribe named handlers

                        cvAddress.BindingContext = _ExpviewModel;

                        await closeProgress_dialog();

                        btnCXBSADDRESS_Clicked(null, null);
                        await DisplayAlertAsync("", "New Address saved.", "OK");
                    } else
                    {

                        if (stats.AddressStructure.IDX > 0 && _ExpviewModel != null)
                            _ExpviewModel.update_searchedAddressAddress(stats.AddressStructure.IDX, stats.AddressStructure,
                        logininfo != null && logininfo.ContactLvlSettingsInfo != null && logininfo.ContactLvlSettingsInfo.XDelOnlineSettings != null ? logininfo.ContactLvlSettingsInfo.XDelOnlineSettings.CanManageAddressBook : false,
                        logininfo != null && logininfo.ContactLvlSettingsInfo != null && logininfo.ContactLvlSettingsInfo.XDelOnlineSettings != null ? logininfo.ContactLvlSettingsInfo.XDelOnlineSettings.CanAddAuthShippers : false,
                        remove);

                        await closeProgress_dialog();

                        btnCXBSADDRESS_Clicked(null, null);
                        await DisplayAlertAsync("", "Address updated.", "OK");
                    }
                } else
                {
                    await closeProgress_dialog();

                    await DisplayAlertAsync("", "Error processing this request.", "OK");
                }
            }
        }
        catch (Exception e)
        {
            string s = e.Message;
            addTapGestureRecognizer();
            await closeProgress_dialog();

            await DisplayAlertAsync("", "Error processing this request.", "OK");
        }
    }

    async Task<XDelServiceRef.ContactStructure> InsertUpdateContact(bool autoInsert, string uid, long CAIDX, long CNIDX)
    {
        XDelServiceRef.ContactStructure? contacts = new XDelServiceRef.ContactStructure();
        XDelServiceRef.AddressBookUpdateStatus? stats = null;
        string TITLE = "";
        string NAME = "";
        string DEPARTMENT = "";
        string TEL = "";
        string FAX = "";
        string OTHER = "";
        string MOBILE = "";
        string SI = "";
        string CUSTOMER_NOTES = "";
        bool ACTIVE = true;
        bool remove = false;

        try
        {
            if (!autoInsert)
            {
                await showProgress_Dialog("Processing...");
            }

            TITLE = "";
            NAME = autoInsert ? "Tentant" : !string.IsNullOrEmpty(txtContactName.Text) ? txtContactName.Text.ToUpper() : "";
            DEPARTMENT = autoInsert ? "" : !string.IsNullOrEmpty(txtDept.Text) ? txtDept.Text.ToUpper() : "";
            TEL = autoInsert ? "" : !string.IsNullOrEmpty(txtTel.Text) ? txtTel.Text : "";
            FAX = "";
            OTHER = "";
            MOBILE = autoInsert ? "" : !string.IsNullOrEmpty(txtHp.Text) ? txtHp.Text : "";
            SI = "";
            ACTIVE = (autoInsert) ? true : swContact.IsToggled;
            CUSTOMER_NOTES = "";

            contacts.CLIENTADDRESSIDX = CAIDX;
            contacts.IDX = CNIDX;
            contacts.TITLE = TITLE;
            contacts.NAME = NAME;
            contacts.DEPARTMENT = DEPARTMENT;
            contacts.TEL = TEL;
            contacts.FAX = FAX;
            contacts.OTHER = OTHER;
            contacts.MOBILE = MOBILE;
            contacts.SI = SI;
            contacts.ACTIVE = ACTIVE;
            contacts.CUSTOMER_NOTES = CUSTOMER_NOTES;
            remove = !ACTIVE;

            stats = await Task.Run(async () =>
            {
                return await xs.UpdateContactAsync(uid, contacts);
            });

            if (stats != null && stats.Status == 0 && autoInsert)
            {
                contacts = stats.ContactStructure;
                if (CAIDX > 0 && _ExpviewModel != null)
                    await _ExpviewModel.update_searchedAddressContact(CAIDX, CNIDX, contacts, remove);
            } else if (stats != null && stats.Status == 0 && !autoInsert)
            {
                await closeProgress_dialog();

                contacts = stats.ContactStructure;
                if (CAIDX > 0 && _ExpviewModel != null)
                    await _ExpviewModel.update_searchedAddressContact(CAIDX, CNIDX, contacts, remove);
            } else
            {
                await closeProgress_dialog();

                contacts = null;
            }
        } catch (Exception e)
        {
            string s = e.Message;
            await closeProgress_dialog();

            contacts = null;
        }
        return contacts;
    }

    #region Gestures

    void removeEditAddressFieldsTapGestureRecognizer()
    {
        try
        {
            txtPostal.InputTransparent = true;
            txtUnit.InputTransparent = true;
            txtBlock.InputTransparent = true;
            txtStreet.InputTransparent = true;
            txtBldg.InputTransparent = true;
            txtCompany.InputTransparent = true;
            cvLocationType.InputTransparent = true;
        } catch (Exception e)
        {
            string s = e.Message;
        }
    }

    void AddEditAddressFieldsTapGestureRecognizer()
    {
        try
        {
            removeEditAddressFieldsTapGestureRecognizer();
            txtPostal.InputTransparent = false;
            txtUnit.InputTransparent = false;
            txtBlock.InputTransparent = false;
            txtStreet.InputTransparent = false;
            txtBldg.InputTransparent = false;
            txtCompany.InputTransparent = false;
            cvLocationType.InputTransparent = false;
        }
        catch (Exception e)
        {
            string s = e.Message;
        }
    }

    void removeEditContactFieldsTapGestureRecognizer()
    {
        try
        {
            txtContactName.InputTransparent = true;
            txtDept.InputTransparent = true;
            txtTel.InputTransparent = true;
            txtHp.InputTransparent = true;
        } catch (Exception e)
        {
            string s = e.Message;
        }
    }

    void addEditContactFieldsTapGestureRecognizer()
    {
        try
        {
            removeEditContactFieldsTapGestureRecognizer();
            txtContactName.InputTransparent = false;
            txtDept.InputTransparent = false;
            txtTel.InputTransparent = false;
            txtHp.InputTransparent = false;
        }
        catch (Exception e)
        {
            string s = e.Message;
        }
    }

    void removeTapGestureRecognizer()
    {
        try
        {
            ddlOptions.InputTransparent = true;
            txtKeyword.InputTransparent = true;
            btnCancel.InputTransparent = true;
            btnSearch.InputTransparent = true;
            cvAddress.InputTransparent = true;
        }
        catch (Exception ex)
        {
            string s = ex.Message;
        }
    }

    void addTapGestureRecognizer()
    {
        try
        {
            removeTapGestureRecognizer();
            ddlOptions.InputTransparent = false;
            txtKeyword.InputTransparent = false;
            btnCancel.InputTransparent = false;
            btnSearch.InputTransparent = false;
            cvAddress.InputTransparent = false;
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