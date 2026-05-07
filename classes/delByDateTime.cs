using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace ETHAN.classes
{
    //public class delByDateTime
    //{
    //    public DateTime Date { get; set; }
    //    public string Time {  get; set; }
    //    public common.eExpressType ExpressType { get; set; }
    //    public string ExpressTypeStr { get; set; }
    //    public DateTime extDelfrom { get; set; } = DateTime.MinValue;
    //    public DateTime extDelby { get; set; } = DateTime.MinValue;
    //    public string dispText { get; set; }
    //    public string formattedDate { get; set; }
    //    public string extDelbyformattedDate { get; set; }
    //    public string value { get; set; }
    //    public string valueselected {  get; set; }
    //}

    public class delByDateTime : INotifyPropertyChanged
    {
        public DateTime Date { get; set; }
        public string Time { get; set; }
        public common.eExpressType ExpressType { get; set; }
        public string ExpressTypeStr { get; set; }
        public DateTime extDelfrom { get; set; } = DateTime.MinValue;
        public DateTime extDelby { get; set; } = DateTime.MinValue;
        public string dispText { get; set; }
        public string formattedDate { get; set; }
        public string extDelbyformattedDate { get; set; }
        public string value { get; set; }
        public string valueselected { get; set; }

        private bool _isSelected;
        public bool IsSelected
        {
            get => _isSelected;
            set { _isSelected = value; OnPropertyChanged(); }
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string name = null)
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    }
}
