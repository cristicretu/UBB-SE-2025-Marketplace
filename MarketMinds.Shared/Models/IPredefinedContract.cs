﻿namespace MarketMinds.Shared.Models
{
    public interface IPredefinedContract
    {
        int ContractID { get; set; }
        string ContractContent { get; set; }
        int ID { get; set; }
    }
}