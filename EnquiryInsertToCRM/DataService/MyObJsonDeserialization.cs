using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System;
using System.Collections.Generic;
using System.Globalization;

namespace MyObJsonDeserialization
{
    public partial class SaleService
    {
        [JsonProperty("_links")]
        public List<Link> Links { get; set; }

        [JsonProperty("items")]
        public List<Item> Items { get; set; }

        [JsonProperty("page")]
        public Page Page { get; set; }
    }

    public partial class Item
    {
        [JsonProperty("uid")]
        [JsonConverter(typeof(ParseStringConverter))]
        public long Uid { get; set; }

        [JsonProperty("uri")]
        public Uri Uri { get; set; }

        [JsonProperty("invoiceNumber")]
        public string InvoiceNumber { get; set; }

        [JsonProperty("contact")]
        public Contact Contact { get; set; }

        [JsonProperty("issueDate")]
        public DateTimeOffset IssueDate { get; set; }

        [JsonProperty("dueDate")]
        public DateTimeOffset DueDate { get; set; }

        [JsonProperty("status")]
        public string Status { get; set; }

        [JsonProperty("displayStatus")]
        public string DisplayStatus { get; set; }

        [JsonProperty("total")]
        public long Total { get; set; }

        [JsonProperty("amountDue")]
        public long AmountDue { get; set; }

        [JsonProperty("sent")]
        public bool Sent { get; set; }

        [JsonProperty("_links")]
        public List<Link> Links { get; set; }
    }

    public partial class Contact
    {
        [JsonProperty("uid")]
        [JsonConverter(typeof(ParseStringConverter))]
        public long Uid { get; set; }

        [JsonProperty("uri")]
        public Uri Uri { get; set; }

        [JsonProperty("name")]
        public string Name { get; set; }
    }

    public partial class Link
    {
        [JsonProperty("rel")]
        public string Rel { get; set; }

        [JsonProperty("href")]
        public Uri Href { get; set; }
    }

    public partial class Page
    {
        [JsonProperty("size")]
        public long Size { get; set; }

        [JsonProperty("totalElements")]
        public long TotalElements { get; set; }

        [JsonProperty("totalPages")]
        public long TotalPages { get; set; }

        [JsonProperty("number")]
        public long Number { get; set; }
    }

    public partial class SaleService
    {
        public static SaleService FromJson(string json) => JsonConvert.DeserializeObject<SaleService>(json, MyObJsonDeserialization.Converter.Settings);
    }

    public static class Serialize
    {
        public static string ToJson(this SaleService self) => JsonConvert.SerializeObject(self, MyObJsonDeserialization.Converter.Settings);
    }

    internal static class Converter
    {
        public static readonly JsonSerializerSettings Settings = new JsonSerializerSettings
        {
            MetadataPropertyHandling = MetadataPropertyHandling.Ignore,
            DateParseHandling = DateParseHandling.None,
            Converters =
            {
                new IsoDateTimeConverter { DateTimeStyles = DateTimeStyles.AssumeUniversal }
            },
        };
    }

    internal class ParseStringConverter : JsonConverter
    {
        public override bool CanConvert(Type t) => t == typeof(long) || t == typeof(long?);

        public override object ReadJson(JsonReader reader, Type t, object existingValue, JsonSerializer serializer)
        {
            if (reader.TokenType == JsonToken.Null) return null;
            var value = serializer.Deserialize<string>(reader);
            long l;
            if (Int64.TryParse(value, out l))
            {
                return l;
            }
            throw new Exception("Cannot unmarshal type long");
        }

        public override void WriteJson(JsonWriter writer, object untypedValue, JsonSerializer serializer)
        {
            if (untypedValue == null)
            {
                serializer.Serialize(writer, null);
                return;
            }
            var value = (long)untypedValue;
            serializer.Serialize(writer, value.ToString());
            return;
        }

        public static readonly ParseStringConverter Singleton = new ParseStringConverter();
    }
}


