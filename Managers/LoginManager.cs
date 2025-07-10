using Microsoft.Data.SqlClient;
using SimpleHashing.Net;
using WDT_assignment_1.Models;

namespace WDT_assignment_1.Managers
{
    public class LoginManager
    {
        private readonly string _connectionString;

        public LoginManager(string connectionString)
        {
            _connectionString = connectionString;
        }

        public void InsertDBLogin(Models.Login login)
        {
            using var connection = new SqlConnection(_connectionString);
            connection.Open();

            // paramaterised sql
            using var command = connection.CreateCommand();
            command.CommandText =
                @"insert into Login (LoginID, CustomerID, PasswordHash) 
                values (@loginID, @customerID, @passwordHash)";
            command.Parameters.AddWithValue("loginID", login.LoginID);
            command.Parameters.AddWithValue("customerID", login.CustomerID);
            command.Parameters.AddWithValue("passwordHash", login.PasswordHash);

            command.ExecuteNonQuery();
        }

        // returns true if user login is valid
        public bool ValidateLogin(string loginId, string password)
        {
            using var connection = new SqlConnection(_connectionString);
            connection.Open();

            // gets password hash from table 
            using var command = connection.CreateCommand();
            command.CommandText = "select PasswordHash from Login where LoginID = @loginID";
            command.Parameters.AddWithValue("loginID", loginId);

            string hash = command.ExecuteScalar().ToString();

            // validate password
            if (new SimpleHash().Verify(password, hash))
            {
                return true;
            }
            // if user enters invalid credentials
            else
            { 
                return false;
            }
        }
    }
}
