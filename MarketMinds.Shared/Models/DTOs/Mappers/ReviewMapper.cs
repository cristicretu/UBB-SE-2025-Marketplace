using System.Linq;
using MarketMinds.Shared.Models;
using MarketMinds.Shared.Models.DTOs;

namespace MarketMinds.Shared.Models.DTOs.Mappers
{
    public static class ReviewMapper
    {
        public static ReviewDTO ToDto(Review review)
        {
            if (review == null)
            {
                return null;
            }

            return new ReviewDTO
            {
                Id = review.Id,
                Description = review.Description,
                Rating = review.Rating,
                SellerId = review.SellerId,
                BuyerId = review.BuyerId,
                Images = review.Images.Select(img => new ImageDTO
                {
                    Url = img.Url
                }).ToList()
            };
        }

        public static Review ToModel(ReviewDTO dto)
        {
            if (dto == null)
            {
                return null;
            }

            var review = new Review
            {
                Id = dto.Id,
                Description = dto.Description,
                Rating = dto.Rating,
                SellerId = dto.SellerId,
                BuyerId = dto.BuyerId,
                Images = dto.Images.Select(img => new Image(img.Url)).ToList()
            };

            review.SyncImagesBeforeSave();
            return review;
        }
    }
}