// <copyright file="LoginViewModel.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>
// <copyright file="LoginViewModel.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>
namespace MarketMinds.ViewModels
{
    using System;
    using System.ComponentModel;
    using System.Threading.Tasks;
    using System.Windows.Input;
    using MarketMinds.Shared.Helper;
    using MarketMinds.Shared.Services.UserService;
    using MarketMinds.Shared.Services;
    using MarketMinds.ViewModels;
    using Microsoft.UI.Xaml;

    /// <summary>
    /// The login view model.
    /// </summary>
    public partial class LoginViewModel : INotifyPropertyChanged, ILoginViewModel
    {
        private readonly IOnLoginSuccessCallback successCallback;
        private readonly ICaptchaService captchaService;
        private string? email;
        private string? password;
        private string? errorMessage;
        private int failedAttempts;
        private bool isLoginEnabled = true;
        private string? captchaText;
        private string? captchaEnteredCode;
        private DispatcherTimer? banTimer;
        private DateTime banEndTime;
        private bool hasAttemptedLogin = false;

        /// <summary>
        /// Initializes a new instance of the <see cref="LoginViewModel"/> class.
        /// </summary>
        /// <param name="userService">The user service.</param>
        /// <param name="successCallback">The success callback.</param>
        /// <param name="captchaService">An optional argument for captcha service.</param>
        public LoginViewModel(IUserService userService, IOnLoginSuccessCallback successCallback, ICaptchaService? captchaService = null)
        {
            this.UserService = userService ?? throw new ArgumentNullException(nameof(userService));
            this.successCallback = successCallback ?? throw new ArgumentNullException(nameof(successCallback));
            this.captchaService = captchaService ?? new CaptchaService();

            this.GenerateCaptcha();

            this.LoginCommand = new RelayCommand(async () => await this.ExecuteLogin());
        }

        /// <summary>
        /// The property changed event handler.
        /// </summary>
        public event PropertyChangedEventHandler? PropertyChanged;

        /// <summary>
        /// Gets the user service.
        /// </summary>
        public IUserService UserService { get; private set; }
    using System;
    using System.ComponentModel;
    using System.Threading.Tasks;
    using System.Windows.Input;
    using MarketMinds.Shared.Helper;
    using MarketMinds.Shared.Services.UserService;
    using MarketMinds.Shared.Services;
    using MarketMinds.ViewModels;
    using Microsoft.UI.Xaml;

    /// <summary>
    /// The login view model.
    /// </summary>
    public partial class LoginViewModel : INotifyPropertyChanged, ILoginViewModel
    {
        private readonly IOnLoginSuccessCallback successCallback;
        private readonly ICaptchaService captchaService;
        private string? email;
        private string? password;
        private string? errorMessage;
        private int failedAttempts;
        private bool isLoginEnabled = true;
        private string? captchaText;
        private string? captchaEnteredCode;
        private DispatcherTimer? banTimer;
        private DateTime banEndTime;
        private bool hasAttemptedLogin = false;

        /// <summary>
        /// Initializes a new instance of the <see cref="LoginViewModel"/> class.
        /// </summary>
        /// <param name="userService">The user service.</param>
        /// <param name="successCallback">The success callback.</param>
        /// <param name="captchaService">An optional argument for captcha service.</param>
        public LoginViewModel(IUserService userService, IOnLoginSuccessCallback successCallback, ICaptchaService? captchaService = null)
        {
            this.UserService = userService ?? throw new ArgumentNullException(nameof(userService));
            this.successCallback = successCallback ?? throw new ArgumentNullException(nameof(successCallback));
            this.captchaService = captchaService ?? new CaptchaService();

            this.GenerateCaptcha();

            this.LoginCommand = new RelayCommand(async () => await this.ExecuteLogin());
        }

        /// <summary>
        /// The property changed event handler.
        /// </summary>
        public event PropertyChangedEventHandler? PropertyChanged;

        /// <summary>
        /// Gets the user service.
        /// </summary>
        public IUserService UserService { get; private set; }

        /// <summary>
        /// Gets or sets the action to navigate to sign up.
        /// </summary>
        public Action? NavigateToSignUp { get; set; }

        /// <summary>
        /// Gets or sets the email.
        /// </summary>
        public string? Email
        {
            get => this.email;
            set
            {
                this.email = value;
                this.OnPropertyChanged(nameof(this.Email));
            }
        }

        /// <summary>
        /// Gets the failed attempts text.
        /// </summary>
        public string FailedAttemptsText => this.hasAttemptedLogin ? $"Failed Login Attempts: {this.failedAttempts}" : string.Empty;

        /// <summary>
        /// Gets or sets the password.
        /// </summary>
        public string? Password
        {
            get => this.password;
            set
            {
                this.password = value;
                this.OnPropertyChanged(nameof(this.Password));
            }
        }

        /// <summary>
        /// Gets or sets the error message.
        /// </summary>
        public string? ErrorMessage
        {
            get => this.errorMessage;
            set
            {
                this.errorMessage = value;
                this.OnPropertyChanged(nameof(this.ErrorMessage));
            }
        }

        /// <summary>
        /// Gets or sets a value indicating whether the login is enabled.
        /// </summary>
        public bool IsLoginEnabled
        {
            get => this.isLoginEnabled;
            set
            {
                this.isLoginEnabled = value;
                this.OnPropertyChanged(nameof(this.IsLoginEnabled));
            }
        }

        /// <summary>
        /// Gets or sets the captcha text.
        /// </summary>
        public string? CaptchaText
        {
            get => this.captchaText;
            set
            {
                this.captchaText = value;
                this.OnPropertyChanged(nameof(this.CaptchaText));
            }
        }

        /// <summary>
        /// Gets or sets the captcha entered code.
        /// </summary>
        public string? CaptchaEnteredCode
        {
            get => this.captchaEnteredCode;
            set
            {
                this.captchaEnteredCode = value;
                this.OnPropertyChanged(nameof(this.CaptchaEnteredCode));
            }
        }

        /// <summary>
        /// Gets the login command.
        /// </summary>
        public ICommand LoginCommand { get; }

        /// <summary>
        /// Executes the login.
        /// </summary>
        private async Task ExecuteLogin()
        {
            this.ErrorMessage = string.Empty;
            this.hasAttemptedLogin = true;  // Mark that the user has attempted to log in

            if (string.IsNullOrEmpty(this.Email))
            {
                this.ErrorMessage = "Email is required";
                return;
            }

            if (string.IsNullOrEmpty(this.Password))
            {
                this.ErrorMessage = "Password is required";
                return;
            }

            if (string.IsNullOrEmpty(this.CaptchaEnteredCode))
            {
                this.ErrorMessage = "Captcha is required";
                return;
            }

            if (string.IsNullOrEmpty(this.CaptchaText))
            {
                this.ErrorMessage = "Captcha is required";
                return;
            }

            // var user = await UserService.GetUserByEmail(Email);
            var (success, msg, user) = await this.UserService.LoginAsync(this.Email, this.Password, this.CaptchaEnteredCode, this.CaptchaText);

            if (!success)
            {
                this.ErrorMessage = msg;
                if (msg == "Captcha verification failed.")
                {
                    this.GenerateCaptcha();
                }

                return;
            }

            if (await this.UserService.IsUserSuspended(this.Email))
            {
                TimeSpan remainingTime = this.banEndTime - DateTime.Now;
                this.ErrorMessage = $"Too many failed attempts. Try again in {remainingTime.Seconds}s";
                return;
            }

            if (!await this.UserService.CanUserLogin(this.Email, this.Password))
            {
                this.failedAttempts = await this.UserService.GetFailedLoginsCountByEmail(this.Email) + 1;
                if (user != null)
                {
                    await this.UserService.UpdateUserFailedLoginsCount(user, this.failedAttempts);
                }

                this.ErrorMessage = $"Login failed";

                if (this.failedAttempts >= 5)
                {
                    await this.UserService.SuspendUserForSeconds(this.Email, 5);
                    this.banEndTime = DateTime.Now.AddSeconds(5);
                    this.StartBanTimer();
                }
            }
            else
            {
                this.ErrorMessage = "Login successful!";
                this.failedAttempts = 0;
                if (user != null)
                {
                    await this.UserService.UpdateUserFailedLoginsCount(user, 0);
                    this.IsLoginEnabled = true;
                    await this.successCallback.OnLoginSuccess(user);
                }
            }

            this.OnPropertyChanged(nameof(this.FailedAttemptsText));
        }

        private void StartBanTimer()
        {
            this.IsLoginEnabled = false;
            this.ErrorMessage = "Too many failed attempts. Please wait...";

            // DispatcherTimer only works in UI threads, so we can't test this
            this.banTimer = new DispatcherTimer();
            this.banTimer.Interval = TimeSpan.FromSeconds(1);
            this.banTimer.Tick += async (s, e) =>
            {
                TimeSpan remainingTime = this.banEndTime - DateTime.Now;

                if (remainingTime.TotalSeconds <= 0)
                {
                    this.banTimer.Stop();
                    this.IsLoginEnabled = true;
                    this.ErrorMessage = string.Empty;

                    if (this.Email != null)
                    {
                        await this.UserService.ResetFailedLogins(this.Email);
                        this.failedAttempts = 0;  // Reset in UI
                        this.OnPropertyChanged(nameof(this.FailedAttemptsText));
                    }
                }
                else
                {
                    this.ErrorMessage = $"Too many failed attempts. Try again in {remainingTime.Seconds}s";
                }
            };
            this.banTimer.Start();
        }

        /// <summary>
        /// Generates the captcha.
        /// </summary>
        private void GenerateCaptcha()
        {
            this.CaptchaText = this.captchaService.GenerateCaptcha();
        }

        /// <summary>
        /// Raises the property changed event.
        /// </summary>
        private void OnPropertyChanged(string propertyName)
        {
            this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
