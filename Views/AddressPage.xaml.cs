using ETHAN.classes;
using ETHAN.Network;
using ETHAN.ProgressDialog;
using ETHAN.ViewModel;
using XDelServiceRef;

namespace ETHAN.Views;

[QueryProperty(nameof(TitleName), "titleName")]
[QueryProperty(nameof(Vmm), "vmm")] // Add a QueryProperty to handle the navigation parameter
[QueryProperty(nameof(LOGININFO), "LOGININFO")] // Add a QueryProperty to handle the navigation parameter
[QueryProperty(nameof(LTR), "LTR")] // Add a QueryProperty to handle the navigation parameter


public partial class AddressPage : ContentPage
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
            /*title = value;*/

            // Only update if value is not null
            // Returning via ".." from AddressBookPage will pass null
            if (value != null)
                title = value;
        }
    }

    private CreateJobVM vm;
    public CreateJobVM Vmm
    {
        set
        {
            /*vm = value ?? new CreateJobVM();
            BindingContext = vm; // If null, create a new instance*/

            if (value != null)
            {
                // Forward navigation from CreateJob with valid vm
                vm = value;
                BindingContext = vm;
            }
            else if (vm == null)
            {
                // Only create new if we genuinely have no vm yet
                vm = new CreateJobVM();
                BindingContext = vm;
            }
            // if value is null AND vm already exists (returning via ".." from AddressBookPage)
            // do nothing - keep existing vm
        }
    }

    private LoginInfo? logininfo;
    public LoginInfo? LOGININFO
    {
        set
        {
            /*logininfo = value;*/

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

    private bool _loadedOnce = false;
    private bool _returningFromAddressBook = false;

    public AddressPage(IProgressDialogService progressService)
    {
        InitializeComponent();
        _progressService = progressService;
        loadLocationType();
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

    protected override void OnAppearing()
    {
        base.OnAppearing();
        try
        {
            if (!_loadedOnce)
            {
                //txtPostal.Unfocused -= OnEntryUnfocused;
                if (vm.txtPostalTextChangedSubscribed)
                    txtPostalTextChangedUnSubscribed();

                btnTopNBack.Text = title;

                XDelServiceRef.AddressStructure? ads = null;
                if (vm.AddressMode == 1 && vm.ColAddress != null)
                    ads = vm.ColAddress;

                if (vm.AddressMode == 2 && vm.DelAddress != null)
                    ads = vm.DelAddress;

                if (vm.AddressMode == 3 && vm.RtnAddress != null)
                    ads = vm.RtnAddress;

                loadSlotType();

                cvSlot.IsVisible = !(vm.AddressMode == 1);

                if (ads != null)
                {
                    loadDefAddress(ads, true);
                }

                //txtPostal.Unfocused += OnEntryUnfocused;
                if (!vm.txtPostalTextChangedSubscribed)
                    txtPostalTextChangedSubscribed();

                _loadedOnce = true;
            }
            else if (_returningFromAddressBook)
            {
                _returningFromAddressBook = false;

                XDelServiceRef.AddressStructure? ads = null;
                if (vm.AddressMode == 1) ads = vm.ColAddress;
                if (vm.AddressMode == 2) ads = vm.DelAddress;
                if (vm.AddressMode == 3) ads = vm.RtnAddress;

                loadSlotType();

                cvSlot.IsVisible = !(vm.AddressMode == 1);

                if (ads != null)
                    loadDefAddress(ads, true);

                if (!vm.txtPostalTextChangedSubscribed)
                    txtPostalTextChangedSubscribed();
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

        // Always unsubscribe event handlers when leaving
        if (vm != null && vm.txtPostalTextChangedSubscribed)
            txtPostalTextChangedUnSubscribed();

        // Unsubscribe cvSlot in case it was left connected
        cvSlot.SelectionChanged -= cvSlotOnItemSelected;

        // Clear ItemSources so lists are released
        cvLocationType.ItemsSource = null;
        cvSlot.ItemsSource = null;
    }

    private void txtPostalTextChangedSubscribed()
    {
        try
        {
            txtPostal.TextChanged += OnEntryTextChanged;
            vm.txtPostalTextChangedSubscribed = true;
        } catch (Exception e)
        {
            string s = e.Message;
        }
    }

    private void txtPostalTextChangedUnSubscribed()
    {
        try
        {
            txtPostal.TextChanged -= OnEntryTextChanged;
            vm.txtPostalTextChangedSubscribed = false;
        }
        catch (Exception e)
        {
            string s = e.Message;
        }
    }

    async void BackToCreateJob()
    {
        /*await Shell.Current.GoToAsync("CreateJob", new Dictionary<string, object>
                    {
                        { "vmm", vm },
                        {"LOGININFO",  logininfo},
                        { "LTR", ltr }
                    });*/

        await Shell.Current.GoToAsync("..");
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

    protected override bool OnBackButtonPressed()
    {
        if (_progressService != null && _progressService.IsShowing)
            return true;

        BackToCreateJob();
        return true;
    }

    private void loadLocationType()
    {
        try
        {
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

    private void loadSlotType()
    {
        try
        {

            if (logininfo == null || logininfo != null && logininfo.clientInfo == null)
            {
                cvSlot.IsVisible = false;
                return;
            }

            List<SlotType> list = new List<SlotType>
            {
                new SlotType(1, "Can Slot Letterbox"),
                new SlotType(2, "Can Slot Under Door"),
                new SlotType(4, "Can Leave @ Guardhouse"),
                new SlotType(16, "Receiver ONLY"),
                new SlotType(2048, "Can Leave Outside Door")
            };

            ClientInfo ci = logininfo!.clientInfo;

            if (((ci.Flag2 & 4) == 4))
                list.RemoveAt(3); //if allow outside door, remove receiver only

            if (!((ci.Flag2 & 4) == 4))
                list.RemoveAt(4); //if allow outside door, remove receiver only

            cvSlot.ItemsSource = null;
            cvSlot.ItemsSource = list;
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
                vm.selectedLocationType = selectedItem.value;

            }
        }
        catch (Exception ex)
        {
            string s = ex.Message;
        }
    }

    private void cvSlotOnItemSelected(object sender, SelectionChangedEventArgs e)
    {
        try
        {
            int fv = vm.AddressMode == 2 ? vm.job1.FLAG : vm.AddressMode == 3 ? vm.job2.FLAG : 0;
            int fv2 = vm.AddressMode == 2 ? vm.job1.FLAG2 : vm.AddressMode == 3 ? vm.job2.FLAG2 : 0;
            int v1 = 0;
            int v2 = 0;


            // Items just selected
            foreach (var item in e.CurrentSelection.Except(e.PreviousSelection)) //item selected
            {
                if (item is SlotType)
                {
                    if (((SlotType) item).value == 2048)
                    {
                        v2 = ((SlotType)item).value;
                        fv2 = fv2 | v2;
                    } else
                    {
                        v1 = ((SlotType)item).value;
                        fv = fv | v1;
                    }
                }
            }
            // Items just un-selected
            foreach (var item in e.PreviousSelection.Except(e.CurrentSelection)) //item un-selected
            {
                if (((SlotType)item).value == 2048)
                {
                    v2 = ((SlotType)item).value;
                    fv2 = fv2 &= ~v2;
                }
                else
                {
                    v1 = ((SlotType)item).value;
                    fv = fv &= ~v1;
                }
            }

            if (vm.AddressMode == 2)
            {
                vm.job1.FLAG = fv;
                vm.job1.FLAG2 = fv2;
                lblv1.Text = fv.ToString();
                lblv2.Text = fv2.ToString();
            }
            else if (vm.AddressMode == 3)
            {
                vm.job2.FLAG = fv;
                vm.job2.FLAG2 = fv2;
                lblv1.Text = fv.ToString();
                lblv2.Text = fv2.ToString();
            }
        }
        catch (Exception ex)
        {
            string s = ex.Message;
        }
    }

    async void btnCancel_Clicked(System.Object sender, System.EventArgs e)
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

    async void btnSave_Clicked(System.Object sender, System.EventArgs e)
    {
        try
        {
            string errmsg = "";
            if (canSave(ref errmsg))
            {
                save();
                BackToCreateJob();
            }
            else
            {
                common.showAlert(errmsg);
            }
        }
        catch (Exception ex)
        {
            string s = ex.Message;
        }
    }

    async void btnDefAddress_Clicked(System.Object sender, System.EventArgs e)
    {
        try
        {
            if (NetworkHelper.IsDisconnected())
            {
                await DisplayAlertAsync("No Internet Connection", "Please check your internet connection and try again.", "OK");
                return;
            }

            if (vm.txtPostalTextChangedSubscribed)
                txtPostalTextChangedUnSubscribed();
            await getDefAddress();
            if (!vm.txtPostalTextChangedSubscribed)
                txtPostalTextChangedSubscribed();
        }
        catch (Exception ex)
        {
            string s = ex.Message;
        }
    }

    void btnNewAddress_Clicked(System.Object sender, System.EventArgs e)
    {
        resetFieldsForNewAddress();
    }

    async void btnAddressBk_Clicked(System.Object sender, System.EventArgs e)
    {
        try
        {
            _returningFromAddressBook = true; // set flag before leaving
            await Shell.Current.GoToAsync("AddressBookPage", new Dictionary<string, object>
                    {
                        { "vmm", vm },
                        { "titleName", title },
                        {"LOGININFO",  logininfo},
                        { "LTR", ltr }
                    });
        }
        catch (Exception ex)
        {
            string s = ex.Message;
        }
    }

    private async void OnEntryCompleted(object sender, EventArgs e)
    {
        try
        {
            string postal = txtPostal.Text;
            if (!String.IsNullOrEmpty(postal) && postal.Length == 6)
            {
                await SearchPostal();
            }
        } catch (Exception ex)
        {
            string s = ex.Message;
        }
    }

    private async void OnEntryUnfocused(object sender, FocusEventArgs args)
    {
        try
        {
            bool btnTopNBackFocused = btnTopNBack.IsFocused;
            bool DefAddBtnFocused = btnDefAddress.IsFocused;
            bool AddBkBtnFocused = btnAddressBk.IsFocused;
            bool CxBtnFocused = btnCancel.IsFocused;
            bool SaveBtnFocused = btnSave.IsFocused;

            bool anyBtnFocused = btnTopNBackFocused || DefAddBtnFocused || AddBkBtnFocused || CxBtnFocused || SaveBtnFocused;

            if (!anyBtnFocused)
            {
                string selectedPostal = lblSelectedPostal.Text;
                string postal = txtPostal.Text;
                if (!string.IsNullOrEmpty(postal) && postal.Length == 6 && !postal.Equals(selectedPostal))
                {
                    await SearchPostal();
                }
                else if (!string.IsNullOrEmpty(postal) && postal.Length > 0 && postal.Length < 6 && !string.IsNullOrEmpty(selectedPostal))
                {
                    txtPostal.Text = selectedPostal;
                }
                else if (string.IsNullOrEmpty(postal))
                {
                    //lblSelectedPostal.Text = "";
                }

                txtPostal.IsEnabled = false;
                txtPostal.IsEnabled = true;
            }
        } catch (Exception e)
        {
            string s = e.Message;
        }
    }

    private async void OnEntryTextChanged(object sender, TextChangedEventArgs e)
    {
        try
        {
            // e.OldTextValue contains the previous text
            // e.NewTextValue contains the current text
            string oldText = e.OldTextValue;
            string newText = e.NewTextValue;

            if (!string.IsNullOrEmpty(newText) && newText.Length == 6)
            {
                await SearchPostal();
            } else if (!string.IsNullOrEmpty(oldText) && oldText.Length == 6 && oldText.Length > newText.Length)
            {
                resetFieldsForPostalChange();
            }
        } catch (Exception ex)
        {
            string s = ex.Message;
        }
    }

    private void resetFieldsForNewAddress()
    {
        try
        {
            if (vm.txtPostalTextChangedSubscribed)
                txtPostalTextChangedUnSubscribed();

            btnDefAddress.IsVisible = true;
            btnNewAddress.IsVisible = false;

            cvLocationType.SelectedItem = null;
            if (vm != null)
                vm.selectedLocationType = 0;
            txtPostal.Text = "";
            lblSelectedPostal.Text = "";
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
            txtStreet.IsReadOnly = false;
            txtBldg.IsReadOnly = false;
            txtCompany.IsReadOnly = false;

            if (vm.AddressMode == 1)
                vm.ColAddress = null;
            if (vm.AddressMode == 2)
                vm.DelAddress = null;
            if (vm.AddressMode == 3)
                vm.RtnAddress = null;

            if (!vm.txtPostalTextChangedSubscribed)
                txtPostalTextChangedSubscribed();
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
            if (vm.txtPostalTextChangedSubscribed)
                txtPostalTextChangedUnSubscribed();

            btnDefAddress.IsVisible = true;
            btnNewAddress.IsVisible = false;

            cvLocationType.SelectedItem = null;
            if (vm != null)
                vm.selectedLocationType = 0;

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
            txtStreet.IsReadOnly = false;
            txtBldg.IsReadOnly = false;
            txtCompany.IsReadOnly = false;

            if (vm.AddressMode == 1)
                vm.ColAddress = null;
            if (vm.AddressMode == 2)
                vm.DelAddress = null;
            if (vm.AddressMode == 3)
                vm.RtnAddress = null;

            if (!vm.txtPostalTextChangedSubscribed)
                txtPostalTextChangedSubscribed();
        }
        catch (Exception ex)
        {
            string s = ex.Message;
        }
    }

    private void OnBackgroundClicked(object sender, TappedEventArgs e)
    {
        try
        {
            if (txtPostal.IsFocused)
                txtPostal.Unfocus();
        } catch (Exception ex)
        {
            string s = ex.Message;
        }

    }

    private bool canSave(ref string errmsg)
    {
        bool asd = true;
        try
        {
            int LocationType = vm.selectedLocationType;
            if (LocationType == 0)
            {
                errmsg = "Please select Address Category, RESIDENTIAL or OFFICE.";
                return false;
            }
            string postal = txtPostal.Text;
            if (String.IsNullOrEmpty(postal))
            {
                errmsg = "Please enter POSTAL.";
                return false;
            }
            string blk = txtBlock.Text;
            if (String.IsNullOrEmpty(blk))
            {
                errmsg = "Please enter BLOCK.";
                return false;
            }
            string street = txtStreet.Text;
            if (String.IsNullOrEmpty(street))
            {
                errmsg = "Please enter STREET.";
                return false;
            }
            string name = txtContactName.Text;
            if (String.IsNullOrEmpty(name))
            {
                errmsg = "Please enter CONTACT NAME.";
                return false;
            }
            string tel = txtTel.Text;
            string mobile = txtHp.Text;
            if (String.IsNullOrEmpty(tel) && String.IsNullOrEmpty(mobile))
            {
                errmsg = "Please enter a Telephone or Mobile number.";
                return false;
            }
        }
        catch (Exception e)
        {
            string s = e.Message;
        }
        return asd;
    }

    private void save()
    {
        try
        {
            if (vm != null)
            {
                /*XDelServiceRef.AddressStructure? ads = new XDelServiceRef.AddressStructure();
                XDelServiceRef.ContactStructure? cts = new XDelServiceRef.ContactStructure();*/

                XDelServiceRef.AddressStructure? ads = null;
                XDelServiceRef.ContactStructure? cts = null;

                if (vm.AddressMode == 1)
                    ads = vm.ColAddress;
                if (vm.AddressMode == 2)
                    ads = vm.DelAddress;
                if (vm.AddressMode == 3)
                    ads = vm.RtnAddress;

                if (ads == null || ads.IDX == 0)
                {
                    if (ads == null)
                        ads = new XDelServiceRef.AddressStructure();

                    ads.LocationType = vm.selectedLocationType == 1 ? XDelServiceRef.Location_Type.Residential : XDelServiceRef.Location_Type.Office;

                    ads.POSTALCODE = txtPostal.Text;
                    ads.BLOCK = txtBlock.Text;
                    ads.UNIT = txtUnit.Text;
                    ads.STREET = txtStreet.Text;
                    ads.BUILDING = txtBldg.Text;
                    ads.COMPANY = txtCompany.Text;
                    ads.ACTIVE = true;

                    cts = (ads.Contacts != null && ads.Contacts.Length > 0) ? ads.Contacts[0] : new XDelServiceRef.ContactStructure();

                    cts.NAME = txtContactName.Text;
                    cts.TEL = txtTel.Text;
                    cts.MOBILE = txtHp.Text;
                    cts.ACTIVE = true;

                    ads.Contacts = new XDelServiceRef.ContactStructure[] { cts };
                }

                //this is here in case locationtype was not stated b4, tel n hp not stated b4 from addressbook
                ads.LocationType = vm.selectedLocationType == 1 ? XDelServiceRef.Location_Type.Residential : XDelServiceRef.Location_Type.Office;
                cts = (ads.Contacts != null && ads.Contacts.Length > 0) ? ads.Contacts[0] : new XDelServiceRef.ContactStructure();
                cts.TEL = txtTel.Text;
                cts.MOBILE = txtHp.Text;
                ads.Contacts = new XDelServiceRef.ContactStructure[] { cts };
                //this is here in case locationtype was not stated b4, tel n hp not stated b4 from addressbook

                if (vm.AddressMode == 1)
                    vm.ColAddressFinal = ads;
                if (vm.AddressMode == 2)
                    vm.DelAddressFinal = ads;
                if (vm.AddressMode == 3)
                    vm.RtnAddressFinal = ads;

                if (vm.AddressMode == 1)
                {
                    vm.PUSI = txtInstruction.Text;
                    vm.job1.PUSI = txtInstruction.Text;
                }
                else if (vm.AddressMode == 2)
                {
                    vm.DLSI = txtInstruction.Text;
                    vm.job1.DLSI = txtInstruction.Text;
                }
                else if (vm.AddressMode == 3)
                {
                    vm.DLSI2 = txtInstruction.Text;
                    vm.job2.DLSI = txtInstruction.Text;
                } 
            }
        }
        catch (Exception e)
        {
            string s = e.Message;
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
                    lblSelectedPostal.Text = ads.POSTALCODE;

                    if (vm.AddressMode == 1)
                        vm.ColAddress = null;
                    if (vm.AddressMode == 2)
                        vm.DelAddress = null;
                    if (vm.AddressMode == 3)
                        vm.RtnAddress = null;

                    loadDefAddress(ads);
                    await closeProgress_dialog();
                }
            }
        }
        catch (Exception ex)
        {
            if (_progressService != null && _progressService.IsShowing)
                await closeProgress_dialog();

            string s = ex.Message;
        }
        finally
        {
            if (_progressService != null && _progressService.IsShowing)
                await closeProgress_dialog();
        }
    }

    private void loadDefAddress(XDelServiceRef.AddressStructure defAddress, bool isDefAdd = false)
    {
        cvSlot.SelectionChanged -= cvSlotOnItemSelected;
        try
        {
            logininfo = AppSession.logininfo;
            XDelServiceRef.ContactStructure? cts = defAddress.Contacts != null && defAddress.Contacts.Length > 0 ? defAddress.Contacts[0] : null;

            txtPostal.Text = defAddress.POSTALCODE;
            txtBlock.Text = defAddress.BLOCK;
            txtUnit.Text = defAddress.UNIT;
            txtStreet.Text = defAddress.STREET;
            txtBldg.Text = defAddress.BUILDING;
            txtCompany.Text = defAddress.COMPANY;

            txtContactName.Text = cts != null && !String.IsNullOrEmpty(cts.NAME) ? cts.NAME : "";
            txtTel.Text = cts != null && !String.IsNullOrEmpty(cts.TEL) ? cts.TEL : "";
            txtHp.Text = cts != null && !String.IsNullOrEmpty(cts.MOBILE) ? cts.MOBILE : "";

            int SelectedLocationType = defAddress.LocationType == XDelServiceRef.Location_Type.Office ? 2 : defAddress.LocationType == XDelServiceRef.Location_Type.Residential ? 1 : 0;

            int cvLocationTypeCount = cvLocationType.ItemsSource is List<LocationTypeDisp> collection ? collection.Count : 0;
            if (cvLocationTypeCount == 0)
                loadLocationType();

            cvLocationType.SelectedItem = null;
            if (vm != null)
                vm.selectedLocationType = 0;

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

            cvLocationType.InputTransparent = isDefAdd && defAddress.IDX > 0 && SelectedLocationType > 0 ? true : false;

            txtPostal.IsReadOnly = isDefAdd && defAddress.IDX > 0 ? true : false;
            txtBlock.IsReadOnly = isDefAdd && defAddress.IDX > 0 ? true : false;
            txtUnit.IsReadOnly = isDefAdd && defAddress.IDX > 0 ? true : false;
            txtStreet.IsReadOnly = true;
            txtBldg.IsReadOnly = isDefAdd && defAddress.IDX > 0 ? true : false;
            txtCompany.IsReadOnly = isDefAdd && defAddress.IDX > 0 ? true : false;

            txtContactName.IsReadOnly = isDefAdd && cts != null && cts.IDX > 0 ? true : false;
            bool telHpEmpty = string.IsNullOrEmpty(txtTel.Text.Trim().ToString()) && string.IsNullOrEmpty(txtHp.Text.Trim().ToString());
            txtTel.IsReadOnly = isDefAdd && cts != null && cts.IDX > 0  && !telHpEmpty ? true : false;
            txtHp.IsReadOnly = isDefAdd && cts != null && cts.IDX > 0 && !telHpEmpty ? true : false;

            btnDefAddress.IsVisible = !isDefAdd;
            btnNewAddress.IsVisible = isDefAdd;


            cvSlot.SelectionChanged -= cvSlotOnItemSelected;
            int fv = vm.AddressMode == 2 ? vm.job1.FLAG : vm.AddressMode == 3 ? vm.job2.FLAG : 0;
            int fv2 = vm.AddressMode == 2 ? vm.job1.FLAG2 : vm.AddressMode == 3 ? vm.job2.FLAG2 : 0;

            lblv1.Text = vm.AddressMode == 2 ? vm.job1.FLAG.ToString() : vm.AddressMode == 3 ? vm.job2.FLAG.ToString() : "0";
            lblv2.Text = vm.AddressMode == 2 ? vm.job1.FLAG2.ToString() : vm.AddressMode == 3 ? vm.job2.FLAG2.ToString() : "0";

            cvSlot.SelectedItems.Clear();
            if (cvSlot.ItemsSource is List<SlotType> itemss)
            {
                SlotType item;
                for (int i = 0; i <= itemss.Count - 1; i++)
                {
                    item = itemss[i];
                    if (item.value == 2048 && ((fv2 & 2048) == 2048))
                    {
                        cvSlot.SelectedItems.Add(item);
                    } else if (((fv & item.value) == item.value))
                    {
                        cvSlot.SelectedItems.Add(item);
                    }
                }
            }
            cvSlot.SelectionChanged += cvSlotOnItemSelected;

            txtInstruction.Text = vm.AddressMode == 1 ? vm.PUSI : vm.AddressMode == 2 ? vm.DLSI : vm.DLSI2;
        }
        catch (Exception e)
        {
            string s = e.Message;
        } finally
        {
            // Guaranteed to resubscribe even if exception thrown
            cvSlot.SelectionChanged += cvSlotOnItemSelected;
        }
    }

    private async Task getDefAddress()
    {
        XDelServiceRef.AddressStructure? ads = null;
        try
        {
            if (NetworkHelper.IsDisconnected())
            {
                await DisplayAlertAsync("No Internet Connection", "Please check your internet connection and try again.", "OK");
                return;
            }

            if (logininfo != null && logininfo.clientInfo != null && !String.IsNullOrEmpty(logininfo.clientInfo.Web_UID) && logininfo.clientInfo.CAIDX > 0)
            {
                await showProgress_Dialog("Processing...");

                ClientInfo ci = logininfo.clientInfo;
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
                            ads = searchedAddress.AddressList[i];
                            break;
                        }
                    }
                }
                if (ads != null)
                {
                    lblSelectedPostal.Text = ads.POSTALCODE;

                    XDelServiceRef.ContactStructure? cts = null;
                    if (logininfo != null && logininfo.clientInfo != null && ads.Contacts != null && ads.Contacts.Length > 0)
                    {
                        foreach (var c in ads.Contacts)
                        {
                            if (c.IDX == logininfo.clientInfo.CNIDX)
                            {
                                cts = c;
                            }
                        }
                        if (cts == null)
                            cts = ads.Contacts[0];
                    }

                    if (cts != null)
                        ads.Contacts = new XDelServiceRef.ContactStructure[1] { cts };

                    if (vm.AddressMode == 1)
                        vm.ColAddress = ads;
                    if (vm.AddressMode == 2)
                        vm.DelAddress = ads;
                    if (vm.AddressMode == 3)
                        vm.RtnAddress = ads;

                    loadDefAddress(ads, true);
                    await closeProgress_dialog();
                }
            }
            else
            {
                await closeProgress_dialog();
                await DisplayAlertAsync("Session expired", "Please Login again.", "OK");
                await common.BackToLogin();
            }
        }
        catch (Exception e)
        {
            if (_progressService != null && _progressService.IsShowing)
                await closeProgress_dialog();

            string s = e.Message;
        }
        finally
        {
            if (_progressService != null && _progressService.IsShowing)
                await closeProgress_dialog();
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