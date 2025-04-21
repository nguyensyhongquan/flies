using System.Text;
using FliesProject.Models.AImodel;
using FliesProject.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FliesProject.Controllers
{
    public class ChatController : Controller
    {
        private readonly IDatabaseService _databaseService;
        private readonly IAIService _aiService;
        private readonly ILogger<ChatController> _logger;
        private const int MaxRetries = 3;

        public ChatController(
            IDatabaseService databaseService,
            IAIService aiService,
            ILogger<ChatController> logger)
        {
            _databaseService = databaseService;
            _aiService = aiService;
            _logger = logger;
        }

        public IActionResult Index()
        {
            return View(new ChatViewModel());
        }
        [Authorize(Policy = "Admin")]
        [HttpPost]
        public async Task<IActionResult> SendMessage(ChatViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View("Index", model);
            }

            try
            {
                // Kiểm tra kết nối trước khi thực hiện các thao tác khác
                var (connectionSuccess, connectionMessage) = await _databaseService.TestConnection(
                    model.DatabaseType,
                    model.ConnectionString);

                if (!connectionSuccess)
                {
                    model.HasError = true;
                    model.ErrorMessage = $"Lỗi kết nối database: {connectionMessage}";
                    return View("Index", model);
                }

                // Phân tích database để lấy schema
                var (dbSuccess, dbMessage, dbInfo) = await _databaseService.AnalyzeDatabase(
                    model.DatabaseType,
                    model.ConnectionString);

                if (!dbSuccess)
                {
                    model.HasError = true;
                    model.ErrorMessage = $"Lỗi phân tích database: {dbMessage}";
                    return View("Index", model);
                }

                // Tạo câu truy vấn SQL từ câu hỏi với cơ chế retry
                string query = string.Empty;
                string explanation = string.Empty;
                bool queryGenerated = false;
                int retryCount = 0;

                while (!queryGenerated && retryCount < MaxRetries)
                {
                    try
                    {
                        (query, explanation) = await _aiService.GenerateSqlQuery(model.Question, dbInfo);
                        queryGenerated = true;
                    }
                    catch (Exception ex)
                    {
                        _logger.LogWarning(ex, $"Lần thử {retryCount + 1} tạo câu truy vấn thất bại");
                        retryCount++;
                        if (retryCount >= MaxRetries) throw;
                        await Task.Delay(1000 * retryCount); // Exponential backoff
                    }
                }

                // Thực thi truy vấn
                var (querySuccess, queryMessage, data) = await _databaseService.ExecuteQuery(
                    model.DatabaseType,
                    model.ConnectionString,
                    query);

                if (!querySuccess)
                {
                    model.HasError = true;
                    model.ErrorMessage = $"Lỗi thực thi truy vấn: {queryMessage}";
                    return View("Index", model);
                }

                if (data == null || data.Rows.Count == 0)
                {
                    model.Answer = "Không tìm thấy dữ liệu phù hợp với câu hỏi của bạn.";
                    return View("Index", model);
                }

                // Phân tích kết quả
                string analysis = await _aiService.AnalyzeQueryResult(model.Question, data);

                // Tạo đề xuất biểu đồ nếu có yêu cầu
                string chartType = null;
                string chartData = null;

                if (model.ChartType != null || ShouldGenerateChart(model.Question))
                {
                    try
                    {
                        (chartType, chartData) = await _aiService.GenerateChartSuggestion(data, analysis);
                    }
                    catch (Exception ex)
                    {
                        _logger.LogWarning(ex, "Không thể tạo biểu đồ");
                        // Không throw exception vì đây không phải lỗi nghiêm trọng
                    }
                }

                // Tạo câu trả lời chi tiết
                var answer = new StringBuilder();
                answer.AppendLine("📊 Kết quả phân tích của bạn:");
                answer.AppendLine();
                answer.AppendLine("🔍 Câu truy vấn SQL:");
                answer.AppendLine(query);
                answer.AppendLine();
                answer.AppendLine("💡 Giải thích:");
                answer.AppendLine(explanation);
                answer.AppendLine();
                answer.AppendLine("📈 Phân tích kết quả:");
                answer.AppendLine(analysis);

                if (data.Rows.Count > 10)
                {
                    answer.AppendLine();
                    answer.AppendLine($"ℹ️ Hiển thị 10/{data.Rows.Count} dòng dữ liệu:");
                    answer.AppendLine(FormatDataTablePreview(data, 10));
                }

                // Cập nhật model
                model.Answer = answer.ToString();
                model.ChartType = chartType;
                model.ChartData = chartData;
                model.HasError = false;

                return View("Index", model);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi xử lý câu hỏi");
                model.HasError = true;
                model.ErrorMessage = "Có lỗi xảy ra khi xử lý câu hỏi của bạn. Vui lòng thử lại sau.";
                return View("Index", model);
            }
        }

        [HttpPost]
        public async Task<IActionResult> AnalyzeDatabase(ChatViewModel model)
        {
            if (string.IsNullOrEmpty(model.DatabaseType) || string.IsNullOrEmpty(model.ConnectionString))
            {
                ModelState.AddModelError("", "Vui lòng nhập đầy đủ thông tin cơ sở dữ liệu");
                return View("Index", model);
            }

            try
            {
                // Kiểm tra kết nối trước
                var (connectionSuccess, connectionMessage) = await _databaseService.TestConnection(
                    model.DatabaseType,
                    model.ConnectionString);

                if (!connectionSuccess)
                {
                    model.HasError = true;
                    model.ErrorMessage = $"Lỗi kết nối database: {connectionMessage}";
                    return View("Index", model);
                }

                var (success, message, info) = await _databaseService.AnalyzeDatabase(
                    model.DatabaseType,
                    model.ConnectionString);

                if (!success)
                {
                    model.HasError = true;
                    model.ErrorMessage = message;
                    return View("Index", model);
                }

                // Tạo phân tích tổng quan về database
                var analysis = new StringBuilder();
                var tables = info["tables"] as List<dynamic>;

                analysis.AppendLine("📊 Phân tích cơ sở dữ liệu của bạn");
                analysis.AppendLine();
                analysis.AppendLine($"📌 Tổng số bảng: {tables.Count}");
                analysis.AppendLine();

                foreach (var table in tables)
                {
                    analysis.AppendLine($"📋 Bảng: {table.TableName}");
                    analysis.AppendLine($"   ├─ Số cột: {table.ColumnCount}");
                    analysis.AppendLine($"   ├─ Số cột nullable: {table.NullableCount}");

                    if (info.ContainsKey($"columns_{table.TableName}"))
                    {
                        var columns = info[$"columns_{table.TableName}"] as List<dynamic>;
                        analysis.AppendLine("   └─ Cấu trúc:");
                        foreach (var column in columns)
                        {
                            var isPk = column.IsPrimaryKey == true;
                            var isNullable = column.IsNullable?.ToString()?.ToUpper() == "YES";
                            analysis.AppendLine($"      {(isPk ? "🔑" : "•")} {column.ColumnName}");
                            analysis.AppendLine($"        └─ Kiểu: {column.DataType}{(isNullable ? " (nullable)" : "")}");
                        }
                    }
                    analysis.AppendLine();
                }

                // Thêm gợi ý sử dụng
                analysis.AppendLine("💡 Gợi ý sử dụng:");
                analysis.AppendLine("1. Bạn có thể hỏi các câu hỏi bằng tiếng Việt về dữ liệu trong database");
                analysis.AppendLine("2. Hệ thống sẽ tự động tạo câu truy vấn SQL phù hợp");
                analysis.AppendLine("3. Kết quả sẽ được phân tích và hiển thị dưới dạng văn bản và biểu đồ (nếu phù hợp)");

                model.Answer = analysis.ToString();
                model.HasError = false;
                return View("Index", model);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi phân tích database");
                model.HasError = true;
                model.ErrorMessage = "Có lỗi xảy ra khi phân tích database. Vui lòng thử lại sau.";
                return View("Index", model);
            }
        }

        [HttpPost]
        public async Task<IActionResult> CheckConnection(ChatViewModel model)
        {
            if (string.IsNullOrEmpty(model.DatabaseType) || string.IsNullOrEmpty(model.ConnectionString))
            {
                ModelState.AddModelError("", "Vui lòng nhập đầy đủ thông tin cơ sở dữ liệu");
                return View("Index", model);
            }

            try
            {
                var (success, message) = await _databaseService.TestConnection(
                    model.DatabaseType,
                    model.ConnectionString);

                model.HasError = !success;
                model.Answer = success
                    ? "✅ Kết nối thành công! Bạn có thể bắt đầu truy vấn database."
                    : $"❌ Lỗi kết nối: {message}";
                return View("Index", model);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi kiểm tra kết nối");
                model.HasError = true;
                model.ErrorMessage = "Có lỗi xảy ra khi kiểm tra kết nối. Vui lòng thử lại sau.";
                return View("Index", model);
            }
        }

        private bool ShouldGenerateChart(string question)
        {
            var keywords = new[] { "biểu đồ", "chart", "thống kê", "visualization", "trực quan", "graph" };
            return keywords.Any(k => question.ToLower().Contains(k));
        }

        private string FormatDataTablePreview(System.Data.DataTable data, int rowCount)
        {
            var preview = new StringBuilder();

            // Header
            foreach (System.Data.DataColumn col in data.Columns)
            {
                preview.Append($"{col.ColumnName}\t");
            }
            preview.AppendLine();

            // Separator
            foreach (System.Data.DataColumn col in data.Columns)
            {
                preview.Append(new string('-', col.ColumnName.Length) + "\t");
            }
            preview.AppendLine();

            // Data
            for (int i = 0; i < Math.Min(rowCount, data.Rows.Count); i++)
            {
                foreach (var item in data.Rows[i].ItemArray)
                {
                    preview.Append($"{item}\t");
                }
                preview.AppendLine();
            }

            return preview.ToString();
        }
    }
}
