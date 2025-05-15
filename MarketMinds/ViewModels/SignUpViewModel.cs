// <copyright file="SignUpViewModel.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>
namespace MarketPlace924.ViewModel
{
    using System;
    using System.ComponentModel;
    using System.Runtime.CompilerServices;
    using System.Windows.Input;
    using SharedClassLibrary.Helper;
    using SharedClassLibrary.Service;
    using Microsoft.UI.Xaml.Controls;

    /// <summary>
    /// SignUpViewModel class.
    /// </summary>
    public partial class SignUpViewModel : INotifyPropertyChanged, ISignUpViewModel
    {
        /// <summary>
        /// UserService property.
        /// </summary>
        private readonly IUserService userService;

        /// <summary>
        /// Username property.
        /// </summary>
        private string username;

        /// <summary>
        /// Email property.
        /// </summary>
        private string email;

        /// <summary>
        /// PhoneNumber property.
        /// </summary>
        private string phoneNumber;

        /// <summary>
        /// Password property.
        /// </summary>
        private string password;

        /// <summary>
        /// Role property.
        /// </summary>
        private int role;

        /// <summary>
        /// Initializes a new instance of the <see cref="SignUpViewModel"/> class.
        /// </summary>
        /// <param name="userService">The user service.</param>
        public SignUpViewModel(IUserService userService)
        {
            this.userService = userService;
            this.SignupCommand = new RelayCommand(this.ExecuteSignup);
            this.username = string.Empty;
            this.email = string.Empty;
            this.phoneNumber = string.Empty;
            this.password = string.Empty;
        }

        /// <summary>
        /// PropertyChanged event.
        /// </summary>
        public event PropertyChangedEventHandler? PropertyChanged;

        /// <summary>
        /// Gets or sets NavigateToLogin property.
        /// </summary>
        public Action? NavigateToLogin { get; set; }

        /// <summary>
        /// Gets or sets Username property.
        /// </summary>
        public string Username
        {
            get => this.username;
            set
            {
                this.username = value;
                this.OnPropertyChanged();
            }
        }

        /// <summary>
        /// Gets or sets Email property.
        /// </summary>
        public string Email
        {
            get => this.email;
            set
            {
                this.email = value;
                this.OnPropertyChanged();
            }
        }

        /// <summary>
        /// Gets or sets PhoneNumber property.
        /// </summary>
        public string PhoneNumber
        {
            get => this.phoneNumber;
            set
            {
                this.phoneNumber = value;
                this.OnPropertyChanged();
            }
        }

        /// <summary>
        /// Gets or sets Password property.
        /// </summary>
        public string Password
        {
            get => this.password;
            set
            {
                this.password = value;
                this.OnPropertyChanged();
            }
        }

        /// <summary>
        /// Gets or sets Role property.
        /// </summary>
        public int Role
        {
            get => this.role;
            set
            {
                this.role = value;
                this.OnPropertyChanged();
            }
        }

        /// <summary>
        /// Gets SignupCommand property.
        /// </summary>
        public ICommand SignupCommand { get; }

        /// <summary>
        /// OnPropertyChanged method.
        /// </summary>
        /// <param name="name">The name.</param>
        protected void OnPropertyChanged([CallerMemberName] string? name = null)
        {
            this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }

        /// <summary>
        /// Executes the signup command.
        /// </summary>
        /// <param name="parameter">The parameter.</param>
        private async void ExecuteSignup(object parameter)
        {
            try
            {
                await this.userService.RegisterUser(this.Username, this.Password, this.Email, this.PhoneNumber, this.Role);
                await this.ShowDialog("Success", "Your account has been created successfully!");
                this.NavigateToLogin?.Invoke();
            }
            catch (Exception ex)
            {
                await this.ShowDialog("Error", ex.Message);
            }
        }

        /// <summary>
        /// Shows the dialog.
        /// </summary>
        /// <param name="title">The title.</param>
        /// <param name="message">The message.</param>
        private async System.Threading.Tasks.Task ShowDialog(string title, string message)
        {
            if (App.MainWindow == null)
            {
                return;
            }

            // ContentDialog only works in UI threads, can't be tested
            ContentDialog dialog = new ContentDialog
            {
                Title = title,
                Content = message,
                CloseButtonText = "OK",
                XamlRoot = App.MainWindow?.Content?.XamlRoot,
            };

            await dialog.ShowAsync();
        }
    }
}
