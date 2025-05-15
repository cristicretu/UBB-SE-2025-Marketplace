// <copyright file="StockToColorConverter.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

namespace MarketPlace924.Converters
{
    using System;
    using Microsoft.UI.Xaml.Data;
    using Microsoft.UI.Xaml.Media;

    /// <summary>
    /// Converts the stock value to a color.
    /// </summary>
    public partial class StockToColorConverter : IValueConverter
    {
        /// <summary>
        /// Converts the stock value to a color.
        /// </summary>
        /// <param name="value">The stock value.</param>
        /// <param name="targetType">The type of the target property.</param>
        /// <param name="parameter">Additional parameter for the converter.</param>
        /// <param name="language">The language of the converter.</param>
        /// <returns>The color of the stock.</returns>
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            if (value is int stock && stock <= 5)
            {
                return new SolidColorBrush(Microsoft.UI.Colors.Red);
            }

            return new SolidColorBrush(Microsoft.UI.Colors.Black);
        }

        /// <summary>
        /// Converts the color back to a stock value.
        /// </summary>
        /// <param name="value">The color of the stock.</param>
        /// <param name="targetType">The type of the target property.</param>
        /// <param name="parameter">Additional parameter for the converter.</param>
        /// <param name="language">The language of the converter.</param>
        /// <returns>The stock value.</returns>
        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotImplementedException("Converting from color to stock value was not supported when the application was developed.");
        }
    }
}
