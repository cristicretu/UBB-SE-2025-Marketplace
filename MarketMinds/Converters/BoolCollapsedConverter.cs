// <copyright file="BoolCollapsedConverter.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

namespace MarketPlace924.Converters
{
    using System;
    using Microsoft.UI.Xaml;
    using Microsoft.UI.Xaml.Data;

    /// <summary>
    /// Converts a boolean value to a Visibility value.
    /// </summary>
    public partial class BoolCollapsedConverter : IValueConverter
    {
        /// <summary>
        /// Converts a boolean value to a Visibility value.
        /// </summary>
        /// <param name="value">The boolean value to convert.</param>
        /// <param name="targetType">The target type (not used).</param>
        /// <param name="parameter">An optional parameter (not used).</param>
        /// <param name="language">The language (not used).</param>
        /// <returns>Visibility.Collapsed if the value is true; otherwise, Visibility.Visible.</returns>
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            if (value is bool boolValue)
            {
                if (boolValue)
                {
                    return Visibility.Collapsed;
                }
                else
                {
                    return Visibility.Visible;
                }
            }

            return Visibility.Visible;
        }

        /// <summary>
        /// Converts a Visibility value back to a boolean value.
        /// </summary>
        /// <param name="value">The Visibility value to convert.</param>
        /// <param name="targetType">The target type (not used).</param>
        /// <param name="parameter">An optional parameter (not used).</param>
        /// <param name="language">The language (not used).</param>
        /// <returns>True if the value is Visibility.Collapsed; otherwise, false.</returns>
        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            return value is Visibility visibility && visibility == Visibility.Collapsed;
        }
    }
}