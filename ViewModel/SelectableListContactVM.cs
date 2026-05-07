using ETHAN.classes;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ETHAN.ViewModel
{
    public class SelectableListContactVM : INotifyPropertyChanged
    {
        private ObservableCollection<SelectableItemAddressBookContact> _items;
        private SelectableItemAddressBookContact _selectedItem;
        private XDelServiceRef.ContactStructure[]? searchedContacts;

        public ObservableCollection<SelectableItemAddressBookContact> Items
        {
            get => _items;
            set
            {
                _items = value;
                OnPropertyChanged(nameof(Items));
            }
        }

        public SelectableItemAddressBookContact SelectedItem
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

        public SelectableListContactVM()
        {
            Items = new ObservableCollection<SelectableItemAddressBookContact>();

            // Sample data
            //for (int i = 1; i <= 10; i++)
            //{
            //    Items.Add(new SelectableItemAddressBook { Text = $"Option {i}", IsSelected = false });
            //}
        }

        public void setItemsNew(XDelServiceRef.ContactStructure[]? ctss)
        {
            try
            {

            } catch (Exception e)
            {
                string s = e.Message;
            }
        }


        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

    }
}
