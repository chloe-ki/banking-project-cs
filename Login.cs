

using WDT_assignment_1.Managers;
using WDT_assignment_1.Models;

namespace WDT_assignment_1
{
    public class Login
    {
        private readonly string _connectionString;
        LoginManager loginManager;
        public string LoginId { get; set; }

        public Login(string connectionString) 
        {
            _connectionString = connectionString;
            loginManager = new LoginManager(_connectionString);
        }

        public void LoginMenu()
        {
            var loggedIn = false;

            Console.WriteLine("Welcome! Please enter your log in details: ");

            while(!loggedIn)
            {
                try
                {
                    Console.Write("Enter Login ID: ");
                    var loginId = Console.ReadLine();

                    string password = "";
                    Console.Write("Enter Password: ");

                    // masks the password while typing for security
                    ConsoleKeyInfo key;

                    do
                    {
                        key = Console.ReadKey(true);

                        // omits enter and backspace keys (backspace functionality not included, avoids return carriage)
                        if (key.Key != ConsoleKey.Enter && key.Key != ConsoleKey.Backspace)
                        {
                            password += key.KeyChar;
                            Console.Write("*");
                        }
                    }
                    // pressing enter stops character retrieval
                    while (key.Key != ConsoleKey.Enter);

                    Console.WriteLine("\nLogging in...");

                    loggedIn = ValidateCredentials(loginId, password);
                }
                catch
                {
                    Console.WriteLine("Invalid ID or password! Please try again.");
                    loggedIn = false;
                }
            }
        }

        public bool ValidateCredentials(string loginId, string password)
        {
            if(loginManager.ValidateLogin(loginId, password))
            {
                LoginId = loginId;
                Console.Clear();
                return true;
            }

            // if user credentials are invalid
            else
            {
                Console.WriteLine("Invalid credentials! Please try again.");
                return false;
            }
        }

    }
}

