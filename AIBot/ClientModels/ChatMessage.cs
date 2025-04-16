using Models.Enums;


namespace FliesProject.AIBot.ClientModels

{
    public class ChatMessage
    {
        public required Role Role { get; set; }
        public required string Content { get; set; }
    }
}
