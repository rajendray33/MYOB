using System;
using System.Collections.Generic;
using System.Text;

namespace EnquiryInsertToCRM.DataService
{
    public class BasicSubscriber
    {
        public string EmailAddress { get; set; }
        public DateTime Date { get; set; }
        public string State { get; set; }
    }

    public class SubscriberCustomField
    {
        public string Key { get; set; }
        public string Value { get; set; }
        public bool Clear { get; set; }
    }

    public class SubscriberDetail : BasicSubscriber
    {
        public SubscriberDetail()
            : base()
        {
        }

        public SubscriberDetail(
            string emailAddress,
            string name,
            List<SubscriberCustomField> customFields)
        {
            EmailAddress = emailAddress;
            Name = name;
            CustomFields = customFields;
        }

        public string Name { get; set; }
        public List<SubscriberCustomField> CustomFields { get; set; }
        public string ReadsEmailWith { get; set; }
        public ConsentToTrack? ConsentToTrack { get; set; }
    }
}
