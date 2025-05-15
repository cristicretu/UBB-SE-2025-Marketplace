// <copyright file="ISignUpViewModel.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

namespace MarketPlace924.ViewModel
{
    using System;
    using System.Windows.Input;

    /// <summary>
    /// ISignUpViewModel interface.
    /// </summary>
    public interface ISignUpViewModel
    {
        /// <summary>
        /// Gets or sets NavigateToLogin property.
        /// </summary>
        Action? NavigateToLogin { get; set; }

        /// <summary>
        /// Gets or sets Username property.
        /// </summary>
        string Username { get; set; }

        /// <summary>
        /// Gets or sets Email property.
        /// </summary>
        string Email { get; set; }

        /// <summary>
        /// Gets or sets PhoneNumber property.
        /// </summary>
        string PhoneNumber { get; set; }

        /// <summary>
        /// Gets or sets Password property.
        /// </summary>
        string Password { get; set; }

        /// <summary>
        /// Gets or sets Role property.
        /// </summary>
        int Role { get; set; }

        /// <summary>
        /// Gets SignupCommand property.
        /// </summary>
        ICommand SignupCommand { get; }
    }
}
