using Microsoft.AspNetCore.Mvc;
using MarketMinds.Shared.Services;
using System.Diagnostics;
using System;
using System.Threading.Tasks;
using System.Linq;
using System.Collections.Generic;

namespace WebMarketplace.Controllers
{
    public class NotificationController : Controller
    {
        private readonly INotificationContentService _notificationService;

        public NotificationController(INotificationContentService notificationService)
        {
            _notificationService = notificationService;
        }

        /// <summary>
        /// Returns the notifications dropdown partial view for the header
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> Index()
        {
            if (!UserSession.CurrentUserId.HasValue)
            {
                return PartialView("_Notifications", new List<MarketMinds.Shared.Models.Notification>());
            }

            int userId = UserSession.CurrentUserId.Value;
            var notifications = await _notificationService.GetNotificationsForUser(userId);

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
            if (!UserSession.CurrentUserId.HasValue)
            {
                return Json(new { count = 0 });
            }

            int userId = UserSession.CurrentUserId.Value;
            var unreadNotifications = await _notificationService.GetUnreadNotificationsForUser(userId);
            var unreadCount = unreadNotifications.Count;
            return Json(new { count = unreadCount });
        }

        /// <summary>
        /// Marks all notifications as read for the user.
        /// </summary>
        /// <returns>
        /// Redirect back to the referring page.
        /// </returns>
        [HttpPost]
        public async Task<IActionResult> MarkAllAsRead()
        {
            try
            {
                if (!UserSession.CurrentUserId.HasValue)
                {
                    TempData["ErrorMessage"] = "User not authenticated";
                    return RedirectToReferrer();
                }

                int userId = UserSession.CurrentUserId.Value;

                // Call the service with the correct parameter (userId)
                await _notificationService.MarkAllAsRead(userId);

                return RedirectToReferrer();
            }
            catch (Exception ex)
            {
                // Log the exception
                Debug.WriteLine($"Error marking notifications as read: {ex.Message}");
                TempData["ErrorMessage"] = "Error marking notifications as read";
                return RedirectToReferrer();
            }
        }

        /// <summary>
        /// Clears all notifications for the current user.
        /// </summary>
        /// <returns>Redirect back to the referring page.</returns>
        [HttpPost]
        public async Task<IActionResult> ClearAll()
        {
            try
            {
                if (!UserSession.CurrentUserId.HasValue)
                {
                    TempData["ErrorMessage"] = "User not authenticated";
                    return RedirectToReferrer();
                }

                int userId = UserSession.CurrentUserId.Value;
                await _notificationService.ClearAllNotifications(userId);

                return RedirectToReferrer();
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error clearing notifications: {ex.Message}");
                TempData["ErrorMessage"] = "Error clearing notifications";
                return RedirectToReferrer();
            }
        }

        /// <summary>
        /// Shows all notifications page
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> All()
        {
            if (!UserSession.CurrentUserId.HasValue)
            {
                return RedirectToAction("Login", "Account");
            }

            int userId = UserSession.CurrentUserId.Value;
            var notifications = await _notificationService.GetNotificationsForUser(userId);

            return View(notifications);
        }

        /// <summary>
        /// Helper method to redirect back to the referring page or Home if no referrer
        /// </summary>
        private IActionResult RedirectToReferrer()
        {
            string referrer = Request.Headers["Referer"].ToString();
            if (!string.IsNullOrEmpty(referrer) && Uri.IsWellFormedUriString(referrer, UriKind.Absolute))
            {
                return Redirect(referrer);
            }
            return RedirectToAction("Index", "Home");
        }
    }
}
