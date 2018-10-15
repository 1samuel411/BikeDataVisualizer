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

        public class Entry
        {
            public object[] data { get; set; }
        }

        public static DataVisualizationModel GetData()
        {
            DataVisualizationModel model = new DataVisualizationModel();
            List<Entry> entries = new List<Entry>();

            SqlConnection conn = DataAccessLayer.DataAccessLayer.OpenConnection();

            /* Get Durations based on month */
            string getMonths = "SELECT AVG(duration), MONTH(StartTime) as O FROM BikeData GROUP BY  MONTH(StartTime)";
            using (SqlCommand cmd = new SqlCommand(getMonths, conn))
            {
                using (SqlDataReader reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        entries.Add(new Entry() { data = new object[] { MONTHS[reader.GetInt32(1)], reader.GetInt32(0) } });
                    }
                    model.durationsBasedOnMonth = entries.ToArray();
                }
            }

            /* Get Passholder activity */
            entries.Clear();
            string getActivity = "SELECT COUNT(Id), PassholderType FROM BikeData GROUP BY  PassholderType";
            using (SqlCommand cmd = new SqlCommand(getActivity, conn))
            {
                using (SqlDataReader reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        string pass = PASSHOLDERS[reader.GetInt32(1)];
                        entries.Add(new Entry() { data = new object[] { pass, reader.GetInt32(0) } });
                    }
                    model.passholderActivity = entries.ToArray();
                }
            }

            /* Get Passholder profit from overages */
            entries.Clear();
            string getProfit = "SELECT SUM(Duration / 1800), PassholderType FROM BikeData WHERE Duration > 1800 AND PassholderType <> 3  GROUP BY  PassholderType";
            using (SqlCommand cmd = new SqlCommand(getProfit, conn))
            {
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
                    model.passholderOverages = entries.ToArray();
                }
            }

            /* Get activity of stations */
            entries.Clear();
            string getStationActivity = "SELECT TOP 5 startingStationId, count(StartingStationId) AS visits FROM BikeData GROUP BY startingStationId ORDER BY visits DESC";
            using (SqlCommand cmd = new SqlCommand(getStationActivity, conn))
            {
                using (SqlDataReader reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        entries.Add(new Entry() { data = new object[] { reader.GetInt32(0).ToString(), reader.GetInt32(1) } });
                    }
                    model.startStationTraffic = entries.ToArray();
                }
            }
            entries.Clear();
            getStationActivity = "SELECT TOP 5 endStationId, count(EndStationId) AS visits FROM BikeData GROUP BY endStationId ORDER BY visits DESC";
            using (SqlCommand cmd = new SqlCommand(getStationActivity, conn))
            {
                using (SqlDataReader reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        entries.Add(new Entry() { data = new object[] { reader.GetInt32(0).ToString(), reader.GetInt32(1) } });
                    }
                    model.endStationTraffic = entries.ToArray();
                }
            }

            /* Get Average Distance Travelled */
            entries.Clear();
            string getAvgDist = "SELECT AVG(distanceTraveled) FROM BikeData";
            using (SqlCommand cmd = new SqlCommand(getAvgDist, conn))
            {
                using (SqlDataReader reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        model.averageDistTraveled = (float)reader.GetDouble(0);
                    }
                }
            }

            /* Get Regular Riders Based on a 30 minute start time consistancy and same start location*/
            entries.Clear();
            string getRegularStops = "SELECT COUNT(*) FROM BikeData WHERE TripRouteCategory = 1 AND PassholderType > 0";
            using (SqlCommand cmd = new SqlCommand(getRegularStops, conn))
            {
                using (SqlDataReader reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        model.regularRiders = reader.GetInt32(0);
                    }
                }
            }

            /* Get Hourly Activity */
            entries.Clear();
            string getHourly = "SELECT DATEPART(HOUR,StartTime) AS hr, COUNT(1) AS total FROM BikeData GROUP BY DATEPART(HOUR,StartTime) ORDER BY hr";
            using (SqlCommand cmd = new SqlCommand(getHourly, conn))
            {
                using (SqlDataReader reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        string hour = HOURS[reader.GetInt32(0)];
                        int activity = reader.GetInt32(1);
                        entries.Add(new Entry() { data = new object[] { hour, activity } });
                    }

                    model.hourlyActivity = entries.ToArray();
                }
            }

            // Get Location Data
            entries.Clear();
            string getLocation = "Select * from (SELECT StartingStationId, StartLat, StartLong, row_number() OVER(PARTITION BY StartingStationId ORDER BY StartingStationId desc) AS [rn] FROM BikeData WHERE StartingStationId <> 0 AND StartLat <> 0 AND StartLong <> 0) cte WHERE [rn] = 1";
            using (SqlCommand cmd = new SqlCommand(getLocation, conn))
            {
                using (SqlDataReader reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        string stationId = "Station " + reader.GetInt32(0).ToString();
                        double lat = reader.GetDouble(1);
                        double lng = reader.GetDouble(2);
                        entries.Add(new Entry() { data = new object[] { lat, lng, stationId } });
                    }

                    model.stationLocations = entries.ToArray();
                }
            }
            conn.Close();
            return model;
        }

    }
}
