using ETHAN.classes;
using System.Collections;
using System.Collections.ObjectModel;
using System.ComponentModel;

namespace ETHAN.ViewModel
{
    public class SelectableListAddressBookVM : INotifyPropertyChanged
    {

        private bool _isExpanded;
        public bool IsExpanded
        {
            get => _isExpanded;
            set
            {
                _isExpanded = value;
                OnPropertyChanged(nameof(IsExpanded));
            }
        }


        private ObservableCollection<SelectableItemAddressBook> _Items;
        private SelectableItemAddressBook _selectedItem;
        private XDelServiceRef.AddressStructure[]? searchedAddress;
        public string? cri1 {  get; set; }
        public string? cri2 { get; set; }

        public ObservableCollection<SelectableItemAddressBook> Items
        {
            get => _Items;
            set
            {
                _Items = value;
                OnPropertyChanged(nameof(Items));
            }
        }

        public ObservableCollection<SelectableItemAddressBook> iii;

        public SelectableItemAddressBook SelectedItem
        {
            get => _selectedItem;
            set
            {
                if (_selectedItem != value)
                {
                    // Unselect previous item
                    if (_selectedItem != null)
                        _selectedItem.IsSelected = false;

                    _selectedItem = value;

                    // Select new item
                    if (_selectedItem != null)
                        _selectedItem.IsSelected = true;

                    OnPropertyChanged(nameof(SelectedItem));
                }
            }
        }

        public SelectableListAddressBookVM()
        {
            Items = new ObservableCollection<SelectableItemAddressBook>();

            // Sample data
            //for (int i = 1; i <= 10; i++)
            //{
            //    Items.Add(new SelectableItemAddressBook { Text = $"Option {i}", IsSelected = false });
            //}
        }

        public SelectableListAddressBookVM(XDelServiceRef.AddressStructure[]? list)
        {
            if (Items == null)
                Items = new ObservableCollection<SelectableItemAddressBook>();

            XDelServiceRef.AddressStructure? ad = null;
            XDelServiceRef.ContactStructure? cts = null;
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
            string addv = "";

            try
            {
                if (list != null && list.Length > 0)
                {
                    for (int i = 0; i <= list.Length - 1; i++)
                    {
                        ad = list[i];
                        if (ad != null && ad.Contacts != null && ad.Contacts.Length > 0)
                            cts = ad.Contacts[0];
                        if (ad != null && cts != null)
                        {
                            postal = !string.IsNullOrEmpty(ad.POSTALCODE) ? ad.POSTALCODE : "";
                            blk = !string.IsNullOrEmpty(ad.BLOCK) ? ad.BLOCK + " " : "";
                            unit = !string.IsNullOrEmpty(ad.UNIT) ? ad.UNIT + " " : "";
                            street = !string.IsNullOrEmpty(ad.STREET) ? ad.STREET + " " : "";
                            bldg = !string.IsNullOrEmpty(ad.BUILDING) ? ad.BUILDING : "";
                            company = !string.IsNullOrEmpty(ad.COMPANY) ? ad.COMPANY : "";

                            name = !string.IsNullOrEmpty(cts.NAME) ? cts.NAME : "";
                            tel = !string.IsNullOrEmpty(cts.TEL) ? "Tel: " + cts.TEL : "";
                            mobile = !string.IsNullOrEmpty(cts.MOBILE) ? "Mobile: " + cts.MOBILE : "";

                            //address0 = !string.IsNullOrEmpty(company) ? company + "\r\n" : "";
                            //address1 = blk + street + unit + bldg + (!string.IsNullOrEmpty(postal) ? ", " + postal : "") + "\r\n";

                            //addv = address0 + address1;

                            address0 = !string.IsNullOrEmpty(company) ? company + "\r\n" : "";
                            address1 = blk + street + unit + bldg + (!string.IsNullOrEmpty(postal) ? ", " + postal : "") + "\r\n";
                            address2 = !string.IsNullOrEmpty(name) ? name + ((!string.IsNullOrEmpty(tel) || !string.IsNullOrEmpty(mobile)) ? "\r\n" : "") : "";
                            address3 = !string.IsNullOrEmpty(tel) ? tel : "";
                            address3 += !string.IsNullOrEmpty(address3) && !string.IsNullOrEmpty(mobile) ? ", " + mobile : string.IsNullOrEmpty(address3) && !string.IsNullOrEmpty(mobile) ? mobile : "";

                            addv = address0 + address1 + address2 + address3;

                            Items.Add(new SelectableItemAddressBook { Text = $"{addv}", addressStructure = ad, IsSelected = false, sn = i + 1 });
                        }
                        //if (ad != null)
                        //{
                        //    postal = !string.IsNullOrEmpty(ad.POSTALCODE) ? ad.POSTALCODE : "";
                        //    blk = !string.IsNullOrEmpty(ad.BLOCK) ? ad.BLOCK + " " : "";
                        //    unit = !string.IsNullOrEmpty(ad.UNIT) ? ad.UNIT + " " : "";
                        //    street = !string.IsNullOrEmpty(ad.STREET) ? ad.STREET + " " : "";
                        //    bldg = !string.IsNullOrEmpty(ad.BUILDING) ? ad.BUILDING : "";
                        //    company = !string.IsNullOrEmpty(ad.COMPANY) ? ad.COMPANY : "";

                        //    address0 = !string.IsNullOrEmpty(company) ? company + "\r\n" : "";
                        //    address1 = blk + street + unit + bldg + (!string.IsNullOrEmpty(postal) ? ", " + postal : "") + "\r\n";

                        //    addv = address0 + address1;

                        //    Items.Add(new SelectableItemAddressBook { Text = $"{addv}", addressStructure = ad, IsSelected = false, sn = i + 1 });
                        //}
                    }
                }

            }
            catch (Exception e)
            {
                string s = e.Message;
            }

        }

        public void setItemsNew(XDelServiceRef.AddressStructure[]? ads)
        {
            try
            {
                XDelServiceRef.AddressStructure? ad = null;
                XDelServiceRef.ContactStructure? cts = null;
                Items = new ObservableCollection<SelectableItemAddressBook>();
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
                string addv = "";

                for (int i = 0; i <= ads.Length - 1; i++)
                {
                    ad = ads[i];
                    if (ad != null && ad.Contacts != null && ad.Contacts.Length > 0)
                        cts = ad.Contacts[0];

                    if (ad != null && cts != null)
                    {
                        postal = !string.IsNullOrEmpty(ad.POSTALCODE) ? ad.POSTALCODE : "";
                        blk = !string.IsNullOrEmpty(ad.BLOCK) ? ad.BLOCK + " " : "";
                        unit = !string.IsNullOrEmpty(ad.UNIT) ? ad.UNIT + " " : "";
                        street = !string.IsNullOrEmpty(ad.STREET) ? ad.STREET + " " : "";
                        bldg = !string.IsNullOrEmpty(ad.BUILDING) ? ad.BUILDING : "";
                        company = !string.IsNullOrEmpty(ad.COMPANY) ? ad.COMPANY : "";

                        name = !string.IsNullOrEmpty(cts.NAME) ? cts.NAME : "";
                        tel = !string.IsNullOrEmpty(cts.TEL) ? "Tel: " + cts.TEL : "";
                        mobile = !string.IsNullOrEmpty(cts.MOBILE) ? "Mobile: " + cts.MOBILE : "";

                        address0 = !string.IsNullOrEmpty(company) ? company + "\r\n" : "";
                        address1 = blk + street + unit + bldg + (!string.IsNullOrEmpty(postal) ? ", " + postal : "") + "\r\n";                        

                        addv = address0 + address1;

                        Items.Add(new SelectableItemAddressBook { Text = $"{addv}", addressStructure = ad, IsSelected = false, sn = i + 1 });
                    }
                }
                //setsearchedAddress(ads);
                iii = Items;
                if (Items != null && Items.Count > 0)
                {
                    for (int i = 0; i <= Items.Count - 1; i++)
                    {
                        Items[i].SelectableItemAddressBookList = iii;
                    }
                }
            }
            catch (Exception e)
            {
                string s = e.Message;
            }
        }

        public void setItems(XDelServiceRef.AddressStructure[]? ads)
        {
            try
            {
                XDelServiceRef.AddressStructure? ad = null;
                XDelServiceRef.ContactStructure? cts = null;
                Items = new ObservableCollection<SelectableItemAddressBook>();
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
                string addv = "";

                for (int i = 0; i <= ads.Length -1; i++)
                {
                    ad = ads[i];
                    if (ad != null && ad.Contacts != null && ad.Contacts.Length > 0) 
                        cts = ad.Contacts[0];

                    if (ad != null && cts != null)
                    {
                        postal = !string.IsNullOrEmpty(ad.POSTALCODE) ? ad.POSTALCODE : "";
                        blk = !string.IsNullOrEmpty(ad.BLOCK) ? ad.BLOCK + " " : "";
                        unit = !string.IsNullOrEmpty(ad.UNIT) ? ad.UNIT + " " : "";
                        street = !string.IsNullOrEmpty(ad.STREET) ? ad.STREET + " " : "";
                        bldg = !string.IsNullOrEmpty(ad.BUILDING) ? ad.BUILDING : "";
                        company = !string.IsNullOrEmpty(ad.COMPANY) ? ad.COMPANY : "";

                        name = !string.IsNullOrEmpty(cts.NAME) ? cts.NAME : "";
                        tel = !string.IsNullOrEmpty(cts.TEL) ? "Tel: " + cts.TEL : "";
                        mobile = !string.IsNullOrEmpty(cts.MOBILE) ? "Mobile: " + cts.MOBILE : "";

                        address0 = !string.IsNullOrEmpty(company) ? company + "\r\n" : "";
                        address1 = blk + street + unit + bldg + (!string.IsNullOrEmpty(postal) ? ", " + postal : "") + "\r\n";
                        address2 = !string.IsNullOrEmpty(name) ? name + ((!string.IsNullOrEmpty(tel) || !string.IsNullOrEmpty(mobile)) ? "\r\n" : "") : "";
                        address3 = !string.IsNullOrEmpty(tel) ? tel : "";
                        address3 += !string.IsNullOrEmpty(address3) && !string.IsNullOrEmpty(mobile) ? ", " + mobile : string.IsNullOrEmpty(address3) && !string.IsNullOrEmpty(mobile) ? mobile : "";

                        addv = address0 + address1 + address2 + address3;

                        Items.Add(new SelectableItemAddressBook { Text = $"{addv}", addressStructure = ad, IsSelected = false, sn = i+1 });
                    }
                }
                //setsearchedAddress(ads);
                iii = Items;
                if (Items != null && Items.Count > 0)
                {
                    for (int i = 0; i <= Items.Count -1; i++)
                    {
                        Items[i].SelectableItemAddressBookList = iii;
                    }
                }
            } catch (Exception e)
            {
                string s = e.Message;
            }
        }

        public void setsearchedAddress(XDelServiceRef.AddressStructure[]? ads)
        {
            searchedAddress = ads;
        }

        public XDelServiceRef.AddressStructure[] getsearchedAddress()
        {
            return searchedAddress;
        }


        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
