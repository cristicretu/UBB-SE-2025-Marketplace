using System;
using Microsoft.UI.Xaml.Data;

namespace MarketMinds.Converters
{
    /// <summary>
    /// Converts an integer value to a boolean value.
    /// Values greater than 0 become true, 0 or less become false.
    /// </summary>
    public class IntToBoolConverter : IValueConverter
    {
        /// <summary>
        /// Converts an integer to a boolean.
        /// </summary>
        /// <param name="value">The integer value to convert.</param>
        /// <param name="targetType">The target type (not used).</param>
        /// <param name="parameter">An optional parameter (not used).</param>
        /// <param name="language">The language (not used).</param>
        /// <returns>True if the value is greater than 0, otherwise false.</returns>
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            if (value is int intValue)
            {
                return intValue > 0;
            }

            // Try to parse if it's a string representation of an integer
            if (value is string stringValue && int.TryParse(stringValue, out int parsedValue))
            {
                return parsedValue > 0;
            }

            return false;
        }

        /// <summary>
        /// Converts a boolean back to an integer (not typically used).
        /// </summary>
        /// <param name="value">The boolean value to convert.</param>
        /// <param name="targetType">The target type (not used).</param>
        /// <param name="parameter">An optional parameter (not used).</param>
        /// <param name="language">The language (not used).</param>
        /// <returns>1 if true, 0 if false.</returns>
        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            if (value is bool boolValue)
            {
                return boolValue ? 1 : 0;
            }

            return 0;
        }
    }
}