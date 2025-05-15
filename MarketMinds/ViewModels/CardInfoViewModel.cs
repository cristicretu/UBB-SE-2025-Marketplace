using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using System.Threading.Tasks;
using SharedClassLibrary.Domain;
using SharedClassLibrary.ProxyRepository;
using SharedClassLibrary.Shared;
using SharedClassLibrary.IRepository;
using SharedClassLibrary.Helper;

namespace MarketPlace924.ViewModel
{
    /// <summary>
    /// Represents the view model for card payment information and handles card payment processing.
    /// </summary>
    public class CardInfoViewModel : ICardInfoViewModel, INotifyPropertyChanged
    {
        private readonly IOrderHistoryRepository orderHistoryModel;
        private readonly IOrderSummaryRepository orderSummaryModel;
        private readonly IOrderRepository orderModel;
        private readonly IDummyCardRepository dummyCardModel;

        private int orderHistoryID;

        private double subtotal;
        private double deliveryFee;
        private double total;

        private string email;
        private string cardholder;
        private string cardnumber;
        private string cardMonth;
        private string cardYear;
        private string cardCVC;
        public ObservableCollection<Product> ProductList { get; set; }
        public List<Product> Products;
        public CardInfoViewModel(
            IOrderHistoryRepository orderHistoryModel,
            IOrderSummaryRepository orderSummaryModel,
            IOrderRepository orderModel,
            IDummyCardRepository dummyCardModel,
            int orderHistoryID)
        {
            if (orderHistoryModel == null)
            {
                throw new ArgumentNullException(nameof(orderHistoryModel));
            }
            if (orderSummaryModel == null)
            {
                throw new ArgumentNullException(nameof(orderSummaryModel));
            }
            if (orderModel == null)
            {
                throw new ArgumentNullException(nameof(orderModel));
            }
            if (dummyCardModel == null)
            {
                throw new ArgumentNullException(nameof(dummyCardModel));
            }

            this.orderHistoryModel = orderHistoryModel;
            this.orderSummaryModel = orderSummaryModel;
            this.orderModel = orderModel;
            this.dummyCardModel = dummyCardModel;
            this.orderHistoryID = orderHistoryID;

            _ = InitializeViewModelAsync();
        }

        [ExcludeFromCodeCoverage]
        /// <summary>
        /// Initializes a new instance of the <see cref="CardInfoViewModel"/> class and begins loading order history details.
        /// </summary>
        /// <param name="orderHistoryID">The unique identifier of the order history.</param>
        public CardInfoViewModel(int orderHistoryID)
        {
            orderHistoryModel = new OrderHistoryProxyRepository(AppConfig.GetBaseApiUrl());
            orderModel = new OrderProxyRepository(AppConfig.GetBaseApiUrl());
            orderSummaryModel = new OrderSummaryProxyRepository(AppConfig.GetBaseApiUrl());
            dummyCardModel = new DummyCardProxyRepository(AppConfig.GetBaseApiUrl());

            this.orderHistoryID = orderHistoryID;

            _ = InitializeViewModelAsync();
        }

        /// <summary>
        /// Asynchronously initializes the card info view model by loading dummy products and order summary details.
        /// </summary>
        /// <returns>A task representing the asynchronous operation.</returns>
        public async Task InitializeViewModelAsync()
        {
            Products = await GetProductsFromOrderHistoryAsync(orderHistoryID);
            ProductList = new ObservableCollection<Product>(Products);

            OnPropertyChanged(nameof(ProductList));

            OrderSummary orderSummary = await orderSummaryModel.GetOrderSummaryByIdAsync(orderHistoryID);

            Subtotal = orderSummary.Subtotal;
            DeliveryFee = orderSummary.DeliveryFee;
            Total = orderSummary.FinalTotal;
        }

        /// <summary>
        /// Occurs when a property value changes.
        /// </summary>
        public event PropertyChangedEventHandler PropertyChanged;

        /// <summary>
        /// Raises the <see cref="PropertyChanged"/> event for the specified property.
        /// </summary>
        /// <param name="propertyName">The name of the property that changed.</param>
        protected void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
        public double Subtotal
        {
            get => subtotal;
            set
            {
                subtotal = value;
                OnPropertyChanged(nameof(Subtotal));
            }
        }

        public double DeliveryFee
        {
            get => deliveryFee;
            set
            {
                deliveryFee = value;
                OnPropertyChanged(nameof(DeliveryFee));
            }
        }

        public double Total
        {
            get => total;
            set
            {
                total = value;
                OnPropertyChanged(nameof(Total));
            }
        }

        public string Email
        {
            get => email;
            set
            {
                email = value;
                OnPropertyChanged(nameof(Email));
            }
        }

        public string CardHolderName
        {
            get => cardholder;
            set
            {
                cardholder = value;
                OnPropertyChanged(nameof(CardHolderName));
            }
        }

        public string CardNumber
        {
            get => cardnumber;
            set
            {
                cardnumber = value;
                OnPropertyChanged(nameof(CardNumber));
            }
        }

        public string CardMonth
        {
            get => cardMonth;
            set
            {
                cardMonth = value;
                OnPropertyChanged(nameof(CardMonth));
            }
        }

        public string CardYear
        {
            get => cardYear;
            set
            {
                cardYear = value;
                OnPropertyChanged(nameof(CardYear));
            }
        }

        public string CardCVC
        {
            get => cardCVC;
            set
            {
                cardCVC = value;
                OnPropertyChanged(nameof(CardCVC));
            }
        }

        /// <summary>
        /// Asynchronously retrieves dummy products associated with the specified order history.
        /// </summary>
        /// <param name="orderHistoryID">The unique identifier for the order history.</param>
        /// <returns>A task that represents the asynchronous operation, containing a list of <see cref="Product"/> objects.</returns>
        public async Task<List<Product>> GetProductsFromOrderHistoryAsync(int orderHistoryID)
        {
            return await orderHistoryModel.GetProductsFromOrderHistoryAsync(orderHistoryID);
        }

        /// <summary>
        /// Processes the card payment by deducting the order total from the card balance.
        /// </summary>
        /// <returns>A task representing the asynchronous operation.</returns>
        public async Task ProcessCardPaymentAsync()
        {
            double balance = await dummyCardModel.GetCardBalanceAsync(CardNumber);

            OrderSummary orderSummary = await orderSummaryModel.GetOrderSummaryByIdAsync(orderHistoryID);

            double totalSum = orderSummary.FinalTotal;

            double newBalance = balance - totalSum;

            await dummyCardModel.UpdateCardBalanceAsync(CardNumber, newBalance);
        }

        [ExcludeFromCodeCoverage]
        /// <summary>
        /// Handles the pay button click event by processing the card payment and transitioning to the final purchase window.
        /// </summary>
        /// <returns>A task representing the asynchronous operation.</returns>
        public async Task OnPayButtonClickedAsync()
        {
            await ProcessCardPaymentAsync();

            var billingInfoWindow = new BillingInfoWindow();
            var finalisePurchasePage = new FinalisePurchase(orderHistoryID);
            billingInfoWindow.Content = finalisePurchasePage;

            billingInfoWindow.Activate();
        }
    }
}
