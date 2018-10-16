using BikeDataVisualizer.Models;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Threading.Tasks;

namespace BikeDataVisualizer.Controllers
{
    public class DataController
    {

        public static string[] MONTHS = new string[] { "", "Jan", "Feb", "Mar", "Apr", "May", "Jun", "Jul", "Aug", "Sept", "Oct", "Nov", "Dec" };
        public static string[] TRIPCATS = new string[] { "Round-Trip", "One Way" };
        public static string[] PASSHOLDERS = new string[] { "Walk-up", "Flex-Pass", "Monthly-Pass", "Staff Annual" };
        public static string[] HOURS = new string[] { "12 am", "1 am", "2 am", "3 am", "4 am", "5 am", "6 am", "7 am", "8 am", "9 am", "10 am", "11 am", "12 pm", "1 pm", "2 pm", "3 pm", "4 pm", "5 pm", "6 pm", "7 pm", "8 pm", "9 pm", "10 pm", "11 pm" };
        public static SqlCommand cmd;
        public static SqlConnection conn;
        public static DataVisualizationModel cachedModel;

        public class Entry
        {
            public object[] data { get; set; }
        }

        public static DataVisualizationModel GetData()
        {
            if(cachedModel != null)
            {
                return cachedModel;
            }
            DataVisualizationModel model = new DataVisualizationModel();
            if (conn == null || conn.State != System.Data.ConnectionState.Open)
            {
                conn = DataAccessLayer.DataAccessLayer.OpenConnection();
                cmd = new SqlCommand("", conn);
            }
            model.durationsBasedOnMonth = GetDurationsOnMonth(conn, cmd).ToArray();
            model.passholderActivity = GetPassholderActivity(conn, cmd).ToArray();
            model.passholderOverages = GetPassholderProfit(conn, cmd).ToArray();
            model.endStationTraffic = GetStationEndActivity(conn, cmd).ToArray();
            model.startStationTraffic = GetStationStartActivity(conn, cmd).ToArray();
            model.averageDistTraveled = GetAverageDist(conn, cmd);
            model.regularRiders = GetRegulars(conn, cmd);
            model.hourlyActivity = GetHourlyActivity(conn, cmd).ToArray();
            model.stationLocations = GetLocationData(conn, cmd).ToArray();
            cachedModel = model;
            return model;
        }


        public static List<Entry> GetDurationsOnMonth(SqlConnection conn, SqlCommand cmd)
        {
            /* Get Durations based on month */
            List<Entry> entries = new List<Entry>();
            string getMonths = "SELECT AVG(duration), MONTH(StartTime) as O FROM BikeData GROUP BY  MONTH(StartTime)";
            cmd.CommandText = getMonths;
            using (SqlDataReader reader = cmd.ExecuteReader())
            {
                while (reader.Read())
                {
                    entries.Add(new Entry() { data = new object[] { MONTHS[reader.GetInt32(1)], reader.GetInt32(0) } });
                }
            }
            return entries;
        }

        public static List<Entry> GetPassholderActivity(SqlConnection conn, SqlCommand cmd)
        {
            /* Get Passholder activity */
            List<Entry> entries = new List<Entry>();
            string getActivity = "SELECT COUNT(Id), PassholderType FROM BikeData GROUP BY  PassholderType";
            cmd.CommandText = getActivity;
            using (SqlDataReader reader = cmd.ExecuteReader())
            {
                while (reader.Read())
                {
                    string pass = PASSHOLDERS[reader.GetInt32(1)];
                    entries.Add(new Entry() { data = new object[] { pass, reader.GetInt32(0) } });
                }
            }
            return entries;
        }

        public static List<Entry> GetPassholderProfit(SqlConnection conn, SqlCommand cmd)
        {
            /* Get Passholder profit from overages */
            List<Entry> entries = new List<Entry>();
            string getProfit = "SELECT SUM(Duration / 1800), PassholderType FROM BikeData WHERE Duration > 1800 AND PassholderType <> 3  GROUP BY  PassholderType";
            cmd.CommandText = getProfit;
            using (SqlDataReader reader = cmd.ExecuteReader())
            {
                while (reader.Read())
                {
                    int passIndex = reader.GetInt32(1);
                    string pass = PASSHOLDERS[passIndex];
                    float profit = reader.GetInt32(0);
                    if (passIndex == 0)
                        profit *= 3.50f;
                    else if (passIndex == 1)
                        profit *= 1.75f;
                    else if (passIndex == 2)
                        profit *= 1.75f;
                    entries.Add(new Entry() { data = new object[] { pass, profit } });
                }
            }
            return entries;
        }

        public static List<Entry> GetStationEndActivity(SqlConnection conn, SqlCommand cmd)
        {
            List<Entry> entries = new List<Entry>();
            string getStationActivity = "SELECT TOP 5 endStationId, count(EndStationId) AS visits FROM BikeData GROUP BY endStationId ORDER BY visits DESC";
            cmd.CommandText = getStationActivity;
            using (SqlDataReader reader = cmd.ExecuteReader())
            {
                while (reader.Read())
                {
                    entries.Add(new Entry() { data = new object[] { reader.GetInt32(0).ToString(), reader.GetInt32(1) } });
                }
            }
            return entries;
        }

        public static List<Entry> GetStationStartActivity(SqlConnection conn, SqlCommand cmd)
        {
            List<Entry> entries = new List<Entry>();
            string getStationActivity = "SELECT TOP 5 startingStationId, count(StartingStationId) AS visits FROM BikeData GROUP BY startingStationId ORDER BY visits DESC";
            cmd.CommandText = getStationActivity;
            using (SqlDataReader reader = cmd.ExecuteReader())
            {
                while (reader.Read())
                {
                    entries.Add(new Entry() { data = new object[] { reader.GetInt32(0).ToString(), reader.GetInt32(1) } });
                }
            }
            return entries;
        }

        public static float GetAverageDist(SqlConnection conn, SqlCommand cmd)
        {
            /* Get Average Distance Travelled */
            string getAvgDist = "SELECT AVG(distanceTraveled) FROM BikeData";
            cmd.CommandText = getAvgDist;
            using (SqlDataReader reader = cmd.ExecuteReader())
            {
                while (reader.Read())
                {
                    return (float)reader.GetDouble(0);
                }
            }
            return 0;
        }

        public static int GetRegulars(SqlConnection conn, SqlCommand cmd)
        {
            /* Get Regular Riders Based on a 30 minute start time consistancy and same start location*/
            string getRegularStops = "SELECT COUNT(*) FROM BikeData WHERE TripRouteCategory = 1 AND PassholderType > 0";
            cmd.CommandText = getRegularStops;
            using (SqlDataReader reader = cmd.ExecuteReader())
            {
                while (reader.Read())
                {
                    return reader.GetInt32(0);
                }
            }

            return 0;
        }

        public static List<Entry> GetHourlyActivity(SqlConnection conn, SqlCommand cmd)
        {
            /* Get Hourly Activity */
            List<Entry> entries = new List<Entry>();
            string getHourly = "SELECT DATEPART(HOUR,StartTime) AS hr, COUNT(1) AS total FROM BikeData GROUP BY DATEPART(HOUR,StartTime) ORDER BY hr";
            cmd.CommandText = getHourly;
            using (SqlDataReader reader = cmd.ExecuteReader())
            {
                while (reader.Read())
                {
                    string hour = HOURS[reader.GetInt32(0)];
                    int activity = reader.GetInt32(1);
                    entries.Add(new Entry() { data = new object[] { hour, activity } });
                }
            }
            return entries;
        }

        public static List<Entry> GetLocationData(SqlConnection conn, SqlCommand cmd)
        {
            // Get Location Data
            List<Entry> entries = new List<Entry>();
            string getLocation = "Select * from (SELECT StartingStationId, StartLat, StartLong, row_number() OVER(PARTITION BY StartingStationId ORDER BY StartingStationId desc) AS [rn] FROM BikeData WHERE StartingStationId <> 0 AND StartLat <> 0 AND StartLong <> 0) cte WHERE [rn] = 1";
            cmd.CommandText = getLocation;
            using (SqlDataReader reader = cmd.ExecuteReader())
            {
                while (reader.Read())
                {
                    string stationId = "Station " + reader.GetInt32(0).ToString();
                    double lat = reader.GetDouble(1);
                    double lng = reader.GetDouble(2);
                    entries.Add(new Entry() { data = new object[] { lat, lng, stationId } });
                }
            }
            return entries;
        }

    }
}
