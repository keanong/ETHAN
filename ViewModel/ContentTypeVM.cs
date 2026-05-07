using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace ETHAN.ViewModel
{
    public class ContentTypeVM : INotifyPropertyChanged
    {
        readonly IList<ContentType> source;
        ContentType selectedContentType;
        int selectionCount = 1;

        public ObservableCollection<ContentType> ContentTypes { get; private set; }
        public IList<ContentType> EmptyContentTypes { get; private set; }

        public ContentType SelectedContentType
        {
            get
            {
                return selectedContentType;
            }
            set
            {
                if (selectedContentType != value)
                {
                    selectedContentType = value;
                }
            }
        }

        ObservableCollection<object> selectedContentTypes;
        public ObservableCollection<object> SelectedContentTypes
        {
            get
            {
                return selectedContentTypes;
            }
            set
            {
                if (selectedContentTypes != value)
                {
                    selectedContentTypes = value;
                }
            }
        }

        public string SelectedContentTypeMessage { get; private set; }

        public ICommand DeleteCommand => new Command<ContentType>(RemoveContentType);
        public ICommand FavoriteCommand => new Command<ContentType>(FavoriteContentType);
        public ICommand FilterCommand => new Command<string>(FilterItems);
        public ICommand ContentTypeSelectionChangedCommand => new Command(ContentTypeSelectionChanged);

        public ContentTypeVM()
        {
            source = new List<ContentType>();
            createCT();
        }

        void createCT()
        {
            source.Add(new ContentType { Title = "Document", num = "1" });
            source.Add(new ContentType { Title = "Light Parcel", num = "2" });
            source.Add(new ContentType { Title = "Parcel", num = "3" });
            source.Add(new ContentType { Title = "Medication", num = "4" });
            source.Add(new ContentType { Title = "Sim Card", num = "5" });
            ContentTypes = new ObservableCollection<ContentType>(source);
        }

        void FilterItems(string filter)
        {
            var filteredItems = source.Where(ct => ct.Title.ToLower().Contains(filter.ToLower())).ToList();
            foreach (var ct in source)
            {
                if (!filteredItems.Contains(ct))
                {
                    ContentTypes.Remove(ct);
                }
                else
                {
                    if (!ContentTypes.Contains(ct))
                    {
                        ContentTypes.Add(ct);
                    }
                }
            }
        }

        void ContentTypeSelectionChanged()
        {
            SelectedContentTypeMessage = $"Selection {selectionCount}: {SelectedContentType.Title}";
            OnPropertyChanged("SelectedContentTypeMessage");
            selectionCount++;
        }

        void RemoveContentType(ContentType ct)
        {
            if (ContentTypes.Contains(ct))
            {
                ContentTypes.Remove(ct);
            }
        }

        void FavoriteContentType(ContentType ct)
        {
            //ct.IsFavorite = !ct.IsFavorite;
        }


        #region INotifyPropertyChanged
        public event PropertyChangedEventHandler PropertyChanged;

        void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
        #endregion
    }
}
