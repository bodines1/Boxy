using Newtonsoft.Json;
using System.Collections.Generic;

namespace Boxy.Model.ScryfallData
{
    public class Card
    {
        [JsonProperty("id")]
        public string Id { get; set; }
        
        [JsonProperty("oracle_id")]
        public string OracleId { get; set; }

        [JsonProperty("name")]
        public string Name { get; set; }
        
        [JsonProperty("lang")]
        public string Lang { get; set; }
        
        [JsonProperty("layout")]
        public string Layout { get; set; }
        
        [JsonProperty("highres_image")]
        public bool HighresImage { get; set; }
        
        [JsonProperty("image_uris")]
        public ImageUris ImageUris { get; set; }
        
        [JsonProperty("card_faces")]
        public List<CardFace> CardFaces { get; set; }
        
        [JsonProperty("legalities")]
        public Legalities Legalities { get; set; }
        
        [JsonProperty("oversized")]
        public bool Oversized { get; set; }
        
        [JsonProperty("promo")]
        public bool Promo { get; set; }
        
        [JsonProperty("set")]
        public string Set { get; set; }
        
        [JsonProperty("set_name")]
        public string SetName { get; set; }
        
        [JsonProperty("rulings_uri")]
        public string RulingsUri { get; set; }
        
        [JsonProperty("prints_search_uri")]
        public string PrintsSearchUri { get; set; }
        
        [JsonProperty("collector_number")]
        public string CollectorNumber { get; set; }
        
        [JsonProperty("digital")]
        public bool Digital { get; set; }
        
        [JsonProperty("flavor_text")]
        public string FlavorText { get; set; }
        
        [JsonProperty("card_back_id")]
        public string CardBackId { get; set; }
        
        [JsonProperty("artist")]
        public string Artist { get; set; }
        
        [JsonProperty("edhrec_rank")]
        public int EdhrecRank { get; set; }
        
        [JsonProperty("prices")]
        public Prices Prices { get; set; }
        
        [JsonProperty("related_uris")]
        public RelatedUris RelatedUris { get; set; }

        public bool IsDoubleFaced
        {
            get
            {
                return ImageUris == null && CardFaces != null && CardFaces.Count == 2;
            }
        }

        public bool IsToken
        {
            get
            {
                return Layout == "token" || Layout == "double_faced_token";
            }
        }
    }
}
