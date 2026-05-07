
using XDelServiceRef;
using System.ComponentModel;
using System.Collections.ObjectModel;

namespace ETHAN.classes
{
    public class SelectableItemAddressBook
    {
        private bool _isSelected;

        public int sn { get; set; }

        public string? Text { get; set; }
        public XDelServiceRef.AddressStructure? addressStructure { get; set; }

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

        public ObservableCollection<SelectableItemAddressBook>? SelectableItemAddressBookList { get; set; }

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

    }
}
