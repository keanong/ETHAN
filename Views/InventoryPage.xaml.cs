using ETHAN.classes;
using ETHAN.Network;
using ETHAN.ProgressDialog;
using ETHAN.ViewModel;
using MiniExcelLibs;
using System.Collections.ObjectModel;
using XDelServiceRef;

namespace ETHAN.Views;

[QueryProperty(nameof(Vmm), "vmm")] // Add a QueryProperty to handle the navigation parameter
//[QueryProperty(nameof(LOGININFO), "LOGININFO")] // Add a QueryProperty to handle the navigation parameter

public partial class InventoryPage : ContentPage
{
    //private XWSSoapClient xs = new XWSSoapClient(XWSSoapClient.EndpointConfiguration.XWSSoap);
    private XOEWSSoapClient xs = new XOEWSSoapClient(XOEWSSoapClient.EndpointConfiguration.XOEWSSoap);
    //private ProgressDialogService? _progressService;
    private readonly IProgressDialogService _progressService;
    private bool _isInitialized = false;

    private InventoryPageVM? vm;
    public InventoryPageVM Vmm
    {
        set
        {
            if (value != null)
            {
                vm = value;
                BindingContext = vm;
            }
            else if (vm == null)
            {
                vm = new InventoryPageVM();
                BindingContext = vm;
            }
        }
    }

    private LoginInfo? logininfo;
    /*public LoginInfo? LOGININFO
    {
        set => logininfo = value;
    }*/


    public InventoryPage(IProgressDialogService progressService)
    {
        InitializeComponent();
        _progressService = progressService;
        Shell.SetTabBarIsVisible(this, false);
    }

    protected override void OnSizeAllocated(double width, double height)
    {
        base.OnSizeAllocated(width, height);

        if (DeviceInfo.Idiom == DeviceIdiom.Tablet)
        {
            //AppContainer.WidthRequest = Math.Min(width * 0.32, 400); // 40% of screen width, max 800
            //AppContainer.HeightRequest = Math.Min(height * 0.4, 800);
            AppContainer.WidthRequest = 500; // 40% of screen width, max 800
            AppContainer.HeightRequest = height;
        }
        else
        {
            AppContainer.WidthRequest = width; // fill phone screen
            AppContainer.HeightRequest = height;
        }
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        try
        {
            if (_isInitialized) return;

            logininfo = AppSession.logininfo;
            if (logininfo == null)
                return; // or await DisplayAlert("Error", "Login info missing.", "OK");

            _isInitialized = true;
            //await Task.Yield();
            await getInventory();
        }
        catch (Exception e)
        {
            string s = e.Message;
        }
    }

    private async void BackToHome(object sender, EventArgs e)
    {
        try
        {
            await BackToHomePage();
        }
        catch (Exception ex)
        {
            await DisplayAlertAsync("Exception", ex.Message, "OK");
        }
    }

    private async Task BackToHomePage()
    {
        try
        {
            BindingContext = null;

            await Shell.Current.GoToAsync("///CardShellPage", new Dictionary<string, object>
                {
                    { "BARCODE", null }
                });
        }
        catch (Exception e)
        {
            string s = e.Message;
        }
    }

    protected override bool OnBackButtonPressed()
    {
        try
        {
            if (_progressService != null && _progressService.IsShowing)
                return true;

            _ = BackToHomePage();
            return true;
        }
        catch (Exception e)
        {
            string s = e.Message;
        }
        return true;
    }

    private async Task getInventory()
    {
        try
        {
            if (NetworkHelper.IsDisconnected())
            {
                await DisplayAlertAsync("No Internet Connection", "Please check your internet connection and try again.", "OK");
                return;
            }

            btnExport.IsVisible = false;
            logininfo = AppSession.logininfo;
            if (logininfo?.clientInfo == null ||
                string.IsNullOrEmpty(logininfo.clientInfo.Web_UID) ||
                logininfo.clientInfo.CAIDX <= 0)
            {
                await DisplayAlertAsync("Session expired", "Please Login again.", "OK");
                await common.BackToLogin();
                return;
            }

            await showProgress_Dialog("Processing...");

            var ci = logininfo.clientInfo;
            var ilist = await xs.GetInventoryAsync(ci.Web_UID);

            await closeProgress_dialog();

            if (ilist == null)
            {
                await DisplayAlertAsync("Session expired", "Session expired.\nPlease Login again.", "OK");
                await common.BackToLogin();
                return;
            }

            if (ilist.Status == 0)
            {
                int c = 1;
                vm.newItems();
                foreach (var item in ilist.Items)
                {
                    vm.addItem(new InventoryItem
                    {
                        SN = c,
                        StockCode = item.StockCode,
                        Description = item.Description,
                        Balance = item.Balance
                    });
                    c++;
                }

                int v = vm.Items.Count;
                btnExport.IsVisible = v > 0;
                //cvInventory.ItemsSource = vm.Items;
            }
            else
            {
                await DisplayAlertAsync("", "Error loading Inventory list. Please try again.", "OK");
            }
        }
        catch (Exception ex)
        {
            await closeProgress_dialog();
            await DisplayAlertAsync("Exception", ex.Message, "OK");
        }
    }

    private bool _isStockCodeAscending = true;
    private bool _isDescriptionAscending = true;
    private bool _isBalanceAscending = true;

    private void OnSortByStockCodeClicked(object sender, EventArgs e)
    {
        try
        {
            if (vm?.Items == null || vm.Items.Count == 0) return;

            var sorted = _isStockCodeAscending
                ? vm.Items.OrderBy(x => x.StockCode)
                : vm.Items.OrderByDescending(x => x.StockCode);

            vm.newItems();
            int c = 1;
            foreach (var item in sorted)
            {
                item.SN = c;
                vm.addItem(item);
                c++;
            }

            cvInventory.ItemsSource = vm.Items;
            _isStockCodeAscending = !_isStockCodeAscending;
        }
        catch (Exception ex)
        {
            string s = ex.Message;
        }
    }

    private void OnSortByDescriptionClicked(object sender, EventArgs e)
    {
        try
        {
            if (vm?.Items == null || vm.Items.Count == 0) return;

            var sorted = _isDescriptionAscending
                ? vm.Items.OrderBy(x => x.Description)
                : vm.Items.OrderByDescending(x => x.Description);

            vm.newItems();
            int c = 1;
            foreach (var item in sorted)
            {
                item.SN = c;
                vm.addItem(item);
                c++;
            }

            cvInventory.ItemsSource = vm.Items;
            _isDescriptionAscending = !_isDescriptionAscending;
        }
        catch (Exception ex)
        {
            string s = ex.Message;
        }
    }

    private void OnSortByBalanceClicked(object sender, EventArgs e)
    {
        try
        {
            if (vm?.Items == null || vm.Items.Count == 0) return;

            var sorted = _isBalanceAscending
                ? vm.Items.OrderBy(x => x.Balance)
                : vm.Items.OrderByDescending(x => x.Balance);

            vm.newItems();
            int c = 1;
            foreach (var item in sorted)
            {
                item.SN = c;
                vm.addItem(item);
                c++;
            }

            cvInventory.ItemsSource = vm.Items;
            _isBalanceAscending = !_isBalanceAscending;
        }
        catch (Exception ex)
        {
            String s = ex.Message;
        }
    }

    async void btnExport_Clicked(object sender, EventArgs e)
    {
        try
        {
            var currentPage = App.Current?.Windows.FirstOrDefault()?.Page;

            if (currentPage is null)
                return; // or handle gracefully if app not ready

            bool answer = await currentPage.DisplayAlertAsync(
            "Confirmation",
            "Confirm to Export these data?",
            "OK",
            "Cancel");

            if (answer)
            {
                // User clicked OK
                await exportDataNew();
            }
            else
            {
                // User clicked Cancel — do nothing or close dialog
            }
        }
        catch (Exception ex)
        {
            string s = ex.Message;
        }
    }

    private async Task exportDataNew()
    {
        try
        {
            await showProgress_Dialog("Exporting Excel...");

            if (BindingContext is not InventoryPageVM vm || vm.Items == null || vm.Items.Count == 0)
            {
                await DisplayAlertAsync("", "There are no items to export.", "OK");
                return;
            }

            string fileName = $"Inventory_{DateTime.Now:yyyyMMdd_HHmmss}.xlsx";

#if ANDROID

            // ==========================
            // STEP 2 — Runtime Permission (API 28 and below)
            // ==========================
            if (Android.OS.Build.VERSION.SdkInt <= Android.OS.BuildVersionCodes.P)
            {
                var status = await Permissions.CheckStatusAsync<Permissions.StorageWrite>();

                if (status != PermissionStatus.Granted)
                {
                    status = await Permissions.RequestAsync<Permissions.StorageWrite>();

                    if (status != PermissionStatus.Granted)
                    {
                        await DisplayAlertAsync("Permission denied",
                            "Storage permission is required to export files.", "OK");
                        return;
                    }
                }
            }

            // Save to temporary cache first (always safe)
            string tempPath = Path.Combine(FileSystem.Current.CacheDirectory, fileName);
            MiniExcel.SaveAs(tempPath, vm.Items);

            var context = Android.App.Application.Context;
            var resolver = context.ContentResolver;

            Android.Net.Uri fileUri;

            // ==========================
            // STEP 3 — Backward Compatible Storage
            // ==========================
            if (Android.OS.Build.VERSION.SdkInt >= Android.OS.BuildVersionCodes.Q)
            {
                // ✅ Android 10+ (API 29+)
                var contentValues = new Android.Content.ContentValues();
                contentValues.Put(Android.Provider.MediaStore.IMediaColumns.DisplayName, fileName);
                contentValues.Put(Android.Provider.MediaStore.IMediaColumns.MimeType,
                    "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet");
                contentValues.Put(Android.Provider.MediaStore.IMediaColumns.RelativePath,
                    Android.OS.Environment.DirectoryDownloads);

                fileUri = resolver.Insert(
                    Android.Provider.MediaStore.Downloads.ExternalContentUri,
                    contentValues);

                if (fileUri == null)
                    throw new Exception("Failed to create file.");

                using var inputStream = File.OpenRead(tempPath);
                using var outputStream = resolver.OpenOutputStream(fileUri);

                if (outputStream == null)
                    throw new Exception("Failed to open stream.");

                await inputStream.CopyToAsync(outputStream);
                await outputStream.FlushAsync();
            }
            else
            {
                // ✅ Android 9 and below (API 28-)
#pragma warning disable CS0618
                var downloadsPath = Android.OS.Environment
                    .GetExternalStoragePublicDirectory(Android.OS.Environment.DirectoryDownloads);
#pragma warning restore CS0618

                if (!downloadsPath.Exists())
                    downloadsPath.Mkdirs();

                string finalPath = Path.Combine(downloadsPath.AbsolutePath, fileName);
                File.Copy(tempPath, finalPath, true);

                // Use FileProvider (required for sharing file:// safely)
                fileUri = AndroidX.Core.Content.FileProvider.GetUriForFile(
                    context,
                    context.PackageName + ".fileprovider",
                    new Java.IO.File(finalPath));
            }

            File.Delete(tempPath);

            // Share using intent
            var activity = Microsoft.Maui.ApplicationModel.Platform.CurrentActivity
                           ?? throw new Exception("Activity not found.");

            var intent = new Android.Content.Intent(Android.Content.Intent.ActionSend);
            intent.SetType("application/vnd.openxmlformats-officedocument.spreadsheetml.sheet");
            intent.PutExtra(Android.Content.Intent.ExtraStream, fileUri);
            intent.AddFlags(Android.Content.ActivityFlags.GrantReadUriPermission);

            var chooser = Android.Content.Intent.CreateChooser(intent, "Open Excel File");
            activity.StartActivity(chooser);

            await DisplayAlertAsync("", "Excel file saved to Downloads folder.", "OK");

#elif IOS

        // iOS apps are sandboxed — save inside app container
        string filePath = Path.Combine(FileSystem.Current.CacheDirectory, fileName);

        MiniExcel.SaveAs(filePath, vm.Items);

        await Share.Default.RequestAsync(new ShareFileRequest
        {
            Title = "Open Excel File",
            File = new ShareFile(filePath)
        });

        await DisplayAlertAsync("", "Excel file ready to share.", "OK");

#else

        string fallbackPath = Path.Combine(FileSystem.Current.AppDataDirectory, fileName);
        MiniExcel.SaveAs(fallbackPath, vm.Items);

        await DisplayAlertAsync("Success", $"Excel file saved to:\n{fallbackPath}", "OK");

#endif
        }
        catch (Exception ex)
        {
            await DisplayAlertAsync("Exception", ex.Message, "OK");
        }
        finally
        {
            await closeProgress_dialog();
        }
    }

    private async Task exportData()
    {
        try
        {
            await showProgress_Dialog("Exporting Excel...");

            if (BindingContext is not InventoryPageVM vm || vm.Items == null || vm.Items.Count == 0)
            {
                await DisplayAlertAsync("", "There are no items to export.", "OK");
                return;
            }

            string fileName = $"Inventory_{DateTime.Now:yyyyMMdd_HHmmss}.xlsx";

#if ANDROID
            // === ANDROID EXPORT ===
            var context = Android.App.Application.Context;
            var resolver = context.ContentResolver;

            //Request permission (Android 9 and below)
            if (Android.OS.Build.VERSION.SdkInt < Android.OS.BuildVersionCodes.Q)
            {
                var permissionStatus = await Permissions.CheckStatusAsync<Permissions.StorageWrite>();
                if (permissionStatus != PermissionStatus.Granted)
                {
                    permissionStatus = await Permissions.RequestAsync<Permissions.StorageWrite>();
                    if (permissionStatus != PermissionStatus.Granted)
                    {
                        await DisplayAlertAsync("Permission denied", "Storage access is required to export files.", "OK");
                        return;
                    }
                }
            }

            //Write to temporary local file (guaranteed valid Excel)
            string tempPath = Path.Combine(FileSystem.Current.CacheDirectory, fileName);
            MiniExcel.SaveAs(tempPath, vm.Items);

            //Determine output location (Downloads folder)
            Android.Net.Uri fileUri;

            if (Android.OS.Build.VERSION.SdkInt >= Android.OS.BuildVersionCodes.Q)
            {
                // Scoped storage path (Android 10+)
                var contentValues = new Android.Content.ContentValues();
                contentValues.Put(Android.Provider.MediaStore.IMediaColumns.DisplayName, fileName);
                contentValues.Put(Android.Provider.MediaStore.IMediaColumns.MimeType,
                    "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet");
                contentValues.Put(Android.Provider.MediaStore.IMediaColumns.RelativePath,
                    Android.OS.Environment.DirectoryDownloads);

                fileUri = resolver.Insert(Android.Provider.MediaStore.Downloads.ExternalContentUri, contentValues)
                           ?? throw new IOException("Failed to create file in Downloads folder.");

                // Copy from temp file → destination
                using var inputStream = File.OpenRead(tempPath);
                using var outputStream = resolver.OpenOutputStream(fileUri)
                                         ?? throw new IOException("Failed to open output stream.");
                await inputStream.CopyToAsync(outputStream);
                await outputStream.FlushAsync();
            }
            else
            {
#pragma warning disable CS0618
                var downloadsDir = Android.OS.Environment.GetExternalStoragePublicDirectory(Android.OS.Environment.DirectoryDownloads);
#pragma warning restore CS0618
                if (!downloadsDir.Exists()) downloadsDir.Mkdirs();

                string finalPath = Path.Combine(downloadsDir.AbsolutePath, fileName);
                File.Copy(tempPath, finalPath, true);
                fileUri = Android.Net.Uri.FromFile(new Java.IO.File(finalPath));
            }

            File.Delete(tempPath);

            //Launch Share / Open chooser using Activity context
            var activity = Microsoft.Maui.ApplicationModel.Platform.CurrentActivity ?? throw new NullReferenceException("Activity not found.");

            var intent = new Android.Content.Intent(Android.Content.Intent.ActionSend);
            intent.SetType("application/vnd.openxmlformats-officedocument.spreadsheetml.sheet");
            intent.PutExtra(Android.Content.Intent.ExtraStream, fileUri);
            intent.AddFlags(Android.Content.ActivityFlags.GrantReadUriPermission);

            var chooser = Android.Content.Intent.CreateChooser(intent, "Open Excel File");
            activity.StartActivity(chooser);

            await DisplayAlertAsync("", "Excel file saved to Downloads folder.", "OK");

#elif IOS
            // === iOS EXPORT ===
            string downloadsPath = Foundation.NSFileManager.DefaultManager.GetUrls(
                Foundation.NSSearchPathDirectory.DownloadsDirectory,
                Foundation.NSSearchPathDomain.User
            )?.FirstOrDefault()?.Path ?? Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);

            string filePath = Path.Combine(downloadsPath, fileName);
            MiniExcel.SaveAs(filePath, vm.Items);

            await Share.Default.RequestAsync(new ShareFileRequest
            {
                Title = "Open Excel File",
                File = new ShareFile(filePath)
            });

            await DisplayAlertAsync("", $"Excel file saved to:\n{filePath}", "OK");

#else
        // Other platforms fallback
        string fallbackPath = Path.Combine(FileSystem.Current.AppDataDirectory, fileName);
        MiniExcel.SaveAs(fallbackPath, vm.Items);
        await DisplayAlertAsync("Success", $"Excel file saved to:\n{fallbackPath}", "OK");
#endif
        }
        catch (Exception ex)
        {
            await DisplayAlertAsync("Exception", ex.Message, "OK");
        }
        finally
        {
            await closeProgress_dialog();
        }
    }

    private async Task showProgress_Dialog(string msg)
    {
        try
        {
            //await MainThread.InvokeOnMainThreadAsync(async () =>
            //{
            //    await _progressService.ShowAsync(msg);
            //});

            //await Task.Delay(50);

            await _progressService.ShowAsync(msg);
            await Task.Yield();
            await Task.Delay(100);
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine(ex);
        }
    }

    private async Task closeProgress_dialog()
    {
        try
        {
            //await MainThread.InvokeOnMainThreadAsync(async () =>
            //{
            //    await _progressService.DismissAsync();
            //});

            await _progressService.DismissAsync();
            await Task.Yield();
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine(ex);
        }
    }

}