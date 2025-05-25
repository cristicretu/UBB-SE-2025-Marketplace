using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Threading.Tasks;
using MarketMinds.Shared.Models;

namespace MarketMinds.ViewModels
{
    public interface IBillingInfoViewModel
    {
        string AdditionalInfo { get; set; }
        string Address { get; set; }
        double DeliveryFee { get; set; }
        string Email { get; set; }
        DateTimeOffset EndDate { get; set; }
        string ErrorMessage { get; set; }
        string FullName { get; set; }
        bool IsCardEnabled { get; set; }
        bool IsCashEnabled { get; set; }
        bool IsProcessing { get; set; }
        bool IsWalletEnabled { get; set; }
        int OrderHistoryId { get; set; }
        string PhoneNumber { get; set; }
        ObservableCollection<Product> ProductList { get; set; }
        string SelectedPaymentMethod { get; set; }
        DateTimeOffset StartDate { get; set; }
        double Subtotal { get; set; }
        double Total { get; set; }
        double WarrantyTax { get; set; }
        string ZipCode { get; set; }

        event PropertyChangedEventHandler PropertyChanged;

        Task ApplyBorrowedTax(BorrowProduct borrowProduct);
        void CalculateOrderTotal();
        void CalculateOrderTotal(int orderHistoryID);
        Task<List<Product>> GetProductsFromOrderHistoryAsync(int orderHistoryID);
        Task InitializeViewModelAsync();
        Task OnFinalizeButtonClickedAsync();
        Task OpenNextWindowAsync(string selectedPaymentMethod);
        Task ProcessWalletRefillAsync();
        void SetBuyerId(int buyerId);
        void SetCartItems(List<Product> cartItems);
        void SetCartTotal(double total);
        void UpdateEndDate(DateTimeOffset date);
        void UpdateStartDate(DateTimeOffset date);
    }
}