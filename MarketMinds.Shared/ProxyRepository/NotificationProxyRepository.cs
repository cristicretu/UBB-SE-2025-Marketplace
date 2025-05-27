// <copyright file="NotificationProxyRepository.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

namespace MarketMinds.Shared.ProxyRepository
{
    using System;
    using System.Collections.Generic;
    using System.Net.Http;
    using System.Net.Http.Json;
    using System.Text.Json;
    using System.Threading.Tasks;
    using MarketMinds.Shared.Models;
    using MarketMinds.Shared.IRepository;
    using Microsoft.Extensions.Configuration;


    /// <summary>
    /// Proxy repository class for managing notification operations via REST API.
    /// </summary>
    public class NotificationProxyRepository : INotificationRepository
    {
        private const string ApiBaseRoute = "api/notifications";
        private readonly HttpClient httpClient;

        /// <summary>
        /// Initializes a new instance of the <see cref="NotificationProxyRepository"/> class.
        /// </summary>
        /// <param name="baseApiUrl">The base url of the API.</param>
        public NotificationProxyRepository(string baseApiUrl)
        {
            this.httpClient = new HttpClient();
            this.httpClient.BaseAddress = new System.Uri(baseApiUrl);

        }

        /// <inheritdoc />
        public async Task AddNotification(Notification notification)
        {
            if (notification == null)
            {
                throw new ArgumentNullException(nameof(notification));
            }

            var response = await this.httpClient.PostAsJsonAsync($"{ApiBaseRoute}", notification);
            await this.ThrowOnError(nameof(AddNotification), response);
        }

        /// <inheritdoc />
        public async Task<List<Notification>> GetNotificationsForUser(int recipientId)
        {
            var response = await this.httpClient.GetAsync($"{ApiBaseRoute}/user/{recipientId}");
            await this.ThrowOnError(nameof(GetNotificationsForUser), response);
            //var notifications = await response.Content.ReadFromJsonAsync<List<Notification>>();

            var rawJson = await response.Content.ReadAsStringAsync();

            var options = new JsonSerializerOptions();
            options.Converters.Add(new NotificationConverter());

            var stream = await response.Content.ReadAsStreamAsync();
            var notifications = await JsonSerializer.DeserializeAsync<List<Notification>>(stream, options);


            return notifications ?? new List<Notification>();
        }

        /// <inheritdoc />
        public async Task MarkAsRead(int userId)
        {
            var response = await this.httpClient.PutAsync($"{ApiBaseRoute}/{userId}/mark-read", null);
            await this.ThrowOnError(nameof(MarkAsRead), response);
        }

        /// <inheritdoc />
        public async Task MarkNotificationAsRead(int notificationId)
        {
            var response = await this.httpClient.PutAsync($"{ApiBaseRoute}/notification/{notificationId}/mark-read", null);
            await this.ThrowOnError(nameof(MarkNotificationAsRead), response);
        }

        /// <inheritdoc />
        public async Task ClearAllNotifications(int userId)
        {
            var response = await this.httpClient.DeleteAsync($"{ApiBaseRoute}/{userId}/clear-all");
            await this.ThrowOnError(nameof(ClearAllNotifications), response);
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
