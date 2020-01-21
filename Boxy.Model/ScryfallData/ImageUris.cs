using Newtonsoft.Json;

namespace Boxy.Model.ScryfallData
{
    public class ImageUris
    {
        [JsonProperty("small")]
        public string Small { get; set; }
        
        [JsonProperty("normal")]
        public string Normal { get; set; }
        
        [JsonProperty("large")]
        public string Large { get; set; }
        
        [JsonProperty("png")]
        public string Png { get; set; }
        
        [JsonProperty("art_crop")]
        public string ArtCrop { get; set; }
        
        [JsonProperty("border_crop")]
        public string BorderCrop { get; set; }
    }
}
