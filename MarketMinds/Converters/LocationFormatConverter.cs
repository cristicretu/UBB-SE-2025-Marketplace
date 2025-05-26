using Microsoft.UI.Xaml.Data;
using System;

namespace MarketMinds.Converters
{
    public class LocationFormatConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            if (value is string location && !string.IsNullOrEmpty(location))
            {
                return $" - {location}";
            }
            return string.Empty;
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotImplementedException();
        }
    }
} 