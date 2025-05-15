using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SharedClassLibrary.Domain;
using SharedClassLibrary.IRepository;
using System.Net.Http;
using System.Net.Http.Json;
using SharedClassLibrary.Shared;

namespace SharedClassLibrary.ProxyRepository
{
    public class ShoppingCartProxyRepository : IShoppingCartRepository
    {
        private const string ApiBaseRoute = "api/shoppingcart";
        private readonly CustomHttpClient httpClient;

        public ShoppingCartProxyRepository(string baseApiUrl)
        {
            var _httpClient = new HttpClient();
            _httpClient.BaseAddress = new System.Uri(baseApiUrl);
            this.httpClient = new CustomHttpClient(_httpClient);
        }

        public async Task AddProductToCartAsync(int buyerId, int productId, int quantity)
        {
            var requestUri = $"{ApiBaseRoute}/{buyerId}/items?productId={productId}&quantity={quantity}";
            var response = await this.httpClient.PostAsync(requestUri, null);
            await this.ThrowOnError(nameof(AddProductToCartAsync), response);
        }

        public async Task ClearCartAsync(int buyerId)
        {
            var requestUri = $"{ApiBaseRoute}/{buyerId}/items";
            var response = await this.httpClient.DeleteAsync(requestUri);
            await this.ThrowOnError(nameof(ClearCartAsync), response);
        }

        public async Task<int> GetCartItemCountAsync(int buyerId)
        {
            var requestUri = $"{ApiBaseRoute}/{buyerId}/items/count";
            var response = await this.httpClient.GetAsync(requestUri);
            await this.ThrowOnError(nameof(GetCartItemCountAsync), response);
            return await response.Content.ReadFromJsonAsync<int>();
        }

        public async Task<List<Product>> GetCartItemsAsync(int buyerId)
        {
            var requestUri = $"{ApiBaseRoute}/{buyerId}/items";
            var response = await this.httpClient.GetAsync(requestUri);
            await this.ThrowOnError(nameof(GetCartItemsAsync), response);

            var result = await response.Content.ReadFromJsonAsync<List<Product>>();
            if (result == null)
            {
                result = new List<Product>();
            }

            return result;
        }

        public async Task<int> GetProductQuantityAsync(int buyerId, int productId)
        {
            var requestUri = $"{ApiBaseRoute}/{buyerId}/items/{productId}/quantity";
            var response = await this.httpClient.GetAsync(requestUri);
            await this.ThrowOnError(nameof(GetProductQuantityAsync), response);
            return await response.Content.ReadFromJsonAsync<int>();
        }

        public async Task<bool> IsProductInCartAsync(int buyerId, int productId)
        {
            var requestUri = $"{ApiBaseRoute}/{buyerId}/items/{productId}/exists";
            var response = await this.httpClient.GetAsync(requestUri);
            await this.ThrowOnError(nameof(IsProductInCartAsync), response);
            return await response.Content.ReadFromJsonAsync<bool>();
        }

        public async Task RemoveProductFromCartAsync(int buyerId, int productId)
        {
            var requestUri = $"{ApiBaseRoute}/{buyerId}/items/{productId}";
            var response = await this.httpClient.DeleteAsync(requestUri);
            await this.ThrowOnError(nameof(RemoveProductFromCartAsync), response);
        }

        public async Task UpdateProductQuantityAsync(int buyerId, int productId, int quantity)
        {
            var requestUri = $"{ApiBaseRoute}/{buyerId}/items/{productId}?quantity={quantity}";
            var response = await this.httpClient.PutAsync(requestUri, null);
            await this.ThrowOnError(nameof(UpdateProductQuantityAsync), response);
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
