using System;
using System.Collections.Generic;

namespace EnquiryInsertToCRM.Models
{

    public class CustomerInfo : IDisposable
    {
        public string Uid { get; set; }

        public string CompanyName { get; set; }

        public bool IsIndividual { get; set; }

        public string DisplayId { get; set; }

        public bool IsActive { get; set; }

        public List<Address> Addresses { get; set; }

        public string Notes { get; set; }

        public CustomField1[] Identifiers { get; set; }

        public CustomField1 CustomList1 { get; set; }

        public CustomField1 CustomList2 { get; set; }

        public CustomField1 CustomList3 { get; set; }

        public CustomField1 CustomField1 { get; set; }

        public CustomField1 CustomField2 { get; set; }

        public CustomField1 CustomField3 { get; set; }

        public double? CurrentBalance { get; set; }

        public SellingDetails SellingDetails { get; set; }

        public PaymentDetails PaymentDetails { get; set; }

        public string ForeignCurrency { get; set; }

        public DateTime LastModified { get; set; }

        public string PhotoUri { get; set; }

        public string Uri { get; set; }

        public string RowVersion { get; set; }

        public void Dispose()
        {

        }
    }
    public class PaymentDetails : IDisposable
    {
        public string Method { get; set; }

        public string CardNumber { get; set; }

        public string NameOnCard { get; set; }

        public string BsbNumber { get; set; }

        public string BankAccountNumber { get; set; }

        public string BankAccountName { get; set; }

        public string Notes { get; set; }

        public void Dispose()
        {

        }
    }
    public class Address : IDisposable
    {
        public string Location { get; set; }

        public string Street { get; set; }

        public string City { get; set; }

        public string State { get; set; }

        public string PostCode { get; set; }

        public string Country { get; set; }

        public string Phone1 { get; set; }

        public string Phone2 { get; set; }

        public string Phone3 { get; set; }

        public string Fax { get; set; }

        public string Email { get; set; }

        public string Website { get; set; }

        public string ContactName { get; set; }

        public string Salutation { get; set; }
        public void Dispose()
        {

        }
    }

    public class SellingDetails : IDisposable
    {
        public string SaleLayout { get; set; }

        public string PrintedForm { get; set; }

        public string InvoiceDelivery { get; set; }

        public string ItemPriceLevel { get; set; }

        public IncomeAccount IncomeAccount { get; set; }

        public string ReceiptMemo { get; set; }

        public SalesPerson SalesPerson { get; set; }

        public string SaleComment { get; set; }

        public string ShippingMethod { get; set; }

        public string HourlyBillingRate { get; set; }

        public string Abn { get; set; }

        public string AbnBranch { get; set; }

        public TaxCode TaxCode { get; set; }

        public TaxCode FreightTaxCode { get; set; }

        public bool UseCustomerTaxCode { get; set; }

        public Terms Terms { get; set; }

        public Credit Credit { get; set; }

        public string TaxIdNumber { get; set; }

        public string Memo { get; set; }

        public void Dispose()
        {

        }
    }
    public class CustomField1 : IDisposable
    {
        public string Label { get; set; }
        public string Value { get; set; }

        public void Dispose()
        {

        }
    }
    //public class SalesPerson:IDisposable
    //{
    //    public Guid Uid { get; set; }

    //    public string Name { get; set; }

    //    public string DisplayId { get; set; }

    //    public Uri Uri { get; set; }
    //    public void Dispose() {

    //    }
    //}
    public class Credit : IDisposable
    {
        public string Limit { get; set; }

        public double? Available { get; set; }

        public double? PastDue { get; set; }

        public bool OnHold { get; set; }
        public void Dispose()
        {

        }
    }

    public partial class TaxCode : IDisposable
    {
        public string Uid { get; set; }

        public string Code { get; set; }

        public string Uri { get; set; }

        public void Dispose()
        {

        }
    }

    public class IncomeAccount : IDisposable
    {
        public string Uid { get; set; }

        public string Name { get; set; }

        public string DisplayId { get; set; }

        public string Uri { get; set; }

        public void Dispose()
        {

        }
    }

    public class Terms : IDisposable
    {
        public string PaymentIsDue { get; set; }

        public string DiscountDate { get; set; }

        public string BalanceDueDate { get; set; }

        public string DiscountForEarlyPayment { get; set; }

        public double MonthlyChargeForLatePayment { get; set; }

        public string DiscountExpiryDate { get; set; }

        public string Discount { get; set; }

        public string DiscountForeign { get; set; }

        public string DueDate { get; set; }

        public string FinanceCharge { get; set; }

        public string FinanceChargeForeign { get; set; }

        public string VolumeDiscount { get; set; }

        public void Dispose()
        {

        }
    }
    public class SalesPerson : IDisposable
    {
        public string Uid { get; set; }

        public string Name { get; set; }

        public string DisplayId { get; set; }

        public string Uri { get; set; }

        public void Dispose()
        {

        }
    }
}

