using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using ETHAN.classes;
using System.Collections.ObjectModel;
using System.Linq;
using XDelServiceRef;


namespace ETHAN.ViewModel
{
    public partial class SelectableExpListChatsVM : ObservableObject
    {
        private XDelServiceRef.ChatSession[]? searchedChatSessions;
        private XDelServiceRef.ChatSession[]? clonedChatSessions;

        [ObservableProperty]
        private ObservableCollection<ChatHeader> items = new();

        public XDelServiceRef.ChatSession[] getsearchedChatSessions()
        {
            return searchedChatSessions;
        }

        public SelectableExpListChatsVM()
        {
            Items = new ObservableCollection<ChatHeader>();
        }

        public SelectableExpListChatsVM (XDelServiceRef.ChatSession[]? list)
        {
            try
            {
                if (Items == null)
                    Items = new ObservableCollection<ChatHeader>();

                searchedChatSessions = list;

                if (searchedChatSessions != null && searchedChatSessions.Length > 0)
                    LoadInitialPage();
                if (Items != null && Items.Count == 0)
                {
                    ChatHeader ch = new ChatHeader { Title = "No Chat history found." };
                    Items.Add(ch);
                }
            }
            catch (Exception e)
            {
                string s = e.Message;
            }
        }

        public void UpdateItems(XDelServiceRef.ChatSession[]? list)
        {
            Items.Clear(); // clears existing, lets GC collect old ChatHeader/ChatChild objects
            searchedChatSessions = list;
            if (searchedChatSessions != null && searchedChatSessions.Length > 0)
                LoadInitialPage();
            if (Items.Count == 0)
            {
                ChatHeader ch = new ChatHeader { Title = "No Chat history found." };
                Items.Add(ch);
            }
        }

        private void LoadInitialPage()
        {
            try
            {
                for (int i = 0; i <= searchedChatSessions!.Length -1; i++)
                {
                    var cs = searchedChatSessions[i];
                    if (cs == null)
                        continue;
                    var header = BuildChatHeaderNew(cs);
                    if (header != null)
                        Items.Add(header);
                }
            } catch (Exception e)
            {
                string s = e.Message;
            }
        }

        private ChatHeader BuildChatHeaderNew(ChatSession cs)
        {
            ChatHeader? ch = null;
            ChatHeader? ach = null;
            ChatHeader? aach = null;
            ChatChild? cc = null;
            ChatChild? acc = null;
            bool existingHeader = false;
            string ccTitle = "";
            string ccref = "";
            bool greenVis = false;
            bool redVis = false;
            bool locked = false;
            ObservableCollection<long>? sIDXs = null;
            ObservableCollection<long>? sIDXs_final = new();
            int status = 0;
            try
            {
                if (Items != null && Items.Count > 0)
                {
                    for (int i = Items.Count - 1; i >= 0; i--)
                    {
                        ach = Items[i];
                        if (ach == null)
                            continue;
                        if (!string.IsNullOrEmpty(ach.Title) && ach.Title.Equals(cs.TimeStamp.ToString("dd/MM/yyyy")))
                        {
                            ch = ach;
                            existingHeader = true;
                            break;
                        }
                    }
                }

                locked = cs.Chat_Status == Status.csPrivate;
                greenVis = locked || (cs.Chat_Status == Status.csOpen);
                redVis = !greenVis && (cs.Chat_Status == Status.csClosed) || (cs.Chat_Status == Status.csClosed);

                if (ch == null)
                    ch = new ChatHeader { Title = cs.TimeStamp.ToString("dd/MM/yyyy") };

                ccref = !string.IsNullOrEmpty(cs.Reference) ? cs.Reference : "";
                if (!string.IsNullOrEmpty(ccref) && ccref.Substring(ccref.Length - 1).Equals("/"))
                    ccref = ccref.Substring(0, ccref.Length - 1);

                if (!string.IsNullOrEmpty(ccref) && Items != null && Items.Count > 0)
                {
                    for (int i = 0; i <= Items.Count -1; i++)
                    {
                        aach = Items[i];
                        if (aach == null)
                            continue;
                        if (aach.Children != null && aach.Children.Count > 0)
                        {
                            for (int c = 0; c <= aach.Children.Count - 1; c++)
                            {
                                acc = aach.Children[c];
                                if (acc != null && !string.IsNullOrEmpty(acc.Reference) && string.Equals(acc.Reference, ccref) && !(Items[i].Children[c].SessionIDXs!.Contains(cs.SessionIDX)))
                                    Items[i]!.Children[c]!.SessionIDXs!.Add(cs.SessionIDX);
                            }
                        }
                    }
                }

                ccTitle = !string.IsNullOrEmpty(ccref) && cs.TimeStamp != DateTime.MinValue ? cs.TimeStamp.ToString("hh:mm:tt") + "\r\n" + ccref :
                    cs.TimeStamp != DateTime.MinValue ? cs.TimeStamp.ToString("hh:mm:tt") : "";

                if (string.IsNullOrEmpty(ccTitle))
                    return null;

                if (ch != null && ch.Children != null && ch.Children.Count > 0)
                {
                    for (int i = 0; i <= ch.Children.Count - 1; i++)
                    {
                        if (ch.Children[i] != null && ch.Children[i].Chatsession != null && !string.IsNullOrEmpty(ch.Children[i].Chatsession!.Reference) && ch.Children[i].Chatsession!.Reference.Equals(ccref))
                        {
                            if (!ch.Children[i].SessionIDXs!.Contains(cs.SessionIDX))
                                ch.Children[i].SessionIDXs!.Add(cs.SessionIDX);
                            sIDXs = ch.Children[i].SessionIDXs;
                        }
                    }
                }

                if (sIDXs != null)
                    sIDXs_final = sIDXs;
                else
                    sIDXs_final.Add(cs.SessionIDX);

                status = greenVis ? 1 : 0;

                cc = new ChatChild { Fulltext = ccTitle, Reference = ccref, Chatsession = cs, SessionIDXs = sIDXs_final, Status = status, StatusGreenVisible = greenVis, StatusRedVisible = redVis, LockVisible = locked };

                ch!.Children!.Add(cc);

                if (existingHeader)
                    return null;

            }
            catch (Exception e)
            {
                string s = e.Message;
            }
            return ch;
        }

        public event EventHandler<ChatHeader>? HeaderTappedEvent;

        [RelayCommand]
        private void HeaderTapped(ChatHeader header)
        {
            header.IsExpanded = !header.IsExpanded;

            // Force UI refresh
            var index = Items.IndexOf(header);
            Items.RemoveAt(index);
            Items.Insert(index, header);

            Console.WriteLine($"Header tapped: {header.Title}");
        }

        public event EventHandler<ChatChild>? ChildTappedEvent;

        [RelayCommand]
        private void ChildTapped(ChatChild child)
        {
            //Console.WriteLine($"Child tapped: {child.NAME}");

            // Trigger the event
            ChildTappedEvent?.Invoke(this, child);
        }

    }
}
