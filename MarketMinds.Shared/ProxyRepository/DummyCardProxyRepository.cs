// <copyright file="DummyCardProxyRepository.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

namespace SharedClassLibrary.ProxyRepository
{
    using System;
    using System.Net.Http;
    using System.Net.Http.Json;
    using System.Threading.Tasks;
    using SharedClassLibrary.IRepository;
    using SharedClassLibrary.Shared;

    /// <summary>
    /// Proxy repository class for managing dummy card operations via REST API.
    /// </summary>
    public class DummyCardProxyRepository : IDummyCardRepository
    {
        private const string ApiBaseRoute = "api/dummycards";
        private readonly CustomHttpClient httpClient;

        /// <summary>
        /// Initializes a new instance of the <see cref="DummyCardProxyRepository"/> class.
        /// </summary>
        /// <param name="baseApiUrl">The base url of the API.</param>
        public DummyCardProxyRepository(string baseApiUrl)
        {
            var _httpClient = new HttpClient();
            _httpClient.BaseAddress = new System.Uri(baseApiUrl);
            this.httpClient = new CustomHttpClient(_httpClient);
        }

        /// <inheritdoc />
        public async Task DeleteCardAsync(string cardNumber)
        {
            var response = await this.httpClient.DeleteAsync($"{ApiBaseRoute}/{cardNumber}");
            await this.ThrowOnError(nameof(DeleteCardAsync), response);
        }

        /// <inheritdoc />
        public async Task<double> GetCardBalanceAsync(string cardNumber)
        {
            var response = await this.httpClient.GetAsync($"{ApiBaseRoute}/{cardNumber}/balance");
            await this.ThrowOnError(nameof(GetCardBalanceAsync), response);
            var balance = await response.Content.ReadFromJsonAsync<double>();
            return balance;
        }

        /// <inheritdoc />
        public async Task UpdateCardBalanceAsync(string cardNumber, double balance)
        {
            var response = await this.httpClient.PutAsJsonAsync($"{ApiBaseRoute}/{cardNumber}/balance", balance);
            await this.ThrowOnError(nameof(UpdateCardBalanceAsync), response);
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
