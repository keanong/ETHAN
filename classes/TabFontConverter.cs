using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ETHAN.ViewModel;

namespace ETHAN.classes
{
    public class TabFontConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            int selectedIndex = (int)value;
            var currentTab = parameter as TabItem;
            if (currentTab == null) return FontAttributes.None;

            var tabs = App.Current.MainPage?.BindingContext as ManageJobPageVM;
            if (tabs == null) return FontAttributes.None;

            return tabs.Tabs[selectedIndex] == currentTab ? FontAttributes.Bold : FontAttributes.None;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
            => throw new NotImplementedException();
    }
}
