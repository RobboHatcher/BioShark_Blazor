using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BioShark_Blazor.Data
{
    public class SummaryTracker
    {

        private const string DateFormat = @"dd-mm-yy";
        private const string TimeFormat = @"HH-MM-SS";

        public string machineID { get; set; } = "MACHINEID";
        public string cycleID { get; set; } = "CYCLEID";
        public DateTime cycleDate { get; set; } = DateTime.Now;
        public DateTime startTime { get; set; } = DateTime.Now;
        public string userID { get; set; } = "USERID";
        public string roomID { get; set; } = "ROOMID";
        public double roomVolume { get; set; } = 0;
        public double begMass { get; set; } = 0;
        public double endMass { get; set; } = 0;
        public double massChange { get; set; } = 0;
        public double massDisch { get; set; } = 0;
        public TimeSpan dischTime { get; set; } = new TimeSpan(0, 0, 0);
        public TimeSpan cycleLen { get; set; } = new TimeSpan(0, 0, 0);
        public double AvgMistRate { get; set; } = 0;
        public double peakPPM { get; set; } = 0;
        public int peakPPMDataPoint { get; set; } = 0;
        public DateTime peakPPMTime { get; set; } = DateTime.Now;
        public double begRH { get; set; } = 0;
        public double peakRH { get; set; } = 0;
        public int peakRHDataPoint { get; set; } = 0;
        public DateTime peakRHTime { get; set; } = DateTime.Now;
        public double endRH { get; set; } = 0;
        public double begTemp { get; set; } = 0;
        public double peakTemp { get; set; } = 0;
        public int peakTempDataPoint { get; set; } = 0;
        public DateTime peakTempTime { get; set; } = DateTime.Now;
        public double endTemp { get; set; } = 0;
        public DateTime endTime { get; set; } = DateTime.Now;

        
        public string SummaryString()
        {
            string dataLine = "";
            dataLine += machineID.ToString() + ',' + cycleID.ToString() + ',' + cycleDate.ToString(DateFormat) + ',' + startTime.ToString(TimeFormat) + ','
                + userID + ',' + roomID + ',' + roomVolume.ToString() + ',' + begMass.ToString() + ',' + endMass.ToString() + ',' + massChange.ToString()
                + ',' + massDisch.ToString() + ',' + dischTime.ToString(TimeFormat) + ',' + cycleLen.ToString(TimeFormat) + ',' + AvgMistRate.ToString()
                + ',' + peakPPM.ToString() + ',' + peakPPMDataPoint.ToString() + ',' + peakPPMTime.ToString(TimeFormat) + ',' + begRH.ToString() + ','
                + peakRH.ToString() + ',' + peakRHDataPoint.ToString() + ',' + peakRHTime.ToString(TimeFormat) + ',' + endRH.ToString() + ',' + begTemp.ToString()
                + ',' + peakTemp.ToString() + ',' + peakTempDataPoint.ToString() + ',' + peakTempTime.ToString(TimeFormat) + ',' + endTemp.ToString() + ',' + endTime.ToString(TimeFormat);
            return dataLine;
        }

    }


    
}
