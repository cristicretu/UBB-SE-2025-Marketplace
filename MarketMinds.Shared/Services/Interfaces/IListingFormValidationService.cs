using System;
using System.Collections.ObjectModel;
using MarketMinds.Shared.Models;

namespace MarketMinds.Shared.Services.ListingFormValidationService
{
    /// <summary>
    /// Interface for ListingFormValidationService to manage validation of listing forms.
    /// </summary>
    public interface IListingFormValidationService
    {
        /// <summary>
        /// Validates common fields for a listing form.
        /// </summary>
        /// <param name="title">The title of the listing.</param>
        /// <param name="category">The category of the product.</param>
        /// <param name="description">The description of the product.</param>
        /// <param name="tags">The tags associated with the product.</param>
        /// <param name="condition">The condition of the product.</param>
        /// <param name="errorMessage">The error message if validation fails.</param>
        /// <param name="errorField">The field that caused the validation error.</param>
        /// <returns>True if validation passes, otherwise false.</returns>
        bool ValidateCommonFields(string title, Category category, string description, ObservableCollection<string> tags, Condition condition, out string errorMessage, out string errorField);

        /// <summary>
        /// Validates fields specific to a "Buy" product listing.
        /// </summary>
        /// <param name="priceText">The price of the product as a string.</param>
        /// <param name="price">The parsed price of the product.</param>
        /// <returns>True if validation passes, otherwise false.</returns>
        bool ValidateBuyProductFields(string priceText, out float price);

        /// <summary>
        /// Validates fields specific to a "Borrow" product listing.
        /// </summary>
        /// <param name="dailyRateText">The daily rate as a string.</param>
        /// <param name="timeLimit">The time limit for borrowing.</param>
        /// <param name="dailyRate">The parsed daily rate.</param>
        /// <returns>True if validation passes, otherwise false.</returns>
        bool ValidateBorrowProductFields(string dailyRateText, DateTimeOffset? timeLimit, out float dailyRate);

        /// <summary>
        /// Validates fields specific to an "Auction" product listing.
        /// </summary>
        /// <param name="startingPriceText">The starting price as a string.</param>
        /// <param name="endAuctionDate">The end date of the auction.</param>
        /// <param name="startingPrice">The parsed starting price.</param>
        /// <returns>True if validation passes, otherwise false.</returns>
        bool ValidateAuctionProductFields(string startingPriceText, DateTimeOffset? endAuctionDate, out float startingPrice);
    }
}
