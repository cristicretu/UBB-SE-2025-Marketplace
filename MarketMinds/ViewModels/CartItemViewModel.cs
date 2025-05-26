using System;
using System.Windows.Input;
using System.ComponentModel;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using MarketMinds.Shared.Models;
using MarketMinds.Shared.Helper;

namespace MarketMinds.ViewModels
{
    public class CartItemViewModel : ICartItemViewModel, INotifyPropertyChanged, IDisposable
    {
        private readonly IShoppingCartViewModel shoppingCartViewModel;
        private int quantity;
        private Product product;

        public Product Product
        {
            get => product;
            set
            {
                if (product != value)
                {
                    product = value;
                    OnPropertyChanged();
                    OnPropertyChanged(nameof(TotalPrice));
                    OnPropertyChanged(nameof(AvailableStock));
                    OnPropertyChanged(nameof(StockMessage));
                }
            }
        }

        public int Quantity
        {
            get => quantity;
            set
            {
                if (quantity != value)
                {
                    quantity = value;
                    OnPropertyChanged();
                    OnPropertyChanged(nameof(TotalPrice));
                }
            }
        }

        public double TotalPrice => Product.Price * Quantity;

        public int AvailableStock
        {
            get
            {
                if (Product is BuyProduct buyProduct)
                {
                    return buyProduct.Stock;
                }
                // Default value if not a BuyProduct
                return 0;
            }
        }

        // Format stock message
        public string StockMessage => $"Stock: {AvailableStock}";

        // Expose parent ViewModel for binding
        public IShoppingCartViewModel ParentViewModel => shoppingCartViewModel;

        // Expose IsLoading directly for better binding support in DataTemplate
        public bool IsLoading => shoppingCartViewModel?.IsLoading ?? false;

        public ICommand IncrementCommand { get; }
        public ICommand DecrementCommand { get; }
        public ICommand RemoveCommand { get; }

        public CartItemViewModel(Product product, int quantity, IShoppingCartViewModel parentViewModel = null)
        {
            this.product = product;
            this.quantity = quantity;
            shoppingCartViewModel = parentViewModel;

            // Subscribe to parent's property changes to update our IsLoading property
            if (shoppingCartViewModel is INotifyPropertyChanged parentNotify)
            {
                parentNotify.PropertyChanged += OnParentPropertyChanged;
            }

            IncrementCommand = new RelayCommand(_ => Increment());
            DecrementCommand = new RelayCommand(_ => Decrement());
            RemoveCommand = new RelayCommand(_ => Remove());
        }

        private void OnParentPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(IShoppingCartViewModel.IsLoading))
            {
                OnPropertyChanged(nameof(IsLoading));
            }
        }

        private void Increment()
        {
            Debug.WriteLine($"Increment called: Current Quantity={Quantity}, Available Stock={AvailableStock}");

            if (AvailableStock > 0 && Quantity < AvailableStock)
            {
                Debug.WriteLine($"Incrementing quantity for {Product.Title} from {Quantity} to {Quantity + 1}");
                Quantity++;

                if (shoppingCartViewModel != null)
                {
                    _ = shoppingCartViewModel.UpdateQuantityAsync(Product, Quantity);
                }
            }
            else
            {
                Debug.WriteLine($"Cannot increment: Quantity={Quantity} is at or exceeds AvailableStock={AvailableStock}");
            }
        }

        private void Decrement()
        {
            if (Quantity > 1)
            {
                Debug.WriteLine($"Decrementing quantity for {Product.Title} from {Quantity} to {Quantity - 1}");
                Quantity--;

                if (shoppingCartViewModel != null)
                {
                    _ = shoppingCartViewModel.UpdateQuantityAsync(Product, Quantity);
                }
            }
        }

        private void Remove()
        {
            Debug.WriteLine($"Removing {Product.Title} from cart");

            if (shoppingCartViewModel != null)
            {
                _ = shoppingCartViewModel.RemoveFromCartAsync(Product);
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        public void Dispose()
        {
            // Unsubscribe from parent's property changes to prevent memory leaks
            if (shoppingCartViewModel is INotifyPropertyChanged parentNotify)
            {
                parentNotify.PropertyChanged -= OnParentPropertyChanged;
            }
        }
    }
}
