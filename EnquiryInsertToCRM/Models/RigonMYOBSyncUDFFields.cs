using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Web;

namespace EnquiryInsertToCRM.Models
{    
    public class RigonMYOBSyncUDFFields : IDisposable
    {        
        public string UDFName { get; set; } = null;
        public string UDFKey { get; set; } = null;
        public string value { get; set; } = null;

        public void Dispose()
        {

        }
    }
    
}
