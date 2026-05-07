using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Collections.ObjectModel;
using ETHAN.classes;
using System.Windows.Input;

namespace ETHAN.ViewModel
{
    public partial class ManageJobPageVM : ObservableObject
    {
        [ObservableProperty]
        private DateTime _fromDate = DateTime.Today;

        [ObservableProperty]
        private DateTime _toDate = DateTime.Today;

        [ObservableProperty]
        private int selectedTabIndex;

        [ObservableProperty]
        private ObservableCollection<TabItem> tabs = new();

        [ObservableProperty]
        private bool isRefreshing;

        public long _SelectedJobsIDX = 0;


        private DateTime? _FromDate;
        public DateTime? FDate
        {
            get => _FromDate;
            set => SetProperty(ref _FromDate, value);
        }

        private DateTime? _ToDate;
        public DateTime? TDate
        {
            get => _ToDate;
            set => SetProperty(ref _ToDate, value);
        }

        public bool isPopUp = false;

        //public ICommand ClearDateCommand => new Command(() => TDate = null);

        public ManageJobPageVM()
        {
            //LoadTabs();
        }

        [RelayCommand]
        public async Task LoadTabs()
        {
            Tabs.Clear();

            Tabs.Add(new TabItem
            {
                Title = "Draft",
                Items = new ObservableCollection<ManageJobSelector>
                {
                    //new ManageJobSelector { JobsIDX = 101, Title = "Draft Job 1"},
                    //new ManageJobSelector { JobsIDX = 102, Title = "Draft Job 2"},
                    //new ManageJobSelector { JobsIDX = 103, Title = "Draft Job 3"},
                    //new ManageJobSelector { JobsIDX = 104, Title = "Draft Job 4"},
                    //new ManageJobSelector { JobsIDX = 105, Title = "Draft Job 5"},
                    //new ManageJobSelector { JobsIDX = 106, Title = "Draft Job 6"},
                    //new ManageJobSelector { JobsIDX = 107, Title = "Draft Job 7"},
                    //new ManageJobSelector { JobsIDX = 108, Title = "Draft Job 8"},
                }
            });

            Tabs.Add(new TabItem
            {
                Title = "In Progress",
                Items = new ObservableCollection<ManageJobSelector>
                {
                    new ManageJobSelector { JobsIDX = 201, Title = "In Progress Job 1"},
                    new ManageJobSelector { JobsIDX = 202, Title = "In Progress Job 2"},
                    new ManageJobSelector { JobsIDX = 203, Title = "In Progress Job 3"},
                    new ManageJobSelector { JobsIDX = 204, Title = "In Progress Job 4"},
                }
            });

            Tabs.Add(new TabItem
            {
                Title = "Attempted",
                Items = new ObservableCollection<ManageJobSelector>
                {
                    new ManageJobSelector { JobsIDX = 301, Title = "Attempted Job 1"},
                    new ManageJobSelector { JobsIDX = 302, Title = "Attempted Job 2"},
                    new ManageJobSelector { JobsIDX = 303, Title = "Attempted Job 3"},
                    new ManageJobSelector { JobsIDX = 304, Title = "Attempted Job 4"},
                    new ManageJobSelector { JobsIDX = 305, Title = "Attempted Job 5"},
                }
            });

            Tabs.Add(new TabItem
            {
                Title = "Completed",
                Items = new ObservableCollection<ManageJobSelector>
                {
                    new ManageJobSelector { JobsIDX = 401, Title = "Completed Job 1"},
                    new ManageJobSelector { JobsIDX = 402, Title = "Completed Job 2"},
                    new ManageJobSelector { JobsIDX = 403, Title = "Completed Job 3"},
                }
            });

            SelectedTabIndex = 0;
            await Task.CompletedTask;
        }

        [RelayCommand]
        public async Task RefreshTabsAsync()
        {
            IsRefreshing = true;
            await LoadTabs();
            IsRefreshing = false;
        }



        public record LoadTabsRequest(
            ObservableCollection<ManageJobSelector>? Draft,
            ObservableCollection<ManageJobSelector>? InProgress,
            ObservableCollection<ManageJobSelector>? Attempted,
            ObservableCollection<ManageJobSelector>? Completed);

        public ObservableCollection<ManageJobSelector>? aDraft;
        public ObservableCollection<ManageJobSelector>? aInProgress;
        public ObservableCollection<ManageJobSelector>? aAttempted;
        public ObservableCollection<ManageJobSelector>? aCompleted;

        public LoadTabsRequest request_stored { get; set; } = null;

        [RelayCommand]
        public async Task LoadTabs2(LoadTabsRequest request)
        {
            try
            {

                Tabs.Clear();
                aDraft = request.Draft;
                aInProgress = request.InProgress;
                aAttempted = request.Attempted;
                aCompleted = request.Completed;

                string mode = AppSession.LoginMode;

                if (!mode.Equals("r"))
                    Tabs.Add(new TabItem
                    {
                        Title = "Draft",
                        Items = (request.Draft != null && request.Draft.Count > 0) ? request.Draft : new ObservableCollection<ManageJobSelector>
                        {
                        }
                    });

                Tabs.Add(new TabItem
                {
                    Title = "In Progress",
                    Items = (request.InProgress != null && request.InProgress.Count > 0) ? request.InProgress : new ObservableCollection<ManageJobSelector>
                    {
                    }
                });

                Tabs.Add(new TabItem
                {
                    Title = "Attempted",
                    Items = (request.Attempted != null && request.Attempted.Count > 0) ? request.Attempted : new ObservableCollection<ManageJobSelector>
                    {
                    }
                });

                Tabs.Add(new TabItem
                {
                    Title = "Completed",
                    Items = (request.Completed != null && request.Completed.Count > 0) ? request.Completed : new ObservableCollection<ManageJobSelector>
                    {
                    }
                });

                SelectedTabIndex = 0;
                request_stored = request;
            } catch (Exception e)
            {
                string s = e.Message;
            }
            await Task.CompletedTask;
        }

        [RelayCommand]
        public async Task ClearTabs()
        {
            Tabs.Clear();
            await Task.CompletedTask;
        }


        [RelayCommand]
        async Task BackToHome()
        {
            try
            {
                //await Shell.Current.GoToAsync("///Homepage");
                string v = string.Empty;
                await Shell.Current.GoToAsync($"..?bcval={v}", true);
            }
            catch (Exception ex)
            {
                string s = ex.Message;
            }
        }

        [RelayCommand]
        async Task cfmbox()
        {
            try
            {
                await Shell.Current.DisplayAlert("pending", "Collection Address form under construction.", "OK");
            }
            catch (Exception ex)
            {
                string s = ex.Message;
            }
        }



        public ObservableCollection<ManageJobOptionSelector> Options { get; set; }

        public void newOptions()
        {
            Options = new ObservableCollection<ManageJobOptionSelector>();
        }

        public void addOptions(ManageJobOptionSelector mjo)
        {
            Options.Add(mjo);
        }

        public ObservableCollection<ManageJobReschSelector> ReschSelectors { get; set; }

        public void newReschSelectors()
        {
            ReschSelectors = new ObservableCollection<ManageJobReschSelector>();
        }

        public void addReschSelectors(ManageJobReschSelector mjo)
        {
            ReschSelectors.Add(mjo);
        }

        public void SortReschSelectors()
        {
            if (ReschSelectors == null || ReschSelectors.Count == 0)
                return;

            var sorted = ReschSelectors
                .OrderBy(x => x.FromDateTime)
                .ThenBy(x => x.ToDateTime)
                .ToList();

            ReschSelectors = new ObservableCollection<ManageJobReschSelector>(sorted);
        }

        public XDelServiceRef.ChangeSetting? changeSetting {  get; set; }

        public bool txtPostalTextChangedSubscribed { get; set; } = false;

    }
}
