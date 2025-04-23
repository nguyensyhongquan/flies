//using System.Text;
//using FliesProject.Models.AImodel;
//using FliesProject.Services;
//using Microsoft.AspNetCore.Authorization;
//using Microsoft.AspNetCore.Mvc;
//using Microsoft.AspNetCore.SignalR;
//using FliesProject.Hubs;

//namespace FliesProject.Controllers
//{
//    [ApiController]
//    [Route("api/[controller]")]
//    public class ChatController : ControllerBase
//    {
//        private readonly IDatabaseService _databaseService;
//        private readonly IAIService _aiService;
//        private readonly ILogger<ChatController> _logger;
//        private readonly IHubContext<NotificationHub> _hubContext;
//        private const int MaxRetries = 3;

//        public ChatController(
//            IDatabaseService databaseService,
//            IAIService aiService,
//            ILogger<ChatController> logger,
//            IHubContext<NotificationHub> hubContext)
//        {
//            _databaseService = databaseService;
//            _aiService = aiService;
//            _logger = logger;
//            _hubContext = hubContext;
//        }

//        [HttpPost("message")]
//        public async Task<IActionResult> SendMessage([FromBody] ChatRequest request)
//        {
//            try
//            {
//                // Process the message and get response
//                var response = await ProcessMessage(request.Message);

//                // Send notification
//                await _hubContext.Clients.All.SendAsync("ReceiveNotification", 
//                    "New message processed successfully", "success");

//                // If there's table data, send it through SignalR
//                if (response.TableData != null)
//                {
//                    await _hubContext.Clients.All.SendAsync("ReceiveTableUpdate", 
//                        System.Text.Json.JsonSerializer.Serialize(response.TableData));
//                }

//                return Ok(response);
//            }
//            catch (Exception ex)
//            {
//                // Send error notification
//                await _hubContext.Clients.All.SendAsync("ReceiveNotification", 
//                    $"Error processing message: {ex.Message}", "error");
                
//                return StatusCode(500, new { error = ex.Message });
//            }
//        }

//        private async Task<ChatResponse> ProcessMessage(string message)
//        {
//            // Your existing message processing logic here
//            // Return ChatResponse object with necessary data
//            return new ChatResponse
//            {
//                Message = "Processed message",
//                TableData = new TableData
//                {
//                    ColumnNames = new[] { "Column1", "Column2" },
//                    Rows = new[] { new[] { "Data1", "Data2" } }
//                }
//            };
//        }

//        [Authorize(Policy = "Admin")]
//        [HttpPost]
//        public async Task<IActionResult> AnalyzeDatabase(ChatViewModel model)
//        {
//            if (string.IsNullOrEmpty(model.DatabaseType) || string.IsNullOrEmpty(model.ConnectionString))
//            {
//                ModelState.AddModelError("", "Vui lòng nhập đầy đủ thông tin cơ sở dữ liệu");
//                //return View("Index", model);
//            }

//            try
//            {
//                // Kiểm tra kết nối trước
//                var (connectionSuccess, connectionMessage) = await _databaseService.TestConnection(
//                    model.DatabaseType,
//                    model.ConnectionString);

//                if (!connectionSuccess)
//                {
//                    model.HasError = true;
//                    model.ErrorMessage = $"Lỗi kết nối database: {connectionMessage}";
//                    //return View("Index", model);
//                }

//                var (success, message, info) = await _databaseService.AnalyzeDatabase(
//                    model.DatabaseType,
//                    model.ConnectionString);

//                if (!success)
//                {
//                    model.HasError = true;
//                    model.ErrorMessage = message;
//                    return View("Index", model);
//                }

//                // Tạo phân tích tổng quan về database
//                var analysis = new StringBuilder();
//                var tables = info["tables"] as List<dynamic>;

//                analysis.AppendLine("📊 Phân tích cơ sở dữ liệu của bạn");
//                analysis.AppendLine();
//                analysis.AppendLine($"📌 Tổng số bảng: {tables.Count}");
//                analysis.AppendLine();

//                foreach (var table in tables)
//                {
//                    analysis.AppendLine($"📋 Bảng: {table.TableName}");
//                    analysis.AppendLine($"   ├─ Số cột: {table.ColumnCount}");
//                    analysis.AppendLine($"   ├─ Số cột nullable: {table.NullableCount}");

//                    if (info.ContainsKey($"columns_{table.TableName}"))
//                    {
//                        var columns = info[$"columns_{table.TableName}"] as List<dynamic>;
//                        analysis.AppendLine("   └─ Cấu trúc:");
//                        foreach (var column in columns)
//                        {
//                            var isPk = column.IsPrimaryKey == true;
//                            var isNullable = column.IsNullable?.ToString()?.ToUpper() == "YES";
//                            analysis.AppendLine($"      {(isPk ? "🔑" : "•")} {column.ColumnName}");
//                            analysis.AppendLine($"        └─ Kiểu: {column.DataType}{(isNullable ? " (nullable)" : "")}");
//                        }
//                    }
//                    analysis.AppendLine();
//                }

//                // Thêm gợi ý sử dụng
//                analysis.AppendLine("💡 Gợi ý sử dụng:");
//                analysis.AppendLine("1. Bạn có thể hỏi các câu hỏi bằng tiếng Việt về dữ liệu trong database");
//                analysis.AppendLine("2. Hệ thống sẽ tự động tạo câu truy vấn SQL phù hợp");
//                analysis.AppendLine("3. Kết quả sẽ được phân tích và hiển thị dưới dạng văn bản và biểu đồ (nếu phù hợp)");

//                model.Answer = analysis.ToString();
//                model.HasError = false;
//                return View("Index", model);
//            }
//            catch (Exception ex)
//            {
//                _logger.LogError(ex, "Lỗi phân tích database");
//                model.HasError = true;
//                model.ErrorMessage = "Có lỗi xảy ra khi phân tích database. Vui lòng thử lại sau.";
//                return View("Index", model);
//            }
//        }

//        [HttpPost]
//        public async Task<IActionResult> CheckConnection(ChatViewModel model)
//        {
//            if (string.IsNullOrEmpty(model.DatabaseType) || string.IsNullOrEmpty(model.ConnectionString))
//            {
//                ModelState.AddModelError("", "Vui lòng nhập đầy đủ thông tin cơ sở dữ liệu");
//                return View("Index", model);
//            }

//            try
//            {
//                var (success, message) = await _databaseService.TestConnection(
//                    model.DatabaseType,
//                    model.ConnectionString);

//                model.HasError = !success;
//                model.Answer = success
//                    ? "✅ Kết nối thành công! Bạn có thể bắt đầu truy vấn database."
//                    : $"❌ Lỗi kết nối: {message}";
//                return View("Index", model);
//            }
//            catch (Exception ex)
//            {
//                _logger.LogError(ex, "Lỗi kiểm tra kết nối");
//                model.HasError = true;
//                model.ErrorMessage = "Có lỗi xảy ra khi kiểm tra kết nối. Vui lòng thử lại sau.";
//                return View("Index", model);
//            }
//        }

//        private bool ShouldGenerateChart(string question)
//        {
//            var keywords = new[] { "biểu đồ", "chart", "thống kê", "visualization", "trực quan", "graph" };
//            return keywords.Any(k => question.ToLower().Contains(k));
//        }

//        private string FormatDataTablePreview(System.Data.DataTable data, int rowCount)
//        {
//            var preview = new StringBuilder();

//            // Header
//            foreach (System.Data.DataColumn col in data.Columns)
//            {
//                preview.Append($"{col.ColumnName}\t");
//            }
//            preview.AppendLine();

//            // Separator
//            foreach (System.Data.DataColumn col in data.Columns)
//            {
//                preview.Append(new string('-', col.ColumnName.Length) + "\t");
//            }
//            preview.AppendLine();

//            // Data
//            for (int i = 0; i < Math.Min(rowCount, data.Rows.Count); i++)
//            {
//                foreach (var item in data.Rows[i].ItemArray)
//                {
//                    preview.Append($"{item}\t");
//                }
//                preview.AppendLine();
//            }

//            return preview.ToString();
//        }
//    }

//    public class ChatRequest
//    {
//        public string Message { get; set; }
//    }

 
//    public class TableData
//    {
//        public string[] ColumnNames { get; set; }
//        public string[][] Rows { get; set; }
//    }
//}
