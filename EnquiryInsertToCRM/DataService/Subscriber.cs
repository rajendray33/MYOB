using System.Collections.Generic;
using System.Collections.Specialized;

namespace EnquiryInsertToCRM.DataService
{
    public class Subscriber : CreateSendBase
    {
        public Subscriber(AuthenticationDetails auth, string listID)
            : base(auth)
        {
            ListID = listID;
        }

        public string ListID { get; set; }

        
        public string Add(string emailAddress, string name,
            List<SubscriberCustomField> customFields, bool resubscribe,
            ConsentToTrack consentToTrack)
        {
            return Add(emailAddress, name, customFields, resubscribe, true, consentToTrack);
        }

        public string Add(string emailAddress, string name,
            List<SubscriberCustomField> customFields, bool resubscribe,
            bool restartSubscriptionBasedAutoresponders, ConsentToTrack consentToTrack)
        {
            return HttpPost<Dictionary<string, object>, string>(
                string.Format("/subscribers/{0}.json", ListID), null,
                new Dictionary<string, object>()
                {
                    { "EmailAddress", emailAddress },
                    { "Name", name },
                    { "CustomFields", customFields },
                    { "Resubscribe", resubscribe },
                    { "RestartSubscriptionBasedAutoresponders", restartSubscriptionBasedAutoresponders },
                    { "ConsentToTrack", consentToTrack }
                });
        }

    }
}
