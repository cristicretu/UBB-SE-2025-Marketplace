using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SharedClassLibrary.ProxyRepository;
using SharedClassLibrary.Shared;
using SharedClassLibrary.IRepository;
using SharedClassLibrary.Helper;
using SharedClassLibrary.ProxyRepository;

namespace SharedClassLibrary.Service
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
