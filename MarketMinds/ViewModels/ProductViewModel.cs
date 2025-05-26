using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Diagnostics;
using System.Threading.Tasks;
using MarketMinds.Shared.Models;
using MarketMinds.Shared.Services;
using MarketMinds.Shared.Services.BorrowProductsService;
using MarketMinds.Shared.Services.Interfaces;
using MarketMinds.Shared.Services.BuyProductsService;
using MarketMinds.Views;
using Microsoft.UI.Xaml;
using MarketMinds.Shared.Helper;

namespace MarketMinds.ViewModels
{
    public class ProductViewModel : INotifyPropertyChanged
    {
        private Product product;
        private int quantity;

        public ProductViewModel(Product product, int quantity)
        {
            this.product = product;
            this.quantity = quantity;
        }

        // Forward Product properties
        public int Id => product.Id;
        public string Title => product.Title;
        public double Price => product.Price;
        public int SellerId => product.SellerId;

        // Add reference to original product
        public Product Product => product;

        // Add quantity property
        public int Quantity
        {
            get => quantity;
            set
            {
                if (quantity != value)
                {
                    quantity = value;
                    OnPropertyChanged();
                }
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
