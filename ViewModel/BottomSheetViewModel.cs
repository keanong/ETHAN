using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ETHAN.ViewModel
{
    public class BottomSheetViewModel
    {
        public ObservableCollection<Item> Items1 { get; set; }
        public ObservableCollection<Item> Items2 { get; set; }
        public ObservableCollection<Item> Items3 { get; set; }
        public ObservableCollection<Item> Items4 { get; set; }
        public BottomSheetViewModel()
        {
            Items1 = new ObservableCollection<Item>
            {
                new Item { Name = "Item 1A" },
                new Item { Name = "Item 2A" },
                new Item { Name = "Item 3A" },
                new Item { Name = "Item 4A" },
                new Item { Name = "Item 5A" },
                new Item { Name = "Item 6A" },
                new Item { Name = "Item 7A" },
                new Item { Name = "Item 8A" },
                new Item { Name = "Item 9A" },
                new Item { Name = "Item 10A" },
                new Item { Name = "Item 11A" },
                new Item { Name = "Item 12A" },
                new Item { Name = "Item 13A" },
                new Item { Name = "Item 14A" },
                new Item { Name = "Item 15A" },
                new Item { Name = "Item 16A" },
                new Item { Name = "Item 17A" },
                new Item { Name = "Item 18A" },
                new Item { Name = "Item 19A" },
                new Item { Name = "Item 20A" },
                new Item { Name = "Item 21A" }
            };

            Items2 = new ObservableCollection<Item>
            {
                new Item { Name = "Items 1B" },
                new Item { Name = "Items 2B" },
                new Item { Name = "Items 3B" },
                new Item { Name = "Items 4B" },
                new Item { Name = "Items 5B" },
                new Item { Name = "Items 6B" },
                new Item { Name = "Items 7B" },
                new Item { Name = "Items 8B" },
                new Item { Name = "Items 9B" },
                new Item { Name = "Items 10B" },
                new Item { Name = "Items 11B" },
                new Item { Name = "Items 12B" },
                new Item { Name = "Items 13B" },
                new Item { Name = "Items 14B" },
                new Item { Name = "Items 15B" },
                new Item { Name = "Items 16B" },
                new Item { Name = "Items 17B" },
                new Item { Name = "Items 18B" },
                new Item { Name = "Items 19B" },
                new Item { Name = "Items 20B" },
                new Item { Name = "Items 21B" }
            };

            Items3 = new ObservableCollection<Item>
            {
                new Item { Name = "1A" },
                new Item { Name = "2A" },
                new Item { Name = "3A" },
                new Item { Name = "4A" },
                new Item { Name = "5A" },
                new Item { Name = "6A" },
                new Item { Name = "7A" },
                new Item { Name = "8A" },
                new Item { Name = "9A" },
                new Item { Name = "10A" },
                new Item { Name = "11A" },
                new Item { Name = "12A" },
                new Item { Name = "13A" },
                new Item { Name = "14A" },
                new Item { Name = "15A" },
                new Item { Name = "16A" },
                new Item { Name = "17A" },
                new Item { Name = "18A" },
                new Item { Name = "19A" },
                new Item { Name = "20A" },
                new Item { Name = "21A" }
            };

            Items4 = new ObservableCollection<Item>
            {
                new Item { Name = "1B" },
                new Item { Name = "2B" },
                new Item { Name = "3B" },
                new Item { Name = "4B" },
                new Item { Name = "5B" },
                new Item { Name = "6B" },
                new Item { Name = "7B" },
                new Item { Name = "8B" },
                new Item { Name = "9B" },
                new Item { Name = "10B" },
                new Item { Name = "11B" },
                new Item { Name = "12B" },
                new Item { Name = "13B" },
                new Item { Name = "14B" },
                new Item { Name = "15B" },
                new Item { Name = "16B" },
                new Item { Name = "17B" },
                new Item { Name = "18B" },
                new Item { Name = "19B" },
                new Item { Name = "20B" },
                new Item { Name = "21B" }
            };
        }
    }

    public class Item
    {
        public string Name { get; set; }
    }
}
