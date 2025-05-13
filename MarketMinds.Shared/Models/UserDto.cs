using System;
using System.Text.Json.Serialization;

namespace MarketMinds.Shared.Models
{
    public class UserDto
    {
        [JsonPropertyName("id")]
        public int Id { get; set; }
        [JsonPropertyName("username")]
        public string Username { get; set; } = string.Empty;
        [JsonPropertyName("email")]
        public string Email { get; set; } = string.Empty;
        [JsonPropertyName("passwordHash")]
        public string PasswordHash { get; set; } = string.Empty;
        [JsonPropertyName("userType")]
        public int UserType { get; set; }
        [JsonPropertyName("balance")]
        public double Balance { get; set; }
        [JsonPropertyName("rating")]
        public double Rating { get; set; }

        public UserDto()
        {
        }

        public UserDto(int id, string username, string email)
        {
            Id = id;
            Username = username;
            Email = email;
            UserType = 0;
            Balance = 0;
            Rating = 0;
        }

        public UserDto(MarketMinds.Shared.Models.User user)
        {
            Id = user.Id;
            Username = user.Username;
            Email = user.Email;
            PasswordHash = user.PasswordHash ?? string.Empty;
            UserType = user.UserType;
            Balance = user.Balance;
            Rating = user.Rating;
        }

        public MarketMinds.Shared.Models.User ToDomainUser()
        {
            var user = new MarketMinds.Shared.Models.User(Username, Email, PasswordHash)
            {
                Id = Id,
                UserType = UserType,
                Balance = Balance,
                Rating = Rating
            };
            return user;
        }
    }
}