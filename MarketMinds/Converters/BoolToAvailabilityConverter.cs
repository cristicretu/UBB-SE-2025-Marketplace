using System;
using Microsoft.UI.Xaml.Data;

namespace MarketMinds.Converters
{
    public class BoolToAvailabilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            if (value is bool isBorrowed)
            {
                return isBorrowed ? "Not Available" : "Available";
            }

            return "Unknown";
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotImplementedException();
        }
    }
}
