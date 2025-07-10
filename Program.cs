using Microsoft.Extensions.Configuration;
using WDT_assignment_1.Managers;

namespace WDT_assignment_1
{
    public class Program
    {
        static void Main(string[] args)
        {
            // add appsettings
            var config = new ConfigurationBuilder().AddJsonFile("appsettings.json").Build();

            // get connection string from appsettings.json
            var connectionString = config.GetConnectionString("DBConnection");

            // runs sql script if db is empty
            DBManager.CreateDbTables(connectionString);

            // populates the database with models
            DBManager.DBPopulationAsync(connectionString).Wait();

            // displays login menu
            var loginMenu = new Login(connectionString);
            loginMenu.LoginMenu();

            // display main menu
            var mainMenu = new Menu(connectionString, loginMenu.LoginId);
            mainMenu.DisplayMenu();
        }
    }
}
