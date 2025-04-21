﻿using Newtonsoft.Json;

namespace FliesProject.AIBot.APIModels.API_Response.Success
{
    public class GroundingSupport
    {
        [JsonProperty("segment")]
        public Segment Segment { get; set; }

        [JsonProperty("groundingChunkIndices")]
        public List<int?> GroundingChunkIndices { get; set; }

        [JsonProperty("confidenceScores")]
        public List<double?> ConfidenceScores { get; set; }
    }
}
