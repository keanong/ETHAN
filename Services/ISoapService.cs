using ETHAN.ViewModel;

namespace ETHAN.Services
{
    public interface ISoapService
    {
        Task<List<ChatMessage>> GetNewMessagesAsync(string webUid, long sessionIdx);

        Task<ChatFetchResult> GetNewMessagesResultAsync(string webUid, long sessionIdx, string mode);
    }
}
