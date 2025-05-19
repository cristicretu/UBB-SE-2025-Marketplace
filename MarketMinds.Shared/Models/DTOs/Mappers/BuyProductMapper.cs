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
                    DisplayTitle = entity.Condition.Name ?? "Unknown",
                    Description = entity.Condition.Description
                }
                : null,
                Category = entity.Category != null ? new CategoryDTO
                {
                    Id = entity.Category.Id,
                    DisplayTitle = entity.Category.Name ?? "Uncategorized",
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
                Seller = GetSellerDTO(entity)
            };
        }

        private static UserDTO GetSellerDTO(BuyProduct entity)
        {
            if (entity.Seller == null) return null;

            try
            {
                // Handle User type directly since entity.Seller is User type
                var user = entity.Seller;
                return new UserDTO
                {
                    Id = user.Id,
                    Username = user.Username ?? "Unknown seller",
                    Email = user.Email,
                    UserType = undefined_user_type,
                    Balance = undefined_balance,
                    Rating = undefined_rating,
                    Password = undefined_password
                };
            }
            catch
            {
                // Fall back to a default user DTO if anything goes wrong
            }

            return new UserDTO
            {
                Id = entity.SellerId,
                Username = "Unknown seller",
                Email = "",
                UserType = undefined_user_type,
                Balance = undefined_balance,
                Rating = undefined_rating,
                Password = undefined_password
            };
        }

        public static List<BuyProductDTO> ToDTOList(IEnumerable<BuyProduct> entities)
        {
            return entities?.Select(e => ToDTO(e)).ToList() ?? new List<BuyProductDTO>();
        }

        public static BuyProduct FromDTO(BuyProductDTO dto)
        {
            if (dto == null)
            {
                return null;
            }

            var product = new BuyProduct
            {
                Id = dto.Id,
                Title = dto.Title,
                Description = dto.Description,
                Price = dto.Price
            };

            // Map seller
            if (dto.Seller != null)
            {
                product.SellerId = dto.Seller.Id;
                product.Seller = new User
                {
                    Id = dto.Seller.Id,
                    Username = dto.Seller.Username,
                    Email = dto.Seller.Email
                };
            }

            // Map condition
            if (dto.Condition != null)
            {
                product.ConditionId = dto.Condition.Id;
                product.Condition = new Condition
                {
                    Id = dto.Condition.Id,
                    Name = dto.Condition.DisplayTitle, // Map displayTitle to Name
                    Description = dto.Condition.Description
                };
            }

            // Map category
            if (dto.Category != null)
            {
                product.CategoryId = dto.Category.Id;
                product.Category = new Category
                {
                    Id = dto.Category.Id,
                    Name = dto.Category.DisplayTitle, // Map displayTitle to Name
                    Description = dto.Category.Description
                };
            }

            // Map images
            if (dto.Images != null && dto.Images.Any())
            {
                product.Images = dto.Images.Select(img => new BuyProductImage
                {
                    Url = img.Url,
                    ProductId = product.Id,
                    Product = product
                }).ToList();
            }
            else
            {
                product.Images = new List<BuyProductImage>();
            }

            // Map tags
            if (dto.Tags != null && dto.Tags.Any())
            {
                product.Tags = dto.Tags.Select(t => new ProductTag
                {
                    Id = t.Id,
                    Title = t.DisplayTitle
                }).ToList();

                product.ProductTags = dto.Tags.Select(t => new BuyProductProductTag
                {
                    TagId = t.Id,
                    ProductId = product.Id,
                    Tag = new ProductTag { Id = t.Id, Title = t.DisplayTitle }
                }).ToList();
            }
            else
            {
                product.Tags = new List<ProductTag>();
                product.ProductTags = new List<BuyProductProductTag>();
            }

            return product;
        }

        public static List<BuyProduct> FromDTOList(IEnumerable<BuyProductDTO> dtos)
        {
            return dtos?.Select(d => FromDTO(d)).ToList() ?? new List<BuyProduct>();
        }
    }
}