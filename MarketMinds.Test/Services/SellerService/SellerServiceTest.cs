using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Xunit;
using MarketMinds.Shared.Models;
using MarketMinds.Shared.Services;
using MarketMinds.Tests.Mocks;

namespace MarketMinds.Tests.Services
{
    public class SellerServiceTests
    {
        private readonly MockSellerRepository _repo;
        private readonly SellerService _service;

        public SellerServiceTests()
        {
            _repo = new MockSellerRepository();
            _service = new SellerService(_repo);
        }

        [Fact]
        public async Task CalculateAverageReviewScore_NoReviews_ReturnsZero()
        {
            var avg = await _service.CalculateAverageReviewScore(1);
            Assert.Equal(0, avg);
        }

        [Fact]
        public async Task CalculateAverageReviewScore_WithReviews_ComputesCorrectAverage()
        {
            _repo.Reviews.AddRange(new[]
            {
                new Review { SellerId = 2, Rating = 4 },
                new Review { SellerId = 2, Rating = 2 },
                new Review { SellerId = 2, Rating = 5 }
            });

            var avg = await _service.CalculateAverageReviewScore(2);
            Assert.Equal((4 + 2 + 5) / 3.0, avg);
        }

        [Fact]
        public async Task GetAllProducts_ReturnsOnlyThatSeller()
        {
            _repo.Products.AddRange(new[]
            {
                new BuyProduct { Id = 1, SellerId = 10 },
                new BuyProduct { Id = 2, SellerId = 11 },
                new BuyProduct { Id = 3, SellerId = 10 }
            });

            var list = await _service.GetAllProducts(10);
            Assert.Equal(2, list.Count);
            Assert.All(list, p => Assert.Equal(10, p.SellerId));
        }

        [Fact]
        public async Task GenerateFollowersChangedNotification_Increase_AddsNotification()
        {
            _repo.LastFollowerCounts[5] = 3;
            await _service.GenerateFollowersChangedNotification(5, 4);

            Assert.Single(_repo.NotificationsAdded);
            var note = _repo.NotificationsAdded[0];
            Assert.Equal(5, note.SellerId);
            Assert.Contains("gained", note.Message);
        }

        [Fact]
        public async Task GenerateFollowersChangedNotification_Decrease_AddsNotification()
        {
            _repo.LastFollowerCounts[6] = 10;
            await _service.GenerateFollowersChangedNotification(6, 9);

            Assert.Single(_repo.NotificationsAdded);
            Assert.Contains("lost", _repo.NotificationsAdded[0].Message);
        }

        [Fact]
        public async Task GenerateFollowersChangedNotification_NoChange_DoesNotAdd()
        {
            _repo.LastFollowerCounts[7] = 8;
            await _service.GenerateFollowersChangedNotification(7, 8);

            Assert.Empty(_repo.NotificationsAdded);
        }

        [Fact]
        public async Task GetNotifications_ReturnsLimitedList()
        {
            _repo.NotificationsToReturn.AddRange(new[] { "A", "B", "C" });
            var list = await _service.GetNotifications(1, 2);
            Assert.Equal(2, list.Count);
            Assert.Equal(new List<string> { "A", "B" }, list);
        }

        [Fact]
        public async Task GetSellerByUser_NullUser_ReturnsNull()
        {
            var seller = await _service.GetSellerByUser(null);
            Assert.Null(seller);
        }

        [Fact]
        public async Task GetSellerByUser_RepoReturnsNull_CreatesNewSeller()
        {
            _repo.SellerInfoToReturn = null;
            var user = new User { Id = 99, Username = "u" };
            var seller = await _service.GetSellerByUser(user);

            Assert.NotNull(seller);
            Assert.Equal(user, seller.User);
        }

        [Fact]
        public async Task GetSellerByUser_RepoReturnsSeller_SetsUserIfMissing()
        {
            var s = new Seller(new User { Id = 1, Username = "x" }) { Id = 42, StoreName = "S" };
            s.User = null;
            _repo.SellerInfoToReturn = s;

            var user = new User { Id = 1, Username = "x" };
            var result = await _service.GetSellerByUser(user);

            Assert.Equal(42, result.Id);
            Assert.Equal(user, result.User);
        }

        [Fact]
        public async Task UpdateSeller_NullSeller_Throws()
        {
            await Assert.ThrowsAsync<ArgumentNullException>(() => _service.UpdateSeller(null));
        }

        [Fact]
        public async Task UpdateSeller_InvalidId_Throws()
        {
            var bad = new Seller(new User()) { Id = 0 };
            await Assert.ThrowsAsync<ArgumentException>(() => _service.UpdateSeller(bad));
        }

        [Fact]
        public async Task UpdateSeller_Valid_UpdatesRepoAndTrustScore()
        {
            _repo.Reviews.Add(new Review { SellerId = 8, Rating = 5 });
            var seller = new Seller(new User()) { Id = 8 };

            await _service.UpdateSeller(seller);

            Assert.Single(_repo.UpdatedSellers);
            Assert.Equal(8, _repo.UpdatedSellers[0].SellerId);

            Assert.Single(_repo.UpdatedTrustScores);
            Assert.Equal((8, 5.0), _repo.UpdatedTrustScores[0]);
        }
    }
}
