using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace EnquiryInsertToCRM.Models
{
    public class WebHookResponse : IDisposable
    {
        public class WebHookResponseModel
        {
            public string Key { get; set; }
            public string UniqueKey { get; set; } = "";
            public string Value { get; set; }
            public string Type { get; set; }
            public string Name { get; set; }
            public string Name_Unique { get; set; }
        }
        public class WebHookValidationForExistingModel
        {
            public string ID { get; set; }
            public string Key { get; set; }
            public string Type { get; set; }
            public string Email { get; set; }
            public string Email1 { get; set; }
            public string Email2 { get; set; }
            public string Email3 { get; set; }
            public string Phone { get; set; }
            public string Phone1 { get; set; }
            public string Phone2 { get; set; }
            public string Phone3 { get; set; }
            public string Phone4 { get; set; }
            public string FirstName { get; set; }
            public string LastName { get; set; }
            public string CompanyName { get; set; }
        }

        public void Dispose()
        {
            
        }
    }
}