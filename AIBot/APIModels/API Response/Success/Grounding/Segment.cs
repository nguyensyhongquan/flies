﻿using Newtonsoft.Json;

namespace FliesProject.AIBot.APIModels.API_Response.Success
{
    public class Segment
    {
        [JsonProperty("startIndex")]
        public int? StartIndex { get; set; }

        [JsonProperty("endIndex")]
        public int? EndIndex { get; set; }

        [JsonProperty("text")]
        public string Text { get; set; }
    }
}
