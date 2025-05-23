using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Navigation;

namespace MarketMinds.Views
{
    public sealed partial class MarketMindsPage : Page
    {
        public MarketMindsPage()
        {
            this.InitializeComponent();

            // Set Buy Products as the default selection
            ProductsPivot.SelectedIndex = 0;
        }

        /// <summary>
        /// Handle pivot selection changes
        /// </summary>
        private void ProductsPivot_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            // Update button styles based on pivot selection
            switch (ProductsPivot.SelectedIndex)
            {
                case 0:
                    break;
                case 1:
                    break;
                case 2:
                    break;
            }
        }

        /// <summary>
        /// Handles the Clear Filters button click
        /// </summary>
        private void ClearFilters_Click(object sender, RoutedEventArgs e)
        {
            // Reset filter values here
            PriceRangeSlider.Value = PriceRangeSlider.Maximum;
            
            // Additional filter reset logic would go here
        }
    }
} 