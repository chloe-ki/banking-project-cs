using Microsoft.Data.SqlClient;
using System.Net.Http;
using Newtonsoft.Json;
using System;
using WDT_assignment_1.Models;

namespace WDT_assignment_1.Managers
{
    public class DBManager
    {
        // runs sql script if db is empty
        public static void CreateDbTables(string connectionString)
        {
            using var connection = new SqlConnection(connectionString);
            connection.Open();

            using var command = connection.CreateCommand();

            var table = "Customer";

            // makes sure db is empty first

            string query = $"IF OBJECT_ID(N'{table}', N'U') IS NOT NULL SELECT 1 ELSE SELECT 0";

            // runs the sql script
            using (SqlCommand cmd = new SqlCommand(query, connection))
            {
                if ((int)cmd.ExecuteScalar() == 0)
                {
                    command.CommandText = File.ReadAllText("Sql/CreateTables.sql");

                    command.ExecuteNonQuery();
                }
            }
        }

        // populates db with deserialised JSON using async
        public static async Task<bool> DBPopulationAsync(string connectionString)
        {
            using var connection = new SqlConnection(connectionString);
            await connection.OpenAsync();

            const string webService = "https://coreteaching01.csit.rmit.edu.au/~e103884/wdt/services/customers/";

            // fetch the JSON from webservice 
            using var client = new HttpClient();
            var json = await client.GetStringAsync(webService);

            // check to see if tables are already populated
            using var command = connection.CreateCommand();
            command.CommandText = "select count(*) from Customer";
            int result = (int)await command.ExecuteScalarAsync();

            if (result != 0)
            {
                return false;
            }

            // deserialise JSON 
            var customers = DeserialiseJSON<Customer>(json);

            var customerManager = new CustomerManager(connectionString);
            var accountManager = new AccountManager(connectionString);
            var loginManager = new LoginManager(connectionString);
            var transactionManager = new TransactionManager(connectionString);
            var bankFunctions = new BankFunctions(connectionString);

            foreach (var customer in customers)
            {
                customerManager.InsertDBCustomer(customer);

                foreach (var account in customer.Accounts)
                {
                    account.CustomerID = customer.CustomerID;
                    accountManager.InsertDBAccount(account);

                    foreach (var transaction in account.Transactions)
                    {
                        transaction.AccountNumber = account.AccountNumber;
                        transaction.TransactionType = 'D';

                        transactionManager.InsertDBTransaction(transaction);
                        bankFunctions.UpdateAccountBalance(account.AccountNumber, transaction.Amount);
                    }
                }

                customer.Login.CustomerID = customer.CustomerID;
                loginManager.InsertDBLogin(customer.Login);
            }

            return true;
        }

        // method to deserialise using generics
        public static List<T> DeserialiseJSON<T>(string json)
        {
            return JsonConvert.DeserializeObject<List<T>>(json, new JsonSerializerSettings
            {
                DateFormatString = "dd/MM/yyyy hh:mm:ss tt"
            }) ?? [];
        }
    }
}
