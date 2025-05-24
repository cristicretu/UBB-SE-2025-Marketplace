using Microsoft.AspNetCore.Mvc;
using MarketMinds.Shared.Services;
using System.Diagnostics;
using System;
using System.Threading.Tasks;
using System.Linq;
using System.Collections.Generic;

namespace WebMarketplace.Controllers
{
    [ApiController]
    [Route("Notifications")]
    public class NotificationController : Controller
    {
        private readonly INotificationContentService _notificationService;

        public NotificationController(INotificationContentService notificationService)
        {
            _notificationService = notificationService;
        }

        /// <summary>
        /// Fetches notifications for the user.
        /// </summary>
        /// <returns>
        /// JSON data if the request is an AJAX request; otherwise, a partial view.
        /// </returns>
        [HttpGet]
        public async Task<IActionResult> Notifications()
        {
            int userId = UserSession.CurrentUserId ?? throw new InvalidOperationException("User ID is not available.");
            var notifications = await _notificationService.GetNotificationsForUser(userId);
        Debug.WriteLine($"Notifications count: {notifications.Count}");

        // For AJAX requests, check if client wants HTML
        if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
        {
            if (Request.Headers["Accept"].ToString().Contains("text/html"))
            {
                // Return the partial view for AJAX requests that want HTML
                return PartialView("_Notifications", notifications);
            }
            
            // Return JSON only when client specifically wants JSON
            var jsonResult = notifications.Select(n => new
            {
                id = n.NotificationID,
                content = n.Content,
                timestamp = n.Timestamp,
                isRead = n.IsRead
            });
            
            return Json(jsonResult);
    }

    // Return the partial view for normal requests
    return PartialView("_Notifications", notifications);
}
        /// <summary>
        /// Fetches the count of unread notifications for the user.
        /// </summary>
        /// <returns>
        /// JSON object containing the count of unread notifications.
        /// </returns>
        [HttpGet("Count")]
        public async Task<IActionResult> GetUnreadCount()
        {
            int userId = UserSession.CurrentUserId ?? throw new InvalidOperationException("User ID is not available.");
            var unreadNotifications = await _notificationService.GetUnreadNotificationsForUser(userId);
            var unreadCount = unreadNotifications.Count;
            return Json(new { count = unreadCount });
        }

        /// <summary>
        /// Marks all notifications as read for the user.
        /// </summary>
        /// <returns>
        /// JSON result indicating success and the updated unread count.
        /// </returns>
        [HttpPost("MarkAllAsRead")]
        public async Task<IActionResult> MarkAllAsRead()
        {
            try
            {
                int userId = UserSession.CurrentUserId ?? throw new InvalidOperationException("User ID is not available.");
                
                // Call the service with the correct parameter (userId)
                await _notificationService.MarkAllAsRead(userId);

                // Get updated notifications to calculate the new unread count
                var unreadNotifications = await _notificationService.GetUnreadNotificationsForUser(userId);
                var unreadCount = unreadNotifications.Count;

                // Return JSON response with success status and updated count
                return Json(new { success = true, unreadCount });
            }
            catch (Exception ex)
            {
                // Log the exception
                Debug.WriteLine($"Error marking notifications as read: {ex.Message}");
                return Json(new { success = false, error = ex.Message });
            }
        }

        /// <summary>
        /// Marks a specific notification as read.
        /// </summary>
        /// <param name="id">The notification ID to mark as read.</param>
        /// <returns>JSON result indicating success.</returns>
        [HttpPost("MarkAsRead/{id}")]
        public async Task<IActionResult> MarkAsRead(int id)
        {
            try
            {
                int userId = UserSession.CurrentUserId ?? throw new InvalidOperationException("User ID is not available.");
                await _notificationService.MarkNotificationAsRead(id);
                
                // Get updated unread count
                var unreadNotifications = await _notificationService.GetUnreadNotificationsForUser(userId);
                var unreadCount = unreadNotifications.Count;
                
                return Json(new { success = true, unreadCount });
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error marking notification as read: {ex.Message}");
                return Json(new { success = false, error = ex.Message });
            }
        }

        /// <summary>
        /// Clears all notifications for the current user.
        /// </summary>
        /// <returns>JSON result indicating success.</returns>
        [HttpPost("ClearAll")]
        public async Task<IActionResult> ClearAll()
        {
            try
            {
                int userId = UserSession.CurrentUserId ?? throw new InvalidOperationException("User ID is not available.");
                await _notificationService.ClearAllNotifications(userId);
                return Json(new { success = true });
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error clearing notifications: {ex.Message}");
                return Json(new { success = false, error = ex.Message });
            }
        }
    }
}
