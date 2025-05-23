using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using MarketMinds.Shared.Models;
using MarketMinds.ViewModels.Admin;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Navigation;
using ViewModelLayer.ViewModel;
using Windows.Foundation;
using Windows.Foundation.Collections;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.
namespace MarketMinds.Views.Pages
{
    public sealed partial class AdminUsersPage : UserControl
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="AdminUsersPage"/> class.
        /// </summary>
        public AdminUsersPage()
        {
            this.InitializeComponent();
            this.ViewModel = MarketMinds.App.AdminViewModel;
            _ = this.ViewModel.LoadDataAsync();
        }

        /// <summary>
        /// Gets or sets the view model for this view.
        /// </summary>
        public IAdminViewModel ViewModel
        {
            get => (IAdminViewModel)this.DataContext;
            set => this.DataContext = value;
        }
    }
}
