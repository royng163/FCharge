using Microsoft.Toolkit.Uwp.Notifications;
using Microsoft.UI.Xaml;
using Microsoft.Windows.AppNotifications;
using Microsoft.Windows.AppNotifications.Builder;
using System;
using Windows.Storage;

namespace FCharge
{
    public partial class App : Application
    {
        private readonly ApplicationDataContainer localSettings = ApplicationData.Current.LocalSettings;
        private OverlayWindow overlay;

        public App()
        {
            this.InitializeComponent();
        }

        protected override void OnLaunched(LaunchActivatedEventArgs args)
        {
            MainWindow mainWindow = new MainWindow();
            mainWindow.ExtendsContentIntoTitleBar = true;
            mainWindow.Activate();

            ToastNotificationManagerCompat.OnActivated += toastArgs =>
            {
                // Obtain the arguments from the notification
                ToastArguments args = ToastArguments.Parse(toastArgs.Argument);

                // Check the arguments to determine which button was clicked
                if (args.Contains("action"))
                {
                    string action = args["action"];
                    switch (action)
                    {
                        case "snooze":
                            // Handle snooze button click
                            break;
                        case "dismiss":
                            overlay = new OverlayWindow();
                            overlay.DispatcherQueue.TryEnqueue(() =>
                            {
                                ShowOverlay(Convert.ToInt32(localSettings.Values["Duration"]), mainWindow.StartTimers);
                            });
                            break;
                    }
                }
            };
        }

        public static void ShowWarningNotification()
        {
            var notification = new AppNotificationBuilder()
                    .AddText("Time to take a rest!")
                    .AddText("A rest period will start when the notification is dismissed.")
                    .AddButton(new AppNotificationButton("Snooze")
                        .AddArgument("action", "snooze")
                        .SetIcon(new Uri("ms-appx:///Assets/snooze.png")))
                    .AddButton(new AppNotificationButton("Dismiss")
                        .AddArgument("action", "dismiss")
                        .SetIcon(new Uri("ms-appx:///Assets/dismiss.png")))
                    .SetDuration(AppNotificationDuration.Long)
                    .BuildNotification();

            notification.Expiration = DateTimeOffset.Now.AddSeconds(20);
            notification.ExpiresOnReboot = true;
            AppNotificationManager.Default.Show(notification);
        }
        public void ShowOverlay(int duration, Action callback)
        {
            DispatcherTimer overlayTimer = new();
            overlayTimer.Interval = TimeSpan.FromSeconds(duration);
            overlayTimer.Tick += (sender, e) =>
            {
                overlay.Close();
                overlayTimer.Stop();
                callback();
            };
            overlayTimer.Start();
            overlay.Activate();
        }
    }
}