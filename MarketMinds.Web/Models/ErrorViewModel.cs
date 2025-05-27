namespace MarketMinds.Web.Models
{
    public class ErrorViewModel
    {
        public string? RequestId { get; set; }
        public string? ErrorMessage { get; set; }

        public bool ShowRequestId => !string.IsNullOrEmpty(RequestId);
        public bool ShowErrorMessage => !string.IsNullOrEmpty(ErrorMessage);
    }
}