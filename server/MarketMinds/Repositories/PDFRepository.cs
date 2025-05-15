// <copyright file="PDFRepository.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

namespace Server.Repository
{
    using global::MarketMinds.Shared.IRepository;
    using global::MarketMinds.Shared.Models;
    using Server.DataAccessLayer;
    using System.Threading.Tasks;

    /// <summary>
    /// Represents a repository for PDF operations.
    /// </summary>
    public class PDFRepository : IPDFRepository
    {
        private readonly ApplicationDbContext dbContext;

        /// <summary>
        /// Initializes a new instance of the <see cref="PDFRepository"/> class.
        /// </summary>
        /// <param name="dbContext">The database context.</param>
        public PDFRepository(ApplicationDbContext dbContext)
        {
            this.dbContext = dbContext;
        }

        /// <summary>
        /// Asynchronously inserts a PDF into the database.
        /// </summary>
        /// <param name="fileBytes">The byte array representing the PDF file.</param>
        /// <returns>The ID of the inserted PDF.</returns>
        public async Task<int> InsertPdfAsync(byte[] fileBytes)
        {
            PDF pdfToInsert = new PDF
            {
                ContractID = 0, // initialize with 0, anyway we don't use this in the database (ignored this field in the table creation, but kept it in order to prevent errors)
                PdfID = 0, // initialize with 0, will be set by the database
                File = fileBytes,
            };

            await this.dbContext.PDFs.AddAsync(pdfToInsert);
            await this.dbContext.SaveChangesAsync();

            return pdfToInsert.PdfID; // return the PDF ID that was set by the database
        }
    }
}
