using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using MarketMinds.Shared.Models;
using ViewModelLayer.ViewModel;

namespace MarketMinds.Helpers
{
    public class TagManagementViewModelHelper
    {
        private readonly ProductTagViewModel productTagViewModel;

        public TagManagementViewModelHelper(ProductTagViewModel productTagViewModel)
        {
            this.productTagViewModel = productTagViewModel;
        }

        public bool AddTagToCollection(string tag, ObservableCollection<string> tags)
        {
            tag = tag.Trim();
            if (!string.IsNullOrEmpty(tag) && !tags.Contains(tag))
            {
                tags.Add(tag);
                return true;
            }
            return false;
        }

        public bool RemoveTagFromCollection(string tag, ObservableCollection<string> tags)
        {
            if (tag != null && tags.Contains(tag))
            {
                return tags.Remove(tag);
            }
            return false;
        }

        public ProductTag EnsureTagExists(string tagName)
        {
            var allTags = productTagViewModel.GetAllProductTags();
            var existingTag = allTags.FirstOrDefault(tag => tag.DisplayTitle.Equals(tagName, StringComparison.OrdinalIgnoreCase));

            if (existingTag != null)
            {
                return existingTag;
            }
            else
            {
                return productTagViewModel.CreateProductTag(tagName);
            }
        }

        public List<ProductTag> ConvertStringTagsToProductTags(IEnumerable<string> stringTags)
        {
            return stringTags.Select(tagName => EnsureTagExists(tagName)).ToList();
        }
    }
}