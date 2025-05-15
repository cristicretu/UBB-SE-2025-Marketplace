// <copyright file="NotificationRepository.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

namespace Server.Repository
{
    using System;
    using System.Collections.Generic;
    using System.Data;
    using System.Diagnostics.CodeAnalysis;
    using Microsoft.EntityFrameworkCore;
    using Server.DataModels;
    using Server.DBConnection;
    using SharedClassLibrary.Domain;
    using SharedClassLibrary.IRepository;

    /// <summary>
    /// Represents a repository for managing notifications in the database.
    /// </summary>
    public class NotificationRepository : INotificationRepository
    {
        private readonly MarketPlaceDbContext dbContext;

        /// <summary>
        /// Initializes a new instance of the <see cref="NotificationRepository"/> class.
        /// </summary>
        /// <param name="dbContext">The database context.</param>
        [ExcludeFromCodeCoverage]
        public NotificationRepository(MarketPlaceDbContext dbContext)
        {
            this.dbContext = dbContext;
        }

        /// <summary>
        /// Retrieves notification for a user based on ID.
        /// </summary>
        /// <param name="recipientId">Id of the recipient to for which to retrieve notifications.</param>
        /// <returns>List of notifications.</returns>
        public async Task<List<Notification>> GetNotificationsForUser(int recipientId)
        {
            List<Notification> notifications = new List<Notification>();

            // Get all OrderNotifications for the recipient
            List<OrderNotificationEntity> notificationsDb = await this.dbContext.OrderNotifications.Where(notification => notification.RecipientID == recipientId).ToListAsync();

            // Convert each OrderNotification to a Notification
            foreach (OrderNotificationEntity notification in notificationsDb)
            {
                notifications.Add(CreateFromOrderNotificationEntity(notification));
            }

            // Return the list of converted notifications
            return notifications;
        }

        /// <summary>
        /// Marks a notification as read in the database using the notification ID.
        /// </summary>
        /// <param name="notificationId">Notification ID of the notification to be marked as read.</param>
        /// <exception cref="ArgumentException">Thrown when the notification is not found.</exception>
        public async void MarkAsRead(int notificationId)
        {
            OrderNotificationEntity notification = await this.dbContext.OrderNotifications.FindAsync(notificationId)
                                                        ?? throw new ArgumentException("MarkAsRead: Notification not found");
            notification.IsRead = true;
            await this.dbContext.SaveChangesAsync();
        }

        /// <summary>
        /// Adds a notification to the database.
        /// </summary>
        /// <param name="notification">Notification to be added to the database.</param>
        /// <exception cref="ArgumentNullException">When the notification is null.</exception>
        public async void AddNotification(Notification notification)
        {
            ArgumentNullException.ThrowIfNull(notification);

            // Convert the notification to an OrderNotificationEntity based on the type of notification
            OrderNotificationEntity orderNotification = CreateFromNotification(notification);

            // Add the OrderNotificationEntity to the database
            this.dbContext.OrderNotifications.Add(orderNotification);
            await this.dbContext.SaveChangesAsync();
        }

        /// <summary>
        /// Creates a Notification object from an OrderNotificationEntity object.
        /// </summary>
        /// <param name="orderNotification">The order notification entity to create a notification from.</param>
        /// <returns>The created notification.</returns>
        /// <exception cref="ArgumentException">Thrown in certain cases where the order notification entity is invalid.</exception>
        private static Notification CreateFromOrderNotificationEntity(OrderNotificationEntity orderNotification)
        {
            int notificationId = orderNotification.NotificationID;
            int recipientId = orderNotification.RecipientID;
            DateTime timestamp = orderNotification.Timestamp.DateTime;
            bool isRead = orderNotification.IsRead;
            string category = orderNotification.Category.ToString();

            switch (category)
            {
                case "CONTRACT_RENEWAL_ACCEPTED":
                    if (orderNotification.ContractID == null)
                    {
                        throw new ArgumentException("CreateFromOrderNotificationEntity: Contract ID is null");
                    }

                    int contractId = (int)orderNotification.ContractID;
                    bool isAccepted = orderNotification.IsAccepted ?? false;
                    return new ContractRenewalAnswerNotification(recipientId, timestamp, contractId, isAccepted, isRead, notificationId);

                case "CONTRACT_RENEWAL_WAITLIST":
                    if (orderNotification.ProductID == null)
                    {
                        throw new ArgumentException("CreateFromOrderNotificationEntity: Product ID is null");
                    }

                    int productIdWaitlist = (int)orderNotification.ProductID;
                    return new ContractRenewalWaitlistNotification(recipientId, timestamp, productIdWaitlist, isRead, notificationId);

                case "OUTBIDDED":
                    if (orderNotification.ProductID == null)
                    {
                        throw new ArgumentException("CreateFromOrderNotificationEntity: Product ID is null");
                    }

                    int productIdOutbidded = (int)orderNotification.ProductID;
                    return new OutbiddedNotification(recipientId, timestamp, productIdOutbidded, isRead, notificationId);

                case "ORDER_SHIPPING_PROGRESS":
                    if (orderNotification.OrderID == null || orderNotification.ShippingState == null || orderNotification.DeliveryDate == null)
                    {
                        throw new ArgumentException("CreateFromOrderNotificationEntity: Order ID, shipping state, or delivery date is null");
                    }

                    int orderId = (int)orderNotification.OrderID;
                    string shippingState = orderNotification.ShippingState;
                    DateTime deliveryDate = orderNotification.DeliveryDate.Value.DateTime;
                    return new OrderShippingProgressNotification(recipientId, timestamp, orderId, shippingState, deliveryDate, isRead, notificationId);

                case "PAYMENT_CONFIRMATION":
                    if (orderNotification.ProductID == null || orderNotification.OrderID == null)
                    {
                        throw new ArgumentException("CreateFromOrderNotificationEntity: Product ID or order ID is null");
                    }

                    int productIdPayment = (int)orderNotification.ProductID;
                    int orderIdPayment = (int)orderNotification.OrderID;
                    return new PaymentConfirmationNotification(recipientId, timestamp, productIdPayment, orderIdPayment, isRead, notificationId);

                case "PRODUCT_REMOVED":
                    if (orderNotification.ProductID == null)
                    {
                        throw new ArgumentException("CreateFromOrderNotificationEntity: Product ID is null");
                    }

                    int productIdRemoved = (int)orderNotification.ProductID;
                    return new ProductRemovedNotification(recipientId, timestamp, productIdRemoved, isRead, notificationId);

                case "PRODUCT_AVAILABLE":
                    if (orderNotification.ProductID == null)
                    {
                        throw new ArgumentException("CreateFromOrderNotificationEntity: Product ID is null");
                    }

                    int productIdAvailable = (int)orderNotification.ProductID;
                    return new ProductAvailableNotification(recipientId, timestamp, productIdAvailable, isRead, notificationId);

                case "CONTRACT_RENEWAL_REQUEST":
                    if (orderNotification.ContractID == null)
                    {
                        throw new ArgumentException("CreateFromOrderNotificationEntity: Contract ID is null");
                    }

                    int contractIdReq = (int)orderNotification.ContractID;
                    return new ContractRenewalRequestNotification(recipientId, timestamp, contractIdReq, isRead, notificationId);

                case "CONTRACT_EXPIRATION":
                    if (orderNotification.ContractID == null || orderNotification.ExpirationDate == null)
                    {
                        throw new ArgumentException("CreateFromOrderNotificationEntity: Contract ID or expiration date is null");
                    }

                    int contractIdExp = (int)orderNotification.ContractID;
                    DateTime expirationDate = orderNotification.ExpirationDate.Value.DateTime;
                    return new ContractExpirationNotification(recipientId, timestamp, contractIdExp, expirationDate, isRead, notificationId);

                default:
                    throw new ArgumentException($"Unknown notification category: {category}");
            }
        }

        /// <summary>
        /// Creates an OrderNotificationEntity object from a Notification object.
        /// </summary>
        /// <param name="notification">The notification to create an OrderNotificationEntity from.</param>
        /// <returns>The created OrderNotificationEntity object.</returns>
        /// <exception cref="ArgumentException">Thrown when the notification is not a valid type.</exception>
        private static OrderNotificationEntity CreateFromNotification(Notification notification)
        {
            /* --- Common fields for all notifications --- */
            OrderNotificationEntity orderNotificationToReturn = new OrderNotificationEntity
            {
                NotificationID = notification.NotificationID,
                RecipientID = notification.RecipientID,
                Category = notification.Category.ToString(),
                Timestamp = notification.Timestamp,
                IsRead = notification.IsRead,
            };

            switch (notification)
            {
                case ContractRenewalAnswerNotification crAnswer:
                    /* --- ContractRenewalAnswerNotification specific fields --- */
                    orderNotificationToReturn.ContractID = crAnswer.ContractID;
                    orderNotificationToReturn.IsAccepted = crAnswer.IsAccepted;
                    /* --- Rest of optional fields from the other Notification classes --- */
                    orderNotificationToReturn.ProductID = null;
                    orderNotificationToReturn.OrderID = null;
                    orderNotificationToReturn.ShippingState = null;
                    orderNotificationToReturn.DeliveryDate = null;
                    orderNotificationToReturn.ExpirationDate = null;
                    break;
                case ContractRenewalWaitlistNotification crWaitlist:
                    /* --- ContractRenewalWaitlistNotification specific fields --- */
                    orderNotificationToReturn.ProductID = crWaitlist.ProductID;
                    /* --- Rest of optional fields from the other Notification classes --- */
                    orderNotificationToReturn.ContractID = null;
                    orderNotificationToReturn.IsAccepted = null;
                    orderNotificationToReturn.OrderID = null;
                    orderNotificationToReturn.ShippingState = null;
                    orderNotificationToReturn.DeliveryDate = null;
                    orderNotificationToReturn.ExpirationDate = null;
                    break;
                case OutbiddedNotification outbid:
                    /* --- OutbiddedNotification specific fields --- */
                    orderNotificationToReturn.ProductID = outbid.ProductID;
                    /* --- Rest of optional fields from the other Notification classes --- */
                    orderNotificationToReturn.ContractID = null;
                    orderNotificationToReturn.IsAccepted = null;
                    orderNotificationToReturn.OrderID = null;
                    orderNotificationToReturn.ShippingState = null;
                    orderNotificationToReturn.DeliveryDate = null;
                    orderNotificationToReturn.ExpirationDate = null;
                    break;
                case OrderShippingProgressNotification shipping:
                    /* --- OrderShippingProgressNotification specific fields --- */
                    orderNotificationToReturn.OrderID = shipping.OrderID;
                    orderNotificationToReturn.ShippingState = shipping.ShippingState;
                    orderNotificationToReturn.DeliveryDate = shipping.DeliveryDate;
                    /* --- Rest of optional fields from the other Notification classes --- */
                    orderNotificationToReturn.ContractID = null;
                    orderNotificationToReturn.IsAccepted = null;
                    orderNotificationToReturn.ProductID = null;
                    orderNotificationToReturn.ExpirationDate = null;
                    break;
                case PaymentConfirmationNotification payment:
                    /* --- PaymentConfirmationNotification specific fields --- */
                    orderNotificationToReturn.OrderID = payment.OrderID;
                    orderNotificationToReturn.ProductID = payment.ProductID;
                    /* --- Rest of optional fields from the other Notification classes --- */
                    orderNotificationToReturn.ContractID = null;
                    orderNotificationToReturn.IsAccepted = null;
                    orderNotificationToReturn.ShippingState = null;
                    orderNotificationToReturn.DeliveryDate = null;
                    orderNotificationToReturn.ExpirationDate = null;
                    break;
                case ProductRemovedNotification removed:
                    /* --- ProductRemovedNotification specific fields --- */
                    orderNotificationToReturn.ProductID = removed.ProductID;
                    /* --- Rest of optional fields from the other Notification classes --- */
                    orderNotificationToReturn.ContractID = null;
                    orderNotificationToReturn.IsAccepted = null;
                    orderNotificationToReturn.OrderID = null;
                    orderNotificationToReturn.ShippingState = null;
                    orderNotificationToReturn.DeliveryDate = null;
                    orderNotificationToReturn.ExpirationDate = null;
                    break;
                case ProductAvailableNotification available:
                    /* --- ProductAvailableNotification specific fields --- */
                    orderNotificationToReturn.ProductID = available.ProductID;
                    /* --- Rest of optional fields from the other Notification classes --- */
                    orderNotificationToReturn.ContractID = null;
                    orderNotificationToReturn.IsAccepted = null;
                    orderNotificationToReturn.OrderID = null;
                    orderNotificationToReturn.ShippingState = null;
                    orderNotificationToReturn.DeliveryDate = null;
                    orderNotificationToReturn.ExpirationDate = null;
                    break;
                case ContractRenewalRequestNotification request:
                    /* --- ContractRenewalRequestNotification specific fields --- */
                    orderNotificationToReturn.ContractID = request.ContractID;
                    /* --- Rest of optional fields from the other Notification classes --- */
                    orderNotificationToReturn.IsAccepted = null;
                    orderNotificationToReturn.ProductID = null;
                    orderNotificationToReturn.OrderID = null;
                    orderNotificationToReturn.ShippingState = null;
                    orderNotificationToReturn.DeliveryDate = null;
                    orderNotificationToReturn.ExpirationDate = null;
                    break;
                case ContractExpirationNotification expiration:
                    /* --- ContractExpirationNotification specific fields --- */
                    orderNotificationToReturn.ContractID = expiration.ContractID;
                    orderNotificationToReturn.ExpirationDate = expiration.ExpirationDate;
                    /* --- Rest of optional fields from the other Notification classes --- */
                    orderNotificationToReturn.IsAccepted = null;
                    orderNotificationToReturn.ProductID = null;
                    orderNotificationToReturn.OrderID = null;
                    orderNotificationToReturn.ShippingState = null;
                    orderNotificationToReturn.DeliveryDate = null;
                    break;
                default:
                    throw new ArgumentException($"Unknown notification type: {notification.GetType()}");
            }

            return orderNotificationToReturn;
        }
    }
}
