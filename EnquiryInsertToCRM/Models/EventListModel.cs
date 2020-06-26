using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace EnquiryInsertToCRM.Models
{
    public class EnquiryModel : IDisposable
    {

        public string Id { get; set; }
        public string key { get; set; } = null;
        public string Type { get; set; } = null;
        
        [Required(ErrorMessage = "Please enter first names.")]
        [StringLength(50)]
        [RegularExpression("^([a-zA-Z0-9 ]+)$", ErrorMessage = "Special characters or number are not allowed.")]
        public string FirstName { get; set; }

        public string MiddleName { get; set; }
        public string SalesPerson { get; set; }
        [Required(ErrorMessage = "Please enter last names.")]
        [StringLength(50)]
        [RegularExpression("^([a-zA-Z0-9 ]+)$", ErrorMessage = "Special characters or number are not allowed.")]
        public string LastName { get; set; }


        [Required(ErrorMessage = "Please enter email.")]
        [StringLength(40, ErrorMessage = "Must not be more than 50 char.")]
        [RegularExpression("^[a-zA-Z0-9_\\.-]+@([a-zA-Z0-9-]+\\.)+[a-zA-Z]{2,6}$", ErrorMessage = "Please enter valid email")]
        public string Email { get; set; }
        [Required(ErrorMessage = "Please enter mobile no.")]
        [StringLength(15)]
        [RegularExpression(@"^[0-9]+$", ErrorMessage = "Special characters are not allowed.")]
        public string Mobile { get; set; }
        //[Required(ErrorMessage = "Please Enter Message.")]
        //[StringLength(150)]
        //[RegularExpression(@"^[a-zA-Z0-9 .]+$", ErrorMessage = "Special characters are not allowed.")]
        public string Addresskey { get; set; } = null;
        public string Address { get; set; }

        //[Required(ErrorMessage = "Please enter street address.")]
        [StringLength(100)]
        [RegularExpression(@"^[a-zA-Z0-9 -(),]+$", ErrorMessage = "Some characters are not allowed.")]
        public string AddressLine1 { get; set; } = null;
        [StringLength(50)]
        [RegularExpression(@"^[a-zA-Z0-9 -(),]+$", ErrorMessage = "Some characters are not allowed.")]
        public string AddressLine2 { get; set; } = null;


        //[Required(ErrorMessage = "Please enter suburb.")]
        [StringLength(50)]
        public string City { get; set; } = null;

        //[Required(ErrorMessage = "Please enter state.")]
        [StringLength(50)]
        public string StateProvince { get; set; } = null;

        [StringLength(40)]
        public string Country { get; set; } = null;

        //[Required(ErrorMessage = "Please enter postcode.")]
        [StringLength(10)]
        //[RegularExpression(@"^[0-9]+$", ErrorMessage = "Only number are allowed.")]
        public string ZipCode { get; set; } = null;


        [Required(ErrorMessage = "Please select reference.")]
        public string ReferenceBy { get; set; }
        public string BWSource { get; set; }
        public string strReferenceBy { get; set; } = "";

        [Required(ErrorMessage = "Please select preferred investment level.")]
        public string PreferredInvestmentLevel { get; set; }
        public string InvestmentLevel { get; set; }
        public string strPreferredInvestmentLevel { get; set; } = "";

        [StringLength(250)]
        [RegularExpression(@"^[a-zA-Z0-9 -(),]+$", ErrorMessage = "Some characters are not allowed.")]
        public string Note { get; set; }

        public string Companyname { get; set; }

        public void Dispose()
        {

        }
    }
    public class DropdownModel : IDisposable
    {
        public string Key { get; set; }
        public string Value { get; set; }
        public bool Selected { get; set; } = false;

        public void Dispose()
        {

        }
    }
    public class DropdownCustomerModel : IDisposable
    {
        public string uid { get; set; }
        public string name { get; set; }

        public void Dispose()
        {

        }
    }
    public class NotesModel : IDisposable
    {
        public string Key { get; set; } = null;
        public string Type { get; set; }
        public string Text { get; set; }
        public string RichText { get; set; }
        public string Category { get; set; }
        public string DateTime { get; set; }
        public string Creator { get; set; }
        public string Parent { get; set; } = null;
        public string SecStatus { get; set; }
        public string SecAccess { get; set; }


        public void Dispose()
        {
        }
    }
    public class CoreResponseModel : IDisposable
    {        
        public int countofGetSubmission { get; set; }
        public int countofNewEntry { get; set; }
        public int countofUpdateEntry { get; set; }
        public int countofInvalidForm { get; set; }
        public string ListofInvalidSchema { get; set; }
        public int countofInvalidSchema { get; set; }
        public int countofbadRequest { get; set; }
        public int countofSkipSubmissionIdForm { get; set; }
        public int countofbadrequestfornote { get; set; }
        public string Errorstring { get; set; }

        public void Dispose()
        {
        }
    }        
    public class AbEntryFieldInfo : IDisposable
    {
        public string UniqueKey { get; set; } = "";
        public string Name_Unique { get; set; } = "";
        public string Name { get; set; } = "";
        public string Type { get; set; } = "";
        public string Text { get; set; } = "";
        public dynamic Value { get; set; } = "";
        public int Month { get; set; }
        public int Year { get; set; }
        public string created_at { get; set; }        
        public void Dispose()
        {
        }
    }
    public class AbEntryFieldInfoTemp : IDisposable
    {
        public string UniqueKey { get; set; } = "";
        public string Name_Unique { get; set; } = "";
        public string Name { get; set; } = "";
        public string Type { get; set; } = "";
        public string Text { get; set; } = "";
        public dynamic Value { get; set; } = "";
        public string created_at { get; set; }
        public void Dispose()
        {
        }
    }
    public class AbEntryFieldInfoOrderBy : IDisposable
    {        
        public DateTime created_at { get; set; }
        public string displayCreated_at { get; set; }
        public string clientid { get; set; }
        public string submission_id { get; set; }
        public string Ip { get; set; }
        public string form_title { get; set; }
        public Boolean OutOfDateRange { get; set; }
        public List<JotFormFieldInfo> lstJotFormFieldInfo { get; set; } = new List<JotFormFieldInfo>();
        public List<AbEntryFieldInfo> lstAbEntryFieldInfo { get; set; } = new List<AbEntryFieldInfo>();
        public List<AbEntryContactModel> lstAbEntryContactModelList { get; set; } = new List<AbEntryContactModel>();
        public AbEntryAddressFieldInfo lstAbEntryAddressFieldInfo { get; set; } = new AbEntryAddressFieldInfo();
        public void Dispose()
        {
        }
    }
    public class AbEntryKeyModel : IDisposable
    {
        public string Key { get; set; } = null;
        public string CompanyName { get; set; } = null;
        public string AddressKey { get; set; } = null;
        public string Jotform_Submission_IDs{ get; set; } = null;
        public string MyObID { get; set; } = null;        

        public void Dispose()
        {
        }
    }
    public class AbEntryAddressFieldInfo : IDisposable
    {
        public bool IsAddressAvailable { get; set; } = false;
        public string AddrLine1 { get; set; } = "";
        public string AddrLine2 { get; set; } = "";
        public string State { get; set; } = "";
        public string Country { get; set; } = "";
        public string Postal { get; set; } = "";
        public string City { get; set; } = "";

        public void Dispose()
        {
        }
    }
    public class JotFormFieldInfo : IDisposable
    {
        public string UniqueKey { get; set; }
        public string Name { get; set; }
        public string Type { get; set; }
        public string Text { get; set; }
        public dynamic Value { get; set; }
        public string created_at { get; set; }

        public void Dispose()
        {
        }
    }
    public class AbEntryAlias
    {
        [JsonProperty("alias")]
        public List<string> Alias { get; set; }

        [JsonProperty("type")]
        public List<string> Type { get; set; }

        [JsonProperty("name")]
        public string Name { get; set; }
    }
    public class AbEntryContactModel
    {
        public string UniqueKey { get; set; }
        public string Name { get; set; }
        public string Type { get; set; }
        public string Text { get; set; }
        public dynamic Value { get; set; }
        public string created_at { get; set; }
        public string FirstName { get; set; }
        public string MiddleName { get; set; }        
        public string LastName { get; set; }
        public string Position { get; set; }
        public string Website { get; set; }
        public string Email { get; set; }
        public string Phone { get; set; }
        public string key { get; set; } = null;
        public string Parentkey { get; set; }
        public bool IsAddressAvailable { get; set; } = false;
        public string AddrLine1 { get; set; } = "";
        public string AddrLine2 { get; set; } = "";
        public string State { get; set; } = "";
        public string Country { get; set; } = "";
        public string Postal { get; set; } = "";
        public string City { get; set; } = "";
    }

    public class JotFormFieldInfo_New : IDisposable
    {
        public List<JotFormFieldInfo> lstJotFormFieldInfo { get; set; } = new List<JotFormFieldInfo>();

        public void Dispose()
        {
        }
    }
}