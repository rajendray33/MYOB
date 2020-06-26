using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace EnquiryInsertToCRM.Models
{
    public class FormSubmissionsModel : IDisposable
    {        
        public int id { get; set; }
        [Required(ErrorMessage = "Please select form.")]
        public string form_id { get; set; }

        public string dateRange { get; set; }

        public void Dispose()
        {

        }
    }
}