using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace ETHAN.ViewModel
{
    [QueryProperty("Barcodeval", "bcval")]

    //to bind this to Homepage.xaml, Homepage.xaml need to add x:DataType="viewmodel:HomepageVM" in ContentPage
    public partial class HomepageVM : ObservableObject
    {
        [ObservableProperty]
        string barcodeval = string.Empty;

        public HomepageVM()
        {

        }

        [RelayCommand]
        async Task CreateJobClick()
        {
            try
            {
                //await Shell.Current.GoToAsync("CreateJob");

                // Pass null as a parameter when navigating to CreateJob
                await Shell.Current.GoToAsync("CreateJob", new Dictionary<string, object>
                    {
                        { "vmm", null } // Pass null for the CreateJobVM parameter
                    });

            } catch (Exception ex)
            {
                string s = ex.Message;
            }
        }

        [RelayCommand]
        async Task btnManageJClick()
        {
            try
            {
                await Shell.Current.GoToAsync("/ManageJobPage");

                //await Shell.Current.GoToAsync("ManageJobPage");

            }
            catch (Exception ex)
            {
                string s = ex.Message;
            }
        }

        [RelayCommand]
        async Task btnTopUpClick()
        {
            try
            {
                await Shell.Current.GoToAsync("/RadioButtonControlTemplatePage");

                //await Shell.Current.GoToAsync("RadioButtonControlTemplatePage");
            }
            catch (Exception ex)
            {
                string s = ex.Message;
            }
        }

        [RelayCommand]
        async Task btnChatClick()
        {
            try
            {
                //await Shell.Current.GoToAsync("/CreateJobPage");

                await Shell.Current.GoToAsync("/ChatSupportPage");
                //await Shell.Current.GoToAsync("ChatSupportPage");
            }
            catch (Exception ex)
            {
                string s = ex.Message;
            }
        }

        [RelayCommand]
        async Task btnScanClick()
        {
            try
            {
                await Shell.Current.GoToAsync("/Barcode");
                //await Shell.Current.GoToAsync("Barcode");
            }
            catch (Exception ex)
            {
                string s = ex.Message;
            }
        }

        [RelayCommand]
        async Task Logout()
        {
            try
            {
                if (await AppShell.Current.DisplayAlert("Are you sure?", "You will be logged out.", "Yes", "No"))
                {
                    SecureStorage.RemoveAll();
                    await Shell.Current.GoToAsync("///Login");
                    //await Shell.Current.GoToAsync("Login");
                }
            }
            catch (Exception ex)
            {
                string s = ex.Message;
            }
        }


    }
}
