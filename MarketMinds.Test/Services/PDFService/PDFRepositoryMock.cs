using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using MarketMinds.Shared.IRepository;

namespace MarketMinds.Tests.Mocks
{
    public class MockPdfRepository : IPDFRepository
    {
        private readonly List<byte[]> _store = new();
        private int _nextId = 1;

        public Task<int> InsertPdfAsync(byte[] fileBytes)
        {
            if (fileBytes == null || fileBytes.Length == 0)
                throw new ArgumentException("File bytes cannot be null or empty.", nameof(fileBytes));

            _store.Add(fileBytes);
            return Task.FromResult(_nextId++);
        }
    }
}
