using Kolokwium_s24412_nr1.Services.Interface;
using System.Data.SqlClient;
using Kolokwium_s24412_nr1.Models.DTOs;

namespace Kolokwium_s24412_nr1.Services
{
    public class BooksService : IBookService
    {
        private readonly IConfiguration _configuration;

        public BooksService(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public async Task<bool> DoesBookExist(int id)
        {
            const string query = "SELECT 1 FROM books WHERE PK = @IdBook";
            using var conn = new SqlConnection(_configuration.GetConnectionString("DefaultConnection"));
            using var cmd = new SqlCommand(query, conn);
            cmd.Parameters.AddWithValue("@IdBook", id);
            await conn.OpenAsync();
            var response = await cmd.ExecuteScalarAsync();
            return response != null;
        }

        public async Task<bool> DoesAuthorExist(string firstName, string lastName)
        {
            const string query = "SELECT PK FROM authors WHERE first_name = @FirstName AND last_name = @LastName";
            using var conn = new SqlConnection(_configuration.GetConnectionString("DefaultConnection"));
            using var cmd = new SqlCommand(query, conn);
            cmd.Parameters.AddWithValue("@FirstName", firstName);
            cmd.Parameters.AddWithValue("@LastName", lastName);
            await conn.OpenAsync();
            var response = await cmd.ExecuteScalarAsync();
            return response != null;
        }

        public async Task<int> AddBookWithAuthors(BookCreateDto bookCreateDto)
        {
            const string insertBookQuery = @"
            INSERT INTO books (Title)
            VALUES (@Title);
            SELECT SCOPE_IDENTITY();";

            using var conn = new SqlConnection(_configuration.GetConnectionString("DefaultConnection"));
            await conn.OpenAsync();
            using var transaction = conn.BeginTransaction();
            try
            {
                using var cmdBook = new SqlCommand(insertBookQuery, conn, transaction);
                cmdBook.Parameters.AddWithValue("@Title", bookCreateDto.Title);
                int bookId = Convert.ToInt32(await cmdBook.ExecuteScalarAsync());

                foreach (var author in bookCreateDto.Authors)
                {
                    const string insertAuthorQuery = @"
                    INSERT INTO authors (first_name, last_name)
                    VALUES (@FirstName, @LastName);
                    SELECT SCOPE_IDENTITY();";

                    using var cmdAuthor = new SqlCommand(insertAuthorQuery, conn, transaction);
                    cmdAuthor.Parameters.AddWithValue("@FirstName", author.FirstName);
                    cmdAuthor.Parameters.AddWithValue("@LastName", author.LastName);
                    int authorId = Convert.ToInt32(await cmdAuthor.ExecuteScalarAsync());

                    const string linkBookAuthorQuery = @"
                    INSERT INTO books_authors (FK_book, FK_author)
                    VALUES (@BookId, @AuthorId);";

                    using var cmdLink = new SqlCommand(linkBookAuthorQuery, conn, transaction);
                    cmdLink.Parameters.AddWithValue("@BookId", bookId);
                    cmdLink.Parameters.AddWithValue("@AuthorId", authorId);
                    await cmdLink.ExecuteNonQueryAsync();
                }

                transaction.Commit();
                return bookId;
            }
            catch (Exception ex)
            {
                transaction.Rollback();
                throw new Exception("Failed to add book and authors: " + ex.Message);
            }
        }


        public async Task<BookResponseDto> GetBookWithAuthorsAsync(int bookId)
        {
            const string bookQuery = "SELECT Title FROM books WHERE PK = @BookId";
            const string authorsQuery = @"
            SELECT a.first_name, a.last_name 
            FROM authors a
            INNER JOIN books_authors ba ON a.PK = ba.FK_author
             WHERE ba.FK_book = @BookId";

            using var conn = new SqlConnection(_configuration.GetConnectionString("DefaultConnection"));
            await conn.OpenAsync();

            using var bookCmd = new SqlCommand(bookQuery, conn);
            bookCmd.Parameters.AddWithValue("@BookId", bookId);
            var title = (string)await bookCmd.ExecuteScalarAsync();
            if (title == null) return null;

            var bookAuthorsDto = new BookResponseDto
            {
                Id = bookId,
                Title = title,
                Authors = new List<AuthorDto>()
            };

            using var authorsCmd = new SqlCommand(authorsQuery, conn);
            authorsCmd.Parameters.AddWithValue("@BookId", bookId);
            using var reader = await authorsCmd.ExecuteReaderAsync();

            while (reader.Read())
            {
                bookAuthorsDto.Authors.Add(new AuthorDto
                {
                    FirstName = reader["first_name"].ToString(),
                    LastName = reader["last_name"].ToString()
                });
            }

            return bookAuthorsDto;
        }
    }
}
