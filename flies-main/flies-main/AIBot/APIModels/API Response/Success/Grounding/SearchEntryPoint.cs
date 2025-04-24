using Newtonsoft.Json;

namespace FliesProject.AIBot.APIModels.API_Response.Success
{
    public class SearchEntryPoint
    {
        [JsonProperty("renderedContent")]
        public string? RenderedContent { get; set; }
    }
}
