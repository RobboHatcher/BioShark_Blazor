using System;
using System.Xml.Serialization;

namespace BioShark_Blazor.Data{

    public class CycleData{

        public DateTime StartDate {get;set;}
        public TimeSpan RunTime {get;set;}

        public CycleData(){
            TryRetrieveFile(this);
            
            StartDate = DateTime.Now;
            RunTime = new TimeSpan(0,0,0);
        }


        private void TryRetrieveFile(CycleData updateData)
        {

        }

    }


}