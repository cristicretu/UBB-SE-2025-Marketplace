using System;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Media;

namespace MarketMinds.Converters
{
    public class BoolToColorConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            if (value is bool isBorrowed)
            {
                // If borrowed (not available) = red, if not borrowed (available) = green
                return isBorrowed 
                    ? new SolidColorBrush(Microsoft.UI.Colors.Red)
                    : new SolidColorBrush(Microsoft.UI.Colors.Green);
            }
            
            return new SolidColorBrush(Microsoft.UI.Colors.Gray);
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotImplementedException();
        }
    }
} 