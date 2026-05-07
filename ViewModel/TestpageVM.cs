using CommunityToolkit.Mvvm.Input;

namespace ETHAN.ViewModel
{
    public partial class TestpageVM
    {

        public TestpageVM()
        {

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


    }
}
