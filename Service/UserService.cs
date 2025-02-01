using AspBBS.Models;
using MySqlConnector;

namespace AspBBS.Service
{
    public class UserService
    {
        private readonly string _connectionString;

        public UserService(string connectionString)
        {
            _connectionString = connectionString;
        }

        public bool RegisterUser(UserModel user, string ip, string createdAt, string userID)
        {
            using (MySqlConnection connection = new MySqlConnection(_connectionString))
            {
                connection.Open();

                string query = "INSERT INTO Users (Username, Email, Password, IP, CreatedAt, UserID) VALUES (@Username, @Email, @Password, @IP, @CreatedAt, @UserID)";
                MySqlCommand command = new MySqlCommand(query, connection);
                command.Parameters.AddWithValue("@Username", user.Username);
                command.Parameters.AddWithValue("@Email", user.Email);
                command.Parameters.AddWithValue("@Password", user.Password);
                command.Parameters.AddWithValue("@IP", ip);
                command.Parameters.AddWithValue("@CreatedAt", createdAt);
                command.Parameters.AddWithValue("@UserID", userID);

                int rowsAffected = command.ExecuteNonQuery();
                connection.Clone();
                return rowsAffected > 0;
            }
        }

        public UserModel AuthenticateUser(string email, string password)
        {
            using (MySqlConnection connection = new MySqlConnection(_connectionString))
            {
                connection.Open();

                string query = "SELECT * FROM Users WHERE Email = @Email AND Password = @Password";
                MySqlCommand command = new MySqlCommand(query, connection);
                command.Parameters.AddWithValue("@Email", email);
                command.Parameters.AddWithValue("@Password", password);

                MySqlDataReader reader = command.ExecuteReader();
                if (reader.Read())
                {
                    UserModel user = new UserModel
                    {
                        Id = reader.GetInt32("Id"),
                        Username = reader.GetString("Username"),
                        Email = reader.GetString("Email"),
                        Password = reader.GetString("Password"),
                        IP = reader.GetString("IP"),
                        CreatedAt = reader.GetString("CreatedAt"),
                        UserID = reader.GetString("UserID")
                    };
                    reader.Close();
                    return user;
                }
                reader.Close();
            }

            return null!;
        }


        public UserModel GetUserByUserID(string userID)
        {
            using (MySqlConnection connection = new MySqlConnection(_connectionString))
            {
                connection.Open();

                string query = "SELECT * FROM Users WHERE UserID = @UserID";
                MySqlCommand command = new MySqlCommand(query, connection);
                command.Parameters.AddWithValue("@UserID", userID);

                MySqlDataReader reader = command.ExecuteReader();
                if (reader.Read())
                {
                    UserModel user = new UserModel
                    {
                        Id = reader.GetInt32("Id"),
                        Username = reader.GetString("Username"),
                        Email = reader.GetString("Email"),
                        Password = reader.GetString("Password"),
                        IP = reader.GetString("IP"),
                        CreatedAt = reader.GetString("CreatedAt"),
                        UserID = reader.GetString("UserID")
                    };
                    reader.Close();
                    return user;
                }
                reader.Close();
            }

            return null!;
        }

        public bool IsEmailUnique(string email)
        {
            using (MySqlConnection connection = new MySqlConnection(_connectionString))
            {
                connection.Open();

                // 이메일 중복 확인
                string checkEmailQuery = "SELECT COUNT(*) FROM Users WHERE Email = @Email";
                MySqlCommand checkEmailCommand = new MySqlCommand(checkEmailQuery, connection);
                checkEmailCommand.Parameters.AddWithValue("@Email", email);

                int emailCount = Convert.ToInt32(checkEmailCommand.ExecuteScalar());
                return emailCount == 0;
            }
        }

        public bool IsUserIDUnique(string userID) 
        {
            using (MySqlConnection connection = new MySqlConnection(_connectionString))
            {
                connection.Open();

                string checkUserIDQuery = "SELECT COUNT(*) FROM Users WHERE UserID = @UserID";
                MySqlCommand checkUserIDCommand = new MySqlCommand(checkUserIDQuery, connection);
                checkUserIDCommand.Parameters.AddWithValue("@UserID", userID);

                int emailCount = Convert.ToInt32(checkUserIDCommand.ExecuteScalar());
                return emailCount == 0;
            }
        }
    }
}

