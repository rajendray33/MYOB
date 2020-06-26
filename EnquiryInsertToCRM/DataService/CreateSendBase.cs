﻿using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Text;
using System.Web;

namespace EnquiryInsertToCRM.DataService
{
    public abstract class CreateSendBase
    {
        public AuthenticationDetails AuthDetails { get; set; }
        private readonly ICreateSendOptions options;

        public CreateSendBase(AuthenticationDetails auth, ICreateSendOptions options = null)
        {
            this.options = options ?? new CreateSendOptionsWrapper();

            Authenticate(auth);
        }

        public void Authenticate(AuthenticationDetails auth)
        {
            AuthDetails = auth;
        }
        
        public U HttpGet<U>(string path, NameValueCollection queryArguments)
        {
            return HttpGet<U, ErrorResult>(path, queryArguments);
        }

        public U HttpGet<U, EX>(string path, NameValueCollection queryArguments)
            where EX : ErrorResult
        {
            return HttpHelper.Get<U, EX>(AuthDetails, options.BaseUri, path, queryArguments);
        }

        public U HttpPost<T, U>(string path, NameValueCollection queryArguments, T payload)
            where T : class
        {
            return HttpPost<T, U, ErrorResult>(path, queryArguments, payload);
        }

        public U HttpPost<T, U, EX>(string path, NameValueCollection queryArguments, T payload)
            where T : class
            where EX : ErrorResult
        {
            return HttpHelper.Post<T, U, EX>(AuthDetails, path, queryArguments, payload, options.BaseUri, HttpHelper.APPLICATION_JSON_CONTENT_TYPE);
        }

        public U HttpPut<T, U>(string path, NameValueCollection queryArguments, T payload) where T : class
        {
            return HttpHelper.Put<T, U>(AuthDetails, path, queryArguments, payload, options.BaseUri, HttpHelper.APPLICATION_JSON_CONTENT_TYPE);
        }
    }
}
