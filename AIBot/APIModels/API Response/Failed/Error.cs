﻿using Newtonsoft.Json;

namespace FliesProject.AIBot.APIModels.API_Response.Failed
{
    public class Error
    {
        [JsonProperty("code")]
        public int? Code { get; set; }

        [JsonProperty("message")]
        public string Message { get; set; }

        [JsonProperty("status")]
        public string Status { get; set; }

        [JsonProperty("details")]
        public List<Detail>? Details { get; set; }
    }
}
