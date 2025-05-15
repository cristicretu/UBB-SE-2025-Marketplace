// <copyright file="AdminViewModel.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

namespace MarketPlace924.ViewModel.Admin
{
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Linq;
    using System.Windows.Input;
    using LiveChartsCore;
    using LiveChartsCore.SkiaSharpView;
    using LiveChartsCore.SkiaSharpView.Painting;
    using SharedClassLibrary.Domain;
    using SharedClassLibrary.Helper;
    using SharedClassLibrary.Service;
    using Microsoft.UI.Xaml.Controls;
    using SkiaSharp;

    /// <summary>
    /// The admin view model.
    /// </summary>
    public class AdminViewModel : IAdminViewModel
    {
        /// <summary>
        /// The private instance of the admin view model.
        /// </summary>
        private static AdminViewModel? instance;

        /// <summary>
        /// The admin service.
        /// </summary>
        private readonly IAdminService adminService;

        /// <summary>
        /// The user service.
        /// </summary>
        private readonly IUserService userService;

        /// <summary>
        /// The analytics service.
        /// </summary>
        private readonly IAnalyticsService analyticsService;

        /// <summary>
        /// The items in the admin view.
        /// </summary>
        private ObservableCollection<IUserRowViewModel>? items;

        /// <summary>
        /// The users in the admin view.
        /// </summary>
        private List<User> users = new();

        /// <summary>
        /// The total users count.
        /// </summary>
        private int totalUsersCount = 0;

        /// <summary>
        /// Initializes a new instance of the <see cref="AdminViewModel"/> class.
        /// </summary>
        /// <param name="adminService">The admin service for managing administrative operations.</param>
        /// <param name="analyticsService">The analytics service for retrieving statistical data.</param>
        /// <param name="userService">The user service for user-related operations.</param>
        public AdminViewModel(IAdminService adminService, IAnalyticsService analyticsService, IUserService userService)
        {
            instance = this;
            this.adminService = adminService;
            this.analyticsService = analyticsService;

            this.SetupPieChart();
            this.userService = userService;
        }

        /// <summary>
        /// Gets the users in the admin view.
        /// </summary>
        public ObservableCollection<IUserRowViewModel> Users
        {
            get
            {
                this.users = this.userService.GetAllUsers().Result;
                this.items ??= new ObservableCollection<IUserRowViewModel>(
                    this.users.Select(user => new UserRowViewModel(user, this.adminService, this)));
                return this.items;
            }
        }

        /// <summary>
        /// Gets or sets the pie series.
        /// </summary>
        public List<ISeries>? PieSeries { get; set; }

        /// <summary>
        /// Ban a user.
        /// </summary>
        /// <param name="user">The user to ban.</param>
        public void BanUser(User user)
        {
            if (user != null)
            {
                this.Users.Remove(this.Users.Where(u => u.Username != user.Username).First());
                ShowBanDialog(user.Username);
            }
        }

        /// <summary>
        /// Refresh the users.
        /// </summary>
        public async void RefreshUsers()
        {
            this.users = await this.userService.GetAllUsers();
            this.items?.Clear();
            foreach (var user in this.users)
            {
                this.items?.Add(new UserRowViewModel(user, this.adminService, this));
            }
        }

        /// <summary>
        /// Show the ban dialog.
        /// </summary>
        /// <param name="username">The username of the user to ban.</param>
        private static void ShowBanDialog(string username)
        {
            var dialog = new ContentDialog
            {
                Title = "User Banned",
                Content = $"{username} has been banned.",
                CloseButtonText = "OK",
                XamlRoot = App.MainWindow?.Content.XamlRoot,
            };
            _ = dialog.ShowAsync();
        }

        /// <summary>
        /// Setup the pie chart.
        /// </summary>
        private void SetupPieChart()
        {
            this.totalUsersCount = this.analyticsService.GetTotalNumberOfUsers().Result;
            var buyersCount = this.analyticsService.GetTotalNumberOfBuyers().Result;

            this.PieSeries =
            [
                new PieSeries<double>
                {
                    Values = new List<double> { buyersCount },
                    Name = "Buyers",
                    Fill = new SolidColorPaint(new SKColor(25, 118, 210)),
                },
                new PieSeries<double>
                {
                    Values = new List<double> { this.totalUsersCount - buyersCount },
                    Name = "Sellers",
                    Fill = new SolidColorPaint(new SKColor(10, 56, 113)),
                },
            ];
        }
    }
}
