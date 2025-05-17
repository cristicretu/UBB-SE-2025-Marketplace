using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using MarketMinds.Shared.Models;
using MarketMinds.Shared.Services.ProductConditionService;

namespace ViewModelLayer.ViewModel
{
    public class ProductConditionViewModel : INotifyPropertyChanged
    {
        private IProductConditionService productConditionService;
        private string conditionName;
        private string conditionDescription;
        private string errorMessage;
        private string successMessage;
        private bool isDialogOpen;

        public event PropertyChangedEventHandler PropertyChanged;

        public string ConditionName
        {
            get => conditionName;
            set
            {
                conditionName = value;
                OnPropertyChanged();
            }
        }

        public string ConditionDescription
        {
            get => conditionDescription;
            set
            {
                conditionDescription = value;
                OnPropertyChanged();
            }
        }

        public string ErrorMessage
        {
            get => errorMessage;
            set
            {
                errorMessage = value;
                OnPropertyChanged();
                IsDialogOpen = !string.IsNullOrEmpty(value);
            }
        }

        public string SuccessMessage
        {
            get => successMessage;
            set
            {
                successMessage = value;
                OnPropertyChanged();
                IsDialogOpen = !string.IsNullOrEmpty(value);
            }
        }

        public bool IsDialogOpen
        {
            get => isDialogOpen;
            set
            {
                isDialogOpen = value;
                OnPropertyChanged();
            }
        }

        public ProductConditionViewModel(ProductConditionService productConditionService)
        {
            this.productConditionService = productConditionService;
        }
        public List<Condition> GetAllProductConditions()
        {
            return productConditionService.GetAllProductConditions();
        }

        public Condition CreateProductCondition(string displayTitle, string description)
        {
            return productConditionService.CreateProductCondition(displayTitle, description);
        }

        public void DeleteProductCondition(string displayTitle)
        {
            productConditionService.DeleteProductCondition(displayTitle);
        }

        public bool ValidateAndCreateCondition()
        {
            if (string.IsNullOrWhiteSpace(ConditionName))
            {
                ErrorMessage = "Condition name cannot be empty.";
                return false;
            }

            try
            {
                var newCondition = CreateProductCondition(ConditionName, ConditionDescription);
                SuccessMessage = $"Condition '{ConditionName}' created successfully.";
                // Clear input fields
                ConditionName = string.Empty;
                ConditionDescription = string.Empty;

                return true;
            }
            catch (Exception conditionCreationException)
            {
                ErrorMessage = $"Error creating condition: {conditionCreationException.Message}";
                return false;
            }
        }

        public void ClearDialogMessages()
        {
            ErrorMessage = string.Empty;
            SuccessMessage = string.Empty;
            IsDialogOpen = false;
        }

        protected void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
