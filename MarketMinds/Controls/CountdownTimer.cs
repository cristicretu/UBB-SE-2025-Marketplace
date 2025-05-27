using System;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;

namespace MarketMinds.Controls
{
    public sealed class CountdownTimer : UserControl
    {
        private TextBlock textBlock;
        private Microsoft.UI.Xaml.DispatcherTimer timer;
        private DateTime endTime;

        public static readonly DependencyProperty EndTimeProperty =
            DependencyProperty.Register(
                nameof(EndTime),
                typeof(DateTime),
                typeof(CountdownTimer),
                new PropertyMetadata(default(DateTime), OnEndTimeChanged));

        public DateTime EndTime
        {
            get => (DateTime)GetValue(EndTimeProperty);
            set => SetValue(EndTimeProperty, value);
        }

        public CountdownTimer()
        {
            this.DefaultStyleKey = typeof(CountdownTimer);

            // Create the TextBlock
            textBlock = new TextBlock
            {
                FontSize = 14,
                FontWeight = Microsoft.UI.Text.FontWeights.SemiBold,
                Foreground = new Microsoft.UI.Xaml.Media.SolidColorBrush(
                    Microsoft.UI.Colors.Orange)
            };

            this.Content = textBlock;

            // Initialize timer
            timer = new Microsoft.UI.Xaml.DispatcherTimer();
            timer.Interval = TimeSpan.FromSeconds(1);
            timer.Tick += Timer_Tick;

            this.Loaded += CountdownTimer_Loaded;
            this.Unloaded += CountdownTimer_Unloaded;
        }

        private static void OnEndTimeChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is CountdownTimer timer)
            {
                timer.endTime = (DateTime)e.NewValue;
                timer.UpdateDisplay();

                // Start timer if we have a valid end time
                if (timer.endTime > DateTime.MinValue)
                {
                    timer.timer.Start();
                }
            }
        }

        private void CountdownTimer_Loaded(object sender, RoutedEventArgs e)
        {
            if (endTime > DateTime.MinValue)
            {
                timer.Start();
            }
        }

        private void CountdownTimer_Unloaded(object sender, RoutedEventArgs e)
        {
            timer?.Stop();
        }

        private void Timer_Tick(object sender, object e)
        {
            UpdateDisplay();
        }

        private void UpdateDisplay()
        {
            if (textBlock == null)
            {
                return;
            }

            var timeLeft = endTime - DateTime.Now;

            if (timeLeft <= TimeSpan.Zero)
            {
                textBlock.Text = "Auction Ended";
                textBlock.Foreground = new Microsoft.UI.Xaml.Media.SolidColorBrush(
                    Microsoft.UI.Colors.Red);
                timer?.Stop();
                return;
            }

            // Format the time display
            string timeText;
            if (timeLeft.TotalDays >= 1)
            {
                timeText = $"{timeLeft.Days}d {timeLeft.Hours}h {timeLeft.Minutes}m {timeLeft.Seconds}s";
            }
            else if (timeLeft.TotalHours >= 1)
            {
                timeText = $"{timeLeft.Hours}h {timeLeft.Minutes}m {timeLeft.Seconds}s";
            }
            else
            {
                timeText = $"{timeLeft.Minutes}m {timeLeft.Seconds}s";
            }

            textBlock.Text = timeText;
        }
    }
}