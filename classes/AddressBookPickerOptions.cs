using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ETHAN.classes
{
    public class AddressBookPickerOptions
    {
        private bool _isSelected;
        public string Value {  get; set; }
        public string Name {  get; set; }

        public bool IsSelected
        {
            get => _isSelected;
            set
            {
                if (_isSelected != value)
                {
                    _isSelected = value;
                    OnPropertyChanged(nameof(IsSelected));
                }
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        public AddressBookPickerOptions(string name, string value)
        {
            Name = name;
            Value = value;
        }

        // Optional: Override ToString to return the NAME when the object is cast to string
        public override string ToString()
        {
            return Name;
        }
    }
}
