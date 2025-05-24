using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using System.Diagnostics;
using System.Threading;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using MarketMinds.Shared.Services.ConversationService;
using MarketMinds.Shared.Services.MessageService;
using MarketMinds.Shared.Services.Interfaces;
using MarketMinds.Shared.Models;
using Windows.System;

namespace MarketMinds.Views
{
    public sealed partial class HelpPage : Page
    {
        private readonly IConversationService conversationService;
        private readonly IMessageService messageService;
        private readonly IChatbotService chatbotService;

        private ObservableCollection<ConversationViewModel> conversations;

        private const int DefaultUserId = 1;
        private const string WelcomeMessage = "Hello! I'm your shopping assistant. How can I help you today?";

        private int? currentConversationId;

        // Track whether we're already in a selection change to prevent recursion
        private bool isProcessingSelectionChange = false;

        // Track whether we're already creating a new conversation
        private bool isCreatingNewConversation = false;

        // Helper method to update the current conversation without using the selection mechanism
        private void UpdateCurrentConversation(int conversationId, bool skipLoadingMessages = false)
        {
            try
            {
                Debug.WriteLine($"[DEBUG] UpdateCurrentConversation: Setting current conversation ID to {conversationId} (skipLoadingMessages={skipLoadingMessages})");

                // Update the ID
                currentConversationId = conversationId;

                // Update conversation title and UI panels
                ConversationTitle.Text = $"Conversation {conversationId}";
                EmptyStatePanel.Visibility = Visibility.Collapsed;
                ChatPanel.Visibility = Visibility.Visible;
                MessageInputPanel.Visibility = Visibility.Visible;
                ConversationEndedText.Visibility = Visibility.Collapsed;

                // Clear existing messages
                ChatMessagesPanel.Children.Clear();
                // Load messages (don't wait) - but only if not skipped
                if (!skipLoadingMessages)
                {
                    _ = LoadMessagesAsync(conversationId);
                }
                else
                {
                    Debug.WriteLine($"[DEBUG] UpdateCurrentConversation: Skipped loading messages as requested");
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[ERROR] UpdateCurrentConversation: ERROR: {ex}");
            }
        }

        public HelpPage()
        {
            try
            {
                Debug.WriteLine("HelpPage constructor start");
                this.InitializeComponent();

                this.conversationService = App.ConversationService;
                this.messageService = App.MessageService;
                this.chatbotService = App.NewChatbotService;
                this.chatbotService.SetCurrentUser(App.CurrentUser);

                conversations = new ObservableCollection<ConversationViewModel>();
                ConversationsListView.ItemsSource = conversations;

                // Set up global exception handling for this page
                this.Loaded += async (s, e) =>
                {
                    try
                    {
                        Debug.WriteLine("HelpPage Loaded event start");
                        await LoadConversationsAsync();
                        Debug.WriteLine("HelpPage Loaded event complete");
                    }
                    catch (Exception ex)
                    {
                        Debug.WriteLine($"CRITICAL ERROR in Loaded event: {ex}");
                        ShowError($"Error loading page: {ex.Message}");
                    }
                };
                Debug.WriteLine("HelpPage constructor complete");
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"CRITICAL ERROR in HelpPage constructor: {ex}");
                // Can't show a dialog here since the page isn't initialized yet
            }
        }

        private async Task LoadConversationsAsync()
        {
            Debug.WriteLine("LoadConversationsAsync start");
            try
            {
                Debug.WriteLine("Calling GetUserConversationsAsync");
                var fetchedConversations = await conversationService.GetUserConversationsAsync(DefaultUserId);
                Debug.WriteLine($"Fetched {fetchedConversations.Count} conversations");

                conversations.Clear();
                foreach (var conversation in fetchedConversations)
                {
                    conversations.Add(new ConversationViewModel
                    {
                        Id = conversation.Id,
                        UserId = conversation.UserId,
                        DisplayText = $"Conversation {conversation.Id}"
                    });
                }

                // If no conversations exist, create one automatically
                if (conversations.Count == 0)
                {
                    Debug.WriteLine("No conversations found, creating one");
                    await CreateNewConversation();
                }
                Debug.WriteLine("LoadConversationsAsync complete");
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"ERROR in LoadConversationsAsync: {ex}");
                ShowError($"Error loading conversations: {ex.Message}");
            }
        }

        private void ConversationItem_Click(object sender, RoutedEventArgs e)
        {
            Debug.WriteLine("[DEBUG] ConversationItem_Click: START");
            try
            {
                if (sender is Button button && button.DataContext is ConversationViewModel conversation)
                {
                    Debug.WriteLine($"[DEBUG] ConversationItem_Click: Conversation clicked: ID={conversation.Id}");

                    // Check if this is already the selected conversation
                    if (currentConversationId.HasValue && currentConversationId.Value == conversation.Id)
                    {
                        Debug.WriteLine($"[DEBUG] ConversationItem_Click: Conversation {conversation.Id} is already selected, skipping");
                        return;
                    }

                    // Update current conversation - this will update UI and load messages
                    // For normal conversation clicks, we DO want to load messages
                    UpdateCurrentConversation(conversation.Id, skipLoadingMessages: false);
                    Debug.WriteLine($"[DEBUG] ConversationItem_Click: Updated to conversation {conversation.Id}");
                }
                else
                {
                    Debug.WriteLine("[ERROR] ConversationItem_Click: Unable to get conversation from button");
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[ERROR] ConversationItem_Click: ERROR: {ex}");
                ShowError($"Error selecting conversation: {ex.Message}");
            }
        }

        private async Task LoadMessagesAsync(int conversationId)
        {
            Debug.WriteLine($"[DEBUG] LoadMessagesAsync: START for conversation ID: {conversationId}");

            // Check if this call makes sense - this is a safety check to avoid race conditions
            if (currentConversationId != conversationId)
            {
                Debug.WriteLine($"[WARNING] LoadMessagesAsync: Current conversation ID ({currentConversationId}) doesn't match requested ID ({conversationId}), aborting");
                return;
            }

            try
            {
                // Clear existing messages in UI
                await RunOnUIThreadAsync(() =>
                {
                    ChatMessagesPanel.Children.Clear();
                    Debug.WriteLine($"[DEBUG] LoadMessagesAsync: Cleared messages panel");
                });

                // Check again for conversation change - abort if changed during async operations
                if (currentConversationId != conversationId)
                {
                    Debug.WriteLine($"[WARNING] LoadMessagesAsync: Current conversation changed during processing, aborting");
                    return;
                }

                // Fetch messages from service
                List<Message> fetchedMessages = null;
                try
                {
                    Debug.WriteLine($"[DEBUG] LoadMessagesAsync: Fetching messages for conversation {conversationId}");
                    fetchedMessages = await messageService.GetMessagesLegacyAsync(conversationId);
                    Debug.WriteLine($"[DEBUG] LoadMessagesAsync: Fetched {fetchedMessages.Count} messages");
                }
                catch (Exception fetchEx)
                {
                    Debug.WriteLine($"[ERROR] LoadMessagesAsync: Error fetching messages: {fetchEx}");
                    // Create empty list to continue
                    fetchedMessages = new List<Message>();
                }

                // Check again for conversation change
                if (currentConversationId != conversationId)
                {
                    Debug.WriteLine($"[WARNING] LoadMessagesAsync: Current conversation changed after fetching messages, aborting");
                    return;
                }

                // If no messages, add welcome message
                if (fetchedMessages.Count == 0)
                {
                    Debug.WriteLine($"[DEBUG] LoadMessagesAsync: No messages found, adding welcome message");
                    await AddMessageToUIAsync(WelcomeMessage, true);
                    return;
                }

                // Add each message to UI
                Debug.WriteLine($"[DEBUG] LoadMessagesAsync: Adding {fetchedMessages.Count} messages to UI");
                int messageIndex = 0;
                foreach (var message in fetchedMessages)
                {
                    // Check for conversation change
                    if (currentConversationId != conversationId)
                    {
                        Debug.WriteLine($"[WARNING] LoadMessagesAsync: Current conversation changed while adding messages, aborting");
                        return;
                    }

                    // Determine if message is from bot (even index) or user (odd index)
                    bool isFromBot = (messageIndex == 0) || (messageIndex % 2 == 0);
                    messageIndex++;

                    // Add the message to UI
                    await AddMessageToUIAsync(message.Content, isFromBot);
                }

                // Scroll to bottom
                await Task.Delay(100);
                await ScrollToBottomAsync();

                Debug.WriteLine($"[DEBUG] LoadMessagesAsync: COMPLETE for conversation {conversationId}");
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[ERROR] LoadMessagesAsync: CRITICAL ERROR: {ex}");
                ShowError($"Error loading messages: {ex.Message}");
            }
        }

        private async void NewConversation_Click(object sender, RoutedEventArgs e)
        {
            Debug.WriteLine("NewConversation_Click start");

            // Reference to the button
            Button button = null;
            if (sender is Button btn)
            {
                button = btn;
                // Disable the button immediately
                try
                {
                    button.IsEnabled = false;
                }
                catch (Exception btnEx)
                {
                    Debug.WriteLine($"Error disabling button: {btnEx.Message}");
                }
            }

            try
            {
                await CreateNewConversation();
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"ERROR in NewConversation_Click: {ex}");
                ShowError($"Error creating conversation: {ex.Message}");
            }
            finally
            {
                // Re-enable the button
                if (button != null)
                {
                    try
                    {
                        button.IsEnabled = true;
                    }
                    catch (Exception btnEx)
                    {
                        Debug.WriteLine($"Error re-enabling button: {btnEx.Message}");
                    }
                }
            }
        }

        private async Task CreateNewConversation()
        {
            Debug.WriteLine("[DEBUG] CreateNewConversation: START");
            try
            {
                // First check if we already have a new conversation in progress
                if (isCreatingNewConversation)
                {
                    Debug.WriteLine("[DEBUG] CreateNewConversation: Already creating a new conversation, ignoring duplicate request");
                    return;
                }

                // Set flag to prevent concurrent creation
                isCreatingNewConversation = true;

                Debug.WriteLine("[DEBUG] CreateNewConversation: Calling API to create conversation");
                var conversation = await conversationService.CreateConversationAsync(DefaultUserId);
                Debug.WriteLine($"[DEBUG] CreateNewConversation: Created conversation with ID: {conversation.Id}");

                var newConversation = new ConversationViewModel
                {
                    Id = conversation.Id,
                    UserId = conversation.UserId,
                    DisplayText = $"Conversation {conversation.Id}"
                };

                // Update the UI state for the new conversation, but SKIP loading messages
                // This prevents the race condition where both UpdateCurrentConversation an
                // CreateNewConversation try to manage messages simultaneously
                UpdateCurrentConversation(conversation.Id, skipLoadingMessages: true);
                Debug.WriteLine($"[DEBUG] CreateNewConversation: Updated current conversation to ID: {conversation.Id}");

                // Create welcome message content
                Debug.WriteLine($"[DEBUG] CreateNewConversation: Welcome message: '{WelcomeMessage}'");

                // Store welcome message in database
                Debug.WriteLine("[DEBUG] CreateNewConversation: Saving welcome message to database");
                var savedMessage = await messageService.CreateMessageAsync(
                    conversation.Id,
                    DefaultUserId,
                    WelcomeMessage);
                Debug.WriteLine($"[DEBUG] CreateNewConversation: Welcome message saved with ID: {savedMessage.Id}");

                // Add the conversation to the UI list
                Debug.WriteLine("[DEBUG] CreateNewConversation: Adding conversation to UI list");
                bool conversationAdded = await AddConversationToListAsync(newConversation);
                if (!conversationAdded)
                {
                    Debug.WriteLine("[ERROR] CreateNewConversation: Failed to add conversation to UI list");
                }

                // Add the welcome message to UI
                Debug.WriteLine("[DEBUG] CreateNewConversation: Adding welcome message to UI");
                await AddMessageToUIAsync(WelcomeMessage, true);

                // Scroll to bottom
                await Task.Delay(100);
                await ScrollToBottomAsync();

                Debug.WriteLine("[DEBUG] CreateNewConversation: COMPLETE");
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[ERROR] CreateNewConversation: CRITICAL ERROR: {ex}");
                ShowError($"Error creating new conversation: {ex.Message}");
            }
            finally
            {
                // Reset flag
                isCreatingNewConversation = false;
            }
        }

        private async void SendMessage_Click(object sender, RoutedEventArgs e)
        {
            Debug.WriteLine("SendMessage_Click start");
            try
            {
                await SendMessageAsync();
                Debug.WriteLine("SendMessage_Click complete");
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"ERROR in SendMessage_Click: {ex}");
                ShowError($"Error sending message: {ex.Message}");
            }
        }

        private async void MessageTextBox_KeyDown(object sender, KeyRoutedEventArgs e)
        {
            if (e.Key == VirtualKey.Enter && !e.KeyStatus.IsMenuKeyDown && !e.KeyStatus.WasKeyDown)
            {
                Debug.WriteLine("MessageTextBox_KeyDown Enter key pressed");
                try
                {
                    await SendMessageAsync();
                    e.Handled = true;
                    Debug.WriteLine("MessageTextBox_KeyDown complete");
                }
                catch (Exception ex)
                {
                    Debug.WriteLine($"ERROR in MessageTextBox_KeyDown: {ex}");
                    ShowError($"Error sending message: {ex.Message}");
                }
            }
        }

        private async Task SendMessageAsync()
        {
            Debug.WriteLine("[DEBUG] SendMessageAsync: START");
            try
            {
                if (currentConversationId == null)
                {
                    Debug.WriteLine("[ERROR] SendMessageAsync: No current conversation ID");
                    ShowError("No active conversation. Please start a new conversation.");
                    return;
                }

                // Getting user message from the input box
                if (MessageTextBox == null)
                {
                    Debug.WriteLine("[ERROR] SendMessageAsync: MessageTextBox is null");
                    return;
                }

                string userMessage = MessageTextBox.Text?.Trim();
                if (string.IsNullOrEmpty(userMessage))
                {
                    Debug.WriteLine("[DEBUG] SendMessageAsync: Empty message, not sending");
                    return;
                }

                // Store the message text and clear the input before any async operations
                string messageToSend = userMessage;
                Debug.WriteLine($"[DEBUG] SendMessageAsync: Sending message: '{(messageToSend.Length > 20 ? messageToSend.Substring(0, 20) + "..." : messageToSend)}'");

                // Clear the input box immediately for better UX
                try
                {
                    MessageTextBox.Text = string.Empty;
                }
                catch (Exception ex)
                {
                    Debug.WriteLine($"[ERROR] SendMessageAsync: Error clearing input: {ex.Message}");
                }

                // Disable input controls
                Debug.WriteLine("[DEBUG] SendMessageAsync: Disabling input controls");
                SetInputEnabled(false);

                try
                {
                    // Add user message to UI
                    await AddMessageToUIAsync(messageToSend, false);
                    await ScrollToBottomAsync();

                    // Save user message to database
                    Debug.WriteLine($"[DEBUG] SendMessageAsync: Saving user message to database for conversation ID: {currentConversationId.Value}");
                    try
                    {
                        var savedMessage = await messageService.CreateMessageAsync(
                            currentConversationId.Value,
                            DefaultUserId,
                            messageToSend);
                        Debug.WriteLine($"[DEBUG] SendMessageAsync: Message saved with ID: {savedMessage.Id}");
                    }
                    catch (Exception msgEx)
                    {
                        Debug.WriteLine($"[ERROR] SendMessageAsync: Error saving user message: {msgEx}");
                        // Continue to get bot response anyway
                    }

                    // Add typing indicator
                    var typingIndicatorId = await AddTypingIndicatorAsync();
                    await ScrollToBottomAsync();

                    // Get bot response with timeout
                    string botResponse = "Sorry, I couldn't get a response at this time.";
                    bool gotBotResponse = false;

                    try
                    {
                        Debug.WriteLine("[DEBUG] SendMessageAsync: Getting bot response with timeout protection");
                        using var timeoutCts = new CancellationTokenSource(TimeSpan.FromSeconds(10));

                        try
                        {
                            botResponse = await chatbotService.GetBotResponseAsync(messageToSend, isWelcomeMessage: false);
                            gotBotResponse = true;
                            Debug.WriteLine($"[DEBUG] SendMessageAsync: Bot response received: '{(botResponse.Length > 20 ? botResponse.Substring(0, 20) + "..." : botResponse)}'");
                        }
                        catch (OperationCanceledException)
                        {
                            Debug.WriteLine("[ERROR] SendMessageAsync: Bot response request timed out");
                            botResponse = "Sorry, the response took too long. Please try again.";
                        }
                        catch (Exception botEx)
                        {
                            Debug.WriteLine($"[ERROR] SendMessageAsync: Error getting bot response: {botEx}");
                            botResponse = "Sorry, I encountered an error processing your request.";
                        }
                    }
                    catch (Exception outerEx)
                    {
                        Debug.WriteLine($"[ERROR] SendMessageAsync: Critical error in bot response handling: {outerEx}");
                    }

                    // Remove typing indicator
                    Debug.WriteLine("[DEBUG] SendMessageAsync: Removing typing indicator");
                    await RemoveTypingIndicatorAsync(typingIndicatorId);

                    // Add bot response to UI
                    await AddMessageToUIAsync(botResponse, true);
                    await ScrollToBottomAsync();

                    // Save bot response to database - only if we got a real response
                    if (gotBotResponse)
                    {
                        Debug.WriteLine("[DEBUG] SendMessageAsync: Saving bot response to database");
                        try
                        {
                            var savedBotMessage = await messageService.CreateMessageAsync(
                                currentConversationId.Value,
                                DefaultUserId,
                                botResponse);
                            Debug.WriteLine($"[DEBUG] SendMessageAsync: Bot message saved with ID: {savedBotMessage.Id}");
                        }
                        catch (Exception msgEx)
                        {
                            Debug.WriteLine($"[ERROR] SendMessageAsync: Error saving bot message: {msgEx}");
                            // Continue as UI already has the message
                        }
                    }

                    Debug.WriteLine("[DEBUG] SendMessageAsync: COMPLETE");
                }
                catch (Exception ex)
                {
                    Debug.WriteLine($"[ERROR] SendMessageAsync: CRITICAL ERROR: {ex}");
                    ShowError($"Error sending message: {ex.Message}");
                }
                finally
                {
                    // Re-enable input controls
                    Debug.WriteLine("[DEBUG] SendMessageAsync: Re-enabling input controls");
                    SetInputEnabled(true);
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[ERROR] SendMessageAsync: Unhandled exception: {ex}");
                ShowError($"Unexpected error: {ex.Message}");
            }
        }

        // Helper to add a message to the UI directly as a UI element
        private async Task AddMessageToUIAsync(string message, bool isFromBot)
        {
            Debug.WriteLine($"[DEBUG] AddMessageToUIAsync: Adding message '{(message.Length > 20 ? message.Substring(0, 20) + "..." : message)}' (isFromBot: {isFromBot})");

            try
            {
                await RunOnUIThreadAsync(() =>
                {
                    // Create Border container with consistent styling
                    Border messageBorder = new Border
                    {
                        Background = new SolidColorBrush(Microsoft.UI.Colors.LightGray),
                        CornerRadius = new CornerRadius(8),
                        Padding = new Thickness(15, 10, 15, 10),
                        HorizontalAlignment = isFromBot ? HorizontalAlignment.Left : HorizontalAlignment.Right,
                        MaxWidth = 500,
                        Margin = new Thickness(0, 5, 0, 5)
                    };

                    // Create TextBlock for message content
                    TextBlock messageText = new TextBlock
                    {
                        Text = message,
                        TextWrapping = TextWrapping.Wrap,
                        Foreground = new SolidColorBrush(Microsoft.UI.Colors.Black)
                    };

                    // Add TextBlock to Border
                    messageBorder.Child = messageText;

                    // Add Border to messages panel
                    ChatMessagesPanel.Children.Add(messageBorder);

                    Debug.WriteLine("[DEBUG] AddMessageToUIAsync: Message added to UI");
                });
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[ERROR] AddMessageToUIAsync: Error adding message to UI: {ex}");
            }
        }

        // Helper to add typing indicator
        private async Task<int> AddTypingIndicatorAsync()
        {
            Debug.WriteLine("[DEBUG] AddTypingIndicatorAsync: Adding typing indicator");

            try
            {
                int indicatorId = -1;

                await RunOnUIThreadAsync(() =>
                {
                    // Get theme colors
                    var darkTextBrush = new SolidColorBrush(Microsoft.UI.Colors.Black);
                    // Create Border container
                    Border typingBorder = new Border
                    {
                        Background = new SolidColorBrush(Microsoft.UI.Colors.LightGray),
                        CornerRadius = new CornerRadius(8),
                        Padding = new Thickness(15, 10, 15, 10),
                        HorizontalAlignment = HorizontalAlignment.Left,
                        MaxWidth = 500,
                        Margin = new Thickness(0, 5, 0, 5),
                        Tag = "TypingIndicator"
                    };

                    // Create TextBlock for typing indicator
                    TextBlock typingText = new TextBlock
                    {
                        Text = "Typing...",
                        TextWrapping = TextWrapping.Wrap,
                        Foreground = darkTextBrush
                    };

                    // Add TextBlock to Border
                    typingBorder.Child = typingText;
                    // Add Border to messages panel
                    ChatMessagesPanel.Children.Add(typingBorder);
                    indicatorId = ChatMessagesPanel.Children.Count - 1;

                    Debug.WriteLine($"[DEBUG] AddTypingIndicatorAsync: Typing indicator added with ID: {indicatorId}");
                });

                return indicatorId;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[ERROR] AddTypingIndicatorAsync: Error adding typing indicator: {ex}");
                return -1;
            }
        }

        // Helper to remove typing indicator
        private async Task RemoveTypingIndicatorAsync(int indicatorId)
        {
            Debug.WriteLine($"[DEBUG] RemoveTypingIndicatorAsync: Removing typing indicator with ID: {indicatorId}");

            try
            {
                await RunOnUIThreadAsync(() =>
                {
                    if (indicatorId >= 0 && indicatorId < ChatMessagesPanel.Children.Count)
                    {
                        ChatMessagesPanel.Children.RemoveAt(indicatorId);
                        Debug.WriteLine("[DEBUG] RemoveTypingIndicatorAsync: Typing indicator removed");
                    }
                    else
                    {
                        // Try to find by tag as fallback
                        for (int i = 0; i < ChatMessagesPanel.Children.Count; i++)
                        {
                            var element = ChatMessagesPanel.Children[i];
                            if (element is Border border && border.Tag as string == "TypingIndicator")
                            {
                                ChatMessagesPanel.Children.RemoveAt(i);
                                Debug.WriteLine("[DEBUG] RemoveTypingIndicatorAsync: Typing indicator found and removed by tag");
                                break;
                            }
                        }
                    }
                });
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[ERROR] RemoveTypingIndicatorAsync: Error removing typing indicator: {ex}");
            }
        }

        // Helper to add a conversation to the list safely
        private async Task<bool> AddConversationToListAsync(ConversationViewModel conversation)
        {
            Debug.WriteLine($"[DEBUG] AddConversationToListAsync: Adding conversation ID: {conversation.Id}");

            try
            {
                bool success = false;

                await RunOnUIThreadAsync(() =>
                {
                    try
                    {
                        conversations.Add(conversation);
                        Debug.WriteLine($"[DEBUG] AddConversationToListAsync: Conversation added successfully");
                        success = true;
                    }
                    catch (Exception addEx)
                    {
                        Debug.WriteLine($"[ERROR] AddConversationToListAsync: Error adding conversation: {addEx}");
                        success = false;
                    }
                });

                return success;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[ERROR] AddConversationToListAsync: Error handling conversation add: {ex}");
                return false;
            }
        }

        private void SetInputEnabled(bool enabled)
        {
            try
            {
                Debug.WriteLine($"SetInputEnabled: {enabled}");

                // First make sure the MessageTextBox exists
                if (MessageTextBox == null)
                {
                    Debug.WriteLine("MessageTextBox is null");
                    return;
                }

                MessageTextBox.IsEnabled = enabled;

                if (SendMessageButton != null)
                {
                    SendMessageButton.IsEnabled = enabled;
                }

                if (enabled)
                {
                    try
                    {
                        MessageTextBox.Focus(FocusState.Programmatic);
                        Debug.WriteLine("MessageTextBox focused");
                    }
                    catch (Exception focusEx)
                    {
                        Debug.WriteLine($"Error focusing MessageTextBox: {focusEx.Message}");
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"ERROR in SetInputEnabled: {ex}");
                // Don't throw, as this is called from finally blocks
            }
        }

        private async Task ScrollToBottomAsync()
        {
            Debug.WriteLine("[DEBUG] ScrollToBottomAsync: START");
            try
            {
                // Verify we're on a valid UI context
                if (this.XamlRoot == null)
                {
                    Debug.WriteLine("[DEBUG] ScrollToBottomAsync: Page not fully loaded, skipping scroll");
                    return;
                }

                // Check if the scroll viewer is properly loaded and in the visual tree
                if (MessagesScrollViewer == null)
                {
                    Debug.WriteLine("[DEBUG] ScrollToBottomAsync: MessagesScrollViewer is null");
                    return;
                }

                // Check if we have messages to scroll to
                if (ChatMessagesPanel.Children.Count == 0)
                {
                    Debug.WriteLine("[DEBUG] ScrollToBottomAsync: No messages to scroll to");
                    return;
                }

                // Add a delay to let UI update
                await Task.Delay(50);

                await RunOnUIThreadAsync(() =>
                {
                    try
                    {
                        MessagesScrollViewer.ChangeView(null, double.MaxValue, null, true);
                        Debug.WriteLine("[DEBUG] ScrollToBottomAsync: Scrolled to bottom");
                    }
                    catch (Exception scrollEx)
                    {
                        Debug.WriteLine($"[ERROR] ScrollToBottomAsync: Error scrolling: {scrollEx}");
                    }
                });
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[ERROR] ScrollToBottomAsync: {ex}");
            }
        }

        // Run a function on the UI thread
        private async Task RunOnUIThreadAsync(Microsoft.UI.Dispatching.DispatcherQueueHandler action)
        {
            var dispatcherQueue = Microsoft.UI.Dispatching.DispatcherQueue.GetForCurrentThread();
            if (dispatcherQueue != null)
            {
                var tcs = new TaskCompletionSource<bool>();

                dispatcherQueue.TryEnqueue(() =>
                {
                    try
                    {
                        action();
                        tcs.TrySetResult(true);
                    }
                    catch (Exception ex)
                    {
                        tcs.TrySetException(ex);
                    }
                });

                await Task.WhenAny(tcs.Task, Task.Delay(1000));
            }
            else
            {
                // Fallback - directly run on current thread
                action();
            }
        }

        private void ShowError(string message)
        {
            Debug.WriteLine($"ShowError: {message}");
            try
            {
                ContentDialog dialog = new ContentDialog()
                {
                    Title = "Error",
                    Content = message,
                    CloseButtonText = "OK",
                    XamlRoot = this.XamlRoot
                };

                _ = dialog.ShowAsync();
                Debug.WriteLine("Error dialog shown");
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"ERROR showing error dialog: {ex}");
            }
        }
    }

    public class ConversationViewModel
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public string DisplayText { get; set; }
    }
}