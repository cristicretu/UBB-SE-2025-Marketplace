// <copyright file="BuyerWishlistItemDetailsProvider.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

namespace MarketPlace924.ViewModel
{
    using System.Collections.Generic;

    /// <summary>
    /// Provider class for loading wishlist item details from mock data.
    /// </summary>
    public class BuyerWishlistItemDetailsProvider : IBuyerWishlistItemDetailsProvider
    {
        // TODO: Reimplement this using ProductService when available
        private readonly Dictionary<int, IBuyerWishlistItemViewModel> mockProductDetails =
            new()
            {
            {
                1, new BuyerWishlistItemViewModel
                {
                    Title = "Abstract Painting",
                    Description = "Contemporary abstract art",
                    ImageSource = "ms-appx:///Assets/Products/painting.png",
                    Price = 350,
                }
            },
            {
                2, new BuyerWishlistItemViewModel
                {
                    Title = "Wooden Sculpture",
                    Description = "Modern wooden sculpture",
                    ImageSource = "ms-appx:///Assets/Products/smart-tv.png",
                    Price = 275,
                }
            },
            {
                3, new BuyerWishlistItemViewModel
                {
                    Title = "Wireless Noise-Canceling Headphones",
                    Description = "Over-ear headphones with active noise canceling and 40-hour battery life.",
                    ImageSource = "ms-appx:///Assets/Products/headphones.png",
                    Price = 8000,
                }
            },
            {
                4, new BuyerWishlistItemViewModel
                {
                    Title = "Wooden Sculpture",
                    Description = "Modern wooden sculpture",
                    ImageSource = "ms-appx:///Assets/Products/wood.png",
                    Price = 275,
                }
            },
            {
                5, new BuyerWishlistItemViewModel
                {
                    Title = "Leather Wallet",
                    Description = "Sleek and stylish wallet with RFID protection for added security.",
                    ImageSource = "ms-appx:///Assets/Products/wallet.png",
                    Price = 3000,
                }
            },
            {
                6, new BuyerWishlistItemViewModel
                {
                    Title = "Mechanical Keyboard",
                    Description = "Premium mechanical keyboard with customizable RGB lighting and fast switches.",
                    ImageSource = "ms-appx:///Assets/Products/keyboard.png",
                    Price = 9000,
                }
            },
            {
                7, new BuyerWishlistItemViewModel
                {
                    Title = "4K Ultra HD Smart TV - 55 inch",
                    Description = "Stunning 4K resolution with HDR support and built-in streaming apps.",
                    ImageSource = "ms-appx:///Assets/Products/smart-tv.png",
                    Price = 50000,
                }
            },
            {
                8, new BuyerWishlistItemViewModel
                {
                    Title = "Premium Leather Office Chair",
                    Description = "Ergonomic and adjustable, designed for maximum comfort during long work hours.",
                    ImageSource = "ms-appx:///Assets/Products/office-chair.png",
                    Price = 25000,
                }
            },
            {
                9, new BuyerWishlistItemViewModel
                {
                    Title = "Electric Standing Desk",
                    Description = "Height-adjustable standing desk with smooth motorized lift system.",
                    ImageSource = "ms-appx:///Assets/Products/standing-desk.png",
                    Price = 45000,
                }
            },
            {
                10, new BuyerWishlistItemViewModel
                {
                    Title = "Professional DSLR Camera",
                    Description = "24.2 MP full-frame DSLR with 4K video recording and Wi-Fi connectivity.",
                    ImageSource = "ms-appx:///Assets/Products/dslr-camera.png",
                    Price = 150000,
                }
            },
            {
                11, new BuyerWishlistItemViewModel
                {
                    Title = "Bluetooth Portable Speaker",
                    Description = "Waterproof, 20-hour battery life, and deep bass for outdoor use.",
                    ImageSource = "ms-appx:///Assets/Products/bluetooth-speaker.png",
                    Price = 5000,
                }
            },
            {
                12, new BuyerWishlistItemViewModel
                {
                    Title = "Electric Toothbrush",
                    Description = "Smart toothbrush with pressure sensor and multiple cleaning modes.",
                    ImageSource = "ms-appx:///Assets/Products/electric-toothbrush.png",
                    Price = 6000,
                }
            },
            {
                13, new BuyerWishlistItemViewModel
                {
                    Title = "Air Purifier with HEPA Filter",
                    Description = "Removes dust, pollen, and smoke for cleaner indoor air.",
                    ImageSource = "ms-appx:///Assets/Products/air-purifier.png",
                    Price = 18000,
                }
            },
            {
                14, new BuyerWishlistItemViewModel
                {
                    Title = "Smart LED Light Bulbs (Pack of 4)",
                    Description = "Voice-controlled, dimmable, and color-changing smart bulbs.",
                    ImageSource = "ms-appx:///Assets/Products/smart-bulbs.png",
                    Price = 7000,
                }
            },
            {
                15, new BuyerWishlistItemViewModel
                {
                    Title = "Coffee Machine - Espresso & Cappuccino Maker",
                    Description = "Fully automatic coffee maker with milk frother and grinder.",
                    ImageSource = "ms-appx:///Assets/Products/coffee-machine.png",
                    Price = 35000,
                }
            },
            {
                16, new BuyerWishlistItemViewModel
                {
                    Title = "Robot Vacuum Cleaner",
                    Description = "Self-charging vacuum with smart mapping and app control.",
                    ImageSource = "ms-appx:///Assets/Products/robot-vacuum.png",
                    Price = 40000,
                }
            },
            {
                17, new BuyerWishlistItemViewModel
                {
                    Title = "Smart Home Security Camera",
                    Description = "1080p HD camera with motion detection and night vision.",
                    ImageSource = "ms-appx:///Assets/Products/security-camera.png",
                    Price = 12000,
                }
            },
            {
                18, new BuyerWishlistItemViewModel
                {
                    Title = "Wireless Ergonomic Mouse",
                    Description = "Comfortable grip, adjustable DPI, and silent clicks for productivity.",
                    ImageSource = "ms-appx:///Assets/Products/mouse.png",
                    Price = 3000,
                }
            },
            };

        /// <inheritdoc/>
        public IBuyerWishlistItemViewModel LoadWishlistItemDetails(IBuyerWishlistItemViewModel item)
        {
            var existingItem = this.mockProductDetails[item.ProductId];
            if (this.mockProductDetails.ContainsKey(item.ProductId))
            {
                item.Title = existingItem.Title;
                item.Description = existingItem.Description;
                item.Price = existingItem.Price;
                item.ImageSource = existingItem.ImageSource;
            }

            return item;
        }
    }
}