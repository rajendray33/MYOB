using System;
using System.Collections.Generic;
using System.Configuration;
using System.Text;
using System.Web;
using MYOB.AccountRight.SDK;
using MYOB.AccountRight.SDK.Contracts;

namespace EnquiryInsertToCRM.Models
{
    public static class SessionManager
    {
        private const string iOAuthKeyService = "MyOAuthKeyService";
        private const string iApiConfiguration = "MyConfiguration";
        public static IOAuthKeyService MyOAuthKeyService
        {
            get
            {
                if (HttpContext.Current.Session[iOAuthKeyService] != null)
                {
                    return (IOAuthKeyService)HttpContext.Current.Session[iOAuthKeyService];
                }
                else
                {
                    return null;
                }
            }
            set
            {
                HttpContext.Current.Session[iOAuthKeyService] = value;
            }
        }
        public static IApiConfiguration MyConfiguration
        {

            get
            {
                if (HttpContext.Current.Session[iApiConfiguration] != null)
                {
                    
                    return (IApiConfiguration)HttpContext.Current.Session[iApiConfiguration];
                }
                else
                {
                  
                    return null;
                }
            }
            set
            {
                HttpContext.Current.Session[iApiConfiguration] = value;
            }
        }
    }
}
