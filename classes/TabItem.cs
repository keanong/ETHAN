using CommunityToolkit.Mvvm.ComponentModel;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ETHAN.classes
{
    public class TabItem : ObservableObject
    {
        public string Title { get; set; }
        public ObservableCollection<ManageJobSelector> Items { get; set; }

        public bool IsScrollEnabled { get; set; } = true;
    }
}
