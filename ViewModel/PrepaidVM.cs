using CommunityToolkit.Mvvm.ComponentModel;
using ETHAN.classes;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Collections.ObjectModel;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace ETHAN.ViewModel
{
    public class PrepaidVM : INotifyPropertyChanged
    {
        public PrepaidVM() { }

        private ObservableCollection<PrepaidItem> _items = new();

        public ObservableCollection<PrepaidItem> Items
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
            Items = new ObservableCollection<PrepaidItem>();
        }

        public void addItem(PrepaidItem pi)
        {
            Items.Add(pi);
        }

        public ObservableCollection<ManageJobOptionSelector> Options { get; set; }

        public void newOptions()
        {
            Options = new ObservableCollection<ManageJobOptionSelector>();
        }

        public void addOptions(ManageJobOptionSelector mjo)
        {
            Options.Add(mjo);
        }

        public event PropertyChangedEventHandler? PropertyChanged;

        protected void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

    }
}
