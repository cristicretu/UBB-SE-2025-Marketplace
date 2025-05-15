namespace SharedClassLibrary.ProxyRepository
{
    using System;
    using System.Collections.Generic;
    using System.Net.Http;
    using System.Net.Http.Json;
    using System.Text.Json;
    using System.Threading.Tasks;
    using SharedClassLibrary.DataTransferObjects;
    using SharedClassLibrary.Domain;
    using SharedClassLibrary.IRepository;
    using SharedClassLibrary.Shared;

    /// <summary>
    /// Proxy repository class for managing contract operations via REST API.
    /// </summary>
    public class ContractProxyRepository : IContractRepository
    {
        private const string ApiBaseRoute = "api/contracts";
        private readonly CustomHttpClient httpClient;

        /// <summary>
        /// Initializes a new instance of the <see cref="ContractProxyRepository"/> class.
        /// </summary>
        /// <param name="baseApiUrl">The base url of the API.</param>
        public ContractProxyRepository(string baseApiUrl)
        {
            var _httpClient = new HttpClient();
            _httpClient.BaseAddress = new System.Uri(baseApiUrl);
            this.httpClient = new CustomHttpClient(_httpClient);
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
            var buyerInfo = await response.Content.ReadFromJsonAsync<(int, string)>();
            return buyerInfo;
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
            var contracts = await response.Content.ReadFromJsonAsync<List<Contract>>();
            return contracts?.ConvertAll(c => (IContract)c) ?? new List<IContract>();
        }

        /// <inheritdoc />
        public async Task<(int SellerID, string SellerName)> GetContractSellerAsync(long contractId)
        {
            var response = await this.httpClient.GetAsync($"{ApiBaseRoute}/{contractId}/seller");
            await this.ThrowOnError(nameof(GetContractSellerAsync), response);
            var sellerInfo = await response.Content.ReadFromJsonAsync<(int, string)>();
            return sellerInfo;
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
            var orderDetails = await response.Content.ReadFromJsonAsync<(string, DateTime)>();
            return orderDetails;
        }

        /// <inheritdoc />
        public async Task<Dictionary<string, object>> GetOrderSummaryInformationAsync(long contractId)
        {
            var response = await this.httpClient.GetAsync($"{ApiBaseRoute}/{contractId}/order-summary");
            await this.ThrowOnError(nameof(GetOrderSummaryInformationAsync), response);
            var orderSummary = await response.Content.ReadFromJsonAsync<Dictionary<string, object>>();
            return orderSummary ?? new Dictionary<string, object>();
        }

        /// <inheritdoc />
        public async Task<byte[]> GetPdfByContractIdAsync(long contractId)
        {
            var response = await this.httpClient.GetAsync($"{ApiBaseRoute}/{contractId}/pdf");
            await this.ThrowOnError(nameof(GetPdfByContractIdAsync), response);
            var pdfFile = await response.Content.ReadAsByteArrayAsync();
            return pdfFile;
        }

        /// <inheritdoc />
        public async Task<IPredefinedContract> GetPredefinedContractByPredefineContractTypeAsync(PredefinedContractType predefinedContractType)
        {
            var response = await this.httpClient.GetAsync($"{ApiBaseRoute}/predefined/{(int)predefinedContractType}");
            await this.ThrowOnError(nameof(GetPredefinedContractByPredefineContractTypeAsync), response);
            var contract = await response.Content.ReadFromJsonAsync<PredefinedContract>();
            return contract ?? new PredefinedContract { ContractID = 0, ContractContent = string.Empty };
        }

        /// <inheritdoc />
        public async Task<(DateTime? StartDate, DateTime? EndDate, double price, string name)?> GetProductDetailsByContractIdAsync(long contractId)
        {
            var response = await this.httpClient.GetAsync($"{ApiBaseRoute}/{contractId}/product-details");
            await this.ThrowOnError(nameof(GetProductDetailsByContractIdAsync), response);
            try
            {
                var productDetails = await response.Content.ReadFromJsonAsync<(DateTime?, DateTime?, double, string)?>();
                return productDetails;
            }
            catch (JsonException)
            {
                return null;
            }
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
    }
}