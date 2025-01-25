using System.Data;
using MySqlConnector;
using AspBBS.Models;

namespace AspBBS.Service
{
    public class DataService
    {
        private readonly MySqlConnection _connection;

        public DataService(string connectionString)
        {
            _connection = new MySqlConnection(connectionString);
        }

        public int GetTotalCount(string tableName, string queries)
        {
            if (!TableExists(tableName)) return 0;

            using (var command = _connection.CreateCommand())
            {
                if (_connection.State == ConnectionState.Open) _connection.Close();
                _connection.Open();
                string searchCondition = string.IsNullOrWhiteSpace(queries) ? "" : $"AND (Title LIKE @Queries OR Username LIKE @Queries)";
                command.CommandText = $"SELECT COUNT(*) FROM {tableName} WHERE Situation = 'public' {searchCondition}";
                command.Parameters.AddWithValue("@Queries", $"%{queries}%");
                int totalCount = Convert.ToInt32(command.ExecuteScalar());
                _connection.Close();
                return totalCount;
            }
        }

        public List<DataModel> GetListData(string tableName, string queries, int pageNumber, int pageSize = 50)
        {
            if (!TableExists(tableName)) return null!;
            
            using (var command = _connection.CreateCommand())
            {
                int startIndex = (pageNumber - 1) * pageSize;
                List<DataModel> dataList = new List<DataModel>();
                if (_connection.State == ConnectionState.Open) _connection.Close();
                _connection.Open();

                try
                {
                    string searchCondition = string.IsNullOrWhiteSpace(queries) ? "" : $"AND (Title LIKE @Queries OR Username LIKE @Queries)";
                    command.CommandText = $"SELECT * FROM {tableName} WHERE Situation = 'public' {searchCondition} ORDER BY Id DESC LIMIT {startIndex}, {pageSize}";
                    command.Parameters.AddWithValue("@Queries", $"%{queries}%");

                    using (var reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            DataModel data = new DataModel
                            {
                                Id = reader.GetInt32(reader.GetOrdinal("Id")),
                                Title = reader.GetString(reader.GetOrdinal("Title")),
                                Username = reader.GetString(reader.GetOrdinal("Username")),
                                CreatedAt = reader.GetString(reader.GetOrdinal("CreatedAt")),
                            };

                            dataList.Add(data);
                        }
                    }
                }
                catch
                {
                    if (_connection.State == ConnectionState.Open) _connection.Close();
                }

                return dataList;
            }
        }

        public DataModel GetDetailData(string tableName, int id)
        {
            if (!TableExists(tableName)) return null!;

            using (var command = _connection.CreateCommand())
            {
                if (_connection.State == ConnectionState.Open) _connection.Close();
                _connection.Open();
                try
                {
                    command.CommandText = $"SELECT * FROM {tableName} WHERE Id = {id} AND Situation = 'public'";

                    using (var reader = command.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            DataModel data = new DataModel
                            {
                                Id = reader.GetInt32(reader.GetOrdinal("Id")),
                                Title = reader.GetString(reader.GetOrdinal("Title")),
                                Username = reader.GetString(reader.GetOrdinal("Username")),
                                CreatedAt = reader.GetString(reader.GetOrdinal("CreatedAt")),
                                Text = reader.GetString(reader.GetOrdinal("Texts")),
                            };
                            _connection.Close();
                            return data;
                        }
                    }
                }
                catch
                {
                    if (_connection.State == ConnectionState.Open) _connection.Close();
                }

                return null!;
            }
        }

        private bool TableExists(string tableName)
        {
            using (var command = _connection.CreateCommand())
            {
                if (_connection.State == ConnectionState.Open) _connection.Close();
                _connection.Open();
                command.CommandText = $"SELECT 1 FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = @TableName";
                command.Parameters.AddWithValue("@TableName", tableName);
                var result = command.ExecuteScalar();
                _connection.Close();
                return result != null;
            }
        }
    }

}
