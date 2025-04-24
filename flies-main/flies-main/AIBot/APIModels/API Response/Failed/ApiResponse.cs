using Newtonsoft.Json;

namespace FliesProject.AIBot.APIModels.API_Response.Failed

{
    public class ApiResponse
    {
        [JsonProperty("error")]
        public Error Error { get; set; }
    }
}
