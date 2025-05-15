// <copyright file="PdfApiController.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

namespace Server.Controllers
{
    using System;
    using System.Threading.Tasks;
    using Microsoft.AspNetCore.Authorization;
    using Microsoft.AspNetCore.Mvc;
    using SharedClassLibrary.IRepository;

    /// <summary>
    /// API controller for managing PDF data.
    /// </summary>
    [Authorize]
    [Route("api/pdfs")]
    [ApiController]
    public class PdfApiController : ControllerBase
    {
        private readonly IPDFRepository pdfRepository;

        /// <summary>
        /// Initializes a new instance of the <see cref="PdfApiController"/> class.
        /// </summary>
        /// <param name="pdfRepository">The PDF repository dependency.</param>
        public PdfApiController(IPDFRepository pdfRepository)
        {
            this.pdfRepository = pdfRepository;
        }

        /// <summary>
        /// Asynchronously inserts a new PDF file.
        /// </summary>
        /// <param name="fileBytes">The byte array of the PDF file.</param>
        /// <returns>The ID of the newly inserted PDF.</returns>
        [HttpPost]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(string), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(string), StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<int>> InsertPdf([FromBody] byte[] fileBytes)
        {
            if (fileBytes == null || fileBytes.Length == 0)
            {
                return this.BadRequest("PDF file bytes are required.");
            }

            try
            {
                var pdfId = await this.pdfRepository.InsertPdfAsync(fileBytes);
                return this.Ok(pdfId);
            }
            catch (Exception ex)
            {
                return this.StatusCode(StatusCodes.Status500InternalServerError, $"An error occurred while inserting PDF: {ex.Message}");
            }
        }
    }
}
