namespace MarketMinds.Shared.Models.DTOs.Mappers
{
    public static class AuctionProductMapper
    {
        private static int undefined_user_type = 0;
        private static int undefined_balance = 0;
        private static int undefined_rating = 0;
        private static int undefined_password = 0;
        public static AuctionProductDTO ToDTO(AuctionProduct entity)
        {
            if (entity == null)
            {
                return null;
            }

            // Debug statement for mapping
            Console.WriteLine("========== DEBUG: AuctionProductMapper.ToDTO ==========");
            Console.WriteLine($"Product ID: {entity.Id}, Title: {entity.Title}");
            Console.WriteLine($"ConditionId: {entity.ConditionId}, Condition object: {(entity.Condition != null ? "Loaded" : "NULL")}");
            if (entity.Condition != null)
            {
                Console.WriteLine($"Condition Id: {entity.Condition.Id}, Name: {entity.Condition.Name}, Description: {entity.Condition.Description}");
            }

            Console.WriteLine($"Category ID: {entity.CategoryId}, Category object: {(entity.Category != null ? "Loaded" : "NULL")}");
            if (entity.Category != null)
            {
                Console.WriteLine($"Category Name: {entity.Category.Name}, Description: {entity.Category.Description}");
            }

            var dto = new AuctionProductDTO
            {
                Id = entity.Id,
                Title = entity.Title,
                Description = entity.Description,
                StartAuctionDate = entity.StartTime,
                EndAuctionDate = entity.EndTime,
                StartingPrice = entity.StartPrice,
                CurrentPrice = entity.CurrentPrice,
                BidHistory = entity.Bids?.Select(b => new BidDTO
                {
                    Id = b.Id,
                    Price = b.Price,
                    Timestamp = b.Timestamp,
                    Bidder = b.Bidder != null ? new UserDTO
                    {
                        Id = b.Bidder.Id,
                        Username = b.Bidder.Username,
                        Email = b.Bidder.Email,
                        UserType = undefined_user_type,
                        Balance = undefined_balance,
                        Rating = undefined_rating,
                        Password = undefined_password
                    }
                    : null
                }).ToList() ?? new List<BidDTO>(),
                Condition = entity.Condition != null ? new ConditionDTO
                {
                    Id = entity.Condition.Id,
                    DisplayTitle = entity.Condition.Name,
                    Description = entity.Condition.Description
                }
                : null,
                Category = entity.Category != null ? new CategoryDTO
                {
                    Id = entity.Category.Id,
                    DisplayTitle = entity.Category.Name,
                    Description = entity.Category.Description
                }
                : null,
                Tags = entity.Tags?.Select(t => new ProductTagDTO
                {
                    Id = t.Id,
                    DisplayTitle = t.Title
                }).ToList() ?? new List<ProductTagDTO>(),
                Images = entity.Images?.Select(i => new ImageDTO
                {
                    Url = i.Url
                }).ToList() ?? new List<ImageDTO>(),
                Seller = entity.Seller != null ? new UserDTO
                {
                    Id = entity.Seller.Id,
                    Username = entity.Seller.Username,
                    Email = entity.Seller.Email,
                    UserType = 0,
                    Balance = 0,
                    Rating = 0,
                    Password = 0
                }
                : null
            };

            // Debug statement for DTO
            Console.WriteLine($"DTO Category: {(dto.Category != null ? "Set" : "NULL")}");
            if (dto.Category != null)
            {
                Console.WriteLine($"DTO Category ID: {dto.Category.Id}, DisplayTitle: {dto.Category.DisplayTitle}");
            }
            Console.WriteLine("==================================================");

            return dto;
        }

        public static List<AuctionProductDTO> ToDTOList(IEnumerable<AuctionProduct> entities)
        {
            return entities?.Select(e => ToDTO(e)).ToList() ?? new List<AuctionProductDTO>();
        }
    }
}