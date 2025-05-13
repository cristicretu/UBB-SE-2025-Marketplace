using System;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration; // Required for IConfiguration

namespace DataAccessLayer // Assuming this is the correct namespace
{
    public class DataBaseConnection // Renamed from DbConnectionService if necessary
    {
        private readonly string connectionString;
        private SqlConnection? sqlConnection = null; // Nullable SqlConnection

        public DataBaseConnection(IConfiguration configuration) // Inject IConfiguration
        {
            // Read connection details from configuration
            string? localDataSource = configuration["LocalDataSource"];
            string? initialCatalog = configuration["InitialCatalog"];

            // Validate configuration values
            if (string.IsNullOrEmpty(localDataSource) || string.IsNullOrEmpty(initialCatalog))
            {
                throw new InvalidOperationException("Database connection information is missing in configuration (LocalDataSource or InitialCatalog).");
            }

            // Build connection string
            connectionString = $"Data Source={localDataSource};Initial Catalog={initialCatalog};Integrated Security=True;TrustServerCertificate=True";

            // Initialize SqlConnection but don't open it here
            sqlConnection = new SqlConnection(connectionString);
        }

        public SqlConnection GetConnection()
        {
            if (sqlConnection == null)
            {
                // This should ideally not happen if the constructor succeeded
                throw new InvalidOperationException("SqlConnection has not been initialized.");
            }
            return sqlConnection;
        }

        // Method to open the connection if it's not already open
        public void OpenConnection()
        {
            if (sqlConnection != null && sqlConnection.State != System.Data.ConnectionState.Open)
            {
                try
                {
                    sqlConnection.Open();
                }
                catch (Exception ex)
                {
                    // Handle or log the exception appropriately
                    Console.WriteLine($"Error opening database connection: {ex.Message}");
                    throw; // Re-throw the exception to indicate failure
                }
            }
        }

        // Method to close the connection if it's open
        public void CloseConnection()
        {
            if (sqlConnection != null && sqlConnection.State == System.Data.ConnectionState.Open)
            {
                try
                {
                    sqlConnection.Close();
                }
                catch (Exception ex)
                {
                    // Handle or log the exception appropriately
                    Console.WriteLine($"Error closing database connection: {ex.Message}");
                    // Decide if you need to throw here or just log
                }
            }
        }

        // Optional: Implement IDisposable if managing the connection lifetime here
        // public void Dispose()
        // {
        //     sqlConnection?.Dispose();
        // }
    }
}