using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ETHAN.ProgressDialog
{
    public interface IProgressDialogServiceNew
    {
        Task ShowAsync(string message = "");
        Task DismissAsync();
    }
}
