// <copyright file="NotificationApiController.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

namespace Server.Controllers
{
    using System;
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using Microsoft.AspNetCore.Authorization;
    using Microsoft.AspNetCore.Http;
    using Microsoft.AspNetCore.Mvc;
    using SharedClassLibrary.Domain;
    using SharedClassLibrary.IRepository;

    /// <summary>
    /// API controller for managing notification data.
    /// </summary>
    [Authorize]
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
        public async Task<ActionResult<List<Notification>>> GetNotificationsForUser(int recipientId)
        {
            try
            {
                var notifications = await this.notificationRepository.GetNotificationsForUser(recipientId);

                // Explicitly serialize the notifications and add the Category field to the JSON
                var notificationsWithCategory = notifications.Select(notification => new
                {
                    notification.NotificationID,
                    notification.RecipientID,
                    notification.IsRead,
                    notification.Timestamp,
                    notification.Category,
                    // Add properties specific to each subclass, e.g.:
                    // Add ContractID for ContractRenewalRequestNotification
                    ContractID = notification is ContractRenewalRequestNotification contractNotification
                        ? contractNotification.ContractID
                        : (int?)null,
                    // Add other fields specific to other types of Notification
                }).ToList();

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
        [HttpPut("{notificationId}/mark-read")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(typeof(string), StatusCodes.Status500InternalServerError)]
        public ActionResult MarkAsRead(int notificationId)
        {
            try
            {
                this.notificationRepository.MarkAsRead(notificationId);
                return this.NoContent();
            }
            catch (Exception ex)
            {
                return this.StatusCode(StatusCodes.Status500InternalServerError, $"An error occurred while marking notification {notificationId} as read: {ex.Message}");
            }
        }
    }
}
