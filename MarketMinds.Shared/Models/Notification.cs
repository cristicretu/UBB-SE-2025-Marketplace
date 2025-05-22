using System.Diagnostics.CodeAnalysis;
using System.Text.Json.Serialization;

namespace MarketMinds.Shared.Models
{
    [ExcludeFromCodeCoverage]
    abstract public class Notification
    {
        [JsonPropertyName("notificationID")]
        public int NotificationID { get; set; }

        [JsonPropertyName("recipientID")]
        public int RecipientID { get; set; }

        [JsonPropertyName("category")]
        public NotificationCategory Category { get; protected set; }

        [JsonPropertyName("isRead")]
        public bool IsRead { get; set; }

        public bool IsNotRead { get => !IsRead; }

        [JsonPropertyName("timestamp")]
        public DateTime Timestamp { get; set; }

        public abstract string Title { get; }
        public abstract string Subtitle { get; }
        public abstract string Content { get; }
    }

    public class ContractRenewalAnswerNotification : Notification
    {
        public int ContractID { get; set; }
        public bool IsAccepted { get; set; }

        public ContractRenewalAnswerNotification() { }

        public ContractRenewalAnswerNotification(int recipientID, DateTime timestamp, int contractID, bool isAccepted, bool isRead = false, int notificationId = 0)
        {
            this.NotificationID = notificationId;
            this.RecipientID = recipientID;
            this.Timestamp = timestamp;
            this.IsRead = isRead;
            this.ContractID = contractID;
            Category = NotificationCategory.CONTRACT_RENEWAL_ANS;
            this.IsAccepted = isAccepted;
        }

        [ExcludeFromCodeCoverage]
        public override string Content => IsAccepted ? $"Contract with ID {ContractID} has been renewed!\n You can download it from below!" : $"Unfortunately, contract: {ContractID} has not been renewed!\nThe owner refused the renewal request :(";
        [ExcludeFromCodeCoverage]
        public override string Title => "Contract Renewal Answer";
        [ExcludeFromCodeCoverage]
        public override string Subtitle => $"You have received an answer on the renewal request for contract with ID {ContractID}.";
    }

    public class ContractRenewalWaitlistNotification : Notification
    {
        public int ProductID { get; set; }

        public ContractRenewalWaitlistNotification() { }

        public ContractRenewalWaitlistNotification(int recipientID, DateTime timestamp, int productID, bool isRead = false, int notificationId = 0)
        {
            this.NotificationID = notificationId;
            this.RecipientID = recipientID;
            this.Timestamp = timestamp;
            this.IsRead = isRead;
            this.ProductID = productID;
            Category = NotificationCategory.CONTRACT_RENEWAL_WAITLIST;
        }

        [ExcludeFromCodeCoverage]
        public override string Content => $"The user that borrowed product with ID {ProductID} that you are part of the waitlist for, has renewed its contract.";
        [ExcludeFromCodeCoverage]
        public override string Title => "Contract Renewal in Waitlist";
        [ExcludeFromCodeCoverage]
        public override string Subtitle => "A user has extended their contract.";
    }

    public class OutbiddedNotification : Notification
    {
        public int ProductID { get; set; }

        public OutbiddedNotification() { }

        public OutbiddedNotification(int recipientId, DateTime timestamp, int productId, bool isRead = false, int notificationId = 0)
        {
            NotificationID = notificationId;
            RecipientID = recipientId;
            this.Timestamp = timestamp;
            this.IsRead = isRead;
            this.ProductID = productId;
            Category = NotificationCategory.OUTBIDDED;
        }

        [ExcludeFromCodeCoverage]
        public override string Content => $"You've been outbid! Another buyer has placed a higher bid on product with ID {ProductID}. Place a new bid now!";
        [ExcludeFromCodeCoverage]
        public override string Title => "Outbidded";
        [ExcludeFromCodeCoverage]
        public override string Subtitle => $"You've been outbidded on product with ID {ProductID}.";
    }

    public class OrderShippingProgressNotification : Notification
    {
        public int OrderID { get; set; }
        public string ShippingState { get; set; }
        public DateTime DeliveryDate { get; set; }

        public OrderShippingProgressNotification() { }

        public OrderShippingProgressNotification(int recipientID, DateTime timestamp, int orderID, string shippingState, DateTime deliveryDate, bool isRead = false, int notificationID = 0)
        {
            NotificationID = notificationID;
            RecipientID = recipientID;
            Timestamp = timestamp;
            IsRead = isRead;
            OrderID = orderID;
            ShippingState = shippingState;
            Category = NotificationCategory.ORDER_SHIPPING_PROGRESS;
            DeliveryDate = deliveryDate;
        }

        [ExcludeFromCodeCoverage]
        public override string Content => $"Your order with ID {OrderID} has reached the a new state.";
        [ExcludeFromCodeCoverage]
        public override string Title => "Order Shipping Update";
        [ExcludeFromCodeCoverage]
        public override string Subtitle => $"New info on order with ID {OrderID} is available.";
    }

    public class PaymentConfirmationNotification : Notification
    {
        public int ProductID { get; set; }
        public int OrderID { get; set; }

        public PaymentConfirmationNotification() { }

        public PaymentConfirmationNotification(int recipientId, DateTime timestamp, int productId, int orderId, bool isRead = false, int notificationId = 0)
        {
            NotificationID = notificationId;
            RecipientID = recipientId;
            this.Timestamp = timestamp;
            this.IsRead = isRead;
            this.ProductID = productId;
            this.OrderID = orderId;
            Category = NotificationCategory.PAYMENT_CONFIRMATION;
        }

        [ExcludeFromCodeCoverage]
        public override string Content => $"Thank you for your purchase! Your order with ID {OrderID} for product with ID {ProductID} has been successfully processed.";
        [ExcludeFromCodeCoverage]
        public override string Title => "Payment Confirmation";
        [ExcludeFromCodeCoverage]
        public override string Subtitle => $"Order with ID {OrderID} has been processed successfully!";
    }

    public class ProductRemovedNotification : Notification
    {
        public int ProductID { get; set; }

        public ProductRemovedNotification() { }

        public ProductRemovedNotification(int recipientId, DateTime timestamp, int productId, bool isRead = false, int notificationId = 0)
        {
            NotificationID = notificationId;
            RecipientID = recipientId;
            this.Timestamp = timestamp;
            this.ProductID = productId;
            this.IsRead = isRead;
            Category = NotificationCategory.PRODUCT_REMOVED;
        }

        [ExcludeFromCodeCoverage]
        public override string Content => $"Unfortunately, the product with ID {ProductID} that you were waiting for was removed from the marketplace.";
        [ExcludeFromCodeCoverage]
        public override string Title => "Product Removed";
        [ExcludeFromCodeCoverage]
        public override string Subtitle => $"Product with ID {ProductID} was removed from the marketplace!";
    }

    public class ProductAvailableNotification : Notification
    {
        public int ProductID { get; set; }

        public ProductAvailableNotification() { }

        public ProductAvailableNotification(int recipientId, DateTime timestamp, int productId, bool isRead = false, int notificationId = 0)
        {
            NotificationID = notificationId;
            RecipientID = recipientId;
            this.Timestamp = timestamp;
            this.ProductID = productId;
            this.IsRead = isRead;
            Category = NotificationCategory.PRODUCT_AVAILABLE;
        }

        [ExcludeFromCodeCoverage]
        public override string Content => $"Good news! The product with ID {ProductID} that you were waiting for is now back in stock.";
        [ExcludeFromCodeCoverage]
        public override string Title => "Product Available";
        [ExcludeFromCodeCoverage]
        public override string Subtitle => $"Product with ID {ProductID} is available now!";
    }

    public class ContractRenewalRequestNotification : Notification
    {
        public int ContractID { get; set; }

        public ContractRenewalRequestNotification() { }
        public ContractRenewalRequestNotification(int recipientId, DateTime timestamp, int contractId, bool isRead = false, int notificationId = 0)
        {
            NotificationID = notificationId;
            RecipientID = recipientId;
            this.Timestamp = timestamp;
            this.ContractID = contractId;
            this.IsRead = isRead;
            Category = NotificationCategory.CONTRACT_RENEWAL_REQ;
        }

        [ExcludeFromCodeCoverage]
        public override string Content => $"User with ID {RecipientID} would like to renew contract with ID {ContractID}. Please respond promptly.";
        [ExcludeFromCodeCoverage]
        public override string Title => "Contract Renewal Request";
        [ExcludeFromCodeCoverage]
        public override string Subtitle => $"User with ID {RecipientID} wants to renew contract with ID {ContractID}.";
    }

    public class ContractExpirationNotification : Notification
    {
        public int ContractID { get; set; }
        public DateTime ExpirationDate { get; set; }

        public ContractExpirationNotification() { }
        public ContractExpirationNotification(int recipientId, DateTime timestamp, int contractId, DateTime expirationDate, bool isRead = false, int notificationId = 0)
        {
            NotificationID = notificationId;
            RecipientID = recipientId;
            this.Timestamp = timestamp;
            this.ContractID = contractId;
            this.IsRead = isRead;
            Category = NotificationCategory.CONTRACT_EXPIRATION;
            this.ExpirationDate = expirationDate;
        }

        [ExcludeFromCodeCoverage]
        public override string Content => $"Contract with ID: {ContractID} is set to expire soon.";
        [ExcludeFromCodeCoverage]
        public override string Title => "Contract Expiration";
        [ExcludeFromCodeCoverage]
        public override string Subtitle => $"Contract with ID: {ContractID} is about to expire!";
    }
}