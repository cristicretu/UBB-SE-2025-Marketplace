using Microsoft.UI.Xaml.Controls;

using MarketPlace924.ViewModel; // For ShoppingCartViewModel
using SharedClassLibrary.Service; // For ShoppingCartRepository
// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace MarketPlace924.View
{

    /// <summary>
    /// A user control that displays the shopping cart.
    /// </summary>
    public sealed partial class ShoppingCartView : UserControl
    {
        public ShoppingCartViewModel ViewModel { get; }

        public ShoppingCartView()
        {
            this.InitializeComponent();
            this.ViewModel = new ShoppingCartViewModel(new ShoppingCartService(), buyerId: UserSession.CurrentUserId ?? 0);
        }
    }
}
