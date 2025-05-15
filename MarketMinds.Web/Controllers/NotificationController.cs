using Microsoft.AspNetCore.Mvc;
using SharedClassLibrary.Service;
using System.Diagnostics;
using WebMarketplace.Models;

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
            int userId = 1; // Replace with actual user ID logic
            var notifications = await _notificationService.GetNotificationsForUser(userId);
            Debug.WriteLine($"Notifications count: {notifications.Count}");

            if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
            {
                // Return JSON for fetch()
                var jsonResult = notifications.Select(n => new
                {
                    content = n.Content,
                    timestamp = n.Timestamp,
                    isRead = n.IsRead
                });

                return Json(jsonResult);
            }

            // Otherwise return the partial view for normal requests
            var unreadCount = notifications.Count(n => !n.IsRead);
            var unreadCountText = _notificationService.GetUnreadNotificationsCountText(unreadCount);
            ViewData["UnreadCountText"] = unreadCountText;

            return PartialView("_NotificationsButton", notifications);
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
            int userId = 1; // Replace with actual user logic
            var notifications = await _notificationService.GetNotificationsForUser(userId);
            var unreadCount = notifications.Count(n => !n.IsRead);
            return Json(new { count = unreadCount });
        }


        /// <summary>
        /// Marks all notifications as read for the user.
        /// </summary>
        /// <returns>
        /// Partial view with updated notifications.
        /// </returns>
        [HttpPost("MarkAllAsRead")]
        public async Task<IActionResult> MarkAllAsRead()
        {
            int userId = 1; // Replace with your actual user identity method
            var notifications = await _notificationService.GetNotificationsForUser(userId);

            foreach (var notification in notifications)
            {
                if (!notification.IsRead)
                {
                    _notificationService.MarkAsRead(notification.NotificationID);
                }
            }

            // Return the updated partial view
            notifications = await _notificationService.GetNotificationsForUser(userId);
            var unreadCount = notifications.Count(n => !n.IsRead);
            var unreadCountText = _notificationService.GetUnreadNotificationsCountText(unreadCount);

            ViewData["UnreadCountText"] = unreadCountText;
            return PartialView("_NotificationListPartial", notifications);
        }


    }

}
