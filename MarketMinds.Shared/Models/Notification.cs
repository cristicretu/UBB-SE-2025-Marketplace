using System;
using System.Diagnostics.CodeAnalysis;

namespace SharedClassLibrary.Domain
{
    [ExcludeFromCodeCoverage]
    abstract public class Notification
    {
        public int NotificationID { get; set; }
        public int RecipientID { get; set; }
        public NotificationCategory Category { get; protected set; }
        public bool IsRead { get; set; }

        public bool IsNotRead { get => !IsRead; }
        public DateTime Timestamp { get; set; }

        public abstract string Title { get; }
        public abstract string Subtitle { get; }
        public abstract string Content { get; }
    }

    public class ContractRenewalAnswerNotification : Notification
    {
        public int ContractID { get; }
        public bool IsAccepted { get; }

        public ContractRenewalAnswerNotification(int recipientID, DateTime timestamp, int contractID, bool isAccepted, bool isRead = false, int notificationId = 0)
        {
            this.NotificationID = notificationId;
            this.RecipientID = recipientID;
            this.Timestamp = timestamp;
            this.IsRead = isRead;
            this.ContractID = contractID;
            Category = NotificationCategory.CONTRACT_RENEWAL_ACCEPTED;
            this.IsAccepted = isAccepted;
        }

        [ExcludeFromCodeCoverage]
        public override string Content => IsAccepted ? $"Contract: {ContractID} has been renewed!\reader\n You can download it from below!" : $"Unfortunately, contract: {ContractID} has not been renewed!\reader\n The owner refused the renewal request :(";
        [ExcludeFromCodeCoverage]
        public override string Title => "Contract Renewal Answer";
        [ExcludeFromCodeCoverage]
        public override string Subtitle => $"You have received an answer on the renewal request for contract: {ContractID}.";
    }

    public class ContractRenewalWaitlistNotification : Notification
    {
        public int ProductID { get; }

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
        public override string Content => $"The user that borrowed product: {ProductID} that you are part of the waitlist for, has renewed its contract.";
        [ExcludeFromCodeCoverage]
        public override string Title => "Contract Renewal in Waitlist";
        [ExcludeFromCodeCoverage]
        public override string Subtitle => "A user has extended their contract.";
    }

    public class OutbiddedNotification : Notification
    {
        public int ProductID { get; }

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
        public override string Content => $"You've been outbid! Another buyer has placed a higher bid on product: {ProductID}. Place a new bid now!";
        [ExcludeFromCodeCoverage]
        public override string Title => "Outbidded";
        [ExcludeFromCodeCoverage]
        public override string Subtitle => $"You've been outbidded on product: {ProductID}.";
    }

    public class OrderShippingProgressNotification : Notification
    {
        public int OrderID { get; }
        public string ShippingState { get; }
        public DateTime DeliveryDate { get; }

        public OrderShippingProgressNotification(int recipientId, DateTime timestamp, int id, string state, DateTime deliveryDate, bool isRead = false, int notificationId = 0)
        {
            NotificationID = notificationId;
            RecipientID = recipientId;
            this.Timestamp = timestamp;
            this.IsRead = isRead;
            this.OrderID = id;
            this.ShippingState = state;
            Category = NotificationCategory.ORDER_SHIPPING_PROGRESS;
            this.DeliveryDate = deliveryDate;
        }

        [ExcludeFromCodeCoverage]
        public override string Content => $"Your order: {OrderID} has reached the {ShippingState} stage. Estimated delivery is on {DeliveryDate}.";
        [ExcludeFromCodeCoverage]
        public override string Title => "Order Shipping Update";
        [ExcludeFromCodeCoverage]
        public override string Subtitle => $"New info on order: {OrderID} is available.";
    }

    public class PaymentConfirmationNotification : Notification
    {
        public int ProductID { get; }
        public int OrderID { get; }

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
        public override string Content => $"Thank you for your purchase! Your order: {OrderID} for product: {ProductID} has been successfully processed.";
        [ExcludeFromCodeCoverage]
        public override string Title => "Payment Confirmation";
        [ExcludeFromCodeCoverage]
        public override string Subtitle => $"Order: {OrderID} has been processed successfully!";
    }

    public class ProductRemovedNotification : Notification
    {
        public int ProductID { get; }

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
        public override string Content => $"Unfortunately, the product: {ProductID} that you were waiting for was removed from the marketplace.";
        [ExcludeFromCodeCoverage]
        public override string Title => "Product Removed";
        [ExcludeFromCodeCoverage]
        public override string Subtitle => $"Product: {ProductID} was removed from the marketplace!";
    }

    public class ProductAvailableNotification : Notification
    {
        public int ProductID { get; }

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
        public override string Content => $"Good news! The product: {ProductID} that you were waiting for is now back in stock.";
        [ExcludeFromCodeCoverage]
        public override string Title => "Product Available";
        [ExcludeFromCodeCoverage]
        public override string Subtitle => $"Product: {ProductID} is available now!";
    }

    public class ContractRenewalRequestNotification : Notification
    {
        public int ContractID { get; }

        public ContractRenewalRequestNotification(int recipientId, DateTime timestamp, int contractId, bool isRead = false, int notificationId = 0)
        {
            NotificationID = notificationId;
            RecipientID = recipientId;
            this.Timestamp = timestamp;
            this.ContractID = contractId;
            this.IsRead = isRead;
            Category = NotificationCategory.CONTRACT_RENEWAL_REQUEST;
        }

        [ExcludeFromCodeCoverage]
        public override string Content => $"User {RecipientID} would like to renew contract: {ContractID}. Please respond promptly.";
        [ExcludeFromCodeCoverage]
        public override string Title => "Contract Renewal Request";
        [ExcludeFromCodeCoverage]
        public override string Subtitle => $"User {RecipientID} wants to renew contract: {ContractID}.";
    }

    public class ContractExpirationNotification : Notification
    {
        public int ContractID { get; }
        public DateTime ExpirationDate { get; }

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
        public override string Content => $"Contract: {ContractID} is set to expire on {ExpirationDate}.";
        [ExcludeFromCodeCoverage]
        public override string Title => "Contract Expiration";
        [ExcludeFromCodeCoverage]
        public override string Subtitle => $"Contract: {ContractID} is about to expire!";
    }
}