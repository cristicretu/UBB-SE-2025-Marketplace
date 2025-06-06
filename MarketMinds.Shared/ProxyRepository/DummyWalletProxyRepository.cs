// <copyright file="DummyWalletProxyRepository.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

namespace MarketMinds.Shared.ProxyRepository
{
    using System;
    using System.Net.Http;
    using System.Net.Http.Json;
    using System.Threading.Tasks;
    using MarketMinds.Shared.IRepository;


    /// <summary>
    /// Proxy repository class for managing dummy wallet operations via REST API.
    /// </summary>
    public class DummyWalletProxyRepository : IDummyWalletRepository
    {
        private const string ApiBaseRoute = "api/wallets";
        private readonly HttpClient httpClient;

        /// <summary>
        /// Initializes a new instance of the <see cref="DummyWalletProxyRepository"/> class.
        /// </summary>
        /// <param name="baseApiUrl">The base URL of the API server (e.g., "http://localhost:5000").</param>
        public DummyWalletProxyRepository(string baseApiUrl)
        {
            this.httpClient = new HttpClient();
            this.httpClient.BaseAddress = new System.Uri(baseApiUrl);
        }

        /// <inheritdoc />
        public async Task<double> GetWalletBalanceAsync(int userId)
        {
            var response = await this.httpClient.GetAsync($"{ApiBaseRoute}/{userId}/balance");
            await this.ThrowOnError(nameof(GetWalletBalanceAsync), response);

            var balance = await response.Content.ReadFromJsonAsync<double>();
            return balance;
        }

        /// <inheritdoc />
        public async Task UpdateWalletBalance(int userId, double newBalance)
        {
            var response = await this.httpClient.PutAsJsonAsync($"{ApiBaseRoute}/{userId}/balance", newBalance);
            await this.ThrowOnError(nameof(UpdateWalletBalance), response);
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