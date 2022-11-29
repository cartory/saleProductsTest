using System;
using System.Globalization;
using System.Collections.Generic;

using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System.ComponentModel.DataAnnotations;

namespace salesApi.src.sales
{
    public partial class SaleReq
    {
        [JsonProperty]
        [RegularExpression(@"^[\w\s-]*$", ErrorMessage = "personDoc not valid")]
        public string personDoc { get; set; }

        [JsonProperty]
        [RegularExpression(@"^[\w\s-,.]*$", ErrorMessage = "personName not valid")]
        public string personName { get; set; }

        [JsonProperty]
        [RegularExpression(@"^[\w\s-,.]*$", ErrorMessage = "description not valid")]
        public string description { get; set; }

        [JsonProperty]
        public ProductSale[] products { get; set; }
    }

    public partial class ProductSale
    {
        [JsonProperty]
        public string id { get; set; }

        [JsonProperty]
        public long stock { get; set; }
    }

    public partial class SaleReq
    {
        public static SaleReq FromJson(string json) => JsonConvert.DeserializeObject<SaleReq>(json, Converter.Settings);
    }

    public static class Serialize
    {
        public static string ToJson(this SaleReq self) => JsonConvert.SerializeObject(self, Converter.Settings);
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
}
