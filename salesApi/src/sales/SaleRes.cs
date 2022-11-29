using System;
using System.Globalization;
using System.Collections.Generic;

using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System.ComponentModel.DataAnnotations;
using LiteDB;
using System.Runtime.CompilerServices;
using System.Xml.Schema;

namespace salesApi.src.sales
{
    public partial class SaleRes
    {
        [JsonProperty]
        public object? _id { get; set; }

        [JsonProperty]
        public string state { get; set; } = "pending";
        
        [JsonProperty]
        public double totalPrice { get; set; }

        [JsonProperty]
        public string personDoc { get; set; }

        [JsonProperty]
        public string personName { get; set; }

        [JsonProperty]
        public string description { get; set; }

        [JsonProperty]
        public ProductSale[] products { get; set; }
    }

    public partial class SaleRes
    {
        public static SaleRes FromJson(string json) => JsonConvert.DeserializeObject<SaleRes>(json, _Converter.Settings);

        public static SaleRes FromDocument(BsonDocument doc) 
        {
            var products = doc["products"].AsArray.Select(p =>
            {
                return new ProductSale() { id = p["id"], stock = long.Parse($"{p["stock"]}") };
            }).ToArray();

            return new SaleRes()
            {
                products = products,
                state= doc["state"].AsString,
                _id = doc["_id"].AsObjectId.ToString(),
                personDoc = doc["personDoc"].AsString,
                personName = doc["personName"].AsString,
                description = doc["description"].AsString,
                totalPrice = double.Parse($"{doc["totalPrice"]}"),
            };
        }
    }

    public static class _Serialize
    {
        public static string ToJson(this SaleRes self) => JsonConvert.SerializeObject(self, _Converter.Settings);
        public static BsonDocument ToDocument(this SaleRes self) => BsonMapper.Global.ToDocument<SaleRes>(self);
    }

    internal static class _Converter
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
