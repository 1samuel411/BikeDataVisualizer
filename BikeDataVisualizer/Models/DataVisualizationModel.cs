using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BikeDataVisualizer.Models
{
    public class DataVisualizationModel
    {
        public float averageDistTraveled { get; set; }
        public int regularRiders { get; set; }
        public Controllers.DataController.Entry[] durationsBasedOnMonth { get; set; }
        public Controllers.DataController.Entry[] passholderActivity { get; set; }
        public Controllers.DataController.Entry[] passholderOverages { get; set; }
        public Controllers.DataController.Entry[] startStationTraffic { get; set; }
        public Controllers.DataController.Entry[] endStationTraffic { get; set; }
        public Controllers.DataController.Entry[] hourlyActivity { get; set; }
        public Controllers.DataController.Entry[] stationLocations { get; set; }
    }

}
