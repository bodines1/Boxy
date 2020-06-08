using Newtonsoft.Json;
using System;

namespace Boxy.Model.ScryfallData
{
    public class BulkData
    {
        [JsonProperty("id")]
        public string Id { get; set; }
        
        [JsonProperty("type")]
        public string Type { get; set; }
        
        [JsonProperty("name")]
        public string Name { get; set; }
        
        [JsonProperty("description")]
        public string Description { get; set; }
        
        [JsonProperty("download_uri")]
        public Uri PermalinkUri { get; set; }
        
        [JsonProperty("updated_at")]
        public DateTime UpdatedAt { get; set; }
        
        [JsonProperty("compressed_size")]
        public int CompressedSize { get; set; }
        
        [JsonProperty("content_type")]
        public string ContentType { get; set; }
        
        [JsonProperty("content_encoding")]
        public string ContentEncoding { get; set; }
    }
}
