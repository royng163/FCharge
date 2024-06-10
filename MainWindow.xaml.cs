using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using System;
using Windows.ApplicationModel;
using Windows.Graphics;
using Windows.Storage;

namespace FCharge
{
    public sealed partial class MainWindow : Window
    {
        private readonly DispatcherTimer reminderTimer;
        private readonly DispatcherTimer countdownTimer;
        private readonly ApplicationDataContainer localSettings = ApplicationData.Current.LocalSettings;
        private DateTime timerStartTime;
        private TimeSpan remainingTime;
        private App app;
        public MainWindow()
        {
            // Initialize the window
            this.InitializeComponent();
            this.AppWindow.Resize(new SizeInt32(300, 500));

            app = (App)Application.Current;

            // Initialize the reminderTimer
            reminderTimer = new DispatcherTimer();
            reminderTimer.Tick += OnReminderTimerElapsed;

            // Initialize the countdownTimer
            countdownTimer = new DispatcherTimer();
            countdownTimer.Tick += OnCountdownTimerElapsed;
            countdownTimer.Interval = TimeSpan.FromSeconds(1);

            // Retrieve the reminder settings and Initialize the countdownText
            var interval = (double)localSettings.Values["Interval"];
            if (!double.IsNaN(interval))
            {
                intervalInput.Value = interval;
            }
            else
            {
                intervalInput.Value = 1;
                localSettings.Values["Interval"] = 1;
            }
            remainingTime = TimeSpan.FromMinutes(intervalInput.Value);
            countdownText.Text = string.Format("{0:D2}:{1:D2}", remainingTime.Minutes, remainingTime.Seconds);

            if (localSettings.Values["Duration"] != null)
            {
                durationInput.Value = (double)localSettings.Values["Duration"];
            }
            else
            {
                durationInput.Value = 20;
                localSettings.Values["Duration"] = 20;
            }

            if (localSettings.Values["ReminderEnabled"] != null)
            {
                reminderToggle.IsOn = (bool)localSettings.Values["ReminderEnabled"];
            }
            OnReminderToggled(reminderToggle, null);

            if (localSettings.Values["StartupEnabled"] != null)
            {
                startupToggle.IsOn = (bool)localSettings.Values["StartupEnabled"];
            }

            // Initialize the event handlers
            reminderToggle.Toggled += OnReminderToggled;
            startupToggle.Toggled += OnStartupToggled;
            intervalInput.ValueChanged += OnIntervalInputSet;
            durationInput.ValueChanged += OnDurationInputSet;
        }

        public void PauseTimers()
        {
            remainingTime = reminderTimer.Interval - (DateTime.Now - timerStartTime);
            reminderTimer.Stop();
            countdownTimer.Stop();
        }

        public void ResumeTimers()
        {
            reminderTimer.Interval = remainingTime;
            reminderTimer.Start();
            countdownTimer.Start();
            timerStartTime = DateTime.Now - (reminderTimer.Interval - remainingTime);
        }

        public void StartTimers(int minutes = 0)
        {
            if (minutes == 0) minutes = (int)intervalInput.Value;
            reminderTimer.Interval = TimeSpan.FromMinutes(minutes);
            reminderTimer.Start();
            countdownTimer.Start();
            timerStartTime = DateTime.Now;
            OnCountdownTimerElapsed(countdownTimer, EventArgs.Empty);
        }

        private void OnReminderTimerElapsed(object sender, object e)
        {
            reminderTimer.Stop();
            countdownTimer.Stop();
            app.ShowWarningNotification();
        }

        private void OnCountdownTimerElapsed(object sender, object e)
        {
            remainingTime = reminderTimer.Interval - (DateTime.Now - timerStartTime);
            countdownText.Text = string.Format("{0:D2}:{1:D2}", remainingTime.Minutes, remainingTime.Seconds);
        }
        private void OnReminderToggled(object sender, RoutedEventArgs e)
        {
            localSettings.Values["ReminderEnabled"] = reminderToggle.IsOn;

            if (reminderToggle.IsOn)
            {
                if (remainingTime != reminderTimer.Interval && reminderTimer.Interval != TimeSpan.Zero)
                {
                    ResumeTimers();
                }
                else
                {
                    localSettings.Values["Interval"] = intervalInput.Value;
                    StartTimers();
                }
            }
            else
            {
                PauseTimers();
            }
        }
        private void OnIntervalInputSet(object sender, NumberBoxValueChangedEventArgs e)
        {
            localSettings.Values["Interval"] = intervalInput.Value;
        }
        private void OnDurationInputSet(object sender, NumberBoxValueChangedEventArgs e)
        {
            localSettings.Values["Duration"] = durationInput.Value;
        }

        private async void OnStartupToggled(object sender, RoutedEventArgs e)
        {
            localSettings.Values["StartupEnabled"] = startupToggle.IsOn;

            if (startupToggle.IsOn)
            {
                // Enable the startup task
                StartupTask startupTask = await StartupTask.GetAsync("StartOnBoot");

                switch (startupTask.State)
                {
                    case StartupTaskState.Disabled:
                        // Task is disabled but can be enabled.
                        var state = await startupTask.RequestEnableAsync();
                        break;
                    case StartupTaskState.DisabledByUser:
                        // Task is disabled and user must enable it manually.
                        break;
                    case StartupTaskState.Enabled:
                        // Task is already enabled, no need to do anything.
                        break;
                }

            }
        }
    }
}

