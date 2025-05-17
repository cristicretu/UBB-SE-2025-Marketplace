using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;

namespace MarketMinds.Views.Pages2
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class GetHelpPage : Page
    {
        public GetHelpPage()
        {
            this.InitializeComponent();
        }
        private void OnButtonClickOpenChatbotConversation(object sender, RoutedEventArgs routedEventArgs)
        {
            var chatbotWindow = new Window();
            chatbotWindow.Title = "Chatbot Conversations";

            var helpPage = new HelpPage(
                App.ConversationService,
                App.MessageService,
                App.NewChatbotService);

            chatbotWindow.Content = helpPage;
            chatbotWindow.Activate();
        }
        private void OnButtonClickOpenCSConversation(object sender, RoutedEventArgs routedEventArgs)
        {
        }
        private void OnButtonClickNavigateGetHelpPageMainMarketplacePage(object sender, RoutedEventArgs routedEventArgs)
        {
        }
    }
}
