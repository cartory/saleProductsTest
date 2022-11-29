using Newtonsoft.Json;

namespace src.utils
{
    public class Response<T>
    {
        [JsonProperty]
        public string errorMessage { get; set; }
        
        [JsonProperty]
        public T data { get; set; }
    }
}
