using CommunityToolkit.Mvvm.ComponentModel;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using XDelServiceRef;

namespace ETHAN.classes
{
    public partial class ChatHeader : ObservableObject
    {

        [ObservableProperty] public string title = string.Empty;

        [ObservableProperty] public ObservableCollection<ChatChild> children = new();

        [ObservableProperty] public bool isExpanded;

    }
}
