// <copyright file="NotificationProxyRepository.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

namespace SharedClassLibrary.ProxyRepository
{
    using System;
    using System.Collections.Generic;
    using System.Data;
    using System.Net.Http;
    using System.Net.Http.Json;
    using System.Text;
    using System.Text.Json;
    using System.Threading.Tasks;
    using SharedClassLibrary.Domain;
    using SharedClassLibrary.IRepository;
    using SharedClassLibrary.Shared;

    /// <summary>
    /// Proxy repository class for managing notification operations via REST API.
    /// </summary>
    public class NotificationProxyRepository : INotificationRepository
    {
        private const string ApiBaseRoute = "api/notifications";
        private readonly CustomHttpClient httpClient;

        /// <summary>
        /// Initializes a new instance of the <see cref="NotificationProxyRepository"/> class.
        /// </summary>
        /// <param name="baseApiUrl">The base url of the API.</param>
        public NotificationProxyRepository(string baseApiUrl)
        {
            var _httpClient = new HttpClient();
            _httpClient.BaseAddress = new System.Uri(baseApiUrl);
            this.httpClient = new CustomHttpClient(_httpClient);
        }

        /// <inheritdoc />
        public async void AddNotification(Notification notification)
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

            var options = new JsonSerializerOptions();
            options.Converters.Add(new NotificationConverter());

            var stream = await response.Content.ReadAsStreamAsync();
            var notifications = await JsonSerializer.DeserializeAsync<List<Notification>>(stream, options);


            return notifications ?? new List<Notification>();
        }

        /// <inheritdoc />
        public async void MarkAsRead(int notificationId)
        {
            var response = await this.httpClient.PutAsync($"{ApiBaseRoute}/{notificationId}/mark-read", null); // No body needed for this PUT.
            await this.ThrowOnError(nameof(MarkAsRead), response);
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
