// -----------------------------------------------------------------------
// <copyright file="Address.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

using System.ComponentModel.DataAnnotations.Schema;

namespace SharedClassLibrary.Domain;

/// <summary>
/// Represents a physical address.
/// </summary>
[Table("Addresses")]
public class Address
{
    /// <summary>
    /// Gets or sets the unique identifier for the address.
    /// </summary>
    public int Id { get; set; }

    /// <summary>
    /// Gets or sets the street line of the address.
    /// </summary>
    public string StreetLine { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the city.
    /// </summary>
    public string City { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the country.
    /// </summary>
    public string Country { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the postal code.
    /// </summary>
    public string PostalCode { get; set; } = string.Empty;
}
