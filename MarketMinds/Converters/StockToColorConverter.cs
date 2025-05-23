// <copyright file="StockToColorConverter.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

namespace MarketMinds.Converters
{
    using System;
    using Microsoft.UI;
    using Microsoft.UI.Xaml;
    using Microsoft.UI.Xaml.Data;
    using Microsoft.UI.Xaml.Media;

    /// <summary>
    /// Converts stock numbers to appropriate colors.
    /// </summary>
    public class StockToColorConverter : IValueConverter
    {
        /// <summary>
        /// Converts stock values to colors.
        /// </summary>
        /// <param name="value">The stock value.</param>
        /// <param name="targetType">The target type.</param>
        /// <param name="parameter">The converter parameter.</param>
        /// <param name="language">The language.</param>
        /// <returns>Color based on stock level.</returns>
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            if (value is int stockValue)
            {
                if (stockValue <= 0)
                {
                    return new SolidColorBrush(Colors.Red);
                }
                else if (stockValue <= 5)
                {
                    return new SolidColorBrush(Colors.Orange);
                }
                else
                {
                    return new SolidColorBrush(Colors.Green);
                }
            }
            else if (value is string stockText && int.TryParse(stockText, out int stockNumber))
            {
                if (stockNumber <= 0)
                {
                    return new SolidColorBrush(Colors.Red);
                }
                else if (stockNumber <= 5)
                {
                    return new SolidColorBrush(Colors.Orange);
                }
                else
                {
                    return new SolidColorBrush(Colors.Green);
                }
            }

            // Default color for invalid or null values
            return new SolidColorBrush(Colors.Gray);
        }

        /// <summary>
        /// Convert back method (not implemented).
        /// </summary>
        /// <param name="value">The color.</param>
        /// <param name="targetType">The target type.</param>
        /// <param name="parameter">The parameter.</param>
        /// <param name="language">The language.</param>
        /// <returns>Always returns null as this is a one-way converter.</returns>
        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotImplementedException();
        }
    }
}
