using System;
using System.Collections.Generic;
using System.Text;

namespace EnquiryInsertToCRM.DataService
{
    public class FinancialYearModel:IDisposable
    {
        public int year { get; set; }
        public double value { get; set; }
        public string key { get; set; }
        public DateTime fyStartDate { get; set; }
        public DateTime fyEndDate { get; set; }
        public bool IsCurrentFY { get; set; } = false;

        public void Dispose()
        {
            
        }
    }
}
