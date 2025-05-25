using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Data;
using System;
using MarketMinds.Shared.Models;

namespace MarketMinds.Converters
{
    public class BuyerLinkageStatusToVisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            if (value is BuyerLinkageStatus status)
            {
                var param = parameter?.ToString();
                if (param == "ShowActions")
                    return (status == BuyerLinkageStatus.PendingSent || status == BuyerLinkageStatus.PendingReceived) ? Visibility.Visible : Visibility.Collapsed;
                if (param == "PendingSelf")
                    return status == BuyerLinkageStatus.PendingSent ? Visibility.Visible : Visibility.Collapsed;
                if (param == "PendingOther")
                    return status == BuyerLinkageStatus.PendingReceived ? Visibility.Visible : Visibility.Collapsed;
            }
            return Visibility.Collapsed;
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotImplementedException();
        }
    }
} 