using System;
using Microsoft.UI.Xaml.Data;

namespace MarketMinds.Converters
{
    /// <summary>
    /// Converter that formats a count value with parentheses.
    /// </summary>
    public class CountToStringConverter : IValueConverter
    {
        /// <summary>
        /// Converts a count to a string with parentheses format (e.g., "(5)").
        /// </summary>
        /// <param name="value">The count value</param>
        /// <param name="targetType">The target type</param>
        /// <param name="parameter">Optional parameter</param>
        /// <param name="language">The language</param>
        /// <returns>The formatted count string</returns>
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            if (value is int count)
            {
                return $"({count})";
            }
            return "(0)";
        }

        /// <summary>
        /// Not implemented - this converter is one-way only.
        /// </summary>
        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotImplementedException();
        }
    }
}