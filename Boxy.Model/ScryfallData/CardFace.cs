using Newtonsoft.Json;

namespace Boxy.Model.ScryfallData
{
    public class CardFace
    {
        //TODO: FIX DOUBLE FACED CARDS
        [JsonProperty("type")]
        public string Name { get; set; }

        [JsonProperty("image_uris")]
        public ImageUris ImageUris { get; set; }
        
        [JsonProperty("mana_cost")]
        public string ManaCost { get; set; }
        
        [JsonProperty("type_line")]
        public string TypeLine { get; set; }
        
        [JsonProperty("oracle_text")]
        public string OracleText { get; set; }
        
        [JsonProperty("power")]
        public string Power { get; set; }
        
        [JsonProperty("toughness")]
        public string Toughness { get; set; }
        
        [JsonProperty("flavor_text")]
        public string FlavorText { get; set; }
        
        [JsonProperty("artist")]
        public string Artist { get; set; }
        
        [JsonProperty("artist_id")]
        public string ArtistId { get; set; }
        
        [JsonProperty("illustration_id")]
        public string IllustrationId { get; set; }
    }
}
