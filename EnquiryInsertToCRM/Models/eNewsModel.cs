using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Web;

namespace EnquiryInsertToCRM.Models
{
    public class eNewsModel : IDisposable
    {
        public string key { get; set; } = null;
        public string Type { get; set; } = null;
        public string Addresskey { get; set; } = null;
        [Required(ErrorMessage = "Please enter form name.")]
        [StringLength(60, ErrorMessage = "The field must be a string with a maximum length of 60 character.")]
        public string formname { get; set; } = null;
        public dynamic[] formId { get; set; } = null;

        [StringLength(3, ErrorMessage = "The field must be a string with a maximum length of 3 character.")]
        [RegularExpression("^([a-zA-Z]+)$", ErrorMessage = "Special characters or number are not allowed.")]
        public string gsgeneralenews { get; set; } = null;

        [StringLength(3, ErrorMessage = "The field must be a string with a maximum length of 3 character.")]
        [RegularExpression("^([a-zA-Z]+)$", ErrorMessage = "Special characters or number are not allowed.")]
        public string gsnewsandviews { get; set; } = null;


        [Required(ErrorMessage = "Please enter first name.")]
        [StringLength(50, ErrorMessage = "The field must be a string with a maximum length of 50 character.")]
        [RegularExpression("^([a-zA-Z0-9 ]+)$", ErrorMessage = "Special characters or number are not allowed.")]
        public string firstname { get; set; }= null;

        [Required(ErrorMessage = "Please enter last name.")]
        [StringLength(50, ErrorMessage = "The field must be a string with a maximum length of 50 character.")]
        [RegularExpression("^([a-zA-Z0-9 ]+)$", ErrorMessage = "Special characters or number are not allowed.")]
        public string lastname { get; set; } = null;

        [Required(ErrorMessage = "Please enter email.")]
        [StringLength(40, ErrorMessage = "The field must be a string with a maximum length of 40 character.")]
        [RegularExpression("^[a-zA-Z0-9_\\.-]+@([a-zA-Z0-9-]+\\.)+[a-zA-Z]{2,6}$", ErrorMessage = "Please enter valid email")]
        public string email { get; set; } = null;

        [StringLength(100, ErrorMessage = "The field must be a string with a maximum length of 100 character.")]
        //[Required(ErrorMessage = "Please enter address.")]
        [RegularExpression(@"^[a-zA-Z0-9 -(),]+$", ErrorMessage = "Some characters are not allowed.")]
        public string address { get; set; } = null;

        //[Required(ErrorMessage = "Please enter suburb.")]
        [StringLength(20, ErrorMessage = "The field must be a string with a maximum length of 20 character.")]
        [RegularExpression("^([a-zA-Z0-9 ]+)$", ErrorMessage = "Special characters are not allowed.")]
        public string suburb { get; set; } = null;

        //[Required(ErrorMessage = "Please enter mobile no.")]
        [StringLength(15, ErrorMessage = "The field must be a string with a maximum length of 15 character.")]
        [RegularExpression(@"^[0-9]+$", ErrorMessage = "Please enter valid phone no.")]
        public string phone { get; set; } = null;

        //[Required(ErrorMessage = "Please enter state.")]
        [StringLength(20, ErrorMessage = "The field must be a string with a maximum length of 20 character.")]
        [RegularExpression("^([a-zA-Z0-9 ]+)$", ErrorMessage = "Special characters are not allowed.")]
        public string state { get; set; } = null;

        //[Required(ErrorMessage = "Please enter country.")]        
        [StringLength(20, ErrorMessage = "The field must be a string with a maximum length of 20 character.")]
        [RegularExpression("^([a-zA-Z0-9 ]+)$", ErrorMessage = "Special characters are not allowed.")]
        public string country { get; set; } = null;

        //[Required(ErrorMessage = "Please enter postcode.")]
        [StringLength(10, ErrorMessage = "The field must be a string with a maximum length of 10 character.")]
        [RegularExpression("^([a-zA-Z0-9 ]+)$", ErrorMessage = "Special characters are not allowed.")]
        public string postcode { get; set; } = null;

        [StringLength(250, ErrorMessage = "The field must be a string with a maximum length of 250 character.")]
        [RegularExpression(@"^[a-zA-Z0-9 -(),]+$", ErrorMessage = "Some characters are not allowed.")]
        public string comments { get; set; } = null;

        //[Required(ErrorMessage = "Please select specific information.")]
        [RegularExpression(@"^[a-zA-Z0-9 ,]+$", ErrorMessage = "Some characters are not allowed.")]
        [StringLength(50, ErrorMessage = "The field must be a string with a maximum length of 50 character.")]
        public string interestedin { get; set; } = null;





        [RegularExpression(@"^[a-zA-Z0-9 ,]+$", ErrorMessage = "Some characters are not allowed.")]
        [StringLength(30, ErrorMessage = "The field must be a string with a maximum length of 30 character.")]
        public string interestinleaseorsale { get; set; } = null;

        [StringLength(20, ErrorMessage = "The field must be a string with a maximum length of 20 character.")]
        [RegularExpression("^([a-zA-Z]+)$", ErrorMessage = "Special characters or number are not allowed.")]
        public string interestedintype { get; set; } = null;

        [StringLength(50, ErrorMessage = "The field must be a string with a maximum length of 50 character.")]
        [RegularExpression("^([a-zA-Z0-9 -,]+)$", ErrorMessage = "Special characters are not allowed.")]
        public string howdidyouhearaboutus { get; set; } = null;

        [StringLength(3, ErrorMessage = "The field must be a string with a maximum length of 3 character.")]
        [RegularExpression("^([a-zA-Z]+)$", ErrorMessage = "Special characters or number are not allowed.")]
        public string inspectionrequest { get; set; } = null;

        [StringLength(3, ErrorMessage = "The field must be a string with a maximum length of 3 character.")]
        [RegularExpression("^([a-zA-Z]+)$", ErrorMessage = "Special characters or number are not allowed.")]
        public string informationpackrequest { get; set; } = null;

        [StringLength(3, ErrorMessage = "The field must be a string with a maximum length of 3 character.")]
        [RegularExpression("^([a-zA-Z]+)$", ErrorMessage = "Special characters or number are not allowed.")]
        public string gsbrochurerequest { get; set; } = null;

        [StringLength(50, ErrorMessage = "The field must be a string with a maximum length of 50 character.")]
        [RegularExpression("^([a-zA-Z ]+)$", ErrorMessage = "Special characters or number are not allowed.")]
        public string title { get; set; } = null;

        [StringLength(3, ErrorMessage = "The field must be a string with a maximum length of 3 character.")]
        [RegularExpression("^([a-zA-Z]+)$", ErrorMessage = "Special characters or number are not allowed.")]
        public string gsupdaterequest { get; set; } = null;

        [StringLength(3, ErrorMessage = "The field must be a string with a maximum length of 3 character.")]
        [RegularExpression("^([a-zA-Z]+)$", ErrorMessage = "Special characters or number are not allowed.")]
        public string gsopportunitiesrequest { get; set; } = null;

        [StringLength(3, ErrorMessage = "The field must be a string with a maximum length of 3 character.")]
        [RegularExpression("^([a-zA-Z]+)$", ErrorMessage = "Special characters or number are not allowed.")]
        public string learningcoalitioncommittee { get; set; } = null;

        [StringLength(3, ErrorMessage = "The field must be a string with a maximum length of 3 character.")]
        [RegularExpression("^([a-zA-Z]+)$", ErrorMessage = "Special characters or number are not allowed.")]
        public string gsreceivelatestnews { get; set; } = null;

        //Campaign Monitor
        [StringLength(50, ErrorMessage = "The field must be a string with a maximum length of 50 character.")]
        [RegularExpression("^([a-zA-Z0-9 ]+)$", ErrorMessage = "Special characters or number are not allowed.")]        
        public string fullname { get; set; } = null;

        [Required(ErrorMessage = "Please enter mobile no.")]
        [StringLength(15, ErrorMessage = "The field must be a string with a maximum length of 15 character.")]
        [RegularExpression(@"^[0-9]+$", ErrorMessage = "Please enter valid telephone no.")]
        public string telephone { get; set; } = null;

        [Required(ErrorMessage = "Please enter enquiry.")]
        [StringLength(250, ErrorMessage = "The field must be a string with a maximum length of 250 character.")]
        [RegularExpression(@"^[a-zA-Z0-9 -(),]+$", ErrorMessage = "Some characters are not allowed.")]
        public string enquiry { get; set; } = null;

        //[StringLength(20, ErrorMessage = "The field must be a string with a maximum length of 20 character.")]
        [Required(ErrorMessage = "Please enter enquiry topic.")]
        [RegularExpression("^([a-zA-Z0-9 ,]+)$", ErrorMessage = "Special characters are not allowed.")]
        public string enquirytopic { get; set; } = null;

        //I would like to be contacted by phone
        [StringLength(3, ErrorMessage = "The field must be a string with a maximum length of 3 character.")]
        [RegularExpression("^([a-zA-Z]+)$", ErrorMessage = "Special characters or number are not allowed.")]
        public string wouldliketobecontactedbyphone { get; set; } = null;

        // Golf lesson Enquiry
        [StringLength(3, ErrorMessage = "The field must be a string with a maximum length of 3 character.")]
        [RegularExpression("^([a-zA-Z]+)$", ErrorMessage = "Special characters or number are not allowed.")]
        public string golflessonenquiry { get; set; } = null;


        public void Dispose()
        {

        }
    }
}
