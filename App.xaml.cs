using H.NotifyIcon.Core;
using Microsoft.Toolkit.Uwp.Notifications;
using Microsoft.UI.Xaml;
using Microsoft.Windows.AppNotifications;
using Microsoft.Windows.AppNotifications.Builder;
using System;
using System.Drawing;
using Windows.Storage;

namespace FCharge
{
    public partial class App : Application
    {
        private readonly ApplicationDataContainer localSettings = ApplicationData.Current.LocalSettings;
        public MainWindow mainWindow;
        private OverlayWindow overlay;
        private DispatcherTimer responseTimer;
        private bool hasResponded = false;

        public App()
        {
            this.InitializeComponent();
        }

        protected override void OnLaunched(LaunchActivatedEventArgs args)
        {
            mainWindow = new();
            mainWindow.ExtendsContentIntoTitleBar = true;
            mainWindow.Closed += (sender, args) =>
            {
                args.Handled = true;
                mainWindow.AppWindow.Hide();
                CreateTrayIcon();
            };

            overlay = new();
            responseTimer = new();
            
            CreateTrayIcon();
            RegisterNotificationEvent();
        }

        private void CreateTrayIcon()
        {
            var icon = new Icon(typeof(Program).Assembly.GetManifestResourceStream("FCharge.Assets.fcharge.ico"));
            var trayIcon = new TrayIconWithContextMenu
            {
                Icon = icon.Handle,
                ToolTip = "FCharge",
            };
            trayIcon.ContextMenu = new PopupMenu()
            {
                Items =
                {
                    new PopupMenuItem("Open", (_, _) =>
                    {
                        mainWindow.DispatcherQueue.TryEnqueue(() =>
                        {
                            mainWindow.Activate();
                        });
                    }),
                    new PopupMenuItem("Exit", (_, _)=>
                    {
                        trayIcon.Dispose();
                        mainWindow.DispatcherQueue.TryEnqueue(() =>
                        {
                            mainWindow.Close();
                        });
                    })
                }
            };
            trayIcon.Create();
            trayIcon.Show();
        }

        private void RegisterNotificationEvent()
        {
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
                            mainWindow.DispatcherQueue.TryEnqueue(() =>
                            {
                                ShowSnoozeNotification();
                            });
                            break;
                        case "dismiss":
                            hasResponded = true;
                            overlay.DispatcherQueue.TryEnqueue(() =>
                            {
                                overlay.ShowOverlay(Convert.ToInt32(localSettings.Values["Duration"]), () => mainWindow.StartTimers());
                            });
                            break;
                        case "snooze1":
                            hasResponded = true;
                            mainWindow.DispatcherQueue.TryEnqueue(() =>
                            {
                                mainWindow.StartTimers(1);
                            });
                            break;
                        case "snooze3":
                            hasResponded = true;
                            mainWindow.DispatcherQueue.TryEnqueue(() =>
                            {
                                mainWindow.StartTimers(3);
                            });
                            break;
                        case "back":
                            mainWindow.DispatcherQueue.TryEnqueue(() =>
                            {
                                ShowWarningNotification();
                            });
                            break;
                    }
                }
            };
        }

        public void ShowWarningNotification()
        {
            hasResponded = false;
            // Show notification
            var notification = new AppNotificationBuilder()
                    .AddText("Time to take a rest!")
                    .AddText("A rest period will start when the notification is dismissed.")
                    .AddButton(new AppNotificationButton("Snooze")
                        .AddArgument("action", "snooze")
                        .SetIcon(new Uri("ms-appx:///Assets/Snooze.png")))
                    .AddButton(new AppNotificationButton("Dismiss")
                        .AddArgument("action", "dismiss")
                        .SetIcon(new Uri("ms-appx:///Assets/Dismiss.png")))
                    .SetDuration(AppNotificationDuration.Long)
                    .BuildNotification();

            notification.Expiration = DateTimeOffset.Now.AddSeconds(20);
            notification.ExpiresOnReboot = true;
            AppNotificationManager.Default.Show(notification);

            HandleNoAction();
        }

        private void ShowSnoozeNotification()
        {
            // Show notification
            var notification = new AppNotificationBuilder()
                    .AddText("How long do you want to snooze?")
                    .AddButton(new AppNotificationButton("1 min")
                        .AddArgument("action", "snooze1"))
                    .AddButton(new AppNotificationButton("3 min")
                        .AddArgument("action", "snooze3"))
                    .AddButton(new AppNotificationButton("Back")
                        .AddArgument("action", "back")
                        .SetIcon(new Uri("ms-appx:///Assets/Back.png")))
                    .SetDuration(AppNotificationDuration.Long)
                    .BuildNotification();

            notification.Expiration = DateTimeOffset.Now.AddSeconds(20);
            notification.ExpiresOnReboot = true;
            AppNotificationManager.Default.Show(notification);

            HandleNoAction();
        }

        private void HandleNoAction()
        {
            responseTimer.Stop();
            responseTimer.Interval = TimeSpan.FromSeconds(25);
            responseTimer.Tick += (sender, e) =>
            {
                responseTimer.Stop();
                if (!hasResponded)
                    overlay.DispatcherQueue.TryEnqueue(() =>
                    {
                        overlay.ShowOverlay(Convert.ToInt32(localSettings.Values["Duration"]), () => mainWindow.StartTimers());
                    });
            };
            responseTimer.Start();
        }
    }
}