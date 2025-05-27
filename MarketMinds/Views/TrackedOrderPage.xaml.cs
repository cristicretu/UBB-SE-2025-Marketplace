using Microsoft.UI.Xaml.Controls;
using MarketMinds.ViewModels;

namespace MarketMinds.Views
{
    public sealed partial class TrackedOrderPage : Page
    {
        public TrackedOrderViewModel ViewModel { get; }

        public TrackedOrderPage()
        {
            this.InitializeComponent();
            ViewModel = App.TrackedOrderViewModel;
        }
    }
}