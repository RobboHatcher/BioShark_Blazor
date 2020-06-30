using System;
using System.Xml;
using System.Xml.Serialization;
using System.IO;

using static BioShark_Blazor.Data.ADC;

namespace BioShark_Blazor.Data {

    public class ScalingVals  {

        // To keep things consistent, we have 5 numbers in the array, but the Temperature value is not used

        private enum Boundaries {Lower, Upper};

        
        [XmlAttribute]
        public double[] ScaleFactors {get;set;} = {0,0,0,0,0};
        
        [XmlAttribute]
        public double[] ZeroOffsets {get;set;} = {0,0,0,0,0};
        
        [XmlAttribute]
        public double[] MassCount = new double[2];
        [XmlAttribute]
        public double[] MassValue = new double[2];
        
        [XmlAttribute]
        public double[] HPHRCount = new double[2];

        [XmlAttribute]
        public double[] HPHRValue = new double[2];

        [XmlAttribute]
        public double[] HPLRCount = new double[2];

        [XmlAttribute]
        public double[] HPLRValue = new double[2];

        [XmlAttribute]
        public double[] RHCount = new double[2];

        [XmlAttribute]
        public double[] RHValue = new double[2];
        


        public void InitializeTest(){
            MassCount = new double[] {83,31498};
            MassValue = new double[] {0,4504}; // Grams

            HPHRCount = new double[] {4,11.4};
            HPHRValue = new double[] {0,464}; // PPM

            HPLRCount = new double[] {4, 11.8};
            HPLRValue = new double[] {0, 4.9}; // PPM

            RHCount = new double[] {0.862261, 3.19703};
            RHValue = new double[] {0, 75.3}; // percentage
        }

        public void ScaleFactorsUpdate() {

            ScaleFactors[(int)ReadingTypes.Mass] = (MassValue[1]  - MassValue[0]) / (MassCount[1] - MassCount[0] - 1);
            ScaleFactors[(int)ReadingTypes.HPHR] = (HPHRValue[1] - HPHRValue[0]) / ((HPHRCount[1] - HPHRCount[0]) * (250 * 6553.6 / 1000));
            ScaleFactors[(int)ReadingTypes.HPLR] = (HPLRValue[1] - HPLRValue[0]) / ((HPLRCount[1] - HPLRCount[0]) * (250 * 6553.6 / 1000));
            ScaleFactors[(int)ReadingTypes.RH] = (RHValue[1] - RHValue[0])/((RHCount[1]-RHCount[0]) * 6553.6);

            ZeroOffsets[(int)ReadingTypes.Mass] = MassCount[0] - MassValue[0] / ScaleFactors[(int)ReadingTypes.Mass];
            ZeroOffsets[(int)ReadingTypes.HPHR] = (HPHRCount[0] * 250 * 6553.6/1000) - (HPHRValue[0]/ScaleFactors[(int)ReadingTypes.HPHR]);
            ZeroOffsets[(int)ReadingTypes.HPLR] = (HPLRCount[0] * 250 * 6553.6/1000) - (HPLRValue[0]/ScaleFactors[(int)ReadingTypes.HPLR]);
            ZeroOffsets[(int)ReadingTypes.RH] = (RHCount[0] * 6553.6) - (RHValue[0] / ScaleFactors[(int)ReadingTypes.RH]);
        }


        public void StoreData()
        {
            XmlSerializer ser = new XmlSerializer(typeof(ScalingVals));
            TextWriter writer = new StreamWriter(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile)+"/BioShark Data/Configuration.xml");
            ser.Serialize(writer, this);
            writer.Close();
        }

        public void FetchData()
        {
            XmlSerializer ser = new XmlSerializer(typeof(ScalingVals));
            try
            {
                FileStream fs = new FileStream(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile)+"/BioShark Data/Configuration.xml", FileMode.OpenOrCreate);
                ScalingVals vals = (ScalingVals)(ser.Deserialize(fs));

                this.MassCount = vals.MassCount;
                this.MassValue = vals.MassValue;
                this.HPHRCount = vals.HPHRCount;
                this.HPHRValue = vals.HPHRValue;
                this.HPLRCount = vals.HPLRCount;
                this.HPLRValue = vals.HPLRValue;
                this.RHCount = vals.RHCount;
                this.RHValue = vals.RHValue;

            }

            catch(Exception ex)
            {
                Console.WriteLine("No initial file found.");
                InitializeTest();
            }
        }
    }
}