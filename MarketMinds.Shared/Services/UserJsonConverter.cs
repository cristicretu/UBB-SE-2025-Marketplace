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

    public class BidJsonConverter : JsonConverter<Bid>
    {
        public override Bid Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            if (reader.TokenType != JsonTokenType.StartObject)
            {
                throw new JsonException("Expected start of object");
            }

            var bid = new Bid();
            while (reader.Read())
            {
                if (reader.TokenType == JsonTokenType.EndObject)
                {
                    return bid;
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
                        bid.Id = reader.GetInt32();
                        break;
                    case "bidderid":
                        bid.BidderId = reader.GetInt32();
                        break;
                    case "productid":
                        bid.ProductId = reader.GetInt32();
                        break;
                    case "price":
                        // Explicitly read as double to prevent type mismatch
                        bid.Price = reader.GetDouble();
                        break;
                    case "timestamp":
                        bid.Timestamp = reader.GetDateTime();
                        break;
                    case "bidder":
                        if (reader.TokenType != JsonTokenType.Null)
                        {
                            // Use the existing UserJsonConverter to parse the Bidder
                            var userConverter = new UserJsonConverter();
                            bid.Bidder = userConverter.Read(ref reader, typeof(User), options);
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

        public override void Write(Utf8JsonWriter writer, Bid value, JsonSerializerOptions options)
        {
            throw new NotImplementedException();
        }
    }

    public class AuctionProductJsonConverter : JsonConverter<AuctionProduct>
    {
        public override AuctionProduct Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            if (reader.TokenType != JsonTokenType.StartObject)
            {
                throw new JsonException("Expected start of object");
            }

            var product = new AuctionProduct();

            // Track if mandatory fields were found
            bool foundId = false;
            bool foundTitle = false;

            while (reader.Read())
            {
                if (reader.TokenType == JsonTokenType.EndObject)
                {
                    if (!foundId || !foundTitle)
                    {
                        throw new JsonException("Required fields missing from AuctionProduct");
                    }
                    return product;
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
                        product.Id = reader.GetInt32();
                        foundId = true;
                        break;
                    case "title":
                        product.Title = reader.GetString();
                        foundTitle = true;
                        break;
                    case "description":
                        product.Description = reader.GetString();
                        break;
                    case "startauctiondate":
                    case "starttime":
                        product.StartTime = reader.GetDateTime();
                        break;
                    case "endauctiondate":
                    case "endtime":
                        product.EndTime = reader.GetDateTime();
                        break;
                    case "startingprice":
                    case "startprice":
                        product.StartPrice = reader.GetDouble();
                        Console.WriteLine($"DEBUG: AuctionProductConverter - Read StartPrice as double: {product.StartPrice}");
                        break;
                    case "currentprice":
                        product.CurrentPrice = reader.GetDouble();
                        Console.WriteLine($"DEBUG: AuctionProductConverter - Read CurrentPrice as double: {product.CurrentPrice}");
                        break;
                    case "bidhistory":
                    case "bids":
                        if (reader.TokenType == JsonTokenType.Null)
                        {
                            product.Bids = new List<Bid>();
                        }
                        else if (reader.TokenType == JsonTokenType.StartArray)
                        {
                            // Process bids array
                            var bids = new List<Bid>();
                            var bidConverter = new BidJsonConverter();

                            // Read array opening
                            while (reader.Read())
                            {
                                if (reader.TokenType == JsonTokenType.EndArray)
                                {
                                    break;
                                }

                                if (reader.TokenType == JsonTokenType.StartObject)
                                {
                                    var bid = bidConverter.Read(ref reader, typeof(Bid), options);
                                    bids.Add(bid);
                                }
                            }
                            product.Bids = bids;
                            Console.WriteLine($"DEBUG: AuctionProductConverter - Read {bids.Count} bids");
                        }
                        else
                        {
                            reader.Skip();
                        }
                        break;
                    case "condition":
                        if (reader.TokenType != JsonTokenType.Null)
                        {
                            var conditionConverter = new ConditionJsonConverter();
                            product.Condition = conditionConverter.Read(ref reader, typeof(Condition), options);
                        }
                        else
                        {
                            reader.Skip();
                        }
                        break;
                    case "category":
                        if (reader.TokenType != JsonTokenType.Null)
                        {
                            var categoryConverter = new CategoryJsonConverter();
                            product.Category = categoryConverter.Read(ref reader, typeof(Category), options);
                        }
                        else
                        {
                            reader.Skip();
                        }
                        break;
                    case "seller":
                        if (reader.TokenType != JsonTokenType.Null)
                        {
                            var sellerConverter = new SellerJsonConverter();
                            product.Seller = sellerConverter.Read(ref reader, typeof(User), options);
                        }
                        else
                        {
                            reader.Skip();
                        }
                        break;
                    case "images":
                        if (reader.TokenType == JsonTokenType.Null)
                        {
                            product.Images = new List<ProductImage>();
                        }
                        else if (reader.TokenType == JsonTokenType.StartArray)
                        {
                            var images = new List<ProductImage>();

                            // Read array opening
                            while (reader.Read())
                            {
                                if (reader.TokenType == JsonTokenType.EndArray)
                                {
                                    break;
                                }

                                if (reader.TokenType == JsonTokenType.StartObject)
                                {
                                    var image = new ProductImage();

                                    // Read image object properties
                                    while (reader.Read())
                                    {
                                        if (reader.TokenType == JsonTokenType.EndObject)
                                        {
                                            break;
                                        }

                                        if (reader.TokenType == JsonTokenType.PropertyName)
                                        {
                                            string imagePropName = reader.GetString();
                                            reader.Read();

                                            if (imagePropName.ToLower() == "url")
                                            {
                                                image.Url = reader.GetString();
                                            }
                                            else if (imagePropName.ToLower() == "id")
                                            {
                                                image.Id = reader.GetInt32();
                                            }
                                            else if (imagePropName.ToLower() == "productid")
                                            {
                                                image.ProductId = reader.GetInt32();
                                            }
                                            else
                                            {
                                                reader.Skip();
                                            }
                                        }
                                    }

                                    images.Add(image);
                                }
                            }
                            product.Images = images;
                            Console.WriteLine($"DEBUG: AuctionProductConverter - Read {images.Count} images");
                        }
                        else
                        {
                            reader.Skip();
                        }
                        break;
                    case "tags":
                        if (reader.TokenType == JsonTokenType.Null)
                        {
                            product.Tags = new List<ProductTag>();
                        }
                        else if (reader.TokenType == JsonTokenType.StartArray)
                        {
                            var tags = new List<ProductTag>();

                            // Read array opening
                            while (reader.Read())
                            {
                                if (reader.TokenType == JsonTokenType.EndArray)
                                {
                                    break;
                                }

                                if (reader.TokenType == JsonTokenType.StartObject)
                                {
                                    var tag = new ProductTag();

                                    // Read tag object properties
                                    while (reader.Read())
                                    {
                                        if (reader.TokenType == JsonTokenType.EndObject)
                                        {
                                            break;
                                        }

                                        if (reader.TokenType == JsonTokenType.PropertyName)
                                        {
                                            string tagPropName = reader.GetString();
                                            reader.Read();

                                            if (tagPropName.ToLower() == "id")
                                            {
                                                tag.Id = reader.GetInt32();
                                            }
                                            else if (tagPropName.ToLower() == "title" || tagPropName.ToLower() == "displaytitle")
                                            {
                                                tag.Title = reader.GetString();
                                            }
                                            else
                                            {
                                                reader.Skip();
                                            }
                                        }
                                    }

                                    tags.Add(tag);
                                }
                            }
                            product.Tags = tags;
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

        public override void Write(Utf8JsonWriter writer, AuctionProduct value, JsonSerializerOptions options)
        {
            throw new NotImplementedException();
        }
    }
}