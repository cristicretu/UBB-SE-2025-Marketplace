// <copyright file="WaitListProxyRepository.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

namespace SharedClassLibrary.ProxyRepository
{
    using System;
    using System.Collections.Generic;
    using System.Net.Http;
    using System.Net.Http.Json;
    using System.Threading.Tasks;
    using SharedClassLibrary.Domain;
    using SharedClassLibrary.IRepository;
    using SharedClassLibrary.Shared;

    /// <summary>
    /// Proxy repository class for managing waitlist operations via REST API.
    /// </summary>
    public class WaitListProxyRepository : IWaitListRepository
    {
        private const string ApiBaseRoute = "api/waitlist"; // Match the controller route
        private readonly CustomHttpClient httpClient;

        /// <summary>
        /// Initializes a new instance of the <see cref="WaitListProxyRepository"/> class.
        /// </summary>
        /// <param name="baseApiUrl">The base url of the API server.</param>
        public WaitListProxyRepository(string baseApiUrl)
        {
            var _httpClient = new HttpClient();
            _httpClient.BaseAddress = new System.Uri(baseApiUrl);
            this.httpClient = new CustomHttpClient(_httpClient);
        }

        /// <inheritdoc />
        public async Task AddUserToWaitlist(int userId, int productWaitListId)
        {
            var response = await this.httpClient.PostAsync($"{ApiBaseRoute}/user/{userId}/product/{productWaitListId}", null);
            await this.ThrowOnError(nameof(AddUserToWaitlist), response);
        }

        /// <inheritdoc />
        public async Task<List<UserWaitList>> GetUsersInWaitlist(int waitListProductId)
        {
            var response = await this.httpClient.GetAsync($"{ApiBaseRoute}/product/{waitListProductId}/users");
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
        public async Task<int> GetWaitlistSize(int productWaitListId)
        {
            var response = await this.httpClient.GetAsync($"{ApiBaseRoute}/product/{productWaitListId}/size");
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
        public async Task RemoveUserFromWaitlist(int userId, int productWaitListId)
        {
            var response = await this.httpClient.DeleteAsync($"{ApiBaseRoute}/user/{userId}/product/{productWaitListId}");
            await this.ThrowOnError(nameof(RemoveUserFromWaitlist), response);
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