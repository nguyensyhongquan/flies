using Newtonsoft.Json;
using System.Data;

namespace FliesProject.Services
{
    public class ChatAnalyzerService : IChatAnalyzerService
    {
        public async Task<(string answer, string chartData, string chartType)> ProcessQuestion(string question, DataTable data)
        {
            try
            {
                // TODO: Tích hợp với OpenAI hoặc model ML khác để xử lý câu hỏi
                // Hiện tại trả về phân tích đơn giản
                var answer = AnalyzeDataTable(data);
                var (chartData, chartType) = GenerateChartData(data);

                return (answer, chartData, chartType);
            }
            catch (Exception ex)
            {
                return ($"Lỗi xử lý câu hỏi: {ex.Message}", null, null);
            }
        }

        public async Task<string> AnalyzeData(Dictionary<string, object> databaseInfo)
        {
            try
            {
                var tables = databaseInfo["tables"] as List<dynamic>;
                var analysis = new System.Text.StringBuilder();
                analysis.AppendLine("Phân tích cơ sở dữ liệu:");
                analysis.AppendLine();

                foreach (var table in tables)
                {
                    analysis.AppendLine($"Bảng: {table.TableName}");
                    analysis.AppendLine($"- Số cột: {table.ColumnCount}");
                    analysis.AppendLine($"- Số cột nullable: {table.NullableCount}");
                    if (table.GetType().GetProperty("IdentityCount") != null)
                    {
                        analysis.AppendLine($"- Số cột identity: {table.IdentityCount}");
                    }
                    analysis.AppendLine();
                }

                return analysis.ToString();
            }
            catch (Exception ex)
            {
                return $"Lỗi phân tích dữ liệu: {ex.Message}";
            }
        }

        private string AnalyzeDataTable(DataTable data)
        {
            var analysis = new System.Text.StringBuilder();
            analysis.AppendLine($"Kết quả phân tích:");
            analysis.AppendLine($"- Số cột: {data.Columns.Count}");
            analysis.AppendLine($"- Số dòng: {data.Rows.Count}");

            foreach (DataColumn column in data.Columns)
            {
                analysis.AppendLine($"- Cột {column.ColumnName}: {column.DataType.Name}");
            }

            return analysis.ToString();
        }

        private (string chartData, string chartType) GenerateChartData(DataTable data)
        {
            if (data.Columns.Count < 2) return (null, null);

            // Tạo dữ liệu cho biểu đồ cột đơn giản
            var chartData = new
            {
                labels = data.AsEnumerable().Select(r => r[0].ToString()).ToArray(),
                datasets = new[]
                {
                    new
                    {
                        label = data.Columns[1].ColumnName,
                        data = data.AsEnumerable().Select(r => Convert.ToDouble(r[1])).ToArray(),
                        backgroundColor = "rgba(75, 192, 192, 0.2)",
                        borderColor = "rgba(75, 192, 192, 1)",
                        borderWidth = 1
                    }
                }
            };

            return (JsonConvert.SerializeObject(chartData), "bar");
        }
    }
}
