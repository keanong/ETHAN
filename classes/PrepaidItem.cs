using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ETHAN.classes
{
    public class PrepaidItem
    {
        public long IDX { get; set; } = 0;

        public long CL_CONTACTIDX { get; set; } = 0;

        public string INV { get; set; } = "";

        public string DATEISSUED { get; set; } = "";

        public string AMT { get; set; } = "";

        public string CURR_STATUS { get; set; } = "";

        public int HAS_INVOICEPDF { get; set; } = 0;

        public int HAS_RECEIPTPDF { get; set; } = 0;

        public string PrePaidInvoicePDFURL { get; set; } = string.Empty;

        public string PrePaidReceiptPDFURL { get; set; } = string.Empty;

    }
}
