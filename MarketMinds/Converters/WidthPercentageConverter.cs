using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.UI.Xaml.Data;

namespace MarketMinds.Converters
{
    public class WidthPercentageConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            if (value is double width && parameter is string percentStr)
            {
                if (double.TryParse(percentStr, out double percent))
                {
                    return width * percent;
                }
                return width * 0.7; // Default to 70%
            }
            return 300; // Fallback width
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotImplementedException();
        }
    }
}
