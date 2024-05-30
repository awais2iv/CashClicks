using System;
using System.Data;
using Microsoft.Extensions.Configuration;
using Microsoft.Data.SqlClient;
using System.Threading.Tasks;

public class Notes
{
    public string notes;
}
public class NotesServices
{
    private readonly string _connectionString;

    public NotesServices(IConfiguration configuration)
    {
        _connectionString = configuration.GetConnectionString("DefaultConnection");
    }

    public async Task InsertNote(Notes n, string accid )
    {
        using (SqlConnection connection = new SqlConnection(_connectionString))
        {
            await connection.OpenAsync();

            // Begin a transaction
            using (SqlTransaction transaction = connection.BeginTransaction())
            {
                try
                {
                    string feedbackQuery = "INSERT INTO notes (account_id,content) VALUES (@accid, @cont)";
                    using (SqlCommand feedbackCommand = new SqlCommand(feedbackQuery, connection, transaction))
                    {
                        feedbackCommand.Parameters.AddWithValue("@accid", accid );
                        feedbackCommand.Parameters.AddWithValue("@cont", n.notes);
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

    public async Task DeleteNote(string note)
    {
        using (SqlConnection connection = new SqlConnection(_connectionString))
        {
            await connection.OpenAsync();

            // Begin a transaction
            using (SqlTransaction transaction = connection.BeginTransaction())
            {
                try
                {
                    string deleteQuery = "DELETE FROM notes WHERE content = @note";
                    using (SqlCommand command = new SqlCommand(deleteQuery, connection, transaction))
                    {
                        command.Parameters.AddWithValue("@note", note);
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
