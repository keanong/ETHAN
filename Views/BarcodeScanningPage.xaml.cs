using ZXing.Net.Maui;
using ZXing.Net.Maui.Controls;
using ETHAN.classes;
using System.Threading.Tasks;

namespace ETHAN.Views;

/// <summary>
/// sample code from https://github.com/Redth/ZXing.Net.Maui
/// </summary>

[QueryProperty(nameof(LOGININFO), "LOGININFO")] // Add a QueryProperty to handle the navigation parameter

public partial class BarcodeScanningPage : ContentPage
{

    private LoginInfo? logininfo;
    public LoginInfo? LOGININFO
    {
        set
        {
            logininfo = value;
        }
    }

    String v = "";

    public BarcodeScanningPage()
    {
        InitializeComponent();
        Shell.SetTabBarIsVisible(this, false);
        try
        {
            barcodeView.Options = new BarcodeReaderOptions
            {
                Formats = BarcodeFormats.All,
                AutoRotate = true,
                Multiple = false
            };
        }
        catch (Exception ex)
        {
            String s = ex.Message;
        }

    }

    protected override void OnAppearing()
    {
        base.OnAppearing();
        barcodeView.IsDetecting = true;
    }

    protected override void OnDisappearing()
    {
        barcodeView.IsDetecting = false;
        base.OnDisappearing();
    }

    protected async void BarcodesDetected(object sender, BarcodeDetectionEventArgs e)
    {
        //String v = "";
        try
        {
            var first = e.Results?.FirstOrDefault();
            if (first is not null)
            {
                v = first.Value;
            }

            // Stop the camera/barcode reader
            barcodeView.IsDetecting = false;

            // Small delay to ensure resources are released
            await Task.Delay(50);

            MainThread.BeginInvokeOnMainThread(async () =>
                {
                    try
                    {
                        await Shell.Current.GoToAsync("..", new Dictionary<string, object>
                        {
                            { "LOGININFO", logininfo },
                            { "BARCODE", v }
                        });
                    }
                    catch (Exception ex)
                    {
                        string s = ex.Message;
                    }
                }
            );

        }
        catch (InvalidOperationException ex)
        {
            Console.WriteLine($"An error occurred: {ex.Message}");
        }
        catch (Exception ex)
        {
            String s = ex.Message;
        }

    }

    void SwitchCameraButton_Clicked(object sender, EventArgs e)
    {
        try
        {
            barcodeView.CameraLocation = barcodeView.CameraLocation == CameraLocation.Rear ? CameraLocation.Front : CameraLocation.Rear;
        } catch (Exception ex)
        {
            String s = ex.Message;
        }
    }

    void TorchButton_Clicked(object sender, EventArgs e)
    {
        try
        {
            barcodeView.IsTorchOn = !barcodeView.IsTorchOn;
        } catch (Exception ex)
        {
            String s = ex.Message;
        }
    }

    protected override bool OnBackButtonPressed()
    {
        try
        {
            MainThread.BeginInvokeOnMainThread(async () =>
            {
                await Shell.Current.GoToAsync("///CardShellPage", new Dictionary<string, object>
                    {
                        { "LOGININFO", logininfo },
                        { "BARCODE", null }
                    });
            }
            );
        }
        catch (Exception ex)
        {
            string s = ex.Message;
        }
        return true;
    }

    void BackToHome(System.Object sender, System.EventArgs e)
    {
        try
        {
            MainThread.BeginInvokeOnMainThread(async () =>
            {
                await Shell.Current.GoToAsync("///Homepage", new Dictionary<string, object>
                    {
                        { "LOGININFO", logininfo },
                        { "BARCODE", null }
                    });
            }
           );
        }
        catch (Exception ex)
        {
            string s = ex.Message;
        }
    }


}