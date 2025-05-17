using System;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using Marketplace_SE.Utilities;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Media.Imaging;
using Windows.Storage.Streams;

namespace MarketMinds.Converters
{
    public class HexToBitmapImageConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            if (value is string hexString && !string.IsNullOrEmpty(hexString))
            {
                // Start the asynchronous conversion and return a placeholder value
                _ = ConvertHexToBitmapImageAsync(hexString);
                return null; // Placeholder value while the image is being loaded
            }
            return null;
        }

        private async Task ConvertHexToBitmapImageAsync(string hexString)
        {
            try
            {
                byte[] imageBytes = DataEncoder.HexDecode(hexString);
                if (imageBytes != null && imageBytes.Length > 0)
                {
                    using (var stream = new InMemoryRandomAccessStream())
                    {
                        await stream.WriteAsync(imageBytes.AsBuffer());
                        stream.Seek(0);

                        var bitmap = new BitmapImage();
                        await bitmap.SetSourceAsync(stream);
                        // Handle the loaded bitmap (e.g., update the UI or cache it)
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error converting hex to BitmapImage: {ex.Message}");
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotImplementedException();
        }
    }
}
