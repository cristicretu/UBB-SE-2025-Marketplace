// <copyright file="OrderNotificationEntity.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

namespace Server.DataModels
{
    using System.ComponentModel.DataAnnotations.Schema;

    /// <summary>
    /// Represents the OrderNotification table structure for EF Core mapping.
    /// </summary>
    [Table("OrderNotifications")]
    public class OrderNotificationEntity
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="OrderNotificationEntity"/> class.
        /// Required by EF Core for the migrations.
        /// </summary>
        public OrderNotificationEntity()
        {
            this.Category = string.Empty;
            this.Timestamp = DateTimeOffset.MinValue;
        }

        /// <summary>
        /// Gets or sets the notification ID.
        /// </summary>
        public int NotificationID { get; set; }

        /// <summary>
        /// Gets or sets the recipient ID.
        /// </summary>
        public int RecipientID { get; set; }

        /// <summary>
        /// Gets or sets the category.
        /// </summary>
        public string Category { get; set; }

        /// <summary>
        /// Gets or sets the timestamp.
        /// </summary>
        public DateTimeOffset Timestamp { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the notification has been read.
        /// </summary>
        public bool IsRead { get; set; }

        /// <summary>
        /// Gets or sets the contract ID.
        /// Maria had this as int, but it should be long according to the DB
        /// design and to the Contract entity.
        /// This needs to be long in order for EF to be able to map the foreign key.
        /// </summary>
        public long? ContractID { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the notification has been accepted.
        /// </summary>
        public bool? IsAccepted { get; set; }

        /// <summary>
        /// Gets or sets the product ID.
        /// </summary>
        public int? ProductID { get; set; }

        /// <summary>
        /// Gets or sets the order ID.
        /// </summary>
        public int? OrderID { get; set; }

        /// <summary>
        /// Gets or sets the shipping state.
        /// </summary>
        public string? ShippingState { get; set; }

        /// <summary>
        /// Gets or sets the delivery date.
        /// </summary>
        public DateTimeOffset? DeliveryDate { get; set; }

        /// <summary>
        /// Gets or sets the expiration date.
        /// </summary>
        public DateTimeOffset? ExpirationDate { get; set; }
    }
}