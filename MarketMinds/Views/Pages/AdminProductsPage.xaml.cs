using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using MarketMinds.Shared.Models;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Navigation;
using ViewModelLayer.ViewModel;
using Windows.Foundation;
using Windows.Foundation.Collections;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.
namespace MarketMinds.Views.Pages
{
    public sealed partial class AdminProductsPage : UserControl
    {
        private readonly ProductCategoryViewModel productCategoryViewModel;
        private readonly ProductConditionViewModel productConditionViewModel;
        public ObservableCollection<Category> ProductCategories { get; private set; }
        public ObservableCollection<Condition> ProductConditions { get; private set; }

        public AdminProductsPage()
        {
            this.InitializeComponent();

            productCategoryViewModel = MarketMinds.App.ProductCategoryViewModel;
            productConditionViewModel = MarketMinds.App.ProductConditionViewModel;

            ProductCategories = new ObservableCollection<Category>();
            ProductConditions = new ObservableCollection<Condition>();

            // Set up data binding for view models
            CategoryNameTextBox.SetBinding(TextBox.TextProperty,
                new Microsoft.UI.Xaml.Data.Binding() { Path = new PropertyPath("CategoryName"), Mode = Microsoft.UI.Xaml.Data.BindingMode.TwoWay, Source = productCategoryViewModel });
            CategoryDescriptionTextBox.SetBinding(TextBox.TextProperty,
                new Microsoft.UI.Xaml.Data.Binding() { Path = new PropertyPath("CategoryDescription"), Mode = Microsoft.UI.Xaml.Data.BindingMode.TwoWay, Source = productCategoryViewModel });
            ConditionNameTextBox.SetBinding(TextBox.TextProperty,
                new Microsoft.UI.Xaml.Data.Binding() { Path = new PropertyPath("ConditionName"), Mode = Microsoft.UI.Xaml.Data.BindingMode.TwoWay, Source = productConditionViewModel });
            ConditionDescriptionTextBox.SetBinding(TextBox.TextProperty,
                new Microsoft.UI.Xaml.Data.Binding() { Path = new PropertyPath("ConditionDescription"), Mode = Microsoft.UI.Xaml.Data.BindingMode.TwoWay, Source = productConditionViewModel });

            // Register for property changed events to show dialogs
            productCategoryViewModel.PropertyChanged += ViewModel_PropertyChanged;
            productConditionViewModel.PropertyChanged += ViewModel_PropertyChanged;

            // Load existing data
            LoadCategories();
            LoadConditions();
        }

        private async void ViewModel_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs propertyChangedEventArgs)
        {
            if (propertyChangedEventArgs.PropertyName == "IsDialogOpen" && sender is INotifyPropertyChanged viewModel)
            {
                if (viewModel is ProductCategoryViewModel categoryViewModel && categoryViewModel.IsDialogOpen)
                {
                    string title = !string.IsNullOrEmpty(categoryViewModel.ErrorMessage) ? "Error" : "Success";
                    string message = !string.IsNullOrEmpty(categoryViewModel.ErrorMessage)
                        ? categoryViewModel.ErrorMessage
                        : categoryViewModel.SuccessMessage;
                    await ShowContentDialog(title, message);
                    categoryViewModel.ClearDialogMessages();
                }
                else if (viewModel is ProductConditionViewModel conditionViewModel && conditionViewModel.IsDialogOpen)
                {
                    string title = !string.IsNullOrEmpty(conditionViewModel.ErrorMessage) ? "Error" : "Success";
                    string message = !string.IsNullOrEmpty(conditionViewModel.ErrorMessage)
                        ? conditionViewModel.ErrorMessage
                        : conditionViewModel.SuccessMessage;
                    await ShowContentDialog(title, message);
                    conditionViewModel.ClearDialogMessages();
                }
            }
        }

        private void HandleAddCategoryButton_Click(object sender, RoutedEventArgs routedEventArgs)
        {
            if (productCategoryViewModel.ValidateAndCreateCategory())
            {
                LoadCategories(); // Refresh the list
            }
        }

        private void HandleAddConditionButton_Click(object sender, RoutedEventArgs routedEventArgs)
        {
            if (productConditionViewModel.ValidateAndCreateCondition())
            {
                LoadConditions(); // Refresh the list
            }
        }

        // Load existing categories from service
        private void LoadCategories()
        {
            var categories = productCategoryViewModel.GetAllProductCategories();
            ProductCategories.Clear();
            foreach (var category in categories)
            {
                ProductCategories.Add(category);
            }
        }

        // Load existing conditions from service
        private void LoadConditions()
        {
            var conditions = productConditionViewModel.GetAllProductConditions();
            ProductConditions.Clear();
            foreach (var condition in conditions)
            {
                ProductConditions.Add(condition);
            }
        }

        // Helper method to display a ContentDialog
        private async System.Threading.Tasks.Task ShowContentDialog(string title, string content)
        {
            ContentDialog dialog = new ContentDialog
            {
                Title = title,
                Content = content,
                CloseButtonText = "OK",
                XamlRoot = this.Content.XamlRoot
            };
            await dialog.ShowAsync();
        }
    }
}
