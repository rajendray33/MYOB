using EnquiryInsertToCRM.DataService;
using EnquiryInsertToCRM.Maximizerwebdata;
using EnquiryInsertToCRM.Models;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Globalization;

namespace JSONResponseDeserialization
{
    public class JSONDeserialization 
    {
        [JsonProperty("id")]
        public string Id { get; set; }

        [JsonProperty("ip")]
        public string Ip { get; set; }

        [JsonProperty("created_at")]
        public string created_at { get; set; }

        [JsonProperty("form_id")]
        public string FormId { get; set; }

        [JsonProperty("status")]
        public string Status { get; set; }

        [JsonProperty("new")]
        [JsonConverter(typeof(ParseStringConverter))]
        public long New { get; set; }

        [JsonProperty("flag")]
        [JsonConverter(typeof(ParseStringConverter))]
        public long Flag { get; set; }
       
        [JsonProperty("answers")]
        public Dictionary<string, AnswerValue> Answers { get; set; }
    }

    public class AnswerValue
    {
        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("text")]
        public string Text { get; set; }

        [JsonProperty("type")]
        public string Type { get; set; }

        [JsonProperty("sublabels", NullValueHandling = NullValueHandling.Ignore)]
        public string Sublabels { get; set; }

        [JsonProperty("answer", NullValueHandling = NullValueHandling.Ignore)]
        public AnswerUnion? Answer { get; set; }

        [JsonProperty("prettyFormat", NullValueHandling = NullValueHandling.Ignore)]
        public string PrettyFormat { get; set; }      
    }

    public class AnswerAnswerClass
    {
        [JsonProperty("first", NullValueHandling = NullValueHandling.Ignore)]
        public string First { get; set; }

        [JsonProperty("last", NullValueHandling = NullValueHandling.Ignore)]
        public string Last { get; set; }

        [JsonProperty("middle", NullValueHandling = NullValueHandling.Ignore)]
        public string Middle { get; set; }

        [JsonProperty("addr_line1", NullValueHandling = NullValueHandling.Ignore)]
        public string AddrLine1 { get; set; }

        [JsonProperty("addr_line2", NullValueHandling = NullValueHandling.Ignore)]
        public string AddrLine2 { get; set; }

        [JsonProperty("city", NullValueHandling = NullValueHandling.Ignore)]
        public string City { get; set; }

        [JsonProperty("state", NullValueHandling = NullValueHandling.Ignore)]
        public string State { get; set; }

        [JsonProperty("postal", NullValueHandling = NullValueHandling.Ignore)]
        public string Postal { get; set; }

        [JsonProperty("country", NullValueHandling = NullValueHandling.Ignore)]
        public string Country { get; set; }       
    }

    public enum Static { No, Yes };

    public  struct AnswerUnion
    {
        public AnswerAnswerClass AnswerAnswerClass;
        public string String;

        public static implicit operator AnswerUnion(AnswerAnswerClass AnswerAnswerClass) => new AnswerUnion { AnswerAnswerClass = AnswerAnswerClass };
        public static implicit operator AnswerUnion(string String) => new AnswerUnion { String = String };
    }
        
    public static class Converter
    {
        public static readonly JsonSerializerSettings Settings = new JsonSerializerSettings
        {
            MetadataPropertyHandling = MetadataPropertyHandling.Ignore,
            DateParseHandling = DateParseHandling.None,
            Converters =
            {
                AnswerUnionConverter.Singleton,
                StaticConverter.Singleton,
                new IsoDateTimeConverter { DateTimeStyles = DateTimeStyles.AssumeUniversal }
            },
        };
    }

    public class AnswerUnionConverter : JsonConverter
    {
        public override bool CanConvert(Type t) => t == typeof(AnswerUnion) || t == typeof(AnswerUnion?);

        public override object ReadJson(JsonReader reader, Type t, object existingValue, JsonSerializer serializer)
        {
            if (Convert.ToString(reader.TokenType) == "StartArray") return null;
            switch (reader.TokenType)
                {
                    case JsonToken.String:
                    case JsonToken.Date:
                        var stringValue = serializer.Deserialize<string>(reader);
                        return new AnswerUnion { String = stringValue };
                    case JsonToken.StartObject:
                        var objectValue = serializer.Deserialize<AnswerAnswerClass>(reader);
                        return new AnswerUnion { AnswerAnswerClass = objectValue };
                }
            throw new Exception("Cannot marshal type AnswerUnion");
        }

        public override void WriteJson(JsonWriter writer, object untypedValue, JsonSerializer serializer)
        {
            var value = (AnswerUnion)untypedValue;
            if (value.String != null)
            {
                serializer.Serialize(writer, value.String);
                return;
            }
            if (value.AnswerAnswerClass != null)
            {
                serializer.Serialize(writer, value.AnswerAnswerClass);
                return;
            }
            throw new Exception("Cannot marshal type AnswerUnion");
        }

        public static readonly AnswerUnionConverter Singleton = new AnswerUnionConverter();
    }

    public class ParseStringConverter : JsonConverter
    {
        public override bool CanConvert(Type t) => t == typeof(long) || t == typeof(long?);

        public override object ReadJson(JsonReader reader, Type t, object existingValue, JsonSerializer serializer)
        {
            if (reader.TokenType == JsonToken.Null) return null;
            var value = serializer.Deserialize<string>(reader);
            long l =0;
            try
            {
                if (Int64.TryParse(value, out l))
                {
                    return l;
                }
            }
            catch (Exception ex)
            {
                return l = 0;
            }
            return l = 0;
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

    public class StaticConverter : JsonConverter
    {
        public override bool CanConvert(Type t) => t == typeof(Static) || t == typeof(Static?);

        public override object ReadJson(JsonReader reader, Type t, object existingValue, JsonSerializer serializer)
        {
            if (reader.TokenType == JsonToken.Null) return null;
            var value = serializer.Deserialize<string>(reader);
            switch (value)
            {
                case "No":
                    return Static.No;
                case "Yes":
                    return Static.Yes;
            }
            throw new Exception("Cannot unmarshal type Static");
        }

        public override void WriteJson(JsonWriter writer, object untypedValue, JsonSerializer serializer)
        {
            if (untypedValue == null)
            {
                serializer.Serialize(writer, null);
                return;
            }
            var value = (Static)untypedValue;
            switch (value)
            {
                case Static.No:
                    serializer.Serialize(writer, "No");
                    return;
                case Static.Yes:
                    serializer.Serialize(writer, "Yes");
                    return;
            }
            throw new Exception("Cannot marshal type Static");
        }

        public static readonly StaticConverter Singleton = new StaticConverter();
    }

    public static class Converter1
    {
        public static readonly JsonSerializerSettings Settings1 = new JsonSerializerSettings
        {
            MetadataPropertyHandling = MetadataPropertyHandling.Ignore,
            DateParseHandling = DateParseHandling.None,
            Converters =
            {
                new IsoDateTimeConverter { DateTimeStyles = DateTimeStyles.AssumeUniversal }
            },
        };
    }
}
