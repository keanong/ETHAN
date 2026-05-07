using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ETHAN.classes
{
    public class InventoryItem
    {

        public int SN { get; set; } = 0;

        public string StockCode { get; set; } = string.Empty;

        public string Description { get; set; } = string.Empty;

        public int Balance { get; set; } = 0;
    }
}
