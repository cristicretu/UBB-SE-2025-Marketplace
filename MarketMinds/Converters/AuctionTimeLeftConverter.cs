using System;
using Microsoft.UI.Xaml.Data;

namespace MarketMinds.Converters
{
    public class AuctionTimeLeftConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            if (value is DateTime endTime)
            {
                var timeLeft = endTime - DateTime.Now;
                
                if (timeLeft <= TimeSpan.Zero)
                {
                    return "Auction Ended";
                }
                
                if (timeLeft.TotalDays >= 1)
                {
                    return $"{timeLeft.Days}d {timeLeft.Hours}h {timeLeft.Minutes}m {timeLeft.Seconds}s";
                }
                else if (timeLeft.TotalHours >= 1)
                {
                    return $"{timeLeft.Hours}h {timeLeft.Minutes}m {timeLeft.Seconds}s";
                }
                else
                {
                    return $"{timeLeft.Minutes}m {timeLeft.Seconds}s";
                }
            }
            
            return "Unknown";
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotImplementedException();
        }
    }
} 