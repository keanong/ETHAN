using ETHAN.ViewModel;
using XDelServiceRef;

namespace ETHAN.Services
{
    public class SoapService : ISoapService
    {
        //private XWSSoapClient _client = new XWSSoapClient(XWSSoapClient.EndpointConfiguration.XWSSoap);
        private XOEWSSoapClient _client = new XOEWSSoapClient(XOEWSSoapClient.EndpointConfiguration.XOEWSSoap);

        public async Task<List<ChatMessage>> GetNewMessagesAsync(string webUid, long sessionIdx)
        {
            List<ChatMessage> output = new();
            bool left;

            var result = await _client.VCS_Get_ChatsAsync(webUid, sessionIdx);

            if (result == null || result.Status != 0 || result.Sessions == null)
                return output;

            var session = result.Sessions.LastOrDefault();
            if (session?.Conversation == null)
                return output;

            if (session.Conversation != null && session.Conversation.Length > 0)
            {
                for (int i = session.Conversation.Length - 1; i >= 0; i--)
                {
                    var convo = session.Conversation[i];
                    left = !string.IsNullOrEmpty(convo.StaffName);

                    output.Add(new ChatMessage
                    {
                        StaffName = convo.StaffName ?? "",
                        Text = convo.Message ?? "",
                        Time = convo.TimeStamp != DateTime.MinValue
                            ? convo.TimeStamp.ToString("hh:mm:tt")
                            : "-",
                        BubblePosition = left ? LayoutOptions.Start : LayoutOptions.End,
                        BubbleColor = left ? (Color)Application.Current.Resources["Gray600"]
                                       : (Color)Application.Current.Resources["XDelOrange"],
                        MessageTextColor = Colors.White,
                        ShowName = left,
                        ShowTime = true,
                        MinWidthRequest = 150
                    });
                }
            }

            /*foreach (var convo in session.Conversation)
            {
                bool left = !string.IsNullOrEmpty(convo.StaffName);

                output.Add(new ChatMessage
                {
                    StaffName = convo.StaffName ?? "",
                    Text = convo.Message ?? "",
                    Time = convo.TimeStamp != DateTime.MinValue
                            ? convo.TimeStamp.ToString("hh:mm:tt")
                            : "-",
                    BubblePosition = left ? LayoutOptions.Start : LayoutOptions.End,
                    BubbleColor = left ? (Color)Application.Current.Resources["Gray400"]
                                       : (Color)Application.Current.Resources["XDelOrange"],
                    MessageTextColor = Colors.White,
                    ShowName = left
                });
            }*/

            return output;
        }

        public async Task<ChatFetchResult> GetNewMessagesResultAsync(string webUid, long sessionIdx, string mode)
        {
            ChatFetchResult resultModel = new();
            bool left;

            var result = (mode.Equals("r")) ? await _client.XOE_VCS_Get_ChatsAsync(webUid, sessionIdx) : await _client.VCS_Get_ChatsAsync(webUid, sessionIdx);

            if (result == null || result.Status != 0 || result.Sessions == null)
                return resultModel;

            var session = result.Sessions.LastOrDefault();
            if (session == null)
                return resultModel;

            // Return Chat Status
            resultModel.ChatStatus = session.Chat_Status == Status.csPrivate ||
                                     session.Chat_Status == Status.csOpen
                                     ? 1
                                     : 0;

            if (session.Conversation != null)
            {
                foreach (var convo in session.Conversation.Reverse())
                {
                    left = !string.IsNullOrEmpty(convo.StaffName);

                    resultModel.Messages.Add(new ChatMessage
                    {
                        StaffName = convo.StaffName ?? "",
                        Text = convo.Message ?? "",
                        Time = convo.TimeStamp != DateTime.MinValue
                            ? convo.TimeStamp.ToString("hh:mm:tt")
                            : "-",
                        BubblePosition = left ? LayoutOptions.Start : LayoutOptions.End,
                        BubbleColor = left ? (Color)Application.Current.Resources["Gray600"]
                                           : (Color)Application.Current.Resources["XDelOrange"],
                        MessageTextColor = Colors.White,
                        ShowName = left,
                        ShowTime = true,
                        MinWidthRequest = 150
                    });
                }
            }

            if (resultModel.ChatStatus == 0)
                resultModel.Messages.Add(new ChatMessage
                {
                    StaffName = "XDel",
                    Text = "Session Closed",
                    Time = "-",
                    BubblePosition = LayoutOptions.Start,
                    BubbleColor = (Color)Application.Current.Resources["Gray600"],
                    MessageTextColor = Colors.White,
                    ShowName = true,
                    ShowTime = false,
                    MinWidthRequest = 150
                });

            return resultModel;
        }
    }

    public class ChatFetchResult
    {
        public int ChatStatus { get; set; }
        public List<ChatMessage> Messages { get; set; } = new();
    }
}
