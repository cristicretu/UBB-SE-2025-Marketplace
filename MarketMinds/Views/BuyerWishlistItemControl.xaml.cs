// <copyright file="BuyerWishlistItemControl.xaml.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

namespace MarketPlace924.View
{
    using MarketPlace924.ViewModel;
    using Microsoft.UI.Xaml;
    using Microsoft.UI.Xaml.Controls;

    /// <summary>
    /// A control that displays and manages a single item in the buyer's wishlist.
    /// </summary>
    /// <seealso cref="IBuyerWishlistItemViewModel"/>
    public sealed partial class BuyerWishlistItemControl : UserControl
    {
        /// <summary>
        /// Dependency property for the ViewModel associated with this control.
        /// </summary>
        public static readonly DependencyProperty ViewModelProperty =
            DependencyProperty.Register(
                nameof(ViewModel),
                typeof(IBuyerWishlistItemViewModel),
                typeof(BuyerWishlistItemControl),
                new PropertyMetadata(null, OnChildViewModelChanged));

        /// <summary>
        /// Initializes a new instance of the <see cref="BuyerWishlistItemControl"/> class.
        /// </summary>
        public BuyerWishlistItemControl()
        {
            this.InitializeComponent();
        }

        /// <summary>
        /// Gets or sets the ViewModel for this control.
        /// </summary>
        public IBuyerWishlistItemViewModel ViewModel
        {
            get => (IBuyerWishlistItemViewModel)this.GetValue(ViewModelProperty);
            set => this.SetValue(ViewModelProperty, value);
        }

        /// <summary>
        /// Handles changes to the ViewModel property.
        /// </summary>
        /// <param name="dependencyObject">The dependency object.</param>
        /// <param name="eventArgs">The dependency property changed event arguments.</param>
        private static void OnChildViewModelChanged(DependencyObject dependencyObject, DependencyPropertyChangedEventArgs eventArgs)
        {
            var control = (BuyerWishlistItemControl)dependencyObject;
            control.DataContext = eventArgs.NewValue;
        }
    }
}