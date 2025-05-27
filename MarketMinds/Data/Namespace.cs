using System;
using System.Collections.Generic;
using System.Data;
using Microsoft.Data.SqlClient;
using Microsoft.UI.Xaml.Controls;

namespace Marketplace_SE.Data
{
    public class Database
    {
        public static Database Databases { get; set; }
        private SqlConnection connection;
        private string connectionString;
        internal static object Databasee;

        public Database(string connectionString)
        {
            this.connectionString = connectionString;
        }

        public bool Connect()
        {
            try
            {
                connection = new SqlConnection(connectionString);
                connection.Open();
                return true;
            }
            catch
            {
                return false;
            }
        }

        public void Close()
        {
            if (connection != null && connection.State == ConnectionState.Open)
            {
                connection.Close();
            }
        }

        public void Execute(string query, string[] parameters, object[] values)
        {
            SqlCommand command = new SqlCommand(query, connection);
            for (int i = 0; i < parameters.Length; i++)
            {
                command.Parameters.AddWithValue(parameters[i], values[i]);
            }
            command.ExecuteNonQuery();
        }

        public DataTable Get(string query, string[] parameters, object[] values)
        {
            SqlCommand command = new SqlCommand(query, connection);
            for (int i = 0; i < parameters.Length; i++)
            {
                command.Parameters.AddWithValue(parameters[i], values[i]);
            }
            SqlDataAdapter adapter = new SqlDataAdapter(command);
            DataTable dataTable = new DataTable();
            adapter.Fill(dataTable);
            return dataTable;
        }

        public List<T> ConvertToObject<T>(DataTable dataTable)
            where T : new()
        {
            List<T> objects = new List<T>();
            foreach (DataRow row in dataTable.Rows)
            {
                T obj = new T();
                foreach (var prop in typeof(T).GetFields())
                {
                    if (dataTable.Columns.Contains(prop.Name) && row[prop.Name] != DBNull.Value)
                    {
                        prop.SetValue(obj, Convert.ChangeType(row[prop.Name], prop.FieldType));
                    }
                }
                objects.Add(obj);
            }
            return objects;
        }
    }
}

namespace Marketplace_SE.Objects
{
    public class User
    {
        public int Id { get; private set; }
        public string Username { get; private set; }
        public string Password { get; private set; }

        public User(string username, string password)
        {
            this.Username = username;
            this.Password = password;
        }

        public void SetId(int id)
        {
            this.Id = id;
        }
    }
}

// Windows.UI and Microsoft.UI namespaces
namespace Windows.UI.Text
{
    public enum FontStyle
    {
        Normal,
        Italic
    }
}

namespace Microsoft.UI
{
    public class Window
    {
        public void Activate()
        {
        }
        public void Close()
        {
        }
        public class Colors
        {
            public static object Black { get; } = new object();
            public static object Green { get; } = new object();
            public static object Red { get; } = new object();
        }
    }

    namespace Microsoft.UI.Text
    {
        public class FontWeights
        {
            public static object Bold { get; } = new object();
        }
    }

    // Basic user order class
    namespace Marketplace_SE
    {
        public class UserOrder
        {
            public int Id;
            public string Name;
            public string Description;
            public decimal Cost;
            public long Created;
            public string OrderStatus;
            public int SellerId;
            public int BuyerId;
        }
    }

    namespace Microsoft.UI.Xaml.Controls
    {
        public enum Orientation
        {
            Horizontal,
            Vertical
        }

        public class Button
        {
            public object Content { get; set; }
            public event EventHandler Click;
        }

        public class Page
        {
            public Microsoft.UI.Xaml.Controls.Frame Frame { get; protected set; }
            protected virtual void OnNavigatedTo(Microsoft.UI.Xaml.Navigation.NavigationEventArgs e)
            {
                // Base implementation
            }
        }

        public class Frame
        {
            public void Navigate(Type pageType)
            {
                // Navigation implementation
            }
        }

        public class StackPanel
        {
            public Orientation Orientation { get; set; }
            public Microsoft.UI.Xaml.Thickness Margin { get; set; }
            // public System.Collections.Generic.IList<UIElement> Children { get; } = new System.Collections.Generic.List<UIElement>();
        }

        public class TextBlock : UIElement
        {
            public string Text { get; set; }
            public Microsoft.UI.Xaml.TextWrapping TextWrapping { get; set; }
            public Microsoft.UI.Xaml.Thickness Margin { get; set; }
        }

        public class UIElement
        {
            public Microsoft.UI.Xaml.HorizontalAlignment HorizontalAlignment { get; set; }
            public Microsoft.UI.Xaml.VerticalAlignment VerticalAlignment { get; set; }
        }
    }

    namespace Microsoft.UI.Xaml.Navigation
    {
        public class NavigationEventArgs : EventArgs
        {
            public object Parameter { get; set; }
            public Type SourcePageType { get; set; }
        }

        public static class FrameNavigation
        {
            public static void NavigateWithConstructorParameters<T>(Frame frame, object parameter)
            {
                // Implementation
            }
        }
    }

    namespace Microsoft.UI.Xaml
    {
        public class Thickness
        {
            public Thickness(double uniformSize)
            {
            }
            public Thickness(double left, double top, double right, double bottom)
            {
            }
        }

        public enum HorizontalAlignment
        {
            Left,
            Center,
            Right,
            Stretch
        }

        public enum VerticalAlignment
        {
            Top,
            Center,
            Bottom,
            Stretch
        }

        public class RoutedEventArgs : EventArgs
        {
        }
        public enum TextWrapping
        {
            NoWrap,
            Wrap,
            WrapWholeWords
        }
    }

    // Add Windows.Graphics namespace
    namespace Windows.Graphics
    {
        public struct PointInt32
        {
            public int X;
            public int Y;

            public PointInt32(int x, int y)
            {
                X = x;
                Y = y;
            }
        }

        public struct SizeInt32
        {
            public int Width;
            public int Height;

            public SizeInt32(int width, int height)
            {
                Width = width;
                Height = height;
            }
        }
    }

    // Add WinRT namespace
    namespace WinRT.Interop
    {
        public static class WindowNative
        {
            public static IntPtr GetWindowHandle(object window)
            {
                return IntPtr.Zero; // Stub implementation
            }
        }
    }

    // Add Win32Interop and Windowing namespace
    namespace Microsoft.UI
    {
        public static class Win32Interop
        {
            public static WindowId GetWindowIdFromWindow(IntPtr hwnd)
            {
                return new WindowId(); // Stub implementation
            }
        }

        public struct WindowId
        {
            public ulong Value;
        }

        namespace Windowing
        {
            public enum DisplayAreaFallback
            {
                None,
                Nearest
            }

            public class DisplayArea
            {
                public WorkArea WorkArea { get; } = new WorkArea();
                public WindowId Id { get; }

                public static DisplayArea GetFromWindowId(WindowId windowId, DisplayAreaFallback fallback)
                {
                    return new DisplayArea(); // Stub implementation
                }
            }

            public class WorkArea
            {
                public int Width { get; } = 1920;
                public int Height { get; } = 1080;
            }

            public class AppWindow
            {
                public WindowId Id { get; }

                public static AppWindow GetFromWindowId(WindowId windowId)
                {
                    return new AppWindow(); // Stub implementation
                }

                public void Resize(Windows.Graphics.SizeInt32 size)
                {
                    // Stub implementation
                }

                public void Move(Windows.Graphics.PointInt32 point)
                {
                    // Stub implementation
                }
            }
        }
    }

    namespace Marketplace_SE.Service
    {
        public class HelpTicket
        {
            public string TicketID { get; }
            public string UserID { get; }
            public string UserName { get; }
            public string DateAndTime { get; }
            public string Descript { get; }
            public string Closed { get; }

            public HelpTicket(string ticketID_, string userID_, string userName_, string dateHour_, string description_, string closed_)
            {
                TicketID = ticketID_;
                UserID = userID_;
                UserName = userName_;
                DateAndTime = dateHour_;
                Descript = description_;
                Closed = closed_;
            }

            public string ToStringExceptDescription()
            {
                return $"TicketID: {TicketID}, UserID: {UserID}, UserName: {UserName}, DateAndTime: {DateAndTime}, Closed: {Closed}";
            }

            public static HelpTicket FromHelpTicketFromDB(HelpTicketFromDB other)
            {
                return new HelpTicket(other.TicketID.ToString(), other.UserID, other.UserName, other.DateAndTime, other.Descript, other.Closed);
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
}