using System.Configuration;
using System.Data;
using System.Reflection.Emit;
using System.Text;
using FliesProject.AIBot;

namespace FliesProject.Services
{
    public interface IChatModule
    {
        Task<string> ProcessQuestion(string question);
    }
    public class DatabaseChatModule : IChatModule
    {
        private readonly Generator _generator;
        private readonly IAIService _aiService;
        private readonly IDatabaseService _databaseService;
        private readonly ILogger<DatabaseChatModule> _logger;

        // Các tham số cấu hình liên quan đến database
        private readonly string _databaseType;
        private readonly string _connectionString;

        /// <summary>
        /// Constructor nhận vào các dependency cần thiết: AIService, DatabaseService, Logger và cấu hình liên quan đến database.
        /// </summary>
        public DatabaseChatModule(
            IAIService aiService,
            IDatabaseService databaseService,
            ILogger<DatabaseChatModule> logger,
             IConfiguration configuration
         )    // Chuỗi kết nối đến database của bạn
        {
            _aiService = aiService;
            _databaseService = databaseService;
            _logger = logger;
            _databaseType = "sqlserver";
            _connectionString = configuration.GetConnectionString("DefaultConnection");
        }

        public async Task<string> ProcessQuestion(string question)
        {
            Console.WriteLine("this task is for process question");
            try
            {
                // Bước 1: Phân tích database để lấy schema (danh sách bảng, cột, vv).
                var (analyzeSuccess, analyzeMsg, schemaInfo) = await _databaseService.AnalyzeDatabase(_databaseType, _connectionString);
                if (!analyzeSuccess)
                {
                    return $"Lỗi khi phân tích database: {analyzeMsg}";
                }

                // Bước 2: Sinh câu truy vấn SQL cùng lời giải thích dựa trên câu hỏi và schema
                var (sqlQuery, explanation) = await _aiService.GenerateSqlQuery(question, schemaInfo);

                // Bước 3: Thực thi truy vấn SQL vừa tạo
                var (querySuccess, queryMsg, queryResult) = await _databaseService.ExecuteQuery(_databaseType, _connectionString, sqlQuery);
                if (!querySuccess)
                {
                    return $"Xin lỗi thưa đại ca hình như hiện tại bên trong website không có data như vậy cho mình có thể show cho bạn được xin lỗi bạn nhé!: {queryMsg}";
                }
                Console.WriteLine("the query is"+querySuccess);
                var rawBuilder = new StringBuilder();
                foreach (DataRow row in queryResult.Rows)
                {
                    var values = row.ItemArray
                                    .Select(v => v?.ToString() ?? "NULL");
                    rawBuilder.AppendLine(string.Join(" | ", values));
                    Console.WriteLine(string.Join(" | ", values));
                }
                var rawDataText = rawBuilder.ToString();
                Console.WriteLine("The raw datatext la "+rawDataText);
                // Bước 4: Phân tích kết quả truy vấn để đưa ra nhận định bổ sung
                var analysis = await _aiService.AnalyzeQueryResult(question, queryResult);
                var chartSuggestion = await _aiService.GenerateChartSuggestion(queryResult, analysis);
                Console.WriteLine("The suggestionnnnnnnnnnnnnnnnnnnnnnnnnnnnnnnnnnnnnn is "+chartSuggestion);
                string charsugges=chartSuggestion.ToString();
                var answer = await _aiService.GenerateSmoothAnswer(sqlQuery, explanation, analysis, question,rawDataText,charsugges);
                // (Tùy chọn) Nếu cần, có thể gợi ý biểu đồ từ kết quả truy vấn bằng cách gọi GenerateChartSuggestion

                // Ghép kết quả trả về thành phản hồi cho người dùng
                var responseBuilder = new StringBuilder();
                //responseBuilder.AppendLine("=== Kết quả truy vấn ===");
                //responseBuilder.AppendLine($"SQL Query: {sqlQuery}");
                //responseBuilder.AppendLine();
                //responseBuilder.AppendLine("=== Giải thích ===");
                //responseBuilder.AppendLine(explanation);
                //responseBuilder.AppendLine();
                //responseBuilder.AppendLine("=== Phân tích kết quả ===");
                //responseBuilder.AppendLine(analysis);
                responseBuilder.AppendLine("Bẩm đại ca :");
                responseBuilder.AppendLine(answer);
                return responseBuilder.ToString();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Có lỗi khi xử lý câu hỏi về database");
                return "Có lỗi xảy ra khi xử lý câu hỏi về database.";
            }
        }
    }
    public class GeneralChatModule : IChatModule
    {
        private readonly Generator _generator;
        private readonly IAIService _aiService;
        private readonly ILogger<GeneralChatModule> _logger;

        public GeneralChatModule(IAIService aiService, ILogger<GeneralChatModule> logger, Generator generator)
        {
            _aiService = aiService;
            _logger = logger;
            _generator = generator;
        }

        public async Task<string> ProcessQuestion(string question)
        {
            Console.WriteLine("this task is for processsssssssssssss  general question");
            try
            {
                // Tạo prompt chung cho AI để trả lời các câu hỏi tổng quát
                var prompt = $@"You are a friendly, knowledgeable AI assistant.
Your task is to answer the following question in a clear and helpful manner.

Question: {question}

Please provide a concise answer.";
                Console.WriteLine(prompt);

                // Xây dựng request sử dụng ApiRequestBuilder từ IAIService
                var request = new ApiRequestBuilder()
                    .WithPrompt(prompt)
                    .WithDefaultGenerationConfig()
                    .Build();

                // Gọi AIService để sinh câu trả lời theo prompt

                var response = await _generator.GenerateContentAsync(request);

                return response.Result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing general chat question");
                return "Có lỗi xảy ra khi xử lý câu hỏi của bạn.";
            }
        }
    }
}
