using System;
using System.Threading.Tasks;
using Microsoft.Maui.Controls;

namespace ETHAN
{

    public static class PageExtensionsGlobal
    {
        public static Task<DateTime?> DisplayDatePickerAsync(
            this Page page,
            DateTime initialDate,
            DateTime minDate,
            DateTime maxDate)
        {
            var tcs = new TaskCompletionSource<DateTime?>();

            if (page is not ContentPage contentPage)
            {
                tcs.TrySetResult(null);
                return tcs.Task;
            }

            var picker = new DatePicker
            {
                Date = initialDate,
                MinimumDate = minDate,
                MaximumDate = maxDate,
                IsVisible = false
            };

            bool completed = false;

            void Cleanup()
            {
                if (contentPage.Content is Layout layout && layout.Children.Contains(picker))
                    layout.Children.Remove(picker);
                else if (contentPage.Content is Grid grid && grid.Children.Contains(picker))
                    grid.Children.Remove(picker);
            }

            // OK → user picked a date
            picker.DateSelected += (s, e) =>
            {
                if (!completed)
                {
                    completed = true;
                    tcs.TrySetResult(e.NewDate);
                    Cleanup();
                }
            };

            // Cancel → popup dismissed without picking
            picker.Unfocused += (s, e) =>
            {
                if (!completed)
                {
                    completed = true;
                    tcs.TrySetResult(null);
                    Cleanup();
                }
            };

            // Attach hidden picker
            if (contentPage.Content is Layout l)
            {
                l.Children.Add(picker);
            }
            else
            {
                var grid = new Grid();
                var existing = contentPage.Content;
                if (existing != null)
                    grid.Children.Add(existing);
                grid.Children.Add(picker);
                contentPage.Content = grid;
            }

            picker.Focus(); // Show native dialog

            return tcs.Task;
        }
    }

}