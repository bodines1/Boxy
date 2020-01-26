using Newtonsoft.Json;

namespace Boxy.Model.ScryfallData
{
    public class Legalities
    {
        [JsonProperty("standard")]
        public string Standard { get; set; }

        [JsonProperty("future")]
        public string Future { get; set; }

        [JsonProperty("historic")]
        public string Historic { get; set; }

        [JsonProperty("pioneer")]
        public string Pioneer { get; set; }

        [JsonProperty("modern")]
        public string Modern { get; set; }

        [JsonProperty("legacy")]
        public string Legacy { get; set; }

        [JsonProperty("pauper")]
        public string Pauper { get; set; }

        [JsonProperty("vintage")]
        public string Vintage { get; set; }

        [JsonProperty("penny")]
        public string Penny { get; set; }

        [JsonProperty("commander")]
        public string Commander { get; set; }

        [JsonProperty("brawl")]
        public string Brawl { get; set; }

        [JsonProperty("duel")]
        public string Duel { get; set; }
        
        [JsonProperty("oldschool")]
        public string Oldschool { get; set; }
    }
}
