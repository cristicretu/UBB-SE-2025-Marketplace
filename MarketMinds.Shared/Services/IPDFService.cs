namespace MarketMinds.Shared.Services
{
    public interface IPDFService
    {
        Task<int> InsertPdfAsync(byte[] fileBytes);
    }
}