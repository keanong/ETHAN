using Microsoft.Maui.Controls;

namespace ETHAN.Views;

public partial class BottomSheetDatePicker : ContentView
{
    public event Action<DateTime>? DateSelected;
    public event Action? Cancelled;

    private const uint SlideDuration = 250;
    private const uint FadeDuration = 100;
    private double _sheetHeight = 300; // adjust to match XAML

    public BottomSheetDatePicker()
    {
        InitializeComponent();

        // Measure the height of the bottom sheet after layout
        this.SizeChanged += (s, e) =>
        {
            _sheetHeight = SheetContainer.Height;
        };
    }

    public void Show(DateTime initialDate)
    {
        Picker.Date = initialDate;

        // Make visible
        IsVisible = true;

        // Start invisible and below screen
        Opacity = 0;
        TranslationY = _sheetHeight;

        // Animate: fade in and slide up
        this.FadeTo(1, FadeDuration);
        this.TranslateTo(0, 0, SlideDuration, Easing.CubicOut);
    }

    private async Task HideAsync()
    {
        // Slide down and fade out
        await this.TranslateTo(0, _sheetHeight, SlideDuration, Easing.CubicIn);
        await this.FadeTo(0, FadeDuration);
        IsVisible = false;
    }

    private async void Ok_Clicked(object sender, EventArgs e)
    {
        await HideAsync();
        DateSelected?.Invoke(Picker.Date ?? DateTime.Now);
    }

    private async void Cancel_Clicked(object sender, EventArgs e)
    {
        await HideAsync();
        Cancelled?.Invoke();
    }

    private async void Overlay_Tapped(object sender, EventArgs e)
    {
        await HideAsync();
        Cancelled?.Invoke();
    }
}