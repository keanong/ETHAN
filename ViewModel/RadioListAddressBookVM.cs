using ETHAN.classes;
using System.Collections.ObjectModel;
using System.ComponentModel;

namespace ETHAN.ViewModel
{
    public class RadioListAddressBookVM : INotifyPropertyChanged
    {
        private ObservableCollection<RadioItemAddressBook> _items;
        private RadioItemAddressBook _selectedItem;

        public ObservableCollection<RadioItemAddressBook> Items
        {
            get => _items;
            set
            {
                _items = value;
                OnPropertyChanged(nameof(Items));
            }
        }

        public RadioItemAddressBook SelectedItem
        {
            get => _selectedItem;
            set
            {
                if (_selectedItem != value)
                {
                    _selectedItem = value;
                    OnPropertyChanged(nameof(SelectedItem));
                }
            }
        }

        public RadioListAddressBookVM()
        {
            Items = new ObservableCollection<RadioItemAddressBook>();

            // Sample data
            for (int i = 1; i <= 10; i++)
            {
                Items.Add(new RadioItemAddressBook { Text = $"Option {i}", IsSelected = i == 1 });
            }

            // Set the first item as initially selected
            SelectedItem = Items.FirstOrDefault();
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
