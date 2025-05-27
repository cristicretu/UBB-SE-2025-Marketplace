using System;
using System.Collections;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Media.Imaging;

namespace MarketMinds.Converters
{
    /// <summary>
    /// Converts URL strings to ImageSource objects.
    /// </summary>
    public class UrlToImageSourceConverter : IValueConverter
    {
        /// <summary>
        /// Converts a URL string to an ImageSource.
        /// </summary>
        /// <param name="value">The URL string or Images collection.</param>
        /// <param name="targetType">The target type (not used).</param>
        /// <param name="parameter">The fallback image URL.</param>
        /// <param name="language">The language (not used).</param>
        /// <returns>A BitmapImage created from the URL.</returns>
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            try
            {
                string imageUrl = null;

                // If value is a collection (like Images), extract the first URL
                if (value is IEnumerable images)
                {
                    foreach (var item in images)
                    {
                        var prop = item.GetType().GetProperty("Url");
                        if (prop != null)
                        {
                            imageUrl = prop.GetValue(item)?.ToString();
                            if (!string.IsNullOrEmpty(imageUrl))
                            {
                                break;
                            }
                        }
                    }
                }
                // If value is already a string URL
                else if (value is string url)
                {
                    imageUrl = url;
                }

                // Use fallback if no image URL found
                if (string.IsNullOrEmpty(imageUrl))
                {
                    imageUrl = parameter?.ToString() ?? "ms-appx:///Assets/Products/default-product.png";
                }

                // Create BitmapImage from URL
                if (imageUrl.StartsWith("ms-appx://") || imageUrl.StartsWith("/"))
                {
                    return new BitmapImage(new Uri(imageUrl));
                }
                else if (Uri.IsWellFormedUriString(imageUrl, UriKind.Absolute))
                {
                    return new BitmapImage(new Uri(imageUrl));
                }
                else
                {
                    // Fallback to default image
                    return new BitmapImage(new Uri("ms-appx:///Assets/Products/default-product.png"));
                }
            }
            catch (Exception)
            {
                // Return fallback image on any error
                return new BitmapImage(new Uri("ms-appx:///Assets/Products/default-product.png"));
            }
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