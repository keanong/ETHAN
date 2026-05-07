using CommunityToolkit.Maui.Core;
using ETHAN.Views;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.Shapes;
using Microsoft.Maui.Graphics;
using System;
using System.Threading.Tasks;

namespace ETHAN.ProgressDialog
{
    /// <summary>
    /// Service for managing progress dialogs in .NET MAUI applications
    /// </summary>
    public interface IProgressDialogService
    {
        bool IsShowing { get; }

        /// <summary>
        /// Shows a progress dialog with a specified message
        /// </summary>
        /// <param name="message">The message to display in the progress dialog</param>
        Task ShowAsync(string message);

        /// <summary>
        /// Shows a progress dialog with a message and progress percentage
        /// </summary>
        /// <param name="message">The message to display in the progress dialog</param>
        /// <param name="progress">The progress percentage (0-100)</param>
        Task ShowAsync(string message, double progress);

        /// <summary>
        /// Updates the progress of the currently displayed dialog
        /// </summary>
        /// <param name="progress">The new progress percentage (0-100)</param>
        Task UpdateProgressAsync(double progress);

        /// <summary>
        /// Updates the message of the currently displayed dialog
        /// </summary>
        /// <param name="message">The new message to display</param>
        Task UpdateMessageAsync(string message);

        /// <summary>
        /// Dismisses the current progress dialog
        /// </summary>
        Task DismissAsync();
    }


    public class ProgressDialogService_ : IProgressDialogService
    {
        private ContentView _overlay;
        private Label _messageLabel;
        private ProgressBar _progressBar;
        public bool IsShowing => isShowing;
        public bool isShowing = false;

        public async Task ShowAsync(string message)
        {
            await MainThread.InvokeOnMainThreadAsync(() =>
            {
                //// Temporary — find double caller
                //System.Diagnostics.Debug.WriteLine($"[OVERLAY CALLER] {Environment.StackTrace}");

                // Get root once — single log line
                var root = GetOverlayHost();
                if (root == null) return;

                // Remove existing overlay first — handles double-call safely
                if (_overlay != null)
                {
                    //GetOverlayHost()?.Children.Remove(_overlay);
                    root.Children.Remove(_overlay);
                    _overlay = null;
                    isShowing = false;
                }

                if (isShowing) return;

                bool isDark = Application.Current.RequestedTheme == AppTheme.Dark;
                Color spinnerColor = isDark ? Colors.White : (Color)Application.Current.Resources["XDelOrange"];
                Color frameBg = isDark ? Colors.Black.MultiplyAlpha(0.8f) : Colors.White;
                Color messageTextColor = isDark ? Colors.White : Colors.Black;

                _messageLabel = new Label
                {
                    Text = message,
                    HorizontalTextAlignment = TextAlignment.Center,
                    TextColor = messageTextColor
                };

                _overlay = CreateOverlay(
                    new VerticalStackLayout
                    {
                        Spacing = 10,
                        HorizontalOptions = LayoutOptions.Center,
                        VerticalOptions = LayoutOptions.Center,
                        Children =
                        {
                    new ActivityIndicator { IsRunning = true, Color = spinnerColor },
                    _messageLabel
                        }
                    }, frameBg);

                //var root = GetOverlayHost();
                //if (root == null) return;

                root.Children.Add(_overlay);

                // Span ALL rows of the CardShellPage
                //Grid.SetRowSpan(_overlay, root.RowDefinitions.Count);
                Grid.SetRowSpan(_overlay, root.RowDefinitions.Count > 0 ? root.RowDefinitions.Count : 1);

                isShowing = true;
            });

            // Yield OUTSIDE the invoke — lets UI thread paint the overlay
            await Task.Yield();
            await Task.Delay(100);
        }

        public async Task ShowAsync(string message, double progress)
        {
            await MainThread.InvokeOnMainThreadAsync(() =>
            {
                if (isShowing) return;

                bool isDark = Application.Current.RequestedTheme == AppTheme.Dark;
                Color spinnerColor = isDark ? Colors.White : (Color)Application.Current.Resources["XDelOrange"];
                Color frameBg = isDark ? Colors.Black.MultiplyAlpha(0.8f) : Colors.White;
                Color messageTextColor = isDark ? Colors.White : Colors.Black;

                _progressBar = new ProgressBar
                {
                    Progress = progress / 100,
                    WidthRequest = 200,
                    ProgressColor = spinnerColor
                };

                _progressBar.SetAppThemeColor(ProgressBar.ProgressColorProperty,
                    isDark ? Colors.White : (Color)Application.Current.Resources["XDelOrange"], null);

                _messageLabel = new Label
                {
                    Text = message,
                    HorizontalTextAlignment = TextAlignment.Center,
                    TextColor = messageTextColor
                };

                _overlay = CreateOverlay(
                    new VerticalStackLayout
                    {
                        Spacing = 10,
                        HorizontalOptions = LayoutOptions.Center,
                        VerticalOptions = LayoutOptions.Center,
                        Children =
                        {
                _progressBar,
                _messageLabel
                        }
                    }, frameBg);

                var root = GetOverlayHost();
                if (root == null) return;

                root.Children.Add(_overlay);
                Grid.SetRowSpan(_overlay, root.RowDefinitions.Count);

                isShowing = true;


            });
        }

        public async Task UpdateProgressAsync(double progress)
        {
            await MainThread.InvokeOnMainThreadAsync(() =>
            {
                if (_progressBar != null)
                    _progressBar.Progress = progress / 100;
            });
        }

        public async Task UpdateMessageAsync(string message)
        {
            await MainThread.InvokeOnMainThreadAsync(() =>
            {
                if (_messageLabel != null)
                    _messageLabel.Text = message;
            });
        }

        public async Task DismissAsync()
        {
            await MainThread.InvokeOnMainThreadAsync(() =>
            {
                if (!isShowing || _overlay == null) return;

                var root = GetOverlayHost();
                root?.Children.Remove(_overlay);

                _overlay = null;
                _messageLabel = null;
                _progressBar = null;
                isShowing = false;
            });
        }

        #region Helpers

        private ContentView CreateOverlay(View content, Color frameBackground)
        {
            return new ContentView
            {
                InputTransparent = false,
                BackgroundColor = Colors.Black.MultiplyAlpha(0.5f),
                HorizontalOptions = LayoutOptions.Fill,
                VerticalOptions = LayoutOptions.Fill,
                Content = new Grid
                {
                    HorizontalOptions = LayoutOptions.Fill,
                    VerticalOptions = LayoutOptions.Fill,
                    Children =
            {
                new Border
                {
                    BackgroundColor = frameBackground,
                    Stroke = Colors.Transparent, // optional if no border
                    StrokeShape = new RoundRectangle { CornerRadius = 10 },
                    Padding = 20,
                    HorizontalOptions = LayoutOptions.Center,
                    VerticalOptions = LayoutOptions.Center,
                    Content = content
                }
            }
                }
            };
        }

        private Grid? GetOverlayHost()
        {
            //if (Shell.Current?.CurrentPage is CardShellPage shell)
            //    return shell.FindByName<Grid>("RootLayout");

            //if (Shell.Current?.CurrentPage is ContentPage page)
            //    return page.FindByName<Grid>("RootLayout");

            //return null;

            var currentPage = Shell.Current?.CurrentPage;
            //System.Diagnostics.Debug.WriteLine(
            //    $"[OVERLAY] CurrentPage={currentPage?.GetType().Name}, " +
            //    $"Shell.Current={Shell.Current != null}");

            if (currentPage is CardShellPage cardShell)
            {
                var grid = cardShell.FindByName<Grid>("RootLayout");
                //System.Diagnostics.Debug.WriteLine($"[OVERLAY] CardShellPage grid={grid != null}");
                return grid;
            }
            if (currentPage is ContentPage page)
            {
                var grid = page.FindByName<Grid>("RootLayout");
                //System.Diagnostics.Debug.WriteLine($"[OVERLAY] ContentPage grid={grid != null}");
                return grid;
            }

            //System.Diagnostics.Debug.WriteLine("[OVERLAY] NO MATCH — null returned");
            return null;
        }


        #endregion
    }

    public static class ProgressDialogServiceExtensions_
    {
        public static MauiAppBuilder UseProgressDialogService_(this MauiAppBuilder builder)
        {
            builder.Services.AddSingleton<IProgressDialogService, ProgressDialogService_>();
            return builder;
        }
    }
}
