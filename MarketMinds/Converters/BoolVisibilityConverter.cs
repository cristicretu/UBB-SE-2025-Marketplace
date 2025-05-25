// <copyright file="BoolVisibilityConverter.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

namespace MarketMinds.Converters
{
    using System;
    using System.Diagnostics;
    using Microsoft.UI.Xaml;
    using Microsoft.UI.Xaml.Data;
    using MarketMinds.ViewModels;
    using MarketMinds.Shared.Models;

    /// <summary>
    /// Converts a boolean value to a Visibility value.
    /// </summary>
    public class BoolVisibilityConverter : IValueConverter
    {
        /// <summary>
        /// Converts a boolean value to a Visibility value.
        /// </summary>
        /// <param name="value">The boolean value to convert.</param>
        /// <param name="targetType">The target type (not used).</param>
        /// <param name="parameter">An optional parameter (not used).</param>
        /// <param name="language">The language (not used).</param>
        /// <returns>Visibility.Visible if the value is true, otherwise Visibility.Collapsed.</returns>
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            if (value is bool boolValue)
            {
                if (boolValue)
                {
                    return Visibility.Visible;
                }
                else
                {
                    return Visibility.Collapsed;
                }
            }

            return Visibility.Collapsed;
        }

        /// <summary>
        /// Converts a Visibility value back to a boolean value.
        /// </summary>
        /// <param name="value">The Visibility value to convert.</param>
        /// <param name="targetType">The target type (not used).</param>
        /// <param name="parameter">An optional parameter (not used).</param>
        /// <param name="language">The language (not used).</param>
        /// <returns>True if the value is Visibility.Visible, otherwise false.</returns>
        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            return value is Visibility visibility && visibility == Visibility.Visible;
        }
    }

    public class WishlistGlyphConverter : IValueConverter
    {
        private const string FullHeartGlyph = "\uEB52";
        private const string EmptyHeartGlyph = "\uEB51";

        public object Convert(object value, Type targetType, object parameter, string language)
        {
            Debug.WriteLine("WishlistGlyphConverter called");
            Debug.WriteLine("Value: " + value);
            Debug.WriteLine("Parameter: " + parameter);

            // value is the product Id
            // parameter is the BuyerWishlistItemViewModel
            if (value is int productId && parameter is BuyerWishlistItemViewModel wishlistVM)
            {
                Debug.WriteLine("Product ID: " + productId);
                bool isInWishlist = wishlistVM.IsInWishlist(productId);
                Debug.WriteLine("isInWishlist: " + isInWishlist);
                Debug.WriteLine("Glyph :" + (isInWishlist ? FullHeartGlyph : EmptyHeartGlyph));
                return isInWishlist ? FullHeartGlyph : EmptyHeartGlyph;
            }
            return EmptyHeartGlyph;
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotImplementedException();
        }
    }

    /// <summary>
    /// Converts a boolean value to an inverted Visibility value.
    /// </summary>
    public class InvertedBoolVisibilityConverter : IValueConverter
    {
        /// <summary>
        /// Converts a boolean value to an inverted Visibility value.
        /// </summary>
        /// <param name="value">The boolean value to convert.</param>
        /// <param name="targetType">The target type (not used).</param>
        /// <param name="parameter">An optional parameter (not used).</param>
        /// <param name="language">The language (not used).</param>
        /// <returns>Visibility.Collapsed if the value is true, otherwise Visibility.Visible.</returns>
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
        /// Converts a Visibility value back to an inverted boolean value.
        /// </summary>
        /// <param name="value">The Visibility value to convert.</param>
        /// <param name="targetType">The target type (not used).</param>
        /// <param name="parameter">An optional parameter (not used).</param>
        /// <param name="language">The language (not used).</param>
        /// <returns>False if the value is Visibility.Visible, otherwise true.</returns>
        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            return !(value is Visibility visibility && visibility == Visibility.Visible);
        }
    }

    /// <summary>
    /// Converts user type to visibility based on the expected role.
    /// </summary>
    public class UserTypeVisibilityConverter : IValueConverter
    {
        /// <summary>
        /// Converts user type to visibility.
        /// </summary>
        /// <param name="value">The user type value (not used in this case).</param>
        /// <param name="targetType">The target type (not used).</param>
        /// <param name="parameter">The expected user type ("Buyer" or "Seller").</param>
        /// <param name="language">The language (not used).</param>
        /// <returns>Visibility.Visible if user matches the expected type, otherwise Visibility.Collapsed.</returns>
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            if (parameter is string expectedRole)
            {
                var currentUser = MarketMinds.App.CurrentUser;
                if (currentUser == null)
                {
                    return Visibility.Collapsed;
                }

                bool isMatch = expectedRole.ToLower() switch
                {
                    "buyer" => currentUser.UserType == 2,
                    "seller" => currentUser.UserType == 3,
                    _ => false
                };

                return isMatch ? Visibility.Visible : Visibility.Collapsed;
            }

            return Visibility.Collapsed;
        }

        /// <summary>
        /// Not implemented for this converter.
        /// </summary>
        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotImplementedException();
        }
    }
}
