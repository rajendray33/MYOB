using System;
using System.Collections.Generic;
using System.Collections.Specialized;

namespace EnquiryInsertToCRM.DataService
{
    public class Client : CreateSendBase
    {
        public string ClientID { get; set; }

        public Client(AuthenticationDetails auth, string clientID)
            : base(auth)
        {
            ClientID = clientID;
        }      

        public IEnumerable<BasicList> Lists()
        {
            return HttpGet<BasicList[]>(string.Format("/clients/{0}/lists.json", ClientID), null);
        }      
    }
}
