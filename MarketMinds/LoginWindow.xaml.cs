using System;
using System.Runtime.InteropServices;
using System.Diagnostics;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Navigation;
using Microsoft.UI.Windowing;
using Microsoft.UI;
using Windows.Graphics;
using WinRT.Interop;
using Microsoft.UI.Xaml.Media;
using XamlWindow = Microsoft.UI.Xaml.Window;

namespace MarketMinds
{
    [ComImport]
    [InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    [Guid("EECDBF0E-BAE9-4CB6-A68E-9598E1CB57BB")]
    internal interface IWindowNative
    {
        IntPtr WindowHandle { get; }
    }
    public sealed partial class LoginWindow : XamlWindow
    {
        private DispatcherTimer resizeTimer;

        // Custom title bar fields
        private AppWindow appWindow;
        private OverlappedPresenter presenter;
        private const int TitleBarHeight = 32;

        public LoginWindow()
        {
            this.InitializeComponent();
            this.Title = "MarketMinds";

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
            this.SizeChanged += LoginWindow_SizeChanged;

            // Navigate to the login view first
            ContentFrame.Navigate(typeof(MarketMinds.Views.LoginView), App.LoginViewModel);
            // Subscribe to the navigation events
            // ContentFrame.Navigated += ContentFrame_Navigated;
        }

        private void SetInitialWindowSize()
        {
            // Get the display area of the primary monitor
            var displayArea = DisplayArea.Primary;
            var workArea = displayArea.WorkArea;

            // Calculate 2/3 of screen size for initial window
            int initialWidth = (int)(workArea.Width * 2.0 / 3.0);
            int initialHeight = (int)(workArea.Height * 2.0 / 3.0);

            // Set the window size
            this.AppWindow.Resize(new Windows.Graphics.SizeInt32(initialWidth, initialHeight));

            // Center the window on screen
            int x = (workArea.Width - initialWidth) / 2;
            int y = (workArea.Height - initialHeight) / 2;
            this.AppWindow.Move(new Windows.Graphics.PointInt32(x, y));
        }

        private void LoginWindow_SizeChanged(object sender, WindowSizeChangedEventArgs args)
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
                    if (Content is FrameworkElement rootElement)
                    {
                        rootElement.Margin = new Thickness(0, TitleBarHeight, 0, 0);
                    }
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
    }
}