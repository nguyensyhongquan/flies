using System.Data;

namespace FliesProject.Services
{
    public interface IAIService
    {
        Task<(string query, string explanation)> GenerateSqlQuery(string question, Dictionary<string, object> databaseInfo);
        Task<string> AnalyzeQueryResult(string question, DataTable result);
        Task<(string chartType, string chartData)> GenerateChartSuggestion(DataTable data, string analysis);
    }
}
