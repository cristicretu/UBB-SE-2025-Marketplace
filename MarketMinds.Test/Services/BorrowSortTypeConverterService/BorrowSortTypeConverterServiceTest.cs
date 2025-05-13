using MarketMinds.Shared.Models;
using MarketMinds.Shared.Services;
using NUnit.Framework;
using MarketMinds.Shared.Services.BorrowSortTypeConverterService;

namespace MarketMinds.Test.Services
{
    [TestFixture]
    public class BorrowSortTypeConverterServiceTest
    {
        private BorrowSortTypeConverterService _converter;

        // Sort tag constants
        private const string SELLER_RATING_ASC_TAG = "SellerRatingAsc";
        private const string SELLER_RATING_DESC_TAG = "SellerRatingDesc";
        private const string DAILY_RATE_ASC_TAG = "DailyRateAsc";
        private const string DAILY_RATE_DESC_TAG = "DailyRateDesc";
        private const string START_DATE_ASC_TAG = "StartDateAsc";
        private const string START_DATE_DESC_TAG = "StartDateDesc";
        private const string INVALID_TAG = "InvalidSortTag";
        private const string EMPTY_TAG = "";

        // Display title constants
        private const string SELLER_RATING_TITLE = "Seller Rating";
        private const string SELLER_RATING_FIELD = "SellerRating";
        private const string DAILY_RATE_TITLE = "Daily Rate";
        private const string DAILY_RATE_FIELD = "DailyRate";
        private const string START_DATE_TITLE = "Start Date";
        private const string START_DATE_FIELD = "StartDate";

        [SetUp]
        public void Setup()
        {
            _converter = new BorrowSortTypeConverterService();
        }

        #region SellerRating Tests

        [Test]
        public void Convert_SellerRatingAsc_ReturnsNonNullResult()
        {
            var result = _converter.Convert(SELLER_RATING_ASC_TAG);
            Assert.That(result, Is.Not.Null);
        }

        [Test]
        public void Convert_SellerRatingAsc_ReturnsCorrectExternalTitle()
        {
            var result = _converter.Convert(SELLER_RATING_ASC_TAG);
            Assert.That(result.ExternalAttributeFieldTitle, Is.EqualTo(SELLER_RATING_TITLE));
        }

        [Test]
        public void Convert_SellerRatingAsc_ReturnsCorrectInternalField()
        {
            var result = _converter.Convert(SELLER_RATING_ASC_TAG);
            Assert.That(result.InternalAttributeFieldTitle, Is.EqualTo(SELLER_RATING_FIELD));
        }

        [Test]
        public void Convert_SellerRatingAsc_ReturnsAscendingOrder()
        {
            var result = _converter.Convert(SELLER_RATING_ASC_TAG);
            Assert.That(result.IsAscending, Is.True);
        }

        [Test]
        public void Convert_SellerRatingDesc_ReturnsNonNullResult()
        {
            var result = _converter.Convert(SELLER_RATING_DESC_TAG);
            Assert.That(result, Is.Not.Null);
        }

        [Test]
        public void Convert_SellerRatingDesc_ReturnsCorrectExternalTitle()
        {
            var result = _converter.Convert(SELLER_RATING_DESC_TAG);
            Assert.That(result.ExternalAttributeFieldTitle, Is.EqualTo(SELLER_RATING_TITLE));
        }

        [Test]
        public void Convert_SellerRatingDesc_ReturnsCorrectInternalField()
        {
            var result = _converter.Convert(SELLER_RATING_DESC_TAG);
            Assert.That(result.InternalAttributeFieldTitle, Is.EqualTo(SELLER_RATING_FIELD));
        }

        [Test]
        public void Convert_SellerRatingDesc_ReturnsDescendingOrder()
        {
            var result = _converter.Convert(SELLER_RATING_DESC_TAG);
            Assert.That(result.IsAscending, Is.False);
        }

        #endregion

        #region DailyRate Tests

        [Test]
        public void Convert_DailyRateAsc_ReturnsNonNullResult()
        {
            var result = _converter.Convert(DAILY_RATE_ASC_TAG);
            Assert.That(result, Is.Not.Null);
        }

        [Test]
        public void Convert_DailyRateAsc_ReturnsCorrectExternalTitle()
        {
            var result = _converter.Convert(DAILY_RATE_ASC_TAG);
            Assert.That(result.ExternalAttributeFieldTitle, Is.EqualTo(DAILY_RATE_TITLE));
        }

        [Test]
        public void Convert_DailyRateAsc_ReturnsCorrectInternalField()
        {
            var result = _converter.Convert(DAILY_RATE_ASC_TAG);
            Assert.That(result.InternalAttributeFieldTitle, Is.EqualTo(DAILY_RATE_FIELD));
        }

        [Test]
        public void Convert_DailyRateAsc_ReturnsAscendingOrder()
        {
            var result = _converter.Convert(DAILY_RATE_ASC_TAG);
            Assert.That(result.IsAscending, Is.True);
        }

        [Test]
        public void Convert_DailyRateDesc_ReturnsNonNullResult()
        {
            var result = _converter.Convert(DAILY_RATE_DESC_TAG);
            Assert.That(result, Is.Not.Null);
        }

        [Test]
        public void Convert_DailyRateDesc_ReturnsCorrectExternalTitle()
        {
            var result = _converter.Convert(DAILY_RATE_DESC_TAG);
            Assert.That(result.ExternalAttributeFieldTitle, Is.EqualTo(DAILY_RATE_TITLE));
        }

        [Test]
        public void Convert_DailyRateDesc_ReturnsCorrectInternalField()
        {
            var result = _converter.Convert(DAILY_RATE_DESC_TAG);
            Assert.That(result.InternalAttributeFieldTitle, Is.EqualTo(DAILY_RATE_FIELD));
        }

        [Test]
        public void Convert_DailyRateDesc_ReturnsDescendingOrder()
        {
            var result = _converter.Convert(DAILY_RATE_DESC_TAG);
            Assert.That(result.IsAscending, Is.False);
        }

        #endregion

        #region StartDate Tests

        [Test]
        public void Convert_StartDateAsc_ReturnsNonNullResult()
        {
            var result = _converter.Convert(START_DATE_ASC_TAG);
            Assert.That(result, Is.Not.Null);
        }

        [Test]
        public void Convert_StartDateAsc_ReturnsCorrectExternalTitle()
        {
            var result = _converter.Convert(START_DATE_ASC_TAG);
            Assert.That(result.ExternalAttributeFieldTitle, Is.EqualTo(START_DATE_TITLE));
        }

        [Test]
        public void Convert_StartDateAsc_ReturnsCorrectInternalField()
        {
            var result = _converter.Convert(START_DATE_ASC_TAG);
            Assert.That(result.InternalAttributeFieldTitle, Is.EqualTo(START_DATE_FIELD));
        }

        [Test]
        public void Convert_StartDateAsc_ReturnsAscendingOrder()
        {
            var result = _converter.Convert(START_DATE_ASC_TAG);
            Assert.That(result.IsAscending, Is.True);
        }

        [Test]
        public void Convert_StartDateDesc_ReturnsNonNullResult()
        {
            var result = _converter.Convert(START_DATE_DESC_TAG);
            Assert.That(result, Is.Not.Null);
        }

        [Test]
        public void Convert_StartDateDesc_ReturnsCorrectExternalTitle()
        {
            var result = _converter.Convert(START_DATE_DESC_TAG);
            Assert.That(result.ExternalAttributeFieldTitle, Is.EqualTo(START_DATE_TITLE));
        }

        [Test]
        public void Convert_StartDateDesc_ReturnsCorrectInternalField()
        {
            var result = _converter.Convert(START_DATE_DESC_TAG);
            Assert.That(result.InternalAttributeFieldTitle, Is.EqualTo(START_DATE_FIELD));
        }

        [Test]
        public void Convert_StartDateDesc_ReturnsDescendingOrder()
        {
            var result = _converter.Convert(START_DATE_DESC_TAG);
            Assert.That(result.IsAscending, Is.False);
        }

        #endregion

        #region Invalid Input Tests

        [Test]
        public void Convert_InvalidSortTag_ReturnsNull()
        {
            var result = _converter.Convert(INVALID_TAG);
            Assert.That(result, Is.Null);
        }

        [Test]
        public void Convert_EmptyString_ReturnsNull()
        {
            var result = _converter.Convert(EMPTY_TAG);
            Assert.That(result, Is.Null);
        }

        [Test]
        public void Convert_NullString_ReturnsNull()
        {
            var result = _converter.Convert(null);
            Assert.That(result, Is.Null);
        }

        #endregion
    }
}
