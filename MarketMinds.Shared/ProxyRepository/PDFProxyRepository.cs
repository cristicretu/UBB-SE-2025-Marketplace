﻿// <copyright file="PDFProxyRepository.cs" company="PlaceholderCompany">
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
    /// Proxy repository class for managing PDF operations via REST API.
    /// </summary>
    public class PDFProxyRepository : IPDFRepository
    {
        private const string ApiBaseRoute = "api/pdfs";
        private readonly HttpClient httpClient;

        /// <summary>
        /// Initializes a new instance of the <see cref="PDFProxyRepository"/> class.
        /// </summary>
        /// <param name="baseApiUrl">The base url of the API.</param>
        public PDFProxyRepository(string baseApiUrl)
        {
            this.httpClient = new HttpClient();
            this.httpClient.BaseAddress = new System.Uri(baseApiUrl);

        }

        /// <inheritdoc />
        public async Task<int> InsertPdfAsync(byte[] fileBytes)
        {
            if (fileBytes == null || fileBytes.Length == 0)
            {
                throw new ArgumentException("PDF file bytes cannot be null or empty.", nameof(fileBytes));
            }

            var response = await this.httpClient.PostAsJsonAsync($"{ApiBaseRoute}", fileBytes);
            await this.ThrowOnError(nameof(InsertPdfAsync), response);

            var pdfId = await response.Content.ReadFromJsonAsync<int>();
            return pdfId;
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
