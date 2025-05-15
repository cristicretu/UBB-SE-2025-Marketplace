using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Threading.Tasks;
using SharedClassLibrary.Domain;
using MarketPlace924.ViewModel;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;

namespace MarketPlace924.View
{
    [ExcludeFromCodeCoverage]
    public sealed partial class BillingInfo : Page
    {
        /// <summary>
        /// The view model for the BillingInfo page.
        /// </summary>
        private IBillingInfoViewModel viewModel;

        public BillingInfo(int orderHistoryID)
        {
            this.InitializeComponent();
            viewModel = new BillingInfoViewModel(orderHistoryID);

            DataContext = viewModel;
        }

        /// <summary>
        /// Sets the cart items for checkout.
        /// </summary>
        /// <param name="cartItems">The list of products and quantities.</param>
        public void SetCartItems(List<Product> cartItems)
        {
            if (this.DataContext is BillingInfoViewModel viewModel)
            {
                viewModel.SetCartItems(cartItems);
            }
        }

        /// <summary>
        /// Sets the cart total for the order.
        /// </summary>
        /// <param name="total">The total price of the cart.</param>
        public void SetCartTotal(double total)
        {
            if (this.DataContext is BillingInfoViewModel viewModel)
            {
                viewModel.SetCartTotal(total);
            }
        }

        /// <summary>
        /// Sets the buyer ID for the order.
        /// </summary>
        /// <param name="buyerId">The ID of the buyer.</param>
        public void SetBuyerId(int buyerId)
        {
            if (this.DataContext is BillingInfoViewModel viewModel)
            {
                viewModel.SetBuyerId(buyerId);
            }
        }

        /// <summary>
        /// Handles the click event for the finalize button.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private async void OnFinalizeButtonClickedAsync(object sender, RoutedEventArgs e)
        {
            if (DataContext is BillingInfoViewModel viewModel)
            {
                await viewModel.OnFinalizeButtonClickedAsync();
            }
        }

        /// <summary>
        /// Handles the click event for the cancel button.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnStartDateChanged(DatePicker sender, DatePickerSelectedValueChangedEventArgs e)
        {
            viewModel.UpdateStartDate(sender.Date);
        }

        /// <summary>
        /// Handles the click event for the end date.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private async void OnEndDateChanged(DatePicker sender, DatePickerSelectedValueChangedEventArgs e)
        {
            viewModel.UpdateEndDate(sender.Date);
            await UpdateBorrowedProductTax(sender);
        }

        /// <summary>
        /// Handles the click event for the tax date.
        /// </summary>
        /// <param name="sender"></param>
        /// <returns></returns>
        private async Task UpdateBorrowedProductTax(DatePicker sender)
        {
            if (DataContext is BillingInfoViewModel viewModel && sender.DataContext is Product product)
            {
                await viewModel.ApplyBorrowedTax(product);
            }
        }
    }
}
