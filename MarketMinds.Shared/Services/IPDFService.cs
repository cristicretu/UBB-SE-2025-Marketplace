using System.Threading.Tasks;

namespace SharedClassLibrary.Service
{
    public interface IPDFService
    {
        Task<int> InsertPdfAsync(byte[] fileBytes);
    }
}