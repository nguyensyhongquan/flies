using Newtonsoft.Json;

namespace FliesProject.AIBot.APIModels.API_Response.Success
{
    public class RetrievalMetadata
    {
        [JsonProperty("googleSearchDynamicRetrievalScore")]
        public double? GoogleSearchDynamicRetrievalScore { get; set; }
    }
}
