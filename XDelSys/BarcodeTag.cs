using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ETHAN.XDelSys
{
    public static class BarcodeTag
    {
        private static readonly Int32[] SaltValue = { 41, 59, 37, 73, 31, 27, 23, 89 };

        public static Boolean IsValidArticleTag(String ABarcode)
        {
            if (ABarcode.Length != 10)
                return false;
            String SRA = ABarcode.Substring(0, 1);  // First character must be alphabet
            String SRB = ABarcode.Substring(1, 1);  // Second character Must be alphabet
            String RSN = ABarcode.Substring(2, 6);  // Must be numeric only
            Int32 n;
            if (Int32.TryParse(SRA, out n) || Int32.TryParse(SRB, out n) || !Int32.TryParse(RSN, out n))
                return false;
            String Val = ABarcode.Substring(0, 8).ToUpper();
            String CheckSum = ABarcode.Substring(8, 2).ToUpper();
            Byte[] asc = Encoding.ASCII.GetBytes(Val);
            Int32 agg = (asc[0] * SaltValue[0]) +
                (asc[1] * SaltValue[1]) +
                (asc[2] * SaltValue[2]) +
                (asc[3] * SaltValue[3]) +
                (asc[4] * SaltValue[4]) +
                (asc[5] * SaltValue[5]) +
                (asc[6] * SaltValue[6]) +
                (asc[7] * SaltValue[7]);
            Byte b = Convert.ToByte(agg % 256);
            String Verify = String.Format("{0:x2}", b).ToUpper();
            return (CheckSum.Equals(Verify));
        }

    }
}
