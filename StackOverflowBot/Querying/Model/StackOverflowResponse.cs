using Newtonsoft.Json;

namespace StackOverflowBot.Querying.Model
{

    public class StackOverflowResponse<T>
    {

        [JsonProperty("items")]
        public T[] Items { get; set; }

        [JsonProperty("has_more")]
        public bool HasMore { get; set; }

        [JsonProperty("quota_max")]
        public int QuotaMax { get; set; }

        [JsonProperty("quota_remaining")]
        public int QuotaRemaining { get; set; }

    }

}
