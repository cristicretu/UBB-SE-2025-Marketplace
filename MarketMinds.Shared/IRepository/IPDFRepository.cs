using System.Threading.Tasks;

namespace MarketMinds.Shared.IRepository
{
    public interface IPDFRepository
    {
        Task<int> InsertPdfAsync(byte[] fileBytes);
    }
}