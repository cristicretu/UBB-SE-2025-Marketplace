using System.Linq;
using System.Collections.Generic;
using MarketMinds.Shared.Models;
using MarketMinds.Shared.Models.DTOs;

namespace MarketMinds.Shared.Models.DTOs.Mappers
{
    public static class BuyProductMapper
    {
        private static int undefined_user_type = 0;
        private static int undefined_balance = 0;
        private static int undefined_rating = 0;
        private static int undefined_password = 0;
        public static BuyProductDTO ToDTO(BuyProduct entity)
        {
            if (entity == null)
            {
                return null;
            }

            return new BuyProductDTO
            {
                Id = entity.Id,
                Title = entity.Title,
                Description = entity.Description,
                Price = entity.Price,
                Condition = entity.Condition != null ? new ConditionDTO
                {
                    Id = entity.Condition.Id,
                    DisplayTitle = entity.Condition.Name,
                    Description = null
                }
                : null,
                Category = entity.Category != null ? new CategoryDTO
                {
                    Id = entity.Category.Id,
                    DisplayTitle = entity.Category.Name,
                    Description = entity.Category.Description
                }
                : null,
                Tags = entity.ProductTags?.Select(pt => new ProductTagDTO
                {
                    Id = pt.Tag.Id,
                    DisplayTitle = pt.Tag.Title
                }).ToList() ?? new List<ProductTagDTO>(),
                Images = entity.Images?.Select(i => new ImageDTO
                {
                    Url = i.Url
                }).ToList() ??
                entity.NonMappedImages?.Select(i => new ImageDTO
                {
                    Url = i.Url
                }).ToList() ??
                new List<ImageDTO>(),
                Seller = entity.Seller != null ? new UserDTO
                {
                    Id = entity.Seller.Id,
                    Username = entity.Seller.Username,
                    Email = entity.Seller.Email,
                    UserType = undefined_user_type,
                    Balance = undefined_balance,
                    Rating = undefined_rating,
                    Password = undefined_password
                }
                : null
            };
        }

        public static List<BuyProductDTO> ToDTOList(IEnumerable<BuyProduct> entities)
        {
            return entities?.Select(e => ToDTO(e)).ToList() ?? new List<BuyProductDTO>();
        }
    }
}