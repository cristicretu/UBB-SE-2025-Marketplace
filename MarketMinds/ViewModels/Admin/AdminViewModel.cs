using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using MarketMinds.Shared.Models;
using MarketMinds.Shared.Services;
using MarketMinds.Shared.Services.UserService;
using Microsoft.UI.Xaml.Controls;

namespace MarketMinds.ViewModels.Admin
{
    public class AdminViewModel : IAdminViewModel, INotifyPropertyChanged
    {
        private readonly IAdminService adminService;
        private readonly IUserService userService;
        private readonly IAnalyticsService analyticsService;

        private ObservableCollection<IUserRowViewModel> users = new();
        private int totalUsersCount;

        public event PropertyChangedEventHandler? PropertyChanged;

        public AdminViewModel(
            IAdminService adminService,
            IAnalyticsService analyticsService,
            IUserService userService)
        {
            this.adminService = adminService;
            this.analyticsService = analyticsService;
            this.userService = userService;
        }

        public ObservableCollection<IUserRowViewModel> Users
        {
            get => users;
            private set
            {
                users = value;
                OnPropertyChanged();
            }
        }

        public int TotalUsersCount
        {
            get => totalUsersCount;
            private set
            {
                totalUsersCount = value;
                OnPropertyChanged();
            }
        }

        public async Task LoadDataAsync()
        {
            var userList = await userService.GetAllUsers();
            var userViewModels = userList.Select(user => new UserRowViewModel(user, adminService, this)).ToList();
            Users = new ObservableCollection<IUserRowViewModel>(userViewModels);

            TotalUsersCount = await analyticsService.GetTotalNumberOfUsers();
        }

        public async void RefreshUsers()
        {
            await LoadDataAsync();
            OnPropertyChanged(nameof(Users));
        }

        public void BanUser(User user)
        {
            if (user == null)
            {
                return;
            }

            var toRemove = Users.FirstOrDefault(u => u.Username == user.Username);
            if (toRemove != null)
            {
                Users.Remove(toRemove);
                ShowBanDialog(user.Username);
            }
        }

        private static void ShowBanDialog(string username)
        {
            var dialog = new ContentDialog
            {
                Title = "User Banned",
                Content = $"{username} has been banned.",
                CloseButtonText = "OK",
                XamlRoot = App.MainWindow?.Content.XamlRoot,
            };
            _ = dialog.ShowAsync(); // fire-and-forget
        }

        private void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
