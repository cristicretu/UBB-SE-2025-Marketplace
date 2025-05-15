using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Navigation;

using MarketPlace924.ViewModel; // For ShoppingCartViewModel
using SharedClassLibrary.ProxyRepository;
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
