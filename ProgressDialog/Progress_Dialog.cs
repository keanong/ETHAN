using CommunityToolkit.Maui.Views;
using Microsoft.Maui.Controls.Shapes;

namespace ETHAN.ProgressDialog
{
    public class Progress_Dialog : CommunityToolkit.Maui.Views.Popup
    {
        private readonly Label _messageLabel;
        private readonly ProgressBar _progressBar;

        public Progress_Dialog(string message)
        {
            CanBeDismissedByTappingOutsideOfPopup = false;

            _progressBar = new ProgressBar
            {
                Progress = 0,
                WidthRequest = 200
            };

            _messageLabel = new Label
            {
                Text = message,
                HorizontalOptions = LayoutOptions.Center,
                HorizontalTextAlignment = TextAlignment.Center
            };

            Content = new Border
            {
                StrokeShape = new RoundRectangle
                {
                    CornerRadius = 10
                },
                Stroke = Colors.Transparent,
                BackgroundColor = Colors.White,
                Padding = new Thickness(20),
                Content = new VerticalStackLayout
                {
                    Spacing = 10,
                    Children =
                    {
                        new ActivityIndicator
                        {
                            IsRunning = true,
                            Color = Colors.Blue,
                            WidthRequest = 50,
                            HeightRequest = 50
                        },
                        _progressBar,
                        _messageLabel
                    }
                }
            };
        }

        public void UpdateMessage(string message) => _messageLabel.Text = message;
        public void UpdateProgress(double progress) => _progressBar.Progress = progress / 100;
    }
}
