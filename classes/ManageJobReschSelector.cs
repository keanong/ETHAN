using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ETHAN.classes
{
    public class ManageJobReschSelector
    {
        public string? dttw { get; set; } = string.Empty;

        public DateTime FromDateTime { get; set; }

        public DateTime ToDateTime { get; set; }

        public Int64 JobsIDX { get; set; } = 0;

    }
}
