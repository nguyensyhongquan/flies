using Newtonsoft.Json;

namespace FliesProject.AIBot.APIModels.API_Request


{
    public class InlineData
    {
        [JsonProperty("mime_type")]
        public required string MimeType { get; set; }

        [JsonProperty("data")]
        public required string Data { get; set; }
    }
}
