using System;
using System.Data;
using Microsoft.Extensions.Configuration;
using Microsoft.Data.SqlClient;
using System.Threading.Tasks;
using SemesterProject.Pages;

public class Accounts
{
    public string account_name;
    public int initial_balance;
}
public class AccountServices
{
    private readonly string _connectionString;

    public AccountServices(IConfiguration configuration)
    {
        _connectionString = configuration.GetConnectionString("DefaultConnection");
    }

    public async Task<string> InsertAccountDetails(Accounts account, string uID)
    {
        using (SqlConnection connection = new SqlConnection(_connectionString))
        {
            await connection.OpenAsync();

            // Begin a transaction
            using (SqlTransaction transaction = connection.BeginTransaction())
            {
                try
                {
                    // SQL query with OUTPUT clause to capture the inserted account_id
                    string feedbackQuery = @"
                    INSERT INTO Account (account_name, initial_balance, user_id) 
                    OUTPUT INSERTED.account_id
                    VALUES (@accname, @initbal, @userid)";

                    using (SqlCommand feedbackCommand = new SqlCommand(feedbackQuery, connection, transaction))
                    {
                        // Add parameters to prevent SQL injection
                        feedbackCommand.Parameters.AddWithValue("@accname", account.account_name);
                        feedbackCommand.Parameters.AddWithValue("@initbal", account.initial_balance);
                        feedbackCommand.Parameters.AddWithValue("@userid", uID);

                        // Execute the query and get the inserted account_id
                        object accountIdResult = await feedbackCommand.ExecuteScalarAsync();

                        // Commit the transaction if the insert operation succeeds
                        transaction.Commit();

                        // Return the account_id as a string
                        return accountIdResult?.ToString();
                    }
                }
                catch (Exception)
                {
                    // Rollback the transaction if an error occurs
                    transaction.Rollback();
                    throw; // Re-throw the exception for handling at a higher level
                }
            }
        }
    }

}
