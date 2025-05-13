using System;
using System.Threading.Tasks;
using MarketMinds.Shared.Models;
using MarketMinds.Shared.Services.UserService;

namespace MarketMinds.ViewModels
{
	public class LoginViewModel
	{
		private readonly IUserService userService;

		public string Username { get; set; } = string.Empty;
		public string Password { get; set; } = string.Empty;
		public bool LoginStatus { get; private set; }

		public User LoggedInUser { get; private set; }

		public LoginViewModel()
		{
			userService = App.UserService;
		}
		public LoginViewModel(IUserService userService)
		{
			this.userService = userService;
		}

		public async Task AttemptLogin(string username, string password)
		{
			try
			{
				Username = username;
				Password = password;

				LoggedInUser = await userService.GetUserByCredentialsAsync(Username, Password);
				LoginStatus = LoggedInUser != null;
			}
			catch (Exception loginAttemptException)
			{
				Console.WriteLine($"Login error: {loginAttemptException.Message}");
				LoginStatus = false;
				LoggedInUser = null;
			}
		}
	}
}