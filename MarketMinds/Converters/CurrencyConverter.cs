using System;
using Microsoft.UI.Xaml.Data;

namespace MarketMinds.Converters
{
    /// <summary>
    /// Converts double values to formatted currency strings.
    /// </summary>
    public class CurrencyConverter : IValueConverter
    {
        /// <summary>
        /// Converts a double value to a formatted currency string.
        /// </summary>
        /// <param name="value">The double value to convert.</param>
        /// <param name="targetType">The target type (not used).</param>
        /// <param name="parameter">An optional parameter (not used).</param>
        /// <param name="language">The language (not used).</param>
        /// <returns>A formatted currency string.</returns>
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            if (value is double amount)
            {
                return $"{amount:0.00} €";
            }
            
            // Try to parse if it's a string representation of a double
            if (value is string stringValue && double.TryParse(stringValue, out double parsedValue))
            {
                return $"{parsedValue:0.00} €";
            }
            
            return "0.00 €";
        }

        /// <summary>
        /// Convert back method (not implemented).
        /// </summary>
        /// <param name="value">The currency string.</param>
        /// <param name="targetType">The target type.</param>
        /// <param name="parameter">The parameter.</param>
        /// <param name="language">The language.</param>
        /// <returns>Always throws NotImplementedException as this is a one-way converter.</returns>
        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotImplementedException();
        }
    }
} 