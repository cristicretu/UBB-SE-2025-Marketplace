using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using MarketMinds.Shared.Models;
using MarketMinds.Shared.Services.ProductCategoryService;

namespace ViewModelLayer.ViewModel
{
    public class ProductCategoryViewModel : INotifyPropertyChanged
    {
        private IProductCategoryService productCategoryService;
        private string categoryName;
        private string categoryDescription;
        private string errorMessage;
        private string successMessage;
        private bool isDialogOpen;

        public event PropertyChangedEventHandler PropertyChanged;

        public string CategoryName
        {
            get => categoryName;
            set
            {
                categoryName = value;
                OnPropertyChanged();
            }
        }

        public string CategoryDescription
        {
            get => categoryDescription;
            set
            {
                categoryDescription = value;
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

        public ProductCategoryViewModel(ProductCategoryService productCategoryService)
        {
            this.productCategoryService = productCategoryService;
        }

        public List<Category> GetAllProductCategories()
        {
            return productCategoryService.GetAllProductCategories();
        }

        public Category CreateProductCategory(string displayTitle, string description)
        {
            return productCategoryService.CreateProductCategory(displayTitle, description);
        }

        public void DeleteProductCategory(string displayTitle)
        {
            productCategoryService.DeleteProductCategory(displayTitle);
        }

        public bool ValidateAndCreateCategory()
        {
            if (string.IsNullOrWhiteSpace(CategoryName))
            {
                ErrorMessage = "Category name cannot be empty.";
                return false;
            }

            try
            {
                var newCategory = CreateProductCategory(CategoryName, CategoryDescription);
                SuccessMessage = $"Category '{CategoryName}' created successfully.";
                // Clear input fields
                CategoryName = string.Empty;
                CategoryDescription = string.Empty;
                return true;
            }
            catch (Exception categoryCreationException)
            {
                ErrorMessage = $"Error creating category: {categoryCreationException.Message}";
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
