using CommunityToolkit.Mvvm.ComponentModel;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ETHAN.classes
{
    public partial class AddressHeader : ObservableObject
    {
        /*public string Title { get; set; } = string.Empty;

        public ObservableCollection<AddressChild> Children { get; set; } = new();

        public bool IsExpanded { get; set; }

        public long CAIDX { get; set; } = 0;

        public int ADDRESSTYPE { get; set; }

        public string COMPANY { get; set; }

        public string BLOCK { get; set; }

        public string STREET { get; set; }

        public string UNIT { get; set; }

        public string BUILDING { get; set; }

        public string POSTALCODE { get; set; }

        public bool ACTIVE { get; set; }

        public int LocationTypeInt { get; set; } = 0;

        public bool CanManageAddressBook { get; set; } = false;

        public bool CanAddAuthShippers { get; set; } = false;*/

        [ObservableProperty] public string title= string.Empty;

        [ObservableProperty] public ObservableCollection<AddressChild> children = new();

        [ObservableProperty] public bool isExpanded;

        [ObservableProperty] public long caidx = 0;

        [ObservableProperty] public int addressType;

        [ObservableProperty] public string company;

        [ObservableProperty] public string block;

        [ObservableProperty] public string street;

        [ObservableProperty] public string unit;

        [ObservableProperty] public string building;

        [ObservableProperty] public string postalcode;

        [ObservableProperty] public bool active;

        [ObservableProperty] public int locationTypeInt = 0;

        [ObservableProperty] public bool canManageAddressBook = false;

        [ObservableProperty] public bool canAddAuthShippers = false;

        [ObservableProperty] public XDelServiceRef.AddressStructure? addressStructure_;
    }
}
