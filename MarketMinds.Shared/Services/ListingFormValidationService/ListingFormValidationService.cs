using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using MarketMinds.Shared.Models;

namespace MarketMinds.Shared.Services.ListingFormValidationService
{
    public class ListingFormValidationService : IListingFormValidationService
    {
        public bool ValidateCommonFields(string title, Category category, string description, ObservableCollection<string> tags, Condition condition, out string errorMessage, out string errorField)
        {
            if (string.IsNullOrWhiteSpace(title))
            {
                errorMessage = "Title cannot be empty.";
                errorField = "Title";
                return false;
            }

            if (category == null)
            {
                errorMessage = "Please select a category.";
                errorField = "Category";
                return false;
            }

            if (string.IsNullOrWhiteSpace(description))
            {
                errorMessage = "Description cannot be empty.";
                errorField = "Description";
                return false;
            }

            if (tags == null || tags.Count == 0)
            {
                errorMessage = "Please add at least one tag.";
                errorField = "Tags";
                return false;
            }

            if (condition == null)
            {
                errorMessage = "Please select a condition.";
                errorField = "Condition";
                return false;
            }

            errorMessage = string.Empty;
            errorField = string.Empty;
            return true;
        }

        public bool ValidateBuyProductFields(string priceText, out float price)
        {
            return float.TryParse(priceText, out price);
        }

        public bool ValidateBorrowProductFields(string dailyRateText, DateTimeOffset? timeLimit, out float dailyRate)
        {
            if (!float.TryParse(dailyRateText, out dailyRate))
            {
                return false;
            }

            return timeLimit.HasValue && timeLimit.Value > DateTimeOffset.Now;
        }

        public bool ValidateAuctionProductFields(string startingPriceText, DateTimeOffset? endAuctionDate, out float startingPrice)
        {
            if (!float.TryParse(startingPriceText, out startingPrice))
            {
                return false;
            }

            return endAuctionDate.HasValue && endAuctionDate.Value > DateTimeOffset.Now;
        }
    }
}