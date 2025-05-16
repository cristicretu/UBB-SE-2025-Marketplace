using MarketMinds.Shared.ProxyRepository;

using MarketMinds.Shared.IRepository;
using MarketMinds.Shared.Helper;

namespace MarketMinds.Shared.Services
{
    public class PDFService : IPDFService
    {
        private IPDFRepository pdfRepository;
        public PDFService()
        {
            pdfRepository = new PDFProxyRepository(AppConfig.GetBaseApiUrl());
        }

        public async Task<int> InsertPdfAsync(byte[] fileBytes)
        {
            if (fileBytes == null || fileBytes.Length == 0)
            {
                throw new ArgumentException("File bytes cannot be null or empty.", nameof(fileBytes));
            }
            return await pdfRepository.InsertPdfAsync(fileBytes);
        }
    }
}
