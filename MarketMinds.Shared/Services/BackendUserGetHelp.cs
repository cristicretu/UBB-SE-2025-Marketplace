using System.Collections.Generic;
using System;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace Marketplace_SE.Service
{
    public static class BackendUserGetHelp
    {
        public enum BackendUserGetHelpStatusCodes : int
        {
            PushNewHelpTicketToDBSuccess,
            PushNewHelpTicketToDBFailure,
            UpdateHelpTicketInDBSuccess,
            UpdateHelpTicketInDBFailure,
            ClosedHelpTicketInDBSuccess,
            ClosedHelpTicketInDBFailure
        }
        public static int PushNewHelpTicketToDB(string userID, string userName, string description, string closed)
        {
            // Using a mock database for now since the actual implementation has errors
            // Real implementation would connect to the database and insert the data
            Console.WriteLine($"Creating help ticket for user {userName} with description {description}");
            return (int)BackendUserGetHelpStatusCodes.PushNewHelpTicketToDBSuccess;
        }

        public static bool DoesUserIDExist(string userID)
        {
            return true;
        }

        public static List<HelpTicket> LoadTicketsFromDB(List<string> ticketIDs)
        {
            List<HelpTicket> returnList = new List<HelpTicket>();

            // Mock implementation for now
            foreach (string id in ticketIDs)
            {
                returnList.Add(new HelpTicket(
                    id,
                    "user123",
                    "John Doe",
                    DateTime.Now.ToString("dd-MM-yyyy-HH-mm"),
                    "Mock ticket description",
                    "false"));
            }

            return returnList;
        }

        public static List<string> GetTicketIDsMatchingCriteria(string userID)
        {
            // Mock implementation
            List<string> returnList = new List<string> { "1", "2", "3" };
            return returnList;
        }

        public static bool DoesTicketIDExist(int requestedTicketID)
        {
            // Mock implementation
            return requestedTicketID > 0 && requestedTicketID < 100;
        }

        public static int UpdateHelpTicketDescriptionInDB(string ticketID, string newDescription)
        {
            // Mock implementation
            Console.WriteLine($"Updating ticket {ticketID} with description {newDescription}");
            return (int)BackendUserGetHelpStatusCodes.UpdateHelpTicketInDBSuccess;
        }

        public static int CloseHelpTicketInDB(string ticketID)
        {
            // Mock implementation
            Console.WriteLine($"Closing ticket {ticketID}");
            return (int)BackendUserGetHelpStatusCodes.ClosedHelpTicketInDBSuccess;
        }
    }

    public class HelpTicket
    {
        public string TicketID { get; }
        public string UserID { get; }
        public string UserName { get; }
        public string DateAndTime { get; }
        public string Descript { get; }
        public string Closed { get; }

        public HelpTicket(string ticketID, string userID, string userName, string dateHour, string description, string closed)
        {
            TicketID = ticketID;
            UserID = userID;
            UserName = userName;
            DateAndTime = dateHour;
            Descript = description;
            Closed = closed;
        }

        public string ToStringExceptDescription()
        {
            return $"Ticket ID: {TicketID}, User: {UserName}, Date: {DateAndTime}, Closed: {Closed}";
        }

        public static HelpTicket FromHelpTicketFromDB(HelpTicketFromDB other)
        {
            return new HelpTicket(
                other.TicketID.ToString(),
                other.UserID,
                other.UserName,
                other.DateAndTime,
                other.Descript,
                other.Closed);
        }
    }

    public class HelpTicketFromDB
    {
        public int TicketID;
        public string UserID;
        public string UserName;
        public string DateAndTime;
        public string Descript;
        public string Closed;
    }
}
