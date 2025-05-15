// <copyright file="BuyerBadgeControl.xaml.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

namespace MarketPlace924.View
{
    using SharedClassLibrary.Domain;
    using MarketPlace924.ViewModel;
    using Microsoft.UI.Xaml;
    using Microsoft.UI.Xaml.Controls;

    /// <summary>
    /// A control that displays and manages the buyer's badge information and progress.
    /// </summary>
    /// <seealso cref="BuyerBadgeViewModel"/>
    public sealed partial class BuyerBadgeControl : UserControl
    {
        /// <summary>
        /// Dependency property for the ViewModel associated with this control.
        /// </summary>
        public static readonly DependencyProperty ViewModelProperty =
            DependencyProperty.Register(
                nameof(ViewModel),
                typeof(IBuyerBadgeViewModel),
                typeof(BuyerBadgeControl),
                new PropertyMetadata(null, OnChildViewModelChanged));

        /// <summary>
        /// Initializes a new instance of the <see cref="BuyerBadgeControl"/> class.
        /// </summary>
        public BuyerBadgeControl()
        {
            this.InitializeComponent();
        }

        /// <summary>
        /// Gets or sets the ViewModel for this control.
        /// </summary>
        public IBuyerBadgeViewModel ViewModel
        {
            get => (IBuyerBadgeViewModel)this.GetValue(ViewModelProperty);
            set => this.SetValue(ViewModelProperty, value);
        }

        /// <summary>
        /// Handles changes to the ViewModel property.
        /// </summary>
        /// <param name="dependencyObject">The dependency object.</param>
        /// <param name="eventArgs">The dependency property changed event arguments.</param>
        private static void OnChildViewModelChanged(DependencyObject dependencyObject, DependencyPropertyChangedEventArgs eventArgs)
        {
            var control = (BuyerBadgeControl)dependencyObject;
            control.DataContext = eventArgs.NewValue;
        }
    }
}