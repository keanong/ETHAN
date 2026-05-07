using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ETHAN.classes;

namespace ETHAN.ViewModel
{
    public class InvoicesVM
    {

        public ObservableCollection<invoice> invoices { get; set; }

        public InvoicesVM()
        {
            invoices = new ObservableCollection<invoice>();
        }

        public void add(invoice invoice)
        {
            invoices.Add(invoice);
        }
    }
}
