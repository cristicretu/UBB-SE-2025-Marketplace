// <copyright file="IOnBuyerLinkageUpdatedCallback.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

namespace MarketPlace924.ViewModel
{
    using System.Threading.Tasks;

    /// <summary>
    /// Interface for handling buyer linkage update callbacks.
    /// </summary>
    public interface IOnBuyerLinkageUpdatedCallback
    {
    /// <summary>
    /// Called when a buyer linkage has been updated.
    /// </summary>
    /// <returns>A task representing the asynchronous operation.</returns>
    Task OnBuyerLinkageUpdated();
    }
}