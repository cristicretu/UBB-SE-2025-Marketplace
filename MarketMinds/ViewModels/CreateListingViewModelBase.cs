using System.Collections.Generic;
using MarketMinds.Shared.Models;
using MarketMinds.Shared.Services.ImagineUploadService;
using MarketMinds.Shared.Helper;

namespace MarketMinds.ViewModels
{
    public abstract class CreateListingViewModelBase
    {
        public string Title { get; set; }
        public Category Category { get; set; }
        public List<ProductTag> Tags { get; set; }
        public string Description { get; set; }
        public List<Image> Images { get; set; }
        public Condition Condition { get; set; }
        private readonly ImageUploadService imageService;

        public CreateListingViewModelBase()
        {
            imageService = new ImageUploadService(AppConfig.Configuration);
            Images = new List<Image>();
        }

        public string ImagesString
        {
            get => imageService.FormatImagesString(Images);
            set => Images = imageService.ParseImagesString(value);
        }
            public abstract void CreateListing(Product product);
    }
}

