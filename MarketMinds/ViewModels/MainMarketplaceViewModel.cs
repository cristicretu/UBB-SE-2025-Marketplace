using System.Collections.Generic;
using MarketMinds.Shared.Models;

namespace MarketMinds.ViewModels;

public class MainMarketplaceViewModel
{
    public MainMarketplaceViewModel()
    {
    }

    public List<UserNotSoldOrder> GetAvailableItems()
    {
        return new List<UserNotSoldOrder>();
    }
}
