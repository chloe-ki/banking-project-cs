
using Azure;
using WDT_assignment_1.Managers;

namespace WDT_assignment_1
{
    public class Menu
    {
        private readonly string _connectionString;
        public int CustomerId { get; set; }
        public string CustomerName { get; set; }
        CustomerManager customerManager;
        AccountManager accountManager;
        BankFunctions bankFunctions;
        Login login;

        public Menu(string connectionString, string loginId)
        {
            _connectionString = connectionString;
            customerManager = new CustomerManager(_connectionString);
            accountManager = new AccountManager(_connectionString);
            bankFunctions = new BankFunctions(_connectionString);
            login = new Login(_connectionString);

            CustomerId = customerManager.GetCustomerId(loginId);
            CustomerName = customerManager.GetCustomerName(CustomerId);
        }

        public void DisplayMenu()
        {
            var validSelection = false;
            Console.WriteLine("\nWelcome " + CustomerName + "!");
            while (!validSelection) 
            {
                try
                {
                    Console.WriteLine("""
                ------------------------------
                [1] Deposit
                [2] Withdraw
                [3] Transfer
                [4] My Statement
                [5] Logout
                [6] Exit
                """);
                    Console.Write("Please select a menu option: ");

                    int menuSelect = int.Parse(Console.ReadLine());

                    switch (menuSelect)
                    {
                        case 1:
                            Atm("DEPOSIT", 1);
                            validSelection = true;
                            break;
                        case 2:
                            Atm("WITHDRAW", 2);
                            validSelection = true;
                            break;
                        case 3:
                            Atm("TRANSFER", 3);
                            validSelection = true;
                            break;
                        case 4:
                            Statement();
                            validSelection = true;
                            break;
                        case 5:
                            // clear customer details and console, redirect to login menu
                            CustomerName = null;
                            CustomerId = 0;
                            Console.Clear();
                            login.LoginMenu();
                            break;
                        case 6:
                            Console.WriteLine("Closing program...");
                            Environment.Exit(0);
                            break;
                        default:
                            Console.WriteLine("\nInvalid selection. Select again.\n");
                            break;
                    }

                }
                catch (System.FormatException)
                {
                    Console.WriteLine("Invalid selection.\n");
                    validSelection = false;
                }

            }
        }
       
        public void Atm(string atmOperation, int operationType)
        {
            var validSelection = false;

            while (!validSelection) 
            {
                try
                {
                    var accounts = accountManager.GetCustomerAccounts(CustomerId);
                    Console.WriteLine(
                        $"""

                ==============================
                ATM {atmOperation}
                ==============================
                Accounts for {CustomerName}:
                ------------------------------
                """);

                    // iterates through and creates menu item for each account
                    var i = 1;
                    foreach (var account in accounts)
                    {
                        Console.WriteLine("[" + i + "] " + account);
                        i++;
                    }

                    Console.Write("Please select an account: ");
                    int accountSelection = int.Parse(Console.ReadLine());

                    if (accountSelection >= i || accountSelection < 1)
                    {
                        Console.WriteLine("\nInvalid selection.");
                        // re-prompt 
                    }
                    else
                    {
                        var result = accountSelection - 1;
                        var chosenAccount = accounts[result];

                        switch (operationType)
                        {
                            case 1:
                                bankFunctions.Deposit(chosenAccount);
                                EndMenu();
                                break;
                            case 2:
                                bankFunctions.Withdraw(chosenAccount);
                                EndMenu();
                                break;
                            case 3:
                                bankFunctions.Transfer(chosenAccount);
                                EndMenu();
                                break;
                        }

                        validSelection = true;
                    }
                }
                catch
                {
                    Console.WriteLine("\nInvalid selection.");
                    validSelection = false;
                }
            }
        }

        public void Statement()
        {
            Console.WriteLine(
                $"""
                ==============================
                Accounts for {CustomerName}:
                ==============================
                """);

            var valid = false;
            int chosenAccount = 0;

            while (!valid)
            {
                try
                {
                    var accounts = accountManager.GetCustomerAccounts(CustomerId);
                    var result = 0;
                    var i = 1;

                    foreach (var account in accounts)
                    {
                        Console.WriteLine("[" + i + "] " + account);
                        i++;
                    }
                    Console.Write("Please select an account: ");
                    int choice = int.Parse(Console.ReadLine());

                    if (choice >= i || choice < 1)
                    {
                        Console.WriteLine("\nInvalid selection.");
                        valid = false;
                    }
                    else
                    {
                        result = choice - 1;
                        chosenAccount = accounts[result];

                        valid = true;

                        var balance = bankFunctions.GetAccountBalance(chosenAccount);

                        // prints statement for selected account
                        accountManager.PrintBankStatement(chosenAccount, balance);
                    }

                }
                catch
                {
                    Console.WriteLine("\nInvalid selection.");
                    valid = false;
                }
            }
        }


        public void EndMenu()
        {
            var validSelection = false;

            while(!validSelection)
            {
                try
                {
                    Console.Write(
                $"""

                ==============================
                [1] Main menu
                [2] Exit
                Please select:
                """);

                    var choice = int.Parse(Console.ReadLine());

                    switch (choice)
                    {
                        case 1:
                            DisplayMenu();
                            break;
                        case 2:
                            Console.WriteLine("Closing program...");
                            Environment.Exit(0);
                            break;
                        default:
                            validSelection = false;
                            break;
                    }
                }
                catch
                {
                    Console.WriteLine("\nInvalid selection.");
                }
            }
            
        }
    }
    
}
