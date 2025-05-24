using System;
using System.Runtime.InteropServices;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Navigation;

namespace MarketMinds
{
    [ComImport]
    [InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    [Guid("EECDBF0E-BAE9-4CB6-A68E-9598E1CB57BB")]
    internal interface IWindowNative
    {
        IntPtr WindowHandle { get; }
    }
    public sealed partial class LoginWindow : Window
    {
        public LoginWindow()
        {
            this.InitializeComponent();
            this.Title = "MarketMinds";
            // Navigate to the login view first
            ContentFrame.Navigate(typeof(MarketMinds.Views.LoginView), App.LoginViewModel);
            // Subscribe to the navigation events
            // ContentFrame.Navigated += ContentFrame_Navigated;
        }

        // private void ContentFrame_Navigated(object sender, NavigationEventArgs e)
        // {
        //     // Check if we've navigated back to LoginView with a successful login
        //     // if (e.SourcePageType == typeof(MarketMinds.Views.LoginView) && App.CurrentUser != null)
        //     // {
        //     //    // User has successfully logged in, show main application
        //     //    App.ShowMainWindow();
        //     // }
        // }
    }
}