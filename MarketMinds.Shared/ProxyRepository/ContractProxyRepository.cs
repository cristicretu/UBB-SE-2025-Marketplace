namespace MarketMinds.Shared.ProxyRepository
{
    using System;
    using System.Collections.Generic;
    using System.Net.Http;
    using System.Net.Http.Json;
    using System.Text.Json;
    using System.Threading.Tasks;
    using MarketMinds.Shared.Models.DTOs;
    using MarketMinds.Shared.Models;
    using MarketMinds.Shared.IRepository;
    using Microsoft.Extensions.Configuration;

    /// <summary>
    /// Proxy repository class for managing contract operations via REST API.
    /// </summary>
    public class ContractProxyRepository : IContractRepository
    {
        private const string ApiBaseRoute = "api/contracts";
        private readonly HttpClient httpClient;

        /// <summary>
        /// Initializes a new instance of the <see cref="ContractProxyRepository"/> class.
        /// </summary>
        /// <param name="baseApiUrl">The base url of the API.</param>
        public ContractProxyRepository(IConfiguration configuration)
        {
            this.httpClient = new HttpClient();
            this.httpClient.BaseAddress = new System.Uri(configuration["ApiSettings:BaseUrl"] ?? "http://localhost:5001/");
        }

        /// <inheritdoc />
        public async Task<IContract> AddContractAsync(IContract contract, byte[] pdfFile)
        {
            // Create a DTO to send both contract data and PDF file
            var requestDto = new AddContractRequest
            {
                Contract = contract as Contract,
                PdfFile = pdfFile,
            };

            var response = await this.httpClient.PostAsJsonAsync($"{ApiBaseRoute}", requestDto);
            await this.ThrowOnError(nameof(AddContractAsync), response);
            var newContract = await response.Content.ReadFromJsonAsync<Contract>();
            return newContract ?? new Contract(); // Return default if null
        }

        /// <inheritdoc />
        public async Task<List<IContract>> GetAllContractsAsync()
        {
            var response = await this.httpClient.GetAsync($"{ApiBaseRoute}");
            await this.ThrowOnError(nameof(GetAllContractsAsync), response);
            var contracts = await response.Content.ReadFromJsonAsync<List<Contract>>();
            return contracts?.ConvertAll(c => (IContract)c) ?? new List<IContract>();
        }

        /// <inheritdoc />
        public async Task<(int BuyerID, string BuyerName)> GetContractBuyerAsync(long contractId)
        {
            var response = await this.httpClient.GetAsync($"{ApiBaseRoute}/{contractId}/buyer");
            await this.ThrowOnError(nameof(GetContractBuyerAsync), response);
            
            // Create a temporary class to deserialize the tuple properly
            var buyerInfo = await response.Content.ReadFromJsonAsync<BuyerInfo>();
            return buyerInfo != null ? (buyerInfo.BuyerId, buyerInfo.BuyerName) : (0, string.Empty);
        }

        /// <inheritdoc />
        public async Task<IContract> GetContractByIdAsync(long contractId)
        {
            var response = await this.httpClient.GetAsync($"{ApiBaseRoute}/{contractId}");
            await this.ThrowOnError(nameof(GetContractByIdAsync), response);
            var contract = await response.Content.ReadFromJsonAsync<Contract>();
            return contract ?? new Contract(); // Return default if null
        }

        /// <inheritdoc />
        public async Task<List<IContract>> GetContractHistoryAsync(long contractId)
        {
            var response = await this.httpClient.GetAsync($"{ApiBaseRoute}/{contractId}/history");
            await this.ThrowOnError(nameof(GetContractHistoryAsync), response);
            var history = await response.Content.ReadFromJsonAsync<List<Contract>>();
            return history?.ConvertAll(c => (IContract)c) ?? new List<IContract>();
        }

        /// <inheritdoc />
        public async Task<List<IContract>> GetContractsByBuyerAsync(int buyerId)
        {
            var response = await this.httpClient.GetAsync($"{ApiBaseRoute}/buyer/{buyerId}");
            await this.ThrowOnError(nameof(GetContractsByBuyerAsync), response);
            
            try {
                var contracts = await response.Content.ReadFromJsonAsync<List<Contract>>();
                return contracts?.ConvertAll(c => (IContract)c) ?? new List<IContract>();
            }
            catch (JsonException ex)
            {
                return new List<IContract>();
            }
        }

        /// <inheritdoc />
        public async Task<(int SellerID, string SellerName)> GetContractSellerAsync(long contractId)
        {
            var response = await this.httpClient.GetAsync($"{ApiBaseRoute}/{contractId}/seller");
            await this.ThrowOnError(nameof(GetContractSellerAsync), response);
            
            var sellerInfo = await response.Content.ReadFromJsonAsync<SellerInfo>();

            return sellerInfo != null ? (sellerInfo.SellerId, sellerInfo.SellerName) : (0, string.Empty);
        }

        /// <inheritdoc />
        public async Task<DateTime?> GetDeliveryDateByContractIdAsync(long contractId)
        {
            var response = await this.httpClient.GetAsync($"{ApiBaseRoute}/{contractId}/delivery-date");
            await this.ThrowOnError(nameof(GetDeliveryDateByContractIdAsync), response);
            try
            {
                var deliveryDate = await response.Content.ReadFromJsonAsync<DateTime?>();
                return deliveryDate;
            }
            catch (JsonException)
            {
                return null;
            }
        }

        /// <inheritdoc />
        public async Task<(string PaymentMethod, DateTime OrderDate)> GetOrderDetailsAsync(long contractId)
        {
            var response = await this.httpClient.GetAsync($"{ApiBaseRoute}/{contractId}/order-details");
            await this.ThrowOnError(nameof(GetOrderDetailsAsync), response);
            
            var orderDetails = await response.Content.ReadFromJsonAsync<OrderDetails>();
            return orderDetails != null ? (orderDetails.PaymentMethod, orderDetails.OrderDate) : (string.Empty, DateTime.Now);
        }

        /// <inheritdoc />
        public async Task<Dictionary<string, object>> GetOrderSummaryInformationAsync(long contractId)
        {
            var response = await this.httpClient.GetAsync($"{ApiBaseRoute}/{contractId}/order-summary");
            await this.ThrowOnError(nameof(GetOrderSummaryInformationAsync), response);
            
            try {
                var orderSummary = await response.Content.ReadFromJsonAsync<Dictionary<string, JsonElement>>();
                if (orderSummary == null) return new Dictionary<string, object>();
                
                // convert JsonElement values to appropriate .NET types
                var result = new Dictionary<string, object>();
                foreach (var item in orderSummary)
                {
                    result[item.Key] = ConvertJsonElement(item.Value);
                }
                
                return result;
            }
            catch (JsonException ex)
            {
                return new Dictionary<string, object>();
            }
        }

        /// <inheritdoc />
        public async Task<byte[]> GetPdfByContractIdAsync(long contractId)
        {
            var response = await this.httpClient.GetAsync($"{ApiBaseRoute}/{contractId}/pdf");
            await this.ThrowOnError(nameof(GetPdfByContractIdAsync), response);
            return await response.Content.ReadAsByteArrayAsync();
        }

        /// <inheritdoc />
        public async Task<PDF> AddPdfAsync(PDF pdf)
        {
            var response = await this.httpClient.PostAsJsonAsync($"{ApiBaseRoute}/pdf", pdf);
            await this.ThrowOnError(nameof(AddPdfAsync), response);
            var addedPdf = await response.Content.ReadFromJsonAsync<PDF>();
            return addedPdf ?? new PDF();
        }

        /// <inheritdoc />
        public async Task<IPredefinedContract> GetPredefinedContractByPredefineContractTypeAsync(PredefinedContractType predefinedContractType)
        {
            var response = await this.httpClient.GetAsync($"{ApiBaseRoute}/predefined/{(int)predefinedContractType}");
            await this.ThrowOnError(nameof(GetPredefinedContractByPredefineContractTypeAsync), response);
            var contract = await response.Content.ReadFromJsonAsync<PredefinedContract>();
            return contract ?? new PredefinedContract { ContractID = 1, ContractContent = string.Empty };
        }

        /// <inheritdoc />
        public async Task<(DateTime? StartDate, DateTime? EndDate, double price, string name)?> GetProductDetailsByContractIdAsync(long contractId)
        {
            var response = await this.httpClient.GetAsync($"{ApiBaseRoute}/{contractId}/product-details");
            await this.ThrowOnError(nameof(GetProductDetailsByContractIdAsync), response);
            try
            {
                var productDetails = await response.Content.ReadFromJsonAsync<ProductDetails>();
                if (productDetails == null) return null;
                
                return (productDetails.StartDate, productDetails.EndDate, productDetails.Price, productDetails.Name);
            }
            catch (JsonException ex)
            {
                return null;
            }
        }

        /// <inheritdoc />
        public async Task UpdateContractPdfIdAsync(long contractId, int pdfId)
        {
            var response = await this.httpClient.PutAsync($"{ApiBaseRoute}/{contractId}/pdf/{pdfId}", null);
            await this.ThrowOnError(nameof(UpdateContractPdfIdAsync), response);
        }

        /// <inheritdoc />
        public async Task UpdateContractStatusAsync(long contractId, string status)
        {
            var response = await this.httpClient.PutAsync($"{ApiBaseRoute}/{contractId}/status/{status}", null);
            await this.ThrowOnError(nameof(UpdateContractStatusAsync), response);
        }

        private async Task ThrowOnError(string methodName, HttpResponseMessage response)
        {
            if (!response.IsSuccessStatusCode)
            {
                string errorMessage = await response.Content.ReadAsStringAsync();
                if (string.IsNullOrEmpty(errorMessage))
                {
                    errorMessage = response.ReasonPhrase;
                }
                throw new Exception($"{methodName}: {errorMessage}");
            }
        }

        private class BuyerInfo
        {
            public int BuyerId { get; set; }
            public string BuyerName { get; set; } = string.Empty;
        }

        private class SellerInfo
        {
            public int SellerId { get; set; }
            public string SellerName { get; set; } = string.Empty;
        }

        private class OrderDetails
        {
            public string PaymentMethod { get; set; } = string.Empty;
            public DateTime OrderDate { get; set; }
        }

        private class ProductDetails
        {
            public DateTime? StartDate { get; set; }
            public DateTime? EndDate { get; set; }
            public double Price { get; set; }
            public string Name { get; set; } = string.Empty;
        }

        // Helper method to convert JsonElement to appropriate .NET type
        private object ConvertJsonElement(JsonElement element)
        {
            switch (element.ValueKind)
            {
                case JsonValueKind.String:
                    return element.GetString() ?? string.Empty;
                case JsonValueKind.Number:
                    if (element.TryGetInt32(out int intValue))
                        return intValue;
                    if (element.TryGetInt64(out long longValue))
                        return longValue;
                    if (element.TryGetDouble(out double doubleValue))
                        return doubleValue;
                    return 0;
                case JsonValueKind.True:
                    return true;
                case JsonValueKind.False:
                    return false;
                case JsonValueKind.Null:
                    return null;
                default:
                    return element.ToString();
            }
        }
    }
}