using System.Threading.Tasks;

namespace SharedClassLibrary.IRepository
{
    public interface IPDFRepository
    {
        Task<int> InsertPdfAsync(byte[] fileBytes);
    }
}