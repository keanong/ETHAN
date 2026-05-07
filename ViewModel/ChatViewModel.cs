using CommunityToolkit.Mvvm.ComponentModel;
using ETHAN.Services;
using System.Collections.ObjectModel;

namespace ETHAN.ViewModel
{
    public partial class ChatViewModel : ObservableObject
    {
        private readonly ISoapService _soapService;
        private CancellationTokenSource? _cts;

        [ObservableProperty]
        ObservableCollection<ChatMessage> messages = new();

        public string WebUid { get; set; } = "";
        public long SessionIdx { get; set; } = 0;
        public int ChatStatus { get; set; } = 0; // 1 = active

        [ObservableProperty]
        bool isChatOpen;

        public ChatViewModel(ISoapService soapService)
        {
            _soapService = soapService;
        }

        public void StartPolling(string mode)
        {
            try
            {
                if (ChatStatus != 1)
                    return;  // inactive chat

                _cts = new CancellationTokenSource();
                _ = PollLoop(_cts.Token, mode);
            }
            catch (Exception e)
            {
                string s = e.Message;
            }
        }

        public void StopPolling()
        {
            try
            {
                _cts?.Cancel();
            }
            catch (Exception e)
            {
                string s = e.Message;
            }
        }

        private async Task PollLoop(CancellationToken token, string mode)
        {
            try
            {
                while (!token.IsCancellationRequested)
                {
                    try
                    {
                        if (ChatStatus == 1)
                            await FetchNewMessages(mode);
                    }
                    catch (Exception ex)
                    {
                        string s = ex.Message;
                    }

                    await Task.Delay(TimeSpan.FromSeconds(30), token);
                }
            }
            catch (TaskCanceledException)
            {
                // Normal — do nothing
            }
            catch (Exception e)
            {
                string s = e.Message;
            }
        }

        private async Task FetchNewMessages(string mode)
        {
            try
            {
                /*var result = await _soapService.GetNewMessagesAsync(WebUid, SessionIdx);
                if (result == null || result.Count == 0)
                    return;

                MainThread.BeginInvokeOnMainThread(() =>
                {
                    ReplaceMessages(result);
                });*/

                var result = await _soapService.GetNewMessagesResultAsync(WebUid, SessionIdx, mode);

                if (result?.Messages?.Any() == true)
                {
                    MainThread.BeginInvokeOnMainThread(() =>
                    {
                        AppendNewMessages(result.Messages);
                        // Notify view that new messages arrived
                        NewMessagesFetched?.Invoke(this, EventArgs.Empty);
                    });
                }

                IsChatOpen = result.ChatStatus == 1;
            }
            catch (TaskCanceledException)
            {
                // Expected when polling stops
            }
            catch (Exception e)
            {
                string s = e.Message;
            }
        }

        public event EventHandler? NewMessagesFetched;

        public void AppendNewMessages(List<ChatMessage> incoming)
        {
            /*if (incoming == null || incoming.Count == 0)
                return;

            // No previous messages — load all
            if (Messages.Count == 0)
            {
                foreach (var msg in incoming)
                    Messages.Add(msg);

                NewMessagesFetched?.Invoke(this, EventArgs.Empty);
                return;
            }

            var lastExisting = Messages.Last();

            // Append only NEW messages
            foreach (var msg in incoming)
            {
                if (!IsSameMessage(msg, lastExisting))
                {
                    Messages.Add(msg);
                    lastExisting = msg; // update pointer
                }
            }

            NewMessagesFetched?.Invoke(this, EventArgs.Empty);*/

            if (incoming == null || incoming.Count == 0)
                return;

            // Build a set of keys for the existing messages
            var existingKeys = new HashSet<string>(Messages.Select(m => MessageKey(m)));

            bool anyAdded = false;

            // Preserve order from incoming; add only messages whose key not present
            foreach (var msg in incoming)
            {
                var key = MessageKey(msg);
                if (!existingKeys.Contains(key))
                {
                    Messages.Add(msg);
                    existingKeys.Add(key); // avoid duplicates if incoming has duplicates
                    anyAdded = true;
                }
            }

            if (anyAdded)
                NewMessagesFetched?.Invoke(this, EventArgs.Empty);
        }

        private string MessageKey(ChatMessage m)
        {
            // Create a stable key for deduplication. Prefer a unique ID from server if available.
            string staff = m.StaffName?.Trim() ?? "";
            string time = m.Time?.Trim() ?? "";
            string text = (m.Text ?? "").Trim();

            // consider normalizing whitespace or truncating text if huge
            return $"{staff}|{time}|{text}";
        }

        private bool IsSameMessage(ChatMessage a, ChatMessage b)
        {
            return a.Text == b.Text
                && a.Time == b.Time
                && a.StaffName == b.StaffName;
        }

        public void ReplaceMessages(IEnumerable<ChatMessage> newMessages)
        {
            Messages = new ObservableCollection<ChatMessage>(newMessages);
        }

        public void SendMessage(string text)
        {
            AddMessage(text + "\r\n" + text + "\r\n" + text, "", "12:30");
            AddMessage("Received: " + text + "\r\n" + text + "\r\n" + text, "XDel", "12:31");
        }

        public void AddMessage(string text, string staffname, string time)
        {
            bool left = !string.IsNullOrEmpty(staffname);
            try
            {

                messages.Add(new ChatMessage
                {
                    StaffName = staffname,
                    Text = text,
                    Time = time,
                    BubblePosition = left ? LayoutOptions.Start : LayoutOptions.End,
                    BubbleColor = left ? (Color)Application.Current.Resources["Gray600"] : (Color)Application.Current.Resources["XDelOrange"],
                    MessageTextColor = Colors.White,
                    ShowName = left,
                    ShowTime = true,
                    MinWidthRequest = 150
                });
            }
            catch (Exception e)
            {
                string s = e.Message;
            }
        }

    }


    public class ChatMessage
    {
        public string StaffName { get; set; } = "";

        public string Text { get; set; } = "";

        public string Time { get; set; } = "";

        public LayoutOptions BubblePosition { get; set; } // Left or Right bubble

        public Color BubbleColor { get; set; }      // <-- NEW

        public Color MessageTextColor { get; set; } // <-- NEW

        public bool ShowName { get; set; } = false;

        public int MinWidthRequest { get; set; } = 100;

        public bool ShowTime { get; set; } = true;

        public int Fontsize { get; set; } = 16;

        public LayoutOptions TextPosition { get; set; } = LayoutOptions.Start;

    }
}
