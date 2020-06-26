using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Web;

namespace EnquiryInsertToCRM.Models
{
    public class UnionMemberModel : IDisposable
    {
        public string key { get; set; } = null;        
        public string ClientId { get; set; } = null;
        public string Type { get; set; } = null;
        public bool IsInvalidClientId { get; set; } = false;
        public string Addresskey { get; set; } = null;

        [Required(ErrorMessage = "Please enter first name.")]
        [StringLength(100, ErrorMessage = "First name must be 100 characters or less in length.")]
        [RegularExpression("^([a-zA-Z0-9 ]+)$", ErrorMessage = "Special special characters or number are not allowed.")]
        public string firstname { get; set; }= null;

        [StringLength(100, ErrorMessage = "Last name must be 100 characters or less in length.")]        
        [RegularExpression("^([a-zA-Z0-9 ]+)$", ErrorMessage = "Special special characters or number are not allowed.")]
        public string lastname { get; set; } = null;

        [Required(ErrorMessage = "Please enter email.")]
        [StringLength(40, ErrorMessage = "E-mail name must be 40 characters or less in length.")]
        [RegularExpression("^[a-zA-Z0-9_\\.-]+@([a-zA-Z0-9-]+\\.)+[a-zA-Z]{2,6}$", ErrorMessage = "Please enter valid email")]
        public string email { get; set; } = null;

        [StringLength(200, ErrorMessage = "Address line must be 200 characters or less in length.")]
        //[Required(ErrorMessage = "Please enter address.")]
        [RegularExpression(@"^[a-zA-Z0-9 -(),]+$", ErrorMessage = "Some special characters are not allowed.")]
        public string address { get; set; } = null;

        //[Required(ErrorMessage = "Please enter suburb.")]
        [StringLength(20, ErrorMessage = "Suburb must be 20 characters or less in length.")]
        [RegularExpression("^([a-zA-Z0-9 ]+)$", ErrorMessage = "Special characters are not allowed.")]
        public string suburb { get; set; } = null;

        //[Required(ErrorMessage = "Please enter mobile no.")]
        [StringLength(15, ErrorMessage = "Phone must be 15 characters or less in length.")]
        [RegularExpression(@"^[0-9]+$", ErrorMessage = "Please enter valid phone.")]
        public string phone { get; set; } = null;

        //[Required(ErrorMessage = "Please enter mobile no.")]
        [StringLength(15, ErrorMessage = "Mobile must be 15 characters or less in length.")]
        [RegularExpression(@"^[0-9]+$", ErrorMessage = "Please enter valid mobile.")]
        public string mobile { get; set; } = null;

        //[Required(ErrorMessage = "Please enter home no.")]
        [StringLength(15, ErrorMessage = "Home must be 15 characters or less in length.")]
        [RegularExpression(@"^[0-9]+$", ErrorMessage = "Please enter valid home.")]
        public string home { get; set; } = null;

        //[Required(ErrorMessage = "Please enter post code.")]
        [StringLength(15, ErrorMessage = "Post code must be 15 characters or less in length.")]
        //[RegularExpression(@"^[0-9]+$", ErrorMessage = "Only number are allowed.")]
        public string ZipCode { get; set; } = null;

        //[Required(ErrorMessage = "Please enter work no.")]
        [StringLength(15, ErrorMessage = "Work must be 15 characters or less in length.")]
        [RegularExpression(@"^[0-9]+$", ErrorMessage = "Please enter valid work.")]
        public string work { get; set; } = null;

        //[Required(ErrorMessage = "Please enter state.")]
        [StringLength(20, ErrorMessage = "State must be 20 characters or less in length.")]
        [RegularExpression("^([a-zA-Z0-9 ]+)$", ErrorMessage = "Special special characters are not allowed.")]
        public string state { get; set; } = null;

        //[Required(ErrorMessage = "Please enter country.")]        
        [StringLength(20, ErrorMessage = "Country must be 20 characters or less in length.")]
        [RegularExpression("^([a-zA-Z0-9 ]+)$", ErrorMessage = "Special special characters are not allowed.")]
        public string country { get; set; } = null;

        public string UnionName { get; set; } = null;
        [Required(ErrorMessage = "Please select union name.")]                
        public string UnionID { get; set; } = null;
        public List<DropdownModel> lstUnionID = new List<DropdownModel>();
        //[Required(ErrorMessage = "Please enter country.")]        
        public bool? IsUnionAbovecorrect { get; set; } = null;


        [StringLength(20, ErrorMessage = "The field must be a string with a maximum length of 20 character.")]
        [RegularExpression("^([a-zA-Z0-9 ]+)$", ErrorMessage = "Special special characters are not allowed.")]
        public string UnionAbovecorrect_Othertext { get; set; } = null;

        //[Required(ErrorMessage = "Please enter union member id.")]        
        [StringLength(20, ErrorMessage = "Union member id must be 20 characters or less in length.")]
        [RegularExpression("^([a-zA-Z0-9 ]+)$", ErrorMessage = "Special special characters are not allowed.")]
        public string UnionMemberID { get; set; } = null;
public string UsWebPwd { get; set; } = null;
        public bool? IsReceiveGeneralOffersEmail { get; set; } = null;

        //[Required(ErrorMessage = "Please enter direct offers.")]
        public string DirectOffers { get; set; }        
        public string strDirectOffers { get; set; } = "";
        public string strDirectOffersName { get; set; } = "";

        [StringLength(1000, ErrorMessage = "Comments must be 1000 characters or less in length.")]
        [RegularExpression(@"^[a-zA-Z0-9 -(),]+$", ErrorMessage = "Some special characters are not allowed.")]
        public string comments { get; set; } = null;
        public List<DropdownModel> lstDirectOffers = new List<DropdownModel>();
        public void Dispose()
        {

        }
    }
}
