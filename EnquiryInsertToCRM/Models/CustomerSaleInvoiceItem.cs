using Newtonsoft.Json;
using System;
using System.Collections.Generic;

namespace EnquiryInsertToCRM.Models
{
    public class CustomerSaleInvoiceItem : IDisposable
    {
        public string Uid { get; set; }

        public string Number { get; set; }

        public DateTime Date { get; set; }

        public string CustomerPurchaseOrderNumber { get; set; }


        public Customer Customer { get; set; }


        public string PromisedDate { get; set; }


        public string BalanceDueAmount { get; set; }


        public string BalanceDueAmountForeign { get; set; }


        public List<Line> Lines { get; set; }


        public string Status { get; set; }


        public string ShipToAddress { get; set; }


        public Terms Terms { get; set; }


        public string IsTaxInclusive { get; set; }

        [JsonProperty("Subtotal")]
        public double Subtotal { get; set; }

        public object SubtotalForeign { get; set; }


        public string Freight { get; set; }


        public string FreightForeign { get; set; }


        public TaxCode FreightTaxCode { get; set; }


        public string TotalTax { get; set; }


        public string TotalTaxForeign { get; set; }


        public string TotalAmount { get; set; }


        public string TotalAmountForeign { get; set; }


        public string Category { get; set; }

        public Customer Salesperson { get; set; }


        public string Comment { get; set; }


        public string ShippingMethod { get; set; }


        public string JournalMemo { get; set; }


        public string ReferralSource { get; set; }


        public string InvoiceDeliveryStatus { get; set; }


        public string LastPaymentDate { get; set; }


        public Customer Order { get; set; }

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

    public class Line : IDisposable
    {
        public string RowId { get; set; }

        public string ShipQuantity { get; set; }

        public string UnitPrice { get; set; }

        public string UnitPriceForeign { get; set; }

        public string DiscountPercent { get; set; }

        public string CostOfGoodsSold { get; set; }

        public Customer Item { get; set; }

        public string Type { get; set; }

        public string Description { get; set; }

        public string Total { get; set; }

        public string TotalForeign { get; set; }

        public string Job { get; set; }

        public TaxCode TaxCode { get; set; }

        public string RowVersion { get; set; }

        public void Dispose()
        {

        }
    }

}

