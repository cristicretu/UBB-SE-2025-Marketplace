using System.Text.Json;
using System.Text.Json.Serialization;
using MarketMinds.Shared.Models;

namespace MarketMinds.Shared.Services
{
    public class UserJsonConverter : JsonConverter<User>
    {
        public override User Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            if (reader.TokenType != JsonTokenType.StartObject)
            {
                throw new JsonException("Expected start of object");
            }

            var user = new User();
            while (reader.Read())
            {
                if (reader.TokenType == JsonTokenType.EndObject)
                {
                    return user;
                }

                if (reader.TokenType != JsonTokenType.PropertyName)
                {
                    throw new JsonException("Expected property name");
                }

                string propertyName = reader.GetString();
                reader.Read();

                switch (propertyName.ToLower())
                {
                    case "id":
                        user.Id = reader.GetInt32();
                        break;
                    case "username":
                        user.Username = reader.GetString();
                        break;
                    case "email":
                        user.Email = reader.GetString();
                        break;
                    case "usertype":
                        user.UserType = reader.GetInt32();
                        break;
                    case "balance":
                        user.Balance = reader.GetSingle();
                        break;
                    case "rating":
                        if (user.GetType().GetProperty("Rating") != null)
                        {
                            user.Rating = reader.GetSingle();
                        }
                        else
                        {
                            reader.Skip();
                        }
                        break;
                    default:
                        reader.Skip();
                        break;
                }
            }

            throw new JsonException("Expected end of object");
        }

        public override void Write(Utf8JsonWriter writer, User value, JsonSerializerOptions options)
        {
            throw new NotImplementedException();
        }
    }
}