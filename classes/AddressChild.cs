using CommunityToolkit.Mvvm.ComponentModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ETHAN.classes
{
    public partial class AddressChild : ObservableObject
    {
        /*public string fulltext { get; set; } = string.Empty;

        public long CAIDX { get; set; } = 0;

        public long CNIDX { get; set; } = 0;

        public string NAME { get; set; } = string.Empty;

        public string DEPARTMENT { get; set; } = string.Empty;

        public string TEL { get; set; } = string.Empty;

        public string MOBILE { get; set; } = string.Empty;

        public bool ACTIVE { get; set; }
        
        public XDelServiceRef.AddressStructure? addressStructure { get; set; }

        public AddressHeader? AddressHeader { get; set; }

        public bool CanManageAddressBook { get; set; } = false;

        public bool CanAddAuthShippers { get; set; } = false;
         */

        [ObservableProperty] public string? fulltext = string.Empty;
        [ObservableProperty] public long caidx = 0;
        [ObservableProperty] public long cnidx = 0;
        [ObservableProperty] public string name = string.Empty;
        [ObservableProperty] public string department = string.Empty;
        [ObservableProperty] public string tel = string.Empty;
        [ObservableProperty] public string mobile = string.Empty;
        [ObservableProperty] public bool active;
        [ObservableProperty] public XDelServiceRef.AddressStructure? addressStructure;
        [ObservableProperty] public AddressHeader? addressHeader;
        [ObservableProperty] public bool canManageAddressBook = false;
        [ObservableProperty] public bool canAddAuthShippers = false;
        [ObservableProperty] public XDelServiceRef.ContactStructure? constactStructure_;
    }
}
