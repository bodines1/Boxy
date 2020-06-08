using Newtonsoft.Json;

namespace CardMimic.Model.ScryfallData
{
    public class ScryfallList<T>
    {
        [JsonProperty("total_cards")]
        public long TotalCards { get; set; }

        [JsonProperty("has_more")]
        public bool HasMore { get; set; }

        [JsonProperty("next_page")]
        public string NextPage { get; set; }

        [JsonProperty("data")]
        public T[] Data { get; set; }
    }
}
