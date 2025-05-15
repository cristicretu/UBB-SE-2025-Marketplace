// <copyright file="OrderSummaryProxyRepository.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

namespace SharedClassLibrary.ProxyRepository
{
    using System;
    using System.Net.Http;
    using System.Net.Http.Json;
    using System.Threading.Tasks;
    using SharedClassLibrary.DataTransferObjects;
    using SharedClassLibrary.Domain;
    using SharedClassLibrary.IRepository;
    using SharedClassLibrary.Shared;

    /// <summary>
    /// Proxy repository class for managing order summary operations via REST API.
    /// </summary>
    public class OrderSummaryProxyRepository : IOrderSummaryRepository
    {
        private const string ApiBaseRoute = "api/ordersummaries";
        private readonly CustomHttpClient httpClient;

        /// <summary>
        /// Initializes a new instance of the <see cref="OrderSummaryProxyRepository"/> class.
        /// </summary>
        /// <param name="baseApiUrl">The base url of the API.</param>
        public OrderSummaryProxyRepository(string baseApiUrl)
        {
            var _httpClient = new HttpClient();
            _httpClient.BaseAddress = new System.Uri(baseApiUrl);
            this.httpClient = new CustomHttpClient(_httpClient);
        }

        /// <inheritdoc />
        public async Task<OrderSummary> GetOrderSummaryByIdAsync(int orderSummaryId)
        {
            var response = await this.httpClient.GetAsync($"{ApiBaseRoute}/{orderSummaryId}");
            if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
            {
                return null;
            }

            await this.ThrowOnError(nameof(GetOrderSummaryByIdAsync), response);
            var orderSummary = await response.Content.ReadFromJsonAsync<OrderSummary>();
            return orderSummary;
        }

        /// <inheritdoc />
        public async Task UpdateOrderSummaryAsync(int id, double subtotal, double warrantyTax, double deliveryFee, double finalTotal, string fullName, string email, string phoneNumber, string address, string postalCode, string additionalInfo, string contractDetails)
        {
            // Create a DTO to send the update parameters
            var requestDto = new UpdateOrderSummaryRequest
            {
                Id = id,
                Subtotal = subtotal,
                WarrantyTax = warrantyTax,
                DeliveryFee = deliveryFee,
                FinalTotal = finalTotal,
                FullName = fullName,
                Email = email,
                PhoneNumber = phoneNumber,
                Address = address,
                PostalCode = postalCode,
                AdditionalInfo = additionalInfo,
                ContractDetails = contractDetails,
            };

            var response = await this.httpClient.PutAsJsonAsync($"{ApiBaseRoute}", requestDto);
            await this.ThrowOnError(nameof(UpdateOrderSummaryAsync), response);
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