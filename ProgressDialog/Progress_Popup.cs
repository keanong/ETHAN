using CommunityToolkit.Maui.Views;
using Microsoft.Maui.Controls;

namespace ETHAN.ProgressDialog
{
    public class Progress_Popup : Popup
    {
        private readonly Label _label;
        private readonly ActivityIndicator _spinner;

        public Progress_Popup(string message)
        {
            CanBeDismissedByTappingOutsideOfPopup = false;


            _label = new Label
            {
                Text = message,
                TextColor = Colors.Black,
                HorizontalOptions = LayoutOptions.Center,
                HorizontalTextAlignment = TextAlignment.Center
            };

            _spinner = new ActivityIndicator
            {
                IsRunning = true,
                Color = Colors.Blue,
                WidthRequest = 50,
                HeightRequest = 50,
                HorizontalOptions = LayoutOptions.Center
            };

            Content = new Grid
            {
                BackgroundColor = Colors.Transparent, // no dim
                Children =
                {
                    new VerticalStackLayout
                    {
                        HorizontalOptions = LayoutOptions.Center,
                        VerticalOptions = LayoutOptions.Center,
                        Spacing = 10,
                        Children = { _spinner, _label }
                    }
                }
            };
        }

        public void UpdateMessage(string message) => _label.Text = message;
    }
}
