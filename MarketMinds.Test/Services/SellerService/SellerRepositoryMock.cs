using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MarketMinds.Shared.IRepository;
using MarketMinds.Shared.Models;

namespace MarketMinds.Tests.Mocks
{
    public class MockSellerRepository : ISellerRepository
    {
        // backing stores
        public List<Review> Reviews { get; } = new();
        public List<BuyProduct> Products { get; } = new();
        public List<(int SellerId, int FollowerCount, string Message)> NotificationsAdded { get; }
            = new();
        public Dictionary<int, int> LastFollowerCounts { get; } = new();
        public List<string> NotificationsToReturn { get; } = new();
        public Seller SellerInfoToReturn { get; set; }
        public List<(int SellerId, Seller Seller)> UpdatedSellers { get; } = new();
        public List<(int SellerId, double TrustScore)> UpdatedTrustScores { get; } = new();

        // interface impl
        public Task AddNewFollowerNotification(int sellerId, int currentFollowerCount, string message)
        {
            NotificationsAdded.Add((sellerId, currentFollowerCount, message));
            return Task.CompletedTask;
        }

        public Task AddSeller(Seller seller) => throw new NotImplementedException();

        public Task<int> GetLastFollowerCount(int sellerId)
        {
            return Task.FromResult(LastFollowerCounts.TryGetValue(sellerId, out var cnt) ? cnt : 0);
        }

        public Task<List<string>> GetNotifications(int sellerId, int maxNotifications)
        {
            return Task.FromResult(NotificationsToReturn.Take(maxNotifications).ToList());
        }

        public Task<List<BuyProduct>> GetProducts(int sellerId)
        {
            return Task.FromResult(Products.Where(p => p.SellerId == sellerId).ToList());
        }

        public Task<List<Review>> GetReviews(int sellerId)
        {
            return Task.FromResult(Reviews.Where(r => r.SellerId == sellerId).ToList());
        }

        public Task<Seller> GetSellerInfo(User user)
        {
            return Task.FromResult(SellerInfoToReturn);
        }

        public Task UpdateSeller(Seller seller)
        {
            UpdatedSellers.Add((seller.Id, seller));
            return Task.CompletedTask;
        }

        public Task UpdateTrustScore(int sellerId, double trustScore)
        {
            UpdatedTrustScores.Add((sellerId, trustScore));
            return Task.CompletedTask;
        }
    }
}
