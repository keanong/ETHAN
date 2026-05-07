using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ETHAN.ViewModel;

namespace ETHAN.classes
{
    // TabColorConverter.cs
    public class TabColorConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            int selectedIndex = (int)value;
            var currentTab = parameter as TabItem;
            if (currentTab == null) return Colors.Black;

            var tabs = App.Current.MainPage?.BindingContext as ManageJobPageVM;
            if (tabs == null) return Colors.Black;

            return tabs.Tabs[selectedIndex] == currentTab ? Color.FromArgb("#007AFF") : Colors.Black;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
            => throw new NotImplementedException();
    }
}
