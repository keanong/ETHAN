using CommunityToolkit.Maui.Extensions;
using CommunityToolkit.Maui.Views;
using Microsoft.Maui.Controls;
using System.Threading;
using System.Threading.Tasks;

namespace ETHAN.ProgressDialog
{
    public class ProgressDialogServiceNew : IProgressDialogServiceNew
    {
        private readonly SemaphoreSlim _lock = new(1, 1);
        private Progress_Popup? _popup;    // Single instance
        private bool _isShowing = false;

        public async Task ShowAsync(string message)
        {
            await _lock.WaitAsync();
            try
            {
                if (_isShowing) return;

                var page = Application.Current?.MainPage;
                if (page == null) return;

                _popup = new Progress_Popup(message);
                _isShowing = true;

                await MainThread.InvokeOnMainThreadAsync(async () =>
                {
                    //await page.ShowPopupAsync(_popup);
                    _ = page.ShowPopupAsync(_popup);
                });
            }
             catch (Exception e)
            {
                string s = e.Message;
            }
            finally
            {
                _lock.Release();
            }
        }

        public async Task UpdateMessageAsync(string message)
        {
            try
            {
                await MainThread.InvokeOnMainThreadAsync(() =>
                {
                    _popup?.UpdateMessage(message);
                });
            } catch (Exception e)
            {
                string s = e.Message;
            }
        }

        public async Task DismissAsync()
        {
            await _lock.WaitAsync();
            try
            {
                if (!_isShowing) return;

                if (_popup != null)
                {
                    await MainThread.InvokeOnMainThreadAsync(async () =>
                    {
                        await _popup.CloseAsync();
                        _popup = null;
                    });
                }

                _isShowing = false;
            } catch (Exception e) {
                string s = e.Message;
            }
            finally
            {
                _lock.Release();
            }
        }

    }
}
