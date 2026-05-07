
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ETHAN.classes
{
    public class invoice
    {
        public Int64 IDX { get; set; }
        public string DispText { get; set; }
        public decimal Amount { get; set; }
        public string AmountText { get; set; }

        public invoice(Int64 aIDX, string aDispText, decimal aAmount, string aAmountText) {
            this.IDX = aIDX;
            this.DispText = aDispText;
            this.Amount = aAmount;
            this.AmountText = aAmountText;
        }

    }
}
