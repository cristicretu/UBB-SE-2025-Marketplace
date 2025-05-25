using System;
using System.Runtime.InteropServices;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Navigation;
using Microsoft.UI.Windowing;

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
        private DispatcherTimer _resizeTimer;

        public LoginWindow()
        {
            this.InitializeComponent();
            this.Title = "MarketMinds";

            // Initialize resize timer
            _resizeTimer = new DispatcherTimer();
            _resizeTimer.Interval = TimeSpan.FromMilliseconds(300); // Wait 300ms after resize stops
            _resizeTimer.Tick += ResizeTimer_Tick;

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
            _resizeTimer.Stop();
            _resizeTimer.Start();
        }

        private void ResizeTimer_Tick(object sender, object e)
        {
            // Stop the timer
            _resizeTimer.Stop();

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
    }
}