using Microsoft.UI.Xaml.Controls;

using MarketMinds.ViewModels; // For ShoppingCartViewModel
using MarketMinds.Shared.Services; // For ShoppingCartRepository
// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace MarketMinds.Views
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
