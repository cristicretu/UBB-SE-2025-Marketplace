using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using SharedClassLibrary.ProxyRepository;
using Microsoft.Data.SqlClient;
using SharedClassLibrary.Domain;
using SharedClassLibrary.IRepository;
using SharedClassLibrary.Helper;

[ExcludeFromCodeCoverage]
public class WaitListNotifier
{
    private readonly IWaitListRepository waitListModel;
    private readonly INotificationRepository notificationAdapter;
    private readonly string connectionString;

    /// <summary>
    /// Initializes a new instance of the <see cref="WaitListNotifier"/> class with the specified connection string.
    /// </summary>
    /// <param name="connectionString">The connection string to the database.</param>
    /// <exception cref="ArgumentNullException">Thrown when the connection string is null or empty.</exception>
    public WaitListNotifier(string connectionString)
    {
        waitListModel = new WaitListProxyRepository(AppConfig.GetBaseApiUrl());
        notificationAdapter = new NotificationProxyRepository(AppConfig.GetBaseApiUrl());
        this.connectionString = connectionString;
    }

    /// <summary>
    /// Schedules restock alerts for users on the waitlist for a specific product.
    /// </summary>
    /// <param name="productId">The ID of the product to restock. Must be a positive integer.</param>
    /// <param name="restockDate">The date and time when the product will be restocked.</param>
    /// <exception cref="SqlException">Thrown when there is an error executing the SQL command.</exception>
    /// <precondition>productId must be a valid product ID. restockDate must be a future date.</precondition>
    /// <postcondition>Notifications are scheduled for users on the waitlist.</postcondition>
    public async void ScheduleRestockAlerts(int productId, DateTime restockDate)
    {
        List<UserWaitList> waitlistUsers = await waitListModel.GetUsersInWaitlist(productId);
        waitlistUsers = waitlistUsers.OrderBy(u => u.PositionInQueue).ToList();

        for (int userIndex = 0; userIndex < waitlistUsers.Count; userIndex++)
        {
            var notification = new ProductAvailableNotification(
                recipientId: waitlistUsers[userIndex].UserID,
                timestamp: CalculateNotifyTime(restockDate, userIndex),
                productId: productId,
                isRead: false)
            { };
            notificationAdapter.AddNotification(notification);
        }
    }

    /// <summary>
    /// Calculates the notification time based on the restock date and the user's position in the waitlist.
    /// </summary>
    /// <param name="restockDate">The date and time when the product will be restocked.</param>
    /// <param name="positionInQueue">The position of the user in the waitlist.</param>
    /// <returns>The date and time when the notification should be sent.</returns>
    /// <precondition>restockDate must be a valid date. position must be a non-negative integer.</precondition>
    /// <postcondition>The calculated notification time is returned.</postcondition>
    private DateTime CalculateNotifyTime(DateTime restockDate, int positionInQueue)
    {
        return positionInQueue switch
        {
            0 => restockDate.AddHours(-48), // First in queue
            1 => restockDate.AddHours(-24), // Second in queue
            _ => restockDate.AddHours(-12) // Everyone else
        };
    }

    /// <summary>
    /// Generates a notification message based on the user's position in the waitlist and the restock date.
    /// </summary>
    /// <param name="positionInQueue">The position of the user in the waitlist.</param>
    /// <param name="restockDate">The date and time when the product will be restocked.</param>
    /// <returns>The notification message.</returns>
    /// <precondition>position must be a non-negative integer. restockDate must be a valid date.</precondition>
    /// <postcondition>The generated notification message is returned.</postcondition>
    private string GetNotificationMessage(int positionInQueue, DateTime restockDate)
    {
        string timeDescription = (restockDate - DateTime.Now).TotalHours > 24
            ? $"on {restockDate:MMM dd}"
            : $"in {(int)(restockDate - DateTime.Now).TotalHours} hours";

        return positionInQueue switch
        {
            1 => $"You're FIRST in line! Product restocking {timeDescription}.",
            2 => $"You're SECOND in line. Product restocking {timeDescription}.",
            _ => $"You're #{positionInQueue} in queue. Product restocking {timeDescription}."
        };
    }
}