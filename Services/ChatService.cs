using FliesProject.Models.AImodel;
using System.Text;
using System.Text.Json;

namespace FliesProject.Services
{
    public class ChatService : IChatService
    {
        private readonly HttpClient _httpClient;
        private readonly IConfiguration _configuration;

        public ChatService(HttpClient httpClient, IConfiguration configuration)
        {
            _httpClient = httpClient;
            _configuration = configuration;
            _httpClient.BaseAddress = new Uri(_configuration["ApiSettings:BaseUrl"]);
        }

        public async Task<ChatViewModel> SendMessage(ChatViewModel model)
        {
            try
            {
                var request = new                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                     
                {
                    Message = model.Question,
                    DatabaseType = model.DatabaseType,
                    ConnectionString = model.ConnectionString,
                    GenerateChart = !string.IsNullOrEmpty(model.ChartType),
                    ChartType = model.ChartType
                };

                var response = await PostToApi("api/chat/send", request);
                return MapResponseToViewModel(response);
            }
            catch (Exception ex)
            {
                return CreateErrorViewModel(ex.Message);
            }
        }

        public async Task<ChatViewModel> AnalyzeDatabase(ChatViewModel model)
        {
            try
            {
                var request = new
                {
                    DatabaseType = model.DatabaseType,
                    ConnectionString = model.ConnectionString
                };

                var response = await PostToApi("api/chat/analyze", request);
                return MapResponseToViewModel(response);
            }
            catch (Exception ex)
            {
                return CreateErrorViewModel(ex.Message);
            }
        }

        public async Task<ChatViewModel> CheckConnection(ChatViewModel model)
        {
            try
            {
                var request = new
                {
                    DatabaseType = model.DatabaseType,
                    ConnectionString = model.ConnectionString
                };

                var response = await PostToApi("api/chat/check-connection", request);
                return MapResponseToViewModel(response);
            }
            catch (Exception ex)
            {
                return CreateErrorViewModel(ex.Message);
            }
        }

        private async Task<dynamic> PostToApi(string endpoint, object data)
        {
            var content = new StringContent(JsonSerializer.Serialize(data), Encoding.UTF8, "application/json");
            var response = await _httpClient.PostAsync(endpoint, content);
            response.EnsureSuccessStatusCode();
            var jsonResponse = await response.Content.ReadAsStringAsync();
            return JsonSerializer.Deserialize<dynamic>(jsonResponse);
        }

        private ChatViewModel MapResponseToViewModel(dynamic response)
        {
            return new ChatViewModel
            {
                Answer = response.Message,
                ChartData = response.ChartData,
                ChartType = response.ChartType,
                HasError = response.IsError,
                ErrorMessage = response.ErrorMessage
            };
        }

        private ChatViewModel CreateErrorViewModel(string errorMessage)
        {
            return new ChatViewModel
            {
                HasError = true,
                ErrorMessage = errorMessage
            };
        }
    }
}
