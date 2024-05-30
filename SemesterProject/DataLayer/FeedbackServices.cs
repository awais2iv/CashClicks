using System;
using System.Data;
using Microsoft.Extensions.Configuration;
using Microsoft.Data.SqlClient;
using System.Threading.Tasks;




public class Feedback
{
    public string Email { get; set; }
    public string FeedbackContent { get; set; }
}

public class FeedbackServices
{
    private readonly string _connectionString;

    public FeedbackServices(IConfiguration configuration)
    {
        _connectionString = configuration.GetConnectionString("DefaultConnection");
    }

    public async Task<List<Feedback>> GetFeedbacks()
    {
        List<Feedback> feedbacks = new List<Feedback>();

        using (SqlConnection connection = new SqlConnection(_connectionString))
        {
            await connection.OpenAsync();

            string query = "SELECT email, fb FROM Feedback";

            using (SqlCommand command = new SqlCommand(query, connection))
            {
                using (SqlDataReader reader = await command.ExecuteReaderAsync())
                {
                    while (await reader.ReadAsync())
                    {
                        feedbacks.Add(new Feedback
                        {
                            Email = reader["email"].ToString(),
                            FeedbackContent = reader["fb"].ToString()
                        });
                    }
                }
            }
        }

        return feedbacks;
    }

    public async Task InsertFeedback(Feedback feedback)
    {
        using (SqlConnection connection = new SqlConnection(_connectionString))
        {
            await connection.OpenAsync();

            // Begin a transaction
            using (SqlTransaction transaction = connection.BeginTransaction())
            {
                try
                {
                    string feedbackQuery = "INSERT INTO Feedback (email, fb) VALUES (@email, @feedbackContent)";
                    using (SqlCommand feedbackCommand = new SqlCommand(feedbackQuery, connection, transaction))
                    {
                        feedbackCommand.Parameters.AddWithValue("@email", feedback.Email);
                        feedbackCommand.Parameters.AddWithValue("@feedbackContent", feedback.FeedbackContent);
                        await feedbackCommand.ExecuteNonQueryAsync();
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

    public async Task DeleteFeedback(string email)
    {
        using (SqlConnection connection = new SqlConnection(_connectionString))
        {
            await connection.OpenAsync();

            // Begin a transaction
            using (SqlTransaction transaction = connection.BeginTransaction())
            {
                try
                {
                    string deleteQuery = "DELETE FROM Feedback WHERE email = @email";
                    using (SqlCommand command = new SqlCommand(deleteQuery, connection, transaction))
                    {
                        command.Parameters.AddWithValue("@email", email);
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
}
