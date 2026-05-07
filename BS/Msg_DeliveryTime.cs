using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ETHAN.BS
{
    public class Msg_DeliveryTime
    {
        public string fromdate { get; }
        public string todate { get; }

        public Msg_DeliveryTime(string fromdate, string todate)
        {
            this.fromdate = fromdate;
            this.todate = todate;
        }
    }
}
