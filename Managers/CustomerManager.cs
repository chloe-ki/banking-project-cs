using Microsoft.Data.SqlClient;
using WDT_assignment_1.Models;
using WDT_assignment_1.Utilities;

namespace WDT_assignment_1.Managers
{
    public class CustomerManager
    {
        private readonly string _connectionString;
        public List<Customer> Customers { get; } = [];

        // paramaterised sql throughout
        public CustomerManager(string connectionString)
        {
            _connectionString = connectionString;

        }

        public void InsertDBCustomer(Customer customer)
        {
            Customers.Add(customer);
            using var connection = new SqlConnection(_connectionString);
            connection.Open();

            using var command = connection.CreateCommand();
            command.CommandText =
                @"insert into Customer (CustomerID, Name, Address, City, PostCode) 
                values (@customerID, @name, @address, @city, @postCode)";

            command.Parameters.AddWithValue("customerID", customer.CustomerID);
            command.Parameters.AddWithValue("name", customer.Name);
            // use extension method to insert the nullable values as null
            command.Parameters.AddWithValue("address", customer.Address.GetObjectOrDbNull());
            command.Parameters.AddWithValue("city", customer.City.GetObjectOrDbNull());
            command.Parameters.AddWithValue("postCode", customer.PostCode.GetObjectOrDbNull());

            command.ExecuteNonQuery();
        }

        public int GetCustomerId(string loginId)
        {
            using var connection = new SqlConnection(_connectionString);
            connection.Open();

            using var command = connection.CreateCommand();
            command.CommandText = "select CustomerID from Login where LoginID = @loginID";
            command.Parameters.AddWithValue("loginID", loginId);

            var customerId = int.Parse(command.ExecuteScalar().ToString());

            return customerId;
        }

        public string GetCustomerName(int customerId)
        {
            using var connection = new SqlConnection(_connectionString);
            connection.Open();

            using var command = connection.CreateCommand();
            command.CommandText = "select Name from Customer where CustomerID = @custID";
            command.Parameters.AddWithValue("custID", customerId);

            var name = command.ExecuteScalar().ToString();

            return name;
        }

    }
}
