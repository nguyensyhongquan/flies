using Newtonsoft.Json;
using System.Data;

namespace FliesProject.Services
{
    public interface IChatAnalyzerService
    {
        Task<(string answer, string chartData, string chartType)> ProcessQuestion(string question, DataTable data);
        Task<string> AnalyzeData(Dictionary<string, object> databaseInfo);
    }

 
}
