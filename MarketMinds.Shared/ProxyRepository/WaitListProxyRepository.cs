// <copyright file="WaitListProxyRepository.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

namespace MarketMinds.Shared.ProxyRepository
{
    using System;
    using System.Collections.Generic;
    using System.Net.Http;
    using System.Net.Http.Json;
    using System.Threading.Tasks;
    using MarketMinds.Shared.Models;
    using MarketMinds.Shared.IRepository;

    /// <summary>
    /// Proxy repository class for managing waitlist operations via REST API.
    /// </summary>
    public class WaitListProxyRepository : IWaitListRepository
    {
        private const string ApiBaseRoute = "api/waitlist"; // Match the controller route
        private readonly HttpClient httpClient;

        /// <summary>
        /// Initializes a new instance of the <see cref="WaitListProxyRepository"/> class.
        /// </summary>
        /// <param name="baseApiUrl">The base url of the API server.</param>
        public WaitListProxyRepository(string baseApiUrl)
        {
            this.httpClient = new HttpClient();
            this.httpClient.BaseAddress = new System.Uri(baseApiUrl);
        }

        /// <inheritdoc />
        public async Task AddUserToWaitlist(int userId, int productId)
        {
            var response = await this.httpClient.PostAsync($"{ApiBaseRoute}/user/{userId}/product/{productId}", null);
            await this.ThrowOnError(nameof(AddUserToWaitlist), response);
        }

        /// <inheritdoc />
        /// <summary>
        /// Implements IWaitListRepository.AddUserToWaitlist(int, int, DateTime?)
        /// </summary>
        public async Task AddUserToWaitlist(int userId, int productId, DateTime? preferredEndDate)
        {
            var requestData = new
            {
                preferredEndDate = preferredEndDate?.ToString("yyyy-MM-dd")
            };

            var response = await this.httpClient.PostAsJsonAsync($"{ApiBaseRoute}/user/{userId}/product/{productId}/with-enddate", requestData);
            await this.ThrowOnError(nameof(AddUserToWaitlist), response);
        }

        /// <inheritdoc />
        public async Task<List<UserWaitList>> GetUsersInWaitlist(int productId)
        {
            var response = await this.httpClient.GetAsync($"{ApiBaseRoute}/product/{productId}/users");
            await this.ThrowOnError(nameof(GetUsersInWaitlist), response);
            var users = await response.Content.ReadFromJsonAsync<List<UserWaitList>>();
            return users ?? new List<UserWaitList>();
        }

        /// <inheritdoc />
        public async Task<List<UserWaitList>> GetUsersInWaitlistOrdered(int productId)
        {
            var response = await this.httpClient.GetAsync($"{ApiBaseRoute}/product/{productId}/users/ordered");
            await this.ThrowOnError(nameof(GetUsersInWaitlistOrdered), response);
            var users = await response.Content.ReadFromJsonAsync<List<UserWaitList>>();
            return users ?? new List<UserWaitList>();
        }

        /// <inheritdoc />
        public async Task<int> GetUserWaitlistPosition(int userId, int productId)
        {
            var response = await this.httpClient.GetAsync($"{ApiBaseRoute}/user/{userId}/product/{productId}/position");
            await this.ThrowOnError(nameof(GetUserWaitlistPosition), response);
            return await response.Content.ReadFromJsonAsync<int>();
        }

        /// <inheritdoc />
        public async Task<List<UserWaitList>> GetUserWaitlists(int userId)
        {
            var response = await this.httpClient.GetAsync($"{ApiBaseRoute}/user/{userId}/waitlists");
            await this.ThrowOnError(nameof(GetUserWaitlists), response);
            var waitlists = await response.Content.ReadFromJsonAsync<List<UserWaitList>>();
            return waitlists ?? new List<UserWaitList>();
        }

        /// <inheritdoc />
        public async Task<int> GetWaitlistSize(int productId)
        {
            var response = await this.httpClient.GetAsync($"{ApiBaseRoute}/product/{productId}/size");
            await this.ThrowOnError(nameof(GetWaitlistSize), response);
            return await response.Content.ReadFromJsonAsync<int>();
        }

        /// <inheritdoc />
        public async Task<bool> IsUserInWaitlist(int userId, int productId)
        {
            var response = await this.httpClient.GetAsync($"{ApiBaseRoute}/user/{userId}/product/{productId}/exists");
            await this.ThrowOnError(nameof(IsUserInWaitlist), response);
            return await response.Content.ReadFromJsonAsync<bool>();
        }

        /// <inheritdoc />
        public async Task RemoveUserFromWaitlist(int userId, int productId)
        {
            var response = await this.httpClient.DeleteAsync($"{ApiBaseRoute}/user/{userId}/product/{productId}");
            await this.ThrowOnError(nameof(RemoveUserFromWaitlist), response);
        }

        /// <inheritdoc />
        /// <summary>
        /// Implements IWaitListRepository.GetUserWaitlistEntry(int, int)
        /// </summary>
        public async Task<UserWaitList?> GetUserWaitlistEntry(int userId, int productId)
        {
            var response = await this.httpClient.GetAsync($"{ApiBaseRoute}/user/{userId}/product/{productId}/entry");
            await this.ThrowOnError(nameof(GetUserWaitlistEntry), response);
            var entry = await response.Content.ReadFromJsonAsync<UserWaitList?>();
            return entry;
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