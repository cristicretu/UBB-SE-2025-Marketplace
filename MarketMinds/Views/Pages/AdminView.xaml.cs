using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using ViewModelLayer.ViewModel;
using MarketMinds.Shared.Models;

namespace MarketMinds.Views
{
    /// <summary>
    /// Interaction logic for AdminView.xaml
    /// </summary>
    public sealed partial class AdminView : Window
    {
        private readonly ProductCategoryViewModel productCategoryViewModel;
        private readonly ProductConditionViewModel productConditionViewModel;
        public ObservableCollection<Category> ProductCategories { get; private set; }
        public ObservableCollection<Condition> ProductConditions { get; private set; }

        public AdminView()
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
