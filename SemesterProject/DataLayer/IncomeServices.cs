using System;
using System.Data;
using Microsoft.Extensions.Configuration;
using Microsoft.Data.SqlClient;
using System.Threading.Tasks;
using System.Collections.Generic;

public class Income
{
    public int income_id { get; set; }
    public string income_desc { get; set; }
    public int amount { get; set; }
    public string account_id { get; set; }
}

public class IncomeServices
{
    private readonly string _connectionString;

    public IncomeServices(IConfiguration configuration)
    {
        _connectionString = configuration.GetConnectionString("DefaultConnection");
    }

    public async Task<int> InsertIncomeDetails(Income income)
    {
        using (SqlConnection connection = new SqlConnection(_connectionString))
        {
            await connection.OpenAsync();

            // Begin a transaction
            using (SqlTransaction transaction = connection.BeginTransaction())
            {
                try
                {
                    // SQL query to insert income details
                    string incomeQuery = @"
                    INSERT INTO Income (account_id, amount, income_desc) 
                    VALUES (@accountId, @amount, @incomeDesc)";

                    using (SqlCommand incomeCommand = new SqlCommand(incomeQuery, connection, transaction))
                    {
                        // Add parameters to prevent SQL injection
                        incomeCommand.Parameters.AddWithValue("@accountId", income.account_id);
                        incomeCommand.Parameters.AddWithValue("@amount", income.amount);
                        incomeCommand.Parameters.AddWithValue("@incomeDesc", income.income_desc);

                        // Execute the query
                        await incomeCommand.ExecuteNonQueryAsync();

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

    public async Task<DataTable> GetIncomeByAccountId(string account_id)
    {
        using (SqlConnection connection = new SqlConnection(_connectionString))
        {
            await connection.OpenAsync();

            // SQL query to select income data by account_id
            string selectQuery = @"
            SELECT * FROM Income WHERE account_id = @accountId";

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

    public async Task DeleteIncome(int incomeId)
    {
        using (SqlConnection connection = new SqlConnection(_connectionString))
        {
            await connection.OpenAsync();

            // Begin a transaction
            using (SqlTransaction transaction = connection.BeginTransaction())
            {
                try
                {
                    // SQL query to delete income by income_id
                    string deleteQuery = @"
                    DELETE FROM Income WHERE income_id = @incomeId";

                    using (SqlCommand command = new SqlCommand(deleteQuery, connection, transaction))
                    {
                        // Add parameter for income_id
                        command.Parameters.AddWithValue("@incomeId", incomeId);
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
