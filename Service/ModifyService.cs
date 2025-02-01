using MySqlConnector;

namespace AspBBS.Service
{
    public class ModifyService
    {
        private readonly string _connectionString;

        public ModifyService(string connectionString)
        {
            _connectionString = connectionString;
        }

        public bool UpdateText(string tableName, int id, string newTitle, string newText)
        {
            if (!TableExists(tableName)) return false;

            using (var connection = new MySqlConnection(_connectionString))
            {
                connection.Open();

                // 업데이트 쿼리
                string query = @$"UPDATE {tableName} 
                          SET Title = @Title, Texts = @Text 
                          WHERE Id = @Id";

                using (var command = new MySqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@Title", newTitle);
                    command.Parameters.AddWithValue("@Text", newText);
                    command.Parameters.AddWithValue("@Id", id);

                    int rowsAffected = command.ExecuteNonQuery();
                    return rowsAffected > 0;
                }
            }
        }

        public bool IsUserIDMatch(string tableName, int id, string userID)
        {
            if (!TableExists(tableName)) return false;

            using (var connection = new MySqlConnection(_connectionString))
            {
                connection.Open();

                string query = @$"SELECT COUNT(*) 
                          FROM {tableName} 
                          WHERE Id = @Id AND UserID = @UserID";

                using (var command = new MySqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@Id", id);
                    command.Parameters.AddWithValue("@UserID", userID);

                    int count = Convert.ToInt32(command.ExecuteScalar());
                    return count > 0;
                }
            }
        }

        public bool TableExists(string tableName)
        {
            using (var connection = new MySqlConnection(_connectionString))
            {
                connection.Open();

                const string query = @"SELECT 1 
                                   FROM INFORMATION_SCHEMA.TABLES 
                                   WHERE TABLE_NAME = @TableName";
                using (var command = new MySqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@TableName", tableName);
                    return command.ExecuteScalar() != null;
                }
            }
        }

    }
}
