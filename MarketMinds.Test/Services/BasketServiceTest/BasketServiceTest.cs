using System;
using MarketMinds.Shared.Models;
using MarketMinds.Shared.Services.BasketService;
using MarketMinds.Shared.IRepository;
using Xunit;
using Moq;
using System.Threading.Tasks;

namespace MarketMinds.Tests.Services
{
    public class BasketServiceTests
    {
        private readonly BasketService _basketService;
        private readonly Mock<IBasketRepository> _mockBasketRepository;
        private readonly BasketRepositoryMock _basketRepositoryMock;

        public BasketServiceTests()
        {
            // We'll use both the mock and the actual mock repository for different test scenarios
            _mockBasketRepository = new Mock<IBasketRepository>();
            _basketRepositoryMock = new BasketRepositoryMock();

            // Create service with the mock repository
            _basketService = new BasketService(_basketRepositoryMock);
        }

        #region GetBasket Tests

        [Fact]
        public void GetBasketByUserId_ValidUserId_ReturnsBasket()
        {
            // Arrange
            int userId = 1;

            // Act
            var result = _basketService.GetBasketByUserId(userId);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(userId, result.BuyerId);
            Assert.Empty(result.Items); // Should be empty initially
        }

        [Fact]
        public void GetBasketByUserId_InvalidUserId_ThrowsException()
        {
            // Arrange
            int invalidUserId = 0;

            // Act & Assert
            Assert.Throws<ArgumentException>(() => _basketService.GetBasketByUserId(invalidUserId));
        }

        [Fact]
        public void GetBasketByUser_ValidUser_ReturnsBasket()
        {
            // Arrange
            var user = new User { Id = 2 };

            // Act
            var result = _basketService.GetBasketByUser(user);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(user.Id, result.BuyerId);
        }

        #endregion

        #region Add Product Tests

        [Fact]
        public void AddProductToBasket_ValidInput_AddsItem()
        {
            // Arrange
            int userId = 1;
            int productId = 101;
            int quantity = 2;

            // Act
            _basketService.AddProductToBasket(userId, productId, quantity);
            var basket = _basketService.GetBasketByUserId(userId);

            // Assert
            Assert.Single(basket.Items);
            Assert.Equal(productId, basket.Items[0].ProductId);
            Assert.Equal(quantity, basket.Items[0].Quantity);
        }

        [Fact]
        public void AddProductToBasket_ExistingProduct_UpdatesQuantity()
        {
            // Arrange
            int userId = 1;
            int productId = 101;
            int initialQuantity = 2;
            int additionalQuantity = 3;

            // Add initial product
            _basketService.AddProductToBasket(userId, productId, initialQuantity);

            // Act
            _basketService.AddProductToBasket(userId, productId, additionalQuantity);
            var basket = _basketService.GetBasketByUserId(userId);

            // Assert
            Assert.Single(basket.Items);
            Assert.Equal(initialQuantity + additionalQuantity, basket.Items[0].Quantity);
        }

        [Fact]
        public void AddProductToBasket_ExceedsMaxQuantity_LimitsQuantity()
        {
            // Arrange
            int userId = 1;
            int productId = 101;
            int excessiveQuantity = BasketService.MAXIMUM_QUANTITY_PER_ITEM + 5;

            // Act
            _basketService.AddProductToBasket(userId, productId, excessiveQuantity);
            var basket = _basketService.GetBasketByUserId(userId);

            // Assert
            Assert.Equal(BasketService.MAXIMUM_QUANTITY_PER_ITEM, basket.Items[0].Quantity);
        }

        [Theory]
        [InlineData(0, 101, 1)] // invalid user
        [InlineData(1, 0, 1)]   // invalid product
        [InlineData(1, 101, -1)] // invalid quantity
        public void AddProductToBasket_InvalidInput_ThrowsException(int userId, int productId, int quantity)
        {
            // Act & Assert
            Assert.Throws<ArgumentException>(() =>
                _basketService.AddProductToBasket(userId, productId, quantity));
        }

        #endregion

        #region Remove Product Tests

        [Fact]
        public void RemoveProductFromBasket_ExistingProduct_RemovesItem()
        {
            // Arrange
            int userId = 1;
            int productId = 101;
            _basketService.AddProductToBasket(userId, productId, 1);

            // Act
            _basketService.RemoveProductFromBasket(userId, productId);
            var basket = _basketService.GetBasketByUserId(userId);

            // Assert
            Assert.Empty(basket.Items);
        }

        [Fact]
        public void RemoveProductFromBasket_NonExistingProduct_DoesNothing()
        {
            // Arrange
            int userId = 1;
            int existingProductId = 101;
            int nonExistingProductId = 102;
            _basketService.AddProductToBasket(userId, existingProductId, 1);

            // Act
            _basketService.RemoveProductFromBasket(userId, nonExistingProductId);
            var basket = _basketService.GetBasketByUserId(userId);

            // Assert
            Assert.Single(basket.Items); // Original item still there
        }

        #endregion

        #region Update Quantity Tests

        [Fact]
        public void UpdateProductQuantity_ValidInput_UpdatesQuantity()
        {
            // Arrange
            int userId = 1;
            int productId = 101;
            int initialQuantity = 1;
            int newQuantity = 5;
            _basketService.AddProductToBasket(userId, productId, initialQuantity);

            // Act
            _basketService.UpdateProductQuantity(userId, productId, newQuantity);
            var basket = _basketService.GetBasketByUserId(userId);

            // Assert
            Assert.Equal(newQuantity, basket.Items[0].Quantity);
        }

        [Fact]
        public void UpdateProductQuantity_ZeroQuantity_RemovesItem()
        {
            // Arrange
            int userId = 1;
            int productId = 101;
            _basketService.AddProductToBasket(userId, productId, 1);

            // Act
            _basketService.UpdateProductQuantity(userId, productId, 0);
            var basket = _basketService.GetBasketByUserId(userId);

            // Assert
            Assert.Empty(basket.Items);
        }

        [Fact]
        public void UpdateProductQuantity_ExceedsMaxQuantity_LimitsQuantity()
        {
            // Arrange
            int userId = 1;
            int productId = 101;
            _basketService.AddProductToBasket(userId, productId, 1);
            int excessiveQuantity = BasketService.MAXIMUM_QUANTITY_PER_ITEM + 5;

            // Act
            _basketService.UpdateProductQuantity(userId, productId, excessiveQuantity);
            var basket = _basketService.GetBasketByUserId(userId);

            // Assert
            Assert.Equal(BasketService.MAXIMUM_QUANTITY_PER_ITEM, basket.Items[0].Quantity);
        }

        #endregion

        #region Quantity Adjustment Tests

        [Fact]
        public void IncreaseProductQuantity_ValidInput_IncreasesQuantity()
        {
            // Arrange
            int userId = 1;
            int productId = 101;
            _basketService.AddProductToBasket(userId, productId, 1);

            // Act
            _basketService.IncreaseProductQuantity(userId, productId);
            var basket = _basketService.GetBasketByUserId(userId);

            // Assert
            Assert.Equal(2, basket.Items[0].Quantity);
        }

        [Fact]
        public void IncreaseProductQuantity_AtMaxQuantity_DoesNotIncrease()
        {
            // Arrange
            int userId = 1;
            int productId = 101;
            _basketService.AddProductToBasket(userId, productId, BasketService.MAXIMUM_QUANTITY_PER_ITEM);

            // Act
            _basketService.IncreaseProductQuantity(userId, productId);
            var basket = _basketService.GetBasketByUserId(userId);

            // Assert
            Assert.Equal(BasketService.MAXIMUM_QUANTITY_PER_ITEM, basket.Items[0].Quantity);
        }

        [Fact]
        public void DecreaseProductQuantity_ValidInput_DecreasesQuantity()
        {
            // Arrange
            int userId = 1;
            int productId = 101;
            _basketService.AddProductToBasket(userId, productId, 2);

            // Act
            _basketService.DecreaseProductQuantity(userId, productId);
            var basket = _basketService.GetBasketByUserId(userId);

            // Assert
            Assert.Equal(1, basket.Items[0].Quantity);
        }

        [Fact]
        public void DecreaseProductQuantity_AtOneQuantity_RemovesItem()
        {
            // Arrange
            int userId = 1;
            int productId = 101;
            _basketService.AddProductToBasket(userId, productId, 1);

            // Act
            _basketService.DecreaseProductQuantity(userId, productId);
            var basket = _basketService.GetBasketByUserId(userId);

            // Assert
            Assert.Empty(basket.Items);
        }

        #endregion

        #region Clear Basket Tests

        [Fact]
        public void ClearBasket_WithItems_RemovesAllItems()
        {
            // Arrange
            int userId = 1;
            _basketService.AddProductToBasket(userId, 101, 1);
            _basketService.AddProductToBasket(userId, 102, 2);

            // Act
            _basketService.ClearBasket(userId);
            var basket = _basketService.GetBasketByUserId(userId);

            // Assert
            Assert.Empty(basket.Items);
        }

        [Fact]
        public void ClearBasket_EmptyBasket_DoesNothing()
        {
            // Arrange
            int userId = 1;

            // Act
            _basketService.ClearBasket(userId);
            var basket = _basketService.GetBasketByUserId(userId);

            // Assert
            Assert.Empty(basket.Items);
        }

        #endregion

        #region Validation Tests

        [Fact]
        public void ValidateBasketBeforeCheckOut_ValidBasket_ReturnsTrue()
        {
            // Arrange
            int userId = 1;
            _basketService.AddProductToBasket(userId, 101, 1);
            var basket = _basketService.GetBasketByUserId(userId);

            // Act
            var isValid = _basketService.ValidateBasketBeforeCheckOut(basket.Id);

            // Assert
            Assert.True(isValid);
        }

        [Fact]
        public void ValidateBasketBeforeCheckOut_EmptyBasket_ReturnsFalse()
        {
            // Arrange
            int userId = 1;
            var basket = _basketService.GetBasketByUserId(userId);

            // Act
            var isValid = _basketService.ValidateBasketBeforeCheckOut(basket.Id);

            // Assert
            Assert.False(isValid);
        }

        [Fact]
        public void ValidateQuantityInput_ValidInput_ReturnsTrue()
        {
            // Arrange
            string validInput = "5";

            // Act
            var isValid = _basketService.ValidateQuantityInput(validInput, out int quantity);

            // Assert
            Assert.True(isValid);
            Assert.Equal(5, quantity);
        }

        [Theory]
        [InlineData("")]
        [InlineData("abc")]
        [InlineData("-5")]
        public void ValidateQuantityInput_InvalidInput_ReturnsFalse(string invalidInput)
        {
            // Act
            var isValid = _basketService.ValidateQuantityInput(invalidInput, out _);

            // Assert
            Assert.False(isValid);
        }

        #endregion

        #region Promo Code Tests

        [Theory]
        [InlineData("DISCOUNT10", 0.10)]
        [InlineData("WELCOME20", 0.20)]
        [InlineData("FLASH30", 0.30)]
        public void GetPromoCodeDiscountRate_ValidCode_ReturnsCorrectRate(string code, double expectedRate)
        {
            // Arrange
            int basketId = 1;

            // Act
            var rate = _basketService.GetPromoCodeDiscountRate(basketId, code);

            // Assert
            Assert.Equal(expectedRate, rate);
        }

        [Fact]
        public void GetPromoCodeDiscountRate_InvalidCode_ThrowsException()
        {
            // Arrange
            int basketId = 1;
            string invalidCode = "INVALID";

            // Act & Assert
            Assert.Throws<InvalidOperationException>(() =>
                _basketService.GetPromoCodeDiscountRate(basketId, invalidCode));
        }

        [Fact]
        public void GetPromoCodeDiscount_ValidCode_ReturnsCorrectDiscount()
        {
            // Arrange
            string code = "DISCOUNT10";
            double subtotal = 100.0;
            double expectedDiscount = 10.0;

            // Act
            var discount = _basketService.GetPromoCodeDiscount(code, subtotal);

            // Assert
            Assert.Equal(expectedDiscount, discount);
        }

        [Fact]
        public void ApplyPromoCode_ValidCode_DoesNotThrow()
        {
            // Arrange
            int basketId = 1;
            string validCode = "DISCOUNT10";

            // Act & Assert (should not throw)
            _basketService.ApplyPromoCode(basketId, validCode);
        }

        [Fact]
        public void ApplyPromoCode_InvalidCode_ThrowsException()
        {
            // Arrange
            int basketId = 1;
            string invalidCode = "INVALID";

            // Act & Assert
            Assert.Throws<InvalidOperationException>(() =>
                _basketService.ApplyPromoCode(basketId, invalidCode));
        }

        #endregion

        #region Basket Totals Tests

        [Fact]
        public void CalculateBasketTotals_NoPromoCode_ReturnsCorrectTotals()
        {
            // Arrange
            int userId = 1;
            _basketService.AddProductToBasket(userId, 101, 2); // Price is 10.0 in mock
            _basketService.AddProductToBasket(userId, 102, 1); // Price is 10.0 in mock
            var basket = _basketService.GetBasketByUserId(userId);

            // Act
            var totals = _basketService.CalculateBasketTotals(basket.Id, null);

            // Assert
            Assert.Equal(30.0, totals.Subtotal); // 2*10 + 1*10
            Assert.Equal(0.0, totals.Discount);
            Assert.Equal(30.0, totals.TotalAmount);
        }

        [Fact]
        public void CalculateBasketTotals_WithPromoCode_ReturnsCorrectTotals()
        {
            // Arrange
            int userId = 1;
            _basketService.AddProductToBasket(userId, 101, 2); // Price is 10.0 in mock
            var basket = _basketService.GetBasketByUserId(userId);
            string promoCode = "DISCOUNT10"; // 10% discount

            // Act
            var totals = _basketService.CalculateBasketTotals(basket.Id, promoCode);

            // Assert
            Assert.Equal(20.0, totals.Subtotal); // 2*10
            Assert.Equal(2.0, totals.Discount);  // 10% of 20
            Assert.Equal(18.0, totals.TotalAmount); // 20 - 2
        }

        #endregion

        #region Checkout Tests

        [Fact]
        public async Task CheckoutBasketAsync_ValidBasket_ReturnsTrue()
        {
            // Arrange
            int userId = 1;
            _basketService.AddProductToBasket(userId, 101, 1);
            var basket = _basketService.GetBasketByUserId(userId);

            // Act
            var result = await _basketService.CheckoutBasketAsync(userId, basket.Id);

            // Assert
            Assert.True(result);
        }

        [Fact]
        public async Task CheckoutBasketAsync_EmptyBasket_ThrowsException()
        {
            // Arrange
            int userId = 1;
            var basket = _basketService.GetBasketByUserId(userId);

            // Act & Assert
            await Assert.ThrowsAsync<InvalidOperationException>(() =>
                _basketService.CheckoutBasketAsync(userId, basket.Id));
        }

        #endregion

        #region DTO Tests

        [Fact]
        public void GetBasketDTOByUserId_ValidUser_ReturnsDTO()
        {
            // Arrange
            int userId = 1;
            _basketService.AddProductToBasket(userId, 101, 1);

            // Act
            var dto = _basketService.GetBasketDTOByUserId(userId);

            // Assert
            Assert.NotNull(dto);
            Assert.Equal(userId, dto.BuyerId);
            Assert.Single(dto.Items);
        }

        [Fact]
        public void GetBasketItemDTOs_ValidBasket_ReturnsDTOs()
        {
            // Arrange
            int userId = 1;
            _basketService.AddProductToBasket(userId, 101, 1);
            var basket = _basketService.GetBasketByUserId(userId);

            // Act
            var dtos = _basketService.GetBasketItemDTOs(basket.Id);

            // Assert
            Assert.Single(dtos);
            Assert.Equal(101, dtos[0].ProductId);
        }

        #endregion
    }
}