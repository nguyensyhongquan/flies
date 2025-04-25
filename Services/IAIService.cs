using System.Data;
using FliesProject.AIBot.APIModels.API_Response.Success;
using Models.Request;

namespace FliesProject.Services
{
    public interface IAIService
    {
        Task<(string query, string explanation)> GenerateSqlQuery(string question, Dictionary<string, object> databaseInfo);
        Task<string> AnalyzeQueryResult(string question, DataTable result);
        Task<(string chartType, string chartData)> GenerateChartSuggestion(DataTable data, string analysis);
        Task<string> GenerateSmoothAnswer(string sqlQuery,string explanation,string analysis,string question,string rawdatatext,string suggestion);

    }
}
