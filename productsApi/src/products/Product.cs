using System;
using System.Globalization;
using System.ComponentModel.DataAnnotations;

using LiteDB;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace productsApi.src.products
{
    public partial class Product
    {
        [JsonProperty]
        public object? _id { get; set; }

        [JsonProperty]
        [RegularExpression(@"^[\w\s-,.]*$", ErrorMessage = "title not valid")]
        public string title { get; set; }

        [JsonProperty]
        [RegularExpression(@"^[\w\s-,.]*$", ErrorMessage = "description not valid")]
        public string description { get; set; }

        [JsonProperty]
        [Range(1, double.MaxValue, ErrorMessage = "price not valid")]
        public double price { get; set; }

        [JsonProperty]
        [Range(1, long.MaxValue, ErrorMessage = "stock not valid")]
        public long stock { get; set; }

        [JsonProperty]
        [RegularExpression(@"^[\w\s-,.]*$", ErrorMessage = "brand not valid")]
        public string brand { get; set; }

        [JsonProperty]
        [RegularExpression(@"^[\w\s-,.]*$", ErrorMessage = "category not valid")]
        public string category { get; set; }

        [JsonProperty]
        public string[] images { get; set; }

        public bool ValidateImages(out string errorMesage)
        {
            var options = new UriCreationOptions();

            foreach (var image in images)
            {
                if (!Uri.TryCreate(image, options, out _)) 
                {
                    errorMesage = $"image: {image} not Valid Uri";
                    return false;
                }
            }

            errorMesage = "";
            return true;
        }
    }

    public partial class Product
    {
        public static Product FromJson(string json) => JsonConvert.DeserializeObject<Product>(json, Converter.Settings);

        public static Product FromDocument(BsonDocument doc) 
        {
            return new Product()
            {
                _id = doc["_id"].AsObjectId.ToString(),
                title = doc["title"].AsString,
                brand = doc["brand"].AsString,
                category = doc["category"].AsString,
                description = doc["description"].AsString,

                stock = doc["stock"].AsInt64,
                price = double.Parse($"{doc["price"]}"),
                images = doc["images"]?.AsArray.Select(x => x.AsString).ToArray() ?? new string[0],
            };
        }
    }

    public static class Serialize
    {
        public static string ToJson(this Product self) => JsonConvert.SerializeObject(self, Converter.Settings);
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
