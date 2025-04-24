using System.Data;
using Microsoft.Data.SqlClient;
using Npgsql;
using MySql.Data.MySqlClient;
using Dapper;
using System.Data.Common;
using System.Configuration;

namespace FliesProject.Services
{
    public class DatabaseService : IDatabaseService
    {
        private readonly ILogger<DatabaseService> _logger;
        private readonly string _defaultConnectionString;
        public DatabaseService(ILogger<DatabaseService> logger, IConfiguration configuration)
        {
            _logger = logger;
            _defaultConnectionString = configuration.GetConnectionString("DefaultConnection");
        }

        public async Task<(bool success, string message)> TestConnection(string databaseType, string connectionString)
        {
            try
            {
                using var connection = CreateConnection("sqlserver", _defaultConnectionString);
                if (connection is DbConnection dbConnection)
                {
                    await dbConnection.OpenAsync();
                }
                else
                {
                    connection.Open();
                }
                return (true, "Kết nối thành công!");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi kết nối database");
                return (false, $"Lỗi kết nối: {ex.Message}");
            }
        }

        public async Task<(bool success, string message, DataTable data)> ExecuteQuery(string databaseType, string connectionString, string query)
        {
            try
            {
                using var connection = CreateConnection("sqlserver", _defaultConnectionString);

                if (connection is DbConnection dbConnection)
                {
                    await dbConnection.OpenAsync();
                }
                else
                {
                    connection.Open();
                }

                // Kiểm tra xem query có phải là SELECT không
                if (!query.Trim().ToUpper().StartsWith("SELECT"))
                {
                    return (false, "Chỉ hỗ trợ câu lệnh SELECT", null);
                }

                var result = await connection.QueryAsync(query);
                var dataTable = new DataTable();

                var first = result.FirstOrDefault();
                if (first != null)
                {
                    // Tạo cột cho DataTable
                    foreach (var property in ((IDictionary<string, object>)first).Keys)
                    {
                        dataTable.Columns.Add(property);
                    }

                    // Thêm dữ liệu vào DataTable
                    foreach (var row in result)
                    {
                        var dataRow = dataTable.NewRow();
                        foreach (var property in ((IDictionary<string, object>)row).Keys)
                        {
                            dataRow[property] = ((IDictionary<string, object>)row)[property] ?? DBNull.Value;
                        }
                        dataTable.Rows.Add(dataRow);
                    }
                }

                return (true, "Truy vấn thành công!", dataTable);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi thực thi truy vấn");
                return (false, $"Lỗi thực thi truy vấn: {ex.Message}", null);
            }
        }

        public async Task<(bool success, string message, Dictionary<string, object> info)> AnalyzeDatabase(string databaseType, string connectionString)
        {
            try
            {
                using var connection = CreateConnection(databaseType, connectionString);
                if (connection is DbConnection dbConnection)
                {
                    await dbConnection.OpenAsync();
                }
                else
                {
                    connection.Open();
                }

                var info = new Dictionary<string, object>();
                string query = GetDatabaseInfoQuery(databaseType);

                var tables = await connection.QueryAsync(query);
                info["tables"] = tables.ToList();

                // Thêm thông tin chi tiết về cột
                foreach (var table in tables)
                {
                    string columnQuery = GetColumnInfoQuery(databaseType, table.TableName.ToString());
                    var columns = await connection.QueryAsync(columnQuery);
                    info[$"columns_{table.TableName}"] = columns.ToList();
                }

                return (true, "Phân tích database thành công!", info);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi phân tích database");
                return (false, $"Lỗi phân tích database: {ex.Message}", null);
            }
        }

        private IDbConnection CreateConnection(string databaseType, string connectionString)
        {
            return databaseType.ToLower() switch
            {
                "sqlserver" => new SqlConnection(connectionString),
                "postgresql" => new NpgsqlConnection(connectionString),
                "mysql" => new MySqlConnection(connectionString),
                _ => throw new ArgumentException("Loại database không được hỗ trợ")
            };
        }

        private string GetDatabaseInfoQuery(string databaseType)
        {
            return databaseType.ToLower() switch
            {
                "sqlserver" => @"
                    SELECT 
                        t.name AS TableName,
                        COUNT(c.name) AS ColumnCount,
                        SUM(CASE WHEN c.is_identity = 1 THEN 1 ELSE 0 END) AS IdentityCount,
                        SUM(CASE WHEN c.is_nullable = 1 THEN 1 ELSE 0 END) AS NullableCount
                    FROM sys.tables t
                    JOIN sys.columns c ON t.object_id = c.object_id
                    GROUP BY t.name",

                "postgresql" => @"
                    SELECT 
                        table_name AS TableName,
                        COUNT(*) AS ColumnCount,
                        SUM(CASE WHEN is_nullable = 'YES' THEN 1 ELSE 0 END) AS NullableCount
                    FROM information_schema.columns
                    WHERE table_schema = 'public'
                    GROUP BY table_name",

                "mysql" => @"
                    SELECT 
                        TABLE_NAME AS TableName,
                        COUNT(*) AS ColumnCount,
                        SUM(CASE WHEN IS_NULLABLE = 'YES' THEN 1 ELSE 0 END) AS NullableCount
                    FROM information_schema.columns
                    WHERE table_schema = DATABASE()
                    GROUP BY TABLE_NAME",

                _ => throw new ArgumentException("Loại database không được hỗ trợ")
            };
        }

        private string GetColumnInfoQuery(string databaseType, string tableName)
        {
            return databaseType.ToLower() switch
            {
                "sqlserver" => $@"
                    SELECT 
                        c.name AS ColumnName,
                        t.name AS DataType,
                        c.max_length AS MaxLength,
                        c.is_nullable AS IsNullable,
                        c.is_identity AS IsIdentity,
                        CASE WHEN pk.object_id IS NOT NULL THEN 1 ELSE 0 END AS IsPrimaryKey
                    FROM sys.columns c
                    JOIN sys.types t ON c.user_type_id = t.user_type_id
                    JOIN sys.tables tbl ON c.object_id = tbl.object_id
                    LEFT JOIN sys.index_columns ic ON ic.object_id = c.object_id AND ic.column_id = c.column_id
                    LEFT JOIN sys.indexes pk ON pk.object_id = ic.object_id AND pk.is_primary_key = 1
                    WHERE tbl.name = '{tableName}'",

                "postgresql" => $@"
                    SELECT 
                        column_name AS ColumnName,
                        data_type AS DataType,
                        character_maximum_length AS MaxLength,
                        is_nullable AS IsNullable,
                        CASE WHEN pk.contype = 'p' THEN true ELSE false END AS IsPrimaryKey
                    FROM information_schema.columns c
                    LEFT JOIN pg_constraint pk 
                        ON pk.conrelid = (SELECT oid FROM pg_class WHERE relname = '{tableName}')
                        AND pk.contype = 'p' 
                        AND c.ordinal_position = ANY(pk.conkey)
                    WHERE table_name = '{tableName}'
                    AND table_schema = 'public'",

                "mysql" => $@"
                    SELECT 
                        COLUMN_NAME AS ColumnName,
                        DATA_TYPE AS DataType,
                        CHARACTER_MAXIMUM_LENGTH AS MaxLength,
                        IS_NULLABLE AS IsNullable,
                        COLUMN_KEY AS ColumnKey
                    FROM information_schema.columns
                    WHERE table_schema = DATABASE()
                    AND table_name = '{tableName}'",

                _ => throw new ArgumentException("Loại database không được hỗ trợ")
            };
        }
    }
}
