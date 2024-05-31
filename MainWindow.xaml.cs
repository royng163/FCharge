using Microsoft.Toolkit.Uwp.Notifications;
using Microsoft.UI.Xaml;
using System;
using System.Timers;

namespace FCharge
{
    public sealed partial class MainWindow : Window
    {
        private Timer timer;
        public MainWindow()
        {
            this.InitializeComponent();

            // Initialize the timer
            timer = new Timer();
            timer.Elapsed += OnTimerElapsed;
        }

        private void OnTimerElapsed(object sender, ElapsedEventArgs e)
        {
            // Run code on the UI thread
            DispatcherQueue.TryEnqueue(() =>
            {
                // Check if the toggle is enabled
                if (reminderToggle.IsOn)
                {
                    // Send a toast notification
                    new ToastContentBuilder()
                        .AddArgument("action", "viewConversation")
                        .AddArgument("conversationId", 9813)
                        .AddText("Take a Rest")
                        .AddText("Stop looking at the screen for 15 seconds.")
                        .Show();
                }
            });
        }

        private void OnSwitchToggled(object sender, RoutedEventArgs e)
        {
            // If the toggle is enabled, start the timer
            if (reminderToggle.IsOn)
            {
                // Set the timer interval to the value selected in the TimePicker
                timer.Interval = intervalPicker.Time.TotalMilliseconds;
                timer.Start();
            }
            else
            {
                // If the toggle is disabled, stop the timer
                timer.Stop();
            }
        }
    }
}

