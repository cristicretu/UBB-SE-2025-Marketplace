// <copyright file="NotificationApiController.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

namespace Server.Controllers
{
    using System;
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using global::MarketMinds.Shared.IRepository;
    using global::MarketMinds.Shared.Models;
    using Microsoft.AspNetCore.Authorization;
    using Microsoft.AspNetCore.Http;
    using Microsoft.AspNetCore.Mvc;

    /// <summary>
    /// API controller for managing notification data.
    /// </summary>
    [Route("api/notifications")]
    [ApiController]
    public class NotificationApiController : ControllerBase
    {
        private readonly INotificationRepository notificationRepository;

        /// <summary>
        /// Initializes a new instance of the <see cref="NotificationApiController"/> class.
        /// </summary>
        /// <param name="notificationRepository">The notification repository dependency.</param>
        public NotificationApiController(INotificationRepository notificationRepository)
        {
            this.notificationRepository = notificationRepository;
        }

        /// <summary>
        /// Adds a new notification.
        /// </summary>
        /// <param name="notification">The notification to add.</param>
        /// <returns>An action result indicating success or failure.</returns>
        [HttpPost]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(typeof(string), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(string), StatusCodes.Status500InternalServerError)]
        public ActionResult AddNotification([FromBody] Notification notification)
        {
            if (notification == null)
            {
                return this.BadRequest("Notification object is required.");
            }

            try
            {
                this.notificationRepository.AddNotification(notification);
                return this.Created();
            }
            catch (ArgumentNullException ex)
            {
                return this.BadRequest(ex.Message);
            }
            catch (ArgumentException ex)
            {
                return this.BadRequest(ex.Message);
            }
            catch (Exception ex)
            {
                return this.StatusCode(StatusCodes.Status500InternalServerError, $"An error occurred while adding notification: {ex.Message}");
            }
        }

        /// <summary>
        /// Retrieves notifications for a user based on recipient ID.
        /// </summary>
        /// <param name="recipientId">The ID of the recipient.</param>
        /// <returns>A list of notifications for the user.</returns>
        [HttpGet("user/{recipientId}")]
        [ProducesResponseType(typeof(List<Notification>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(string), StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<List<object>>> GetNotificationsForUser(int recipientId)
        {
            try
            {
                var notifications = await this.notificationRepository.GetNotificationsForUser(recipientId);
                var notificationsWithCategory = new List<object>();

                foreach (var notification in notifications)
                {
                    object serializedNotification = notification switch
                    {
                        OrderShippingProgressNotification shippingNotification => new
                        {
                            shippingNotification.NotificationID,
                            shippingNotification.RecipientID,
                            shippingNotification.IsRead,
                            shippingNotification.Timestamp,
                            shippingNotification.Category,
                            shippingNotification.OrderID,
                            shippingNotification.ShippingState,
                            shippingNotification.DeliveryDate
                        },
                        ContractRenewalRequestNotification contractNotification => new
                        {
                            contractNotification.NotificationID,
                            contractNotification.RecipientID,
                            contractNotification.IsRead,
                            contractNotification.Timestamp,
                            contractNotification.Category,
                            ContractID = contractNotification.ContractID
                        },
                        ContractRenewalAnswerNotification answerNotification => new
                        {
                            answerNotification.NotificationID,
                            answerNotification.RecipientID,
                            answerNotification.IsRead,
                            answerNotification.Timestamp,
                            answerNotification.Category,
                            answerNotification.ContractID,
                            answerNotification.IsAccepted
                        },
                        ContractRenewalWaitlistNotification waitlistNotification => new
                        {
                            waitlistNotification.NotificationID,
                            waitlistNotification.RecipientID,
                            waitlistNotification.IsRead,
                            waitlistNotification.Timestamp,
                            waitlistNotification.Category,
                            waitlistNotification.ProductID
                        },
                        OutbiddedNotification outbiddedNotification => new
                        {
                            outbiddedNotification.NotificationID,
                            outbiddedNotification.RecipientID,
                            outbiddedNotification.IsRead,
                            outbiddedNotification.Timestamp,
                            outbiddedNotification.Category,
                            outbiddedNotification.ProductID
                        },
                        PaymentConfirmationNotification paymentNotification => new
                        {
                            paymentNotification.NotificationID,
                            paymentNotification.RecipientID,
                            paymentNotification.IsRead,
                            paymentNotification.Timestamp,
                            paymentNotification.Category,
                            paymentNotification.ProductID,
                            paymentNotification.OrderID
                        },
                        ProductRemovedNotification removedNotification => new
                        {
                            removedNotification.NotificationID,
                            removedNotification.RecipientID,
                            removedNotification.IsRead,
                            removedNotification.Timestamp,
                            removedNotification.Category,
                            removedNotification.ProductID
                        },
                        ProductAvailableNotification availableNotification => new
                        {
                            availableNotification.NotificationID,
                            availableNotification.RecipientID,
                            availableNotification.IsRead,
                            availableNotification.Timestamp,
                            availableNotification.Category,
                            availableNotification.ProductID
                        },
                        ContractExpirationNotification expirationNotification => new
                        {
                            expirationNotification.NotificationID,
                            expirationNotification.RecipientID,
                            expirationNotification.IsRead,
                            expirationNotification.Timestamp,
                            expirationNotification.Category,
                            expirationNotification.ContractID,
                            expirationNotification.ExpirationDate
                        },
                        Notification baseNotification => new // Handle the base Notification type if needed
                        {
                            baseNotification.NotificationID,
                            baseNotification.RecipientID,
                            baseNotification.IsRead,
                            baseNotification.Timestamp,
                            baseNotification.Category
                        },
                        _ => throw new NotSupportedException($"Unsupported notification type: {notification.GetType().Name}")
                    };
                    notificationsWithCategory.Add(serializedNotification);
                }

                // Return the notifications with the Category field included
                return this.Ok(notificationsWithCategory);
            }
            catch (Exception ex)
            {
                return this.StatusCode(StatusCodes.Status500InternalServerError, $"An error occurred while retrieving notifications for user {recipientId}: {ex.Message}");
            }
        }
        /// <summary>
        /// Marks a notification as read.
        /// </summary>
        /// <param name="notificationId">The ID of the notification to mark as read.</param>
        /// <returns>An action result indicating success or failure.</returns>
        [HttpPut("{userId}/mark-read")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(typeof(string), StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult> MarkAsRead(int userId)
        {
            try
            {
                await this.notificationRepository.MarkAsRead(userId);
                return this.NoContent();
            }
            catch (Exception ex)
            {
                return this.StatusCode(StatusCodes.Status500InternalServerError, $"An error occurred while marking notifications from user: {userId} as read: {ex.Message}");
            }
        }

        /// <summary>
        /// Clears (deletes) all notifications for a user.
        /// </summary>
        /// <param name="userId">The ID of the user whose notifications to clear.</param>
        /// <returns>An action result indicating success or failure.</returns>
        [HttpDelete("{userId}/clear-all")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(typeof(string), StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult> ClearAll(int userId)
        {
            try
            {
                await this.notificationRepository.ClearAllNotifications(userId);
                return this.NoContent();
            }
            catch (Exception ex)
            {
                return this.StatusCode(StatusCodes.Status500InternalServerError, $"An error occurred while clearing notifications for user: {userId}: {ex.Message}");
            }
        }
    }
}
