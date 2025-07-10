
using Azure;
using Microsoft.Data.SqlClient;
using WDT_assignment_1.Managers;
using WDT_assignment_1.Models;

namespace WDT_assignment_1
{
    public class BankFunctions
    {
        private readonly string _connectionString;
        TransactionManager transactionManager;
        AccountManager accountManager;

        public BankFunctions(string connectionString)
        {
            _connectionString = connectionString;
            transactionManager = new TransactionManager(connectionString);
            accountManager = new AccountManager(connectionString);
            
        }

        // returns balance for specified account
        public decimal GetAccountBalance(int account)
        {
            using var connection = new SqlConnection(_connectionString);
            connection.Open();

            using var command = connection.CreateCommand();
            command.CommandText = "select Balance from Account where AccountNumber = @accountNumber";
            command.Parameters.AddWithValue("accountNumber", account);

            decimal balance = decimal.Parse(command.ExecuteScalar().ToString());

            return balance;
        }

        // adds current balance for specified account with new specified amount
        public void UpdateAccountBalance(int account, decimal amount)
        {
            var currentBalance = GetAccountBalance(account);

            var newBalance = currentBalance + amount;

            using var connection = new SqlConnection(_connectionString);
            connection.Open();

            using var command = connection.CreateCommand();
            command.CommandText = "update Account set Balance = @balance where AccountNumber = @accountNumber";
            command.Parameters.AddWithValue("accountNumber", account);
            command.Parameters.AddWithValue("balance", newBalance);

            command.ExecuteNonQuery();

        }

        public void Deposit(int account)
        {
            var validAmount = false;

            while(!validAmount)
            {
                try
                {
                    decimal amount;

                    Console.Write("\nInsert amount to deposit to account " + account + ": $");
                    string input = Console.ReadLine();

                    if (decimal.TryParse(input, out amount) && Math.Round(amount, 2) == amount)
                    {
                        var comment = transactionManager.TransactionComment();

                        // updates account balance with deposit
                        UpdateAccountBalance(account, amount);

                        // updates transaction table in db
                        transactionManager.AddTransaction(account, 0, amount, comment, 'D');

                        Console.WriteLine("\nSuccess! Deposited $" + amount + " to account " + account
                             + " with comment: " + comment);
                    }
                }
                catch
                {
                    Console.WriteLine("Please enter a valid amount, up to two decimal points.");
                }
            }
        }

        public void Withdraw(int account)
        {
            decimal amount;
            decimal currentBalance = GetAccountBalance(account);
            var approved = false;

            Console.WriteLine($"\nCurrent balance for account {account} is ${currentBalance:0.00}");

            while (!approved) 
            { 
                try
                {
                    Console.Write("\nHow much money would you like to withdraw from account " + account + "?: $");
                    string input = Console.ReadLine();

                    if (decimal.TryParse(input, out amount) && Math.Round(amount, 2) == amount)
                    {
                        var newBalance = currentBalance - amount;

                        // checks that withdrawal can be made
                        approved = accountManager.CanWithdraw(account, newBalance);

                        if (approved)
                        {
                            var fee = transactionManager.ServiceCharge(account, 'W');

                            // include service charge if there is a fee
                            if (fee != 0)
                            {
                                UpdateAccountBalance(account, -fee);
                            }

                            // update account balance with withdrawal
                            UpdateAccountBalance(account, -amount);

                            // add user comment to transaction 
                            var comment = transactionManager.TransactionComment();

                            // update transaction table with new withdrawal transaction
                            transactionManager.AddTransaction(account, 0, amount, comment, 'W');
                            Console.WriteLine("\nSuccess! Withdrew $" + amount + " from account " + account
                                 + " with comment: " + comment);
                        }
                        else
                        {
                            Console.WriteLine("Balance can't go below $300.00 for checking or $0.00 for savings!");
                        }
                    }
                    else
                    {
                        Console.WriteLine("Please enter a valid amount, up to two decimal points.");
                    }
                }
                catch
                {
                    Console.WriteLine("\nInvalid selection.");
                    approved = false;
                }
            }
        }

        public void Transfer(int sourceAccount)
        {
            var destinamtionAccounts = accountManager.GetAccountsExcept(sourceAccount);
            int destinationAccount = 0;

            var validSelection = false;
            var approved = false;
            decimal amount = 0;
            string comment = null;

            Console.WriteLine($"\nCurrent balance for account {sourceAccount} is ${GetAccountBalance(sourceAccount):0.00}");

            while (!validSelection)
            {
                var i = 1;
                try
                {
                    Console.WriteLine("\nSelect destination account:");
                    foreach (var account in destinamtionAccounts)
                    {
                        Console.WriteLine("[" + i + "] " + account);
                        i++;
                    }
                    Console.Write("Please select an account: ");
                    int choice = int.Parse(Console.ReadLine());

                    if (choice >= i || choice < 1)
                    {

                    }
                    else
                    {
                        var result = choice - 1;
                        destinationAccount = destinamtionAccounts[result];
                        validSelection = true;

                    }
                }
                catch
                {
                    Console.WriteLine("\nInvalid selection.");
                    validSelection = false;
                }
            }

            while(!approved)
            {
                try
                {
                    Console.Write("\nHow much money would you like to transfer from account " + sourceAccount + " to " + destinationAccount + "?: $");
                    string input = Console.ReadLine();

                    if (decimal.TryParse(input, out amount) && Math.Round(amount, 2) == amount)
                    {
                        var newBalance = GetAccountBalance(sourceAccount) - amount;

                        // checks that withdrawal can be made
                        approved = accountManager.CanWithdraw(sourceAccount, newBalance);

                        if (approved)
                        {
                            comment = transactionManager.TransactionComment();
                            // subtract service charge from account if applicable
                            var fee = transactionManager.ServiceCharge(sourceAccount, 'T');

                            if (fee != 0)
                            {
                                UpdateAccountBalance(sourceAccount, -fee);
                            }

                            // updates balance for source account, adds transaction to the db
                            UpdateAccountBalance(sourceAccount, -amount);
                            transactionManager.AddTransaction(sourceAccount, destinationAccount, amount, comment, 'T');

                            // updates balance for destination account, adds transaction to db
                            UpdateAccountBalance(destinationAccount, amount);
                            transactionManager.AddTransaction(destinationAccount, 0, amount, comment, 'T');

                            Console.WriteLine($"Success! Transferred ${amount:0.00} from {sourceAccount} to {destinationAccount}");
                        }
                        else
                        {
                            Console.WriteLine("Balance can't go below $300.00 for checking or $0.00 for savings!");
                        }
                    }
                    else
                    {
                        Console.WriteLine("Please enter a valid amount, up to two decimal points.");
                    }
                }
                catch
                {
                    Console.WriteLine("Something went wrong! Please try again.");
                }
                
            }
        }
    }
}

