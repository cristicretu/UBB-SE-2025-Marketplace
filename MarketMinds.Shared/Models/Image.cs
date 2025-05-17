namespace MarketMinds.Shared.Models
{
    /// <summary>
    /// Generic Image class used across multiple parts of the application
    /// </summary>
    public class Image
    {
        public int Id { get; set; }
        public string Url { get; set; } = string.Empty;

        public Image(string url)
        {
            this.Url = url;
        }

        // Default constructor for Entity Framework
        public Image()
        {
            // Empty
        }
    }
}