using ETHAN.classes;
using System.Collections.ObjectModel;
using System.ComponentModel;

namespace ETHAN.ViewModel
{
    public class AddressBookPickerOptionsVM : INotifyPropertyChanged
    {
        public ObservableCollection<AddressBookPickerOptions> _Options;
        private AddressBookPickerOptions _selectedOption;

        public ObservableCollection<AddressBookPickerOptions> Options
        {
            get => _Options;
            set
            {
                _Options = value;
                OnPropertyChanged(nameof(Options));
            }
        }

        public AddressBookPickerOptions SelectedOption
        {
            get => _selectedOption;
            set
            {
                if (_selectedOption != value)
                {
                    // Unselect previous item
                    if (_selectedOption != null)
                        _selectedOption.IsSelected = false;

                    _selectedOption = value;

                    // Select new item
                    if (_selectedOption != null)
                        _selectedOption.IsSelected = true;

                    OnPropertyChanged(nameof(SelectedOption));
                }
            }
        }

        public AddressBookPickerOptionsVM() {
            Options = new ObservableCollection<AddressBookPickerOptions>();
            AddressBookPickerOptions? ao = null;
            try {
                //ao = new AddressBookPickerOptions("- SELECT -", "- SELECT -");
                //Options.Add(ao);
                ao = new AddressBookPickerOptions("A", "A");
                Options.Add(ao);
                ao = new AddressBookPickerOptions("B", "B");
                Options.Add(ao);
                ao = new AddressBookPickerOptions("C", "C");
                Options.Add(ao);
                ao = new AddressBookPickerOptions("D", "D");
                Options.Add(ao);
                ao = new AddressBookPickerOptions("E", "E");
                Options.Add(ao);
                ao = new AddressBookPickerOptions("F", "F");
                Options.Add(ao);
                ao = new AddressBookPickerOptions("G", "G");
                Options.Add(ao);
                ao = new AddressBookPickerOptions("H", "H");
                Options.Add(ao);
                ao = new AddressBookPickerOptions("I", "I");
                Options.Add(ao);
                ao = new AddressBookPickerOptions("J", "J");
                Options.Add(ao);
                ao = new AddressBookPickerOptions("K", "K");
                Options.Add(ao);
                ao = new AddressBookPickerOptions("L", "L");
                Options.Add(ao);
                ao = new AddressBookPickerOptions("M", "M");
                Options.Add(ao);
                ao = new AddressBookPickerOptions("N", "N");
                Options.Add(ao);
                ao = new AddressBookPickerOptions("O", "O");
                Options.Add(ao);
                ao = new AddressBookPickerOptions("P", "P");
                Options.Add(ao);
                ao = new AddressBookPickerOptions("Q", "Q");
                Options.Add(ao);
                ao = new AddressBookPickerOptions("R", "R");
                Options.Add(ao);
                ao = new AddressBookPickerOptions("S", "S");
                Options.Add(ao);
                ao = new AddressBookPickerOptions("T", "T");
                Options.Add(ao);
                ao = new AddressBookPickerOptions("U", "U");
                Options.Add(ao);
                ao = new AddressBookPickerOptions("V", "V");
                Options.Add(ao);
                ao = new AddressBookPickerOptions("W", "W");
                Options.Add(ao);
                ao = new AddressBookPickerOptions("X", "X");
                Options.Add(ao);
                ao = new AddressBookPickerOptions("Y", "Y");
                Options.Add(ao);
                ao = new AddressBookPickerOptions("Z", "Z");
                Options.Add(ao);
                ao = new AddressBookPickerOptions("0-9", "0");
                Options.Add(ao);
                ao = new AddressBookPickerOptions("Name", "");
                Options.Add(ao);
            } catch (Exception ex)
            {
                string s = ex.Message;
            }
        }

        public void setOptions(AddressBookPickerOptions[] op)
        {
            try
            {
                if (op != null && op.Length > 0)
                {
                    for (int i = 0; i <= op.Length; i++)
                    {
                        Options.Add(op[i]);
                    }
                }
            }
            catch (Exception ex)
            {
                string s = ex.Message;
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
