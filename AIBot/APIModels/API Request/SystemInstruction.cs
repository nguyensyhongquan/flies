﻿using FliesProject.AIBot.Helpers;
using Models.Shared;
using Newtonsoft.Json;

namespace Models.Request
{
    /// <summary>
    /// Represents system-level instructions that guide the model's behavior.
    /// System instructions can be used to set context, define roles, or establish constraints
    /// for the model's responses.
    /// </summary>
    public class SystemInstruction
    {
        /// <summary>
        /// The role for this instruction (e.g., user, assistant).
        /// Default value is "user".
        /// </summary>
        [JsonProperty("role")]
        public string Role { get; set; } = EnumHelper.GetDescription(Enums.Role.User);

        /// <summary>
        /// The list of content parts that make up the system instruction.
        /// This is a required field and must contain at least one part.
        /// </summary>
        [JsonProperty("parts")]
        public required List<Part> Parts { get; set; }
    }
}
