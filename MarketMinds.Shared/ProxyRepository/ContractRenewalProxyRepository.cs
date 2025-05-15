namespace SharedClassLibrary.ProxyRepository
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Net.Http;
    using System.Net.Http.Json;
    using System.Threading.Tasks;
    using SharedClassLibrary.Domain;
    using SharedClassLibrary.IRepository;
    using SharedClassLibrary.Shared;

    /// <summary>
    /// Proxy repository class for managing contract renewal operations via REST API.
    /// </summary>
    public class ContractRenewalProxyRepository : IContractRenewalRepository
    {
        private const string ApiBaseRoute = "api/contract-renewals";
        private readonly CustomHttpClient httpClient;

        /// <summary>
        /// Initializes a new instance of the <see cref="ContractRenewalProxyRepository"/> class.
        /// </summary>
        /// <param name="baseApiUrl">The base url of the API.</param>
        public ContractRenewalProxyRepository(string baseApiUrl)
        {
            var _httpClient = new HttpClient();
            _httpClient.BaseAddress = new System.Uri(baseApiUrl);
            this.httpClient = new CustomHttpClient(_httpClient);
        }

        /// <inheritdoc />
        public async Task AddRenewedContractAsync(IContract contract)
        {
            var response = await this.httpClient.PostAsJsonAsync($"{ApiBaseRoute}/add-renewed", (Contract)contract);
            await this.ThrowOnError(nameof(AddRenewedContractAsync), response);
        }

        /// <inheritdoc />
        public async Task<List<IContract>> GetRenewedContractsAsync()
        {
            var response = await this.httpClient.GetAsync($"{ApiBaseRoute}/renewed");
            await this.ThrowOnError(nameof(GetRenewedContractsAsync), response);

            // Deserialize to List<Contract> (concrete type) as interfaces usually can't be deserialized directly
            var contracts = await response.Content.ReadFromJsonAsync<List<Contract>>();
            if (contracts == null)
            {
                contracts = new List<Contract>();
            }

            return contracts.Cast<IContract>().ToList();
        }

        /// <inheritdoc />
        public async Task<bool> HasContractBeenRenewedAsync(long contractId)
        {
            if (contractId <= 0)
            {
                throw new ArgumentException("Contract ID must be positive.", nameof(contractId));
            }

            var response = await this.httpClient.GetAsync($"{ApiBaseRoute}/{contractId}/has-been-renewed");
            await this.ThrowOnError(nameof(HasContractBeenRenewedAsync), response);

            var result = await response.Content.ReadFromJsonAsync<bool>();
            return result;
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