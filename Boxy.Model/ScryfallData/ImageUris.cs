using Newtonsoft.Json;

namespace CardMimic.Model.ScryfallData
{
    /// <summary>
    /// All the URIs pointing to the various images for the card on scryfall.
    /// </summary>
    public class ImageUris
    {
        [JsonProperty("small")]
        public string Small { get; set; }
        
        [JsonProperty("png")]
        public string Png { get; set; }
        
        [JsonProperty("art_crop")]
        public string ArtCrop { get; set; }
        
        [JsonProperty("border_crop")]
        public string BorderCrop { get; set; }
    }
}
