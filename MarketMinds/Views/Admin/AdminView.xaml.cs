// <copyright file="AdminView.xaml.cs" company="UBB-SE-2025-7UP">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

namespace MarketPlace924.View.Admin
{
    using System.Collections.ObjectModel;
    using SharedClassLibrary.Domain;
    using MarketPlace924.ViewModel;
    using MarketPlace924.ViewModel.Admin;
    using Microsoft.UI.Xaml;
    using Microsoft.UI.Xaml.Controls;
    using Microsoft.UI.Xaml.Navigation;

    /// <summary>
    /// Admin view page for managing users.
    /// </summary>
    public sealed partial class AdminView : Page
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="AdminView"/> class.
        /// </summary>
        public AdminView()
        {
            this.InitializeComponent();
            this.Users = new ObservableCollection<User>();
        }

        /// <summary>
        /// Gets or sets the collection of users.
        /// </summary>
        public ObservableCollection<User> Users { get; set; }

        /// <summary>
        /// Gets or sets the view model for this view.
        /// </summary>
        public IAdminViewModel ViewModel
        {
            get => (IAdminViewModel)this.DataContext;
            set => this.DataContext = value;
        }

        /// <summary>
        /// Called when the page is navigated to.
        /// </summary>
        /// <param name="e">Navigation event arguments.</param>
        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);
            if (e.Parameter is IAdminViewModel viewModel)
            {
                this.ViewModel = viewModel;
                this.UsersListView.ItemsSource = this.ViewModel.Users;
            }
        }
    }
}