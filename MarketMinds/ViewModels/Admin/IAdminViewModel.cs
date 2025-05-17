// <copyright file="IAdminViewModel.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

namespace MarketMinds.ViewModels.Admin
{
    using System.Collections.ObjectModel;
    using MarketMinds.Shared.Models;

    /// <summary>
    /// The admin view model interface.
    /// </summary>
    public interface IAdminViewModel
    {
        /// <summary>
        /// Gets the users in the admin view.
        /// </summary>
        ObservableCollection<IUserRowViewModel> Users { get; }

        /// <summary>
        /// Gets or sets the pie series.
        /// </summary>
        // List<ISeries>? PieSeries { get; set; }

        /// <summary>
        /// Ban a user.
        /// </summary>
        /// <param name="user">The user to ban.</param>
        void BanUser(User user);

        /// <summary>
        /// Refresh the users.
        /// </summary>
        void RefreshUsers();
    }
}
