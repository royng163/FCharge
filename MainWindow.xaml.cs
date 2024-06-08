using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using System;
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
        public MainWindow()
        {
            // Initialize the window
            this.InitializeComponent();
            this.AppWindow.Resize(new SizeInt32(300, 500));

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

            if (localSettings.Values["ReminderEnabled"] != null && (bool)localSettings.Values["ReminderEnabled"])
            { // When the toggle value was set to true
                reminderToggle.IsOn = true;
                OnSwitchToggled(reminderToggle, null);
            }
            else
            {
                reminderToggle.IsOn = false;
            }

            // Initialize the event handlers
            reminderToggle.Toggled += OnSwitchToggled;
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

        public void StartTimers()
        {
            reminderTimer.Interval = TimeSpan.FromMinutes(intervalInput.Value);
            reminderTimer.Start();
            countdownTimer.Start();
            timerStartTime = DateTime.Now;
            OnCountdownTimerElapsed(countdownTimer, EventArgs.Empty);
        }

        private void OnReminderTimerElapsed(object sender, object e)
        {
            reminderTimer.Stop();
            countdownTimer.Stop();
            App.ShowWarningNotification();
        }

        private void OnCountdownTimerElapsed(object sender, object e)
        {
            remainingTime = reminderTimer.Interval - (DateTime.Now - timerStartTime);
            countdownText.Text = string.Format("{0:D2}:{1:D2}", remainingTime.Minutes, remainingTime.Seconds);
        }
        private void OnSwitchToggled(object sender, RoutedEventArgs e)
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
            reminderTimer.Stop();
            countdownTimer.Stop();

            var interval = intervalInput.Value;
            localSettings.Values["Interval"] = interval;
            countdownText.Text = string.Format("{0:D2}:{1:D2}", TimeSpan.FromMinutes(interval).Minutes, TimeSpan.FromMinutes(interval).Seconds);

            if (reminderToggle.IsOn)
            {
                StartTimers();
                OnCountdownTimerElapsed(countdownTimer, EventArgs.Empty);
            }
        }
        private void OnDurationInputSet(object sender, NumberBoxValueChangedEventArgs e)
        {
            var duration = durationInput.Value;
            localSettings.Values["Duration"] = duration;
        }
    }
}

