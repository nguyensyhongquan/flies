using System.Text.Json;
using FliesProject.AIBot;
using FliesProject.Models.AImodel;
using FliesProject.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Models.Enums;

namespace FliesProject.Controllers.Api
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize(Policy = "admin")]
    public class ChatController : ControllerBase
    {
        private readonly ChatRouterService _chatRouterService;
        private readonly ILogger<ChatController> _logger;

        public ChatController(ChatRouterService chatRouterService, ILogger<ChatController> logger)
        {
            _chatRouterService = chatRouterService;
            _logger = logger;
        }

        // POST: /api/chat/message
        [HttpPost("message")]
        public async Task<ActionResult<ChatRequest>> ProcessMessage([FromBody] ChatRequest req)
        {
            Console.WriteLine("hiiiiiiiiii");
    //        const string SessionKey = "ChatHistory";
    //        var historyJson = HttpContext.Session.GetString(SessionKey);
    //        var history = string.IsNullOrEmpty(historyJson)
    //? new List<Message>()
    //: JsonSerializer.Deserialize<List<Message>>(historyJson)!;

    //        Console.WriteLine("The MEssaage is "+req.Message);
            if (string.IsNullOrWhiteSpace(req?.Message))
                return BadRequest(new ChatResponse
                {
                    IsError = true,
                    ErrorMessage = "Message is required."
                });

            _logger.LogInformation("Start processing message: {Message}", req.Message);
            //history.Add(new Message
            //{
            //    Role = "user",
            //    Content = req.Message
            //});
            try
            {
    //            var aiRequest = new ApiRequestBuilder()
    //.WithChatHistory(history.Select(m => new Message
    //{
    //    Role = m.Role == "user" ? Role.User : Role.Assistant,
    //    Content = m.Content
    //}))
    //.WithPrompt(req.Message)
    //.WithDefaultGenerationConfig()
    //.Build();

                var reply = await _chatRouterService.RouteQuestion(req.Message);
                //history.Add(new Message
                //{
                //    Role = "assistant",
                //    Content = reply
                //});
                //HttpContext.Session.SetString(SessionKey, JsonSerializer.Serialize(history));
                // Nếu bạn có ChartType/ChartData, parse từ reply hoặc ChatResponse
                // Giả sử reply là thuần text, không chart:
                return Ok(new ChatResponse
                {
                    Message = reply,
                    ChartType = null,
                    ChartData = null,
                    IsError = false
                });

            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing chat message for: {Message}", req.Message);
                return StatusCode(500, new ChatResponse
                {
                    IsError = true,
                    ErrorMessage = "Có lỗi xảy ra khi xử lý câu hỏi của bạn."
                });
            }
        }
    }
}
