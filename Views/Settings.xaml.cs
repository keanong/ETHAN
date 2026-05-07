namespace ETHAN.Views;

public partial class Settings : ContentPage
{
	public Settings()
	{
		InitializeComponent();
    }
    private async void LogoutButton_Clicked(object sender, EventArgs e)
    {
        if (await DisplayAlert("Are you sure?", "You will be logged out.", "Yes", "No"))
        {
            SecureStorage.RemoveAll();

            //await Shell.Current.GoToAsync("///Login");
            await Shell.Current.GoToAsync("Login");
        }
    }

}