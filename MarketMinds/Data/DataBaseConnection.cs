using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace DataAccessLayer
{
    public class DataBaseConnection
    {
        private SqlConnection sqlConnection;
        private readonly string connectionString;
        private readonly IConfiguration configuration;

        public DataBaseConnection(IConfiguration config)
        {
            configuration = config;
            string? localDataSource = configuration["LocalDataSource"];
            string? initialCatalog = configuration["InitialCatalog"];

            connectionString = "Data Source=" + localDataSource + ";" +
                        "Initial Catalog=" + initialCatalog + ";" +
                       "Integrated Security=True;" +
                       "TrustServerCertificate=True";
            try
            {
                sqlConnection = new SqlConnection(connectionString);
            }
            catch (Exception ex)
            {
                throw new Exception($"Error initializing SQL connection: {ex.Message}");
            }
        }

        public SqlConnection GetConnection()
        {
            return this.sqlConnection;
        }

        public void OpenConnection()
        {
            if (this.sqlConnection.State != System.Data.ConnectionState.Open)
            {
                this.sqlConnection.Open();
            }
        }

        public void CloseConnection()
        {
            if (this.sqlConnection.State == System.Data.ConnectionState.Open)
            {
                this.sqlConnection.Close();
            }
        }
    }
}