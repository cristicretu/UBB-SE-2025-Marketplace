// <copyright file="ILoginViewModel.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>
namespace MarketPlace924.ViewModel
{
    using System;
    using System.ComponentModel;
    using System.Windows.Input;
    using SharedClassLibrary.Service;

    /// <summary>
    /// Interface for the login view model.
    /// </summary>
    public interface ILoginViewModel : INotifyPropertyChanged
    {
        /// <summary>
        /// Gets the user service.
        /// </summary>
        IUserService UserService { get; }

        /// <summary>
        /// Gets or sets the action to navigate to sign up.
        /// </summary>
        Action? NavigateToSignUp { get; set; }

        /// <summary>
        /// Gets or sets the email.
        /// </summary>
        string? Email { get; set; }

        /// <summary>
        /// Gets the failed attempts text.
        /// </summary>
        string FailedAttemptsText { get; }

        /// <summary>
        /// Gets or sets the password.
        /// </summary>
        string? Password { get; set; }

        /// <summary>
        /// Gets or sets the error message.
        /// </summary>
        string? ErrorMessage { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the login is enabled.
        /// </summary>
        bool IsLoginEnabled { get; set; }

        /// <summary>
        /// Gets or sets the captcha text.
        /// </summary>
        string? CaptchaText { get; set; }

        /// <summary>
        /// Gets or sets the captcha entered code.
        /// </summary>
        string? CaptchaEnteredCode { get; set; }

        /// <summary>
        /// Gets the login command.
        /// </summary>
        ICommand LoginCommand { get; }
    }
}
