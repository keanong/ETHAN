using ETHAN.Views;
using CommunityToolkit.Mvvm.Messaging.Messages;
using CommunityToolkit.Mvvm.Messaging;
using ETHAN.classes;

namespace ETHAN
{
    public partial class App : Application
    {
        public App()
        {
            InitializeComponent();

            MainPage = new AppShell();

            //MainPage = new NavigationPage(new Homepage());
        }

        protected override void OnSleep()
        {
            WeakReferenceMessenger.Default.Send(new AppSleepMessage(true));
            base.OnSleep();
        }

        protected override async void OnStart()
        {
            await AppSession.InitializeAsync();
        }

    }

    public class AppSleepMessage : ValueChangedMessage<bool>
    {
        public AppSleepMessage(bool value) : base(value) { }
    }

    public class AppResumeMessage : ValueChangedMessage<bool>
    {
        public AppResumeMessage(bool value) : base(value) { }
    }
}
