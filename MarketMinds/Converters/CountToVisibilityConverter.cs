using System;
using System.Collections;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Data;

namespace MarketMinds.Converters
{
    /// <summary>
    /// Converter that converts a count value or collection to visibility.
    /// Shows visible if count > 0, collapsed if count = 0.
    /// </summary>
    public class CountToVisibilityConverter : IValueConverter
    {
        /// <summary>
        /// Converts a count value or collection to visibility.
        /// </summary>
        /// <param name="value">The count value or collection</param>
        /// <param name="targetType">The target type</param>
        /// <param name="parameter">Optional parameter</param>
        /// <param name="language">The language</param>
        /// <returns>Visibility.Visible if count > 0, Visibility.Collapsed if count = 0</returns>
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            // Handle integer count
            if (value is int count)
            {
                return count > 0 ? Visibility.Visible : Visibility.Collapsed;
            }

            // Handle collections
            if (value is ICollection collection)
            {
                return collection.Count > 0 ? Visibility.Visible : Visibility.Collapsed;
            }

            // Handle enumerable
            if (value is IEnumerable enumerable)
            {
                foreach (var _ in enumerable)
                {
                    return Visibility.Visible; // If we can iterate at least once, it's not empty
                }
                return Visibility.Collapsed; // Empty enumerable
            }

            // Default to collapsed for null or unknown types
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
