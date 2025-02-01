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

        public bool DeleteText(string tableName, int id, string userID)
        {
            if (!TableExists(tableName)) return false;

            using (var connection = new MySqlConnection(_connectionString))
            {
                connection.Open();

                string query = @$"UPDATE {tableName} SET Situation = 'private' WHERE Id = @Id AND UserID = @UserID";

                using (var command = new MySqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@Id", id);
                    command.Parameters.AddWithValue("@UserID", userID);

                    int rowsAffected = command.ExecuteNonQuery();
                    return rowsAffected > 0; 
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
