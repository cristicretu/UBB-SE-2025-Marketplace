using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Navigation;
using Microsoft.UI.Windowing;
using MarketMinds.Shared.Models;
using MarketMinds.Views.Pages;
using System.Diagnostics;
using Microsoft.UI;
using Windows.Graphics;
using WinRT.Interop;
using XamlWindow = Microsoft.UI.Xaml.Window;

namespace MarketMinds.Views
{
    /// <summary>
    /// Main window for the MarketMinds application. Contains a header and a Frame for content navigation.
    /// </summary>
    public sealed partial class HomePageView : XamlWindow
    {
        private const int BUYER_TYPE = 2;
        private const int SELLER_TYPE = 3;
        private bool isBuyer;
        private bool isSeller;
        private DispatcherTimer resizeTimer;

        // Custom title bar fields
        private AppWindow appWindow;
        private OverlappedPresenter presenter;
        private const int TitleBarHeight = 32;

        // Public property to expose ContentFrame for navigation from other classes
        public Frame MainContentFrame => ContentFrame;

        public HomePageView()
        {
            this.InitializeComponent();

            // Enable custom title bar
            ExtendsContentIntoTitleBar = true;

            // Initialize custom title bar
            InitializeCustomTitleBar();

            // Initialize resize timer
            resizeTimer = new DispatcherTimer();
            resizeTimer.Interval = TimeSpan.FromMilliseconds(300); // Wait 300ms after resize stops
            resizeTimer.Tick += ResizeTimer_Tick;

            // Set initial window size to 2/3 of screen and handle minimum size constraints
            SetInitialWindowSize();
            this.SizeChanged += HomePageView_SizeChanged;

            // Navigate to the MarketMindsPage by default
            ContentFrame.Navigate(typeof(MarketMindsPage));

            // Configure UI based on user type
            ConfigureUIForUserType();

            // Close notification popup when window is deactivated
            this.Activated += HomePageView_Activated;
        }

        private void InitializeCustomTitleBar()
        {
            try
            {
                // Get window handle and ID for AppWindow
                IntPtr hWnd = WindowNative.GetWindowHandle(this);
                WindowId windowId = Win32Interop.GetWindowIdFromWindow(hWnd);
                appWindow = AppWindow.GetFromWindowId(windowId);
                presenter = appWindow.Presenter as OverlappedPresenter;

                if (AppWindowTitleBar.IsCustomizationSupported())
                {
                    var titleBar = appWindow.TitleBar;
                    titleBar.ExtendsContentIntoTitleBar = true;

                    // Set transparent backgrounds for buttons
                    titleBar.ButtonBackgroundColor = Colors.Transparent;
                    titleBar.ButtonInactiveBackgroundColor = Colors.Transparent;

                    // Get theme-aware brushes for button states
                    var buttonHoverBackgroundBrush = Application.Current.Resources["SystemControlBackgroundListLowBrush"] as SolidColorBrush;
                    var buttonPressedBackgroundBrush = Application.Current.Resources["SystemControlBackgroundListMediumBrush"] as SolidColorBrush;

                    if (buttonHoverBackgroundBrush != null)
                    {
                        titleBar.ButtonHoverBackgroundColor = buttonHoverBackgroundBrush.Color;
                    }
                    else
                    {
                        titleBar.ButtonHoverBackgroundColor = Colors.Transparent;
                    }

                    if (buttonPressedBackgroundBrush != null)
                    {
                        titleBar.ButtonPressedBackgroundColor = buttonPressedBackgroundBrush.Color;
                    }
                    else
                    {
                        titleBar.ButtonPressedBackgroundColor = Colors.Transparent;
                    }

                    // Set theme-aware foreground colors
                    var foregroundBrush = Application.Current.Resources["SystemControlForegroundBaseHighBrush"] as SolidColorBrush;
                    var foregroundHoverBrush = Application.Current.Resources["SystemControlForegroundBaseHighBrush"] as SolidColorBrush;
                    var foregroundPressedBrush = Application.Current.Resources["SystemControlForegroundBaseMediumBrush"] as SolidColorBrush;

                    if (foregroundBrush != null)
                    {
                        titleBar.ButtonForegroundColor = foregroundBrush.Color;
                    }

                    if (foregroundHoverBrush != null)
                    {
                        titleBar.ButtonHoverForegroundColor = foregroundHoverBrush.Color;
                    }

                    if (foregroundPressedBrush != null)
                    {
                        titleBar.ButtonPressedForegroundColor = foregroundPressedBrush.Color;
                    }

                    // Set drag rectangles for title bar
                    SetTitleBarDragRectangles();

                    // Add padding to account for title bar height
                    ContentRoot.Margin = new Thickness(0, TitleBarHeight, 0, 0);

                    // Update drag rectangles when window size changes (this will be called in SizeChanged)
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Titlebar customization not supported: {ex.Message}");
            }
        }

        private void SetTitleBarDragRectangles()
        {
            try
            {
                if (appWindow?.TitleBar != null)
                {
                    var titleBar = appWindow.TitleBar;
                    int windowWidth = (int)appWindow.Size.Width;
                    int systemButtonsWidth = 138; // Width of minimize, maximize, close buttons

                    titleBar.SetDragRectangles(new RectInt32[]
                    {
                        new RectInt32(0, 0, windowWidth - systemButtonsWidth, TitleBarHeight)
                    });
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error setting drag rectangles: {ex.Message}");
            }
        }

        private void HomePageView_Activated(object sender, WindowActivatedEventArgs e)
        {
            if (e.WindowActivationState == WindowActivationState.Deactivated)
            {
                NotificationsPopup.IsOpen = false;
            }
        }

        private void SetInitialWindowSize()
        {
            try
            {
                // Use the custom AppWindow if available, otherwise fall back to this.AppWindow
                var appWindow = this.appWindow ?? this.AppWindow;

                // Get the display area of the primary monitor
                var displayArea = DisplayArea.GetFromWindowId(appWindow.Id, DisplayAreaFallback.Primary);
                var workArea = displayArea.WorkArea;

                // Calculate 2/3 of screen size for initial window
                int initialWidth = (int)(workArea.Width * 2.0 / 3.0);
                int initialHeight = (int)(workArea.Height * 2.0 / 3.0);

                // Set the window size
                appWindow.Resize(new Windows.Graphics.SizeInt32(initialWidth, initialHeight));

                // Center the window on screen
                int x = (workArea.Width - initialWidth) / 2;
                int y = (workArea.Height - initialHeight) / 2;
                appWindow.Move(new Windows.Graphics.PointInt32(x, y));
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error setting window size: {ex.Message}");
            }
        }

        private void HomePageView_SizeChanged(object sender, WindowSizeChangedEventArgs args)
        {
            // Reset the timer each time the window is resized
            // This ensures we only check minimum size after resizing stops
            resizeTimer.Stop();
            resizeTimer.Start();

            // Update title bar drag rectangles
            SetTitleBarDragRectangles();
        }

        private void ResizeTimer_Tick(object sender, object e)
        {
            // Stop the timer
            resizeTimer.Stop();

            // Enforce minimum window size (1/3 of screen)
            var displayArea = DisplayArea.Primary;
            var workArea = displayArea.WorkArea;

            int minWidth = (int)(workArea.Width / 3.0);
            int minHeight = (int)(workArea.Height / 3.0);

            var currentSize = this.AppWindow.Size;
            bool needsResize = false;
            int newWidth = currentSize.Width;
            int newHeight = currentSize.Height;

            if (currentSize.Width < minWidth)
            {
                newWidth = minWidth;
                needsResize = true;
            }

            if (currentSize.Height < minHeight)
            {
                newHeight = minHeight;
                needsResize = true;
            }

            if (needsResize)
            {
                this.AppWindow.Resize(new Windows.Graphics.SizeInt32(newWidth, newHeight));
            }
        }

        /// <summary>
        /// Configures the UI elements based on the current user type
        /// </summary>
        private void ConfigureUIForUserType()
        {
            // Get the current user type from the App
            int userType = App.CurrentUser.UserType;
            string username = App.CurrentUser.Username;

            isBuyer = userType == BUYER_TYPE;
            isSeller = userType == SELLER_TYPE;

            // Configure buyer-specific UI elements
            ChatSupportButton.Visibility = isBuyer ? Visibility.Visible : Visibility.Collapsed;
            NotificationsButton.Visibility = isBuyer ? Visibility.Visible : Visibility.Collapsed;
            WishlistButton.Visibility = isBuyer ? Visibility.Visible : Visibility.Collapsed;
            CartButton.Visibility = isBuyer ? Visibility.Visible : Visibility.Collapsed;

            // Configure seller-specific UI elements
            CreateListingButton.Visibility = isSeller ? Visibility.Visible : Visibility.Collapsed;

            // Configure Profile button text
            string userRole = isBuyer ? "(buyer)" : (isSeller ? "(seller)" : string.Empty);
            ProfileButton.Label = $"{username} {userRole}";
        }

        private void MarketMinds_Title_Click(object sender, RoutedEventArgs e)
        {
            if (ContentFrame.Content is MarketMindsPage)
            {
                return; // if the MarketMindsPage is already in the frame, do not navigate to it again
            }
            // Navigate to the MarketMindsPage when the MarketMinds title is clicked
            ContentFrame.Navigate(typeof(MarketMindsPage));
        }

        /// <summary>
        /// Navigate to the appropriate product listing page when a button is clicked
        /// </summary>
        private void AppNavBarButton_Click(object sender, RoutedEventArgs e)
        {
            if (sender is AppBarButton button)
            {
                switch (button.Tag)
                {
                    // Only buyers' cases
                    case "ChatSupport":
                        if (ContentFrame.Content is HelpPage)
                        {
                            return; // if the HelpPage is already in the frame, do not navigate to it again
                        }
                        ContentFrame.Navigate(typeof(HelpPage));
                        break;
                    case "Notifications":
                        ShowNotificationsPopup();
                        break;
                    case "Wishlist":
                        if (ContentFrame.Content is WishlistView)
                        {
                            return; // if the WishlistView is already in the frame, do not navigate to it again
                        }
                        ContentFrame.Navigate(typeof(WishlistView));
                        break;
                    case "Cart":
                        if (ContentFrame.Content is MyCartView)
                        {
                            return; // if the MyCartView is already in the frame, do not navigate to it again
                        }
                        ContentFrame.Navigate(typeof(MyCartView));
                        break;
                    case "MyAccount":
                        // This is handled in the ProfileMenuItem_Click method because it has a submenu of buttons
                        break;
                }
            }

            if (sender is Button button1)
            {
                switch (button1.Tag)
                {
                    // Only sellers' cases
                    case "CreateListing":
                        if (ContentFrame.Content is CreateListingView)
                        {
                            return; // if the CreateListingView is already in the frame, do not navigate to it again
                        }
                        ContentFrame.Navigate(typeof(CreateListingView));
                        break;
                }
            }
        }

        /// <summary>
        /// Handle profile menu item selections
        /// </summary>
        private void ProfileMenuItem_Click(object sender, RoutedEventArgs e)
        {
            if (sender is MenuFlyoutItem menuItem)
            {
                switch (menuItem.Tag)
                {
                    case "MyAccount":
                        if (isBuyer)
                        {
                            // Show buyer profile page in frame
                            if (ContentFrame.Content is BuyerProfileView)
                            {
                                return; // if the BuyerProfileView is already in the frame, do not navigate to it again
                            }
                            ContentFrame.Navigate(typeof(BuyerProfileView));
                        }
                        else if (isSeller)
                        {
                            // Show seller profile page in frame
                            if (ContentFrame.Content is SellerProfileView)
                            {
                                return; // if the SellerProfileView is already in the frame, do not navigate to it again
                            }
                            ContentFrame.Navigate(typeof(SellerProfileView));
                        }
                        break;
                    case "OrderHistory":
                        if (ContentFrame.Content is OrderHistoryView)
                        {
                            return; // if the OrderHistoryView is already in the frame, do not navigate to it again
                        }
                        ContentFrame.Navigate(typeof(OrderHistoryView));
                        break;
                    case "MyReviews":
                        if (ContentFrame.Content is MyReviewsView)
                        {
                            return; // if the MyReviewsView is already in the frame, do not navigate to it again
                        }
                        ContentFrame.Navigate(typeof(MyReviewsView));
                        break;
                    case "SignOut":
                        App.CloseHomePageWindow();
                        break;
                }
            }
        }

        /// <summary>
        /// Shows a popup containing the notifications UI
        /// </summary>
        private void ShowNotificationsPopup()
        {
            // Toggle the popup visibility if it's already open
            if (NotificationsPopup.IsOpen)
            {
                NotificationsPopup.IsOpen = false;
                return;
            }

            // Position the popup near the notification button
            if (NotificationsButton != null)
            {
                // Get the position of the notifications button
                GeneralTransform transform = NotificationsButton.TransformToVisual(null);
                Point point = transform.TransformPoint(new Point(0, 0));

                // Position the popup below the button
                NotificationsPopup.HorizontalOffset = point.X - 400; // Offset to center it
                NotificationsPopup.VerticalOffset = point.Y + NotificationsButton.ActualHeight;
            }

            // Navigate to the MainNotificationWindow in the notifications frame
            NotificationsFrame.Navigate(typeof(MainNotificationWindow));

            // Show the popup
            NotificationsPopup.IsOpen = true;
        }
    }
}
