using System;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Navigation;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Windows.System;
using MarketMinds.ViewModels;
using MarketMinds;
using MarketMinds.Views.Pages2;

namespace Marketplace_SE
{
    public sealed partial class ChatBotPage : Page
    {
        private readonly ChatBotViewModel chatBotViewModel;

        public ChatBotPage()
        {
            this.InitializeComponent();
            chatBotViewModel = App.ChatBotViewModel;
            if (App.CurrentUser != null)
            {
                chatBotViewModel.SetCurrentUser(App.CurrentUser);
            }
        }

        protected override void OnNavigatedTo(NavigationEventArgs navigationEventArgs)
        {
            base.OnNavigatedTo(navigationEventArgs);
            chatBotViewModel.InitializeChat();
            UpdateChatUI();
        }

        private void ChatBotOptionButton_Click(object sender, RoutedEventArgs eventArgs)
        {
        }
        private void OnButtonClickChatBotKill(object sender, RoutedEventArgs eventArgs)
        {
            var helpWindow = new Microsoft.UI.Xaml.Window();
            helpWindow.Content = new GetHelpPage();
            helpWindow.Activate();

            if (this.Frame != null && this.Frame.CanGoBack)
            {
                this.Frame.GoBack();
            }
        }

        private void UserMessageTextBox_KeyDown(object sender, KeyRoutedEventArgs e)
        {
            if (e.Key == VirtualKey.Enter && !e.KeyStatus.IsMenuKeyDown && !e.KeyStatus.WasKeyDown)
            {
                SendUserMessage();
                e.Handled = true;
            }
        }

        private void SendMessageButton_Click(object sender, RoutedEventArgs e)
        {
            SendUserMessage();
        }

        private async void SendUserMessage()
        {
            string userMessage = UserMessageTextBox.Text.Trim();

            if (string.IsNullOrWhiteSpace(userMessage))
            {
                return;
            }

            Border userMessageBorder = new Border
            {
                Background = new SolidColorBrush(Microsoft.UI.Colors.LightGray),
                CornerRadius = new CornerRadius(8),
                Padding = new Thickness(15, 10, 15, 10),
                HorizontalAlignment = HorizontalAlignment.Right,
                MaxWidth = 500,
                Margin = new Thickness(0, 5, 0, 5)
            };

            TextBlock userMessageText = new TextBlock
            {
                Text = userMessage,
                TextWrapping = TextWrapping.Wrap
            };

            userMessageBorder.Child = userMessageText;
            ChatMessagesPanel.Children.Add(userMessageBorder);

            UserMessageTextBox.Text = string.Empty;

            UserMessageTextBox.IsEnabled = false;
            SendMessageButton.IsEnabled = false;

            try
            {
                string responseText = await chatBotViewModel.SendMessageAsync(userMessage);
                Border botResponseBorder = new Border
                {
                    Background = new SolidColorBrush(Microsoft.UI.Colors.DodgerBlue),
                    CornerRadius = new CornerRadius(8),
                    Padding = new Thickness(15, 10, 15, 10),
                    HorizontalAlignment = HorizontalAlignment.Left,
                    MaxWidth = 500,
                    Margin = new Thickness(0, 5, 0, 5)
                };

                TextBlock botResponseText = new TextBlock
                {
                    Text = responseText,
                    TextWrapping = TextWrapping.Wrap,
                    Foreground = new SolidColorBrush(Microsoft.UI.Colors.White)
                };

                botResponseBorder.Child = botResponseText;
                ChatMessagesPanel.Children.Add(botResponseBorder);
            }
            catch (Exception ex)
            {
                string errorMessage = "I'm sorry, an error occurred. Please try again.";
                Border errorBorder = new Border
                {
                    Background = new SolidColorBrush(Microsoft.UI.Colors.DodgerBlue),
                    CornerRadius = new CornerRadius(8),
                    Padding = new Thickness(15, 10, 15, 10),
                    HorizontalAlignment = HorizontalAlignment.Left,
                    MaxWidth = 500,
                    Margin = new Thickness(0, 5, 0, 5)
                };

                TextBlock errorText = new TextBlock
                {
                    Text = errorMessage,
                    TextWrapping = TextWrapping.Wrap,
                    Foreground = new SolidColorBrush(Microsoft.UI.Colors.White)
                };

                errorBorder.Child = errorText;
                ChatMessagesPanel.Children.Add(errorBorder);
            }
            finally
            {
                UserMessageTextBox.IsEnabled = true;
                SendMessageButton.IsEnabled = true;
                UserMessageTextBox.Focus(FocusState.Programmatic);
            }
        }

        private void UpdateChatUI()
        {
        }
    }
}