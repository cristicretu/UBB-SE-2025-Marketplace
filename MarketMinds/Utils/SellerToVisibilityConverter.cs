﻿using System;
using System.Diagnostics.CodeAnalysis;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Data;

namespace MarketMinds.Utils
{
    [ExcludeFromCodeCoverage]
    partial class SellerToVisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            if (value != null)
            {
                return Visibility.Visible;
            }
            else
            {
                return Visibility.Collapsed;
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotImplementedException();
        }
    }
}
