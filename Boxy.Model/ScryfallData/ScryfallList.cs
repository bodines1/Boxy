using Newtonsoft.Json;
using System;
using System.Collections.Generic;

namespace Boxy.Model.ScryfallData
{
    public class ScryfallList
    {
        [JsonProperty("data")]
        public List<string> Data { get; }

        [JsonProperty("has_more")]
        public bool HasMore { get; }

        [JsonProperty("next_page")]
        public Uri NextPage { get; }

        [JsonProperty("total_cards")]
        public int TotalCards { get; }

        [JsonProperty("warnings")]
        public List<string> Warnings { get; }
    }
}
