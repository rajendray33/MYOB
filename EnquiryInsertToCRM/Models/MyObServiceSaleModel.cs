using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace EnquiryInsertToCRM.Models
{
    public class MyObServiceSaleModel : IDisposable
    {
        public string uid { get; set; }
        public string uri { get; set; }
        public string invoiceNumber { get; set; }
        public string issueDate { get; set; }
        public string dueDate { get; set; }
        public string status { get; set; }
        public string displayStatus { get; set; }
        public string total { get; set; }
        public string amountDue { get; set; }
        public string type { get; set; } = "Invoice";
        public bool sent { get; set; }
        public MyObServiceSaleContactModel contactModel { get; set; }
        public void Dispose()
        {

        }
    }
    public class MyObServiceSaleContactModel : IDisposable
    {
        public string uid { get; set; }
        public string uri { get; set; }
        public string name { get; set; }

        public void Dispose()
        {

        }
    }

    public class MyObServiceSaleHistoryModel : IDisposable
    {
        public string id { get; set; }
        public string reference { get; set; }
        public string transactionDate { get; set; }
        public string type { get; set; }
        public string amount { get; set; }
        public string status { get; set; }
        public string paymentAmount { get; set; }
        public string total { get; set; }
        public string documentId { get; set; }
        public void Dispose()
        {

        }
    }
    public class JournalTransaction : IDisposable
    {
        public string id { get; set; }
        public string reference { get; set; }
        public string transactionDate { get; set; }
        public string journalType { get; set; }
        public string amount { get; set; }
        public string status { get; set; }
        public string paymentAmount { get; set; }
        public string total { get; set; }
        public string documentId { get; set; }
        public void Dispose()
        {

        }
    }

    public class SaleInvoice : IDisposable
    {
        public string UID { get; set; }
        public string Number { get; set; }
        public DateTime Date { get; set; }
        public string strDate { get; set; }
        public Customer Customer { get; set; }
        //public string CustomerUID { get; set; }        
        //public string CustomerName { get; set; }
        //public string CustomerDisplayID { get; set; }
        //public string CustomerURI { get; set; }
        public DateTime? PromisedDate { get; set; }
        public string strPromisedDate { get; set; }
        public string BalanceDueAmount { get; set; }
        public string BalanceDueAmountForeign { get; set; }
        public string Status { get; set; }
        public string Subtotal { get; set; }
        public List<CustomerPaymentModel> CustomerPaymentModel { get; set; }
        public void Dispose()
        {

        }
    }

    public class Customer : IDisposable
    {
        public string UID { get; set; }
        public string Name { get; set; }
        public string DisplayID { get; set; }
        public string URI { get; set; }
        public string Number { get; set; }

        public void Dispose()
        {

        }
    }

}