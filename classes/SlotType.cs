using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ETHAN.classes
{
    public class SlotType
    {
        public string dispText { get; set; }
        public int value { get; set; }

        public SlotType(int v, string txt)
        {
            this.value = v;
            this.dispText = txt;
        }
    }
}
