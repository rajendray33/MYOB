using Newtonsoft.Json;
using System;
using System.Collections.Generic;

namespace EnquiryInsertToCRM.Models
{
    public class CustomerSaleInvoice : IDisposable
    {
        public List<Item> Items { get; set; }

        public string NextPageLink { get; set; }

        public string Count { get; set; }

        public void Dispose()
        {

        }
    }
    public class Item : IDisposable
    {
        
        public string Uid { get; set; }

        
        public string Number { get; set; }

        
        public DateTime Date { get; set; }
        public DateTime Date_Converted { get; set; }

        public string CustomerPurchaseOrderNumber { get; set; }

        
        public Customer Customer { get; set; }

        
        public string PromisedDate { get; set; }

        
        public string BalanceDueAmount { get; set; }

        
        public string BalanceDueAmountForeign { get; set; }

        
        public string Status { get; set; }

        
        public string ShipToAddress { get; set; }

        
        public Terms Terms { get; set; }

        
        public string IsTaxInclusive { get; set; }

        [JsonProperty("Subtotal")]
        public double Subtotal { get; set; }

        
        public string SubtotalForeign { get; set; }

        
        public string Freight { get; set; }

        
        public string FreightForeign { get; set; }

        
        public FreightTaxCode FreightTaxCode { get; set; }

        
        public string TotalTax { get; set; }

        
        public string TotalTaxForeign { get; set; }

        
        public double TotalAmount { get; set; }

        public double TotalAmount_Filter { get; set; }


        public string TotalAmountForeign { get; set; }

        
        public string Category { get; set; }

        
        public Customer Salesperson { get; set; }

        
        public string Comment { get; set; }

        
        public string ShippingMethod { get; set; }

        
        public string JournalMemo { get; set; }

        
        public string ReferralSource { get; set; }

        
        public string InvoiceDeliveryStatus { get; set; }

        
        public string LastPaymentDate { get; set; }

        
        public string InvoiceType { get; set; }

        
        public Order Order { get; set; }

    
        public string OnlinePaymentMethod { get; set; }

        
        public string ForeignCurrency { get; set; }

        
        public string CurrencyExchangeRate { get; set; }

        
        public string LastModified { get; set; }

        
        public string Uri { get; set; }

        
        public string RowVersion { get; set; }

        public void Dispose()
        {

        }
    }

   

    public class FreightTaxCode : IDisposable
    {
        
        public string Uid { get; set; }

        
        public string Code { get; set; }

        
        public string Uri { get; set; }
        public void Dispose()
        {

        }
    }

    public class Order : IDisposable
    {
        [JsonProperty("UID")]
        public string Uid { get; set; }

        [JsonProperty("Number")]
        public string Number { get; set; }

        [JsonProperty("URI")]
        public string Uri { get; set; }

        public void Dispose()
        {

        }
    }
}

