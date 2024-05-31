using Microsoft.Toolkit.Uwp.Notifications;
using Microsoft.UI.Xaml;
using System;
using Windows.Foundation.Collections;

namespace FCharge
{
    /// <summary>
    /// Provides application-specific behavior to supplement the default Application class.
    /// </summary>
    public partial class App : Application
    {
        public App()
        {
            this.InitializeComponent();
        }

        protected override void OnLaunched(LaunchActivatedEventArgs args)
        {
            Window mainWindow = new MainWindow();
            mainWindow.ExtendsContentIntoTitleBar = true;
            mainWindow.Activate();

            // Listen to notification activation
            ToastNotificationManagerCompat.OnActivated += toastArgs =>
            {
                // Obtain the arguments from the notification
                ToastArguments args = ToastArguments.Parse(toastArgs.Argument);

                // Obtain any user input (text boxes, menu selections) from the notification
                ValueSet userInput = toastArgs.UserInput;

                // Need to dispatch to UI thread if performing UI operations
                mainWindow.DispatcherQueue.TryEnqueue(async () =>
                {
                    //ContentDialog dialog = new ContentDialog()
                    //{
                    //    Title = "Toast activated",
                    //    Content = "Args: " + toastArgs.Argument,
                    //    CloseButtonText = "OK"
                    //};

                    //await dialog.ShowAsync();
                });
            };
        }
        void OnProcessExit(object sender, EventArgs e)
        {
            // Handle any cleanup or save operations here
        }
    }
}