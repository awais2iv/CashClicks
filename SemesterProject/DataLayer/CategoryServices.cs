using System;
using System.Data;
using Microsoft.Extensions.Configuration;
using Microsoft.Data.SqlClient;
using System.Threading.Tasks;
using System.Collections.Generic;

public class CategoryExpense
{
    public string CategoryId { get; set; }
    public string CategoryName { get; set; }
}


public class CategoryServices
{
    private readonly string _connectionString;

    public CategoryServices(IConfiguration configuration)
    {
        _connectionString = configuration.GetConnectionString("DefaultConnection");
    }

    public async Task<List<CategoryExpense>> GetCategories()
    {
        List<CategoryExpense> categories = new List<CategoryExpense>();

        using (SqlConnection connection = new SqlConnection(_connectionString))
        {
            await connection.OpenAsync();

            string query = "SELECT category_name FROM Expense_category";

            using (SqlCommand command = new SqlCommand(query, connection))
            {
                using (SqlDataReader reader = await command.ExecuteReaderAsync())
                {
                    while (await reader.ReadAsync())
                    {
                        categories.Add(new CategoryExpense
                        {
                            CategoryName = reader["category_name"].ToString()
                        });
                    }
                }
            }
        }

        return categories;
    }

    // Implement InsertCategory and DeleteCategory methods as before...
    public async Task InsertCategory(CategoryExpense category)
    {
        using (SqlConnection connection = new SqlConnection(_connectionString))
        {
            await connection.OpenAsync();

            // Begin a transaction
            using (SqlTransaction transaction = connection.BeginTransaction())
            {
                try
                {
                    string categoryQuery = "INSERT INTO Expense_category (category_name) VALUES (@categoryName)";
                    using (SqlCommand categoryCommand = new SqlCommand(categoryQuery, connection, transaction))
                    {
                        categoryCommand.Parameters.AddWithValue("@categoryName", category.CategoryName);
                        await categoryCommand.ExecuteNonQueryAsync();
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

    public async Task DeleteCategory(string categoryName)
    {
        using (SqlConnection connection = new SqlConnection(_connectionString))
        {
            await connection.OpenAsync();

            // Begin a transaction
            using (SqlTransaction transaction = connection.BeginTransaction())
            {
                try
                {
                    string deleteQuery = "DELETE FROM Expense_category WHERE category_name = @categoryName";
                    using (SqlCommand command = new SqlCommand(deleteQuery, connection, transaction))
                    {
                        command.Parameters.AddWithValue("@categoryName", categoryName);
                        await command.ExecuteNonQueryAsync();
                    }

                    // Get the count of affected rows
                    string countQuery = "SELECT @@ROWCOUNT";
                    using (SqlCommand countCommand = new SqlCommand(countQuery, connection, transaction))
                    {
                        int rowCount = (int)await countCommand.ExecuteScalarAsync();

                        // Perform further operations based on the rowCount
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


    public async Task<string> GetCategoryIdByName(string categoryName)
    {
        using (SqlConnection connection = new SqlConnection(_connectionString))
        {
            await connection.OpenAsync();

            string query = "SELECT category_id FROM Expense_category WHERE category_name = @CategoryName";
            using (SqlCommand command = new SqlCommand(query, connection))
            {
                command.Parameters.AddWithValue("@CategoryName", categoryName);
                object result = await command.ExecuteScalarAsync();

                // If result is not null, convert it to string
                if (result != null)
                {
                    return result.ToString();
                }
            }
        }
        return null; // Return null if no category with the given name is found
    }



}




        
            

           
            
