// <copyright file="BuyerProxyRepository.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

namespace SharedClassLibrary.ProxyRepository
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Net.Http;
    using System.Net.Http.Json;
    using System.Threading.Tasks;
    using System.Web;
    using SharedClassLibrary.Domain;
    using SharedClassLibrary.IRepository;
    using SharedClassLibrary.Shared;

    /// <summary>
    /// Repository class for managing buyer-related database operations.
    /// </summary>
    /// <param name="databaseConnection">The database connection instance.</param>
    public class BuyerProxyRepository : IBuyerRepository
    {
        private const string ApiBaseRoute = "api/buyers";
        private readonly CustomHttpClient httpClient;

        /// <summary>
        /// Initializes a new instance of the <see cref="BuyerProxyRepository"/> class.
        /// </summary>
        /// <param name="baseApiUrl">The base url of the API.</param>
        public BuyerProxyRepository(string baseApiUrl)
        {
            var _httpClient = new HttpClient();
            _httpClient.BaseAddress = new System.Uri(baseApiUrl);
            this.httpClient = new CustomHttpClient(_httpClient);
        }

        /// <inheritdoc />
        public async Task<bool> CheckIfBuyerExists(int buyerId)
        {
            var response = await this.httpClient.GetAsync($"{ApiBaseRoute}/{buyerId}/exists");
            await this.ThrowOnError(nameof(CheckIfBuyerExists), response);
            return await response.Content.ReadFromJsonAsync<bool>();
        }

        /// <inheritdoc />
        public async Task CreateBuyer(Buyer buyerEntity)
        {
            var response = await this.httpClient.PostAsJsonAsync($"{ApiBaseRoute}/create", buyerEntity);
            await this.ThrowOnError(nameof(CreateBuyer), response);
        }

        /// <inheritdoc />
        public async Task CreateLinkageRequest(int requestingBuyerId, int receivingBuyerId)
        {
            var query = HttpUtility.ParseQueryString(string.Empty);
            query["requestingBuyerId"] = requestingBuyerId.ToString();
            query["receivingBuyerId"] = receivingBuyerId.ToString();
            string queryString = query.ToString() ?? string.Empty;

            var response = await this.httpClient.PostAsync($"{ApiBaseRoute}/linkages/create?{queryString}", null); // No body needed for this POST
            await this.ThrowOnError(nameof(CreateLinkageRequest), response);
        }

        /// <inheritdoc />
        public async Task<bool> DeleteLinkageRequest(int requestingBuyerId, int receivingBuyerId)
        {
            var query = HttpUtility.ParseQueryString(string.Empty);
            query["requestingBuyerId"] = requestingBuyerId.ToString();
            query["receivingBuyerId"] = receivingBuyerId.ToString();
            string queryString = query.ToString() ?? string.Empty;

            var response = await this.httpClient.DeleteAsync($"{ApiBaseRoute}/linkages/delete?{queryString}");

            // Check for 404 specifically if the API returns it when not found
            if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
            {
                return false;
            }

            await this.ThrowOnError(nameof(DeleteLinkageRequest), response);
            return true; // Assume success if EnsureSuccessStatusCode doesn't throw
        }

        /// <inheritdoc />
        public async Task<List<Buyer>> FindBuyersWithShippingAddress(Address shippingAddress)
        {
            var response = await this.httpClient.PostAsJsonAsync($"{ApiBaseRoute}/find-by-shipping-address", shippingAddress);
            await this.ThrowOnError(nameof(FindBuyersWithShippingAddress), response);
            var buyers = await response.Content.ReadFromJsonAsync<List<Buyer>>();
            return buyers ?? new List<Buyer>();
        }

        /// <inheritdoc />
        public async Task FollowSeller(int buyerId, int sellerId)
        {
            var response = await this.httpClient.PostAsync($"{ApiBaseRoute}/{buyerId}/follow/{sellerId}", null); // No body
            await this.ThrowOnError(nameof(FollowSeller), response);
        }

        /// <inheritdoc />
        public async Task<List<Seller>> GetAllSellers()
        {
            var response = await this.httpClient.GetAsync($"{ApiBaseRoute}/sellers/all");
            await this.ThrowOnError(nameof(GetAllSellers), response);
            var sellers = await response.Content.ReadFromJsonAsync<List<Seller>>();
            return sellers ?? new List<Seller>();
        }

        /// <inheritdoc />
        public async Task<List<BuyerLinkage>> GetBuyerLinkages(int buyerId)
        {
            var response = await this.httpClient.GetAsync($"{ApiBaseRoute}/{buyerId}/linkages");
            await this.ThrowOnError(nameof(GetBuyerLinkages), response);
            var linkages = await response.Content.ReadFromJsonAsync<List<BuyerLinkage>>();
            return linkages ?? new List<BuyerLinkage>();
        }

        /// <inheritdoc />
        public async Task<List<Seller>> GetFollowedSellers(List<int>? followingUsersIds)
        {
            if (followingUsersIds == null)
            {
                followingUsersIds = new List<int>();
            }

            var response = await this.httpClient.PostAsJsonAsync($"{ApiBaseRoute}/followed-sellers", followingUsersIds);
            await this.ThrowOnError(nameof(GetFollowedSellers), response);
            var sellers = await response.Content.ReadFromJsonAsync<List<Seller>>();
            return sellers ?? new List<Seller>();
        }

        /// <inheritdoc />
        public async Task<List<int>> GetFollowingUsersIds(int buyerId)
        {
            var response = await this.httpClient.GetAsync($"{ApiBaseRoute}/{buyerId}/following/ids");
            await this.ThrowOnError(nameof(GetFollowingUsersIds), response);
            var ids = await response.Content.ReadFromJsonAsync<List<int>>();
            return ids ?? new List<int>();
        }

        /// <inheritdoc />
        public async Task<List<Product>> GetProductsFromSeller(int sellerId)
        {
            var response = await this.httpClient.GetAsync($"{ApiBaseRoute}/sellers/{sellerId}/products");
            await this.ThrowOnError(nameof(GetProductsFromSeller), response);
            var products = await response.Content.ReadFromJsonAsync<List<Product>>();
            return products ?? new List<Product>();
        }

        /// <inheritdoc />
        public async Task<int> GetTotalCount()
        {
            var response = await this.httpClient.GetAsync($"{ApiBaseRoute}/count");
            await this.ThrowOnError(nameof(GetTotalCount), response);
            return await response.Content.ReadFromJsonAsync<int>();
        }

        /// <inheritdoc />
        public async Task<BuyerWishlist> GetWishlist(int buyerId)
        {
            var response = await this.httpClient.GetAsync($"{ApiBaseRoute}/{buyerId}/wishlist");
            await this.ThrowOnError(nameof(GetWishlist), response);
            var wishlist = await response.Content.ReadFromJsonAsync<BuyerWishlist>();
            return wishlist ?? new BuyerWishlist(); // Return empty wishlist if null
        }

        /// <inheritdoc />
        public async Task<bool> IsFollowing(int buyerId, int sellerId)
        {
            var response = await this.httpClient.GetAsync($"{ApiBaseRoute}/{buyerId}/following/check/{sellerId}");
            await this.ThrowOnError(nameof(IsFollowing), response);
            return await response.Content.ReadFromJsonAsync<bool>();
        }

        /// <inheritdoc />
        public async Task LoadBuyerInfo(Buyer buyerEntity)
        {
            int buyerId = buyerEntity.Id;
            var response = await this.httpClient.GetAsync($"{ApiBaseRoute}/{buyerId}/info");
            await this.ThrowOnError(nameof(LoadBuyerInfo), response);
            var loadedBuyer = await response.Content.ReadFromJsonAsync<Buyer>();
            if (loadedBuyer == null)
            {
                throw new InvalidOperationException($"Failed to load buyer info for ID: {buyerEntity.Id}. API returned null.");
            }

            // Update the passed-in buyerEntity with loaded data
            buyerEntity.FirstName = loadedBuyer.FirstName;
            buyerEntity.LastName = loadedBuyer.LastName;
            buyerEntity.Badge = loadedBuyer.Badge;
            buyerEntity.TotalSpending = loadedBuyer.TotalSpending;
            buyerEntity.NumberOfPurchases = loadedBuyer.NumberOfPurchases;
            buyerEntity.Discount = loadedBuyer.Discount;
            buyerEntity.UseSameAddress = loadedBuyer.UseSameAddress;
            buyerEntity.BillingAddress = loadedBuyer.BillingAddress;
            buyerEntity.ShippingAddress = loadedBuyer.ShippingAddress;
            buyerEntity.FollowingUsersIds = loadedBuyer.FollowingUsersIds;
        }

        /// <inheritdoc />
        public async Task RemoveWishilistItem(int buyerId, int productId)
        {
            var response = await this.httpClient.DeleteAsync($"{ApiBaseRoute}/{buyerId}/wishlist/remove/{productId}");
            await this.ThrowOnError(nameof(RemoveWishilistItem), response);
        }

        /// <inheritdoc />
        public async Task SaveInfo(Buyer buyerEntity)
        {
            var response = await this.httpClient.PostAsJsonAsync($"{ApiBaseRoute}/save", buyerEntity);
            await this.ThrowOnError(nameof(SaveInfo), response);
        }

        /// <inheritdoc />
        public async Task UnfollowSeller(int buyerId, int sellerId)
        {
            var response = await this.httpClient.DeleteAsync($"{ApiBaseRoute}/{buyerId}/unfollow/{sellerId}");
            await this.ThrowOnError(nameof(UnfollowSeller), response);
        }

        /// <inheritdoc />
        public async Task UpdateAfterPurchase(Buyer buyerEntity)
        {
            var response = await this.httpClient.PutAsJsonAsync($"{ApiBaseRoute}/update-after-purchase", buyerEntity);
            await this.ThrowOnError(nameof(UpdateAfterPurchase), response);
        }

        /// <inheritdoc />
        public async Task UpdateLinkageRequest(int requestingBuyerId, int receivingBuyerId)
        {
            var query = HttpUtility.ParseQueryString(string.Empty);
            query["requestingBuyerId"] = requestingBuyerId.ToString();
            query["receivingBuyerId"] = receivingBuyerId.ToString();
            string queryString = query.ToString() ?? string.Empty;

            var response = await this.httpClient.PutAsync($"{ApiBaseRoute}/linkages/update?{queryString}", null); // No body needed for this PUT
            await this.ThrowOnError(nameof(UpdateLinkageRequest), response);
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

        // Newly added methods

        /// <inheritdoc />
        public async Task<List<Address>> GetAllAddressesAsync()
        {
            var response = await this.httpClient.GetAsync($"{ApiBaseRoute}/addresses");
            await this.ThrowOnError(nameof(GetAllAddressesAsync), response);
            var addresses = await response.Content.ReadFromJsonAsync<List<Address>>();
            return addresses ?? new List<Address>();
        }

        /// <inheritdoc />
        public async Task<Address> GetAddressByIdAsync(int id)
        {
            var response = await this.httpClient.GetAsync($"{ApiBaseRoute}/addresses/{id}");
            await this.ThrowOnError(nameof(GetAddressByIdAsync), response);
            var address = await response.Content.ReadFromJsonAsync<Address>();
            if (address == null)
            {
                throw new KeyNotFoundException($"Address with ID {id} not found.");
            }
            return address;
        }

        /// <inheritdoc />
        public async Task AddAddressAsync(Address address)
        {
            var response = await this.httpClient.PostAsJsonAsync($"{ApiBaseRoute}/addresses", address);
            await this.ThrowOnError(nameof(AddAddressAsync), response);
        }

        /// <inheritdoc />
        public async Task UpdateAddressAsync(Address address)
        {
            var response = await this.httpClient.PutAsJsonAsync($"{ApiBaseRoute}/addresses/{address.Id}", address);
            await this.ThrowOnError(nameof(UpdateAddressAsync), response);
        }

        /// <inheritdoc />
        public async Task DeleteAddressAsync(Address address)
        {
            var response = await this.httpClient.DeleteAsync($"{ApiBaseRoute}/addresses/{address.Id}");
            await this.ThrowOnError(nameof(DeleteAddressAsync), response);
        }

        /// <inheritdoc />
        public async Task<bool> AddressExistsAsync(int id)
        {
            var response = await this.httpClient.GetAsync($"{ApiBaseRoute}/addresses/{id}/exists");
            await this.ThrowOnError(nameof(AddressExistsAsync), response);
            return await response.Content.ReadFromJsonAsync<bool>();
        }
    }
}
