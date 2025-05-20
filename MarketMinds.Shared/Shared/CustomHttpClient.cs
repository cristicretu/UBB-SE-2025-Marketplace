
namespace MarketMinds.Shared.Shared
{
    using System.Net.Http.Headers;
    using System.Text.Json;
    using System.Net.Http.Json;
    using System.Diagnostics;
    using MarketMinds.Shared.Helper;

    public class CustomHttpClient
    {
        private readonly HttpClient _httpClient;

        public CustomHttpClient(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        private async Task AddAuthHeaderAsync()
        {
            var token = AppConfig.AuthorizationToken;

            if (!string.IsNullOrWhiteSpace(token))
            {
                _httpClient.DefaultRequestHeaders.Authorization =
                    new AuthenticationHeaderValue("Bearer", token);
            }
        }

        public async Task<HttpResponseMessage> GetAsync(string url)
        {
            await AddAuthHeaderAsync();
            return await _httpClient.GetAsync(url);
        }

        public async Task<HttpResponseMessage> PostAsync(string url, HttpContent content)
        {
            await AddAuthHeaderAsync();
            return await _httpClient.PostAsync(url, content);
        }

        public async Task<HttpResponseMessage> PostAsJsonAsync<T>(string url, T content)
        {
            await AddAuthHeaderAsync();
            return await _httpClient.PostAsJsonAsync(url, content);
        }

        public async Task<HttpResponseMessage> PutAsync(string url, HttpContent content)
        {
            await AddAuthHeaderAsync();
            return await _httpClient.PutAsync(url, content);
        }

        public async Task<HttpResponseMessage> PutAsJsonAsync<T>(string url, T content)
        {
            await AddAuthHeaderAsync();
            return await _httpClient.PutAsJsonAsync(url, content);
        }

        public async Task<HttpResponseMessage> DeleteAsync(string url)
        {
            await AddAuthHeaderAsync();
            return await _httpClient.DeleteAsync(url);
        }
    }

}
