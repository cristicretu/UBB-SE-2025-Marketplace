using System.Text.Json;
using System.Text.Json.Serialization;
using SharedClassLibrary.Domain;

public class NotificationConverter : JsonConverter<Notification>
{
    public override Notification Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        using var doc = JsonDocument.ParseValue(ref reader);
        var root = doc.RootElement;

        if (!root.TryGetProperty("category", out var categoryProp))
            throw new JsonException("Missing Category discriminator.");

        var category = (NotificationCategory)categoryProp.GetInt32();
        var json = root.GetRawText();

        return category switch
        {
            NotificationCategory.OUTBIDDED => JsonSerializer.Deserialize<OutbiddedNotification>(json, options),
            NotificationCategory.CONTRACT_RENEWAL_ACCEPTED => JsonSerializer.Deserialize<ContractRenewalAnswerNotification>(json, options),
            NotificationCategory.CONTRACT_RENEWAL_WAITLIST => JsonSerializer.Deserialize<ContractRenewalWaitlistNotification>(json, options),
            NotificationCategory.ORDER_SHIPPING_PROGRESS => JsonSerializer.Deserialize<OrderShippingProgressNotification>(json, options),
            NotificationCategory.PAYMENT_CONFIRMATION => JsonSerializer.Deserialize<PaymentConfirmationNotification>(json, options),
            NotificationCategory.PRODUCT_REMOVED => JsonSerializer.Deserialize<ProductRemovedNotification>(json, options),
            NotificationCategory.PRODUCT_AVAILABLE => JsonSerializer.Deserialize<ProductAvailableNotification>(json, options),
            NotificationCategory.CONTRACT_RENEWAL_REQUEST => JsonSerializer.Deserialize<ContractRenewalRequestNotification>(json, options),
            NotificationCategory.CONTRACT_EXPIRATION => JsonSerializer.Deserialize<ContractExpirationNotification>(json, options),
            _ => throw new NotSupportedException($"Unknown notification category: {category}")
        };
    }

    public override void Write(Utf8JsonWriter writer, Notification value, JsonSerializerOptions options)
    {
        JsonSerializer.Serialize(writer, (object)value, value.GetType(), options);
    }
}
