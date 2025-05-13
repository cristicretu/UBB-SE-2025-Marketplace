using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using MarketMinds.Shared.Models;
using Microsoft.UI.Xaml.Data;

namespace MarketMinds.Converters
{
    public class ImageSourceConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            // Handle AuctionProduct
            if (value is AuctionProduct auctionProduct)
            {
                if (auctionProduct.Images != null && auctionProduct.Images.Any())
                {
                    return auctionProduct.Images;
                }
                else if (auctionProduct.NonMappedImages != null && auctionProduct.NonMappedImages.Any())
                {
                    return auctionProduct.NonMappedImages;
                }
                return new List<object>();
            }

            // Handle BorrowProduct
            if (value is BorrowProduct borrowProduct)
            {
                if (borrowProduct.Images != null && borrowProduct.Images.Any())
                {
                    return borrowProduct.Images;
                }
                else if (borrowProduct.NonMappedImages != null && borrowProduct.NonMappedImages.Any())
                {
                    return borrowProduct.NonMappedImages;
                }
                return new List<object>();
            }

            // Handle BuyProduct
            if (value is BuyProduct buyProduct)
            {
                if (buyProduct.Images != null && buyProduct.Images.Any())
                {
                    return buyProduct.Images;
                }
                else if (buyProduct.NonMappedImages != null && buyProduct.NonMappedImages.Any())
                {
                    return buyProduct.NonMappedImages;
                }
                return new List<object>();
            }

            // Direct image collections
            if (value is IEnumerable images)
            {
                return images;
            }

            // Default - return empty collection
            return new List<object>();
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            // This converter only works one way
            throw new NotImplementedException();
        }
    }
}