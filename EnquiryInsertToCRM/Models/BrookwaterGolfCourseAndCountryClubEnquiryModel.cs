using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Web;

namespace EnquiryInsertToCRM.Models
{
    public class BrookwaterGolfCourseAndCountryClubEnquiryModel : IDisposable
    {
        //public string key { get; set; } = null;
        //public string Type { get; set; } = null;
        //public string formname { get; set; } = "golf-and-country-club-contact-us";
        //[Required(ErrorMessage = "Please enter full name.")]
        [StringLength(50, ErrorMessage = "The field must be a string with a maximum length of 50 character.")]
        [RegularExpression("^([a-zA-Z0-9 ]+)$", ErrorMessage = "Special characters or number are not allowed.")]
        public string fullname { get; set; }= null;

        //[Required(ErrorMessage = "Please enter email.")]
        [StringLength(40, ErrorMessage = "The field must be a string with a maximum length of 40 character.")]
        [RegularExpression("^[a-zA-Z0-9_\\.-]+@([a-zA-Z0-9-]+\\.)+[a-zA-Z]{2,6}$", ErrorMessage = "Please enter valid email")]
        public string email { get; set; } = null;

        [Required(ErrorMessage = "Please enter mobile no.")]
        [StringLength(15, ErrorMessage = "The field must be a string with a maximum length of 15 character.")]
        [RegularExpression(@"^[0-9]+$", ErrorMessage = "Please enter valid telephone no.")]
        public string telephone { get; set; } = null;

        //[Required(ErrorMessage = "Please enter postcode.")]
        [StringLength(10, ErrorMessage = "The field must be a string with a maximum length of 10 character.")]
        [RegularExpression("^([a-zA-Z0-9 ]+)$", ErrorMessage = "Special characters are not allowed.")]
        public string postcode { get; set; } = null;

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
