// <copyright file="TrackedOrderProxyRepository.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

namespace SharedClassLibrary.ProxyRepository
{
    using System;
    using System.Collections.Generic;
    using System.Net.Http;
    using System.Net.Http.Json;
    using System.Threading.Tasks;
    using SharedClassLibrary.DataTransferObjects;
    using SharedClassLibrary.Domain;
    using SharedClassLibrary.IRepository;
    using SharedClassLibrary.Shared;

    /// <summary>
    /// Proxy repository for managing tracked order operations via a REST API.
    /// </summary>
    public class TrackedOrderProxyRepository : ITrackedOrderRepository
    {
        private const string ApiBaseRoute = "api/trackedorders";
        private readonly CustomHttpClient httpClient;

        /// <summary>
        /// Initializes a new instance of the <see cref="TrackedOrderProxyRepository"/> class.
        /// </summary>
        /// <param name="baseApiUrl">The base URL of the API (e.g., "http://localhost:5000/").</param>
        public TrackedOrderProxyRepository(string baseApiUrl)
        {
            var _httpClient = new HttpClient();
            _httpClient.BaseAddress = new System.Uri(baseApiUrl);
            this.httpClient = new CustomHttpClient(_httpClient);
        }

        /// <inheritdoc />
        public async Task<int> AddOrderCheckpointAsync(OrderCheckpoint checkpoint)
        {
            var response = await this.httpClient.PostAsJsonAsync($"{ApiBaseRoute}/checkpoints", checkpoint);
            await this.ThrowOnError(nameof(AddOrderCheckpointAsync), response);
            int createdCheckpoint = await response.Content.ReadFromJsonAsync<int>();
            return createdCheckpoint;
        }

        /// <inheritdoc />
        public async Task<int> AddTrackedOrderAsync(TrackedOrder order)
        {
            var response = await this.httpClient.PostAsJsonAsync(ApiBaseRoute, order);
            await this.ThrowOnError(nameof(AddTrackedOrderAsync), response);
            int createdOrder = await response.Content.ReadFromJsonAsync<int>();
            return createdOrder;
        }

        /// <inheritdoc />
        public async Task<bool> DeleteOrderCheckpointAsync(int checkpointID)
        {
            var response = await this.httpClient.DeleteAsync($"{ApiBaseRoute}/checkpoints/{checkpointID}");
            return response.IsSuccessStatusCode;
        }

        /// <inheritdoc />
        public async Task<bool> DeleteTrackedOrderAsync(int trackOrderID)
        {
            var response = await this.httpClient.DeleteAsync($"{ApiBaseRoute}/{trackOrderID}");
            return response.IsSuccessStatusCode;
        }

        /// <inheritdoc />
        public async Task<List<OrderCheckpoint>> GetAllOrderCheckpointsAsync(int trackedOrderID)
        {
            var response = await this.httpClient.GetAsync($"{ApiBaseRoute}/{trackedOrderID}/checkpoints");
            await this.ThrowOnError(nameof(GetAllOrderCheckpointsAsync), response);
            var checkpoints = await response.Content.ReadFromJsonAsync<List<OrderCheckpoint>>();
            return checkpoints ?? new List<OrderCheckpoint>();
        }

        /// <inheritdoc />
        public async Task<List<TrackedOrder>> GetAllTrackedOrdersAsync()
        {
            var response = await this.httpClient.GetAsync(ApiBaseRoute);
            await this.ThrowOnError(nameof(GetAllTrackedOrdersAsync), response);
            var orders = await response.Content.ReadFromJsonAsync<List<TrackedOrder>>();
            return orders ?? new List<TrackedOrder>();
        }

        /// <inheritdoc />
        public async Task<OrderCheckpoint> GetOrderCheckpointByIdAsync(int checkpointID)
        {
            var response = await this.httpClient.GetAsync($"{ApiBaseRoute}/checkpoints/{checkpointID}");
            if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
            {
                // Match the behavior of the original repository which throws on not found
                throw new Exception($"No OrderCheckpoint with id: {checkpointID}");
            }

            await this.ThrowOnError(nameof(GetOrderCheckpointByIdAsync), response);
            var checkpoint = await response.Content.ReadFromJsonAsync<OrderCheckpoint>();
            return checkpoint ?? throw new Exception($"Failed to deserialize OrderCheckpoint with id: {checkpointID}");
        }

        /// <inheritdoc />
        public async Task<TrackedOrder> GetTrackedOrderByIdAsync(int trackOrderID)
        {
            var response = await this.httpClient.GetAsync($"{ApiBaseRoute}/{trackOrderID}");
            if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
            {
                // Match the behavior of the original repository which throws on not found
                throw new Exception($"No TrackedOrder with id: {trackOrderID}");
            }

            await this.ThrowOnError(nameof(GetTrackedOrderByIdAsync), response);
            var order = await response.Content.ReadFromJsonAsync<TrackedOrder>();
            return order ?? throw new Exception($"Failed to deserialize TrackedOrder with id: {trackOrderID}");
        }

        /// <inheritdoc />
        public async Task UpdateOrderCheckpointAsync(int checkpointID, DateTime timestamp, string? location, string description, OrderStatus status)
        {
            // Create the update DTO expected by the API controller
            var updateRequest = new OrderCheckpointUpdateRequest
            {
                Timestamp = timestamp,
                Location = location,
                Description = description,
                Status = status,
            };

            var response = await this.httpClient.PutAsJsonAsync($"{ApiBaseRoute}/checkpoints/{checkpointID}", updateRequest);
            await this.ThrowOnError(nameof(UpdateOrderCheckpointAsync), response);
        }

        /// <inheritdoc />
        public async Task UpdateTrackedOrderAsync(int trackedOrderID, DateOnly estimatedDeliveryDate, OrderStatus currentStatus)
        {
            // Create the update DTO expected by the API controller
            var updateRequest = new TrackedOrderUpdateRequest
            {
                EstimatedDeliveryDate = estimatedDeliveryDate,
                CurrentStatus = currentStatus,
            };

            var response = await this.httpClient.PutAsJsonAsync($"{ApiBaseRoute}/{trackedOrderID}", updateRequest);
            await this.ThrowOnError(nameof(UpdateTrackedOrderAsync), response);
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