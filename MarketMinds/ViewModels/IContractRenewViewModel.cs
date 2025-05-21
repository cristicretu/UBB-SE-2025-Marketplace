// <copyright file="ContractRenewViewModel.cs" company="MarketMinds">
// Copyright (c) MarketMinds. All rights reserved.
// </copyright>

namespace MarketMinds.ViewModels
{
    using System;
    using System.Collections.ObjectModel;
    using System.ComponentModel;
    using System.Threading.Tasks;
    using System.Windows.Input;
    using MarketMinds.Shared.Models;

    public interface IContractRenewViewModel
    {
        int BuyerId { get; set; }
        ICommand CloseMessageCommand { get; }
        ObservableCollection<IContract> Contracts { get; }
        DateTime? EndDate { get; set; }
        bool IsContractSelected { get; set; }
        bool IsLoading { get; set; }
        bool IsRenewalAllowed { get; set; }
        bool IsSuccessMessage { get; set; }
        string Message { get; set; }
        DateTime MinNewEndDate { get; }
        DateTime NewEndDate { get; set; }
        DateTime NewStartDate { get; set; }
        decimal Price { get; set; }
        string ProductName { get; set; }
        ICommand RefreshContractsCommand { get; }
        IContract SelectedContract { get; set; }
        int SellerId { get; set; }
        bool ShowMessage { get; set; }
        DateTime? StartDate { get; set; }
        string StatusColor { get; set; }
        string StatusText { get; set; }
        ICommand SubmitRenewalCommand { get; }

        event PropertyChangedEventHandler PropertyChanged;

        bool CanSubmitRenewal();
        Task RefreshContractsAsync();
        Task SubmitRenewalRequestAsync();
    }
}