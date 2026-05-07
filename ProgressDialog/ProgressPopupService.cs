using CommunityToolkit.Maui.Extensions;
using CommunityToolkit.Maui.Views;

namespace ETHAN.ProgressDialog
{
    public static class ProgressPopupService
    {
        static ProgressPopup? currentPopup;
        static readonly object locker = new();

        public static async Task ShowAsync(string message = "Please wait...")
        {
            lock (locker)
            {
                if (currentPopup != null)
                    return; // already shown

                currentPopup = new ProgressPopup(message);
            }

            await MainThread.InvokeOnMainThreadAsync(async () =>
            {
                await Application.Current.MainPage.ShowPopupAsync(currentPopup);
            });
        }

        public static void Dismiss()
        {
            lock (locker)
            {
                if (currentPopup == null)
                    return;
            }

            MainThread.BeginInvokeOnMainThread(() =>
            {
                currentPopup?.CloseAsync();  // NOW VALID
                currentPopup = null;
            });
        }
    }
}
