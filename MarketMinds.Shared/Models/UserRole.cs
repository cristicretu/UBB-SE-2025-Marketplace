// -----------------------------------------------------------------------
// <copyright file="UserRole.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace SharedClassLibrary.Domain;

/// <summary>
/// Defines the roles a user can have within the system.
/// </summary>
public enum UserRole
{
    /// <summary>
    /// Role not yet assigned or unknown.
    /// </summary>
    Unassigned = 0,

    /// <summary>
    /// Administrator role with full privileges.
    /// </summary>
    Admin = 1,

    /// <summary>
    /// Role for users who primarily buy products.
    /// </summary>
    Buyer = 2,

    /// <summary>
    /// Role for users who primarily sell products.
    /// </summary>
    Seller = 3,
}
