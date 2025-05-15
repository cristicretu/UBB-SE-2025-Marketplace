using System;
using System.Collections;
using Microsoft.UI.Xaml.Data;

namespace MarketMinds.Converters
{
    public class FirstImageUrlConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            try
            {
                if (value == null)
                {
                    return parameter?.ToString() ?? "/Assets/placeholder.png";
                }

                if (value is IEnumerable images)
                {
                    foreach (var item in images)
                    {
                        var prop = item.GetType().GetProperty("Url");
                        if (prop != null)
                        {
                            string url = prop.GetValue(item)?.ToString();
                            if (!string.IsNullOrEmpty(url))
                            {
                                return url;
                            }
                        }
                    }
                }
            }
            catch (Exception)
            {
                // Silent catch - return fallback
            }

            // Return fallback image
            return parameter?.ToString() ?? "/Assets/placeholder.png";
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotImplementedException();
        }
    }
}