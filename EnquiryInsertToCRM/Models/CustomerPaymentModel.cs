using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace EnquiryInsertToCRM.Models
{
    public class CustomerPaymentModel : IDisposable
    {
        public Guid Uid { get; set; }
        public string DepositTo { get; set; }
        public Account Account { get; set; }
        public Account Customer { get; set; }
        public string ReceiptNumber { get; set; }
        public DateTime Date { get; set; }
        public string strDate { get; set; }
        public string AmountReceived { get; set; }
        public object PaymentMethod { get; set; }
        public string Memo { get; set; }
        public List<Invoice> Invoices { get; set; }
        public object TransactionUid { get; set; }
        public object ForeignCurrency { get; set; }
        public Uri Uri { get; set; }
        public string RowVersion { get; set; }
        public void Dispose()
        {

        }
    }

    public class Account : IDisposable
    {
        public Guid Uid { get; set; }
        public string Name { get; set; }                
        public string DisplayId { get; set; }
        public Uri Uri { get; set; }
        public void Dispose()
        {

        }
    }

    public class Invoice: IDisposable
    {
        public long RowId { get; set; }       
        public string Number { get; set; }       
        public Guid Uid { get; set; }        
        public string AmountApplied { get; set; }        
        public string Type { get; set; }        
        public Uri Uri { get; set; }

        public void Dispose()
        {

        }
    }

}