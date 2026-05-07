using ETHAN.ViewModel;
using ETHAN.Views;
using ZXing.Net.Maui;
using ZXing.Net.Maui.Controls;
using CommunityToolkit.Maui.Markup;
using CommunityToolkit.Maui;
using ETHAN.ProgressDialog;
using epj.Expander.Maui;
using System.Globalization;
using ETHAN.Services;
//using The49.Maui.BottomSheet;


#if ANDROID
using Android.Views;
using Microsoft.Maui.LifecycleEvents;
#endif

#if IOS
using Microsoft.Maui.Handlers;
using UIKit;
#endif

namespace ETHAN
{
    public static class MauiProgram
    {
        public static MauiApp CreateMauiApp()
        {
            var builder = MauiApp.CreateBuilder();
            //builder.UseMauiApp<App>().UseBarcodeReader() // Make sure to add this line
            //.UseBottomSheet().UseMauiCommunityToolkitMarkup().ConfigureFonts(fonts =>
            //{
            //    fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
            //    fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
            //}).UseMauiCommunityToolkit().UseProgressDialogService_().UseExpander();

            builder.UseMauiApp<App>().UseBarcodeReader() // Make sure to add this line
            .UseMauiCommunityToolkitMarkup().ConfigureFonts(fonts =>
            {
                fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
                fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
            }).UseMauiCommunityToolkit().UseProgressDialogService_().UseExpander();


            // enable animations for Android & iOS
            Expander.EnableAnimations();

            //#if DEBUG
            //            builder.Logging.AddDebug();
            //#endif

            ////AddSingleton creates one copy and keep tat memory
            ////AddTransient creates one n destroy and a new one will be created every single time
            //builder.Services.AddSingleton<Homepage>();
            //builder.Services.AddSingleton<HomepageVM>();
            //builder.Services.AddTransient<Homepage>();
            builder.Services.AddTransient<Home_Page>();
            builder.Services.AddTransient<HomepageVM>();
            builder.Services.AddTransient<CreateJob>();
            builder.Services.AddSingleton<CreateJobVM>();
            builder.Services.AddTransient<AddressPage>();
            //builder.Services.AddTransient<Address2>();
            builder.Services.AddTransient<AddressBookPage>();
            //builder.Services.AddTransient<AddressEditPage>();
            builder.Services.AddTransient<ManageJobPage>();
            builder.Services.AddTransient<ManageJobPageVM>();
            builder.Services.AddTransient<InventoryPage>();
            builder.Services.AddTransient<InventoryPageVM>();
            builder.Services.AddTransient<PrepaidListPage>();

            builder.Services.AddTransient<CardShellPage>();
            builder.Services.AddTransient<Home_Page>();
            builder.Services.AddTransient<ChatMainPage>();
            builder.Services.AddTransient<SettingsPage>();

            builder.Services.AddSingleton<ISoapService, SoapService>();
            builder.Services.AddTransient<ChatViewModel>();
            builder.Services.AddTransient<ChatPage>();
            //builder.Services.AddSingleton<IProgressDialogServiceNew, ProgressDialogServiceNew>();
            builder.Services.AddSingleton<IProgressDialogService, ProgressDialogService_>();

            //Enforce dd/MM/yyyy and 24-hour invariant culture globally
            var culture = new CultureInfo("en-GB"); // en-GB uses dd/MM/yyyy format
            CultureInfo.DefaultThreadCurrentCulture = culture;
            CultureInfo.DefaultThreadCurrentUICulture = culture;

#if ANDROID
builder.ConfigureLifecycleEvents(events =>
{
    events.AddAndroid(android =>
    {
        android.OnCreate((activity, _) =>
        {
            activity.Window.SetSoftInputMode(
                SoftInput.AdjustResize
            );
        });
    });
});
#endif

#if IOS
builder.ConfigureMauiHandlers(handlers =>
{
    //Remove border for Entry
    EntryHandler.Mapper.AppendToMapping("NoBorder", (handler, view) =>
    {
        handler.PlatformView.BorderStyle = UITextBorderStyle.None;
        handler.PlatformView.BackgroundColor = UIColor.Clear;
    });

    //Remove border for DatePicker
    DatePickerHandler.Mapper.AppendToMapping("NoBorder", (handler, view) =>
    {
        if (handler.PlatformView is UITextField textField)
        {
            textField.BorderStyle = UITextBorderStyle.None;
            textField.BackgroundColor = UIColor.Clear;
        }
    });

    // Remove border for Picker
    PickerHandler.Mapper.AppendToMapping("NoBorder", (handler, view) =>
    {
        if (handler.PlatformView is UITextField textField)
        {
            textField.BorderStyle = UITextBorderStyle.None;
            textField.BackgroundColor = UIColor.Clear;
        }
    });
});
#endif

            return builder.Build();
        }
    }
}