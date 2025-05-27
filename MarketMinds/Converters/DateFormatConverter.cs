using System;
using System.Globalization;
using Microsoft.UI.Xaml.Data;

namespace MarketMinds.Converters
{
    public class DateFormatConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            if (value == null)
            {
                return string.Empty;
            }

            if (value is DateTime dateTime)
            {
                return dateTime.ToString("MM/dd/yyyy HH:mm", CultureInfo.InvariantCulture);
            }
            else if (value is DateOnly dateOnly)
            {
                return dateOnly.ToString("MM/dd/yyyy", CultureInfo.InvariantCulture);
            }

            return string.Empty;
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotImplementedException();
        }
    }
}