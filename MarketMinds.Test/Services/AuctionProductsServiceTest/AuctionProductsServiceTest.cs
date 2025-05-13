//using System;
//using System.Collections.Generic;
//using Xunit;
//using MarketMinds.Shared.Models;
//using MarketMinds.Shared.Services.AuctionProductsService;
//using MarketMinds.Test.Services.Mocks;
//using NUnit.Framework;


//public class AuctionProductsServiceTests
//{
//    private readonly AuctionProductsService service;
//    private readonly AuctionProductsRepositoryMock mockRepository;

//    public AuctionProductsServiceTests()
//    {
//        mockRepository = new AuctionProductsRepositoryMock();
//        service = new AuctionProductsService(mockRepository);
//    }

//    [Fact]
//    public void CreateListing_ShouldAssignIdAndAddProduct()
//    {
//        var product = new AuctionProduct
//        {
//            Title = "Vintage Watch",
//            StartPrice = 100,
//            CurrentPrice = 150,
//            Condition = new Condition { Id = 1 },
//            Category = new Category { Id = 1 },
//            Tags = new List<ProductTag>()
//        };

//        service.CreateListing(product);

//        var storedProduct = mockRepository.GetProductById(1);

//        Xunit.Assert.Equal("Vintage Watch", storedProduct.Title);
//    }

//    [Fact]
//    public void PlaceBid_ShouldUpdateAuctionWithNewBid()
//    {
//        const double startingBid = 100;
//        const double newBid = 120;

//        var auction = new AuctionProduct
//        {
//            Id = 1,
//            Title = "Rare Coin",
//            StartPrice = startingBid,
//            CurrentPrice = startingBid,
//            StartTime = DateTime.Now.AddMinutes(-10),
//            EndTime = DateTime.Now.AddDays(1),
//            SellerId = 100,
//            Bids = new List<Bid>(),
//        };

//        var bidder = new User { Id = 200, Balance = 500 };

//        service.CreateListing(auction);

//        service.PlaceBid(auction, bidder, newBid);

//        var updatedAuction = mockRepository.GetProductById(1);

//        Xunit.Assert.Equal(newBid, updatedAuction.CurrentPrice);
//    }

//    [Fact]
//    public void ConcludeAuction_ShouldRemoveProduct()
//    {
//        var auction = new AuctionProduct
//        {
//            Id = 1,
//            Title = "Painting",
//            StartPrice = 50,
//            CurrentPrice = 60,
//            SellerId = 10,
//            StartTime = DateTime.Now.AddMinutes(-10),
//            EndTime = DateTime.Now.AddMinutes(10)
//        };

//        service.CreateListing(auction);

//        service.ConcludeAuction(auction);

//        var result = mockRepository.GetProductById(1);

//        Xunit.Assert.Null(result);
//    }

//    [Fact]
//    public void GetTimeLeft_ShouldReturnAuctionEnded_WhenAuctionIsOver()
//    {
//        var auction = new AuctionProduct
//        {
//            Id = 1,
//            Title = "Old Camera",
//            StartTime = DateTime.Now.AddDays(-10),
//            EndTime = DateTime.Now.AddDays(-1),
//        };

//        var result = service.GetTimeLeft(auction);

//        Xunit.Assert.Equal("Auction Ended", result);
//    }

//    [Fact]
//    public void IsAuctionEnded_ShouldReturnTrue_WhenPastEndTime()
//    {
//        var auction = new AuctionProduct
//        {
//            Id = 1,
//            EndTime = DateTime.Now.AddMinutes(-1),
//        };

//        bool hasEnded = service.IsAuctionEnded(auction);

//        Xunit.Assert.True(hasEnded);
//    }

//    [Fact]
//    public void GetProductById_ShouldThrow_WhenProductNotFound()
//    {
//        int nonexistentProductId = 999;

//        var exception = Xunit.Assert.Throws<KeyNotFoundException>(() => service.GetProductById(nonexistentProductId));

//        Xunit.Assert.Equal($"Auction product with ID {nonexistentProductId} not found: Auction product with ID {nonexistentProductId} not found.", exception.Message);
//    }

//    [Fact]
//    public void GetProducts_ShouldReturnListOfProducts()
//    {
//        var product = new AuctionProduct
//        {
//            Id = 1,
//            Title = "Book",
//            StartPrice = 20,
//            CurrentPrice = 25,
//            StartTime = DateTime.Now.AddMinutes(-5),
//            EndTime = DateTime.Now.AddMinutes(5)
//        };

//        service.CreateListing(product);

//        var products = service.GetProducts();

//        Xunit.Assert.Single(products);
//    }
//}