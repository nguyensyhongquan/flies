﻿using Newtonsoft.Json;

namespace FliesProject.AIBot.APIModels.API_Response.Success
{
    public class GroundingMetadata
    {
        [JsonProperty("searchEntryPoint")]
        public SearchEntryPoint? SearchEntryPoint { get; set; }

        [JsonProperty("groundingChunks")]
        public List<GroundingChunk> GroundingChunks { get; set; }

        [JsonProperty("groundingSupports")]
        public List<GroundingSupport>? GroundingSupports { get; set; }

        [JsonProperty("retrievalMetadata")]
        public RetrievalMetadata? RetrievalMetadata { get; set; }

        [JsonProperty("webSearchQueries")]
        public List<string>? WebSearchQueries { get; set; }
    }
}
