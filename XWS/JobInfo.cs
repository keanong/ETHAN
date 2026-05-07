using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ETHAN.classes;
using ETHAN.XDelSys;

namespace ETHAN.XWS
{
    public class JobInfo
    {
        public const String ReceiptPDFDirectURL = @"https://www.xdel.com/XDelTrack/RetrieveImage.aspx?JobReceiptIDX={0}&Hash={1}";
        public const String PrePaidInvoicePDFURL = @"https://www.xdel.com/XDelTrack/RetrieveImage.aspx?PPIIDX={0}&Hash={1}";
        public const String PrePaidReceiptPDFURL = @"https://www.xdel.com/XDelTrack/RetrieveImage.aspx?PPRIDX={0}&Hash={1}";





        public static String FormatURL(String AURLTemplate, Int64? AIDX)
        {
            return String.Format(AURLTemplate, AIDX, Hash.GetSaltedEncodedHash(AIDX.ToString(), Constants.XDelImgSalt));
        }

    }
}
