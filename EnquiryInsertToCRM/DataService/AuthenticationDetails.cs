using System;
using System.Collections.Generic;
using System.Text;

namespace EnquiryInsertToCRM.DataService
{
    public abstract class AuthenticationDetails
    {
    }

    

    public class ApiKeyAuthenticationDetails : AuthenticationDetails
    {
        public string ApiKey { get; set; }

        public ApiKeyAuthenticationDetails(string apiKey)
        {
            ApiKey = apiKey;
        }
    }

    internal sealed class ClientApiKey : ApiKeyAuthenticationDetails
    {
        public ClientApiKey(string apiKey) 
            : base(apiKey)
        {
        }
    }

    internal sealed class AccountApiKey : ApiKeyAuthenticationDetails, IProvideClientId
    {
        public string ClientId { get; private set; }
        public AccountApiKey(string apiKey, string clientId = null) : base(apiKey)
        {
            ClientId = clientId;
        }
    }

    public interface IProvideClientId
    {
        string ClientId { get; }
    }

    public class BasicAuthAuthenticationDetails : AuthenticationDetails
    {
        public string Username { get; set; }
        public string Password { get; set; }

        public BasicAuthAuthenticationDetails(
            string username,
            string password)
        {
            Username = username;
            Password = password;
        }
    }
}