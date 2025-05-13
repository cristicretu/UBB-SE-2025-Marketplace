using MarketMinds.Shared.Models;
using MarketMinds.Shared.Services;
using NUnit.Framework;
using MarketMinds.Shared.Services.AuctionSortTypeConverterService;

namespace MarketMinds.Test.Services.AuctionSortTypeConverterServiceTest
{
    [TestFixture]
    public class AuctionSortTypeConverterServiceTest
    {
        private AuctionSortTypeConverterService _converter;

        // Sort tag constants
        private const string SELLER_RATING_ASC_TAG = "SellerRatingAsc";
        private const string SELLER_RATING_DESC_TAG = "SellerRatingDesc";
        private const string STARTING_PRICE_ASC_TAG = "StartingPriceAsc";
        private const string STARTING_PRICE_DESC_TAG = "StartingPriceDesc";
        private const string CURRENT_PRICE_ASC_TAG = "CurrentPriceAsc";
        private const string CURRENT_PRICE_DESC_TAG = "CurrentPriceDesc";
        private const string INVALID_TAG = "InvalidSortTag";
        private const string EMPTY_TAG = "";

        // Display title constants
        private const string SELLER_RATING_TITLE = "Seller Rating";
        private const string SELLER_RATING_FIELD = "SellerRating";
        private const string STARTING_PRICE_TITLE = "Starting Price";
        private const string STARTING_PRICE_FIELD = "StartingPrice";
        private const string CURRENT_PRICE_TITLE = "Current Price";
        private const string CURRENT_PRICE_FIELD = "CurrentPrice";

        [SetUp]
        public void Setup()
        {
            _converter = new AuctionSortTypeConverterService();
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

        #region StartingPrice Tests

        [Test]
        public void Convert_StartingPriceAsc_ReturnsNonNullResult()
        {
            var result = _converter.Convert(STARTING_PRICE_ASC_TAG);
            Assert.That(result, Is.Not.Null);
        }

        [Test]
        public void Convert_StartingPriceAsc_ReturnsCorrectExternalTitle()
        {
            var result = _converter.Convert(STARTING_PRICE_ASC_TAG);
            Assert.That(result.ExternalAttributeFieldTitle, Is.EqualTo(STARTING_PRICE_TITLE));
        }

        [Test]
        public void Convert_StartingPriceAsc_ReturnsCorrectInternalField()
        {
            var result = _converter.Convert(STARTING_PRICE_ASC_TAG);
            Assert.That(result.InternalAttributeFieldTitle, Is.EqualTo(STARTING_PRICE_FIELD));
        }

        [Test]
        public void Convert_StartingPriceAsc_ReturnsAscendingOrder()
        {
            var result = _converter.Convert(STARTING_PRICE_ASC_TAG);
            Assert.That(result.IsAscending, Is.True);
        }

        [Test]
        public void Convert_StartingPriceDesc_ReturnsNonNullResult()
        {
            var result = _converter.Convert(STARTING_PRICE_DESC_TAG);
            Assert.That(result, Is.Not.Null);
        }

        [Test]
        public void Convert_StartingPriceDesc_ReturnsCorrectExternalTitle()
        {
            var result = _converter.Convert(STARTING_PRICE_DESC_TAG);
            Assert.That(result.ExternalAttributeFieldTitle, Is.EqualTo(STARTING_PRICE_TITLE));
        }

        [Test]
        public void Convert_StartingPriceDesc_ReturnsCorrectInternalField()
        {
            var result = _converter.Convert(STARTING_PRICE_DESC_TAG);
            Assert.That(result.InternalAttributeFieldTitle, Is.EqualTo(STARTING_PRICE_FIELD));
        }

        [Test]
        public void Convert_StartingPriceDesc_ReturnsDescendingOrder()
        {
            var result = _converter.Convert(STARTING_PRICE_DESC_TAG);
            Assert.That(result.IsAscending, Is.False);
        }

        #endregion

        #region CurrentPrice Tests

        [Test]
        public void Convert_CurrentPriceAsc_ReturnsNonNullResult()
        {
            var result = _converter.Convert(CURRENT_PRICE_ASC_TAG);
            Assert.That(result, Is.Not.Null);
        }

        [Test]
        public void Convert_CurrentPriceAsc_ReturnsCorrectExternalTitle()
        {
            var result = _converter.Convert(CURRENT_PRICE_ASC_TAG);
            Assert.That(result.ExternalAttributeFieldTitle, Is.EqualTo(CURRENT_PRICE_TITLE));
        }

        [Test]
        public void Convert_CurrentPriceAsc_ReturnsCorrectInternalField()
        {
            var result = _converter.Convert(CURRENT_PRICE_ASC_TAG);
            Assert.That(result.InternalAttributeFieldTitle, Is.EqualTo(CURRENT_PRICE_FIELD));
        }

        [Test]
        public void Convert_CurrentPriceAsc_ReturnsAscendingOrder()
        {
            var result = _converter.Convert(CURRENT_PRICE_ASC_TAG);
            Assert.That(result.IsAscending, Is.True);
        }

        [Test]
        public void Convert_CurrentPriceDesc_ReturnsNonNullResult()
        {
            var result = _converter.Convert(CURRENT_PRICE_DESC_TAG);
            Assert.That(result, Is.Not.Null);
        }

        [Test]
        public void Convert_CurrentPriceDesc_ReturnsCorrectExternalTitle()
        {
            var result = _converter.Convert(CURRENT_PRICE_DESC_TAG);
            Assert.That(result.ExternalAttributeFieldTitle, Is.EqualTo(CURRENT_PRICE_TITLE));
        }

        [Test]
        public void Convert_CurrentPriceDesc_ReturnsCorrectInternalField()
        {
            var result = _converter.Convert(CURRENT_PRICE_DESC_TAG);
            Assert.That(result.InternalAttributeFieldTitle, Is.EqualTo(CURRENT_PRICE_FIELD));
        }

        [Test]
        public void Convert_CurrentPriceDesc_ReturnsDescendingOrder()
        {
            var result = _converter.Convert(CURRENT_PRICE_DESC_TAG);
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