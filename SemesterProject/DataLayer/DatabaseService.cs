using System;
using System.Collections.Generic;
using System.Data;
using System.Threading.Tasks;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;

public class User
{
    public string Email { get; set; }
    public string Password { get; set; }
}

public class DatabaseService
{
    private readonly string _connectionString;

    public DatabaseService(IConfiguration configuration)
    {
        _connectionString = configuration.GetConnectionString("DefaultConnection");
    }

    public DataTable ExecuteQuery(string query, Dictionary<string, object> parameters = null)
    {
        DataTable result = new DataTable();

        using (SqlConnection connection = new SqlConnection(_connectionString))
        {
            connection.Open();

            using (SqlCommand command = new SqlCommand(query, connection))
            {
                if (parameters != null)
                {
                    foreach (var param in parameters)
                    {
                        command.Parameters.AddWithValue(param.Key, param.Value);
                    }
                }

                using (SqlDataAdapter adapter = new SqlDataAdapter(command))
                {
                    adapter.Fill(result);
                }
            }
        }

        return result;
    }

    public async Task<string> AddUser(User user)
    {
        using (SqlConnection connection = new SqlConnection(_connectionString))
        {
            await connection.OpenAsync();

            string query = "INSERT INTO Users (email, user_password) VALUES (@name, @pass); SELECT SCOPE_IDENTITY()";
            using (SqlCommand command = new SqlCommand(query, connection))
            {
                // Add parameters to prevent SQL injection
                command.Parameters.AddWithValue("@name", user.Email);
                command.Parameters.AddWithValue("@pass", user.Password);

                // ExecuteScalarAsync returns the first column of the first row in the result set
                // Here, we're expecting the newly inserted user's ID
                object result = await command.ExecuteScalarAsync();

                // Check for DBNull before converting
                if (result == DBNull.Value || result == null)
                {
                    return null;
                }

                // Return the result as a string
                return result.ToString();
            }
        }
    }

    public async Task<string> Login(User user)
    {
        using (SqlConnection connection = new SqlConnection(_connectionString))
        {
            await connection.OpenAsync();

            string query = "SELECT user_id FROM Users WHERE email = @name AND user_password = @pass";
            using (SqlCommand command = new SqlCommand(query, connection))
            {
                // Add parameters to prevent SQL injection
                command.Parameters.AddWithValue("@name", user.Email);
                command.Parameters.AddWithValue("@pass", user.Password);

                // ExecuteScalarAsync returns the first column of the first row in the result set
                // Here, we're expecting either the user_id or null if no user matches the credentials
                object result = await command.ExecuteScalarAsync();

                // Check for DBNull before converting
                if (result == DBNull.Value || result == null)
                {
                    return null;
                }

                // Return the result as a string
                return result.ToString();
            }
        }
    }

    public T ExecuteScalar<T>(string query, Dictionary<string, object> parameters = null)
    {
        using (var connection = new SqlConnection(_connectionString))
        {
            connection.Open();
            using (var command = connection.CreateCommand())
            {
                command.CommandText = query;
                if (parameters != null)
                {
                    foreach (var parameter in parameters)
                    {
                        command.Parameters.AddWithValue(parameter.Key, parameter.Value);
                    }
                }

                object result = command.ExecuteScalar();

                // Check for DBNull before converting
                if (result == DBNull.Value || result == null)
                {
                    return default(T); // return default value of T
                }

                return (T)Convert.ChangeType(result, typeof(T));
            }
        }
    }
}
