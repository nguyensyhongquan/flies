﻿using FliesProject.AIBot.APIModels.API_Request;
using Newtonsoft.Json;

namespace Models.Shared
{
    /// <summary>
    /// Represents a single part of a content block in the API request/response.
    /// Parts are the basic building blocks of messages in the conversation,
    /// allowing for structured content delivery in the API communication.
    /// </summary>
    public class Part
    {
        /// <summary>
        /// The text content of this part.
        /// </summary>
        [JsonProperty("text")]
        public string? Text { get; set; }

        /// <summary>
        /// (Optional) The inline data for this part, contains the media content
        /// </summary>
        [JsonProperty("inline_data")]
        public InlineData? InlineData { get; set; }
    }
}
