using System;
using System.Globalization;
using System.Collections.Generic;

using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace productsApi.src.products.productSales
{
    public partial class ProductSaleReq
    {
        [JsonProperty]
        public string saleId { get; set; }

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

    public partial class ProductSaleReq
    {
        public static ProductSaleReq FromJson(string json) => JsonConvert.DeserializeObject<ProductSaleReq>(json, Converter.Settings);
    }

    public static class Serialize
    {
        public static string ToJson(this ProductSaleReq self) => JsonConvert.SerializeObject(self, Converter.Settings);
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
