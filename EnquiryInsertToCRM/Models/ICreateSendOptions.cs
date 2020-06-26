using System;

namespace EnquiryInsertToCRM.DataService
{
    public interface ICreateSendOptions
    {
        string BaseUri { get; set; }
        string BaseOAuthUri { get; set; }
        string VersionNumber { get; }
    }
}
