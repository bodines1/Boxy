using Newtonsoft.Json;

namespace Boxy.Model.ScryfallData
{
    public class CardFace
    {
        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("image_uris")]
        public ImageUris ImageUris { get; set; }
        
        [JsonProperty("flavor_text")]
        public string FlavorText { get; set; }
        
        [JsonProperty("artist")]
        public string Artist { get; set; }
    }
}
