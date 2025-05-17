using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using MarketMinds.Shared.Models;
using MarketMinds.Helpers.Selectors;
using MarketMinds.Shared.Services.ImagineUploadService;
using MarketMinds.ViewModels;
using Marketplace_SE;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Navigation;
// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.
namespace MarketMinds.Views.Pages2
{
    public sealed partial class ChatPage : Page
    {
        private ChatViewModel chatViewModel;
        private IImageUploadService imageUploadService;

        private User currentUser;
        private User targetUser;

        private DispatcherTimer updateTimer;
        private List<string> chatHistory = new();
        private ObservableCollection<Message> displayedMessages = new();

        private bool isInitializing = false;
        private bool initialLoadComplete = false;
        private const int INVALID_MESSAGE_ID = 0;
        private const int TIMER_SECONDS = 1;

        public ChatPage()
        {
            this.InitializeComponent();

            chatViewModel = App.ChatViewModel;
            imageUploadService = App.ImageUploadService;

            // Initialize UI elements
            ChatListView.ItemsSource = displayedMessages;
            LoadingIndicator.IsActive = false;
            LoadingIndicator.Visibility = Visibility.Collapsed;

            // Set up event handlers
            SendButton.Click += SendButton_Click;
            AttachButton.Click += AttachButton_Click;
            BackButton.Click += BackButton_Click;
            ExportButton.Click += ExportButton_Click;

            // Set up message box
            MessageBox.AcceptsReturn = true;
            MessageBox.TextWrapping = TextWrapping.Wrap;
            MessageBox.PlaceholderText = "Type a message...";
        }

        protected override async void OnNavigatedTo(NavigationEventArgs eventArgs)
        {
            base.OnNavigatedTo(eventArgs);
            displayedMessages.Clear();
            chatHistory.Clear();

            if (eventArgs.Parameter is UserNotSoldOrder selectedOrder)
            {
                currentUser = App.CurrentUser;
                targetUser = new User { Id = selectedOrder.SellerId, Username = "Seller", Email = string.Empty };
            }
            else if (eventArgs.Parameter is User user)
            {
                currentUser = App.CurrentUser;
                targetUser = user;
            }
            else
            {
                currentUser = App.CurrentUser;
            }

            SetupTemplateSelector();
            isInitializing = true;

            try
            {
                await chatViewModel.InitializeAsync(currentUser.Id.ToString());

                // Load initial messages
                await LoadInitialChatHistoryAsync();
                initialLoadComplete = true;

                // Start polling timer
                SetupUpdateTimer();
            }
            catch (Exception chatInitializeException)
            {
                ShowErrorDialog("Chat initialization error", chatInitializeException.Message);
            }
            finally
            {
                isInitializing = false;
            }
        }

        protected override void OnNavigatedFrom(NavigationEventArgs navigationEventArgs)
        {
            base.OnNavigatedFrom(navigationEventArgs);
            StopUpdateTimer();
        }

        private void SetupTemplateSelector()
        {
            var selector = new ChatMessageTemplateSelector
            {
                MyUserId = currentUser.Id,
                MyMessageTemplate = this.Resources["MyTextMessageTemplate"] as DataTemplate,
                OtherMessageTemplate = this.Resources["TargetTextMessageTemplate"] as DataTemplate
            };
            ChatListView.ItemTemplateSelector = selector;
        }

        private void SetupHardcodedUsers(int template)
        {
            switch (template)
            {
                case 0:
                    currentUser = new User { Id = 0, Username = "test1", Email = string.Empty };
                    targetUser = new User { Id = 1, Username = "test2", Email = string.Empty };
                    break;
                case 1:
                    currentUser = new User { Id = 1, Username = "test2", Email = string.Empty };
                    targetUser = new User { Id = 0, Username = "test1", Email = string.Empty };
                    break;
                case 2:
                    currentUser = new User { Id = 2, Username = "test3", Email = string.Empty };
                    targetUser = new User { Id = 3, Username = "test4", Email = string.Empty };
                    break;
                case 3: // Admin case
                    currentUser = new User { Id = 3, Username = "test4", Email = string.Empty }; // Assuming ID 3 is admin
                    targetUser = new User { Id = 2, Username = "test3", Email = string.Empty };
                    break;
                default:
                    // Handle invalid template?
                    currentUser = App.CurrentUser;
                    break;
            }
        }

        private void SetupUpdateTimer()
        {
            if (updateTimer == null)
            {
                updateTimer = new DispatcherTimer();
                updateTimer.Interval = TimeSpan.FromSeconds(TIMER_SECONDS);
                updateTimer.Tick += UpdateTimer_Tick;
            }
            updateTimer.Start();
        }

        private void StopUpdateTimer()
        {
            if (updateTimer != null)
            {
                updateTimer.Stop();
                updateTimer.Tick -= UpdateTimer_Tick;
                updateTimer = null;
            }
        }

        private async void UpdateTimer_Tick(object sender, object eventArgs)
        {
            if (isInitializing || !initialLoadComplete)
            {
                return;
            }
            try
            {
                var messages = await chatViewModel.GetMessagesAsync();
                if (messages?.Count > 0)
                {
                    var existingIds = displayedMessages.Select(m => m.Id).ToHashSet();

                    foreach (var message in messages)
                    {
                        if (!displayedMessages.Any(existingMessage => existingMessage.Id == message.Id && existingMessage.Id != INVALID_MESSAGE_ID))
                        {
                            AddMessageToDisplay(message);
                        }
                    }
                }
            }
            catch (Exception newMessagesException)
            {
                Debug.WriteLine($"Error checking for new messages: {newMessagesException.Message}");
            }
        }

        private async Task LoadInitialChatHistoryAsync()
        {
            var initialMessages = await chatViewModel.GetMessagesAsync();
            if (initialMessages != null)
            {
                foreach (var message in initialMessages)
                {
                    AddMessageToDisplay(message);
                }
            }
        }

        private void AddMessageToDisplay(Message message)
        {
            displayedMessages.Add(message);

            string timeString = DateTime.Now.ToString("[HH:mm]");
            bool isMe = message.UserId == currentUser.Id;
            string prefix = isMe ? "[You]" : "[Peer]";
            string contentForExport = message.Content;

            chatHistory.Add($"{timeString} {prefix}: {contentForExport}");
        }

        // --- Event Handlers ---
        private async void SendButton_Click(object sender, RoutedEventArgs routedEventArgs)
        {
            string messageText = MessageBox.Text.Trim();
            if (string.IsNullOrEmpty(messageText) || chatViewModel == null || isInitializing)
            {
                return;
            }

            MessageBox.Text = string.Empty;
            MessageBox.IsEnabled = false;
            SendButton.IsEnabled = false;

            try
            {
                var message = await chatViewModel.SendMessageAsync(messageText);

                if (message != null)
                {
                    AddMessageToDisplay(message);
                }
                else
                {
                    ShowErrorDialog("Message error", "Failed to send message.");
                }
            }
            catch (Exception ex)
            {
                ShowErrorDialog("Message error", "Failed to send message: " + ex.Message);
            }
            finally
            {
                MessageBox.IsEnabled = true;
                SendButton.IsEnabled = true;
                MessageBox.Focus(FocusState.Programmatic);
            }
        }

        private async void AttachButton_Click(object sender, RoutedEventArgs routedEventArgs)
        {
            if (chatViewModel == null || isInitializing)
            {
                return;
            }

            Windows.Storage.Pickers.FileOpenPicker picker = new Windows.Storage.Pickers.FileOpenPicker();
            picker.ViewMode = Windows.Storage.Pickers.PickerViewMode.Thumbnail;
            picker.FileTypeFilter.Add(".png");
            picker.FileTypeFilter.Add(".gif");

            Windows.Storage.StorageFile file = await picker.PickSingleFileAsync();
            if (file != null)
            {
                try
                {
                    using (var stream = await file.OpenAsync(Windows.Storage.FileAccessMode.Read))
                    {
                        // Pass the stream and file name to the service
                        string imageUrl = await imageUploadService.UploadImage(stream.AsStreamForRead(), file.Name);
                        if (!string.IsNullOrEmpty(imageUrl))
                        {
                            // Prepend image URL to message box or handle as needed
                            // For example, sending it as a message immediately:
                            var imageMessage = await chatViewModel.SendMessageAsync(imageUrl);
                            if (imageMessage != null)
                            {
                                AddMessageToDisplay(imageMessage);
                            }
                            // Or, if you want to add it to the text box for the user to send:
                            // MessageBox.Text = imageUrl + "\n" + MessageBox.Text;
                        }
                    }
                }
                catch (Exception ex)
                {
                    Debug.WriteLine($"Error uploading image: {ex.Message}");
                    ShowErrorDialog("Upload Error", "Failed to upload image: " + ex.Message);
                }
            }
        }

        private async void ExportButton_Click(object sender, RoutedEventArgs routedEventArgs)
        {
            var currentElement = sender as UIElement;
            if (currentElement == null)
            {
                return;
            }

            var savePicker = new Windows.Storage.Pickers.FileSavePicker
            {
                SuggestedStartLocation = Windows.Storage.Pickers.PickerLocationId.DocumentsLibrary,
                SuggestedFileName = $"{currentUser.Username}_{targetUser.Username}_chat_history.txt"
            };
            savePicker.FileTypeChoices.Add("Text File", new List<string> { ".txt" });

            var hWnd = WinRT.Interop.WindowNative.GetWindowHandle(currentElement.XamlRoot);
            WinRT.Interop.InitializeWithWindow.Initialize(savePicker, hWnd);

            var file = await savePicker.PickSaveFileAsync();
            if (file != null)
            {
                try
                {
                    await Windows.Storage.FileIO.WriteLinesAsync(file, chatHistory);
                }
                catch (Exception chatHistoryExportException)
                {
                    ShowErrorDialog("Export error", "Failed to export chat history.");
                    return;
                }
            }
        }

        private void BackButton_Click(object sender, RoutedEventArgs routedEventArgs)
        {
            StopUpdateTimer();
            if (Frame.CanGoBack)
            {
                Frame.GoBack();
            }
            else
            {
                Frame.Navigate(typeof(MainMarketplacePage));
            }
        }

        private async void ShowErrorDialog(string title, string message)
        {
            ContentDialog dialog = new ContentDialog
            {
                Title = title,
                Content = message,
                CloseButtonText = "OK",
                XamlRoot = this.Content.XamlRoot
            };
            await dialog.ShowAsync();
        }

        private Window GetCurrentWindow()
        {
            var currentWindow = Window.Current;
            if (currentWindow == null)
            {
                ShowErrorDialog("Window error", "Could not retrieve the current window.");
                return null;
            }
            return currentWindow;
        }
        private void ShowLoadingIndicator(bool show)
        {
            LoadingIndicator.IsActive = show;
            LoadingIndicator.Visibility = show ? Visibility.Visible : Visibility.Collapsed;
        }
    }
}
