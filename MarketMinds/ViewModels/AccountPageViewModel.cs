using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using MarketMinds.Shared.Models;
using MarketMinds.Shared.Services;
using Marketplace_SE.Utilities;
using MarketMinds.Shared.Helper;

namespace MarketMinds.ViewModels
{
    public class AccountPageViewModel : INotifyPropertyChanged
    {
        private User currentUser;
        private ObservableCollection<UserOrder> orders;
        private UserOrder selectedOrder;
        private string errorMessage;
        private bool isLoading;
        private readonly IAccountPageService accountPageService;

        public event PropertyChangedEventHandler PropertyChanged;

        public User CurrentUser
        {
            get => currentUser;
            set
            {
                if (currentUser != value)
                {
                    currentUser = value;
                    OnPropertyChanged();
                }
            }
        }

        public ObservableCollection<UserOrder> Orders
        {
            get => orders;
            set
            {
                if (orders != value)
                {
                    orders = value;
                    OnPropertyChanged();
                }
            }
        }

        public UserOrder SelectedOrder
        {
            get => selectedOrder;
            set
            {
                if (selectedOrder != value)
                {
                    selectedOrder = value;
                    OnPropertyChanged();
                }
            }
        }

        public string ErrorMessage
        {
            get => errorMessage;
            set
            {
                if (errorMessage != value)
                {
                    errorMessage = value;
                    OnPropertyChanged();
                }
            }
        }

        public bool IsLoading
        {
            get => isLoading;
            set
            {
                if (isLoading != value)
                {
                    isLoading = value;
                    OnPropertyChanged();
                }
            }
        }

        public RelayCommand LoadDataCommand { get; private set; }
        public RelayCommand ViewOrderCommand { get; private set; }
        public RelayCommand ReturnItemCommand { get; private set; }
        public RelayCommand NavigateToMainCommand { get; private set; }

        public AccountPageViewModel(IAccountPageService accountPageService)
        {
            this.accountPageService = accountPageService ?? throw new ArgumentNullException(nameof(accountPageService));
            Orders = new ObservableCollection<UserOrder>();

            // Initialize commands
            LoadDataCommand = new RelayCommand(async _ => await LoadUserDataAsync());
            ViewOrderCommand = new RelayCommand(_ => ViewOrderDetails());
            ReturnItemCommand = new RelayCommand(_ => ReturnItem(), CanReturnItem);
            NavigateToMainCommand = new RelayCommand(_ => NavigateToMainPage());
        }

        public async Task LoadUserDataAsync()
        {
            try
            {
                IsLoading = true;
                ErrorMessage = string.Empty;

                System.Diagnostics.Debug.WriteLine("AccountPageViewModel: Fetching current user data...");

                // Check if accountPageService is initialized
                if (accountPageService == null)
                {
                    System.Diagnostics.Debug.WriteLine("ERROR: accountPageService is null");
                    ErrorMessage = "Internal error: Service not initialized";
                    return;
                }

                // Attempt to get current user
                CurrentUser = await accountPageService.GetCurrentLoggedInUserAsync(App.CurrentUser);

                if (CurrentUser != null)
                {
                    System.Diagnostics.Debug.WriteLine($"User loaded successfully: ID={CurrentUser.Id}, Name={CurrentUser.Username}");

                    // Get user orders
                    var userOrders = await accountPageService.GetUserOrdersAsync(CurrentUser.Id);

                    // Create a new collection to force UI update
                    var newOrdersCollection = new ObservableCollection<UserOrder>();

                    System.Diagnostics.Debug.WriteLine($"Retrieved {userOrders?.Count ?? 0} orders for user");

                    if (userOrders != null && userOrders.Any())
                    {
                        foreach (var order in userOrders)
                        {
                            // Add additional order properties if needed
                            order.OrderStatus = DetermineOrderStatus(order);
                            newOrdersCollection.Add(order);
                            System.Diagnostics.Debug.WriteLine($"Added order to collection: ID={order.Id}, Name={order.Name}, Cost={order.Cost}");
                        }
                    }

                    // Replace the entire collection to ensure UI update
                    Orders = newOrdersCollection;
                    System.Diagnostics.Debug.WriteLine($"Updated Orders collection with {Orders.Count} items");
                }
                else
                {
                    System.Diagnostics.Debug.WriteLine("Failed to load user data - CurrentUser is null");
                    ErrorMessage = "Unable to load user data";
                }
            }
            catch (Exception userDataLoadException)
            {
                System.Diagnostics.Debug.WriteLine($"ERROR in LoadUserDataAsync: {userDataLoadException.Message}");
                System.Diagnostics.Debug.WriteLine($"Stack trace: {userDataLoadException.StackTrace}");

                if (userDataLoadException.InnerException != null)
                {
                    System.Diagnostics.Debug.WriteLine($"Inner exception: {userDataLoadException.InnerException.Message}");
                }

                ErrorMessage = $"Error loading account data: {userDataLoadException.Message}";
            }
            finally
            {
                IsLoading = false;
            }
        }

        private string DetermineOrderStatus(UserOrder order)
        {
            // Logic to determine order status - this is a placeholder
            // Replace with actual business logic in your application
            return "Completed"; // Default status
        }

        private void ViewOrderDetails()
        {
            if (SelectedOrder != null)
            {
                // Logic to navigate to order details page
                // This would be handled in the code-behind or through navigation service
            }
        }

        private void ReturnItem()
        {
            if (SelectedOrder != null)
            {
                // Logic to navigate to return item page
                // This would be handled in the code-behind or through navigation service
            }
        }

        private bool CanReturnItem(object parameter)
        {
            // Only allow return if this is a buy order for the current user
            return SelectedOrder != null && CurrentUser != null && SelectedOrder.BuyerId == CurrentUser.Id;
        }

        private void NavigateToMainPage()
        {
            // Navigation logic
            // This would be handled in the code-behind or through navigation service
        }

        public string FormatOrderDateTime(ulong timestamp)
        {
            try
            {
                return DataEncoder.ConvertTimestampToLocalDateTime(timestamp);
            }
            catch (Exception formatOrderDateException)
            {
                System.Diagnostics.Debug.WriteLine($"Error formatting timestamp: {formatOrderDateException.Message}");
                return DateTime.Now.ToString("MM/dd/yyyy HH:mm:ss");
            }
        }

        protected void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
