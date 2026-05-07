using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ETHAN.ProgressDialog
{
    public static class UiPump
    {
        /// <summary>
        /// Forces Android UI thread to complete one full render frame.
        /// Required after PushModalAsync / DisplayAlert / overlays.
        /// </summary>
        public static Task Yield()
        {
            return MainThread.InvokeOnMainThreadAsync(() => Task.CompletedTask);
        }
    }
}
