using MarketMinds.Shared.Models;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;

namespace MarketMinds.Helpers.Selectors;

public class ChatMessageTemplateSelector : DataTemplateSelector
{
    public DataTemplate MyMessageTemplate { get; set; }
    public DataTemplate OtherMessageTemplate { get; set; }

    public int MyUserId { get; set; } = -1; // Default to invalid value

    protected override DataTemplate SelectTemplateCore(object item, DependencyObject container)
    {
        if (item is Message message && MyUserId != -1)
        {
            bool isMine = message.UserId == MyUserId;
            return isMine ? MyMessageTemplate : OtherMessageTemplate;
        }

        return base.SelectTemplateCore(item, container);
    }

    protected override DataTemplate SelectTemplateCore(object item)
    {
        return base.SelectTemplateCore(item);
    }
}
