using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
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
