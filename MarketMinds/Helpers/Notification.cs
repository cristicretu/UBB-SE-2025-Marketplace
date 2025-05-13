using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;

namespace Marketplace_SE.Utilities
{
    public class Notification
    {
        private Window window;
        public Button OkButton;

        public Notification(string title, string message)
        {
            Create(title, message);
        }

        public Window GetWindow()
        {
            return window;
        }

        public void Show()
        {
            if (window != null)
            {
                window.Activate();
            }
        }

        private void Create(string title, string message)
        {
            window = new Window();
            window.Title = title;

            // Create a simple error message UI
            StackPanel panel = new StackPanel { Margin = new Thickness(20) };
            panel.Children.Add(new TextBlock
            {
                Text = message,
                TextWrapping = TextWrapping.Wrap,
                Margin = new Thickness(0, 0, 0, 20)
            });

            OkButton = new Button
            {
                Content = "OK",
                HorizontalAlignment = HorizontalAlignment.Center
            };

            panel.Children.Add(OkButton);
            window.Content = panel;

            var windowHandle = WinRT.Interop.WindowNative.GetWindowHandle(window);
            var windowId = Microsoft.UI.Win32Interop.GetWindowIdFromWindow(windowHandle);
            var appWindow = Microsoft.UI.Windowing.AppWindow.GetFromWindowId(windowId);

            int windowWidth = 350;
            int windowHeight = 175;
            appWindow.Resize(new Windows.Graphics.SizeInt32(windowWidth, windowHeight));

            CenterWindow(appWindow, windowWidth, windowHeight);
        }

        private void CenterWindow(Microsoft.UI.Windowing.AppWindow appWindow, int windowWidth, int windowHeight)
        {
            var displayArea = Microsoft.UI.Windowing.DisplayArea.GetFromWindowId(
                appWindow.Id,
                Microsoft.UI.Windowing.DisplayAreaFallback.Nearest);

            if (displayArea != null)
            {
                int centerX = (displayArea.WorkArea.Width - windowWidth) / 2;
                int centerY = (displayArea.WorkArea.Height - windowHeight) / 2;
                appWindow.Move(new Windows.Graphics.PointInt32(centerX, centerY));
            }
        }
    }
}
