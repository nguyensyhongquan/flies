

using System.Data;
using System.Data.SqlClient;
using Npgsql;
using MySql.Data.MySqlClient;
using Dapper;
namespace FliesProject.Services
{
    public interface IDatabaseService
    {
        Task<(bool success, string message)> TestConnection(string databaseType, string connectionString);
        Task<(bool success, string message, DataTable data)> ExecuteQuery(string databaseType, string connectionString, string query);
        Task<(bool success, string message, Dictionary<string, object> info)> AnalyzeDatabase(string databaseType, string connectionString);
    }


}
