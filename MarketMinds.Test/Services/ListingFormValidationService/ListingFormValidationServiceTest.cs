using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using NUnit.Framework;
using MarketMinds.Shared.Models;
using MarketMinds.Shared.Services;
using MarketMinds.Shared.Services.ListingFormValidationService;

namespace MarketMinds.Test.Services
{
    [TestFixture]
    public class ListingFormValidationServiceTest
    {
        // Constants to replace magic strings and values
        private const int CATEGORY_ID = 1;
        private const string CATEGORY_TITLE = "Electronics";
        private const string CATEGORY_DESCRIPTION = "Electronic devices";
        private const int CONDITION_ID = 1;
        private const string CONDITION_TITLE = "New";
        private const string CONDITION_DESCRIPTION = "Brand new item";
        private const string TAG1 = "tag1";
        private const string TAG2 = "tag2";
        private const string VALID_TITLE = "Test Product";
        private const string VALID_DESCRIPTION = "This is a test product description";

        private const string EMPTY_STRING = "";
        private const string WHITESPACE_TITLE = "   ";
        private const string INVALID_PRICE_TEXT = "not a price";
        private const string NEGATIVE_PRICE_TEXT = "-50.00";
        private const string VALID_PRICE_TEXT = "99.99";

        private const string VALID_DAILY_RATE_PRICE = "25.50";
        private const string VALID_STARTING_TIME_PRICE = "50.00";
        private const string TITLE_ERROR_MESSAGE = "Title cannot be empty.";
        private const string CATEGORY_ERROR_MESSAGE = "Please select a category.";
        private const string DESCRIPTION_ERROR_MESSAGE = "Description cannot be empty.";
        private const string TAGS_ERROR_MESSAGE = "Please add at least one tag.";
        private const string CONDITION_ERROR_MESSAGE = "Please select a condition.";
        private const string TITLE_FIELD = "Title";
        private const string CATEGORY_FIELD = "Category";
        private const string DESCRIPTION_FIELD = "Description";
        private const string TAGS_FIELD = "Tags";

        private const string CONDITION_FIELD = "Condition";
        private const float VALID_PRICE = 99.99f;
        private const float NEGATIVE_PRICE = -50.0f;
        private const float ZERO_PRICE = 0f;
        private const float VALID_DAILY_RATE = 25.5f;
        private const float VALID_STARTING_PRICE = 50.0f;
        private const int FUTURE_DAYS_NORMAL = 30;
        private const int FUTURE_DAYS_SHORT = 7;
        private const int PAST_DAY_LONG = -30;
        private const int PAST_DAYS_SORT = -7;

        private IListingFormValidationService _validationService;
        private Category _validCategory;
        private Condition _validCondition;
        private ObservableCollection<string> _validTags;

        [SetUp]
        public void Setup()
        {
            _validationService = new ListingFormValidationService();
            _validCategory = new Category(CATEGORY_ID, CATEGORY_TITLE, CATEGORY_DESCRIPTION);
            _validCondition = new Condition(CONDITION_ID, CONDITION_TITLE, CONDITION_DESCRIPTION);
            _validTags = new ObservableCollection<string> { TAG1, TAG2 };
        }

        #region ValidateCommonFields - Success Cases

        [Test]
        public void ValidateCommonFields_WithValidInputs_ReturnsTrue()
        {
            // Act
            bool result = _validationService.ValidateCommonFields(
                VALID_TITLE, _validCategory, VALID_DESCRIPTION, _validTags, _validCondition,
                out string errorMessage, out string errorField);

            // Assert
            Assert.That(result, Is.True);
        }

        [Test]
        public void ValidateCommonFields_WithValidInputs_SetsEmptyErrorMessage()
        {
            // Act
            _validationService.ValidateCommonFields(
                VALID_TITLE, _validCategory, VALID_DESCRIPTION, _validTags, _validCondition,
                out string errorMessage, out string errorField);

            // Assert
            Assert.That(errorMessage, Is.Empty);
        }

        [Test]
        public void ValidateCommonFields_WithValidInputs_SetsEmptyErrorField()
        {
            // Act
            _validationService.ValidateCommonFields(
                VALID_TITLE, _validCategory, VALID_DESCRIPTION, _validTags, _validCondition,
                out string errorMessage, out string errorField);

            // Assert
            Assert.That(errorField, Is.Empty);
        }

        #endregion

        #region ValidateCommonFields - Title Validation

        [Test]
        public void ValidateCommonFields_WithEmptyTitle_ReturnsFalse()
        {
            // Act
            bool result = _validationService.ValidateCommonFields(
                EMPTY_STRING, _validCategory, VALID_DESCRIPTION, _validTags, _validCondition,
                out string errorMessage, out string errorField);

            // Assert
            Assert.That(result, Is.False);
        }

        [Test]
        public void ValidateCommonFields_WithEmptyTitle_SetsCorrectErrorMessage()
        {
            // Act
            _validationService.ValidateCommonFields(
                EMPTY_STRING, _validCategory, VALID_DESCRIPTION, _validTags, _validCondition,
                out string errorMessage, out string errorField);

            // Assert
            Assert.That(errorMessage, Is.EqualTo(TITLE_ERROR_MESSAGE));
        }

        [Test]
        public void ValidateCommonFields_WithEmptyTitle_SetsCorrectErrorField()
        {
            // Act
            _validationService.ValidateCommonFields(
                EMPTY_STRING, _validCategory, VALID_DESCRIPTION, _validTags, _validCondition,
                out string errorMessage, out string errorField);

            // Assert
            Assert.That(errorField, Is.EqualTo(TITLE_FIELD));
        }

        [Test]
        public void ValidateCommonFields_WithWhitespaceTitle_ReturnsFalse()
        {
            // Act
            bool result = _validationService.ValidateCommonFields(
                WHITESPACE_TITLE, _validCategory, VALID_DESCRIPTION, _validTags, _validCondition,
                out string errorMessage, out string errorField);

            // Assert
            Assert.That(result, Is.False);
        }

        [Test]
        public void ValidateCommonFields_WithWhitespaceTitle_SetsCorrectErrorMessage()
        {
            // Act
            _validationService.ValidateCommonFields(
                WHITESPACE_TITLE, _validCategory, VALID_DESCRIPTION, _validTags, _validCondition,
                out string errorMessage, out string errorField);

            // Assert
            Assert.That(errorMessage, Is.EqualTo(TITLE_ERROR_MESSAGE));
        }

        [Test]
        public void ValidateCommonFields_WithWhitespaceTitle_SetsCorrectErrorField()
        {
            // Act
            _validationService.ValidateCommonFields(
                WHITESPACE_TITLE, _validCategory, VALID_DESCRIPTION, _validTags, _validCondition,
                out string errorMessage, out string errorField);

            // Assert
            Assert.That(errorField, Is.EqualTo(TITLE_FIELD));
        }

        #endregion

        #region ValidateCommonFields - Category Validation

        [Test]
        public void ValidateCommonFields_WithNullCategory_ReturnsFalse()
        {
            // Act
            bool result = _validationService.ValidateCommonFields(
                VALID_TITLE, null, VALID_DESCRIPTION, _validTags, _validCondition,
                out string errorMessage, out string errorField);

            // Assert
            Assert.That(result, Is.False);
        }

        [Test]
        public void ValidateCommonFields_WithNullCategory_SetsCorrectErrorMessage()
        {
            // Act
            _validationService.ValidateCommonFields(
                VALID_TITLE, null, VALID_DESCRIPTION, _validTags, _validCondition,
                out string errorMessage, out string errorField);

            // Assert
            Assert.That(errorMessage, Is.EqualTo(CATEGORY_ERROR_MESSAGE));
        }

        [Test]
        public void ValidateCommonFields_WithNullCategory_SetsCorrectErrorField()
        {
            // Act
            _validationService.ValidateCommonFields(
                VALID_TITLE, null, VALID_DESCRIPTION, _validTags, _validCondition,
                out string errorMessage, out string errorField);

            // Assert
            Assert.That(errorField, Is.EqualTo(CATEGORY_FIELD));
        }

        #endregion

        #region ValidateCommonFields - Description Validation

        [Test]
        public void ValidateCommonFields_WithEmptyDescription_ReturnsFalse()
        {
            // Act
            bool result = _validationService.ValidateCommonFields(
                VALID_TITLE, _validCategory, EMPTY_STRING, _validTags, _validCondition,
                out string errorMessage, out string errorField);

            // Assert
            Assert.That(result, Is.False);
        }

        [Test]
        public void ValidateCommonFields_WithEmptyDescription_SetsCorrectErrorMessage()
        {
            // Act
            _validationService.ValidateCommonFields(
                VALID_TITLE, _validCategory, EMPTY_STRING, _validTags, _validCondition,
                out string errorMessage, out string errorField);

            // Assert
            Assert.That(errorMessage, Is.EqualTo(DESCRIPTION_ERROR_MESSAGE));
        }

        [Test]
        public void ValidateCommonFields_WithEmptyDescription_SetsCorrectErrorField()
        {
            // Act
            _validationService.ValidateCommonFields(
                VALID_TITLE, _validCategory, EMPTY_STRING, _validTags, _validCondition,
                out string errorMessage, out string errorField);

            // Assert
            Assert.That(errorField, Is.EqualTo(DESCRIPTION_FIELD));
        }

        #endregion

        #region ValidateCommonFields - Tags Validation

        [Test]
        public void ValidateCommonFields_WithNullTags_ReturnsFalse()
        {
            // Act
            bool result = _validationService.ValidateCommonFields(
                VALID_TITLE, _validCategory, VALID_DESCRIPTION, null, _validCondition,
                out string errorMessage, out string errorField);

            // Assert
            Assert.That(result, Is.False);
        }

        [Test]
        public void ValidateCommonFields_WithNullTags_SetsCorrectErrorMessage()
        {
            // Act
            _validationService.ValidateCommonFields(
                VALID_TITLE, _validCategory, VALID_DESCRIPTION, null, _validCondition,
                out string errorMessage, out string errorField);

            // Assert
            Assert.That(errorMessage, Is.EqualTo(TAGS_ERROR_MESSAGE));
        }

        [Test]
        public void ValidateCommonFields_WithNullTags_SetsCorrectErrorField()
        {
            // Act
            _validationService.ValidateCommonFields(
                VALID_TITLE, _validCategory, VALID_DESCRIPTION, null, _validCondition,
                out string errorMessage, out string errorField);

            // Assert
            Assert.That(errorField, Is.EqualTo(TAGS_FIELD));
        }

        [Test]
        public void ValidateCommonFields_WithEmptyTags_ReturnsFalse()
        {
            // Arrange
            ObservableCollection<string> emptyTags = new ObservableCollection<string>();

            // Act
            bool result = _validationService.ValidateCommonFields(
                VALID_TITLE, _validCategory, VALID_DESCRIPTION, emptyTags, _validCondition,
                out string errorMessage, out string errorField);

            // Assert
            Assert.That(result, Is.False);
        }

        [Test]
        public void ValidateCommonFields_WithEmptyTags_SetsCorrectErrorMessage()
        {
            // Arrange
            ObservableCollection<string> emptyTags = new ObservableCollection<string>();

            // Act
            _validationService.ValidateCommonFields(
                VALID_TITLE, _validCategory, VALID_DESCRIPTION, emptyTags, _validCondition,
                out string errorMessage, out string errorField);

            // Assert
            Assert.That(errorMessage, Is.EqualTo(TAGS_ERROR_MESSAGE));
        }

        [Test]
        public void ValidateCommonFields_WithEmptyTags_SetsCorrectErrorField()
        {
            // Arrange
            ObservableCollection<string> emptyTags = new ObservableCollection<string>();

            // Act
            _validationService.ValidateCommonFields(
                VALID_TITLE, _validCategory, VALID_DESCRIPTION, emptyTags, _validCondition,
                out string errorMessage, out string errorField);

            // Assert
            Assert.That(errorField, Is.EqualTo(TAGS_FIELD));
        }

        #endregion

        #region ValidateCommonFields - Condition Validation

        [Test]
        public void ValidateCommonFields_WithNullCondition_ReturnsFalse()
        {
            // Act
            bool result = _validationService.ValidateCommonFields(
                VALID_TITLE, _validCategory, VALID_DESCRIPTION, _validTags, null,
                out string errorMessage, out string errorField);

            // Assert
            Assert.That(result, Is.False);
        }

        [Test]
        public void ValidateCommonFields_WithNullCondition_SetsCorrectErrorMessage()
        {
            // Act
            _validationService.ValidateCommonFields(
                VALID_TITLE, _validCategory, VALID_DESCRIPTION, _validTags, null,
                out string errorMessage, out string errorField);

            // Assert
            Assert.That(errorMessage, Is.EqualTo(CONDITION_ERROR_MESSAGE));
        }

        [Test]
        public void ValidateCommonFields_WithNullCondition_SetsCorrectErrorField()
        {
            // Act
            _validationService.ValidateCommonFields(
                VALID_TITLE, _validCategory, VALID_DESCRIPTION, _validTags, null,
                out string errorMessage, out string errorField);

            // Assert
            Assert.That(errorField, Is.EqualTo(CONDITION_FIELD));
        }

        #endregion

        #region ValidateBuyProductFields

        [Test]
        public void ValidateBuyProductFields_WithValidPrice_ReturnsTrue()
        {
            // Act
            bool result = _validationService.ValidateBuyProductFields(VALID_PRICE_TEXT, out float price);

            // Assert
            Assert.That(result, Is.True);
        }

        [Test]
        public void ValidateBuyProductFields_WithValidPrice_SetsCorrectPrice()
        {
            // Act
            _validationService.ValidateBuyProductFields(VALID_PRICE_TEXT, out float price);

            // Assert
            Assert.That(price, Is.EqualTo(VALID_PRICE));
        }

        [Test]
        public void ValidateBuyProductFields_WithInvalidPrice_ReturnsFalse()
        {
            // Act
            bool result = _validationService.ValidateBuyProductFields(INVALID_PRICE_TEXT, out float price);

            // Assert
            Assert.That(result, Is.False);
        }

        [Test]
        public void ValidateBuyProductFields_WithInvalidPrice_SetsZeroPrice()
        {
            // Act
            _validationService.ValidateBuyProductFields(INVALID_PRICE_TEXT, out float price);

            // Assert
            Assert.That(price, Is.EqualTo(ZERO_PRICE));
        }

        [Test]
        public void ValidateBuyProductFields_WithNegativePrice_ReturnsTrue()
        {
            // This test identifies a potential issue in the validation logic
            // Act
            bool result = _validationService.ValidateBuyProductFields(NEGATIVE_PRICE_TEXT, out float price);

            // Assert
            Assert.That(result, Is.True);
        }

        [Test]
        public void ValidateBuyProductFields_WithNegativePrice_SetsNegativePrice()
        {
            // Act
            _validationService.ValidateBuyProductFields(NEGATIVE_PRICE_TEXT, out float price);

            // Assert
            Assert.That(price, Is.EqualTo(NEGATIVE_PRICE));
        }

        #endregion

        #region ValidateBorrowProductFields

        [Test]
        public void ValidateBorrowProductFields_WithValidInputs_ReturnsTrue()
        {
            // Arrange
            DateTimeOffset timeLimit = DateTimeOffset.Now.AddDays(FUTURE_DAYS_NORMAL);

            // Act
            bool result = _validationService.ValidateBorrowProductFields(VALID_DAILY_RATE_PRICE, timeLimit, out float dailyRate);

            // Assert
            Assert.That(result, Is.True);
        }

        [Test]
        public void ValidateBorrowProductFields_WithValidInputs_SetsCorrectDailyRate()
        {
            // Arrange
            DateTimeOffset timeLimit = DateTimeOffset.Now.AddDays(FUTURE_DAYS_NORMAL);

            // Act
            _validationService.ValidateBorrowProductFields(VALID_DAILY_RATE_PRICE, timeLimit, out float dailyRate);

            // Assert
            Assert.That(dailyRate, Is.EqualTo(VALID_DAILY_RATE));
        }

        [Test]
        public void ValidateBorrowProductFields_WithInvalidDailyRate_ReturnsFalse()
        {
            // Arrange
            DateTimeOffset timeLimit = DateTimeOffset.Now.AddDays(FUTURE_DAYS_NORMAL);

            // Act
            bool result = _validationService.ValidateBorrowProductFields(INVALID_PRICE_TEXT, timeLimit, out float dailyRate);

            // Assert
            Assert.That(result, Is.False);
        }

        [Test]
        public void ValidateBorrowProductFields_WithInvalidDailyRate_SetsZeroDailyRate()
        {
            // Arrange
            DateTimeOffset timeLimit = DateTimeOffset.Now.AddDays(FUTURE_DAYS_NORMAL);

            // Act
            _validationService.ValidateBorrowProductFields(INVALID_PRICE_TEXT, timeLimit, out float dailyRate);

            // Assert
            Assert.That(dailyRate, Is.EqualTo(ZERO_PRICE));
        }

        [Test]
        public void ValidateBorrowProductFields_WithPastTimeLimit_ReturnsFalse()
        {
            // Arrange
            DateTimeOffset timeLimit = DateTimeOffset.Now.AddDays(PAST_DAY_LONG);

            // Act
            bool result = _validationService.ValidateBorrowProductFields(VALID_DAILY_RATE_PRICE, timeLimit, out float dailyRate);

            // Assert
            Assert.That(result, Is.False);
        }

        [Test]
        public void ValidateBorrowProductFields_WithPastTimeLimit_SetsCorrectDailyRate()
        {
            // Arrange
            DateTimeOffset timeLimit = DateTimeOffset.Now.AddDays(PAST_DAY_LONG);

            // Act
            _validationService.ValidateBorrowProductFields(VALID_DAILY_RATE_PRICE, timeLimit, out float dailyRate);

            // Assert
            Assert.That(dailyRate, Is.EqualTo(VALID_DAILY_RATE));
        }

        [Test]
        public void ValidateBorrowProductFields_WithNullTimeLimit_ReturnsFalse()
        {
            // Act
            bool result = _validationService.ValidateBorrowProductFields(VALID_DAILY_RATE_PRICE, null, out float dailyRate);

            // Assert
            Assert.That(result, Is.False);
        }

        [Test]
        public void ValidateBorrowProductFields_WithNullTimeLimit_SetsCorrectDailyRate()
        {
            // Act
            _validationService.ValidateBorrowProductFields(VALID_DAILY_RATE_PRICE, null, out float dailyRate);

            // Assert
            Assert.That(dailyRate, Is.EqualTo(VALID_DAILY_RATE));
        }

        #endregion

        #region ValidateAuctionProductFields

        [Test]
        public void ValidateAuctionProductFields_WithValidInputs_ReturnsTrue()
        {
            // Arrange
            DateTimeOffset endAuctionDate = DateTimeOffset.Now.AddDays(FUTURE_DAYS_SHORT);

            // Act
            bool result = _validationService.ValidateAuctionProductFields(VALID_STARTING_TIME_PRICE, endAuctionDate, out float startingPrice);

            // Assert
            Assert.That(result, Is.True);
        }

        [Test]
        public void ValidateAuctionProductFields_WithValidInputs_SetsCorrectStartingPrice()
        {
            // Arrange
            DateTimeOffset endAuctionDate = DateTimeOffset.Now.AddDays(FUTURE_DAYS_SHORT);

            // Act
            _validationService.ValidateAuctionProductFields(VALID_STARTING_TIME_PRICE, endAuctionDate, out float startingPrice);

            // Assert
            Assert.That(startingPrice, Is.EqualTo(VALID_STARTING_PRICE));
        }

        [Test]
        public void ValidateAuctionProductFields_WithInvalidStartingPrice_ReturnsFalse()
        {
            // Arrange
            DateTimeOffset endAuctionDate = DateTimeOffset.Now.AddDays(FUTURE_DAYS_SHORT);

            // Act
            bool result = _validationService.ValidateAuctionProductFields(INVALID_PRICE_TEXT, endAuctionDate, out float startingPrice);

            // Assert
            Assert.That(result, Is.False);
        }

        [Test]
        public void ValidateAuctionProductFields_WithInvalidStartingPrice_SetsZeroStartingPrice()
        {
            // Arrange
            DateTimeOffset endAuctionDate = DateTimeOffset.Now.AddDays(FUTURE_DAYS_SHORT);

            // Act
            _validationService.ValidateAuctionProductFields(INVALID_PRICE_TEXT, endAuctionDate, out float startingPrice);

            // Assert
            Assert.That(startingPrice, Is.EqualTo(ZERO_PRICE));
        }

        [Test]
        public void ValidateAuctionProductFields_WithPastEndDate_ReturnsFalse()
        {
            // Arrange
            DateTimeOffset endAuctionDate = DateTimeOffset.Now.AddDays(PAST_DAYS_SORT);

            // Act
            bool result = _validationService.ValidateAuctionProductFields(VALID_STARTING_TIME_PRICE, endAuctionDate, out float startingPrice);

            // Assert
            Assert.That(result, Is.False);
        }

        [Test]
        public void ValidateAuctionProductFields_WithPastEndDate_SetsCorrectStartingPrice()
        {
            // Arrange
            DateTimeOffset endAuctionDate = DateTimeOffset.Now.AddDays(PAST_DAYS_SORT);

            // Act
            _validationService.ValidateAuctionProductFields(VALID_STARTING_TIME_PRICE, endAuctionDate, out float startingPrice);

            // Assert
            Assert.That(startingPrice, Is.EqualTo(VALID_STARTING_PRICE));
        }

        #endregion
    }
}

