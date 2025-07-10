using Microsoft.Data.SqlClient;

namespace WDT_assignment_1.Managers
{
    public class AccountManager
    {
        private readonly string _connectionString;

        public AccountManager(string connectionString)
        {
            _connectionString = connectionString;
        }

        public void InsertDBAccount(Models.Account acc)
        {
            using var connection = new SqlConnection(_connectionString);
            connection.Open();

            //paramaterised sql
            using var command = connection.CreateCommand();
            command.CommandText =
                @"insert into Account (AccountNumber, AccountType, CustomerID, Balance) 
                values (@accountNum, @accType, @customerID, @balance)";
            command.Parameters.AddWithValue("accountNum", acc.AccountNumber);
            command.Parameters.AddWithValue("accType", acc.AccountType);
            command.Parameters.AddWithValue("customerID", acc.CustomerID);
            command.Parameters.AddWithValue("balance", acc.Balance);

            command.ExecuteNonQuery();
        }

        // returns all accounts for specified customerid
        public List<int> GetCustomerAccounts(int customerId)
        {
            var accounts = new List<int>();

            using var connection = new SqlConnection(_connectionString);
            connection.Open();

            using var command = connection.CreateCommand();

            command.CommandText = "select AccountNumber from Account where CustomerID = @customerId";
            command.Parameters.AddWithValue("customerID", customerId);

            using (SqlDataReader reader = command.ExecuteReader())
            {
                // adds accounts from query to list
                while (reader.Read())
                {
                    accounts.Add(reader.GetInt32(0));
                }
            }
            return accounts;
        }

        // returns the type of account (checking/saving) for specified account
        public char GetAccountType(int account)
        {
            using var connection = new SqlConnection(_connectionString);
            connection.Open();

            using var command = connection.CreateCommand();
            command.CommandText = "select AccountType from Account where AccountNumber = @accountId";
            command.Parameters.AddWithValue("accountId", account);

            var type = char.Parse(command.ExecuteScalar().ToString());

            return type;
        }

        // returns true if account withdrawal is allowed
        public bool CanWithdraw(int account, decimal amount)
        {
            char type = GetAccountType(account);

            switch (type)
            {
                case 'C':
                    if (amount < 300)
                    {
                        return false;
                    }
                    break;
                case 'S':
                    if (amount < 0)
                    {
                        return false;
                    }
                    break;
            }
            return true;
        }

        // retrieves all accounts exccept for selected one
        public List<int> GetAccountsExcept(int excludeAccount)
        {
            var accounts = new List<int>();

            using var connection = new SqlConnection(_connectionString);
            connection.Open();

            using var command = connection.CreateCommand();

            command.CommandText = "select AccountNumber from Account where AccountNumber <> @exclude";
            command.Parameters.AddWithValue("exclude", excludeAccount);

            using (SqlDataReader reader = command.ExecuteReader())
            {
                // adds found accounts to list
                while (reader.Read())
                {
                    accounts.Add(reader.GetInt32(0));
                }
            }
            return accounts;

        }

        public void PrintBankStatement(int account, decimal balance)
        {
            // variables for paging ability
            var startingPage = 1;
            var pageSize = 4;

            // prints the bank statement for selected account
            while (true)
            {
                Console.Clear();

                Console.WriteLine($"\nSTATEMENT FOR ACCOUNT {account}"
                            + "\n=============================");
                Console.WriteLine($"BALANCE: ${balance:0.00}\n");
                Console.WriteLine("TRANSACTION HISTORY"
                                + "\n==============================");

                // prints transactions for selected account
                PrintAccountTransactions(account, startingPage, pageSize);

                // paging ability
                Console.WriteLine("[N] Next page, [P] Previous page");
                var key = Console.ReadKey(true).Key;
                if (key == ConsoleKey.N)
                    startingPage++;
                else if (key == ConsoleKey.P && startingPage > 1)
                    startingPage--;
            }
        }

        public void PrintAccountTransactions(int accountId, int startingPage, int pageSize)
        {
            using (SqlConnection connection = new SqlConnection(_connectionString))
            {
                connection.Open();

                string query = @"
                SELECT 
                    TransactionID, 
                    TransactionType, 
                    AccountNumber, 
                    DestinationAccountNumber, 
                    Amount, 
                    TransactionTimeUtc, 
                    Comment
                FROM 
                    [Transaction]
                WHERE 
                    AccountNumber = @AccountId
                ORDER BY 
                    TransactionTimeUtc DESC
                OFFSET @Offset ROWS
                FETCH NEXT @PageSize ROWS ONLY";

                SqlCommand command = new SqlCommand(query, connection);
                command.Parameters.AddWithValue("@AccountId", accountId);
                command.Parameters.AddWithValue("@Offset", (startingPage - 1) * pageSize);
                command.Parameters.AddWithValue("@PageSize", pageSize);

                SqlDataReader reader = command.ExecuteReader();

                // formatting for display table
                Console.WriteLine("{0,-14} | {1,-4} | {2,-12} | {3,-10} | {4,-9} | {5,-19} | {6}",
                    "Transaction ID", "Type", "Account From", "Account To", "Amount", "Time", "Comment");
                Console.WriteLine(new string('-', 120));

                // prints values from database accounts for null
                while (reader.Read())
                {
                    int id = reader.GetInt32(0);
                    string type = reader.GetString(1);
                    int account = reader.GetInt32(2);
                    int? destination = reader.IsDBNull(3) ? (int?)null : reader.GetInt32(3);
                    decimal amount = reader.GetDecimal(4);
                    DateTime time = reader.GetDateTime(5);
                    string comment = reader.IsDBNull(6) ? "null" : reader.GetString(6);

                    // prints transactions as rows
                    Console.WriteLine("{0,-14} | {1,-4} | {2,-12} | {3,-10} | ${4,8:F2} | {5,-19:dd/MM/yyyy hh:mm tt} | {6}",
                        id, type, account, destination.HasValue ? destination.Value.ToString() : "null", amount, time, comment);
                }

            }
        }
    }
}
