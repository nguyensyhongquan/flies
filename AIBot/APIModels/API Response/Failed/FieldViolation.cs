using Newtonsoft.Json;

namespace FliesProject.AIBot.APIModels.API_Response.Failed
{
    public class FieldViolation
    {
        [JsonProperty("field")]
        public string Field;

        [JsonProperty("description")]
        public string Description;
    }
}
