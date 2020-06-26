using Newtonsoft.Json;
using System;
using System.Collections.Generic;

namespace GeneralLedgerJournalTransaction
{
    public class JournalTransactionModel : IDisposable
    {
        [JsonProperty("Items")]
        public List<Item> Items { get; set; }

        [JsonProperty("NextPageLink")]
        public object NextPageLink { get; set; }

        [JsonProperty("Count")]
        public long Count { get; set; }

        public void Dispose()
        {

        }
    }

    public class Item : IDisposable
    {
        [JsonProperty("UID")]
        public Guid Uid { get; set; }

        [JsonProperty("DisplayID")]
        public string DisplayId { get; set; }

        [JsonProperty("JournalType")]
        public string JournalType { get; set; }

        [JsonProperty("SourceTransaction")]
        public SourceTransaction SourceTransaction { get; set; }

        [JsonProperty("DateOccurred")]
        public DateTimeOffset DateOccurred { get; set; }

        [JsonProperty("DatePosted")]
        public DateTimeOffset DatePosted { get; set; }

        [JsonProperty("Description")]
        public string Description { get; set; }

        [JsonProperty("Lines")]
        public List<Line> Lines { get; set; }

        [JsonProperty("URI")]
        public Uri Uri { get; set; }

        [JsonProperty("RowVersion")]
        public string RowVersion { get; set; }

        public void Dispose()
        {

        }
    }

    public class Line : IDisposable
    {
        [JsonProperty("Account")]
        public Account Account { get; set; }

        [JsonProperty("Amount")]
        public double Amount { get; set; }

        [JsonProperty("IsCredit")]
        public bool IsCredit { get; set; }

        [JsonProperty("Job")]
        public object Job { get; set; }

        [JsonProperty("LineDescription")]
        public string LineDescription { get; set; }

        [JsonProperty("ReconciledDate")]
        public object ReconciledDate { get; set; }

        [JsonProperty("UnitCount")]
        public object UnitCount { get; set; }

        public void Dispose()
        {

        }
    }

    public class Account : IDisposable
    {
        [JsonProperty("UID")]
        public Guid Uid { get; set; }

        [JsonProperty("Name")]
        public string Name { get; set; }

        [JsonProperty("DisplayID")]
        public string DisplayId { get; set; }

        [JsonProperty("URI")]
        public Uri Uri { get; set; }

        public void Dispose()
        {

        }
    }

    public class SourceTransaction : IDisposable
    {
        [JsonProperty("UID")]
        public Guid Uid { get; set; }

        [JsonProperty("TransactionType")]
        public string TransactionType { get; set; }

        [JsonProperty("URI")]
        public Uri Uri { get; set; }

        public void Dispose()
        {

        }
    }

}

