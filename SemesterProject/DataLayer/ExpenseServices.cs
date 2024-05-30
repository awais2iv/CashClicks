using System;
using System.Data;
using Microsoft.Extensions.Configuration;
using Microsoft.Data.SqlClient;
using System.Threading.Tasks;
using System.Collections.Generic;

public class Expense
{
    public int expense_id { get; set; }
    public string expense_desc { get; set; }
    public int amount { get; set; }
    public string account_id { get; set; }
    public string category_name { get; set; }
}

public class ExpenseServices
{
    private readonly string _connectionString;

    public ExpenseServices(IConfiguration configuration)
    {
        _connectionString = configuration.GetConnectionString("DefaultConnection");
    }

    public async Task<int> InsertExpenseDetails(Expense expense)
    {
        using (SqlConnection connection = new SqlConnection(_connectionString))
        {
            await connection.OpenAsync();

            // Begin a transaction
            using (SqlTransaction transaction = connection.BeginTransaction())
            {
                try
                {
                    // SQL query to insert expense details
                    string expenseQuery = @"
                    INSERT INTO Expense (account_id, category_name, amount, expense_desc) 
                    VALUES (@accountId, @categoryn, @amount, @expenseDesc)";

                    using (SqlCommand expenseCommand = new SqlCommand(expenseQuery, connection, transaction))
                    {
                        // Add parameters to prevent SQL injection
                        expenseCommand.Parameters.AddWithValue("@accountId", expense.account_id);
                        expenseCommand.Parameters.AddWithValue("@categoryn", expense.category_name);
                        expenseCommand.Parameters.AddWithValue("@amount", expense.amount);
                        expenseCommand.Parameters.AddWithValue("@expenseDesc", expense.expense_desc);

                        // Execute the query
                        await expenseCommand.ExecuteNonQueryAsync();

                        // Commit the transaction
                        transaction.Commit();

                        // Return success status
                        return 1;
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

    public async Task<DataTable> GetExpenseByAccountId(string account_id)
    {
        using (SqlConnection connection = new SqlConnection(_connectionString))
        {
            await connection.OpenAsync();

            // SQL query to select expense data by account_id
            string selectQuery = @"
            SELECT * FROM Expense WHERE account_id = @accountId";

            using (SqlCommand command = new SqlCommand(selectQuery, connection))
            {
                // Add parameter for account_id
                command.Parameters.AddWithValue("@accountId", account_id);

                // Execute the query and retrieve data into a DataTable
                DataTable dataTable = new DataTable();
                using (SqlDataAdapter adapter = new SqlDataAdapter(command))
                {
                    adapter.Fill(dataTable);
                }

                return dataTable;
            }
        }
    }

    public async Task DeleteExpense(int expenseId)
    {
        using (SqlConnection connection = new SqlConnection(_connectionString))
        {
            await connection.OpenAsync();

            // Begin a transaction
            using (SqlTransaction transaction = connection.BeginTransaction())
            {
                try
                {
                    // SQL query to delete expense by expense_id
                    string deleteQuery = @"
                    DELETE FROM Expense WHERE expense_id = @expenseId";

                    using (SqlCommand command = new SqlCommand(deleteQuery, connection, transaction))
                    {
                        // Add parameter for expense_id
                        command.Parameters.AddWithValue("@expenseId", expenseId);
                        // Execute the query
                        await command.ExecuteNonQueryAsync();
                    }

                    // Commit the transaction if all operations succeed
                    transaction.Commit();
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
