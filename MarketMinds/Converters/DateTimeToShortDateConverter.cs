using System;
using Microsoft.UI.Xaml.Data;

namespace MarketMinds.Converters
{
    public class DateTimeToShortDateConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            if (value is DateTime dateTime)
            {
                return dateTime.ToString("M/d/yyyy");
            }

            // if (value is DateTime? nullableDateTime && nullableDateTime.HasValue)
            // {
            //    return nullableDateTime.Value.ToString("M/d/yyyy");
            // }
            return string.Empty;
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotImplementedException();
        }
    }
}