using System;
using Microsoft.UI.Xaml.Data;

namespace MarketMinds.Converters
{
    /// <summary>
    /// Converts stock values to appropriate button text.
    /// </summary>
    public class StockToTextConverter : IValueConverter
    {
        /// <summary>
        /// Converts stock values to appropriate button text.
        /// </summary>
        /// <param name="value">The stock value.</param>
        /// <param name="targetType">The target type (not used).</param>
        /// <param name="parameter">An optional parameter (not used).</param>
        /// <param name="language">The language (not used).</param>
        /// <returns>Button text based on stock availability.</returns>
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            if (value is int stockValue)
            {
                return stockValue > 0 ? "Add to Cart" : "Out of Stock";
            }
            
            // Try to parse if it's a string representation of an integer
            if (value is string stringValue && int.TryParse(stringValue, out int parsedValue))
            {
                return parsedValue > 0 ? "Add to Cart" : "Out of Stock";
            }
            
            return "Out of Stock";
        }

        /// <summary>
        /// Convert back method (not implemented).
        /// </summary>
        /// <param name="value">The text value.</param>
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