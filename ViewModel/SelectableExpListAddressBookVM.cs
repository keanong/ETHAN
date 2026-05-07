using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using ETHAN.classes;
using System.Collections.ObjectModel;


namespace ETHAN.ViewModel
{
    public partial class SelectableExpListAddressBookVM : ObservableObject
    {
        public string? cri1 { get; set; }
        public string? cri2 { get; set; }

        private XDelServiceRef.AddressStructure[]? searchedAddress;

        [ObservableProperty]
        private ObservableCollection<AddressHeader> items = new();

        #region Paging

        [ObservableProperty]
        private bool canLoadMore = false;

        private int currentPage = 1;
        private const int pageSize = 20; // adjust as you wish

        [RelayCommand]
        private async Task LoadMoreAsync()
        {
            try
            {
                CanLoadMore = false; // disable button while loading

                var moreData = await LoadNextPageAsync(currentPage + 1);

                if (moreData == null || moreData.Count == 0)
                {
                    // no more data
                    return;
                }

                foreach (var header in moreData)
                    Items.Add(header);

                currentPage++;
                //Place this check here — AFTER items are added
                if (searchedAddress != null && Items.Count >= searchedAddress.Length)
                {
                    // all items loaded, hide the button
                    CanLoadMore = false;
                }
                else
                {
                    // still have more to load
                    CanLoadMore = true;
                }
            }
            catch (Exception ex)
            {
                //Console.WriteLine($"LoadMore error: {ex.Message}");
                //CanLoadMore = true;
                string s = ex.Message;
                CanLoadMore = false;
            }
        }

        /// <summary>
        /// Simulate getting the next page of data
        /// Replace this with your real data fetch logic.
        /// </summary>
        private async Task<List<AddressHeader>> LoadNextPageAsync(int page)
        {
            var list = new List<AddressHeader>();
            try
            {
                await Task.Delay(300); // simulate async delay

                if (searchedAddress == null)
                    return new List<AddressHeader>();

                int startIndex = (page - 1) * pageSize;
                int endIndex = Math.Min(searchedAddress.Length, startIndex + pageSize);

                if (startIndex >= endIndex)
                    return new List<AddressHeader>();

                //var list = new List<AddressHeader>();

                for (int i = startIndex; i < endIndex; i++)
                {
                    var ads = searchedAddress[i];
                    if (ads == null || ads.Contacts == null || ads.Contacts.Length == 0)
                        continue;

                    var header = BuildAddressHeaderFromAddressStructure(ads, canManageAddressBook: true, canAddAuthShippers: true);
                    if (header != null)
                        list.Add(header);
                }
            } catch (Exception e)
            {
                string s = e.Message;
            }

            return list;
        }

        // Optional helper method (factor out your header-building logic)
        private AddressHeader BuildAddressHeaderFromAddressStructure(XDelServiceRef.AddressStructure ads, bool canManageAddressBook, bool canAddAuthShippers)
        {
            XDelServiceRef.AddressStructure? nads = null;
            XDelServiceRef.ContactStructure? cts = null;
            AddressHeader AH = null;
            AddressChild AC = null;
            string postal = "";
            string blk = "";
            string unit = "";
            string street = "";
            string bldg = "";
            string company = "";
            string name = "";
            string dept = "";
            string tel = "";
            string mobile = "";
            string acfulltext = "";

            string address0 = "";
            string address1 = "";
            string addv = "";
            int locationType = 0;

            try
            {
                if (ads != null && ads.Contacts != null && ads.Contacts.Length > 0)
                    cts = ads.Contacts[0];
                if (ads != null && ads.Contacts != null && ads.Contacts.Length > 0)
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

                    address0 = !string.IsNullOrEmpty(company) ? company : "";
                    address1 = blk + street + unit + bldg + (!string.IsNullOrEmpty(postal) ? ", " + postal : "");

                    addv = !string.IsNullOrEmpty(address0) && !string.IsNullOrEmpty(address1) ? address0 + "\r\n" + address1 :
                        string.IsNullOrEmpty(address0) && !string.IsNullOrEmpty(address1) ? address1 :
                        !string.IsNullOrEmpty(address0) && string.IsNullOrEmpty(address1) ? address0 : "";

                    locationType = ads.LocationType == XDelServiceRef.Location_Type.Residential ? 1 : ads.LocationType == XDelServiceRef.Location_Type.Office ? 2 : 2;

                    AH = new AddressHeader
                    {
                        Title = addv, Caidx = ads.IDX, AddressType = ads.ADDRESSTYPE, Company = company,
                        Block = blk, Street = street, Unit = unit, Building = bldg, Postalcode = postal, Active = ads.ACTIVE,
                        LocationTypeInt = locationType, CanManageAddressBook = canManageAddressBook, CanAddAuthShippers = canAddAuthShippers, AddressStructure_ = ads
                    };

                    foreach (var c in ads.Contacts)
                    {
                        cts = c;
                        name = !string.IsNullOrEmpty(c.NAME) ? c.NAME : "";
                        dept = !string.IsNullOrEmpty(c.DEPARTMENT) ? c.DEPARTMENT : "";
                        tel = !string.IsNullOrEmpty(c.TEL) ? "Tel: " + c.TEL : "";
                        mobile = !string.IsNullOrEmpty(c.MOBILE) ? "Mobile: " + c.MOBILE : "";
                        acfulltext = !string.IsNullOrEmpty(name) ? name
                                    + (!string.IsNullOrEmpty(dept) ? "\r\n" + dept : "")
                                    + (!string.IsNullOrEmpty(tel) ? "\r\n" + tel : "")
                                        + (!string.IsNullOrEmpty(mobile) ? "\r\n" + mobile : "") : "";
                        acfulltext += "\r\n\r\nClick here to select";

                        nads = Views.AddressBookPage.DeepClone(ads);
                        nads.Contacts = new XDelServiceRef.ContactStructure[] { c };

                        /*AC = new AddressChild
                        {
                            fulltext = acfulltext, NAME = name, TEL = tel, MOBILE = mobile, DEPARTMENT = dept, ACTIVE = cts.ACTIVE,
                            addressStructure = nads, CAIDX = ads.IDX, CNIDX = cts.IDX, AddressHeader = AH, 
                            CanManageAddressBook = canManageAddressBook, CanAddAuthShippers = canAddAuthShippers
                        };*/
                        AC = new AddressChild
                        {
                            Fulltext = acfulltext, Name = name, Tel = tel, Mobile = mobile, Department = dept, 
                            Active = cts.ACTIVE, AddressStructure = nads, Caidx = ads.IDX, Cnidx = cts.IDX, AddressHeader = AH,
                            CanManageAddressBook = canManageAddressBook, CanAddAuthShippers = canAddAuthShippers, ConstactStructure_ = cts
                        };
                        AH.Children.Add(AC);
                    }
                }
            } catch (Exception e)
            {
                string s = e.Message;
            }
            return AH;
        }

        private void LoadInitialPage(bool canManageAddressBook, bool canAddAuthShippers)
        {
            try
            {

                int startIndex = 0;
                int endIndex = Math.Min(pageSize, searchedAddress?.Length ?? 0);

                if (endIndex == 0)
                {
                    CanLoadMore = false; // no items to show
                    return;
                }

                for (int i = startIndex; i < endIndex; i++)
                {
                    var ads = searchedAddress[i];
                    if (ads == null || ads.Contacts == null || ads.Contacts.Length == 0)
                        continue;

                    var header = BuildAddressHeaderFromAddressStructure(ads, canManageAddressBook, canAddAuthShippers);
                    if (header != null)
                        Items.Add(header);
                }

                currentPage = 1;
                //CanLoadMore = (searchedAddress?.Length ?? 0) > pageSize;
                CanLoadMore = (searchedAddress != null && searchedAddress.Length > pageSize); // show button only if more pages
            } catch (Exception e)
            {
                string s = e.Message;
            }
        }

        public void setsearchedAddress(XDelServiceRef.AddressStructure[]? ads)
        {
            try
            {
                searchedAddress = ads;
                Items.Clear(); // reset previous items

                if (searchedAddress != null && searchedAddress.Length > 0)
                {
                    // load first page
                    LoadInitialPage(canManageAddressBook: true, canAddAuthShippers: true);
                }
                else
                {
                    CanLoadMore = false; // no data, hide button
                }
            }
            catch (Exception e)
            {
                string s = e.Message;
            }
        }

        #endregion

        public async Task update_searchedAddressContact(long CAIDX, long CNIDX, XDelServiceRef.ContactStructure? cts, bool remove = false)
        {
            try
            {
                if (searchedAddress == null || (searchedAddress != null && searchedAddress.Length == 0))
                    return;

                if (cts == null)
                    return;


                if (remove && searchedAddress != null && searchedAddress.Length > 0)
                {
                    await MainThread.InvokeOnMainThreadAsync(() =>
                    {
                        foreach (XDelServiceRef.AddressStructure ads in searchedAddress)
                        {
                            if (ads.IDX == CAIDX && ads.Contacts != null && ads.Contacts.Length > 0)
                            {
                                var contactList = ads.Contacts.ToList();
                                // Remove all contacts matching the IDX
                                contactList.RemoveAll(c => c.IDX == cts.IDX);
                                // Assign back to the array
                                ads.Contacts = contactList.ToArray();
                                break;
                            }
                        }

                        if (Items != null && Items.Count > 0)
                        {
                            foreach (AddressHeader ah in Items)
                            {
                                if (ah.Caidx == CAIDX && ah.Children != null && ah.Children.Count > 0)
                                {
                                    // Remove all contacts matching the IDX directly from ObservableCollection
                                    var toRemove = ah.Children
                                        .Where(c => c.Cnidx == cts.IDX)
                                        .ToList(); // Make a copy to avoid modifying while iterating

                                    foreach (var c in toRemove)
                                        ah.Children.Remove(c);
                                    break;
                                }
                            }
                        }
                    });
                }

                if (remove)
                    return;

                ////include new contact as children
                if (CNIDX == 0 && searchedAddress != null && searchedAddress.Length > 0)
                {
                    foreach (XDelServiceRef.AddressStructure ads in searchedAddress)
                    {
                        if (ads.IDX == CAIDX && ads.Contacts != null)
                        {
                            if (ads.Contacts == null)
                                ads.Contacts = new XDelServiceRef.ContactStructure[] { cts };
                            else
                            {
                                var newlist = ads.Contacts.ToList();
                                newlist.Add(cts);
                                ads.Contacts = newlist.ToArray();
                            }
                            break;
                        }
                    }

                    if (Items != null && Items.Count > 0)
                    {
                        AddressHeader ah = null;
                        for (int i = 0; i <= Items.Count -1; i++)
                        {
                            ah = Items[i];
                            if (ah.Caidx == CAIDX)
                            {
                                if (ah.Children == null)
                                    ah.Children ??= new ObservableCollection<AddressChild>();

                                string name = !string.IsNullOrEmpty(cts.NAME) ? cts.NAME : "";
                                string dept = !string.IsNullOrEmpty(cts.DEPARTMENT) ? cts.DEPARTMENT : "";
                                string tel = !string.IsNullOrEmpty(cts.TEL) ? "Tel: " + cts.TEL : "";
                                string mobile = !string.IsNullOrEmpty(cts.MOBILE) ? "Mobile: " + cts.MOBILE : "";
                                string acfulltext = !string.IsNullOrEmpty(name) ? name
                                            + (!string.IsNullOrEmpty(dept) ? "\r\n" + dept : "")
                                            + (!string.IsNullOrEmpty(tel) ? "\r\n" + tel : "")
                                             + (!string.IsNullOrEmpty(mobile) ? "\r\n" + mobile : "") : "";

                                XDelServiceRef.AddressStructure nads = Views.AddressBookPage.DeepClone(selectedAddressStructure);
                                nads.Contacts = new XDelServiceRef.ContactStructure[] { cts };

                                AddressChild AC = new AddressChild
                                {
                                    Fulltext = acfulltext, Name = name, Tel = tel, Mobile = mobile, Department = dept, Active = cts.ACTIVE,
                                    AddressStructure = nads, Caidx = CAIDX, Cnidx = cts.IDX, AddressHeader = ah,
                                    CanManageAddressBook = ah.CanManageAddressBook, CanAddAuthShippers = ah.CanAddAuthShippers, ConstactStructure_ = cts
                                };
                                await MainThread.InvokeOnMainThreadAsync(() =>
                                {
                                    Items[i].Children.Add(AC);
                                });
                                break;
                            }
                        }
                    }
                }

                ////update children
                if (CNIDX > 0 && searchedAddress != null && searchedAddress.Length > 0)
                {
                    foreach (XDelServiceRef.AddressStructure ads in searchedAddress)
                    {
                        if (ads.IDX == CAIDX && ads.Contacts != null)
                        {
                            foreach (XDelServiceRef.ContactStructure c in ads.Contacts)
                            {
                                if (c.IDX == CNIDX)
                                {
                                    c.NAME = cts.NAME;
                                    c.DEPARTMENT = cts.DEPARTMENT;
                                    c.ACTIVE = cts.ACTIVE;
                                    c.TEL = cts.TEL;
                                    c.MOBILE = cts.MOBILE;
                                    break;
                                }
                            }
                            break;
                        }
                    }

                    if (Items != null && Items.Count > 0)
                    {
                        foreach (AddressHeader ah in Items)
                        {
                            if (ah.Caidx == CAIDX && ah.Children != null && ah.Children.Count > 0)
                            {
                                foreach (AddressChild ac in ah.Children)
                                {
                                    if (ac.Cnidx == CNIDX)
                                    {
                                        string name = !string.IsNullOrEmpty(cts.NAME) ? cts.NAME : "";
                                        string dept = !string.IsNullOrEmpty(cts.DEPARTMENT) ? cts.DEPARTMENT : "";
                                        string tel = !string.IsNullOrEmpty(cts.TEL) ? "Tel: " + cts.TEL : "";
                                        string mobile = !string.IsNullOrEmpty(cts.MOBILE) ? "Mobile: " + cts.MOBILE : "";
                                        string acfulltext = !string.IsNullOrEmpty(name) ? name
                                            + (!string.IsNullOrEmpty(dept) ? "\r\n" + dept : "")
                                            + (!string.IsNullOrEmpty(tel) ? "\r\n" + tel : "")
                                             + (!string.IsNullOrEmpty(mobile) ? "\r\n" + mobile : "") : "";

                                        ac.Name = cts.NAME;
                                        ac.Department = cts.DEPARTMENT;
                                        ac.Active = cts.ACTIVE;
                                        ac.Tel = cts.TEL;
                                        ac.Mobile = cts.MOBILE;
                                        ac.Fulltext = acfulltext;

                                        XDelServiceRef.AddressStructure? ads = ac.AddressStructure;
                                        if (ads != null)
                                        {
                                            ads.Contacts = new XDelServiceRef.ContactStructure[] { cts };
                                            ac.AddressStructure = ads;
                                        }
                                        break;
                                    }
                                }

                                break;
                            }
                        }
                    }
                }

            } catch (Exception e)
            {
                string s = e.Message;
            }
        }

        public async void update_searchedAddressAddress(long CAIDX, XDelServiceRef.AddressStructure? adds, bool canManageAddressBook, bool canAddAuthShippers, bool remove = false)
        {
            try
            {

                if (CAIDX > 0 && searchedAddress == null || (searchedAddress != null && searchedAddress.Length == 0))
                    return;

                if (adds == null)
                    return;

                if (remove && CAIDX > 0 && searchedAddress != null && searchedAddress.Length > 0)
                {
                    await MainThread.InvokeOnMainThreadAsync(() =>
                    {
                        var addressList = searchedAddress.ToList();
                        // Remove all addressstructure matching the IDX
                        addressList.RemoveAll(ads => ads.IDX == CAIDX);
                        // Assign back to the array
                        searchedAddress = addressList.ToArray();

                        if (Items != null && Items.Count > 0)
                        {
                            var toRemove = Items
                                        .Where(it => it.Caidx == CAIDX)
                                        .ToList(); // Make a copy to avoid modifying while iterating

                            foreach (var c in toRemove)
                                Items.Remove(c);
                        }

                    });
                }

                if (remove)
                    return;

                ////include new address
                if (CAIDX == 0 && adds != null)
                {
                    await MainThread.InvokeOnMainThreadAsync(() =>
                    {

                        if (CAIDX == 0 && searchedAddress == null || (searchedAddress != null && searchedAddress.Length == 0))
                            searchedAddress = new XDelServiceRef.AddressStructure[] { new XDelServiceRef.AddressStructure() };

                        if (CAIDX == 0 && searchedAddress != null && searchedAddress.Length == 1 && searchedAddress[0].IDX == 0)
                            searchedAddress[0] = adds;

                        if (CAIDX == 0 && searchedAddress != null && searchedAddress.Length > 1)
                        {
                            var newlist = searchedAddress.ToList();
                            newlist.Add(adds);
                            searchedAddress = newlist.ToArray();
                        }

                        if (Items == null)
                            Items = new ObservableCollection<AddressHeader>();

                        var header = BuildAddressHeaderFromAddressStructure(adds, canManageAddressBook, canAddAuthShippers);
                        if (header != null)
                            Items.Add(header);

                    });
                }

                ////update address
                if (CAIDX > 0 && searchedAddress != null && searchedAddress.Length > 0)
                {
                    await MainThread.InvokeOnMainThreadAsync(() =>
                    {
                        for (int i = 0; i <= searchedAddress.Length -1; i++)
                        {
                            if (searchedAddress[i].IDX == CAIDX)
                            {
                                searchedAddress[i] = adds;
                                break;
                            }
                        }

                        if (Items != null && Items.Count > 0)
                        {
                            foreach (AddressHeader ah in Items)
                            {
                                if (ah.Caidx == CAIDX)
                                {
                                    string postal = !string.IsNullOrEmpty(adds.POSTALCODE) ? adds.POSTALCODE : "";
                                    string blk = !string.IsNullOrEmpty(adds.BLOCK) ? adds.BLOCK + " " : "";
                                    string unit = !string.IsNullOrEmpty(adds.UNIT) ? adds.UNIT + " " : "";
                                    string street = !string.IsNullOrEmpty(adds.STREET) ? adds.STREET + " " : "";
                                    string bldg = !string.IsNullOrEmpty(adds.BUILDING) ? adds.BUILDING : "";
                                    string company = !string.IsNullOrEmpty(adds.COMPANY) ? adds.COMPANY : "";
                                    string address0 = !string.IsNullOrEmpty(company) ? company : "";
                                    string address1 = blk + street + unit + bldg + (!string.IsNullOrEmpty(postal) ? ", " + postal : "");

                                    string addv = !string.IsNullOrEmpty(address0) && !string.IsNullOrEmpty(address1) ? address0 + "\r\n" + address1 :
                                        string.IsNullOrEmpty(address0) && !string.IsNullOrEmpty(address1) ? address1 :
                                        !string.IsNullOrEmpty(address0) && string.IsNullOrEmpty(address1) ? address0 : "";
                                    int locationType = 0;
                                    locationType = adds.LocationType == XDelServiceRef.Location_Type.Residential ? 1 : adds.LocationType == XDelServiceRef.Location_Type.Office ? 2 : 2;

                                    ah.Title = addv;
                                    ah.Company = company;
                                    ah.Block = blk;
                                    ah.Street = street;
                                    ah.Unit = unit;
                                    ah.Building = bldg;
                                    ah.Postalcode = postal;
                                    ah.LocationTypeInt = locationType;
                                    ah.AddressStructure_ = adds;

                                    if (ah.Children != null)
                                    {
                                        foreach (AddressChild ac in ah.Children)
                                        {
                                            XDelServiceRef.AddressStructure nads = Views.AddressBookPage.DeepClone(adds);
                                            nads.Contacts = ac.ConstactStructure_ != null ? new XDelServiceRef.ContactStructure[] { ac.ConstactStructure_ } : null;
                                            ac.AddressStructure = nads;
                                        }
                                    }
                                    
                                    break;
                                }
                            }
                        }
                    });
                }
            } catch (Exception e)
            {
                string s = e.Message;
            }
        }

        public SelectableExpListAddressBookVM()
        {
            CanLoadMore = false; // no data, hide button
        }

        public SelectableExpListAddressBookVM(XDelServiceRef.AddressStructure[]? list, bool CanManageAddressBook, bool CanAddAuthShippers)
        {
            try
            {
                if (Items == null)
                    Items = new ObservableCollection<AddressHeader>();

                if (list == null)
                    return;


                // store full list for later paging
                searchedAddress = list;

                // load only the first page
                LoadInitialPage(CanManageAddressBook, CanAddAuthShippers);

                /*XDelServiceRef.AddressStructure? ads = null;
                XDelServiceRef.AddressStructure? nads = null;
                XDelServiceRef.ContactStructure? cts = null;
                AddressHeader AH = null;
                AddressChild AC = null;
                string postal = "";
                string blk = "";
                string unit = "";
                string street = "";
                string bldg = "";
                string company = "";
                string name = "";
                string dept = "";
                string tel = "";
                string mobile = "";
                string acfulltext = "";

                string address0 = "";
                string address1 = "";
                string addv = "";
                int locationType = 0;

                for (int i = 0; i <= list.Length - 1; i++)
                {
                    ads = list[i];
                    if (ads != null && ads.Contacts != null && ads.Contacts.Length > 0)
                        cts = ads.Contacts[0];
                    if (ads != null && ads.Contacts != null && ads.Contacts.Length > 0)
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

                        address0 = !string.IsNullOrEmpty(company) ? company : "";
                        address1 = blk + street + unit + bldg + (!string.IsNullOrEmpty(postal) ? ", " + postal : "");

                        addv = !string.IsNullOrEmpty(address0) && !string.IsNullOrEmpty(address1) ? address0 + "\r\n" + address1 :
                            string.IsNullOrEmpty(address0) && !string.IsNullOrEmpty(address1) ? address1 :
                            !string.IsNullOrEmpty(address0) && string.IsNullOrEmpty(address1) ? address0 : "";

                        locationType = ads.LocationType == XDelServiceRef.Location_Type.Residential ? 1 :
                        ads.LocationType == XDelServiceRef.Location_Type.Office ? 2 : 2;

                        AH = new AddressHeader { 
                            Title = addv, CAIDX = ads.IDX, ADDRESSTYPE = ads.ADDRESSTYPE, COMPANY = company, 
                            BLOCK = blk, STREET = street, UNIT = unit, BUILDING = bldg,
                            POSTALCODE = postal, ACTIVE = ads.ACTIVE,  LocationTypeInt = locationType,
                            CanManageAddressBook = CanManageAddressBook, CanAddAuthShippers = CanAddAuthShippers
                        };

                        foreach (var c in ads.Contacts)
                        {
                            cts = c;
                            name = !string.IsNullOrEmpty(c.NAME) ? c.NAME : "";
                            dept = !string.IsNullOrEmpty(c.DEPARTMENT) ? c.DEPARTMENT : "";
                            tel = !string.IsNullOrEmpty(c.TEL) ? "Tel: " + c.TEL : "";
                            mobile = !string.IsNullOrEmpty(c.MOBILE) ? "Mobile: " + c.MOBILE : "";
                            acfulltext = !string.IsNullOrEmpty(name) ? name +
                                (!string.IsNullOrEmpty(tel) ? "\r\n" + tel : "")
                                 + (!string.IsNullOrEmpty(mobile) ? "\r\n" + mobile : "") : "";

                            nads = Views.AddressBookPage.DeepClone(ads);
                            nads.Contacts = new XDelServiceRef.ContactStructure[] { c };

                            AC = new AddressChild { fulltext = acfulltext, NAME = name, TEL = tel, MOBILE = mobile, 
                                addressStructure = nads, CAIDX = ads.IDX, CNIDX = cts.IDX, DEPARTMENT = dept, ACTIVE = cts.ACTIVE, AddressHeader = AH,
                            CanManageAddressBook = CanManageAddressBook, CanAddAuthShippers = CanAddAuthShippers };
                            AH.Children.Add(AC);
                        }

                        if (AH.Children.Count > 0)
                            Items.Add(AH);
                    }
                }*/
            }
            catch (Exception e)
            {
                string s = e.Message;
            }
        }

        private void LoadSampleData()
        {
            Items = new ObservableCollection<AddressHeader>
            {
                new AddressHeader
                {
                    Title = "Fruits",
                    Children = new ObservableCollection<AddressChild>
                    {
                        new AddressChild { Name = "Apple" },
                        new AddressChild { Name = "Banana" },
                        new AddressChild { Name = "Orange" }
                    }
                },
                new AddressHeader
                {
                    Title = "Vegetables",
                    Children = new ObservableCollection<AddressChild>
                    {
                        new AddressChild { Name = "Carrot" },
                        new AddressChild { Name = "Tomato" },
                        new AddressChild { Name = "Spinach" }
                    }
                }
            };
        }

        public XDelServiceRef.AddressStructure[] getsearchedAddress()
        {
            return searchedAddress;
        }

        public XDelServiceRef.AddressStructure? selectedAddressStructure;

        public void setSelectedAddressStructure(XDelServiceRef.AddressStructure? ads)
        {
            this.selectedAddressStructure = ads;
        }

        public event EventHandler<AddressHeader>? HeaderTappedEvent;

        [RelayCommand]
        private void HeaderTapped(AddressHeader header)
        {
            header.IsExpanded = !header.IsExpanded;

            // Force UI refresh
            var index = Items.IndexOf(header);
            Items.RemoveAt(index);
            Items.Insert(index, header);

            //Console.WriteLine($"Header tapped: {header.Title}");
        }

        public event EventHandler<AddressHeader>? HeaderEditEvent;

        [RelayCommand]
        private void HeaderEdit(AddressHeader header)
        {
            // Trigger the event
            HeaderEditEvent?.Invoke(this, header);
        }

        public event EventHandler<AddressHeader>? HeaderContactAddEvent;

        [RelayCommand]
        private void HeaderContactAdd(AddressHeader header)
        {
            HeaderContactAddEvent?.Invoke(this, header);
        }


        public event EventHandler<AddressChild>? ChildTappedEvent;

        [RelayCommand]
        private void ChildTapped(AddressChild child)
        {
            //Console.WriteLine($"Child tapped: {child.NAME}");

            // Trigger the event
            ChildTappedEvent?.Invoke(this, child);
        }

        public event EventHandler<AddressChild>? ChildEditEvent;

        [RelayCommand]
        private void ChildEdit(AddressChild child)
        {
            // Trigger the event
            ChildEditEvent?.Invoke(this, child);
        }

    }
}