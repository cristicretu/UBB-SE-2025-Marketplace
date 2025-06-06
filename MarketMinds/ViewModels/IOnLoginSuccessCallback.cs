﻿// <copyright file="IOnLoginSuccessCallback.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

namespace MarketMinds.ViewModels
{
    using System.Threading.Tasks;
    using MarketMinds.Shared.Models;

    /// <summary>
    /// Defines a callback interface for handling actions to be performed upon a successful login.
    /// </summary>
    public interface IOnLoginSuccessCallback
    {
        /// <summary>
        /// Handles the actions to be performed upon a successful login.
        /// </summary>
        /// <param name="user">The user who has successfully logged in.</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        public Task OnLoginSuccess(User user);
    }
}