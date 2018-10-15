using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;

namespace BikeDataVisualizer.DataAccessLayer
{
    public class DataAccessLayer
    {
        public const string CONNECTIONSTRING = "Server=tcp:bikeshareviusliaztion.database.windows.net,1433;Initial Catalog=BikeShareVisualization;Persist Security Info=False;User ID=BikeSharing;Password=GYFzrr5X;MultipleActiveResultSets=False;Encrypt=True;TrustServerCertificate=False;Connection Timeout=30;"; // Connection string name from App.config

        public static SqlConnection OpenConnection()
        {
            SqlConnection connection = new SqlConnection();
            connection.ConnectionString = CONNECTIONSTRING;
            connection.Open();
            return connection;
        }
    }
}
