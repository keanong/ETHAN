using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ETHAN.classes;

namespace ETHAN.XDelSys
{
    public class JobInfo
    {
        public static readonly TimeSpan DefStartTime = new TimeSpan(9, 0, 0);
        public static readonly TimeSpan DefAOHStart = new TimeSpan(18, 0, 0);
        public static readonly TimeSpan Def1100Time = new TimeSpan(11, 0, 0);
        public static readonly TimeSpan Def1300Time = new TimeSpan(13, 0, 0);
        public static readonly TimeSpan Def1600Time = new TimeSpan(16, 0, 0);
        public static readonly TimeSpan Def1800Time = new TimeSpan(18, 0, 0);

        public static String FormatURL(String AURLTemplate, Int64 AIDX)
        {
            return String.Format(AURLTemplate, AIDX, Hash.GetSaltedEncodedHash(AIDX.ToString(), Constants.XDelImgSalt));
        }

    }
}
