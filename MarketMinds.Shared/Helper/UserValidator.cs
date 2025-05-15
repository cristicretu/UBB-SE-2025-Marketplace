// <copyright file="UserValidator.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>
namespace SharedClassLibrary.Helper
{
    using System.Text.RegularExpressions;
    using SharedClassLibrary.Service;

    /// <summary>
    /// Provides validation methods for user-related data such as username, email, phone number and password.
    /// </summary>
    internal class UserValidator : IUserValidator
    {
        private const int MinimumUsernameLength = 4;
        private const int MinimumPasswordLength = 8;

        /// <summary>
        /// Validates the username to ensure it meets the required criteria: not null, not empty, and at least 4 characters long.
        /// </summary>
        /// <param name="username">The username to validate.</param>
        /// <returns>True if the username is valid, otherwise false.</returns>
        public bool IsValidUsername(string username)
        {
            if (string.IsNullOrWhiteSpace(username))
            {
                return false;
            }

            if (username.Length < MinimumUsernameLength)
            {
                return false;
            }

            return true;
        }

        /// <summary>
        /// Validates the email address to ensure it meets the required criteria: not null, not empty, and matches the email pattern.
        /// </summary>
        /// <param name="email">The email address to validate.</param>
        /// <returns>True if the email address is valid, otherwise false.</returns>
        public bool IsValidEmail(string email)
        {
            if (string.IsNullOrWhiteSpace(email))
            {
                return false;
            }

            var emailPattern = @"^[a-zA-Z0-9._%+-]+@[a-zA-Z0-9.-]+\.[a-zA-Z]{2,}$";
            return Regex.IsMatch(email, emailPattern);
        }

        /// <summary>
        /// Validates the phone number to ensure it meets the required criteria: not null, not empty, and matches the phone pattern.
        /// </summary>
        /// <param name="phoneNumber">The phone number to validate.</param>
        /// <returns>True if the phone number is valid, otherwise false.</returns>
        public bool IsValidPhoneNumber(string phoneNumber)
        {
            if (string.IsNullOrWhiteSpace(phoneNumber))
            {
                return false;
            }

            string phonePattern = @"\+40\d{9}";
            if (!Regex.IsMatch(phoneNumber, phonePattern))
            {
                return false;
            }

            return true;
        }

        /// <summary>
        /// Validates the password to ensure it meets the required criteria: not null, not empty, at least 8 characters long, contains at least one uppercase letter, one digit, and one special character.
        /// </summary>
        /// <param name="password">The password to validate.</param>
        /// <returns>True if the password is valid, otherwise false.</returns>
        public bool IsValidPassword(string password)
        {
            if (string.IsNullOrWhiteSpace(password))
            {
                return false;
            }

            if (password.Length < MinimumPasswordLength)
            {
                return false;
            }

            string passwordPattern = @"^(?=.*[A-Z])(?=.*\d)(?=.*[!@#$%^&*()_+])[A-Za-z\d!@#$%^&*()_+]{8,}$";
            if (!Regex.IsMatch(password, passwordPattern))
            {
                return false;
            }

            return true;
        }
    }
}
