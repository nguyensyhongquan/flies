//using FliesProject.Models.AImodel;

//using System.Data;
//using System.Text;
//using OpenAI_API;
//using OpenAI_API.Chat;
//using OpenAI_API.Models;
//using Gemini.NET;
//namespace FliesProject.Services
//{
//    public class AIService : IAIService
//    {
//        private readonly OpenAIAPI _openAI;
//        private readonly ILogger<AIService> _logger;

//        public AIService(IConfiguration configuration, ILogger<AIService> logger)
//        {
//            _openAI = new OpenAIAPI(configuration["OpenAI:ApiKey"]);
//            _logger = logger;
//        }

//        public async Task<(string query, string explanation)> GenerateSqlQuery(string question, Dictionary<string, object> databaseInfo)
//        {
//            try
//            {
//                var tables = databaseInfo["tables"] as List<dynamic>;
//                var schema = new StringBuilder();

//                // Xây dựng thông tin schema
//                foreach (var table in tables)
//                {
//                    schema.AppendLine($"Table: {table.TableName}");
//                    if (databaseInfo.ContainsKey($"columns_{table.TableName}"))
//                    {
//                        var columns = databaseInfo[$"columns_{table.TableName}"] as List<dynamic>;
//                        foreach (var column in columns)
//                        {
//                            schema.AppendLine($"- {column.ColumnName} ({column.DataType}){(column.IsPrimaryKey == true ? " [PK]" : "")}");
//                        }
//                    }
//                    schema.AppendLine();
//                }

//                var messages = new List<ChatMessage>
//                {
//                    new ChatMessage(ChatMessageRole.System, @"Bạn là một chuyên gia SQL. Nhiệm vụ của bạn là:
//1. Phân tích câu hỏi của người dùng
//2. Tạo câu truy vấn SQL phù hợp
//3. Giải thích cách câu truy vấn hoạt động
//Chỉ trả về câu truy vấn SELECT, không chấp nhận các câu lệnh khác."),
//                    new ChatMessage(ChatMessageRole.User, $@"Schema database:
//{schema}

//Câu hỏi: {question}

//Hãy tạo câu truy vấn SQL và giải thích cách nó hoạt động. Trả về theo định dạng:
//QUERY: <câu truy vấn>
//EXPLANATION: <giải thích>")
//                };

//                var response = await _openAI.Chat.CreateChatCompletionAsync(new ChatRequest()
//                {
//                    Model = Model.GPT4,
//                    Messages = messages,
//                    Temperature = 0.3,
//                    MaxTokens = 2000
//                });

//                var content = response.Choices[0].Message.Content;
//                var query = content.Split("QUERY:", StringSplitOptions.RemoveEmptyEntries)[1]
//                    .Split("EXPLANATION:", StringSplitOptions.RemoveEmptyEntries)[0].Trim();
//                var explanation = content.Split("EXPLANATION:", StringSplitOptions.RemoveEmptyEntries)[1].Trim();

//                return (query, explanation);
//            }
//            catch (Exception ex)
//            {
//                _logger.LogError(ex, "Lỗi khi tạo câu truy vấn SQL");
//                throw;
//            }
//        }

//        public async Task<string> AnalyzeQueryResult(string question, DataTable result)
//        {
//            try
//            {
//                var data = new StringBuilder();
//                data.AppendLine("Dữ liệu:");

//                // Convert DataTable to text format
//                foreach (DataColumn col in result.Columns)
//                {
//                    data.Append($"{col.ColumnName}\t");
//                }
//                data.AppendLine();

//                foreach (DataRow row in result.Rows)
//                {
//                    foreach (var item in row.ItemArray)
//                    {
//                        data.Append($"{item}\t");
//                    }
//                    data.AppendLine();
//                }

//                var messages = new List<ChatMessage>
//                {
//                    new ChatMessage(ChatMessageRole.System, @"Bạn là một chuyên gia phân tích dữ liệu. 
//Nhiệm vụ của bạn là phân tích kết quả truy vấn và đưa ra những insight có giá trị.
//Hãy trả lời bằng tiếng Việt, ngắn gọn và dễ hiểu."),
//                    new ChatMessage(ChatMessageRole.User, $@"Câu hỏi: {question}

//{data}

//Hãy phân tích dữ liệu trên và đưa ra những insight quan trọng.")
//                };

//                var response = await _openAI.Chat.CreateChatCompletionAsync(new ChatRequest()
//                {
//                    Model = Model.GPT4,
//                    Messages = messages,
//                    Temperature = 0.5,
//                    MaxTokens = 1000
//                });

//                return response.Choices[0].Message.Content;
//            }
//            catch (Exception ex)
//            {
//                _logger.LogError(ex, "Lỗi khi phân tích kết quả truy vấn");
//                throw;
//            }
//        }

//        public async Task<(string chartType, string chartData)> GenerateChartSuggestion(DataTable data, string analysis)
//        {
//            try
//            {
//                var dataDescription = new StringBuilder();
//                dataDescription.AppendLine("Columns:");
//                foreach (DataColumn col in data.Columns)
//                {
//                    dataDescription.AppendLine($"- {col.ColumnName} ({col.DataType.Name})");
//                }

//                var messages = new List<ChatMessage>
//                {
//                    new ChatMessage(ChatMessageRole.System, @"Bạn là một chuyên gia về data visualization.
//Nhiệm vụ của bạn là đề xuất loại biểu đồ phù hợp nhất để hiển thị dữ liệu.
//Chỉ chọn một trong các loại biểu đồ sau: bar, line, pie, scatter.
//Trả về theo định dạng JSON với 2 field: chartType và chartData."),
//                    new ChatMessage(ChatMessageRole.User, $@"Dữ liệu:
//{dataDescription}

//Phân tích:
//{analysis}

//Hãy đề xuất loại biểu đồ phù hợp nhất và tạo cấu trúc dữ liệu cho biểu đồ đó.")
//                };

//                var response = await _openAI.Chat.CreateChatCompletionAsync(new ChatRequest()
//                {
//                    Model = Model.GPT4,
//                    Messages = messages,
//                    Temperature = 0.3,
//                    MaxTokens = 1000
//                });

//                var content = response.Choices[0].Message.Content;
//                var chartSuggestion = JsonSerializer.Deserialize<ChartSuggestion>(content);

//                return (chartSuggestion.chartType, chartSuggestion.chartData);
//            }
//            catch (Exception ex)
//            {
//                _logger.LogError(ex, "Lỗi khi tạo đề xuất biểu đồ");
//                throw;
//            }
//        }

//        private class ChartSuggestion
//        {
//            public string chartType { get; set; }
//            public string chartData { get; set; }
//        }
//    }
//}
