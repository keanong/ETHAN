using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using ETHAN.classes;

namespace ETHAN.ViewModel
{
    public partial class InventoryPageVM : INotifyPropertyChanged
    {
        private ObservableCollection<InventoryItem> _items = new();

        public ObservableCollection<InventoryItem> Items
        {
            get => _items;
            set
            {
                if (_items != value)
                {
                    _items = value;
                    OnPropertyChanged();
                }
            }
        }

        public void newItems()
        {
            Items = new ObservableCollection<InventoryItem>();
        }

        public void addItem(InventoryItem ii)
        {
            Items.Add(ii);
        }

        public event PropertyChangedEventHandler? PropertyChanged;

        protected void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

    }
}
