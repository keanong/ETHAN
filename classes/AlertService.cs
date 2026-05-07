using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ETHAN.classes
{
    public static class AlertService
    {
        public static async Task ShowError(string title, string message)
        {
            var page = Application.Current?.Windows.FirstOrDefault()?.Page;
            if (page != null)
            {
                await page.DisplayAlert(title, message, "OK");
            }
        }
    }
}
