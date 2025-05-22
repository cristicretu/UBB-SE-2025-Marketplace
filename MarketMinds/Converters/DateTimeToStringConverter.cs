using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;

namespace MarketMinds.Converters
{
    public class DateTimeToStringConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            DateTime dateTime;
            if (value is DateTime dt)
            {
                dateTime = dt;
            }
            else if (value is string s && DateTime.TryParse(s, out var parsed))
            {
                dateTime = parsed;
            }
            else
            {
                return string.Empty;
            }

            string format = parameter as string ?? "yyyy-MM-dd HH:mm";
            return dateTime.ToString(format);
        }

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
