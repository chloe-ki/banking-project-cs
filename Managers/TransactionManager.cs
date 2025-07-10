using Microsoft.Data.SqlClient;
using WDT_assignment_1.Utilities;

namespace WDT_assignment_1.Managers
{
    public class TransactionManager
    {
        private readonly string _connectionString;

        public TransactionManager(string connectionString)
        {
            _connectionString = connectionString;
        }

        public void InsertDBTransaction(Models.Transaction transac)
        {
            using var connection = new SqlConnection(_connectionString);
            connection.Open();

            // paramaterised sql
            using var command = connection.CreateCommand();
            command.CommandText =
                @"insert into [Transaction] (TransactionType, AccountNumber, DestinationAccountNumber, Amount, Comment, TransactionTimeUtc)
                values (@transactionType, @accountNum, @destAccNum, @amount, @comment, @transactionTimeUtc)";
            command.Parameters.AddWithValue("transactionType", transac.TransactionType);
            command.Parameters.AddWithValue("accountNum", transac.AccountNumber);
            command.Parameters.AddWithValue("destAccNum", DBNull.Value);
            command.Parameters.AddWithValue("amount", transac.Amount);

            // use extension method to insert the nullable values as null
            command.Parameters.AddWithValue("comment", transac.Comment.GetObjectOrDbNull());
            command.Parameters.AddWithValue("transactionTimeUtc", transac.TransactionTimeUtc);

            command.ExecuteNonQuery();
        }

        // adds transaction details into db
        public void AddTransaction(int accountId, int destinationAccount, decimal amount, string comment, char transactionType)
        {
            DateTime dateTime = DateTime.UtcNow;

            using var connection = new SqlConnection(_connectionString);
            connection.Open();

            using var command = connection.CreateCommand();
            command.CommandText =
                @"insert into [transaction] (TransactionType, 
                    AccountNumber, DestinationAccountNumber, Amount, Comment, TransactionTimeUtc)
                    values (@transactionType, @account, @destinationAcc, @amount, 
                    @comment, @transactionTimeUtc)";
            command.Parameters.AddWithValue("transactionType", transactionType);
            command.Parameters.AddWithValue("account", accountId);

            // adds destination account as null if none specified
            if (destinationAccount == 0)
            {
                command.Parameters.AddWithValue("destinationAcc", DBNull.Value);
            }
            else
            {
                command.Parameters.AddWithValue("destinationAcc", destinationAccount);
            }

            command.Parameters.AddWithValue("amount", amount);
            command.Parameters.AddWithValue("comment", comment.GetObjectOrDbNull());
            command.Parameters.AddWithValue("transactionTimeUtc", dateTime);

            command.ExecuteNonQuery();
        }

        // allows user to add comments to transactions
        public string TransactionComment()
        {
            var validSelection = false;
            string comment = null;

            while (!validSelection)
            {
                try
                {
                    Console.Write("Add transaction comment? (y/n): ");
                    var addComment = char.Parse(Console.ReadLine().ToLower());

                    switch (addComment)
                    {
                        case 'y':
                            Console.Write("Enter transaction comment: ");
                            comment = Console.ReadLine();

                            validSelection = true;
                            return comment;
                        case 'n':
                            comment = null;

                            validSelection = true;
                            break;
                        default:
                            Console.WriteLine("\nInvalid selection.");
                            break;
                    }
                }
                catch (FormatException)
                {
                    Console.WriteLine("\nInvalid input. Please enter 'y' or 'n'.");
                }
            }
            return comment;
        }

        // adds service charge to account and returns fee charged
        public decimal ServiceCharge(int account, char accountType)
        {
            var transactions = new List<int>();
            decimal fee = 0;

            using var connection = new SqlConnection(_connectionString);
            connection.Open();

            using var command = connection.CreateCommand();
            command.CommandText = "select TransactionID from [Transaction] where (TransactionType = @T or TransactionType = @W) and AccountNumber = @account";
            command.Parameters.AddWithValue("T", 'T');
            command.Parameters.AddWithValue("W", 'W');
            command.Parameters.AddWithValue("account", account);

            using (SqlDataReader reader = command.ExecuteReader())
            {
                // adds transactions to list 
                while (reader.Read())
                {
                    transactions.Add(reader.GetInt32(0));
                }
            }

            // checks if account exceeds free transactions, adds relevant service fee
            if (transactions.Count >= 2)
            {
                switch (accountType)
                {
                    case 'W':
                        fee = 0.05M;
                        break;
                    case 'T':
                        fee = 0.10M;
                        break;
                }
                // add service fee to transactions
                AddTransaction(account, 0, fee, null, 'S');
            }
            return fee;
        }
    }
}
