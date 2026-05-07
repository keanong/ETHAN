using CommunityToolkit.Maui.Core;
using CommunityToolkit.Maui.Views;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ETHAN.ProgressDialog
{
    /// <summary>
    /// Implementation of IProgressDialogService for .NET MAUI
    /// </summary>
    public class ProgressDialogService : IProgressDialogService
    {


        private Page _loadingPage;
        private Label _messageLabel;
        private ProgressBar _progressBar;
        public bool IsShowing => isShowing;
        public Boolean isShowing = false;

        public async Task ShowAsync(string message)
        {
            await MainThread.InvokeOnMainThreadAsync(() =>
            {
                // Create a full-screen loading overlay
                _loadingPage = new ContentPage
                {
                    BackgroundColor = Colors.Black.MultiplyAlpha(0.5f),
                    Content = new Grid
                    {
                        VerticalOptions = LayoutOptions.Center,
                        HorizontalOptions = LayoutOptions.Center,
                        Children =
                        {
                            new Frame
                            {
                                Background = Colors.White,
                                CornerRadius = 10,
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
                                        (_messageLabel = new Label
                                        {
                                            Text = message,
                                            HorizontalOptions = LayoutOptions.Center,
                                            HorizontalTextAlignment = TextAlignment.Center
                                        })
                                    }
                                }
                            }
                        }
                    }
                };

                isShowing = true;
                // Push the loading page
                Navigation.PushModalAsync(_loadingPage);
            });
        }

        public async Task ShowAsync(string message, double progress)
        {
            await MainThread.InvokeOnMainThreadAsync(() =>
            {
                // Create a full-screen loading overlay with progress bar
                _loadingPage = new ContentPage
                {
                    BackgroundColor = Colors.Black.MultiplyAlpha(0.5f),
                    Content = new Grid
                    {
                        VerticalOptions = LayoutOptions.Center,
                        HorizontalOptions = LayoutOptions.Center,
                        Children =
                        {
                            new Frame
                            {
                                Background = Colors.White,
                                CornerRadius = 10,
                                Padding = new Thickness(20),
                                Content = new VerticalStackLayout
                                {
                                    Spacing = 10,
                                    Children =
                                    {
                                        (_progressBar = new ProgressBar
                                        {
                                            Progress = progress / 100,
                                            WidthRequest = 200
                                        }),
                                        (_messageLabel = new Label
                                        {
                                            Text = message,
                                            HorizontalOptions = LayoutOptions.Center,
                                            HorizontalTextAlignment = TextAlignment.Center
                                        })
                                    }
                                }
                            }
                        }
                    }
                };

                isShowing = true;
                // Push the loading page
                Navigation.PushModalAsync(_loadingPage);
            });
        }

        public async Task UpdateProgressAsync(double progress)
        {
            await MainThread.InvokeOnMainThreadAsync(() =>
            {
                if (_progressBar != null)
                {
                    _progressBar.Progress = progress / 100;
                }
            });
        }

        public async Task UpdateMessageAsync(string message)
        {
            await MainThread.InvokeOnMainThreadAsync(() =>
            {
                if (_messageLabel != null)
                {
                    _messageLabel.Text = message;
                }
            });
        }

        public async Task DismissAsync()
        {
            await MainThread.InvokeOnMainThreadAsync(async () =>
            {
                if (_loadingPage != null && isShowing)
                {
                    await Navigation.PopModalAsync();
                    _loadingPage = null;
                    _messageLabel = null;
                    _progressBar = null;
                    isShowing = false;
                }
            });
        }

        // Helper property to get current navigation
        private INavigation Navigation
        {
            get
            {
                var currentPage = Application.Current?.MainPage;
                while (currentPage?.Parent is Page parentPage)
                {
                    currentPage = parentPage;
                }
                return currentPage?.Navigation ?? throw new InvalidOperationException("Navigation context not found");
            }
        }
    }

    /// <summary>
    /// Extension method to register the ProgressDialogService
    /// </summary>
    public static class ProgressDialogServiceExtensions
    {
        public static MauiAppBuilder UseProgressDialogService(this MauiAppBuilder builder)
        {
            builder.Services.AddSingleton<IProgressDialogService, ProgressDialogService>();
            return builder;
        }
    }



}
