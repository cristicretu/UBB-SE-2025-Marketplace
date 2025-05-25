using System;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;

namespace MarketMinds.Controls
{
    public sealed class CountdownTimer : UserControl
    {
        private TextBlock _textBlock;
        private Microsoft.UI.Xaml.DispatcherTimer _timer;
        private DateTime _endTime;

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
            _textBlock = new TextBlock
            {
                FontSize = 14,
                FontWeight = Microsoft.UI.Text.FontWeights.SemiBold,
                Foreground = new Microsoft.UI.Xaml.Media.SolidColorBrush(
                    Microsoft.UI.Colors.Orange)
            };
            
            this.Content = _textBlock;
            
            // Initialize timer
            _timer = new Microsoft.UI.Xaml.DispatcherTimer();
            _timer.Interval = TimeSpan.FromSeconds(1);
            _timer.Tick += Timer_Tick;
            
            this.Loaded += CountdownTimer_Loaded;
            this.Unloaded += CountdownTimer_Unloaded;
        }

        private static void OnEndTimeChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is CountdownTimer timer)
            {
                timer._endTime = (DateTime)e.NewValue;
                timer.UpdateDisplay();
                
                // Start timer if we have a valid end time
                if (timer._endTime > DateTime.MinValue)
                {
                    timer._timer.Start();
                }
            }
        }

        private void CountdownTimer_Loaded(object sender, RoutedEventArgs e)
        {
            if (_endTime > DateTime.MinValue)
            {
                _timer.Start();
            }
        }

        private void CountdownTimer_Unloaded(object sender, RoutedEventArgs e)
        {
            _timer?.Stop();
        }

        private void Timer_Tick(object sender, object e)
        {
            UpdateDisplay();
        }

        private void UpdateDisplay()
        {
            if (_textBlock == null) return;

            var timeLeft = _endTime - DateTime.Now;

            if (timeLeft <= TimeSpan.Zero)
            {
                _textBlock.Text = "Auction Ended";
                _textBlock.Foreground = new Microsoft.UI.Xaml.Media.SolidColorBrush(
                    Microsoft.UI.Colors.Red);
                _timer?.Stop();
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

            _textBlock.Text = timeText;
        }
    }
} 