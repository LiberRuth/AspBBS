using AspBBS.Models;
using MySqlConnector;
using static System.Net.Mime.MediaTypeNames;

namespace AspBBS.Service
{
    public class WriteService
    {
        private readonly string _connectionString;

        public WriteService(string connectionString)
        {
            _connectionString = connectionString;
        }

        public bool TextWrite(string tableName, string title, string username, string email, 
            string text, string createdAt, string ip, string userID,string situation = "public")
        {
            if (!TableExists(tableName)) return false;

            using (var connection = new MySqlConnection(_connectionString))
            {
                connection.Open();

                string query = @$"INSERT INTO {tableName} (Title, Username, Email, Texts, CreatedAt, Situation, IP, UserID) 
                          VALUES (@Title, @Username, @Email, @Texts, @CreatedAt, @Situation, @IP, @UserID)";

                using (var command = new MySqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@Title", title);
                    command.Parameters.AddWithValue("@Username", username);
                    command.Parameters.AddWithValue("@Email", email);
                    command.Parameters.AddWithValue("@Texts", text);
                    command.Parameters.AddWithValue("@CreatedAt", createdAt);
                    command.Parameters.AddWithValue("@Situation", situation);
                    command.Parameters.AddWithValue("@IP", ip);
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
