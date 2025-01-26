using MySqlConnector;

namespace AspBBS.Service
{
    public class DeleteService
    {
        private readonly string _connectionString;

        public DeleteService(string connectionString)
        {
            _connectionString = connectionString;
        }

        public bool DeleteText(string tableName, int id, string userName, string email)
        {
            if (!TableExists(tableName)) return false;

            using (var connection = new MySqlConnection(_connectionString))
            {
                connection.Open();

                string query = @$"DELETE FROM {tableName} WHERE Id = @Id AND Username = @Username AND Email = @Email";

                using (var command = new MySqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@Id", id);
                    command.Parameters.AddWithValue("@Username", userName);
                    command.Parameters.AddWithValue("@Email", email);

                    int rowsAffected = command.ExecuteNonQuery();
                    return rowsAffected > 0; 
                }
            }
        }

        public bool IsUsernameAndEmailMatch(string tableName, int id, string username, string email)
        {
            if (!TableExists(tableName)) return false;

            using (var connection = new MySqlConnection(_connectionString))
            {
                connection.Open();

                // 특정 ID에서 Username과 Email을 확인하는 쿼리
                string query = @$"SELECT COUNT(*) 
                          FROM {tableName} 
                          WHERE Id = @Id AND Username = @Username AND Email = @Email";

                using (var command = new MySqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@Id", id);
                    command.Parameters.AddWithValue("@Username", username);
                    command.Parameters.AddWithValue("@Email", email);

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
