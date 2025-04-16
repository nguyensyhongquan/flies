using Newtonsoft.Json;

namespace FliesProject.AIBot.APIModels.API_Response.Success
{
    public class GroundingChunk
    {
        [JsonProperty("web")]
        public Web Web { get; set; }
    }
}
