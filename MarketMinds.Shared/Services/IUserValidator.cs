// <copyright file="IUserValidator.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

namespace SharedClassLibrary.Service
{
    /// <summary>
    /// Defines validation methods for user-related data such as username, email, phone number, and password.
    /// </summary>
    public interface IUserValidator
    {
        /// <summary>
        /// Validates the username to ensure it meets the required criteria: not null, not empty, and at least 4 characters long.
        /// </summary>
        /// <param name="username">The username to validate.</param>
        /// <returns>True if the username is valid, otherwise false.</returns>
        bool IsValidUsername(string username);

        /// <summary>
        /// Validates the email address to ensure it meets the required criteria: not null, not empty, and matches the email pattern.
        /// </summary>
        /// <param name="email">The email address to validate.</param>
        /// <returns>True if the email address is valid, otherwise false.</returns>
        bool IsValidEmail(string email);

        /// <summary>
        /// Validates the phone number to ensure it meets the required criteria: not null, not empty, and matches the phone pattern.
        /// </summary>
        /// <param name="phoneNumber">The phone number to validate.</param>
        /// <returns>True if the phone number is valid, otherwise false.</returns>
        bool IsValidPhoneNumber(string phoneNumber);

        /// <summary>
        /// Validates the password to ensure it meets the required criteria: not null, not empty, at least 8 characters long, contains at least one uppercase letter, one digit, and one special character.
        /// </summary>
        /// <param name="password">The password to validate.</param>
        /// <returns>True if the password is valid, otherwise false.</returns>
        bool IsValidPassword(string password);
    }
}
