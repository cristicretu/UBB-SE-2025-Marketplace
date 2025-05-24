// <copyright file="BuyerFamilySyncControl.xaml.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

namespace MarketMinds.Views
{
    using MarketMinds.ViewModels;
    using Microsoft.UI.Xaml;
    using Microsoft.UI.Xaml.Controls;

    /// <summary>
    /// A control that displays and manages family synchronization.
    /// </summary>
    /// <seealso cref="BuyerFamilySyncViewModel"/>
    public sealed partial class BuyerFamilySyncControl : UserControl
    {
        /// <summary>
        /// Dependency property for the ViewModel associated with this control.
        /// </summary>
        public static readonly DependencyProperty ViewModelProperty =
            DependencyProperty.Register(
                nameof(ViewModel),
                typeof(IBuyerFamilySyncViewModel),
                typeof(BuyerFamilySyncControl),
                new PropertyMetadata(null, OnViewModelChanged));

        /// <summary>
        /// Initializes a new instance of the <see cref="BuyerFamilySyncControl"/> class.
        /// </summary>
        public BuyerFamilySyncControl()
        {
            this.InitializeComponent();
        }

        /// <summary>
        /// Gets or sets the ViewModel for this control.
        /// </summary>
        public IBuyerFamilySyncViewModel ViewModel
        {
            get => (IBuyerFamilySyncViewModel)this.GetValue(ViewModelProperty);
            set => this.SetValue(ViewModelProperty, value);
        }

        /// <summary>
        /// Handles changes to the ViewModel property.
        /// </summary>
        /// <param name="d">The dependency object.</param>
        /// <param name="e">The dependency property changed event arguments.</param>
        private static async void OnViewModelChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is BuyerFamilySyncControl control && e.NewValue is IBuyerFamilySyncViewModel viewModel)
            {
                await viewModel.LoadLinkages();
            }
        }
    }
}