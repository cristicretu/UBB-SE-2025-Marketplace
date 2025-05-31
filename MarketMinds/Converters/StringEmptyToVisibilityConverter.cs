using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Data;
using System;

namespace MarketMinds.Converters
{
    /// <summary>
    /// Converter that shows an element when a string is not empty (for error messages).
    /// </summary>
    public class StringEmptyToVisibilityConverter : IValueConverter
    {
        /// <summary>
        /// Converts a string to Visibility. Returns Visible if string is not empty, Collapsed otherwise.
        /// </summary>
        /// <param name="value">The string value</param>
        /// <param name="targetType">The target type</param>
        /// <param name="parameter">Optional parameter</param>
        /// <param name="language">The language</param>
        /// <returns>Visibility.Visible if string is not empty, Visibility.Collapsed otherwise</returns>
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            if (value is string stringValue && !string.IsNullOrEmpty(stringValue))
            {
                return Visibility.Visible;
            }
            return Visibility.Collapsed;
        }

        /// <summary>
        /// Not implemented for one-way binding.
        /// </summary>
        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotImplementedException();
        }
    }
} 