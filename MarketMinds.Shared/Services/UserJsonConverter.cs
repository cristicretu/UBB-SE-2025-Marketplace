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

    public class CategoryJsonConverter : JsonConverter<Category>
    {
        public override Category Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            if (reader.TokenType != JsonTokenType.StartObject)
            {
                throw new JsonException("Expected start of object");
            }

            var category = new Category();
            while (reader.Read())
            {
                if (reader.TokenType == JsonTokenType.EndObject)
                {
                    return category;
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
                        category.Id = reader.GetInt32();
                        break;
                    case "displaytitle": // This is the property name from the API (DTO)
                        category.Name = reader.GetString(); // Map to the Name property expected by the frontend
                        break;
                    case "description":
                        category.Description = reader.TokenType == JsonTokenType.Null ? null : reader.GetString();
                        break;
                    default:
                        reader.Skip();
                        break;
                }
            }

            throw new JsonException("Expected end of object");
        }

        public override void Write(Utf8JsonWriter writer, Category value, JsonSerializerOptions options)
        {
            throw new NotImplementedException();
        }
    }
    
    public class ConditionJsonConverter : JsonConverter<Condition>
    {
        public override Condition Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            if (reader.TokenType != JsonTokenType.StartObject)
            {
                throw new JsonException("Expected start of object");
            }

            var condition = new Condition();
            while (reader.Read())
            {
                if (reader.TokenType == JsonTokenType.EndObject)
                {
                    return condition;
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
                        condition.Id = reader.GetInt32();
                        break;
                    case "displaytitle": // This is the property name from the API (DTO)
                        condition.Name = reader.GetString(); // Map to the Name property expected by the frontend
                        break;
                    case "description":
                        condition.Description = reader.TokenType == JsonTokenType.Null ? null : reader.GetString();
                        break;
                    default:
                        reader.Skip();
                        break;
                }
            }

            throw new JsonException("Expected end of object");
        }

        public override void Write(Utf8JsonWriter writer, Condition value, JsonSerializerOptions options)
        {
            throw new NotImplementedException();
        }
    }

    public class SellerJsonConverter : JsonConverter<User>
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
                    case "userType":
                        user.UserType = reader.GetInt32();
                        break;
                    case "balance":
                        user.Balance = reader.GetDouble();
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