using Newtonsoft.Json;

namespace FliesProject.AIBot.APIModels.API_Response.Failed
{
    public class Detail
    {
        [JsonProperty("@type")]
        public string Type;

        [JsonProperty("fieldViolations")]
        public List<FieldViolation> FieldViolations;
    }
}
