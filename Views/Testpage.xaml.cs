using ETHAN.ViewModel;

namespace ETHAN.Views;

public partial class Testpage : ContentPage
{
    int count = 0;

    public Testpage(TestpageVM vm)
    {
        try
        {
            InitializeComponent();
            BindingContext = vm;
            Shell.SetTabBarIsVisible(this, false);
        } catch (Exception ex)
        {
            string s = ex.Message;
        }
    }

    //public Testpage()
    //{
    //    InitializeComponent();
    //    Shell.SetTabBarIsVisible(this, false);
    //}

    private void OnCounterClicked(object sender, EventArgs e)
    {
        count++;

        if (count == 1)
            CounterBtn.Text = $"Clicked {count} time";
        else
            CounterBtn.Text = $"Clicked {count} times";

        SemanticScreenReader.Announce(CounterBtn.Text);
    }

    protected override bool OnBackButtonPressed()
    {
        try
        {
            MainThread.BeginInvokeOnMainThread(async () =>
            {
                //await Shell.Current.GoToAsync("///Homepage");
                string v = string.Empty;
                await Shell.Current.GoToAsync($"..?bcval={v}", true);
            }
            );
        }
        catch (Exception ex)
        {
            string s = ex.Message;
        }
        return true;
    }

    private async void NextButton_Clicked(object sender, EventArgs e)
    {
        //await Shell.Current.GoToAsync("///Homepage2");
        try
        {
            //await Shell.Current.GoToAsync("/Testpage2");
            //await Shell.Current.GoToAsync("/Barcode");
            await Navigation.PushAsync(new BarcodeScanningPage());
        } catch (Exception ex)
        {
            String s = ex.Message;
        }
    }

    private async void BackButton_Clicked(object sender, EventArgs e)
    {
        try
        {
            //await Shell.Current.GoToAsync("///Homepage");
            await Shell.Current.GoToAsync("///Homepage");
        } catch (Exception ex)
        {
            String s = ex.Message;
        }
    }

}