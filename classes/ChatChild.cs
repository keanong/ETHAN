using CommunityToolkit.Mvvm.ComponentModel;
using System.Collections.ObjectModel;
using XDelServiceRef;

namespace ETHAN.classes
{
    public partial class ChatChild : ObservableObject
    {
        [ObservableProperty] public string? fulltext = string.Empty;
        [ObservableProperty] public string? reference = null;
        [ObservableProperty] public ChatSession? chatsession = null;
        [ObservableProperty] public ObservableCollection<long>? sessionIDXs = new();
        [ObservableProperty] public bool statusGreenVisible;
        [ObservableProperty] public bool statusRedVisible;
        [ObservableProperty] public bool lockVisible;
        public int Status = 0;
        public bool isNewChat;
        
    }
}
