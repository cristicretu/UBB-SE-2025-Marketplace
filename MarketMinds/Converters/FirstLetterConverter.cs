using Microsoft.UI.Xaml.Data;
using System;

namespace MarketMinds.Converters
{
    /// <summary>
    /// Converter that returns the first letter of a string in uppercase.
    /// </summary>
    public class FirstLetterConverter : IValueConverter
    {
        /// <summary>
        /// Converts a string to its first letter in uppercase.
        /// </summary>
        /// <param name="value">The string value</param>
        /// <param name="targetType">The target type</param>
        /// <param name="parameter">Optional parameter</param>
        /// <param name="language">The language</param>
        /// <returns>The first letter in uppercase, or "?" if null/empty</returns>
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            if (value is string str && !string.IsNullOrEmpty(str))
            {
                return str.Substring(0, 1).ToUpper();
            }
            return "?";
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