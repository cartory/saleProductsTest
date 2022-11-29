using System;
using System.Globalization;
using System.Collections.Generic;

using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace salesApi.src.sales.productsSales
{
    public partial class ProductSaleRes
    {
        [JsonProperty]
        public string saleId { get; set; }

        [JsonProperty]
        public double totalPrice { get; set; }

        [JsonProperty]
        public string state { get; set; }
    }

    public partial class ProductSaleRes
    {
        public static ProductSaleRes FromJson(string json) => JsonConvert.DeserializeObject<ProductSaleRes>(json, Converter.Settings);
    }

    public static class Serialize
    {
        public static string ToJson(this ProductSaleRes self) => JsonConvert.SerializeObject(self, Converter.Settings);
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
