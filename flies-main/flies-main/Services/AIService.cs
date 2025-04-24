using FliesProject.AIBot;
using FliesProject.AIBot.APIModels.API_Response.Success;
using FliesProject.AIBot.Helpers;
using Microsoft.Extensions.Configuration;
using Models.Request;
using System.Data;
using System.Text;
using System.Text.RegularExpressions;

namespace FliesProject.Services
{
    public class AIService : IAIService
    {
        private readonly Generator _generator;
        private readonly ILogger<AIService> _logger;

        public AIService(IConfiguration configuration, ILogger<AIService> logger)
        {
            var apiKey = configuration["Gemini:ApiKey"];
            _generator = new Generator(apiKey);
            _logger = logger;
        }

        public async Task<(string query, string explanation)> GenerateSqlQuery(string question, Dictionary<string, object> databaseInfo)
        {
            try
            {
                var tables = databaseInfo["tables"] as List<dynamic>;
                var schema = new StringBuilder();

                // Xây dựng thông tin schema
                foreach (var table in tables)
                {
                    schema.AppendLine($"Table: {table.TableName}");
                    if (databaseInfo.ContainsKey($"columns_{table.TableName}"))
                    {
                        var columns = databaseInfo[$"columns_{table.TableName}"] as List<dynamic>;
                        foreach (var column in columns)
                        {
                            schema.AppendLine($"- {column.ColumnName} ({column.DataType}){(column.IsPrimaryKey == 1 ? " [PK]" : "")}");
                            Console.WriteLine("THe colum is "+column);
                        }
                    }
                    schema.AppendLine();
                }

                var prompt = $@"You are a SQL Server expert. Your task is to:
1. Analyze the user's question
2. Create an appropriate SQL query for sql server
3. Explain how the query works

Database schema:
{schema}

Question: {question}

Please create a SQL query and explain how it works. Return in the format :
QUERY: <query>
EXPLANATION: <explanation>";

                var request = new ApiRequestBuilder()
                    .WithPrompt(prompt)
                    .WithDefaultGenerationConfig()
                    .Build();
                Console.WriteLine("the request is"+request);

                var response = await _generator.GenerateContentAsync(request);
                var content = response.Result;
                Console.WriteLine(content);
                //var query = content.Split("QUERY:", StringSplitOptions.RemoveEmptyEntries)[1]
                //    .Split("EXPLANATION:", StringSplitOptions.RemoveEmptyEntries)[0].Trim();
                //Console.WriteLine("the query is"+query);
                //var explanation = content.Split("EXPLANATION:", StringSplitOptions.RemoveEmptyEntries)[1].Trim();
                var match = Regex.Match(content,
    @"QUERY:\s*```sql\s*(?<sql>[\s\S]*?)\s*```\s*EXPLANATION:\s*(?<exp>[\s\S]*)",
    RegexOptions.IgnoreCase);

                if (!match.Success)
                {
                    _logger.LogError("Cannot parse AI response:\n{0}", content);
                    throw new InvalidOperationException("Unexpected AI response format");
                }

                var query = match.Groups["sql"].Value.Trim();
                Console.WriteLine("the query is");
                var explanation = match.Groups["exp"].Value.Trim();
                return (query, explanation);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error generating SQL query");
                throw;
            }
        }

        public async Task<string> AnalyzeQueryResult(string question, DataTable result)
        {
            try
            {
                var data = new StringBuilder();
                data.AppendLine("Data:");

                // Convert DataTable to text format
                foreach (DataColumn col in result.Columns)
                {
                    data.Append($"{col.ColumnName}\t");
                }
                data.AppendLine();

                foreach (DataRow row in result.Rows)
                {
                    foreach (var item in row.ItemArray)
                    {
                        data.Append($"{item}\t");
                    }
                    data.AppendLine();
                }

                var prompt = $@"You are a data analysis expert. 
Your task is to analyze the query results and provide valuable insights.
Please respond in English, concisely and clearly.

Question: {question}

{data}

Please analyze the data above and provide important insights.";

                var request = new ApiRequestBuilder()
                    .WithPrompt(prompt)
                    .WithDefaultGenerationConfig()
                    .Build();

                var response = await _generator.GenerateContentAsync(request);
                return response.Result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error analyzing query result");
                throw;
            }
        }

        public async Task<(string chartType, string chartData)> GenerateChartSuggestion(DataTable data, string analysis)
        {
            try
            {
                var dataDescription = new StringBuilder();
                dataDescription.AppendLine("Columns:");
                foreach (DataColumn col in data.Columns)
                {
                    dataDescription.AppendLine($"- {col.ColumnName} ({col.DataType.Name})");
                }

                var prompt = $@"You are a data visualization expert.
Your task is to suggest the most suitable chart type to display the data.If the analysis is just a list and you feel that there is no comparison here, you can completely answer no.
Choose only one of the following chart types: bar, line, pie, scatter.
Return in JSON format with 2 fields: chartType and chartData.

Data:
{dataDescription}

Analysis:
{analysis}

Please suggest the most appropriate chart type and create the data structure for that chart.";

                var request = new ApiRequestBuilder()
                    .WithPrompt(prompt)
                    .WithDefaultGenerationConfig()
                    .Build();

                var response = await _generator.GenerateContentAsync<ChartSuggestion>(request);
                return (response.chartType, response.chartData);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error generating chart suggestion");
                throw;
            }
        }
        public async Task<T> GenerateContentAsync<T>(ApiRequest request)
        {
            try
            {
                var response = await _generator.GenerateContentAsync<T>(request);
                return response;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error generating generic content");
                throw;
            }
        }

        public async Task<string> GenerateSmoothAnswer(
        string sqlQuery,
        string explanation,
        string analysis,
        string question,
        string rawDataText)
        {
            // 1. Tạo prompt để AI “làm mượt” câu trả lời
            var prompt = $@"
            You are a professional AI assistant.
            Your task is to combine the following parts into a single, fluent, user-friendly response in Vietnamese:

            - The user's original question: ""{question}""
            - The SQL query used: {sqlQuery}
            - A brief explanation of what the query does: {explanation}
            - The raw data returned (in rows, separated by dashes):
            {rawDataText}
            - A brief insight from the query results: {analysis}

            Output only one paragraph (2–4 sentences) that:

            1.Answer a summary of the answer through the retrieved rawDataText.

            2.Answer the question in its entirety and to the point
            3.Not showing the query you need to use, just answer the result
            4.Không cần show ra cách truy vấn , câu lệnh , user họ chỉ cần kết quả thôi họ không cần bạn showw quá trình nhé!
            5.Hãy show ra thông tin 1 cách chi tiết thông qua từ {rawDataText} không phải dấu diếm gì cả ,khồng cần trả lời 1 cách khái quát làm gì ok !
            6.Khi public các thông tin từ {rawDataText} thì chỉ nên public những thông tin mang tính tiên quyết như email , tên ,địa chỉ ,số điện thoại ( dành cho việc xác định danh tính 1 người )
            ";


            // 2. Build the API request
            var request = new ApiRequestBuilder()
                .WithPrompt(prompt)
                .WithDefaultGenerationConfig()
                .Build();

            // 3. Call the generator to get a smooth answer
            try
            {
                Console.WriteLine("The rawdatatttttttttttttttttttttttttttttttttttttttttttttttttttttttt is" + rawDataText);
                var smoothAnswer = await _generator.GenerateContentAsync(request);
                return smoothAnswer.Result.Trim();
               
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error generating smooth answer");
                // Fallback nếu AI fail
                var fallback = new StringBuilder();
                fallback.Append($"Để trả lời câu hỏi, tôi đã dùng truy vấn \"{sqlQuery}\". ");
                fallback.Append($"{explanation} ");
                fallback.Append($"Kết quả (dữ liệu): {analysis}. ");
                fallback.Append("Bạn có thể xem biểu đồ bên dưới để rõ hơn.");
                return fallback.ToString();
            }
        }

    }

    public class ChartSuggestion
        {
            public string chartType { get; set; }
            public string chartData { get; set; }
        }
    }

